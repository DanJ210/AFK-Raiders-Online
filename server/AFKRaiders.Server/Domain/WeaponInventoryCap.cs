namespace AFKRaiders.Server.Domain;

public sealed record LiquidatedWeapon(
    OwnedWeapon Weapon,
    int CatalogValue,
    int CoinsReceived);

public sealed record WeaponInventoryCapResult(
    IReadOnlyList<OwnedWeapon> RetainedWeapons,
    IReadOnlyList<LiquidatedWeapon> LiquidatedWeapons);

public static class WeaponInventoryCap
{
    public const int MaximumOwnedWeapons = 4;
    // BAL-4 Arm 2: overflow sell-back floor(0.25 x catalog value); Kettle stays 0.
    public const int SellBackPermille = 250;

    private sealed record Candidate(
        OwnedWeapon Weapon,
        WeaponDefinition Definition,
        bool IsProtected,
        double ExpectedDamage);

    public static WeaponInventoryCapResult Normalize(
        IReadOnlyList<OwnedWeapon> ownedWeapons,
        string? equippedInstanceId,
        string? neededFallbackInstanceId,
        IReadOnlyList<WeaponDefinition> catalog)
    {
        var catalogById = catalog.ToDictionary(
            definition => definition.Id,
            StringComparer.OrdinalIgnoreCase);
        var uniqueWeapons = new Dictionary<string, OwnedWeapon>(StringComparer.Ordinal);
        foreach (var weapon in ownedWeapons)
        {
            if (!uniqueWeapons.TryAdd(weapon.InstanceId, weapon))
            {
                var existing = uniqueWeapons[weapon.InstanceId];
                if (!existing.CatalogId.Equals(weapon.CatalogId, StringComparison.OrdinalIgnoreCase)
                    || existing != weapon)
                    throw new InvalidOperationException(
                        $"Weapon instance '{weapon.InstanceId}' has conflicting authoritative state.");
            }
        }

        if (equippedInstanceId is not null && !uniqueWeapons.ContainsKey(equippedInstanceId))
            throw new InvalidOperationException(
                $"Equipped weapon instance '{equippedInstanceId}' is not in the owned inventory.");
        if (neededFallbackInstanceId is not null
            && (!uniqueWeapons.TryGetValue(neededFallbackInstanceId, out var fallback)
                || fallback.CurrentDurability <= 0))
            throw new InvalidOperationException(
                $"Needed fallback weapon instance '{neededFallbackInstanceId}' is not usable and owned.");

        var candidates = uniqueWeapons.Values.Select(weapon =>
        {
            if (!catalogById.TryGetValue(weapon.CatalogId, out var definition))
                throw new InvalidOperationException(
                    $"Owned weapon '{weapon.InstanceId}' references unknown catalog item '{weapon.CatalogId}'.");
            if (weapon.MaxDurability <= 0 || definition.Value < 0)
                throw new InvalidOperationException(
                    $"Owned weapon '{weapon.InstanceId}' has invalid durability or catalog value.");
            return new Candidate(
                weapon,
                definition,
                weapon.InstanceId == equippedInstanceId || weapon.InstanceId == neededFallbackInstanceId,
                ExpectedDamage(definition));
        }).ToList();

        candidates.Sort(CompareCandidates);
        var retainedIds = candidates.Take(MaximumOwnedWeapons)
            .Select(candidate => candidate.Weapon.InstanceId)
            .ToHashSet(StringComparer.Ordinal);
        var retained = ownedWeapons
            .Where(weapon => retainedIds.Contains(weapon.InstanceId))
            .DistinctBy(weapon => weapon.InstanceId, StringComparer.Ordinal)
            .ToArray();
        var liquidated = candidates
            .Where(candidate => !retainedIds.Contains(candidate.Weapon.InstanceId))
            .OrderBy(candidate => candidate.Weapon.CatalogId, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Weapon.InstanceId, StringComparer.Ordinal)
            .Select(candidate => new LiquidatedWeapon(
                candidate.Weapon,
                candidate.Definition.Value,
                (int)((long)candidate.Definition.Value * SellBackPermille / 1000)))
            .ToArray();

        return new WeaponInventoryCapResult(retained, liquidated);
    }

    private static int CompareCandidates(Candidate left, Candidate right)
    {
        var comparison = right.IsProtected.CompareTo(left.IsProtected);
        if (comparison != 0) return comparison;

        comparison = (right.Weapon.CurrentDurability > 0).CompareTo(left.Weapon.CurrentDurability > 0);
        if (comparison != 0) return comparison;

        comparison = right.ExpectedDamage.CompareTo(left.ExpectedDamage);
        if (comparison != 0) return comparison;

        comparison = CompareDurabilityRatio(left.Weapon, right.Weapon);
        if (comparison != 0) return comparison;

        comparison = StringComparer.Ordinal.Compare(left.Weapon.CatalogId, right.Weapon.CatalogId);
        return comparison != 0
            ? comparison
            : StringComparer.Ordinal.Compare(left.Weapon.InstanceId, right.Weapon.InstanceId);
    }

    private static int CompareDurabilityRatio(OwnedWeapon left, OwnedWeapon right)
    {
        var leftScaled = (long)left.CurrentDurability * right.MaxDurability;
        var rightScaled = (long)right.CurrentDurability * left.MaxDurability;
        return rightScaled.CompareTo(leftScaled);
    }

    private static double ExpectedDamage(WeaponDefinition definition) =>
        Enumerable.Range(definition.DamageMin, definition.DamageMax - definition.DamageMin + 1)
            .Average(roll => (int)Math.Round(
                roll * definition.DamageMultiplier,
                MidpointRounding.AwayFromZero));
}
