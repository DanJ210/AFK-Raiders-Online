using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;

if (args is ["--compare", var before, var after])
{
    DiaryAuditReport.Write(before, after);
    return;
}
if (args.Length > 0 && args is not ["--diagnostic"])
    throw new ArgumentException("Usage: no arguments, --diagnostic, or --compare <before.json> <after.json>.");

// Isolated simulations only. Output is diagnostics, never a save or gameplay input.
int[] seeds = [101, 202, 303];
int[] horizons = [8, 24];
var diagnostic = args.Contains("--diagnostic", StringComparer.Ordinal);
if (diagnostic) { seeds = [101]; horizons = [8]; }
var start = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
var root = Directory.GetCurrentDirectory();
var catalog = JsonNode.Parse(File.ReadAllText(Path.Combine(root,
    @"server\AFKRaiders.Server\Content\narration-catalog.json")))!;
var pools = catalog["pools"]!.AsArray().ToDictionary(
    node => node!["id"]!.GetValue<string>(),
    node => node!["lines"]!.AsArray().Select(line => line!.GetValue<string>()).ToArray(),
    StringComparer.Ordinal);
var kinds = Regex.Matches(File.ReadAllText(Path.Combine(root,
    @"server\AFKRaiders.Server\Domain\RaidSimulation.cs")),
    "Emit\\(\\s*[^,]+,\\s*\"([A-Za-z]+)\"")
    .Select(match => match.Groups[1].Value).Concat(["RobotDefeated", "RobotEscaped"])
    .Distinct().Order().ToArray();
var runs = new List<RunResult>();
foreach (var trait in Enum.GetValues<RaiderPrimaryTrait>().Where(trait => !diagnostic || trait == RaiderPrimaryTrait.Cautious))
foreach (var seed in seeds)
{
    var clock = new FixedTimeProvider(start);
    var nextSeed = 0;
    var nextId = 0;
    var coordinator = new RaidCoordinator(clock, narrator: new TemplateRaidNarrator(),
        narrativeGenerator: new TemplateNarrativeGenerator(),
        nextSeed: () => nextSeed++ == 0 ? (int)trait : seed * 1000 + nextSeed,
        traderSeed: seed, nextId: () => $"analysis-{seed}-{nextId++:D4}");
    var enrolled = coordinator.EnrollHandler("raider-one", $"diary-variety-{trait}-{seed}");
    if (enrolled.Projection.PrimaryTrait != trait.ToString())
        throw new InvalidOperationException($"Expected {trait}, got {enrolled.Projection.PrimaryTrait}.");
    for (var seconds = 0; seconds <= horizons.Max() * 3600; seconds += 5)
    {
        clock.Now = start.AddSeconds(seconds);
        coordinator.AdvanceRaids();
        if (!horizons.Contains(seconds / 3600) || seconds % 3600 != 0) continue;
        var projection = coordinator.GetHandlerProjection(enrolled.AccessToken);
        var journal = projection.FullJournal
            ?? throw new InvalidOperationException("Projection did not include its journal.");
        runs.Add(Measure(trait.ToString(), seed, seconds / 3600, journal, projection));
    }
}
var summaries = runs.GroupBy(run => (run.Trait, run.Hours)).Select(group => new
{
    group.Key.Trait, group.Key.Hours, Seeds = group.Select(run => run.Seed).ToArray(),
    JournalEntries = group.Sum(run => run.JournalEntries),
    Quotes = group.Sum(run => run.Quotes.Count),
    Repeat20 = group.Sum(run => run.Quotes.Count(quote => quote.Repeat20)),
    Repeat50 = group.Sum(run => run.Quotes.Count(quote => quote.Repeat50)),
    ByKind = Breakdown(group.SelectMany(run => run.Quotes), quote => quote.Kind),
    ByPool = Breakdown(group.SelectMany(run => run.Quotes), quote => quote.Pool),
    HubStays = group.SelectMany(run => run.HubStays).ToArray()
}).ToArray();
var routing = kinds.Select(kind => new
{
    Kind = kind,
    Observed = runs.Where(run => run.Hours == 24).SelectMany(run => run.Quotes)
        .Where(quote => quote.Kind == kind).GroupBy(quote => (quote.Trait, quote.Pool))
        .Select(group => new { group.Key.Trait, group.Key.Pool, Size = pools[group.Key.Pool].Length,
            Quotes = group.Count() }).ToArray()
}).ToArray();
Console.WriteLine(JsonSerializer.Serialize(new
{
    Seeds = seeds, Horizons = horizons, ContentVersion = catalog["contentVersion"]!.GetValue<string>(),
    PoolSizes = pools.Where(pair => pair.Key.StartsWith("voice.") || pair.Key == "hub.activities")
        .ToDictionary(pair => pair.Key, pair => pair.Value.Length),
    Routing = routing, TraitSummary = summaries,
    Runs = runs.Select(run => new { run.Trait, run.Seed, run.Hours, run.JournalEntries,
        Quotes = run.Quotes.Count, run.AuthoritativeFingerprint, run.HubStays,
        AuthoritativeOutcome = diagnostic ? run.AuthoritativeOutcome : null,
        ByKind = Breakdown(run.Quotes, quote => quote.Kind),
        ByPool = Breakdown(run.Quotes, quote => quote.Pool) })
}, new JsonSerializerOptions { WriteIndented = true }));

RunResult Measure(string trait, int seed, int hours, IReadOnlyList<HandlerJournalEntryView> journal,
    HandlerProjection projection)
{
    var quotes = new List<Quote>();
    var hubStays = new List<HubStay>();
    var hubLines = new List<string>();
    var stay = 0;
    foreach (var entry in journal.OrderBy(entry => entry.Sequence))
    {
        if (entry.Kind == "RaidAdmission")
        {
            if (hubLines.Count > 0)
                hubStays.Add(new(seed, stay, hubLines.Count, hubLines.Distinct().Count(), false));
            hubLines.Clear();
            stay++;
        }
        if (entry.Kind == "HubActivity") hubLines.Add(entry.Text);
        const string marker = "\nRaider: ";
        var index = entry.Text.LastIndexOf(marker, StringComparison.Ordinal);
        if (index < 0) continue;
        var text = entry.Text[(index + marker.Length)..].Trim();
        var matchingPools = pools.Where(pair => pair.Key.StartsWith("voice.") &&
            pair.Value.Contains(text, StringComparer.Ordinal)).Select(pair => pair.Key).ToArray();
        if (matchingPools.Length != 1)
            throw new InvalidOperationException($"Quote must identify exactly one pool: {text}");
        quotes.Add(new(trait, entry.Kind, matchingPools[0], text,
            quotes.TakeLast(20).Any(quote => quote.Text == text),
            quotes.TakeLast(50).Any(quote => quote.Text == text)));
    }
    if (hubLines.Count > 0)
        hubStays.Add(new(seed, stay, hubLines.Count, hubLines.Distinct().Count(), true));
    var outcome = JsonSerializer.SerializeToNode(projection)!;
    RemovePresentation(outcome);
    var weaponIds = new Dictionary<string, string>(StringComparer.Ordinal);
    var canonicalOutcome = Regex.Replace(outcome.ToJsonString(), @"wpn_tea_kettle_[a-f0-9]{5}(?![a-z0-9])",
        match =>
        {
            if (!weaponIds.TryGetValue(match.Value, out var id))
                weaponIds.Add(match.Value, id = $"starter-kettle-{weaponIds.Count}");
            return id;
        });
    return new(trait, seed, hours, journal.Count, quotes, hubStays,
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalOutcome))), canonicalOutcome);
}

static object[] Breakdown(IEnumerable<Quote> source, Func<Quote, string> key)
{
    var quotes = source.ToArray();
    return quotes.GroupBy(key).OrderByDescending(group => group.Count()).Select(group => (object)new
    {
        Id = group.Key, Quotes = group.Count(), AllQuotes = quotes.Length,
        Share = group.Count() / (double)quotes.Length,
        Repeat20 = group.Count(quote => quote.Repeat20),
        Repeat50 = group.Count(quote => quote.Repeat50),
        Repeat20Share = group.Count(quote => quote.Repeat20) / (double)group.Count(),
        Repeat50Share = group.Count(quote => quote.Repeat50) / (double)group.Count(),
        Top10 = group.GroupBy(quote => quote.Text).OrderByDescending(lines => lines.Count())
            .ThenBy(lines => lines.Key, StringComparer.Ordinal).Take(10)
            .Select(lines => new { Text = lines.Key, Count = lines.Count() }).ToArray()
    }).ToArray();
}

static void RemovePresentation(JsonNode node)
{
    if (node is JsonValue value && value.TryGetValue<string>(out var text))
    {
        var index = text.IndexOf("\nRaider: ", StringComparison.Ordinal);
        if (index >= 0) node.ReplaceWith(text[..index]);
    }
    else if (node is JsonObject obj)
    {
        foreach (var key in new[] { "Text", "Flavor", "NarrativeSource", "EnrichmentStatus",
            "EnrichmentReason", "AccessToken", "DevelopmentAccessToken", "GeneratedAt",
            "DebriefSummary", "Highlights", "AarDebrief", "TopItemAppraisal" }) obj.Remove(key);
        foreach (var child in obj.ToArray()) if (child.Value is not null) RemovePresentation(child.Value);
    }
    else if (node is JsonArray array)
        foreach (var child in array.ToArray()) if (child is not null) RemovePresentation(child);
}

sealed class FixedTimeProvider(DateTimeOffset initial) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = initial;
    public override DateTimeOffset GetUtcNow() => Now;
}
sealed record Quote(string Trait, string Kind, string Pool, string Text, bool Repeat20, bool Repeat50);
sealed record HubStay(int Seed, int Stay, int Entries, int UniqueLines, bool Partial);
sealed record RunResult(string Trait, int Seed, int Hours, int JournalEntries,
    List<Quote> Quotes, List<HubStay> HubStays, string AuthoritativeFingerprint, string AuthoritativeOutcome);
