namespace AFKRaiders.Server.Domain;

public sealed record TraderStockEntry(
    string CatalogId,
    string Name,
    string Category,
    int? Quantity,
    int UnitPrice);

public sealed record TraderStockSnapshot(long CycleIndex, IReadOnlyList<TraderStockEntry> Items);

public sealed class TraderStockGenerator(int seed, TimeProvider time)
{
    public static readonly TimeSpan CycleDuration = TimeSpan.FromMinutes(10);
    private static readonly DateTimeOffset Epoch = DateTimeOffset.UnixEpoch;

    public long CurrentCycleIndex => CycleIndexAt(time.GetUtcNow());

    public long CycleIndexAt(DateTimeOffset serverTime)
    {
        var ticks = (serverTime.ToUniversalTime() - Epoch).Ticks;
        var cycle = Math.DivRem(ticks, CycleDuration.Ticks, out var remainder);
        return remainder < 0 ? cycle - 1 : cycle;
    }

    public TraderStockSnapshot Generate(
        DateTimeOffset serverTime,
        IReadOnlyList<WeaponDefinition> weapons,
        IReadOnlyList<SupplyDefinition> supplies)
    {
        var cycle = CycleIndexAt(serverTime);
        var nonStarterWeapons = weapons
            .Where(weapon => !weapon.Id.Equals("tea_kettle", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (nonStarterWeapons.Length == 0)
            throw new InvalidOperationException("Trader stock requires at least one non-starter weapon.");
        if (supplies.Count < 3)
            throw new InvalidOperationException("Trader stock requires at least three supply types.");

        var weaponIndex = Mod(cycle + (uint)seed, nonStarterWeapons.Length);
        var selectedWeapon = nonStarterWeapons[weaponIndex];
        var shuffledSupplies = supplies.ToArray();
        var random = new CycleRandom(seed, cycle);
        for (var index = shuffledSupplies.Length - 1; index > 0; index--)
        {
            var swapIndex = random.Next(index + 1);
            (shuffledSupplies[index], shuffledSupplies[swapIndex]) =
                (shuffledSupplies[swapIndex], shuffledSupplies[index]);
        }

        var items = new List<TraderStockEntry>
        {
            new("tea_kettle", "Tea Kettle", "Weapon", null, 0),
            new(selectedWeapon.Id, selectedWeapon.Name, "Weapon", 1, selectedWeapon.Value)
        };
        items.AddRange(shuffledSupplies.Take(3).Select(supply =>
            new TraderStockEntry(supply.Id, supply.Name, supply.Category, 2 + random.Next(3), supply.Value)));
        return new TraderStockSnapshot(cycle, items);
    }

    private static int Mod(long value, int divisor) =>
        (int)((value % divisor + divisor) % divisor);

    private struct CycleRandom
    {
        public CycleRandom(int seed, long cycle)
        {
            var cycleBits = unchecked((ulong)cycle);
            var mixed = unchecked((uint)seed
                ^ (uint)cycleBits
                ^ (uint)(cycleBits >> 32)
                ^ 0x9E3779B9u);
            Value = mixed == 0 ? 0xA341316Cu : mixed;
        }

        private uint Value { get; set; }

        public int Next(int exclusiveMaximum)
        {
            Value ^= Value << 13;
            Value ^= Value >> 17;
            Value ^= Value << 5;
            return (int)(Value % (uint)exclusiveMaximum);
        }
    }
}
