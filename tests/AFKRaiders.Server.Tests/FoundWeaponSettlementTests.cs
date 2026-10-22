using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class FoundWeaponSettlementTests
{
    [Fact]
    public void FieldWeaponsBecomeFullDurabilityEquipmentAndNeverEnterStashSalesOrCapacity()
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
        Assert.Equal(weapon.MaxDurability, weapon.CurrentDurability);
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
}
