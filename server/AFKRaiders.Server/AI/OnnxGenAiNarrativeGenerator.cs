namespace AFKRaiders.Server.AI;

using AFKRaiders.Server.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Text.RegularExpressions;

public sealed class OnnxGenAiNarrativeGenerator : INarrativeGenerator, IDisposable
{
    public static readonly TimeSpan InferenceBudget = TimeSpan.FromSeconds(25);
    private readonly ILogger<OnnxGenAiNarrativeGenerator> logger;
    private readonly TemplateNarrativeGenerator fallback = new();
    private readonly SemaphoreSlim modelGate = new(1, 1);
    private Model? model;
    private Tokenizer? tokenizer;
    private bool disposed;
    public string Name => "OnnxGenAiNarrator";
    public bool IsAvailable => !disposed && model is not null;

    public OnnxGenAiNarrativeGenerator(string? modelPath = null, ILogger<OnnxGenAiNarrativeGenerator>? logger = null)
    {
        this.logger = logger ?? NullLogger<OnnxGenAiNarrativeGenerator>.Instance;
        if (string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath)) return;
        try
        {
            model = new Model(modelPath);
            tokenizer = new Tokenizer(model);
            this.logger.LogInformation("ONNX initialized. Serialized ambient inference budget is {Seconds}s.", InferenceBudget.TotalSeconds);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(ex, "ONNX initialization failed; canon fallback remains available.");
            tokenizer?.Dispose();
            model?.Dispose();
            tokenizer = null;
            model = null;
        }
    }

    public AarDebriefView GenerateAarDebrief(AarDebriefContext context) => fallback.GenerateAarDebrief(context);
    public ItemAppraisalView GenerateItemAppraisal(ItemAppraisalContext context) => fallback.GenerateItemAppraisal(context);

    public async ValueTask<AarDebriefView> GenerateAarDebriefAsync(AarDebriefContext context, CancellationToken ct = default)
    {
        var baseline = fallback.GenerateAarDebrief(context);
        var aside = await GenerateAmbientAsync("Desperanza", "reviewing my paperwork", ct);
        if (aside.Text is null) return baseline;
        var bullets = baseline.Bullets.ToArray();
        bullets[2] += $" Optional Raider aside [Generated]: {aside.Text}";
        return baseline with { Bullets = bullets, Source = "Generated" };
    }

    public async ValueTask<ItemAppraisalView> GenerateItemAppraisalAsync(ItemAppraisalContext context, CancellationToken ct = default)
    {
        var baseline = fallback.GenerateItemAppraisal(context);
        var aside = await GenerateAmbientAsync("Desperanza", "reviewing my priorities", ct);
        return aside.Text is null ? baseline : baseline with
        {
            Appraisal = $"{baseline.Appraisal} Optional Raider aside [Generated]: {aside.Text}",
            Source = "Generated"
        };
    }

    public async ValueTask<string> GenerateEnrichedEventTextAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
        => (await GenerateEventResultAsync(request, ct)).Text;

    public async ValueTask<NarrativeResult> GenerateEventResultAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
    {
        var context = request.EventPayload as AmbientCommsContext;
        var aside = await GenerateAmbientAsync(context?.Location ?? "Desperanza", context?.Activity ?? "considering my priorities", ct, request.EventSequence);
        return aside.Text is null
            ? new(await fallback.GenerateEnrichedEventTextAsync(request, ct), "ArchivalLog", aside.FailureReason)
            : new(aside.Text, "Generated");
    }

    private sealed record AmbientGeneration(string? Text, string? FailureReason = null);

    private async Task<AmbientGeneration> GenerateAmbientAsync(string location, string activity, CancellationToken ct, long sequence = 0)
    {
        if (!IsAvailable) return new(null, "ModelUnavailable");
        var started = System.Diagnostics.Stopwatch.StartNew();
        var inferenceStarted = false;
        try
        {
            var text = await RunBoundedAsync(modelGate, InferenceBudget,
                token =>
                {
                    inferenceStarted = true;
                    return Task.Run(() => JoinAmbientPrefix(InferText(BuildAmbientPrompt(location, activity, sequence), AmbientOutputTokens, token)), CancellationToken.None);
                }, ct);
            var validated = ValidateAmbient(text, out var rejectionReason, location, activity);
            if (validated is null)
                logger.LogWarning("ONNX ambient output rejected after {Ms}ms: {Reason}; raw length {Length}, preview truncated {Truncated}: {Raw}",
                    started.ElapsedMilliseconds, rejectionReason, text.Length, text.Length > 120,
                    text.Length > 120 ? text[..120] : text);
            else
                logger.LogInformation("ONNX ambient Generated in {Ms}ms.", started.ElapsedMilliseconds);
            return new(validated, validated is null ? $"Rejected:{rejectionReason}" : null);
        }
        catch (OperationCanceledException ex)
        {
            var reason = ct.IsCancellationRequested ? "CallerCancelled" :
                inferenceStarted ? "InferenceDeadlineExceeded" : "ModelGateDeadlineExceeded";
            logger.LogWarning(ex, "ONNX ambient fallback after {Ms}ms: {Reason}. Canon narration preserved.",
                started.ElapsedMilliseconds, reason);
            return new(null, reason);
        }
        catch (Exception ex)
        {
            var reason = ct.IsCancellationRequested ? "CallerCancelled" :
                started.Elapsed >= InferenceBudget ? "InferenceDeadlineExceeded" : "InferenceError";
            logger.LogWarning(ex, "ONNX ambient fallback after {Ms}ms: {Reason}. Canon narration preserved.",
                started.ElapsedMilliseconds, reason);
            return new(null, reason);
        }
    }

    internal static async Task<string> RunBoundedAsync(SemaphoreSlim gate, TimeSpan budget,
        Func<CancellationToken, Task<string>> inference, CancellationToken ct)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(budget);
        await gate.WaitAsync(deadline.Token);
        try
        {
            // The backend must terminate the native session on cancellation. Await its exit
            // before releasing the gate; never orphan inference with WaitAsync/WhenAny.
            var result = await inference(deadline.Token);
            deadline.Token.ThrowIfCancellationRequested();
            return result;
        }
        finally { gate.Release(); }
    }

    private string InferText(string prompt, int outputTokens, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (model is null || tokenizer is null) throw new InvalidOperationException("Model not loaded.");
        using var parameters = new GeneratorParams(model);
        using var input = tokenizer.Encode(prompt);
        var promptLength = input[0].Length;
        parameters.SetSearchOption("max_length", promptLength + outputTokens);
        parameters.SetSearchOption("do_sample", false);
        using var generator = new Generator(model, parameters);
        // GenAI termination interrupts an already-running CPU prefill/token operation.
        // Registration is disposed before generator destruction, preventing a native use-after-free.
        using var cancellation = ct.Register(() =>
        {
            try { generator.SetRuntimeOption("terminate_session", "1"); }
            catch (Exception ex) { logger.LogError(ex, "Native ONNX session termination failed."); }
        });
        ct.ThrowIfCancellationRequested();
        generator.AppendTokenSequences(input);
        var text = "";
        while (!generator.IsDone())
        {
            ct.ThrowIfCancellationRequested();
            generator.GenerateNextToken();
            text = tokenizer.Decode(generator.GetSequence(0)[promptLength..].ToArray());
            // Stop at the first sentence end: CPU time is spent only on the one line we keep.
            if (HasSentenceEnd(text)) break;
        }
        ct.ThrowIfCancellationRequested();
        return text;
    }

    internal static bool HasSentenceEnd(string text)
    {
        var t = text.TrimEnd();
        return t.Contains("<|end|>", StringComparison.Ordinal) || t.Contains("<|endoftext|>", StringComparison.Ordinal)
            || (t.Length > 8 && t[^1] is '.' or '!' or '?');
    }

    internal const int AmbientOutputTokens = 32;
    // Prefilled into the assistant turn so Phi continues in the first person.
    internal const string AmbientPrefix = "I consider";

    // SentencePiece suffix decoding may drop the leading word space.
    internal static string JoinAmbientPrefix(string continuation) =>
        continuation.Length > 0 && char.IsLetter(continuation[0])
            ? $"{AmbientPrefix} {continuation}"
            : AmbientPrefix + continuation;

    internal static string BuildAmbientPrompt(string location, string activity, long sequence = 0) =>
        "<|system|>\nWrite one complete 6-10-word dry joke ending in a period. " +
        "First person, physical objects, not digital. No actions, discoveries, other people, rewards or combat. " +
        "No explanation. Examples: I distrust socks with executive ambitions. I consider optimism a defective compass.<|end|>\n" +
        $"<|user|>\nShort opinion about {RaiderCommsContent.StyleHint(activity, sequence)}.<|end|>\n<|assistant|>\n{AmbientPrefix}";

    private static readonly HashSet<string> DeniedWords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Outcome, reward and combat claims: the Comms facts line owns these.
        "loot", "looted", "looting", "scrap", "salvage", "item", "items", "found", "find", "finding", "discovered",
        "recovered", "recover", "reward", "rewards", "xp", "coin", "coins", "credit", "credits", "gold", "cash",
        "money", "treasure", "kill", "killed", "kills", "dead", "died", "die", "death", "damage", "damaged",
        "health", "hp", "hurt", "wound", "wounded", "injured", "bleeding", "robot", "robots", "drone", "drones",
        "enemy", "enemies", "attack", "attacked", "shot", "shoot", "fight", "fought", "won", "win", "lost",
        "lose", "extracted", "extract", "extraction", "escaped", "handler", "raider", "raiders", "teammate",
        "squad", "everyone", "nobody", "someone",
        // Third-person/other-actor narration.
        "he", "she", "they", "them", "him", "her", "his", "hers", "their", "we", "us", "our"
    };

    private static readonly HashSet<string> FirstPersonOpeners = new(StringComparer.Ordinal)
    { "I", "I'm", "I've", "I'd", "I'll" };

    internal static string? ValidateAmbient(string text, params string[] allowedContext)
        => ValidateAmbient(text, out _, allowedContext);

    internal static string? ValidateAmbient(string text, out string? rejectionReason, params string[] allowedContext)
    {
        rejectionReason = null;
        // Strip only generation trailers and surrounding wrapper quotes; content is never rewritten.
        text = Regex.Replace(text, @"<\|(end|endoftext)\|>.*$", "", RegexOptions.Singleline).Trim();
        text = text.Trim('"', '\'', '\u201C', '\u201D', ' ').Trim();
        if (text.Length is < 8 or > 140)
        {
            rejectionReason = "LengthOutside8To140Characters";
            return null;
        }
        if (text.Contains("<|", StringComparison.Ordinal) || text.Contains('\n'))
        {
            rejectionReason = "GenerationArtifactOrMultiline";
            return null;
        }
        if (text[^1] is not ('.' or '!' or '?'))
        {
            rejectionReason = "MissingSentenceTerminator";
            return null;
        }
        if (!Regex.IsMatch(text, @"^[A-Za-z][A-Za-z ,'\-]*[.!?]$", RegexOptions.CultureInvariant))
        {
            rejectionReason = "InvalidCharactersOrMultipleSentences";
            return null;
        }
        var words = text[..^1].Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is < 4 or > 20)
        {
            rejectionReason = "WordCountOutside4To20";
            return null;
        }
        if (!FirstPersonOpeners.Contains(words[0]))
        {
            rejectionReason = "NotFirstPerson";
            return null;
        }
        var allowed = new HashSet<string>(allowedContext
            .SelectMany(c => c.Split([' ', ',', '-'], StringSplitOptions.RemoveEmptyEntries)), StringComparer.OrdinalIgnoreCase);
        foreach (var word in words.Skip(1))
        {
            var bare = word.Trim('\'', '-');
            if (DeniedWords.Contains(bare))
            {
                rejectionReason = $"ForbiddenOutcomeOrActorWord:{bare}";
                return null;
            }
            // Capitalized words are names/places; only the given location/activity context may appear.
            if (bare.Length > 0 && char.IsUpper(bare[0]) && !FirstPersonOpeners.Contains(bare) && !allowed.Contains(bare))
            {
                rejectionReason = $"UnknownProperNoun:{bare}";
                return null;
            }
        }
        return text;
    }
    public static IReadOnlyList<string> ParseBullets(string text)
        => text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => Regex.Replace(l.Trim().TrimStart('▸', '-', '*', '•').Trim(), @"^\d+[.)]\s*", ""))
            .Where(l => l.Length > 0).Take(3).ToArray();

    public void Dispose()
    {
        modelGate.Wait();
        try
        {
            disposed = true;
            tokenizer?.Dispose();
            model?.Dispose();
            tokenizer = null;
            model = null;
        }
        finally { modelGate.Release(); }
    }
}
