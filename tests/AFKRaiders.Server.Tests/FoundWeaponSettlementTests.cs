using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class FoundWeaponSettlementTests
{
    [Fact]
    public void FieldWeaponsBecomeDamagedEquipmentAndNeverEnterStashSalesOrCapacity()
    {
        var catalog = LootCatalog.Default;
        var extracted = Enumerable.Range(0, HomeStash.MaxCapacity + 1)
            .Select(index => new ItemView(
                $"salvage-{index}", "scrap_metal_basic", "Basic Scrap Metal", 8, "scrap"))
            .Append(new ItemView("raid:weapon", "vernerider", "Vernerider", 140, "flavor"))
            .ToArray();

        var settlement = FoundWeaponSettlement.Separate(extracted, [], catalog);
        var weapon = Assert.Single(settlement.WeaponsToKeep);
        Assert.Equal("vernerider", weapon.CatalogId);
        Assert.InRange(weapon.CurrentDurability / (double)weapon.MaxDurability, 0.05, 0.60);
        Assert.Equal(extracted.Length - 1, settlement.StashItems.Count);
        Assert.DoesNotContain(settlement.StashItems, item => item.CatalogId == "vernerider");

        var retry = FoundWeaponSettlement.Separate(extracted, settlement.WeaponsToKeep, catalog);
        Assert.Empty(retry.WeaponsToKeep);
        Assert.DoesNotContain(retry.StashItems, item => item.CatalogId == "vernerider");

        var starter = ShopCatalog.CreateWeaponInstance(ShopCatalog.GetStarterWeapon(), "starter");
        var preparation = RaiderEconomyPolicy.Decide(
            "owner", 1, 1, DateTimeOffset.UnixEpoch, 0,
            [], [starter, weapon], starter.InstanceId,
            new Dictionary<string, int>(), [], catalog,
            ShopCatalog.Weapons, ShopCatalog.Supplies,
            new Dictionary<string, int>());
        Assert.Equal(weapon.InstanceId, preparation.WeaponInstanceIdToEquip);

        var stash = new HomeStash();
        var transfer = stash.TransferHaul(settlement.StashItems);
        Assert.Equal(HomeStash.MaxCapacity, stash.Count);
        Assert.Single(transfer.AutoSoldItems);
        Assert.Equal(HomeStash.MaxCapacity + 1, transfer.IncomingHaul.Count);

        var plan = RaiderEconomyPolicy.Decide(
            "owner", 1, 1, DateTimeOffset.UnixEpoch, 0,
            stash.Items, settlement.WeaponsToKeep, weapon.InstanceId,
            new Dictionary<string, int>(), [], catalog,
            ShopCatalog.Weapons, ShopCatalog.Supplies,
            new Dictionary<string, int>());
        Assert.DoesNotContain(plan.ItemInstanceIdsToSell, instanceId => instanceId == weapon.InstanceId);
    }

    [Theory]
    [InlineData("aspperigo")]
    [InlineData("vernerider")]
    public void FoundConditionIsRepeatableVariedAndWithinFiveToSixtyPercent(string catalogId)
    {
        var catalog = LootCatalog.Default;
        var item = catalog.GetItem(catalogId);
        var extracted = Enumerable.Range(0, 1000)
            .Select(index => new ItemView($"raid-{index}:weapon", item.Id, item.Name, item.Value, item.Flavor))
            .ToArray();

        var first = FoundWeaponSettlement.Separate(extracted, [], catalog);
        var replay = FoundWeaponSettlement.Separate(extracted.Reverse().ToArray(), [], catalog);
        Assert.Equal(first.WeaponsToKeep.OrderBy(weapon => weapon.InstanceId),
            replay.WeaponsToKeep.OrderBy(weapon => weapon.InstanceId));
        Assert.All(first.WeaponsToKeep, weapon =>
        {
            Assert.InRange(weapon.CurrentDurability / (double)weapon.MaxDurability, 0.05, 0.60);
            Assert.False(weapon.IsBroken);
        });
        var definition = ShopCatalog.Weapons.Single(weapon => weapon.Id == catalogId);
        Assert.Equal(Enumerable.Range((int)Math.Ceiling(definition.DurabilityMax * 0.05),
                (int)Math.Floor(definition.DurabilityMax * 0.60) -
                (int)Math.Ceiling(definition.DurabilityMax * 0.05) + 1),
            first.WeaponsToKeep.Select(weapon => weapon.CurrentDurability).Distinct().Order());

        var alreadyRepaired = first.WeaponsToKeep.Select(weapon =>
            weapon with { CurrentDurability = weapon.MaxDurability }).ToArray();
        Assert.Empty(FoundWeaponSettlement.Separate(extracted, alreadyRepaired, catalog).WeaponsToKeep);
        Assert.All(alreadyRepaired, weapon => Assert.Equal(weapon.MaxDurability, weapon.CurrentDurability));
        var purchased = ShopCatalog.CreateWeaponInstance(definition, "trader-purchase");
        Assert.Equal(purchased.MaxDurability, purchased.CurrentDurability);
    }

    [Theory]
    [InlineData("aspperigo")]
    [InlineData("vernerider")]
    public void EquippedFoundWeaponsAreEligibleForExistingAutonomousRepair(string catalogId)
    {
        var catalog = LootCatalog.Default;
        var item = catalog.GetItem(catalogId);
        var weapon = Assert.Single(FoundWeaponSettlement.Separate(
            [new ItemView("raid:weapon", item.Id, item.Name, item.Value, item.Flavor)], [], catalog).WeaponsToKeep);
        var plan = RaiderEconomyPolicy.Decide(
            "owner", 1, (int)RaiderPrimaryTrait.Cautious, DateTimeOffset.UnixEpoch, 100,
            [], [weapon], weapon.InstanceId, new Dictionary<string, int>(), [], catalog,
            ShopCatalog.Weapons, ShopCatalog.Supplies, new Dictionary<string, int>());

        Assert.Equal(weapon.InstanceId, plan.WeaponInstanceIdToRepair);
        Assert.Null(plan.WeaponInstanceIdToEquip);
    }
}
