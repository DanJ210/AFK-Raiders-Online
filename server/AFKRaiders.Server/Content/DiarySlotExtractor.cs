using System.Globalization;
using System.Text.RegularExpressions;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Content;

public enum DiaryDirection { Decreased, Increased }

public sealed record DiarySlots(
    string? Location = null,
    string? Item = null,
    string? Source = null,
    string? Condition = null,
    string? Robot = null,
    string? Activity = null,
    string? Opportunity = null,
    DiaryDirection? GreedDirection = null,
    DiaryDirection? MoodDirection = null,
    bool PositiveShieldLoss = false);

public static class DiarySlotExtractor
{
    // Proposed names are deliberately absent until the designer approves them.
    private static readonly IReadOnlyDictionary<string, string> ApprovedOpportunityNames =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public static DiarySlots Extract(RaidEvent evt, string speakerId) =>
        Extract(evt, speakerId, ApprovedOpportunityNames);

    internal static DiarySlots Extract(RaidEvent evt, string speakerId,
        IReadOnlyDictionary<string, string> approvedOpportunityNames)
    {
        if (string.IsNullOrWhiteSpace(speakerId) || !evt.ParticipantIds.Contains(speakerId, StringComparer.Ordinal))
            return new();

        var text = evt.OriginalText;
        var location = Clean(evt.Location);
        string? item = null, source = null, condition = null, robot = null, activity = null, opportunity = null;
        DiaryDirection? greed = null, mood = null;
        var positiveShieldLoss = false;

        if (evt.ParticipantIds.Count == 1 && Clean(evt.ItemName) is { } ownItem)
        {
            if (evt.Kind == "ItemAllocated" && evt.ItemValue is { } value && text is not null &&
                text.EndsWith($" recovered {ownItem} (+{value} value). Securing cargo with industrial tape.", StringComparison.Ordinal))
                item = ownItem;
            if (evt.Kind == "HiddenPocketSalvaged" && Clean(evt.ItemInstanceId) is not null &&
                evt.ItemValue is { } pocketValue && text ==
                $"Secret Hidden Pocket preserved {ownItem} (Value: {pocketValue})! Medbay drones recovered the concealed salvage.")
                item = ownItem;
        }
        if ((evt.Kind is "SearchCompleted" or "EncounterResolved") && evt.Awards is not null && text is not null)
        {
            var matches = evt.Awards.Where(award => award.RecipientId == speakerId &&
                Clean(award.ItemName) is not null && Clean(award.RecipientName) is not null &&
                Regex.IsMatch(text, $@"(?:\A|\. |: |; ){Regex.Escape(award.RecipientName)} recovered {Regex.Escape(award.ItemName)}(?:; |\.(?: |\z))",
                    RegexOptions.CultureInvariant))
                .Select(award => award.ItemName).Distinct(StringComparer.Ordinal).ToArray();
            if (matches.Length == 1) item = matches[0];
        }

        if (text is not null)
        {
            var sourcePattern = evt.Kind switch
            {
                "Resilience" => @"\APositive mood trimmed \d+ raw damage from (.+)\.\z",
                "UnmitigatedDamage" => @"\AShield depleted! Chassis takes \d+ unmitigated kinetic damage from (.+)\.\z",
                _ => null
            };
            if (sourcePattern is not null)
                source = Capture(text, sourcePattern);
            else if (evt.Kind == "KnockedOut" && Clean(evt.Reason) is { } reason
                && text.EndsWith($" Cause: {reason}.", StringComparison.Ordinal))
                source = reason;

            if (location is not null && (evt.Kind is "HazardWarning" or "ConditionImpact"))
            {
                var locationPattern = Regex.Escape(location);
                var pattern = evt.Kind == "HazardWarning"
                    ? $@"\A(.+?): exposed footing at {locationPattern}\. Impact in 10s; \d+ raw damage before resilience/shields\.\z"
                    : $@"\A(.+?) affects actual occupants of {locationPattern}; \d+ raw damage each\.\z";
                condition = Capture(text, pattern);
            }

            robot = evt.Kind switch
            {
                "RobotDefeated" => Capture(text, @"\A(.+?) is disabled(?:\.|, but no standing contributor remains)"),
                "RobotEscaped" => Capture(text, @"\A(.+?) (?:escapes after \d+ rounds\.|overran all fighters\.|has no remaining fighters\.)"),
                "RobotRound" => Capture(text, @"\A(.+?) has \d+/\d+ health\."),
                "RobotCombatStarted" when location is not null =>
                    BetweenUnique(text, " encounters ", $" at {location}."),
                "RobotParticipantJoined" =>
                    BetweenUnique(text, " joins the fight against ", ". The shared health pool is "),
                _ => null
            };
            if (evt.Kind == "PersonalityChanged" && Clean(evt.Reason) is { } personalityReason)
            {
                var prefix = Regex.Escape(personalityReason);
                var match = Regex.Match(text, $@"\A{prefix}: Greed (-?\d+) -> (-?\d+); Mood (-?\d+) -> (-?\d+); resilience -?\d+\.\z",
                    RegexOptions.CultureInvariant);
                if (match.Success && int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var oldGreed)
                    && int.TryParse(match.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var newGreed)
                    && int.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var oldMood)
                    && int.TryParse(match.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var newMood))
                {
                    greed = Direction(oldGreed, newGreed);
                    mood = Direction(oldMood, newMood);
                    if (personalityReason.StartsWith("Completed ", StringComparison.Ordinal))
                        activity = Gerund(personalityReason["Completed ".Length..]);
                }
            }
            if (evt.Kind == "ActivityInterrupted" && Clean(evt.Activity) is { } interrupted
                && text == $"{interrupted} interrupted for extraction; shared deadlines and other Raiders continue.")
                activity = Gerund(interrupted);
            else if (evt.Kind != "PersonalityChanged" && evt.Kind != "ActivityInterrupted"
                && evt.ParticipantIds.Count == 1 && Clean(evt.Activity) is { } committedActivity)
                activity = Gerund(committedActivity);

            if (evt.Kind == "ShieldAbsorbed")
            {
                var shield = Regex.Match(text, @"\AShield lost (\d+) charge, \d+ HP got through\. Raider called that 'a learning invoice\.'\z",
                    RegexOptions.CultureInvariant);
                positiveShieldLoss = shield.Success && int.TryParse(shield.Groups[1].Value,
                    NumberStyles.None, CultureInfo.InvariantCulture, out var lost) && lost > 0;
            }
        }

        if (evt.OpportunityId is { } id && approvedOpportunityNames.TryGetValue(id, out var name) &&
            Clean(name) is { } displayName && !string.Equals(id, displayName, StringComparison.OrdinalIgnoreCase))
            opportunity = displayName;
        return new(location, item, source, condition, robot, activity, opportunity, greed, mood, positiveShieldLoss);
    }

    private static string? Gerund(string value) => value.ToLowerInvariant() switch
    {
        "searching" => "searching",
        "recovering" => "recovering",
        "rummaging" => "rummaging",
        "salvaging" => "salvaging",
        "dismantling" => "dismantling",
        "surveying" => "surveying",
        "checking" => "checking",
        "packing" => "packing",
        "fightingrobot" => "fighting a robot",
        "waitingforexitdecision" => "waiting for an exit decision",
        _ => null
    };

    private static DiaryDirection? Direction(int oldValue, int newValue) =>
        newValue > oldValue ? DiaryDirection.Increased : newValue < oldValue ? DiaryDirection.Decreased : null;

    private static string? Capture(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.CultureInvariant);
        return match.Success ? Clean(match.Groups[1].Value) : null;
    }

    private static string? BetweenUnique(string text, string prefix, string suffix)
    {
        var start = text.IndexOf(prefix, StringComparison.Ordinal);
        if (start <= 0 || text.IndexOf(prefix, start + 1, StringComparison.Ordinal) >= 0) return null;
        start += prefix.Length;
        var end = text.IndexOf(suffix, start, StringComparison.Ordinal);
        if (end <= start || text.IndexOf(suffix, end + 1, StringComparison.Ordinal) >= 0) return null;
        return Clean(text[start..end]);
    }

    private static string? Clean(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value == value.Trim() && !value.Any(char.IsControl) ? value : null;
}
