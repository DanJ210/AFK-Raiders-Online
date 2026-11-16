using System.Text.Json;
using AFKRaiders.Server.AI;
using AFKRaiders.Server.Content;
using AFKRaiders.Server.Domain;

internal static class SlotCoverage
{
    internal static void Write()
    {
        var counts = new Dictionary<string, Coverage>(StringComparer.Ordinal);
        var interrupted = new Dictionary<string, int>(StringComparer.Ordinal);
        var audit = new CandidateAudit(Path.Combine(Directory.GetCurrentDirectory(), @"docs\DIARY_PILOT_LINES.md"));
        var start = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
        foreach (var trait in Enum.GetValues<RaiderPrimaryTrait>())
        foreach (var seed in new[] { 101, 202, 303 })
        {
            var clock = new FixedTimeProvider(start);
            var nextSeed = 0;
            var nextId = 0;
            var narrator = new CapturingNarrator(evt =>
            {
                if (evt.ParticipantIds.Count != 1) return;
                var slots = DiarySlotExtractor.Extract(evt, evt.ParticipantIds[0]);
                if (evt.Kind == "ActivityInterrupted")
                {
                    var activity = evt.Activity ?? "<missing>";
                    interrupted[activity] = interrupted.GetValueOrDefault(activity) + 1;
                }
                audit.Observe(trait.ToString(), evt, slots);
                if (!counts.TryGetValue(evt.Kind, out var coverage))
                    counts.Add(evt.Kind, coverage = new());
                coverage.Add(slots);
            });
            var coordinator = new RaidCoordinator(clock, narrator: narrator,
                narrativeGenerator: new TemplateNarrativeGenerator(),
                nextSeed: () => nextSeed++ == 0 ? (int)trait : seed * 1000 + nextSeed,
                traderSeed: seed, nextId: () => $"analysis-{seed}-{nextId++:D4}");
            var enrolled = coordinator.EnrollHandler("raider-one", $"diary-variety-{trait}-{seed}");
            if (enrolled.Projection.PrimaryTrait != trait.ToString())
                throw new InvalidOperationException($"Expected {trait}, got {enrolled.Projection.PrimaryTrait}.");
            for (var seconds = 0; seconds <= 24 * 3600; seconds += 5)
            {
                clock.Now = start.AddSeconds(seconds);
                coordinator.AdvanceRaids();
            }
        }
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            Coverage = counts.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value),
            InterruptedActivities = interrupted.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value),
            CandidateAudit = audit.Report()
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class CapturingNarrator(Action<RaidEvent> capture) : IRaidNarrator
    {
        private readonly TemplateRaidNarrator inner = new();
        public string FormatEvent(RaidEvent evt, RaiderProjection? raider)
        {
            capture(evt);
            return inner.FormatEvent(evt, raider);
        }
        public ValueTask EnqueueEnrichmentAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default) =>
            inner.EnqueueEnrichmentAsync(request, ct);
    }

    private sealed class Coverage
    {
        public int Total { get; private set; }
        public int Location { get; private set; }
        public int Item { get; private set; }
        public int Source { get; private set; }
        public int Condition { get; private set; }
        public int Robot { get; private set; }
        public int Activity { get; private set; }
        public int Opportunity { get; private set; }
        public int GreedDirection { get; private set; }
        public int MoodDirection { get; private set; }
        public int PositiveShieldLoss { get; private set; }

        internal void Add(DiarySlots slots)
        {
            Total++;
            if (slots.Location is not null) Location++;
            if (slots.Item is not null) Item++;
            if (slots.Source is not null) Source++;
            if (slots.Condition is not null) Condition++;
            if (slots.Robot is not null) Robot++;
            if (slots.Activity is not null) Activity++;
            if (slots.Opportunity is not null) Opportunity++;
            if (slots.GreedDirection is not null) GreedDirection++;
            if (slots.MoodDirection is not null) MoodDirection++;
            if (slots.PositiveShieldLoss) PositiveShieldLoss++;
        }
    }
}
