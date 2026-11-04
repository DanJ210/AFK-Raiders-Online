using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class WeaponInventoryCapTests
{
    [Fact]
    public void NormalizationProtectsEquippedAndNeededFallbackThenSellsWorstOverflowAtQuarterValue()
    {
        var weapons = new[]
        {
            Owned("equipped-kettle", "tea_kettle", current: 0),
            Owned("fallback-kettle", "tea_kettle", current: 5),
            Owned("asp", "aspperigo", current: 10),
            Owned("vernerider", "vernerider", current: 12),
            Owned("temptrest", "temptrest_ar", current: 14),
            Owned("broken-vernerider", "vernerider", current: 0)
        };

        var result = WeaponInventoryCap.Normalize(
            weapons,
            equippedInstanceId: "equipped-kettle",
            neededFallbackInstanceId: "fallback-kettle",
            ShopCatalog.Weapons);

        Assert.Equal(WeaponInventoryCap.MaximumOwnedWeapons, result.RetainedWeapons.Count);
        Assert.Contains(result.RetainedWeapons, weapon => weapon.InstanceId == "equipped-kettle");
        Assert.Contains(result.RetainedWeapons, weapon => weapon.InstanceId == "fallback-kettle");
        Assert.Contains(result.RetainedWeapons, weapon => weapon.InstanceId == "temptrest");
        Assert.Contains(result.RetainedWeapons, weapon => weapon.InstanceId == "vernerider");
        Assert.Equal(["asp", "broken-vernerider"],
            result.LiquidatedWeapons.Select(item => item.Weapon.InstanceId).Order(StringComparer.Ordinal));
        // BAL-4 Arm 2: floor(0.25 x catalog value): Aspperigo floor(16.25) = 16, Vernerider 35.
        Assert.Equal(16, result.LiquidatedWeapons.Single(item => item.Weapon.InstanceId == "asp").CoinsReceived);
        Assert.Equal(35, result.LiquidatedWeapons.Single(item => item.Weapon.InstanceId == "broken-vernerider").CoinsReceived);
    }

    [Fact]
    public void NormalizationUsesDeterministicDamageDurabilityAndOrdinalTieBreaks()
    {
        var weapons = new[]
        {
            Owned("tie-z", "aspperigo", current: 5),
            Owned("tie-a", "aspperigo", current: 5),
            Owned("kettle", "tea_kettle", current: 10),
            Owned("vernerider", "vernerider", current: 12),
            Owned("temptrest", "temptrest_ar", current: 14),
            Owned("broken-heavy", "temptrest_ar", current: 0)
        };

        var first = WeaponInventoryCap.Normalize(weapons, null, null, ShopCatalog.Weapons);
        var second = WeaponInventoryCap.Normalize(weapons.Reverse().ToArray(), null, null, ShopCatalog.Weapons);

        Assert.Equal(["temptrest", "tie-a", "tie-z", "vernerider"],
            first.RetainedWeapons.Select(weapon => weapon.InstanceId).Order(StringComparer.Ordinal));
        Assert.Equal(first.RetainedWeapons.Select(weapon => weapon.InstanceId).Order(StringComparer.Ordinal),
            second.RetainedWeapons.Select(weapon => weapon.InstanceId).Order(StringComparer.Ordinal));
        Assert.Equal(["broken-heavy", "kettle"],
            first.LiquidatedWeapons.Select(item => item.Weapon.InstanceId).Order(StringComparer.Ordinal));
        // The zero-value loaner Kettle liquidates for exactly zero coins at any sell-back rate.
        Assert.Equal(0, first.LiquidatedWeapons.Single(item => item.Weapon.InstanceId == "kettle").CoinsReceived);
        Assert.Equal(70, first.LiquidatedWeapons.Single(item => item.Weapon.InstanceId == "broken-heavy").CoinsReceived);
    }

    [Fact]
    public void DuplicateInstanceIdsAreDeduplicatedWithoutASecondSale()
    {
        var weapons = new[]
        {
            Owned("duplicate", "vernerider", current: 3),
            Owned("duplicate", "vernerider", current: 3)
        };

        var result = WeaponInventoryCap.Normalize(weapons, null, null, ShopCatalog.Weapons);

        Assert.Single(result.RetainedWeapons);
        Assert.Empty(result.LiquidatedWeapons);
    }

    [Fact]
    public void DuplicateInstanceWithConflictingCatalogAndMissingProtectionFailExplicitly()
    {
        var conflicting = new[]
        {
            Owned("duplicate", "vernerider", current: 3),
            Owned("duplicate", "aspperigo", current: 10)
        };

        Assert.Throws<InvalidOperationException>(() =>
            WeaponInventoryCap.Normalize(conflicting, null, null, ShopCatalog.Weapons));
        Assert.Throws<InvalidOperationException>(() =>
            WeaponInventoryCap.Normalize(
                [Owned("duplicate", "vernerider", current: 3), Owned("duplicate", "vernerider", current: 12)],
                null,
                null,
                ShopCatalog.Weapons));
        Assert.Throws<InvalidOperationException>(() =>
            WeaponInventoryCap.Normalize([Owned("owned", "aspperigo", current: 10)],
                equippedInstanceId: "missing", neededFallbackInstanceId: null, ShopCatalog.Weapons));
        Assert.Throws<InvalidOperationException>(() =>
            WeaponInventoryCap.Normalize([Owned("broken", "tea_kettle", current: 0)],
                equippedInstanceId: null, neededFallbackInstanceId: "broken", ShopCatalog.Weapons));
    }

    private static OwnedWeapon Owned(string instanceId, string catalogId, int current)
    {
        var definition = ShopCatalog.Weapons.Single(weapon =>
            weapon.Id.Equals(catalogId, StringComparison.OrdinalIgnoreCase));
        return ShopCatalog.CreateWeaponInstance(definition, instanceId) with
        {
            CurrentDurability = current
        };
    }
}
