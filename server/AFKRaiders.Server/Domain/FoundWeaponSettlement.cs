namespace AFKRaiders.Server.Domain;

public sealed record FoundWeaponSettlementResult(
    IReadOnlyList<OwnedWeapon> WeaponsToKeep,
    IReadOnlyList<ItemView> StashItems);

public static class FoundWeaponSettlement
{
    public static FoundWeaponSettlementResult Separate(
        IReadOnlyList<ItemView> extractedItems,
        IReadOnlyList<OwnedWeapon> ownedWeapons,
        ILootCatalog lootCatalog)
    {
        var additions = new List<OwnedWeapon>();
        var stashItems = new List<ItemView>();
        var ownedIds = ownedWeapons.ToDictionary(weapon => weapon.InstanceId, StringComparer.Ordinal);
        foreach (var item in extractedItems)
        {
            if (!lootCatalog.TryGetItem(item.CatalogId, out var lootItem)
                || lootItem is null
                || !lootItem.Category.Equals("weapons", StringComparison.OrdinalIgnoreCase))
            {
                stashItems.Add(item);
                continue;
            }

            if (!ShopCatalog.TryGetWeapon(item.CatalogId, out var definition) || definition is null)
                throw new InvalidOperationException($"Found weapon '{item.CatalogId}' has no authoritative weapon definition.");
            if (ownedIds.TryGetValue(item.InstanceId, out var existing))
            {
                if (!existing.CatalogId.Equals(item.CatalogId, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Found weapon instance '{item.InstanceId}' conflicts with existing equipment.");
                continue;
            }

            var weapon = ShopCatalog.CreateWeaponInstance(definition, item.InstanceId);
            ownedIds.Add(weapon.InstanceId, weapon);
            additions.Add(weapon);
        }

        return new FoundWeaponSettlementResult(additions, stashItems);
    }
}
