namespace AFKRaiders.Server.Tests;

using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;
using System.Threading.Channels;

public sealed class NarrativeEnrichmentTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-12T10:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
        public void Advance(TimeSpan delta) => Now += delta;
    }

    [Fact]
    public void TemplateNarrativeGeneratorEmitsThreeToneAccurateBulletsWithHaul()
    {
        var generator = new TemplateNarrativeGenerator();
        var context = new AarDebriefContext(
            RaidId: "raid-test-1",
            RaiderName: "Mira \"Refund\" Malone",
            Outcome: "Extracted",
            SecuredItemCount: 2,
            TotalValue: 45,
            TopItem: new ItemView("item-1", "gyro_stabilizer", "Gyroscopic Stabilizer", 35, "Spins erratically"),
            DamageTaken: 15,
            RobotEncounterResolution: "Escaped",
            FinalGreed: 35,
            JournalHighlights: ["Secured Gyroscopic Stabilizer", "Robot Escaped"]
        );

        var debrief = generator.GenerateAarDebrief(context);

        Assert.NotNull(debrief);
        Assert.Equal("ArchivalLog", debrief.Source);
        Assert.Equal(3, debrief.Bullets.Count);
        Assert.Contains("Gyroscopic Stabilizer", debrief.Bullets[0]);
        Assert.Contains("45 scrap", debrief.Bullets[0]);
        Assert.Contains("Roomba Prime", debrief.Bullets[1]);
        Assert.Contains("Greed factor peaked at 35", debrief.Bullets[2]);
    }

    [Fact]
    public void TemplateNarrativeGeneratorHandlesZeroHaulAndDefeatedRobot()
    {
        var generator = new TemplateNarrativeGenerator();
        var context = new AarDebriefContext(
            RaidId: "raid-test-2",
            RaiderName: "Dev \"Wet Socks\" Kowalski",
            Outcome: "Extracted",
            SecuredItemCount: 0,
            TotalValue: 0,
            TopItem: null,
            DamageTaken: 0,
            RobotEncounterResolution: "Defeated",
            FinalGreed: 15,
            JournalHighlights: ["Robot Defeated"]
        );

        var debrief = generator.GenerateAarDebrief(context);

        Assert.Equal(3, debrief.Bullets.Count);
        Assert.Contains("zero salvage", debrief.Bullets[0]);
        Assert.Contains("disabled", debrief.Bullets[1]);
        Assert.Contains("safety margins", debrief.Bullets[2]);
    }

    [Fact]
    public async Task OnnxGenAiNarrativeGeneratorFallsBackGracefullyWhenModelMissing()
    {
        using var generator = new OnnxGenAiNarrativeGenerator(modelPath: "C:\\NonExistent\\Model\\Path");

        Assert.False(generator.IsAvailable);
        Assert.Equal("OnnxGenAiNarrator", generator.Name);
        var result = await generator.GenerateEventResultAsync(new("missing", "one", 1, "Travel",
            new AmbientCommsContext("Market", "Travel"), _ => { }));
        Assert.Equal("ArchivalLog", result.Source);
        Assert.Equal("ModelUnavailable", result.FailureReason);

        var context = new AarDebriefContext(
            RaidId: "raid-test-3",
            RaiderName: "Mira \"Refund\" Malone",
            Outcome: "Extracted",
            SecuredItemCount: 1,
            TotalValue: 10,
            TopItem: new ItemView("item-1", "scrap_basic", "Basic Scrap", 10, "Scrap"),
            DamageTaken: 0,
            RobotEncounterResolution: null,
            FinalGreed: 15,
            JournalHighlights: []
        );

        // Sync call
        var syncDebrief = generator.GenerateAarDebrief(context);
        Assert.Equal(3, syncDebrief.Bullets.Count);
        Assert.Equal("ArchivalLog", syncDebrief.Source);

        // Async call
        var asyncDebrief = await generator.GenerateAarDebriefAsync(context);
        Assert.Equal(3, asyncDebrief.Bullets.Count);
        Assert.Equal("ArchivalLog", asyncDebrief.Source);
    }

    [Theory]
    [InlineData("▸ First line\n▸ Second line\n▸ Third line")]
    [InlineData("• First bullet\n• Second bullet\n• Third bullet")]
    [InlineData("- Line 1\n- Line 2\n- Line 3")]
    [InlineData("1. One item\n2. Two item\n3. Three item")]
    [InlineData("1) Point A\n2) Point B\n3) Point C\n4) Point D (extra)")]
    public void ParseBulletsExtractsExactlyThreeBulletsAcrossFormats(string rawOutput)
    {
        var bullets = OnnxGenAiNarrativeGenerator.ParseBullets(rawOutput);

        Assert.Equal(3, bullets.Count);
        Assert.All(bullets, b => Assert.False(string.IsNullOrWhiteSpace(b)));
        Assert.All(bullets, b => Assert.DoesNotMatch(@"^[▸•\-\*0-9\.\)\s]", b));
    }

    [Fact]
    public async Task NarrativeEnrichmentWorkerProcessesChannelRequestsAsynchronously()
    {
        var channel = Channel.CreateBounded<NarrativeEnrichmentRequest>(10);
        var mockGenerator = new CustomTestGenerator();
        var worker = new NarrativeEnrichmentWorker(channel.Reader, mockGenerator);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var workerTask = worker.StartAsync(cts.Token);

        var generatedResult = string.Empty;
        var tcs = new TaskCompletionSource<string>();

        var request = new NarrativeEnrichmentRequest(
            RaidId: "raid-worker-1",
            RaiderId: "raider-1",
            EventSequence: 1,
            EventType: "AarDebrief",
            EventPayload: new AarDebriefContext(
                RaidId: "raid-worker-1",
                RaiderName: "Test Raider",
                Outcome: "Extracted",
                SecuredItemCount: 1,
                TotalValue: 20,
                TopItem: null,
                DamageTaken: 0,
                RobotEncounterResolution: null,
                FinalGreed: 20,
                JournalHighlights: []
            ),
            OnGenerated: text =>
            {
                generatedResult = text;
                tcs.TrySetResult(text);
            }
        );

        await channel.Writer.WriteAsync(request, cts.Token);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(2000, cts.Token));
        Assert.Same(tcs.Task, completed);
        Assert.Contains("Mock Bullet 1", generatedResult);

        await worker.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void RaidCoordinatorAttachesAarDebriefToReturnSummaryAndEnqueuesEnrichment()
    {
        var time = new ManualTime();
        var channel = Channel.CreateBounded<NarrativeEnrichmentRequest>(10);
        var narrator = new TemplateRaidNarrator(channel.Writer);
        var coordinator = new RaidCoordinator(time, narrator, fastPacing: true);

        var enrolled = coordinator.EnrollHandler("raider-one");
        var token = enrolled.AccessToken;

        // Advance simulation using coordinator.AdvanceRaids() until Raider extracts and returns home (~90-150s)
        for (var index = 0; index < 240; index++)
        {
            time.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            if (coordinator.GetHandlerProjection(token).LastReturnSummary is not null)
                break;
        }

        var status = coordinator.GetHandlerProjection(token);
        Assert.NotNull(status.LastReturnSummary);
        Assert.NotNull(status.LastReturnSummary.AarDebrief);
        Assert.Equal("ArchivalLog", status.LastReturnSummary.AarDebrief.Source);
        Assert.Equal(3, status.LastReturnSummary.AarDebrief.Bullets.Count);

        // Verify that an enrichment request was enqueued in the channel
        Assert.True(channel.Reader.TryRead(out var enqueuedRequest));
        Assert.Equal("AarDebrief", enqueuedRequest.EventType);
        Assert.Equal(status.LastReturnSummary.RaidId, enqueuedRequest.RaidId);

        // Invoke the callback simulate async SLM enrichment completion
        enqueuedRequest.OnResult!.Invoke(new("▸ AI Bullet Alpha\n▸ AI Bullet Beta\n▸ AI Bullet Gamma", "Generated"));

        // Verify updated AAR debrief source is now "Generated"
        var updatedStatus = coordinator.GetHandlerProjection(token);
        Assert.NotNull(updatedStatus.LastReturnSummary?.AarDebrief);
        Assert.Equal("Generated", updatedStatus.LastReturnSummary.AarDebrief.Source);
        Assert.Equal("AI Bullet Alpha", updatedStatus.LastReturnSummary.AarDebrief.Bullets[0]);
        Assert.Equal("AI Bullet Beta", updatedStatus.LastReturnSummary.AarDebrief.Bullets[1]);
        Assert.Equal("AI Bullet Gamma", updatedStatus.LastReturnSummary.AarDebrief.Bullets[2]);
    }

    private sealed class CustomTestGenerator : INarrativeGenerator
    {
        public string Name => "MockGenerator";
        public bool IsAvailable => true;

        public AarDebriefView GenerateAarDebrief(AarDebriefContext context) =>
            new(["Mock Bullet 1", "Mock Bullet 2", "Mock Bullet 3"], "Mock", DateTimeOffset.UtcNow);

        public ValueTask<AarDebriefView> GenerateAarDebriefAsync(AarDebriefContext context, CancellationToken ct = default) =>
            ValueTask.FromResult(GenerateAarDebrief(context));

        public ValueTask<string> GenerateEnrichedEventTextAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default) =>
            ValueTask.FromResult("Mock Enriched Event");

        public ItemAppraisalView GenerateItemAppraisal(ItemAppraisalContext context) =>
            new(context.CatalogId, context.ItemName, "Mock Appraisal", "Mock Classification", "Mock", DateTimeOffset.UtcNow);

        public ValueTask<ItemAppraisalView> GenerateItemAppraisalAsync(ItemAppraisalContext context, CancellationToken ct = default) =>
            ValueTask.FromResult(GenerateItemAppraisal(context));
    }
}
