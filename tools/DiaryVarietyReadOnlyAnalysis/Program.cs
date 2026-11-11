using System.Text.Json;
using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;

// Advances isolated in-memory simulations only; this utility writes no game data.
const int HorizonHours = 24;
int[] seeds = [101, 202, 303];
var start = DateTimeOffset.Parse("2026-11-08T00:00:00Z");
var results = new List<RunResult>();

foreach (var trait in Enum.GetValues<RaiderPrimaryTrait>())
foreach (var seed in seeds)
{
    var clock = new FixedTimeProvider(start);
    var nextSeed = 0;
    var nextId = 0;
    var coordinator = new RaidCoordinator(
        clock,
        narrator: new TemplateRaidNarrator(),
        narrativeGenerator: new TemplateNarrativeGenerator(),
        nextSeed: () => nextSeed++ == 0 ? (int)trait : seed * 1000 + nextSeed,
        traderSeed: seed,
        nextId: () => $"analysis-{seed}-{nextId++:D4}");

    var enrolled = coordinator.EnrollHandler("raider-one", $"diary-variety-{trait}-{seed}");
    if (enrolled.Projection.PrimaryTrait != trait.ToString())
        throw new InvalidOperationException($"Expected {trait}, got {enrolled.Projection.PrimaryTrait}.");

    for (var seconds = 0; seconds <= HorizonHours * 3600; seconds += 5)
    {
        clock.Now = start.AddSeconds(seconds);
        coordinator.AdvanceRaids();
    }

    var journal = coordinator.GetHandlerProjection(enrolled.AccessToken).FullJournal
        ?? throw new InvalidOperationException("The read-only projection did not include its journal.");
    results.Add(Measure(trait, seed, journal));
}

var summary = results.GroupBy(result => result.Trait).Select(group =>
{
    var entries = group.Sum(result => result.JournalEntries);
    var quotes = group.Sum(result => result.RaiderQuotes);
    var topQuoteKinds = group.SelectMany(result => result.QuoteKindCounts)
        .GroupBy(pair => pair.Key, StringComparer.Ordinal)
        .Select(kind => new
        {
            Kind = kind.Key,
            MeanCountPer24Hours = kind.Sum(pair => pair.Value) / (double)group.Count()
        })
        .OrderByDescending(kind => kind.MeanCountPer24Hours)
        .Take(6)
        .ToArray();
    return new TraitSummary(
        group.Key,
        group.Count(),
        group.Average(result => result.RaiderQuotes),
        group.Average(result => result.UniqueQuoteCount),
        group.Sum(result => result.TopSixQuoteShare * result.RaiderQuotes) / quotes,
        group.Sum(result => result.RepeatedWithin20QuoteShare * result.RaiderQuotes) / quotes,
        group.Sum(result => result.RepeatedWithin50QuoteShare * result.RaiderQuotes) / quotes,
        group.Average(result => result.MeanUniqueHubActivityLinesPerStay),
        group.Sum(result => result.MechanicalPersonalityEntryShare * result.JournalEntries) / entries,
        group.Sum(result => result.StayingDeeperEntryShare * result.JournalEntries) / entries,
        group.Average(result => result.DistinctQuotesPer100JournalEntries),
        group.Average(result => result.RaidAdmissions),
        group.Average(result => result.JournalEntries),
        topQuoteKinds);
}).ToArray();

Console.WriteLine(JsonSerializer.Serialize(new { HorizonHours, Seeds = seeds, Runs = results, TraitSummary = summary },
    new JsonSerializerOptions { WriteIndented = true }));

static RunResult Measure(RaiderPrimaryTrait trait, int seed, IReadOnlyList<HandlerJournalEntryView> journal)
{
    var quotedEntries = journal
        .Select(entry => (Entry: entry, Quote: ExtractAside(entry.Text)))
        .Where(item => item.Quote is not null)
        .ToArray();
    var quotes = quotedEntries.Select(item => item.Quote!).ToArray();
    var quoteKindCounts = quotedEntries
        .GroupBy(item => item.Entry.Kind, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

    var hubGroups = new Dictionary<int, List<string>>();
    var stayIndex = 0;
    foreach (var entry in journal.OrderBy(entry => entry.Sequence))
    {
        if (entry.Kind == "RaidAdmission")
            stayIndex++;
        if (entry.Kind != "HubActivity")
            continue;
        if (!hubGroups.TryGetValue(stayIndex, out var group))
            hubGroups.Add(stayIndex, group = []);
        group.Add(entry.Text);
    }

    return new RunResult(
        trait.ToString(),
        seed,
        journal.Count,
        quotes.Length,
        quotes.Distinct(StringComparer.Ordinal).Count(),
        TopSixShare(quotes),
        RepeatShare(quotes, 20),
        RepeatShare(quotes, 50),
        hubGroups.Count == 0 ? 0 : hubGroups.Values.Average(group => group.Distinct(StringComparer.Ordinal).Count()),
        journal.Count == 0 ? 0 : journal.Count(entry => entry.Kind == "PersonalityChanged") / (double)journal.Count,
        journal.Count == 0 ? 0 : journal.Count(entry =>
            entry.Kind == "PersonalityChanged" && entry.Reason == "Staying deeper") / (double)journal.Count,
        journal.Count == 0 ? 0 : quotes.Distinct(StringComparer.Ordinal).Count() * 100.0 / journal.Count,
        journal.Count(entry => entry.Kind == "RaidAdmission"),
        journal.Count(entry => entry.Kind == "HubActivity"),
        quoteKindCounts);
}

static string? ExtractAside(string text)
{
    const string marker = "\nRaider: ";
    var index = text.LastIndexOf(marker, StringComparison.Ordinal);
    return index < 0 ? null : text[(index + marker.Length)..].Trim();
}

static double TopSixShare(IReadOnlyList<string> quotes)
{
    if (quotes.Count == 0) return 0;
    var topSix = quotes.GroupBy(quote => quote, StringComparer.Ordinal)
        .Select(group => group.Count())
        .OrderByDescending(count => count)
        .Take(6)
        .Sum();
    return topSix / (double)quotes.Count;
}

static double RepeatShare(IReadOnlyList<string> quotes, int lookback)
{
    var repeats = 0;
    for (var index = 0; index < quotes.Count; index++)
    {
        var firstPrior = Math.Max(0, index - lookback);
        if (quotes.Skip(firstPrior).Take(index - firstPrior).Contains(quotes[index], StringComparer.Ordinal))
            repeats++;
    }
    return quotes.Count == 0 ? 0 : repeats / (double)quotes.Count;
}

sealed class FixedTimeProvider(DateTimeOffset initial) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = initial;
    public override DateTimeOffset GetUtcNow() => Now;
}

sealed record RunResult(
    string Trait,
    int Seed,
    int JournalEntries,
    int RaiderQuotes,
    int UniqueQuoteCount,
    double TopSixQuoteShare,
    double RepeatedWithin20QuoteShare,
    double RepeatedWithin50QuoteShare,
    double MeanUniqueHubActivityLinesPerStay,
    double MechanicalPersonalityEntryShare,
    double StayingDeeperEntryShare,
    double DistinctQuotesPer100JournalEntries,
    int RaidAdmissions,
    int HubActivityEntries,
    IReadOnlyDictionary<string, int> QuoteKindCounts);

sealed record TraitSummary(
    string Trait,
    int SeedCount,
    double MeanQuotesPer24Hours,
    double MeanUniqueQuotes,
    double TopSixQuoteShare,
    double RepeatedWithin20QuoteShare,
    double RepeatedWithin50QuoteShare,
    double MeanUniqueHubActivityLinesPerStay,
    double MechanicalPersonalityEntryShare,
    double StayingDeeperEntryShare,
    double MeanDistinctQuotesPer100JournalEntries,
    double MeanRaidAdmissionsPer24Hours,
    double MeanJournalEntries,
    object TopQuoteKinds);
