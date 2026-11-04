namespace AFKRaiders.Server.Domain;

/// <summary>One weighted entry in a BAL-2 robot defeat cargo or weapon table.</summary>
public sealed record RobotDefeatEntry(string CatalogId, int Weight);

/// <summary>
/// BAL-4: pure, authored per-danger robot defeat reward rules.
/// On committed defeat the selected Service Depot opportunity's two generic draws are replaced
/// by exactly two world-scoped defeat cargo slots. Slot A always draws nonweapon cargo;
/// Slot B first rolls the per-danger weapon chance and otherwise draws nonweapon cargo.
/// Draws use fixed weights (each table totals 100) with replacement and no exposure/appetite bias.
/// At most one field weapon per defeated world; Temptrest is never in any field table.
/// </summary>
public static class RobotDefeatRewards
{
    public const string RewardsVersion = "bal-4-defeat-v1";

    internal static string RewardsVersionFor(BalanceExperimentArm arm) =>
        BalanceExperimentRules.RewardsVersion(arm);

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<RobotDefeatEntry>> CargoByDanger =
        new Dictionary<string, IReadOnlyList<RobotDefeatEntry>>(StringComparer.Ordinal)
        {
            ["Low"] =
            [
                new("energy_core_spent", 70),
                new("robot_toe", 20),
                new("robot_knuckle_joint", 10)
            ],
            ["Medium"] =
            [
                new("circuit_bundle_messy", 50),
                new("arc_data_shard_corrupted", 30),
                new("arc_data_shard", 20)
            ],
            ["High"] =
            [
                new("arc_data_shard", 25),
                new("circuit_bundle_messy", 25),
                new("energy_core_overcharged", 15),
                new("arc_data_shard_legendary", 35)
            ]
        };

    /// <summary>Slot B weapon chance in permille: Low 2%, Medium 6%, High 12%.</summary>
    public static readonly IReadOnlyDictionary<string, int> WeaponChancePermille =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Low"] = 20,
            ["Medium"] = 60,
            ["High"] = 120
        };

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<RobotDefeatEntry>> WeaponsByDanger =
        new Dictionary<string, IReadOnlyList<RobotDefeatEntry>>(StringComparer.Ordinal)
        {
            ["Low"] = [new("aspperigo", 100)],
            ["Medium"] = [new("aspperigo", 75), new("vernerider", 25)],
            ["High"] = [new("vernerider", 100)]
        };

    /// <summary>The weapon gate passes strictly below the permille threshold: roll in [0,1).</summary>
    public static bool RollsWeapon(string danger, double roll) =>
        roll * 1000 < RequireTable(WeaponChancePermille, danger);

    /// <summary>Deterministic weighted pick over a fixed table; roll in [0,1) scales over the weight total.</summary>
    public static string Pick(IReadOnlyList<RobotDefeatEntry> entries, double roll)
    {
        if (entries.Count == 0) throw new InvalidOperationException("A robot defeat table cannot be empty.");
        var scaled = roll * entries.Sum(entry => entry.Weight);
        foreach (var entry in entries)
        {
            if (scaled < entry.Weight) return entry.CatalogId;
            scaled -= entry.Weight;
        }
        return entries[^1].CatalogId;
    }

    /// <summary>
    /// Contribution-ranked recipient order at the committed defeat snapshot:
    /// cumulative actual capped damage descending, then JoinedAt ascending,
    /// then ordinal Slot, then ordinal PlayerId.
    /// </summary>
    public static T[] RankContributors<T>(
        IEnumerable<(T Contributor, int DamageDealt, DateTimeOffset JoinedAt, string Slot, string PlayerId)> contributors) =>
        contributors
            .OrderByDescending(entry => entry.DamageDealt)
            .ThenBy(entry => entry.JoinedAt)
            .ThenBy(entry => entry.Slot, StringComparer.Ordinal)
            .ThenBy(entry => entry.PlayerId, StringComparer.Ordinal)
            .Select(entry => entry.Contributor)
            .ToArray();

    /// <summary>
    /// Creates the two defeat cargo instances for one defeated world. Seeded by the world seed with
    /// distinct raid/robot/slot/purpose roll keys; instance identity includes raid, robot and cargo slot.
    /// Invalid tables or unknown references fail explicitly instead of producing default loot.
    /// </summary>
    public static IReadOnlyList<ItemView> CreateDefeatItems(
        ILootCatalog catalog, string raidId, int seed, string robotId, string danger) =>
        CreateDefeatItems(catalog, raidId, seed, robotId, danger, BalanceExperimentArm.Arm3Baseline);

    internal static IReadOnlyList<ItemView> CreateDefeatItems(
        ILootCatalog catalog,
        string raidId,
        int seed,
        string robotId,
        string danger,
        BalanceExperimentArm arm)
    {
        var cargo = BalanceExperimentRules.CargoTable(danger, arm);
        ValidateWeights(cargo, danger, "cargo");
        var weapons = RequireTable(WeaponsByDanger, danger);
        ValidateWeights(weapons, danger, "weapon");

        var slotA = ResolveCargo(catalog,
            Pick(cargo, RaidConditions.Roll(seed, RollKey(raidId, robotId, "a", "cargo"))), danger);
        var gateRoll = RaidConditions.Roll(seed, RollKey(raidId, robotId, "b", "weapon-gate"));
        ItemView ResolveInstance(string slot, LootCatalogItem item) =>
            new($"bal-4:{raidId.Length}:{raidId}:{robotId.Length}:{robotId}:defeat:{slot}:{item.Id}",
                item.Id, item.Name, item.Value, item.Flavor);
        LootCatalogItem slotB;
        if (RollsWeapon(danger, gateRoll))
        {
            var weaponId = Pick(weapons,
                RaidConditions.Roll(seed, RollKey(raidId, robotId, "b", "weapon-select")));
            slotB = ResolveWeapon(catalog, weaponId, danger);
        }
        else
        {
            slotB = ResolveCargo(catalog,
                Pick(cargo, RaidConditions.Roll(seed, RollKey(raidId, robotId, "b", "cargo"))), danger);
        }
        return [ResolveInstance("a", slotA), ResolveInstance("b", slotB)];

        static LootCatalogItem ResolveCargo(ILootCatalog catalog, string catalogId, string danger)
        {
            var item = Require(catalog, catalogId, danger);
            if (item.Category.Equals("weapons", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Robot defeat cargo table for '{danger}' references weapon '{catalogId}' outside the weapon slot.");
            return item;
        }

        static LootCatalogItem ResolveWeapon(ILootCatalog catalog, string catalogId, string danger)
        {
            var item = Require(catalog, catalogId, danger);
            if (!item.Category.Equals("weapons", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"Robot defeat weapon table for '{danger}' references nonweapon '{catalogId}'.");
            if (!ShopCatalog.TryGetWeapon(catalogId, out var definition) || definition is null)
                throw new InvalidOperationException(
                    $"Robot defeat weapon '{catalogId}' has no authoritative weapon definition.");
            return item;
        }

        static LootCatalogItem Require(ILootCatalog catalog, string catalogId, string danger)
        {
            if (!catalog.TryGetItem(catalogId, out var item) || item is null)
                throw new InvalidOperationException(
                    $"Robot defeat table for '{danger}' references unknown catalog item '{catalogId}'.");
            return item;
        }
    }

    private static TValue RequireTable<TValue>(IReadOnlyDictionary<string, TValue> tables, string danger) =>
        tables.TryGetValue(danger, out var table)
            ? table
            : throw new InvalidOperationException($"No robot defeat table exists for danger '{danger}'.");

    private static string RollKey(string raidId, string robotId, string slot, string purpose) =>
        FormattableString.Invariant(
            $"bal-4:{raidId.Length}:{raidId}:{robotId.Length}:{robotId}:defeat:{slot}:{purpose}");

    private static void ValidateWeights(IReadOnlyList<RobotDefeatEntry> entries, string danger, string kind)
    {
        if (entries.Any(entry => entry.Weight <= 0))
            throw new InvalidOperationException($"Robot defeat {kind} table for '{danger}' has a non-positive weight.");
        if (entries.Sum(entry => entry.Weight) != 100)
            throw new InvalidOperationException($"Robot defeat {kind} table for '{danger}' weights must total exactly 100.");
        if (entries.Select(entry => entry.CatalogId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != entries.Count)
            throw new InvalidOperationException($"Robot defeat {kind} table for '{danger}' contains duplicate references.");
        if (entries.Any(entry => entry.CatalogId.Contains("temptrest", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Temptrest is trader-only and can never enter a field table.");
    }
}
