using AFKRaiders.Server.Content;

namespace AFKRaiders.Server.Domain;

// Activity-scoped adaptations of AFK-original ambient/search/flavor content.
public static class RaiderCommsContent
{
    public static string PoolKey(string kind, string? activity) => kind switch
    {
        "AreaActivityStarted" or "AreaActivityCompleted" or "AreaDiary" => $"{kind}:{activity}",
        "RobotRound" or "RobotCombatStarted" or "RobotParticipantJoined" => "Robot",
        "ExtractionStarted" or "Extracted" => "Exit",
        "Travel" or "Search" or "ItemAllocated" or "ConsumableUsed" or "WorldCondition" => kind,
        _ => "General"
    };

    public static IReadOnlyList<string> ResidenceTasks(string activity, string location) => activity switch
    {
        "Surveying" => NarrationContent.Default.Pool("tasks.surveying"),
        "Checking" => NarrationContent.Default.Pool("tasks.checking"),
        _ when location == "Market" => NarrationContent.Default.Pool("tasks.market"),
        _ => NarrationContent.Default.Pool("tasks.general")
    };

    public static string Line(string kind, string? activity, long sequence, string? disposition = null, bool rotate = false)
    {
        IReadOnlyList<string> lines = (kind, activity) switch
        {
            ("AreaDiary", "Surveying") =>
                NarrationContent.Default.Pool("voice.diary-surveying"),
            ("AreaDiary", "Checking") =>
                NarrationContent.Default.Pool("voice.diary-checking"),
            ("AreaDiary", _) =>
                NarrationContent.Default.Pool("voice.diary-general"),
            ("ConsumableUsed", _) =>
                NarrationContent.Default.Pool("voice.consumable"),
            ("WorldCondition", _) =>
                NarrationContent.Default.Pool("voice.condition"),
            ("AreaActivityStarted", "Surveying") =>
                NarrationContent.Default.Pool("voice.start-surveying"),
            ("AreaActivityStarted", "Checking") =>
                NarrationContent.Default.Pool("voice.start-checking"),
            ("AreaActivityStarted", "Rummaging") =>
                NarrationContent.Default.Pool("voice.start-rummaging"),
            ("AreaActivityCompleted", "Surveying") =>
                NarrationContent.Default.Pool("voice.complete-surveying"),
            ("AreaActivityCompleted", "Checking") =>
                NarrationContent.Default.Pool("voice.complete-checking"),
            ("AreaActivityCompleted", "Rummaging") =>
                NarrationContent.Default.Pool("voice.complete-rummaging"),
            ("Travel", _) =>
                NarrationContent.Default.Pool("voice.travel"),
            ("Search", _) =>
                NarrationContent.Default.Pool("voice.search"),
            ("ItemAllocated", _) =>
                NarrationContent.Default.Pool("voice.allocation"),
            ("RobotRound" or "RobotCombatStarted" or "RobotParticipantJoined", _) =>
                NarrationContent.Default.Pool("voice.robot"),
            ("ExtractionStarted" or "Extracted", _) =>
                NarrationContent.Default.Pool("voice.exit"),
            _ when disposition == "Hoarder" =>
                NarrationContent.Default.Pool("voice.hoarder"),
            _ =>
                NarrationContent.Default.Pool("voice.general")
        };
        return lines[rotate ? (int)((ulong)sequence % (ulong)lines.Count) : PickIndex(sequence, lines.Count)];
    }

    public static string StyleHint(string activity, long sequence = 0)
    {
        IReadOnlyList<string> themes = activity switch
        {
            "Hub" or "AtHome" => NarrationContent.Default.Pool("style.hub"),
            "Checking" => NarrationContent.Default.Pool("style.checking"),
            "Surveying" => NarrationContent.Default.Pool("style.surveying"),
            "Rummaging" => NarrationContent.Default.Pool("style.rummaging"),
            "Search" or "Searching" => NarrationContent.Default.Pool("style.search"),
            "RobotRound" or "Fighting" or "FightingRobot" => NarrationContent.Default.Pool("style.robot"),
            "ExtractionStarted" or "Extracting" => NarrationContent.Default.Pool("style.exit"),
            _ => NarrationContent.Default.Pool("style.general")
        };
        return themes[PickIndex(sequence, themes.Count)];
    }

    private static int PickIndex(long sequence, int count)
    {
        var mixed = unchecked((ulong)sequence * 11400714819323198485UL);
        return (int)((mixed ^ (mixed >> 32)) % (uint)count);
    }
}
