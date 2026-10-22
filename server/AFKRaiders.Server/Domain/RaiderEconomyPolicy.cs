using System.Security.Cryptography;
using System.Text;

namespace AFKRaiders.Server.Domain;

public enum RaiderPrimaryTrait
{
    Cautious,
    Hoarder,
    Brawler,
    Gadgeteer
}

public sealed record SupplyPurchasePlan(string CatalogId, int Quantity);

public sealed record RaiderPreparationPlan(
    RaiderPrimaryTrait PrimaryTrait,
    DateTimeOffset ArrivedAt,
    IReadOnlyList<string> ItemInstanceIdsToSell,
    string? WeaponInstanceIdToRepair,
    string? WeaponCatalogIdToBuy,
    string? WeaponInstanceIdToBuy,
    string? WeaponInstanceIdToEquip,
    bool CreateTeaKettleFallback,
    IReadOnlyList<SupplyPurchasePlan> SupplyPurchases,
    IReadOnlyList<StagedSupplyItem> StagedSupplies,
    bool UpdateStagedSupplies,
    int ReserveFloor);

public static class RaiderEconomyPolicy
{
    private sealed record TraitWeights(
        double Weapon,
        double Medical,
        double Recharger,
        double ReservePercent,
        int RepairThresholdPercent,
        bool KeepsThreeItems);

    private sealed record RankedSupply(SupplyDefinition Definition, int CatalogOrder, double Score);

    private sealed record SimulatedWeapon(OwnedWeapon Weapon, int CatalogOrder);

    private static readonly TraitWeights Cautious = new(0.8, 1.5, 1.2, 0.40, 75, false);
    private static readonly TraitWeights Hoarder = new(0.8, 1.0, 1.0, 0.50, 50, true);
    private static readonly TraitWeights Brawler = new(1.5, 0.8, 0.8, 0.15, 25, false);
    private static readonly TraitWeights Gadgeteer = new(1.0, 1.0, 1.5, 0.25, 50, false);

    public static RaiderPrimaryTrait TraitFromSeed(int seed) =>
        (RaiderPrimaryTrait)((uint)seed % Enum.GetValues<RaiderPrimaryTrait>().Length);

    public static string CreateWeaponInstanceId(string ownerId, int visit, string catalogId, string purpose = "purchase")
    {
        var input = Encoding.UTF8.GetBytes($"{ownerId}:{visit}:{catalogId}:{purpose}");
        var suffix = Convert.ToHexString(SHA256.HashData(input))[..12];
        return $"wpn_{catalogId}_{suffix}";
    }

    public static RaiderPreparationPlan Decide(
        string ownerId,
        int visit,
        int traitSeed,
        DateTimeOffset arrivedAt,
        int coins,
        IReadOnlyList<ItemView> stash,
        IReadOnlyList<OwnedWeapon> ownedWeapons,
        string? equippedWeaponInstanceId,
        IReadOnlyDictionary<string, int> supplyStock,
        IReadOnlyList<StagedSupplyItem> stagedSupplies,
        ILootCatalog lootCatalog,
        IReadOnlyList<WeaponDefinition> weaponCatalog,
        IReadOnlyList<SupplyDefinition> supplyCatalog,
        IReadOnlyDictionary<string, int>? traderStock = null,
        int? reserveFloorOverride = null)
    {
        var trait = TraitFromSeed(traitSeed);
        var weights = Weights(trait);
        var lootOrder = lootCatalog.Items
            .Select((item, index) => (item.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.Ordinal);
        var weaponOrder = weaponCatalog
            .Select((weapon, index) => (weapon.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.OrdinalIgnoreCase);

        var keepIds = weights.KeepsThreeItems
            ? stash.OrderByDescending(item => item.Value)
                .ThenBy(item => lootOrder.GetValueOrDefault(item.CatalogId, int.MaxValue))
                .ThenBy(item => item.InstanceId, StringComparer.Ordinal)
                .Take(3)
                .Select(item => item.InstanceId)
                .ToHashSet(StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);
        var itemsToSell = stash
            .Where(item => !keepIds.Contains(item.InstanceId))
            .OrderBy(item => lootOrder.GetValueOrDefault(item.CatalogId, int.MaxValue))
            .ThenBy(item => item.InstanceId, StringComparer.Ordinal)
            .Select(item => item.InstanceId)
            .ToArray();
        var saleValue = stash.Where(item => itemsToSell.Contains(item.InstanceId, StringComparer.Ordinal))
            .Sum(item => item.Value);
        var postSaleCoins = checked(coins + saleValue);
        var percentageReserve = Math.Max(
            (int)Math.Ceiling(postSaleCoins * weights.ReservePercent),
            reserveFloorOverride ?? 0);

        var weapons = ownedWeapons
            .Select(weapon => new SimulatedWeapon(
                weapon,
                weaponOrder.GetValueOrDefault(weapon.CatalogId, int.MaxValue)))
            .ToList();
        string? weaponToRepair = null;
        var currentEquipped = weapons.FirstOrDefault(weapon =>
            weapon.Weapon.InstanceId == equippedWeaponInstanceId);
        var repairWeapon = currentEquipped?.Weapon;

        if (repairWeapon is not null
            && IsRepairDue(repairWeapon, weights.RepairThresholdPercent)
            && CanSpend(postSaleCoins, repairWeapon.RepairCost, percentageReserve, repairWeapon.RepairCost))
        {
            weaponToRepair = repairWeapon.InstanceId;
            RepairInPlan(weapons, repairWeapon.InstanceId);
        }

        var kettle = weaponCatalog.FirstOrDefault(weapon =>
            weapon.Id.Equals("tea_kettle", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("The Tea Kettle fallback is missing from the shop catalog.");
        var kettleOrder = weaponOrder.GetValueOrDefault(kettle.Id, int.MaxValue);
        var usableOwned = BestWeapon(weapons, weaponCatalog);
        var createFallback = false;
        if (usableOwned is null)
        {
            var ownedKettle = weapons.FirstOrDefault(weapon =>
                weapon.Weapon.CatalogId.Equals(kettle.Id, StringComparison.OrdinalIgnoreCase));
            if (ownedKettle is { Weapon.CurrentDurability: <= 0, Weapon.RepairCost: 0 })
            {
                weaponToRepair ??= ownedKettle.Weapon.InstanceId;
                RepairInPlan(weapons, ownedKettle.Weapon.InstanceId);
            }
            else if (ownedKettle is null)
            {
                createFallback = true;
                var fallback = CreateOwnedWeapon(
                    kettle,
                    CreateWeaponInstanceId(ownerId, visit, kettle.Id, "fallback"),
                    weaponCatalog);
                weapons.Add(fallback);
            }
        }

        var bestOwned = BestWeapon(weapons, weaponCatalog);
        var weaponBaseline = bestOwned is null ? Damage(kettle) : Damage(bestOwned.Weapon, weaponCatalog);
        var postRepairCoins = postSaleCoins - (weaponToRepair is null
            ? 0
            : ownedWeapons.First(weapon => weapon.InstanceId == weaponToRepair).RepairCost);
        var candidateWeapon = weaponCatalog
            .Select((weapon, index) => new
            {
                Definition = weapon,
                CatalogOrder = index,
                Benefit = Damage(weapon) - weaponBaseline
            })
            .Where(candidate => candidate.Definition.Value > 0 && candidate.Benefit > 0
                && (traderStock is null || traderStock.GetValueOrDefault(candidate.Definition.Id) > 0)
                && CanSpend(postRepairCoins, candidate.Definition.Value, percentageReserve,
                    Math.Max(bestOwned?.Weapon.RepairCost ?? kettle.RepairCost, candidate.Definition.RepairCost)))
            .Select(candidate => new
            {
                candidate.Definition,
                candidate.CatalogOrder,
                Score = candidate.Benefit * weights.Weapon / candidate.Definition.Value,
                InstanceId = CreateWeaponInstanceId(ownerId, visit, candidate.Definition.Id),
                RepairCost = candidate.Definition.RepairCost
            })
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.CatalogOrder)
            .ThenBy(candidate => candidate.InstanceId, StringComparer.Ordinal)
            .FirstOrDefault();

        string? weaponToBuy = null;
        string? weaponPurchaseInstanceId = null;
        var weaponCoins = postRepairCoins;
        if (candidateWeapon is not null)
        {
            weaponToBuy = candidateWeapon.Definition.Id;
            weaponPurchaseInstanceId = candidateWeapon.InstanceId;
            weaponCoins -= candidateWeapon.Definition.Value;
            weapons.Add(CreateOwnedWeapon(candidateWeapon.Definition, candidateWeapon.InstanceId, weaponCatalog));
        }

        if (createFallback)
            weaponCoins = postRepairCoins - (candidateWeapon?.Definition.Value ?? 0);

        var equipped = BestWeapon(weapons, weaponCatalog);
        if (equipped is null)
        {
            var fallback = CreateOwnedWeapon(
                kettle,
                CreateWeaponInstanceId(ownerId, visit, kettle.Id, "fallback"),
                weaponCatalog);
            weapons.Add(fallback);
            equipped = fallback;
            createFallback = true;
        }

        var equipInstanceId = equipped.Weapon.InstanceId == equippedWeaponInstanceId
            ? null
            : equipped.Weapon.InstanceId;
        var equippedRepairCost = equipped.Weapon.RepairCost;
        var supplyReserve = Math.Max(percentageReserve, equippedRepairCost);
        var remainingCoins = weaponCoins;
        var supplyPurchases = new List<SupplyPurchasePlan>();
        var staged = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var rankedSupplies = supplyCatalog
            .Select((definition, index) => new RankedSupply(
                definition,
                index,
                definition.Value <= 0
                    ? 0
                    : Benefit(definition) * SupplyWeight(definition.Category, weights) / definition.Value))
            .Where(candidate => candidate.Score > 0
                && (traderStock is null || traderStock.GetValueOrDefault(candidate.Definition.Id) > 0))
            .OrderByDescending(candidate => candidate.Score)
            .ThenBy(candidate => candidate.CatalogOrder)
            .ThenBy(candidate => candidate.Definition.Id, StringComparer.Ordinal)
            .ToArray();
        var medicalRemaining = ShopCatalog.MaxStagedMedicalCount;
        var rechargerRemaining = ShopCatalog.MaxStagedRechargerCount;
        var remainingTraderStock = traderStock is null
            ? null
            : new Dictionary<string, int>(traderStock, StringComparer.OrdinalIgnoreCase);

        foreach (var candidate in rankedSupplies)
        {
            var categoryRemaining = IsMedical(candidate.Definition)
                ? medicalRemaining
                : rechargerRemaining;
            var owned = Math.Max(0, supplyStock.GetValueOrDefault(candidate.Definition.Id));
            var use = Math.Min(categoryRemaining, owned);
            if (use <= 0) continue;
            staged[candidate.Definition.Id] = use;
            if (IsMedical(candidate.Definition))
                medicalRemaining -= use;
            else
                rechargerRemaining -= use;
        }

        while (medicalRemaining > 0 || rechargerRemaining > 0)
        {
            var affordable = rankedSupplies.FirstOrDefault(candidate =>
                (IsMedical(candidate.Definition) ? medicalRemaining > 0 : rechargerRemaining > 0)
                && (remainingTraderStock is null
                    || remainingTraderStock.GetValueOrDefault(candidate.Definition.Id) > 0)
                && CanSpend(remainingCoins, candidate.Definition.Value, supplyReserve,
                    equippedRepairCost));
            if (affordable is null) break;

            staged[affordable.Definition.Id] = staged.GetValueOrDefault(affordable.Definition.Id) + 1;
            remainingCoins -= affordable.Definition.Value;
            if (remainingTraderStock is not null)
                remainingTraderStock[affordable.Definition.Id]--;
            var purchaseIndex = supplyPurchases.FindIndex(purchase =>
                purchase.CatalogId.Equals(affordable.Definition.Id, StringComparison.OrdinalIgnoreCase));
            if (purchaseIndex >= 0)
                supplyPurchases[purchaseIndex] = supplyPurchases[purchaseIndex] with
                {
                    Quantity = supplyPurchases[purchaseIndex].Quantity + 1
                };
            else
                supplyPurchases.Add(new SupplyPurchasePlan(affordable.Definition.Id, 1));

            if (IsMedical(affordable.Definition))
                medicalRemaining--;
            else
                rechargerRemaining--;
        }

        var stagedItems = staged
            .Where(entry => entry.Value > 0)
            .Select(entry =>
            {
                var definition = supplyCatalog.First(supply =>
                    supply.Id.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
                return new StagedSupplyItem(definition.Id, definition.Name, definition.Category, entry.Value);
            })
            .OrderBy(item => supplyCatalog.ToList().FindIndex(supply =>
                supply.Id.Equals(item.CatalogId, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var stageChanged = !SameStaging(stagedSupplies, stagedItems);

        return new RaiderPreparationPlan(
            trait,
            arrivedAt,
            itemsToSell,
            weaponToRepair,
            weaponToBuy,
            weaponPurchaseInstanceId,
            equipInstanceId,
            createFallback,
            supplyPurchases,
            stageChanged ? stagedItems : [],
            stageChanged,
            Math.Max(percentageReserve, equippedRepairCost));
    }

    private static TraitWeights Weights(RaiderPrimaryTrait trait) => trait switch
    {
        RaiderPrimaryTrait.Cautious => Cautious,
        RaiderPrimaryTrait.Hoarder => Hoarder,
        RaiderPrimaryTrait.Brawler => Brawler,
        RaiderPrimaryTrait.Gadgeteer => Gadgeteer,
        _ => throw new ArgumentOutOfRangeException(nameof(trait))
    };

    private static bool IsRepairDue(OwnedWeapon weapon, int thresholdPercent) =>
        weapon.CurrentDurability < weapon.MaxDurability
        && weapon.CurrentDurability * 100 <= weapon.MaxDurability * thresholdPercent;

    private static bool CanSpend(int coins, int cost, int percentageReserve, int repairCost) =>
        cost >= 0 && coins >= cost && coins - cost >= Math.Max(percentageReserve, repairCost);

    private static bool IsMedical(SupplyDefinition supply) =>
        supply.Category.Equals("Medical", StringComparison.Ordinal);

    private static double Benefit(SupplyDefinition supply) =>
        Math.Max(supply.HealAmount, Math.Max(supply.ReviveAmount, supply.ChargeAmount));

    private static double SupplyWeight(string category, TraitWeights weights) =>
        category switch
        {
            "Medical" => weights.Medical,
            "ShieldRecharger" => weights.Recharger,
            _ => 0
        };

    private static SimulatedWeapon? BestWeapon(
        IEnumerable<SimulatedWeapon> weapons,
        IReadOnlyList<WeaponDefinition> catalog)
    {
        var catalogOrder = catalog.Select((weapon, index) => (weapon.Id, index))
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.OrdinalIgnoreCase);
        return weapons
            .Where(weapon => weapon.Weapon.CurrentDurability > 0
                && catalogOrder.ContainsKey(weapon.Weapon.CatalogId))
            .OrderByDescending(weapon => Damage(weapon.Weapon, catalog))
            .ThenBy(weapon => catalogOrder[weapon.Weapon.CatalogId])
            .ThenBy(weapon => weapon.Weapon.InstanceId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static OwnedWeapon? BestUsableWeapon(
        IReadOnlyList<OwnedWeapon> weapons,
        IReadOnlyList<WeaponDefinition> catalog) =>
        weapons
            .Where(weapon => weapon.CurrentDurability > 0
                && catalog.Any(definition => definition.Id.Equals(
                    weapon.CatalogId, StringComparison.OrdinalIgnoreCase)))
            .Select(weapon => new
            {
                Weapon = weapon,
                CatalogOrder = catalog.Select((definition, index) => (definition.Id, index))
                    .Single(entry => entry.Id.Equals(
                        weapon.CatalogId, StringComparison.OrdinalIgnoreCase)).index,
                Damage = Damage(weapon, catalog)
            })
            .OrderByDescending(weapon => weapon.Damage)
            .ThenBy(weapon => weapon.CatalogOrder)
            .ThenBy(weapon => weapon.Weapon.InstanceId, StringComparer.Ordinal)
            .Select(weapon => weapon.Weapon)
            .FirstOrDefault();

    private static double Damage(WeaponDefinition weapon) =>
        (weapon.DamageMin + weapon.DamageMax) / 2d * weapon.DamageMultiplier;

    private static double Damage(OwnedWeapon weapon, IReadOnlyList<WeaponDefinition> catalog)
    {
        var definition = catalog.FirstOrDefault(item =>
            item.Id.Equals(weapon.CatalogId, StringComparison.OrdinalIgnoreCase));
        return definition is null ? 0 : Damage(definition);
    }

    private static void RepairInPlan(List<SimulatedWeapon> weapons, string instanceId)
    {
        var index = weapons.FindIndex(weapon => weapon.Weapon.InstanceId == instanceId);
        if (index < 0) return;
        var current = weapons[index];
        weapons[index] = current with
        {
            Weapon = current.Weapon with { CurrentDurability = current.Weapon.MaxDurability }
        };
    }

    private static SimulatedWeapon CreateOwnedWeapon(
        WeaponDefinition definition,
        string instanceId,
        IReadOnlyList<WeaponDefinition> weaponCatalog)
    {
        var order = weaponCatalog.ToList().FindIndex(weapon =>
            weapon.Id.Equals(definition.Id, StringComparison.OrdinalIgnoreCase));
        return new SimulatedWeapon(new OwnedWeapon(
            instanceId,
            definition.Id,
            definition.Name,
            definition.DurabilityMax,
            definition.DurabilityMax,
            definition.RepairCost,
            definition.Rarity,
            definition.Flavor), order);
    }

    private static bool SameStaging(
        IReadOnlyList<StagedSupplyItem> current,
        IReadOnlyList<StagedSupplyItem> next)
    {
        var currentItems = current.Where(item => item.Quantity > 0)
            .ToDictionary(item => item.CatalogId, item => item.Quantity, StringComparer.OrdinalIgnoreCase);
        var nextItems = next.ToDictionary(item => item.CatalogId, item => item.Quantity,
            StringComparer.OrdinalIgnoreCase);
        return currentItems.Count == nextItems.Count
            && currentItems.All(entry => nextItems.GetValueOrDefault(entry.Key) == entry.Value);
    }
}
