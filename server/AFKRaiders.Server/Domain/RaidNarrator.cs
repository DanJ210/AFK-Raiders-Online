using AFKRaiders.Server.Content;

using System.Threading.Channels;

namespace AFKRaiders.Server.Domain;

/// <summary>
/// Core domain contract for narration as specified in AI_SYSTEMS_DESIGN.md.
/// </summary>
public interface IRaidNarrator
{
    /// <summary>
    /// Synchronously formats an authoritative simulation event into tone-accurate parody narrative text.
    /// </summary>
    string FormatEvent(RaidEvent evt, RaiderProjection? raider);

    /// <summary>
    /// Asynchronous queue for future AI-enriched generation (non-blocking).
    /// </summary>
    ValueTask EnqueueEnrichmentAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default);
}

/// <summary>
/// Asynchronous narrative enrichment request contract for AI pipelines.
/// </summary>
public sealed record NarrativeEnrichmentRequest(
    string RaidId,
    string RaiderId,
    long EventSequence,
    string EventType,
    object EventPayload,
    Action<string> OnGenerated,
    Action<NarrativeResult>? OnResult = null,
    Func<bool>? IsCurrent = null
);

public sealed record NarrativeResult(string Text, string Source, string? FailureReason = null);
public sealed record AmbientCommsContext(string Location, string Activity);
/// <summary>
/// Authoritative typed event record emitted by the domain simulation.
/// </summary>
public sealed record RaidEvent(
    long Sequence,
    DateTimeOffset At,
    string Kind,
    IReadOnlyList<string> ParticipantIds,
    string? Location = null,
    string? Activity = null,
    string? Destination = null,
    int? DurationSeconds = null,
    int? HealthDelta = null,
    int? RobotDamageDealt = null,
    int? RobotDamageTaken = null,
    int? RobotRemainingHealth = null,
    string? ItemName = null,
    int? ItemValue = null,
    string? ItemInstanceId = null,
    string? OpportunityId = null,
    string? Reason = null,
    string? Resolution = null,
    string? ThreadId = null,
    string Channel = "Diary",
    IReadOnlyList<ItemAwardView>? Awards = null,
    IReadOnlyDictionary<string, string>? Details = null,
    string? OriginalText = null
);

/// <summary>
/// Projection summary of a Raider used by the narration engine.
/// </summary>
public sealed record RaiderProjection(
    string PlayerId,
    string Slot,
    string Name,
    string Disposition,
    int Health,
    int MaxHealth,
    int Greed,
    int Aptitude,
    int Initiative,
    int BackpackValue
);

/// <summary>
/// Deterministic, event-bound template engine referencing TONE_VOICE.md and LEGAL_GUARDRAILS.md.
/// </summary>
public sealed class TemplateRaidNarrator : IRaidNarrator
{
    public string FormatEvent(RaidEvent evt, RaiderProjection? raider)
    {
        if (evt.OriginalText is not null)
        {
            var rotates = evt.Details?.TryGetValue("VoiceIndex", out _) == true;
            var voiceIndex = rotates ? long.Parse(evt.Details!["VoiceIndex"], System.Globalization.CultureInfo.InvariantCulture) : evt.Sequence;
            return $"{evt.OriginalText}\nRaider: {RaiderCommsContent.Line(evt.Kind, evt.Activity, voiceIndex, raider?.Disposition, rotates)}";
        }
        var name = raider?.Name ?? NarrationContent.Default.Format("narrator.the-operative");
        return evt.Kind switch
        {
            "Deployment" =>
                NarrationContent.Default.Format("narrator.checks-in-the-handler-is", name),

            "DutyAdmission" =>
                NarrationContent.Default.Format("narrator.enters-the-ongoing-raid-from", name),

            "Departure" =>
                NarrationContent.Default.Format("narrator.departs-from-entry-tunnel-into", name),

            "Travel" =>
                NarrationContent.Default.Format("narrator.leaves-for-eta-s-navigation", name, evt.Location, evt.Destination, evt.DurationSeconds),

            "Arrived" =>
                NarrationContent.Default.Format("narrator.arrives-at-area-looks-structurally", name, evt.Location),

            "Search" or "SearchStarted" =>
                evt.OpportunityId switch
                {
                    "market-locker" =>
                        NarrationContent.Default.Format("narrator.searches-staycation-locker-at-market", name),
                    "humming-crate" =>
                        NarrationContent.Default.Format("narrator.searches-humming-crate-at-salvage", name),
                    "depot-locker" =>
                        NarrationContent.Default.Format("narrator.searches-locker-at-service-depot", name),
                    _ =>
                        NarrationContent.Default.Format("narrator.searches-at-standard-operating-procedure", name, evt.Location)
                },

            "CrossedPaths" or "SearchJoined" =>
                evt.Reason ?? NarrationContent.Default.Format("narrator.crosses-paths-with-another-operative", name, evt.Location),

            "OpportunityExhausted" =>
                NarrationContent.Default.Format("narrator.finds-already-emptied-no-consolation", name, evt.OpportunityId ?? "container"),

            "SearchCompleted" or "EncounterResolved" =>
                FormatSearchResolution(evt, raider),

            "ItemAllocated" or "ItemAwarded" =>
                NarrationContent.Default.Format("narrator.recovered-value-securing-cargo-with", name, evt.ItemName, evt.ItemValue),

            "PackingStarted" =>
                NarrationContent.Default.Format("narrator.begins-packing-salvage-into-backpack", name, evt.ItemValue ?? 0),

            "PackingCompleted" =>
                NarrationContent.Default.Format("narrator.finished-packing-backpack-now-slightly", name),

            "RecoveryStarted" =>
                NarrationContent.Default.Format("narrator.catching-breath-and-recovering-from", name),

            "RecoveryCompleted" =>
                NarrationContent.Default.Format("narrator.recovered-composure-ready-to-proceed", name),

            "RobotCombatStarted" =>
                NarrationContent.Default.Format("narrator.encounters-roomba-prime-at-it", name, evt.Location ?? "Service Depot"),

            "RobotJoined" or "RobotParticipantJoined" =>
                NarrationContent.Default.Format("narrator.joins-combat-against-roomba-prime", name),

            "RobotRound" =>
                FormatRobotRound(evt, raider),

            "RobotDefeated" =>
                NarrationContent.Default.Format("narrator.roomba-prime-is-disabled-the"),

            "RobotEscaped" =>
                NarrationContent.Default.Format("narrator.roomba-prime-escapes-after-rounds"),

            "ExtractionStarted" =>
                NarrationContent.Default.Format("narrator.calls-extraction-the-shuttle-requests", name),

            "Extracted" or "ExtractionCompleted" =>
                NarrationContent.Default.Format("narrator.extracts-the-loot-made-it", name),

            "Downed" =>
                evt.Reason ?? SurvivalCanonContent.GetDownedLine((int)evt.Sequence),

            "KnockedOut" =>
                evt.Reason ?? SurvivalCanonContent.GetKnockedOutLine((int)evt.Sequence),

            "HiddenPocketSalvaged" =>
                evt.Reason ?? NarrationContent.Default.Format("narrator.secret-hidden-pocket-check-x", evt.ItemName, evt.ItemValue),

            "HiddenPocketEmpty" =>
                evt.Reason ?? NarrationContent.Default.Format("narrator.secret-hidden-pocket-check-empty"),

            "ConsumableUsed" =>
                evt.Reason ?? NarrationContent.Default.Format("narrator.used-field-supplies", name),

            "HandlerAction" or "CallExtract" or "Calm" or "Pressure" =>
                evt.Reason ?? NarrationContent.Default.Format("narrator.acknowledged-handler-transmission", name),

            "DutyEnabled" =>
                NarrationContent.Default.Format("narrator.autonomous-duty-is-enabled-the"),

            "DutyPaused" =>
                NarrationContent.Default.Format("narrator.duty-paused-the-raider-will"),

            "ReturnedHome" =>
                NarrationContent.Default.Format("narrator.returns-to-desperanza-with-secured", name, evt.Details?.GetValueOrDefault("ItemCount", "0")),

            "RaidWorldClosed" =>
                NarrationContent.Default.Format("narrator.the-ongoing-raid-closes-after"),

            _ => evt.Reason ?? NarrationContent.Default.Format("narrator.reports-event-at", name, evt.Kind, evt.Location ?? "unknown")
        };
    }

    private static string FormatSearchResolution(RaidEvent evt, RaiderProjection? raider)
    {
        if (evt.Resolution == "SoloSearch")
        {
            var awardSummary = evt.Awards is { Count: > 0 }
                ? string.Join("; ", evt.Awards.Select(award => NarrationContent.Default.Format("narrator.recovered", award.RecipientName, award.ItemName)))
                : NarrationContent.Default.Format("narrator.no-items-recovered");
            return NarrationContent.Default.Format("narrator.solosearch-only-one-eligible-raider", awardSummary);
        }
        if (evt.Resolution == "Cooperation")
        {
            var awardSummary = evt.Awards is { Count: > 0 }
                ? string.Join("; ", evt.Awards.Select(award => NarrationContent.Default.Format("narrator.recovered", award.RecipientName, award.ItemName)))
                : NarrationContent.Default.Format("narrator.no-items-recovered");
            return NarrationContent.Default.Format("narrator.cooperation-both-raiders-cooperate-lower", awardSummary);
        }
        if (evt.Resolution == "Contest")
        {
            var awardSummary = evt.Awards is { Count: > 0 }
                ? string.Join("; ", evt.Awards.Select(award => NarrationContent.Default.Format("narrator.recovered", award.RecipientName, award.ItemName)))
                : "Contested";
            return NarrationContent.Default.Format("narrator.contest-a-hoarder-contests-the", awardSummary);
        }
        return evt.Reason ?? NarrationContent.Default.Format("narrator.search-resolved-at", evt.Location);
    }

    private static string FormatRobotRound(RaidEvent evt, RaiderProjection? raider)
    {
        var remaining = evt.RobotRemainingHealth ?? 0;
        var taken = evt.RobotDamageTaken ?? 10;
        return NarrationContent.Default.Format("narrator.roomba-prime-has-health-it", remaining, taken);
    }

    private readonly ChannelWriter<NarrativeEnrichmentRequest>? writer;

    public TemplateRaidNarrator(ChannelWriter<NarrativeEnrichmentRequest>? writer = null)
    {
        this.writer = writer;
    }

    public ValueTask EnqueueEnrichmentAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
    {
        if (writer is null || !writer.TryWrite(request))
            request.OnResult?.Invoke(new("", "CanonFallback", writer is null ? "QueueUnavailable" : "QueueFull"));
        return ValueTask.CompletedTask;
    }
}
