using System.Text.RegularExpressions;
using AFKRaiders.Server.Content;
using AFKRaiders.Server.Domain;

// Read-only model of the C5 document's explicit gates, not a runtime selector.
internal sealed class CandidateAudit
{
    private readonly Candidate[] candidates;
    private readonly HashSet<string> eligible = new(StringComparer.Ordinal);
    private readonly HashSet<string> priority;

    internal CandidateAudit(string path)
    {
        var text = File.ReadAllText(path);
        candidates = text.Split('\n')
            .Select(line => line.Trim().Trim('|').Split('|').Select(cell => cell.Trim()).ToArray())
            .Where(cells => cells.Length == 10 && Regex.IsMatch(cells[0], @"\A[A-Z]{2}-[BCGH]-\d{2}\z"))
            .Select(cells => new Candidate(cells[0], cells[2], cells[3], cells[4], cells[7])).ToArray();
        var section = text.Split("### REVIEW THESE FIRST", StringSplitOptions.None)[1]
            .Split("### Needs human read:", StringSplitOptions.None)[0];
        section = section[section.IndexOf("1. RS-C-03", StringComparison.Ordinal)..];
        priority = Regex.Matches(section, @"\b[A-Z]{2}-[BCGH]-\d{2}\b")
            .Select(match => match.Value).ToHashSet(StringComparer.Ordinal);
        if (candidates.Length != 624 || candidates.Select(c => c.Id).Distinct().Count() != 624 ||
            priority.Count != 152 || priority.Except(candidates.Select(c => c.Id)).Any())
            throw new InvalidDataException("C5 candidate or priority list count changed; inspect the document before measuring.");
        foreach (var candidate in candidates)
        {
            var declared = Regex.Matches(candidate.Slots, @"\{[A-Za-z]+\}").Select(m => m.Value).ToHashSet();
            var used = Regex.Matches(candidate.Text, @"\{[A-Za-z]+\}").Select(m => m.Value).ToHashSet();
            if (!declared.SetEquals(used) || candidate.Shape is not ("J" or "M" or "O"))
                throw new InvalidDataException($"Invalid C5 row {candidate.Id}.");
        }
        SelfCheck();
    }

    private void SelfCheck()
    {
        var at = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
        void Check(string id, RaidEvent evt, bool expected)
        {
            var candidate = candidates.Single(c => c.Id == id);
            var actual = Matches(candidate, evt, DiarySlotExtractor.Extract(evt, "one"));
            if (actual != expected) throw new InvalidOperationException($"Candidate gate self-check failed: {id}.");
        }
        var pc = new RaidEvent(1, at, "PersonalityChanged", ["one"], Location: "Market",
            Reason: "Staying deeper", OriginalText: "Staying deeper: Greed 1 -> 2; Mood 0 -> 0; resilience 0.");
        Check("PC-B-01", pc, true);
        Check("PC-B-07", pc, false);
        Check("PC-B-03", pc, false);
        var shield = pc with { Kind = "ShieldAbsorbed", OriginalText = "Shield lost 0 charge, 0 HP got through. Raider called that 'a learning invoice.'" };
        Check("SH-B-03", shield, false);
        Check("SH-B-03", shield with { OriginalText = "Shield lost 2 charge, 0 HP got through. Raider called that 'a learning invoice.'" }, true);
        var warning = pc with { Kind = "HazardWarning", Reason = "light_fog",
            OriginalText = "Light Fog: exposed footing at Market. Impact in 10s; 12 raw damage before resilience/shields." };
        Check("HW-B-01", warning, true);
        Check("HW-B-05", warning, false);
        var robot = pc with { Kind = "RobotEscaped", OriginalText = "Anxietick overran all fighters. The Service Depot opportunity is lost for this raid." };
        Check("RX-B-01", robot, false);
        Check("RX-B-01", robot with { OriginalText = "Anxietick escapes after 3 rounds. The Service Depot opportunity is lost for this raid." }, true);
        var shared = pc with { Kind = "SearchCompleted", Reason = null, Awards = null,
            OriginalText = "Staycation locker search complete. One recovered Single Sock." };
        Check("SI-B-01", shared, false);
        Check("SI-B-03", shared, true);
        Check("SX-B-01", pc with { Kind = "OpportunityExhausted", OpportunityId = "market-locker" }, false);
    }

    internal void Observe(string trait, RaidEvent evt, DiarySlots slots)
    {
        var traitCode = trait switch
        {
            "Brawler" => 'B', "Cautious" => 'C', "Gadgeteer" => 'G', "Hoarder" => 'H',
            _ => throw new InvalidOperationException($"Unknown trait: {trait}")
        };
        foreach (var candidate in candidates.Where(c => c.Trait == traitCode && !eligible.Contains(c.Id)))
            if (Matches(candidate, evt, slots)) eligible.Add(candidate.Id);
    }

    internal object Report()
    {
        var never = candidates.Select(c => c.Id).Where(id => !eligible.Contains(id)).Order().ToArray();
        return new
        {
            Candidates = candidates.Length, Eligible = eligible.Count, NeverEligible = never,
            PriorityCandidates = priority.Count, PriorityEligible = eligible.Count(priority.Contains),
            PriorityNeverEligible = never.Where(priority.Contains).ToArray(),
            Notice = "Hypothetical C5 explicit fact/slot gates only; all lines remain draft and need human semantic and grammar review."
        };
    }

    private static bool Matches(Candidate c, RaidEvent evt, DiarySlots slots)
    {
        if (!KindMatches(c.Family, evt.Kind) || !SlotsAvailable(c.Slots, slots)) return false;
        var text = evt.OriginalText;
        if (text is null) return false;

        if (c.Family is not ("AD" or "HS") && slots.Location is null &&
            c.Family is not ("UD" or "RS")) return false;
        switch (c.Family)
        {
            case "AI":
                return slots.Location is not null && slots.Activity is not null;
            case "PC":
                if (evt.Reason is null || slots.GreedDirection is null && slots.MoodDirection is null) return false;
                if (c.Slots.Contains("{activity}", StringComparison.Ordinal) &&
                    evt.Reason is not ("Completed searching" or "Completed recovering" or "Completed rummaging" or
                        "Completed salvaging" or "Completed dismantling")) return false;
                foreach (var (word, direction, actual) in new[]
                {
                    ("Greed up only", DiaryDirection.Increased, slots.GreedDirection),
                    ("Greed down only", DiaryDirection.Decreased, slots.GreedDirection),
                    ("Mood up only", DiaryDirection.Increased, slots.MoodDirection),
                    ("Mood down only", DiaryDirection.Decreased, slots.MoodDirection)
                })
                    if (c.Fit.Contains(word, StringComparison.OrdinalIgnoreCase) && actual != direction) return false;
                var requiredReason = c.Number switch
                {
                    3 => "Applied White Bandage", 6 => "Applied Blue Bandage",
                    9 => "Used Panic Paddles", 12 => "Applied Purple Bandage", _ => null
                };
                return requiredReason is null || evt.Reason == requiredReason && slots.MoodDirection == DiaryDirection.Increased;
            case "SH":
                return slots.PositiveShieldLoss;
            case "UD" or "RS":
                return c.Shape == "O" || slots.Source is not null;
            case "EL" or "ES":
                return evt.Reason is not null && Regex.IsMatch(text,
                    c.Family == "EL" ? @" exit chance; heading for extraction\. Greed -?\d+\.\z"
                        : @" exit chance; staying for now\. Greed -?\d+\.\z");
            case "SI":
                if (!Regex.IsMatch(text, @"(?:\. |: |; )[^;]+ recovered [^;]+")) return false;
                var expectedItem = c.Number switch
                {
                    2 => c.Trait switch { 'B' => "Single Sock", 'C' => "Lost Diary Page", 'G' => "Half-Full Battery", _ => "Vintage Comic Book" },
                    5 => c.Trait switch { 'B' => "Robot Toe", 'C' => "Left Boot", 'G' => "Broken Servo", _ => "Rusted Spork" },
                    _ => null
                };
                return expectedItem is null || slots.Item == expectedItem;
            case "SN":
                if (!text.EndsWith("complete. .", StringComparison.Ordinal)) return false;
                var expectedPrefix = c.Id switch
                {
                    "SN-B-01" or "SN-C-03" or "SN-G-03" or "SN-H-01" => "Staycation locker search complete.",
                    "SN-B-02" or "SN-G-02" or "SN-H-04" => "Retail-shelf rummage complete.",
                    "SN-B-03" or "SN-C-01" => "Lost-and-found recovery complete.",
                    "SN-C-02" or "SN-G-04" or "SN-H-02" => "Service Depot tool-cage rummage complete.",
                    _ => null
                };
                return expectedPrefix is null || text.StartsWith(expectedPrefix, StringComparison.Ordinal);
            case "SX":
                return c.Number switch
                {
                    2 => evt.OpportunityId == "market-locker" && slots.Location == "Market",
                    3 => evt.OpportunityId == "humming-crate" && slots.Location == "Salvage Yard",
                    _ => true
                };
            case "AR":
                return c.Number switch
                {
                    1 => slots.Location == "Market", 2 => slots.Location == "Salvage Yard",
                    4 => slots.Location == "Service Depot", 5 => slots.Location == "Extraction Platform",
                    _ => true
                };
            case "AD":
                return c.Number == 3 || evt.Kind == "Departure" && slots.Location == "Entry Tunnel";
            case "HW" or "HI":
                if (slots.Condition is null) return false;
                var condition = c.Number switch
                {
                    1 => ("light_fog", "Light Fog"), 5 => ("apology_weather", "Apology Weather"),
                    8 => ("overtime_red", "Overtime Red"),
                    2 when c.Family == "HW" && c.Trait is 'B' or 'H' => ("overtime_red", "Overtime Red"),
                    2 when c.Family == "HW" && c.Trait is 'C' or 'G' => ("apology_weather", "Apology Weather"),
                    _ => ((string?)null, (string?)null)
                };
                return condition.Item1 is null || evt.Reason == condition.Item1 && slots.Condition == condition.Item2;
            case "RD" or "RX":
                if (c.Family == "RX" && !Regex.IsMatch(text, @"\A.+ escapes after \d+ rounds\. The Service Depot opportunity is lost for this raid\.\z")) return false;
                var robot = Regex.Match(c.Fit, @"\b(Anxietick|Roomba Prime|Passive-Aggressor Drone) only\b");
                return !robot.Success || slots.Robot == robot.Groups[1].Value;
            case "RC":
                return slots.Location == "Service Depot";
            case "HS":
                if (slots.Item is null) return false;
                return c.Number != 3 || slots.Item == (c.Trait switch
                {
                    'B' or 'C' => "Single Sock", 'G' => "Left Boot", _ => "Vintage Comic Book"
                });
            case "KO":
                // A whole-sentence deadline reason fits quoted or colon constructions, not prepositions.
                return slots.Source is null || !slots.Source.StartsWith("World deadline expired", StringComparison.Ordinal) ||
                    !c.Slots.Contains("{source}", StringComparison.Ordinal) ||
                    c.Text.Contains("'{source}'", StringComparison.Ordinal) || c.Text.Contains(": {source}", StringComparison.Ordinal);
            default:
                return true;
        }
    }

    private static bool KindMatches(string family, string kind) => family switch
    {
        "AI" => kind == "ActivityInterrupted", "PC" => kind == "PersonalityChanged",
        "SH" => kind == "ShieldAbsorbed", "UD" => kind == "UnmitigatedDamage", "RS" => kind == "Resilience",
        "EL" or "ES" => kind == "ExtractionDecision", "SI" => kind is "SearchCompleted" or "EncounterResolved",
        "SN" => kind == "SearchCompleted", "SX" => kind == "OpportunityExhausted", "AR" => kind == "Arrived",
        "AD" => kind is "DutyAdmission" or "Departure" or "Deployment", "PS" => kind == "PackingStarted",
        "PF" => kind == "PackingCompleted", "HW" => kind == "HazardWarning", "HI" => kind == "ConditionImpact",
        "RD" => kind == "RobotDefeated", "RX" => kind == "RobotEscaped", "RC" => kind == "RobotDefeatCargoResolved",
        "RV" => kind is "RecoveryStarted" or "RecoveryCompleted", "DN" => kind == "Downed",
        "KO" => kind == "KnockedOut", "DL" => kind == "DeadlineExpired", "HS" => kind == "HiddenPocketSalvaged",
        "HE" => kind == "HiddenPocketEmpty", _ => throw new InvalidDataException($"Unknown C5 family {family}")
    };

    private static bool SlotsAvailable(string declared, DiarySlots s)
    {
        foreach (Match match in Regex.Matches(declared, @"\{([A-Za-z]+)\}"))
            if (match.Groups[1].Value switch
            {
                "location" => s.Location is null, "item" => s.Item is null, "source" => s.Source is null,
                "condition" => s.Condition is null, "robot" => s.Robot is null, "activity" => s.Activity is null,
                "opportunity" => s.Opportunity is null, "greedDirection" => s.GreedDirection is null,
                "moodDirection" => s.MoodDirection is null, _ => throw new InvalidDataException("Unknown C5 slot")
            }) return false;
        return true;
    }

    private sealed record Candidate(string Id, string Shape, string Slots, string Text, string Fit)
    {
        internal string Family => Id[..2];
        internal char Trait => Id[3];
        internal int Number => int.Parse(Id[5..], System.Globalization.CultureInfo.InvariantCulture);
    }
}
