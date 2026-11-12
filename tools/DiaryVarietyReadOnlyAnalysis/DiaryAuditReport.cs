using System.Globalization;
using System.Text.Json.Nodes;

internal static class DiaryAuditReport
{
    internal static void Write(string beforePath, string afterPath)
    {
        var before = JsonNode.Parse(File.ReadAllText(beforePath))!;
        var after = JsonNode.Parse(File.ReadAllText(afterPath))!;
        var baselineRuns = before["Runs"]!.AsArray();
        var afterRuns = after["Runs"]!.AsArray();
        foreach (var run in baselineRuns)
        {
            var paired = afterRuns.Single(other => Key(other!) == Key(run!))!;
            if (Text(run!, "AuthoritativeFingerprint") != Text(paired, "AuthoritativeFingerprint"))
                throw new InvalidOperationException($"Authoritative outcome mismatch: {Key(run!)}");
        }
        Console.WriteLine("\n## Generated measurements\n");
        Console.WriteLine("All counts below are **Measured**. Zero repeats retain their full denominators. " +
            "Source pool IDs identify the existing catalog text, even when an event-fit route uses a subset.\n");
        Console.WriteLine("### Trait totals\n");
        Console.WriteLine("| Trait | Hours | Quotes (before = after) | Before R20 | After R20 | Before R50 | After R50 |");
        Console.WriteLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var baseline in before["TraitSummary"]!.AsArray())
        {
            var paired = after["TraitSummary"]!.AsArray().Single(row => SummaryKey(row!) == SummaryKey(baseline!))!;
            var quotes = Number(baseline!, "Quotes");
            Console.WriteLine($"| {Text(baseline!, "Trait")} | {Number(baseline!, "Hours")} | {quotes} | " +
                $"{Rate(Number(baseline!, "Repeat20"), quotes)} | {Rate(Number(paired, "Repeat20"), quotes)} | " +
                $"{Rate(Number(baseline!, "Repeat50"), quotes)} | {Rate(Number(paired, "Repeat50"), quotes)} |");
        }
        Console.WriteLine("\n### Individual seeds and unchanged outcome fingerprints\n");
        Console.WriteLine("Fingerprints retain committed factual receipts, all event timestamps/fields, thread factual logs, " +
            "KO/loot/coins/shields/stash/stats/stock/Signal and projected timing. Only narration fields and " +
            "appended Raider quotes are excluded. Random GUID-derived starter-kettle IDs are bijectively " +
            "canonicalized in encounter order; identity references and ownership relationships remain. " +
            "AAR/appraisal and highlights are presentation, not outcome evidence.\n");
        Console.WriteLine("| Trait | Seed | Hours | Quotes | Before R20 / R50 | After R20 / R50 | Matched SHA-256 |");
        Console.WriteLine("|---|---:|---:|---:|---|---|---|");
        foreach (var baseline in baselineRuns)
        {
            var paired = afterRuns.Single(row => Key(row!) == Key(baseline!))!;
            var quotes = Number(baseline!, "Quotes");
            Console.WriteLine($"| {Text(baseline!, "Trait")} | {Number(baseline!, "Seed")} | {Number(baseline!, "Hours")} | {quotes} | " +
                $"{Rate(Sum(baseline!, "ByKind", "Repeat20"), quotes)} / {Rate(Sum(baseline!, "ByKind", "Repeat50"), quotes)} | " +
                $"{Rate(Sum(paired, "ByKind", "Repeat20"), quotes)} / {Rate(Sum(paired, "ByKind", "Repeat50"), quotes)} | " +
                $"`{Text(baseline!, "AuthoritativeFingerprint")}` |");
        }
        Console.WriteLine("\n### Baseline event routing, pool size and fallthrough share (24h)\n");
        Console.WriteLine("F denotes trait/general fallthrough; E denotes event/activity fit. " +
            "Unobserved kinds retain the source-audited fallback (7/6/6/12 by trait); no measured share is inferred.\n");
        Console.WriteLine("| Kind | Trait | Source pool (size) | Route | Quotes / all quotes (share) |");
        Console.WriteLine("|---|---|---|---|---|");
        foreach (var kind in before["Routing"]!.AsArray())
        {
            var observed = kind!["Observed"]!.AsArray();
            if (observed.Count == 0)
                Console.WriteLine($"| {Text(kind, "Kind")} | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |");
            foreach (var row in observed)
            {
                var trait = Text(row!, "Trait");
                var total = before["TraitSummary"]!.AsArray().Single(summary =>
                    Text(summary!, "Trait") == trait && Number(summary!, "Hours") == 24)!;
                var pool = Text(row!, "Pool");
                var fallthrough = pool is "voice.general" or "voice.hoarder" or "voice.brawler" or "voice.gadgeteer";
                Console.WriteLine($"| {Text(kind, "Kind")} | {trait} | {pool} ({Number(row!, "Size")}) | " +
                    $"{(fallthrough ? "F" : "E")} | {Rate(Number(row!, "Quotes"), Number(total, "Quotes"))} |");
            }
        }
        foreach (var dimension in new[] { "ByKind", "ByPool" })
        {
            Console.WriteLine($"\n### Before/after by {(dimension == "ByKind" ? "event kind" : "source pool")}\n");
            Console.WriteLine("| Trait | Hours | ID | Before count/share | After count/share | Before R20 / R50 | After R20 / R50 |");
            Console.WriteLine("|---|---:|---|---|---|---|---|");
            foreach (var baseline in before["TraitSummary"]!.AsArray())
            {
                var paired = after["TraitSummary"]!.AsArray().Single(row => SummaryKey(row!) == SummaryKey(baseline!))!;
                foreach (var row in baseline![dimension]!.AsArray())
                {
                    var other = paired[dimension]!.AsArray().Single(candidate => Text(candidate!, "Id") == Text(row!, "Id"))!;
                    Console.WriteLine($"| {Text(baseline, "Trait")} | {Number(baseline, "Hours")} | {Text(row!, "Id")} | " +
                        $"{Rate(Number(row!, "Quotes"), Number(row!, "AllQuotes"))} | {Rate(Number(other, "Quotes"), Number(other, "AllQuotes"))} | " +
                        $"{Rate(Number(row!, "Repeat20"), Number(row!, "Quotes"))} / {Rate(Number(row!, "Repeat50"), Number(row!, "Quotes"))} | " +
                        $"{Rate(Number(other, "Repeat20"), Number(other, "Quotes"))} / {Rate(Number(other, "Repeat50"), Number(other, "Quotes"))} |");
                }
            }
        }
        Console.WriteLine("\n### Top ten exact quotes per pool (before and after)\n");
        Console.WriteLine("The union of each stage's top ten is shown; '-' means outside that stage's top ten, not zero occurrences.\n");
        foreach (var baseline in before["TraitSummary"]!.AsArray())
        {
            var paired = after["TraitSummary"]!.AsArray().Single(row => SummaryKey(row!) == SummaryKey(baseline!))!;
            Console.WriteLine($"<details><summary>{Text(baseline!, "Trait")} / {Number(baseline!, "Hours")}h</summary>\n");
            Console.WriteLine("| Pool | Exact quote | Before count | After count |");
            Console.WriteLine("|---|---|---:|---:|");
            foreach (var pool in baseline!["ByPool"]!.AsArray())
            {
                var other = paired["ByPool"]!.AsArray().Single(row => Text(row!, "Id") == Text(pool!, "Id"))!;
                var left = pool!["Top10"]!.AsArray().ToDictionary(row => Text(row!, "Text"), row => Number(row!, "Count"));
                var right = other["Top10"]!.AsArray().ToDictionary(row => Text(row!, "Text"), row => Number(row!, "Count"));
                foreach (var quote in left.Keys.Union(right.Keys))
                    Console.WriteLine($"| {Text(pool, "Id")} | {quote.Replace("|", "\\|")} | " +
                        $"{(left.TryGetValue(quote, out var l) ? l.ToString(CultureInfo.InvariantCulture) : "-")} | " +
                        $"{(right.TryGetValue(quote, out var r) ? r.ToString(CultureInfo.InvariantCulture) : "-")} |");
            }
            Console.WriteLine("\n</details>\n");
        }
        Console.WriteLine("### Hub entries beside unique lines per stay\n");
        Console.WriteLine("Each cell is entries / unique exact lines; final observed stays may be partial. " +
            "No resident reference or cadence change. Same ten-line pool; LRU changes order, not its size.\n");
        Console.WriteLine("| Trait | Hours | Seed | Stay | Partial | Before entries / unique | After entries / unique |");
        Console.WriteLine("|---|---:|---:|---:|---|---|---|");
        foreach (var baseline in baselineRuns)
        {
            var paired = afterRuns.Single(row => Key(row!) == Key(baseline!))!;
            foreach (var stay in baseline!["HubStays"]!.AsArray())
            {
                var other = paired["HubStays"]!.AsArray().Single(row => Number(row!, "Stay") == Number(stay!, "Stay"))!;
                Console.WriteLine($"| {Text(baseline, "Trait")} | {Number(baseline, "Hours")} | {Number(baseline, "Seed")} | " +
                    $"{Number(stay!, "Stay")} | {stay!["Partial"]!.GetValue<bool>()} | " +
                    $"{Number(stay, "Entries")} / {Number(stay, "UniqueLines")} | {Number(other, "Entries")} / {Number(other, "UniqueLines")} |");
            }
        }
    }

    private static string Text(JsonNode node, string key) => node[key]!.GetValue<string>();
    private static int Number(JsonNode node, string key) => node[key]!.GetValue<int>();
    private static string SummaryKey(JsonNode node) => $"{Text(node, "Trait")}/{Number(node, "Hours")}";
    private static string Key(JsonNode node) => $"{SummaryKey(node)}/{Number(node, "Seed")}";
    private static int Sum(JsonNode node, string array, string field) =>
        node[array]!.AsArray().Sum(row => Number(row!, field));
    private static string Rate(int numerator, int denominator) =>
        $"{numerator}/{denominator} ({(100.0 * numerator / denominator).ToString("F2", CultureInfo.InvariantCulture)}%)";
}
