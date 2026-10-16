using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;
using System.Diagnostics;
using System.Threading.Channels;

namespace AFKRaiders.Server.Tests;

public sealed class CommsAndPacingTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-13T12:00:00Z");

    [Theory]
    [InlineData(2, false)]
    [InlineData(10, false)]
    [InlineData(15, false)]
    [InlineData(2, true)]
    [InlineData(10, true)]
    [InlineData(15, true)]
    public void NormalRaidHasBoundedResidenceAndFiniteRewardsWithoutHandler(int seed, bool shared)
    {
        var sim = new RaidSimulation("normal", Start, seed, riskReward: false);
        sim.AddAutonomousRaider("one", "raider-one", Start);
        if (shared) sim.AddAutonomousRaider("two", "raider-two", Start);
        sim.Advance(Start.AddSeconds(1200));
        Assert.True(sim.Completed);
        var one = sim.Project("one", Start.AddSeconds(1200));
        var projections = shared ? new[] { one, sim.Project("two", Start.AddSeconds(1200)) } : [one];
        foreach (var p in projections)
        {
            var terminal = Assert.Single(p.Events, e => e.Kind == "Extracted");
            Assert.InRange((terminal.At - Start).TotalSeconds, 900, 1200);
            var beats = p.Events.Where(e => e.Kind == "AreaActivityStarted").ToArray();
            Assert.Equal(15, beats.Length);
            Assert.Equal(15, p.Events.Count(e => e.Kind == "AreaActivityCompleted"));
            foreach (var area in new[] { "Market", "Salvage Yard", "Service Depot", "Extraction Platform" })
                Assert.True(beats.Count(e => e.Location == area) >= 3);
            Assert.All(beats, e =>
            {
                Assert.Null(e.ItemInstanceId);
                Assert.Null(e.HealthDelta);
                Assert.Contains("no new loot opportunity", e.FactsText);
                Assert.Contains("Raider: I", e.Text);
            });
        }
        var loot = projections.SelectMany(p => p.MyExtractedLoot).ToArray();
        Assert.Equal(one.Robot.Status == "Defeated" ? 6 : 4, loot.Length);
        Assert.Equal(loot.Length, loot.Select(i => i.InstanceId).Distinct().Count());
        var eventsBefore = one.Events.Count;
        var revision = sim.Revision;
        Assert.False(sim.Advance(Start.AddHours(1)));
        Assert.Equal(revision, sim.Revision);
        Assert.Equal(eventsBefore, sim.Project("one", Start.AddHours(1)).Events.Count);
    }

    [Fact]
    public void ProductionCoordinatorUsesNormalPacingAndRunsWithoutReads()
    {
        var time = new Clock();
        var coordinator = new RaidCoordinator(time, nextSeed: () => 2);
        var handler = coordinator.EnrollHandler("raider-one");
        time.Now = Start.AddSeconds(60);
        coordinator.AdvanceRaids();
        Assert.Null(coordinator.GetHandlerProjection(handler.AccessToken).LastReturnSummary);
        time.Now = Start.AddSeconds(1200);
        coordinator.AdvanceRaids();
        var summary = coordinator.GetHandlerProjection(handler.AccessToken).LastReturnSummary;
        Assert.NotNull(summary);
        Assert.InRange((summary.ExtractedAt - Start).TotalSeconds, 60, 1200);
        Assert.Contains(coordinator.GetHandlerProjection(handler.AccessToken).FullJournal!,
            e => e.Kind == "ExtractionDecision");
    }

    [Fact]
    public void EveryEmissionUsesNarratorAndKeepsOriginalFacts()
    {
        var narrator = new RecordingNarrator();
        var sim = new RaidSimulation("wire", Start, 2, narrator, fastPacing: true);
        sim.AddAutonomousRaider("one", "raider-one", Start);
        sim.Advance(Start.AddSeconds(300));
        var events = sim.Project("one", Start.AddSeconds(300)).Events;
        Assert.Equal(events.Count, narrator.Formatted.Count);
        Assert.All(events, e =>
        {
            Assert.StartsWith("FORMATTED ", e.Text);
            Assert.Equal(narrator.Formatted.Single(f => f.Sequence == e.Sequence).OriginalText, e.FactsText);
        });
    }

    [Fact]
    public void CallbackUpdatesJournalBySourceAndActiveThreadWithoutChangingFactsOrLeaking()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        var coordinator = new RaidCoordinator(time, narrator, narrativeGenerator: new LiveStub());
        var one = coordinator.EnrollHandler("raider-one");
        var two = coordinator.EnrollHandler("raider-two");
        coordinator.AdvanceRaids();
        var before = coordinator.GetHandlerProjection(one.AccessToken);
        var request = narrator.Requests.First(r => r.EventType == "Travel" && r.RaiderId == one.Projection.PlayerId);
        var source = $"raid:{request.RaidId}:{request.EventSequence}";
        var entry = before.FullJournal!.Single(e => e.SourceEventId == source);
        Assert.Equal("Pending", entry.EnrichmentStatus);
        const string aside = "I feel strategically embarrassed about my judgment.";
        request.OnResult!.Invoke(new(aside, "Generated"));
        var after = coordinator.GetHandlerProjection(one.AccessToken);
        var updated = Assert.Single(after.FullJournal!, e => e.SourceEventId == source);
        Assert.Equal(entry.FactsText, updated.FactsText);
        Assert.Equal(entry.Text, updated.Text);
        Assert.Equal("Generated", updated.NarrativeSource);
        Assert.Equal("Ready", updated.EnrichmentStatus);
        Assert.Equal(aside, updated.Flavor);
        Assert.Contains(after.Raid!.CurrentThread!.Logs, l => l.Contains(aside));
        Assert.DoesNotContain(coordinator.GetHandlerProjection(two.AccessToken).FullJournal!, e => e.SourceEventId == source);
        var revision = after.Raid.Revision;
        request.OnResult(new(aside, "Generated"));
        Assert.Equal(revision, coordinator.GetHandlerProjection(one.AccessToken).Raid!.Revision);
    }

    [Fact]
    public void SharedCallbackIsQueuedOnceAndPropagatesIdenticalFlavorToActualParticipants()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        var seed = Enumerable.Range(0, 512).First(s =>
        {
            var simulation = new RaidSimulation("fixture", Start, s, fastPacing: true);
            simulation.AddAutonomousRaider("one", "raider-one", Start);
            simulation.Advance(Start.AddSeconds(30));
            simulation.AddAutonomousRaider("two", "raider-two", Start.AddSeconds(30));
            simulation.Advance(Start.AddSeconds(180));
            return simulation.Project("one", Start.AddSeconds(180)).Events.Any(e => e.Kind == "RobotRound" && e.ParticipantIds.Count == 2);
        });
        var coordinator = new RaidCoordinator(time, narrator, narrativeGenerator: new LiveStub(), fastPacing: true, nextSeed: () => seed);
        var one = coordinator.EnrollHandler("raider-one");
        var two = coordinator.EnrollHandler("raider-two");
        NarrativeEnrichmentRequest? shared = null;
        for (var i = 0; i < 180 && shared is null; i++)
        {
            foreach (var pending in narrator.Requests.Where(r => r.EventType != "AarDebrief").ToArray())
                pending.OnResult?.Invoke(new("I feel deeply suspicious about the paperwork.", "Generated"));
            time.Now = Start.AddSeconds(i);
            coordinator.AdvanceRaids();
            var p = coordinator.GetHandlerProjection(one.AccessToken).Raid;
            var evt = p?.Events.FirstOrDefault(e => e.Kind == "RobotRound" && e.ParticipantIds.Count == 2);
            if (evt is not null) shared = narrator.Requests.Single(r => r.RaidId == p!.RaidId && r.EventSequence == evt.Sequence);
        }
        Assert.NotNull(shared);
        shared.OnResult!.Invoke(new("I feel deeply suspicious about the paperwork.", "Generated"));
        var source = $"raid:{shared.RaidId}:{shared.EventSequence}";
        var a = coordinator.GetHandlerProjection(one.AccessToken).FullJournal!.Single(e => e.SourceEventId == source);
        var b = coordinator.GetHandlerProjection(two.AccessToken).FullJournal!.Single(e => e.SourceEventId == source);
        Assert.Equal(a.Flavor, b.Flavor);
        Assert.Equal(a.FactsText, b.FactsText);
        Assert.Equal(a.Text, b.Text);
    }

    [Fact]
    public void LateAndInvalidCallbacksKeepCanonAndCannotOverwriteNextRaid()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        var coordinator = new RaidCoordinator(time, narrator, narrativeGenerator: new LiveStub());
        var one = coordinator.EnrollHandler("raider-one");
        coordinator.AdvanceRaids();
        var request = narrator.Requests.First(r => r.EventType == "Travel");
        request.OnResult!.Invoke(new("I recovered 99 scrap and killed a dragon.", "Generated"));
        var source = $"raid:{request.RaidId}:{request.EventSequence}";
        Assert.Null(coordinator.GetHandlerProjection(one.AccessToken).FullJournal!.Single(e => e.SourceEventId == source).Flavor);
        time.Now = Start.AddSeconds(1300);
        coordinator.AdvanceRaids();
        request.OnResult(new("I feel quietly hopeful about my dignity.", "Generated"));
        var projection = coordinator.GetHandlerProjection(one.AccessToken);
        Assert.Null(projection.FullJournal!.Single(e => e.SourceEventId == source).Flavor);
        Assert.DoesNotContain(projection.Raid?.Events ?? [], e => e.Flavor is not null);
    }

    [Theory]
    [InlineData("I feel quietly hopeful about my dignity.", true)]
    [InlineData("I'm suspicious of this ceiling, frankly.", true)]
    [InlineData("I've decided this dust has a personal grudge!", true)]
    [InlineData("I could get used to the creaking around the Market.", true)]
    [InlineData("I feel quietly hopeful about my dignity.<|end|>", true)]
    [InlineData("\"I feel quietly hopeful about my dignity.\"", true)]
    [InlineData("I feel victorious after recovering 500 scrap.", false)]
    [InlineData("I found something shiny in the rubble.", false)]
    [InlineData("I hope the robot stays asleep tonight.", false)]
    [InlineData("I think they are watching me again.", false)]
    [InlineData("I miss Rosa and her terrible soup.", false)]
    [InlineData("I hate this. I love this.", false)]
    [InlineData("I \"feel\" fine.", false)]
    [InlineData("Noticed a pile of dusty paperwork.", false)]
    [InlineData("Noticed paperwork.", false)]
    [InlineData("I feel.", false)]
    public void AmbientGuardAcceptsVariedFeelingsAndRejectsArtifactsAndWorldClaims(string text, bool accepted)
        => Assert.Equal(accepted, OnnxGenAiNarrativeGenerator.ValidateAmbient(text, "Market", "surveying the stalls") is not null);

    [Fact]
    public void AmbientPromptIsShortPhiChatTemplateWithFirstPersonPrefill()
    {
        var prompt = OnnxGenAiNarrativeGenerator.BuildAmbientPrompt("Market", "surveying the stalls");
        Assert.StartsWith("<|system|>\n", prompt);
        Assert.Contains("<|end|>\n<|user|>\n", prompt);
        Assert.EndsWith("<|end|>\n<|assistant|>\nI consider", prompt);
        Assert.Contains("complete 6-10-word dry joke ending in a period", prompt);
        Assert.Contains("First person, physical objects, not digital.", prompt);
        Assert.Contains("No actions, discoveries, other people, rewards or combat.", prompt);
        Assert.Contains("Examples: I distrust socks with executive ambitions. I consider optimism a defective compass.", prompt);
        // Keep activity-specific parody guidance compact for CPU prefill.
        Assert.True(prompt.Split([' ', '\n'], StringSplitOptions.RemoveEmptyEntries).Length <= 65, prompt);
        Assert.Equal(32, OnnxGenAiNarrativeGenerator.AmbientOutputTokens);
    }

    [Theory]
    [InlineData("I'm scavenging through the digital wasteland, sifting through the spoils with a skeptical", "MissingSentenceTerminator")]
    [InlineData("I feel quietly", "MissingSentenceTerminator")]
    [InlineData("I feel quietly.\nI feel hopeful.", "GenerationArtifactOrMultiline")]
    [InlineData("I feel quietly. I feel hopeful.", "InvalidCharactersOrMultipleSentences")]
    [InlineData("I feel hopeful.", "WordCountOutside4To20")]
    [InlineData("This paperwork feels deeply judgmental.", "NotFirstPerson")]
    [InlineData("I found something shiny in the rubble.", "ForbiddenOutcomeOrActorWord:found")]
    [InlineData("I miss Rosa very much.", "UnknownProperNoun:Rosa")]
    [InlineData("I", "LengthOutside8To140Characters")]
    public void AmbientRejectionHasSpecificReasonWithoutRepairingText(string text, string expectedReason)
    {
        Assert.Null(OnnxGenAiNarrativeGenerator.ValidateAmbient(text, out var reason, "Market"));
        Assert.Equal(expectedReason, reason);
    }

    [Fact]
    public void AcceptedAmbientHasNoRejectionReason()
    {
        const string text = "I'm treating my zipper as opposing counsel.";
        Assert.Equal(text, OnnxGenAiNarrativeGenerator.ValidateAmbient(text, out var reason));
        Assert.Null(reason);
    }

    [Theory]
    [InlineData("Surveying")]
    [InlineData("Checking")]
    [InlineData("Rummaging")]
    public void ActivityParodyVariesAndCompletionHasItsOwnVoice(string activity)
    {
        var starts = Enumerable.Range(1, 90)
            .Select(i => RaiderCommsContent.Line("AreaActivityStarted", activity, i * 6)).Distinct().ToArray();
        var completions = Enumerable.Range(1, 90)
            .Select(i => RaiderCommsContent.Line("AreaActivityCompleted", activity, i * 6)).Distinct().ToArray();
        Assert.Equal(6, starts.Length);
        Assert.Equal(4, completions.Length);
        Assert.All(starts.Concat(completions), line =>
        {
            Assert.StartsWith("I", line);
            Assert.InRange(line.Length, 20, 140);
        });
        Assert.Empty(starts.Intersect(completions));
        Assert.Equal(RaiderCommsContent.Line("AreaActivityStarted", activity, 42),
            RaiderCommsContent.Line("AreaActivityStarted", activity, 42));
        Assert.Equal(3, Enumerable.Range(0, 90).Select(i => RaiderCommsContent.StyleHint(activity, i * 6)).Distinct().Count());
    }

    [Fact]
    public void GeneratedPromptUsesActivityCanonAndSequenceWithoutNewFacts()
    {
        var first = OnnxGenAiNarrativeGenerator.BuildAmbientPrompt("Market", "Checking", 0);
        var next = OnnxGenAiNarrativeGenerator.BuildAmbientPrompt("Market", "Checking", 1);
        Assert.Contains("zippers", first);
        Assert.Contains("backpacks", next);
        Assert.NotEqual(first, next);
        Assert.Contains("No actions, discoveries, other people, rewards or combat.", first);
        Assert.DoesNotContain("Market", first);
        Assert.Contains("timers", OnnxGenAiNarrativeGenerator.BuildAmbientPrompt("Extraction Platform", "Extracting"));
    }

    [Theory]
    [InlineData(" socks unreliable.", "I consider socks unreliable.")]
    [InlineData("socks unreliable.", "I consider socks unreliable.")]
    [InlineData("", "I consider")]
    public void AmbientPrefillJoinsStrippedSpacesWithoutRewritingOutput(string continuation, string expected)
        => Assert.Equal(expected, OnnxGenAiNarrativeGenerator.JoinAmbientPrefix(continuation));

    [Theory]
    [InlineData(" feel oddly calm.", true)]
    [InlineData(" feel oddly", false)]
    [InlineData(" feel<|end|>", true)]
    public void InferenceStopsAtFirstSentenceEnd(string text, bool done)
        => Assert.Equal(done, OnnxGenAiNarrativeGenerator.HasSentenceEnd(text));

    [Fact]
    public async Task CancellationTerminatesStartedInferenceAndDoesNotOrphanOrOverlapIt()
    {
        using var gate = new SemaphoreSlim(1, 1);
        using var cancellation = new CancellationTokenSource();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var terminated = false;
        var exited = false;
        var inference = OnnxGenAiNarrativeGenerator.RunBoundedAsync(gate, TimeSpan.FromSeconds(10), async token =>
            {
                using var registration = token.Register(() => terminated = true);
                started.SetResult();
                try { await Task.Delay(Timeout.InfiniteTimeSpan, token); }
                finally { exited = true; }
                return "unreachable";
            }, cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => inference);
        Assert.True(terminated);
        Assert.True(exited);
        Assert.Equal(1, gate.CurrentCount);
        Assert.Equal("next", await OnnxGenAiNarrativeGenerator.RunBoundedAsync(gate, TimeSpan.FromSeconds(1),
            _ => Task.FromResult("next"), CancellationToken.None));
    }

    [Fact]
    public async Task DeadlineWhileWaitingForModelDoesNotStartInferenceOrReleaseUnownedGate()
    {
        using var gate = new SemaphoreSlim(0, 1);
        var entered = false;
        var watch = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OnnxGenAiNarrativeGenerator.RunBoundedAsync(gate, TimeSpan.FromMilliseconds(50), _ =>
            {
                entered = true;
                return Task.FromResult("unreachable");
            }, CancellationToken.None));
        Assert.False(entered);
        Assert.Equal(0, gate.CurrentCount);
        Assert.InRange(watch.ElapsedMilliseconds, 30, 2000);
    }

    [Fact]
    public async Task FallbackAarKeepsArchivalSourceThroughWorkerCallback()
    {
        var channel = Channel.CreateBounded<NarrativeEnrichmentRequest>(1);
        var received = new TaskCompletionSource<NarrativeResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var context = new AarDebriefContext("aar", "Test", "Extracted", 0, 0, null, 0, null, 15, []);
        using var worker = new NarrativeEnrichmentWorker(channel.Reader, new TemplateNarrativeGenerator());
        await worker.StartAsync(CancellationToken.None);
        channel.Writer.TryWrite(new("aar", "one", 1, "AarDebrief", context, _ => { }, r => received.TrySetResult(r)));
        var result = await received.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal("ArchivalLog", result.Source);
        Assert.Equal(3, OnnxGenAiNarrativeGenerator.ParseBullets(result.Text).Count);
        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void OldAarCallbackCannotReplaceLaterReturnAndFallbackKeepsItsSource()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        var coordinator = new RaidCoordinator(time, narrator, fastPacing: true, nextSeed: () => 10);
        var one = coordinator.EnrollHandler("raider-one");
        for (var i = 1; i <= 240 && narrator.Requests.Count == 0; i++)
        {
            time.Now = Start.AddSeconds(i);
            coordinator.AdvanceRaids();
        }
        var first = Assert.Single(narrator.Requests);
        var baseline = coordinator.GetHandlerProjection(one.AccessToken).LastReturnSummary!.AarDebrief!;
        first.OnResult!.Invoke(new(string.Join("\n", baseline.Bullets), "ArchivalLog"));
        Assert.Equal("ArchivalLog", coordinator.GetHandlerProjection(one.AccessToken).LastReturnSummary!.AarDebrief!.Source);
        for (var i = (int)(time.Now - Start).TotalSeconds + 1; i <= 600 && narrator.Requests.Count < 2; i++)
        {
            time.Now = Start.AddSeconds(i);
            coordinator.AdvanceRaids();
        }
        Assert.Equal(2, narrator.Requests.Count);
        var later = coordinator.GetHandlerProjection(one.AccessToken).LastReturnSummary!;
        Assert.NotEqual(first.RaidId, later.RaidId);
        first.OnResult.Invoke(new("old alpha\nold beta\nold gamma", "Generated"));
        Assert.Equal(later, coordinator.GetHandlerProjection(one.AccessToken).LastReturnSummary);
    }

    [Fact]
    public async Task GateWaitIsAlsoBoundedAndNeverStartsConcurrentInference()
    {
        using var gate = new SemaphoreSlim(0, 1);
        var started = false;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            OnnxGenAiNarrativeGenerator.RunBoundedAsync(gate, TimeSpan.FromMilliseconds(40), _ =>
            {
                started = true;
                return Task.FromResult("should not start");
            }, CancellationToken.None));
        Assert.False(started);
        Assert.Equal(0, gate.CurrentCount);
    }

    [Fact]
    public async Task FullQueueReportsFallbackRatherThanLeavingPendingForever()
    {
        var channel = Channel.CreateBounded<NarrativeEnrichmentRequest>(new BoundedChannelOptions(1)
        { FullMode = BoundedChannelFullMode.Wait });
        var narrator = new TemplateRaidNarrator(channel.Writer);
        var request = new NarrativeEnrichmentRequest("raid", "one", 1, "Travel", new AmbientCommsContext("Market", "Travel"), _ => { });
        await narrator.EnqueueEnrichmentAsync(request);
        NarrativeResult? result = null;
        await narrator.EnqueueEnrichmentAsync(request with { OnResult = r => result = r });
        Assert.Equal("CanonFallback", result?.Source);
        Assert.Equal("QueueFull", result?.FailureReason);
    }

    [Fact]
    public void LiveBurstAdmitsOneRequestAndExplicitlyKeepsOthersAuthoredWithoutRetrying()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        var coordinator = new RaidCoordinator(time, narrator, narrativeGenerator: new LiveStub(), nextSeed: () => 2);
        var one = coordinator.EnrollHandler("raider-one");
        time.Now = Start.AddSeconds(60);
        coordinator.AdvanceRaids();
        var journal = coordinator.GetHandlerProjection(one.AccessToken).FullJournal!;
        Assert.Single(narrator.Requests);
        Assert.Single(journal, e => e.EnrichmentStatus == "Pending");
        var skipped = journal.Where(e => e.EnrichmentReason == "LiveBudgetBusy").ToArray();
        Assert.NotEmpty(skipped);
        Assert.All(skipped, e =>
        {
            Assert.Equal("NotRequested", e.EnrichmentStatus);
            Assert.Equal("CanonFallback", e.NarrativeSource);
            Assert.Null(e.Flavor);
        });
        narrator.Requests[0].OnResult!.Invoke(new("", "ArchivalLog", "InferenceDeadlineExceeded"));
        var updated = coordinator.GetHandlerProjection(one.AccessToken).FullJournal!;
        Assert.Contains(updated, e => e.EnrichmentStatus == "Fallback" &&
            e.EnrichmentReason == "InferenceDeadlineExceeded");
        Assert.Single(narrator.Requests);
        time.Now = Start.AddSeconds(90);
        coordinator.AdvanceRaids();
        Assert.Equal(2, narrator.Requests.Count);
        Assert.DoesNotContain(narrator.Requests, r => skipped.Any(e => e.SourceEventId == $"raid:{r.RaidId}:{r.EventSequence}"));
    }

    [Fact]
    public void UnavailableModelExplainsEligibleAuthoredEntriesWithoutQueuingOrRepeatedRevisions()
    {
        var time = new Clock();
        var narrator = new RecordingNarrator();
        using var generator = new OnnxGenAiNarrativeGenerator();
        var coordinator = new RaidCoordinator(time, narrator, narrativeGenerator: generator, nextSeed: () => 2);
        var one = coordinator.EnrollHandler("raider-one");
        time.Now = Start.AddSeconds(90);
        coordinator.AdvanceRaids();
        var first = coordinator.GetHandlerProjection(one.AccessToken);
        var eligible = first.FullJournal!.Where(e => e.Kind == "AreaActivityStarted").ToArray();
        Assert.NotEmpty(eligible);
        Assert.All(eligible, e =>
        {
            Assert.Equal("CanonFallback", e.NarrativeSource);
            Assert.Equal("NotRequested", e.EnrichmentStatus);
            Assert.Equal("ModelUnavailable", e.EnrichmentReason);
            Assert.Null(e.Flavor);
        });
        Assert.Empty(narrator.Requests);
        var repeated = coordinator.GetHandlerProjection(one.AccessToken);
        Assert.Equal(first.Raid!.Revision, repeated.Raid!.Revision);
        Assert.Equal(first.FullJournal, repeated.FullJournal);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class RecordingNarrator : IRaidNarrator
    {
        public List<RaidEvent> Formatted { get; } = [];
        public List<NarrativeEnrichmentRequest> Requests { get; } = [];
        public string FormatEvent(RaidEvent evt, RaiderProjection? raider)
        {
            Formatted.Add(evt);
            return $"FORMATTED {evt.OriginalText}";
        }
        public ValueTask EnqueueEnrichmentAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
        {
            Requests.Add(request);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class LiveStub : INarrativeGenerator
    {
        private readonly TemplateNarrativeGenerator template = new();
        public string Name => "LiveStub";
        public bool IsAvailable => true;
        public AarDebriefView GenerateAarDebrief(AarDebriefContext c) => template.GenerateAarDebrief(c);
        public ValueTask<AarDebriefView> GenerateAarDebriefAsync(AarDebriefContext c, CancellationToken ct = default) => template.GenerateAarDebriefAsync(c, ct);
        public ItemAppraisalView GenerateItemAppraisal(ItemAppraisalContext c) => template.GenerateItemAppraisal(c);
        public ValueTask<ItemAppraisalView> GenerateItemAppraisalAsync(ItemAppraisalContext c, CancellationToken ct = default) => template.GenerateItemAppraisalAsync(c, ct);
        public ValueTask<string> GenerateEnrichedEventTextAsync(NarrativeEnrichmentRequest r, CancellationToken ct = default) => ValueTask.FromResult("I feel deeply suspicious about the paperwork.");
    }
}
