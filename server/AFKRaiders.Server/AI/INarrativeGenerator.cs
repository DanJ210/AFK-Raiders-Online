namespace AFKRaiders.Server.AI;

using AFKRaiders.Server.Domain;

/// <summary>
/// A concise 3-bullet deadpan after-action report presented to the Handler in Desperanza.
/// Conforms to docs/AI_SYSTEMS_DESIGN.md Pipeline C and docs/lore/content-guidelines/TONE_VOICE.md.
/// </summary>
public sealed record AarDebriefView(
    IReadOnlyList<string> Bullets,
    string Source, // "Generated" or "ArchivalLog"
    DateTimeOffset GeneratedAt
);

/// <summary>
/// Authoritative simulation facts aggregated for Post-Raid AAR debriefing.
/// </summary>
public sealed record AarDebriefContext(
    string RaidId,
    string RaiderName,
    string Outcome, // "Extracted" or "KnockedOut"
    int SecuredItemCount,
    int TotalValue,
    ItemView? TopItem,
    int DamageTaken,
    string? RobotEncounterResolution,
    int FinalGreed,
    IReadOnlyList<string> JournalHighlights,
    string? CauseOfDefeat = null,
    ItemView? PocketSalvagedItem = null,
    int LostHaulValue = 0
);

/// <summary>
/// Context provided for AI Step 3 deadpan item appraisal and strategic overvaluation.
/// </summary>
public sealed record ItemAppraisalContext(
    string CatalogId,
    string ItemName,
    int ItemValue,
    int Rarity,
    string Category,
    string ItemFlavor,
    string RaiderName,
    string Tone = "DeadpanOvervaluation"
);

/// <summary>
/// A deadpan strategic appraisal evaluating mundane junk as critical infrastructure.
/// Conforms to docs/AI_SYSTEMS_DESIGN.md Step 3 and docs/lore/content-guidelines/TONE_VOICE.md.
/// </summary>
public sealed record ItemAppraisalView(
    string CatalogId,
    string ItemName,
    string Appraisal,
    string StrategicClassification,
    string Source, // "Generated" or "ArchivalLog"
    DateTimeOffset GeneratedAt
);

/// <summary>
/// Contract for narrative skinning, event enrichment, and post-raid AAR debriefs.
/// </summary>
public interface INarrativeGenerator
{
    string Name { get; }
    bool IsAvailable { get; }

    /// <summary>
    /// Synchronously formats an authoritative AAR context using fast template rules (0ms delay).
    /// </summary>
    AarDebriefView GenerateAarDebrief(AarDebriefContext context);

    /// <summary>
    /// Asynchronously generates an enriched AAR debrief (e.g. via local ONNX SLM or fallback).
    /// </summary>
    ValueTask<AarDebriefView> GenerateAarDebriefAsync(AarDebriefContext context, CancellationToken ct = default);

    /// <summary>
    /// Synchronously generates a deadpan item appraisal and strategic overvaluation (0ms delay).
    /// </summary>
    ItemAppraisalView GenerateItemAppraisal(ItemAppraisalContext context);

    /// <summary>
    /// Asynchronously generates an enriched deadpan item appraisal (e.g. via local ONNX SLM or fallback).
    /// </summary>
    ValueTask<ItemAppraisalView> GenerateItemAppraisalAsync(ItemAppraisalContext context, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously generates enriched narrative text for an in-flight raid event.
    /// </summary>
    ValueTask<string> GenerateEnrichedEventTextAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default);

    async ValueTask<NarrativeResult> GenerateEventResultAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default)
        => new(await GenerateEnrichedEventTextAsync(request, ct), "ArchivalLog");
}
