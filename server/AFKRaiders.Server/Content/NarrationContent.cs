using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AFKRaiders.Server.Content;

internal sealed record NarrationPool(string Id, IReadOnlyList<string> Lines);
internal sealed record NarrationTemplate(string Id, string Text);
internal sealed record NarrationDocument(
    int SchemaVersion,
    string ContentVersion,
    IReadOnlyList<NarrationPool> Pools,
    IReadOnlyList<NarrationTemplate> Templates,
    IReadOnlyList<string> EnrichmentEventKinds);

internal sealed class NarrationContent
{
    private const string Source = "narration-catalog.json";
    private readonly FrozenDictionary<string, IReadOnlyList<string>> pools;
    private readonly FrozenDictionary<string, CompositeFormat> templates;
    private readonly FrozenSet<string> enrichmentEventKinds;
    private static readonly FrozenSet<string> SupportedEnrichmentKinds = new[]
    {
        "AreaActivityStarted", "Search", "RobotRound", "Travel", "ExtractionStarted",
        "AreaDiary", "Downed", "KnockedOut"
    }.ToFrozenSet(StringComparer.Ordinal);
    private static readonly FrozenSet<string> RobotProgressTokens = new[]
    {
        "weapon_name", "raider_damage", "robot_name", "robot_hp", "robot_max_hp", "robot_damage_summary"
    }.ToFrozenSet(StringComparer.Ordinal);

    public static NarrationContent Default { get; } = LoadBundled();
    public string ContentVersion { get; }

    private NarrationContent(NarrationDocument document)
    {
        ContentVersion = document.ContentVersion;
        pools = document.Pools.ToFrozenDictionary(pool => pool.Id,
            pool => (IReadOnlyList<string>)Array.AsReadOnly(pool.Lines.ToArray()), StringComparer.Ordinal);
        templates = document.Templates.ToFrozenDictionary(template => template.Id,
            template => CompositeFormat.Parse(template.Text), StringComparer.Ordinal);
        enrichmentEventKinds = document.EnrichmentEventKinds.ToFrozenSet(StringComparer.Ordinal);
    }

    public IReadOnlyList<string> Pool(string id) => pools[id];
    public bool CanEnrich(string kind) => enrichmentEventKinds.Contains(kind);

    public string Format(string id, params object?[] arguments)
    {
        if (arguments.Length != NarrationContracts.TemplateArguments[id])
            throw new InvalidOperationException($"Narration template '{id}' received the wrong argument count.");
        return string.Format(CultureInfo.CurrentCulture, templates[id], arguments);
    }

    private static NarrationContent LoadBundled()
    {
        using var stream = CatalogContent.OpenBundled(Source);
        return Read(stream);
    }

    internal static NarrationContent Read(Stream stream)
    {
        var document = CatalogContent.Read<NarrationDocument>(stream, Source);
        Require(document.SchemaVersion == 1, "Unsupported schemaVersion; expected 1.");
        Require(!string.IsNullOrWhiteSpace(document.ContentVersion), "contentVersion cannot be empty.");
        Require(document.Pools is { Count: > 0 }, "pools must be nonempty.");
        Require(document.Templates is { Count: > 0 }, "templates must be nonempty.");
        Require(document.EnrichmentEventKinds is not null, "enrichmentEventKinds cannot be null.");
        var poolIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pool in document.Pools)
        {
            Require(pool is not null, "pools cannot contain null.");
            Require(!string.IsNullOrWhiteSpace(pool.Id) && NarrationContracts.PoolIds.Contains(pool.Id),
                $"Unknown pool '{pool.Id}'.");
            Require(poolIds.Add(pool.Id), $"Duplicate pool '{pool.Id}'.");
            Require(pool.Lines is { Count: >= 2 }, $"Pool '{pool.Id}' needs at least two distinct lines for voice rotation.");
            var lines = new HashSet<string>(StringComparer.Ordinal);
            foreach (var line in pool.Lines)
            {
                Require(!string.IsNullOrWhiteSpace(line), $"Pool '{pool.Id}' contains a blank line.");
                Require(!line.Any(char.IsControl), $"Pool '{pool.Id}' lines must be single-line text.");
                Require(lines.Add(line), $"Pool '{pool.Id}' contains a duplicate line.");
                if (pool.Id.StartsWith("style.", StringComparison.Ordinal))
                    Require(line.Length <= 40 && Regex.IsMatch(line, @"\A[a-z]+(?: [a-z]+)*\z"),
                        $"Pool '{pool.Id}' must contain short lowercase object cues, not prompt instructions.");
                var remainder = line;
                if (pool.Id == "survival.robot-progress")
                    remainder = Regex.Replace(line, @"\{([a-z_]+)\}", match =>
                    {
                        Require(RobotProgressTokens.Contains(match.Groups[1].Value),
                            $"Pool '{pool.Id}' contains an unknown robot progress token.");
                        return "";
                    });
                Require(!remainder.Contains('{') && !remainder.Contains('}'),
                    $"Pool '{pool.Id}' contains an unsupported placeholder.");
            }
        }
        Require(poolIds.SetEquals(NarrationContracts.PoolIds), "Required narration pools are missing.");

        var templateIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var template in document.Templates)
        {
            Require(template is not null, "templates cannot contain null.");
            Require(!string.IsNullOrWhiteSpace(template.Id)
                && NarrationContracts.TemplateArguments.ContainsKey(template.Id), $"Unknown template '{template.Id}'.");
            Require(templateIds.Add(template.Id), $"Duplicate template '{template.Id}'.");
            Require(!string.IsNullOrWhiteSpace(template.Text), $"Template '{template.Id}' cannot be blank.");
            Require(!template.Text.Any(char.IsControl), $"Template '{template.Id}' must be single-line text.");
            var indices = new HashSet<int>();
            var remainder = Regex.Replace(template.Text, @"\{([0-9]+)(?::(?:HH:mm:ss|P0|F2))?\}", match =>
            {
                Require(int.TryParse(match.Groups[1].Value, out var index),
                    $"Template '{template.Id}' has an invalid placeholder index.");
                indices.Add(index);
                return "";
            });
            Require(!remainder.Contains('{') && !remainder.Contains('}')
                && indices.SetEquals(Enumerable.Range(0, NarrationContracts.TemplateArguments[template.Id])),
                $"Template '{template.Id}' must retain exactly its declared numbered placeholders.");
            try
            {
                _ = CompositeFormat.Parse(template.Text);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"{Source}: Invalid template '{template.Id}'.", exception);
            }
        }
        Require(templateIds.SetEquals(NarrationContracts.TemplateArguments.Keys), "Required narration templates are missing.");
        var enrichmentKinds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var kind in document.EnrichmentEventKinds)
        {
            Require(kind is not null && SupportedEnrichmentKinds.Contains(kind), $"Unsupported enrichment event kind '{kind}'.");
            Require(enrichmentKinds.Add(kind), $"Duplicate enrichment event kind '{kind}'.");
        }
        return new NarrationContent(document);
    }

    private static void Require(
        [System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{Source}: {message}");
    }
}
