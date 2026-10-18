namespace AFKRaiders.Server.Domain;

/// <summary>
/// A validated item entry in the server-owned loot catalog.
/// Provenance: Reused from afk-raiders/src/content/loot-tables/scrap_components.json
/// and related loot tables at git commit f62d088a41da951a1fbe9ed7215d2957a933ce50.
/// </summary>
public sealed record LootCatalogItem(
    string Id,
    string Name,
    int Value,
    int Rarity,
    string Category,
    string Flavor,
    IReadOnlyList<string> EligibilityCategories
);

public static class LootEligibilityCategory
{
    public const string Residential = "Residential";
    public const string Salvage = "Salvage";
    public const string Robot = "Robot";
    public const string Technology = "Technology";

    public static bool IsKnown(string category) =>
        category is Residential or Salvage or Robot or Technology;
}

/// <summary>
/// Weighted pool entry for table-backed generation.
/// </summary>
public sealed record WeightedLootEntry(
    string CatalogId,
    int DropWeight
);

/// <summary>
/// Definition of a world salvage opportunity and its loot backing.
/// </summary>
public sealed record OpportunityDefinition(
    string Id,
    string Location,
    int DurationSeconds,
    IReadOnlyList<string> FixedItemIds,
    int DrawCount,
    IReadOnlyList<string> EligibleCategories,
    string Activity,
    string Title,
    string StartText,
    string CompletedText,
    bool Contestable
);

/// <summary>
/// An instantiated world item ready for simulation.
/// </summary>
public sealed record WorldItemInstance(
    string OpportunityId,
    ItemView Item
);

/// <summary>
/// Exception thrown when the loot catalog or opportunity table fails schema or referential integrity validation.
/// </summary>
public sealed class LootCatalogValidationException(string message) : Exception(message);

/// <summary>
/// Contract for server-owned loot catalog, content provenance, and table-backed generation.
/// </summary>
public interface ILootCatalog
{
    string ContentVersion { get; }
    IReadOnlyList<LootCatalogItem> Items { get; }
    IReadOnlyList<OpportunityDefinition> Opportunities { get; }
    IReadOnlyList<WeightedLootEntry> Pool { get; }
    IReadOnlyList<OpportunityDefinition> SelectWorldOpportunities(int seed);
    LootCatalogItem GetItem(string id);
    bool TryGetItem(string id, out LootCatalogItem? item);
    IReadOnlyList<WorldItemInstance> CreateFixedWorldItems(string raidId);
    IReadOnlyList<WorldItemInstance> CreateTableBackedWorldItems(string raidId, int seed);
    IReadOnlyList<WorldItemInstance> CreateTableBackedWorldItems(string raidId, int seed,
        IReadOnlyList<string> opportunityIds);
    IReadOnlyList<WorldItemInstance> CreateOpportunityItems(string raidId, int seed, string opportunityId,
        string danger, double exposure, double appetite);
}

/// <summary>
/// Default server-owned catalog containing reviewed scrap items and validated opportunity tables.
/// </summary>
public sealed class LootCatalog : ILootCatalog
{
    public const string DefaultContentVersion = "desperanza-shared-loot-v5";

    private readonly Dictionary<string, LootCatalogItem> itemsById;
    private readonly Dictionary<string, OpportunityDefinition> opportunitiesById;

    public string ContentVersion { get; }
    public IReadOnlyList<LootCatalogItem> Items { get; }
    public IReadOnlyList<OpportunityDefinition> Opportunities { get; }
    public IReadOnlyList<WeightedLootEntry> Pool { get; }

    public static LootCatalog Default { get; } = CreateDefault();

    public LootCatalog(
        string contentVersion,
        IEnumerable<LootCatalogItem> items,
        IEnumerable<OpportunityDefinition> opportunities,
        IEnumerable<WeightedLootEntry> pool)
    {
        ContentVersion = string.IsNullOrWhiteSpace(contentVersion)
            ? throw new LootCatalogValidationException("ContentVersion cannot be empty.")
            : contentVersion;

        Items = items?.ToList() ?? throw new LootCatalogValidationException("Items collection cannot be null.");
        Opportunities = opportunities?.ToList() ?? throw new LootCatalogValidationException("Opportunities collection cannot be null.");
        Pool = pool?.ToList() ?? throw new LootCatalogValidationException("Shared weighted pool cannot be null.");

        itemsById = new Dictionary<string, LootCatalogItem>(StringComparer.Ordinal);
        opportunitiesById = new Dictionary<string, OpportunityDefinition>(StringComparer.Ordinal);

        ValidateAndIndex();
    }

    private void ValidateAndIndex()
    {
        if (Items.Count == 0)
        {
            throw new LootCatalogValidationException("Catalog must contain at least one item.");
        }

        foreach (var item in Items)
        {
            if (item is null)
                throw new LootCatalogValidationException("Items cannot contain null.");
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                throw new LootCatalogValidationException("Item CatalogId cannot be null or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                throw new LootCatalogValidationException($"Item '{item.Id}' name cannot be null or whitespace.");
            }
            if (item.Value < 0)
            {
                throw new LootCatalogValidationException($"Item '{item.Id}' cannot have negative value ({item.Value}).");
            }
            if (item.Rarity < 1)
            {
                throw new LootCatalogValidationException($"Item '{item.Id}' cannot have non-positive rarity ({item.Rarity}).");
            }
            if (string.IsNullOrWhiteSpace(item.Flavor))
            {
                throw new LootCatalogValidationException($"Item '{item.Id}' flavor cannot be null or whitespace.");
            }
            ValidateCategories(item.EligibilityCategories, $"Item '{item.Id}'");
            if (!itemsById.TryAdd(item.Id, item))
            {
                throw new LootCatalogValidationException($"Duplicate item CatalogId: '{item.Id}'.");
            }
        }

        if (Pool.Count == 0)
            throw new LootCatalogValidationException("Shared weighted pool cannot be empty.");
        var poolIds = new HashSet<string>(StringComparer.Ordinal);
        long totalWeight = 0;
        foreach (var entry in Pool)
        {
            if (entry is null)
                throw new LootCatalogValidationException("Shared pool contains a null entry.");
            if (string.IsNullOrWhiteSpace(entry.CatalogId))
                throw new LootCatalogValidationException("Shared pool contains an empty item reference.");
            if (!itemsById.ContainsKey(entry.CatalogId))
                throw new LootCatalogValidationException($"Shared pool references unknown item CatalogId: '{entry.CatalogId}'.");
            if (!poolIds.Add(entry.CatalogId))
                throw new LootCatalogValidationException($"Shared pool contains duplicate pool entry '{entry.CatalogId}'.");
            if (entry.DropWeight <= 0)
                throw new LootCatalogValidationException($"Shared pool entry '{entry.CatalogId}' has non-positive dropWeight ({entry.DropWeight}).");
            totalWeight += entry.DropWeight;
        }
        if (totalWeight > int.MaxValue)
            throw new LootCatalogValidationException("Shared pool total dropWeight must fit Int32.");

        if (Opportunities.Count == 0)
        {
            throw new LootCatalogValidationException("Catalog must contain at least one opportunity definition.");
        }

        foreach (var opp in Opportunities)
        {
            if (opp is null)
                throw new LootCatalogValidationException("Opportunities cannot contain null.");
            if (string.IsNullOrWhiteSpace(opp.Id))
            {
                throw new LootCatalogValidationException("Opportunity Id cannot be null or whitespace.");
            }
            if (opp.FixedItemIds is null)
            {
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' fixed item list cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(opp.Location))
            {
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' location cannot be null or whitespace.");
            }
            if (opp.DurationSeconds <= 0)
            {
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' duration must be positive ({opp.DurationSeconds}).");
            }
            if (!OpportunityActivity.IsKnown(opp.Activity))
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' has unknown activity '{opp.Activity}'.");
            if (string.IsNullOrWhiteSpace(opp.Title) || string.IsNullOrWhiteSpace(opp.StartText) ||
                string.IsNullOrWhiteSpace(opp.CompletedText))
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' requires a title and start/completion text.");
            if (opp.DrawCount <= 0)
            {
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' draw count must be positive ({opp.DrawCount}).");
            }
            ValidateCategories(opp.EligibleCategories, $"Opportunity '{opp.Id}'");
            if (!opportunitiesById.TryAdd(opp.Id, opp))
            {
                throw new LootCatalogValidationException($"Duplicate opportunity Id: '{opp.Id}'.");
            }

            // Referential integrity check for fixed items
            var fixedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var itemId in opp.FixedItemIds)
            {
                if (string.IsNullOrWhiteSpace(itemId))
                    throw new LootCatalogValidationException($"Opportunity '{opp.Id}' has an empty fixed item reference.");
                if (!fixedIds.Add(itemId))
                    throw new LootCatalogValidationException($"Opportunity '{opp.Id}' contains duplicate fixed item reference '{itemId}'.");
                if (!itemsById.ContainsKey(itemId))
                {
                    throw new LootCatalogValidationException(
                        $"Opportunity '{opp.Id}' references unknown fixed item CatalogId: '{itemId}'.");
                }
            }

            if (!EligiblePool(opp).Any())
                throw new LootCatalogValidationException($"Opportunity '{opp.Id}' has no pool items eligible for its categories.");
        }
    }

    private static void ValidateCategories(IReadOnlyList<string> categories, string owner)
    {
        if (categories is null || categories.Count == 0)
            throw new LootCatalogValidationException($"{owner} must define at least one eligibility category.");
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var category in categories)
        {
            if (!LootEligibilityCategory.IsKnown(category))
                throw new LootCatalogValidationException($"{owner} has unknown eligibility category '{category}'.");
            if (!seen.Add(category))
                throw new LootCatalogValidationException($"{owner} contains duplicate eligibility category '{category}'.");
        }
    }

    private IEnumerable<WeightedLootEntry> EligiblePool(OpportunityDefinition opportunity) =>
        Pool.Where(entry =>
            GetItem(entry.CatalogId).EligibilityCategories.Any(opportunity.EligibleCategories.Contains));

    public LootCatalogItem GetItem(string id)
    {
        if (itemsById.TryGetValue(id, out var item))
        {
            return item;
        }
        throw new KeyNotFoundException($"Catalog item '{id}' was not found in catalog {ContentVersion}.");
    }

    public bool TryGetItem(string id, out LootCatalogItem? item) => itemsById.TryGetValue(id, out item);

    public IReadOnlyList<OpportunityDefinition> SelectWorldOpportunities(int seed) =>
        Opportunities
            .GroupBy(opportunity => opportunity.Location, StringComparer.Ordinal)
            .Select(group =>
            {
                var candidates = group.ToArray();
                var index = (int)(RaidConditions.Roll(seed, $"opportunity:{group.Key}") * candidates.Length);
                return candidates[index];
            })
            .ToArray();

    /// <summary>
    /// Generates the fixed 6-instance world items required for exact backward compatibility with S5 regression baselines.
    /// </summary>
    public IReadOnlyList<WorldItemInstance> CreateFixedWorldItems(string raidId)
    {
        var scrap = GetItem("scrap_metal_basic");
        var toe = GetItem("robot_toe");
        var circuits = GetItem("circuit_bundle_messy");
        var finger = GetItem("robot_finger");

        return
        [
            new("market-locker", new($"{raidId}:scrap", scrap.Id, scrap.Name, scrap.Value, scrap.Flavor)),
            new("market-locker", new($"{raidId}:toe", toe.Id, toe.Name, toe.Value, toe.Flavor)),
            new("humming-crate", new($"{raidId}:circuits", circuits.Id, circuits.Name, circuits.Value, circuits.Flavor)),
            new("humming-crate", new($"{raidId}:finger", finger.Id, finger.Name, finger.Value, finger.Flavor)),
            new("depot-locker", new($"{raidId}:depot-scrap", scrap.Id, scrap.Name, scrap.Value, scrap.Flavor)),
            new("depot-locker", new($"{raidId}:depot-toe", toe.Id, toe.Name, toe.Value, toe.Flavor))
        ];
    }

    /// <summary>
    /// Generates unique, finite world items deterministically based on a seed and opportunity weighted pools.
    /// Identical seeds produce identical item instances; distinct instances always receive unique instance IDs.
    /// </summary>
    public IReadOnlyList<WorldItemInstance> CreateTableBackedWorldItems(string raidId, int seed) =>
        CreateTableBackedWorldItems(raidId, seed, SelectWorldOpportunities(seed).Select(opportunity => opportunity.Id).ToArray());

    public IReadOnlyList<WorldItemInstance> CreateTableBackedWorldItems(string raidId, int seed,
        IReadOnlyList<string> opportunityIds)
    {
        var result = new List<WorldItemInstance>();
        var selectedIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var opportunityId in opportunityIds)
        {
            if (!selectedIds.Add(opportunityId))
                throw new ArgumentException($"Opportunity '{opportunityId}' was selected more than once.", nameof(opportunityIds));
            if (!opportunitiesById.TryGetValue(opportunityId, out var opp))
                throw new KeyNotFoundException($"Opportunity '{opportunityId}' was not found in catalog {ContentVersion}.");
            var pool = EligiblePool(opp).ToArray();
            var totalWeight = pool.Sum(e => e.DropWeight);

            for (var drawIndex = 0; drawIndex < opp.DrawCount; drawIndex++)
            {
                var roll = (int)(RaidConditions.Roll(seed, $"loot:{opp.Id}:{drawIndex}") * totalWeight);
                var cumulative = 0;
                WeightedLootEntry selected = pool[0];
                foreach (var entry in pool)
                {
                    cumulative += entry.DropWeight;
                    if (roll < cumulative)
                    {
                        selected = entry;
                        break;
                    }
                }

                var catalogItem = GetItem(selected.CatalogId);
                var instanceId = $"{raidId}:{opp.Id}:{drawIndex}:{catalogItem.Id}";
                result.Add(new WorldItemInstance(opp.Id, new ItemView(instanceId, catalogItem.Id, catalogItem.Name, catalogItem.Value, catalogItem.Flavor)));
            }
        }

        return result;
    }

    public IReadOnlyList<WorldItemInstance> CreateOpportunityItems(string raidId, int seed, string opportunityId,
        string danger, double exposure, double appetite)
    {
        var opp = opportunitiesById[opportunityId];
        var pool = EligiblePool(opp).ToArray();
        var min = pool.Min(e => GetItem(e.CatalogId).Value);
        var max = pool.Max(e => GetItem(e.CatalogId).Value);
        var dangerBias = danger switch
        {
            "Low" => 0.0, "Medium" => 1.0, "High" => 2.0,
            _ => throw new ArgumentException("Unknown danger.", nameof(danger))
        };
        var weights = pool.Select(e => e.DropWeight * (1 + (max == min ? 0 :
            (GetItem(e.CatalogId).Value - min) / (double)(max - min)) *
            Math.Max(0, dangerBias + 4 * Math.Clamp(exposure, 0, 1) + appetite))).ToArray();
        var result = new List<WorldItemInstance>();
        for (var draw = 0; draw < opp.DrawCount; draw++)
        {
            var roll = RaidConditions.Roll(seed, $"loot:{opp.Id}:{draw}") * weights.Sum();
            var index = 0;
            while (index < weights.Length - 1 && roll >= weights[index])
                roll -= weights[index++];
            var item = GetItem(pool[index].CatalogId);
            result.Add(new(opp.Id, new($"{raidId}:{opp.Id}:{draw}", item.Id, item.Name, item.Value, item.Flavor)));
        }
        return result;
    }

    private static LootCatalog CreateDefault()
    {
        return Content.CatalogContent.LoadLoot();
    }
}

public static class OpportunityActivity
{
    public const string Searching = "Searching";
    public const string Scavenging = "Scavenging";
    public const string Salvaging = "Salvaging";
    public const string Recovering = "Recovering";
    public const string Rummaging = "Rummaging";
    public const string Dismantling = "Dismantling";

    public static bool IsKnown(string activity) =>
        activity is Searching or Scavenging or Salvaging or Recovering or Rummaging or Dismantling;
}
