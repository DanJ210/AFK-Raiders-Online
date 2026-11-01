namespace AFKRaiders.Server.Domain;

public enum ShieldTier
{
    None = 0,
    Light = 1,
    Medium = 2,
    Heavy = 3
}

/// <summary>
/// A standing Gear Weirdos shield offer. Names are AFK-original placeholders (non-canon, pending approval);
/// charge and mitigation are deliberate parody divergences from the source game's values.
/// </summary>
public sealed record ShieldDefinition(
    string Id,
    string Tier,
    string Name,
    string Color,
    int MaxCharge,
    int MitigationPermille,
    int Value,
    string Flavor);

/// <summary>Raider-owned shield state persisted between raids. Charge is per raid and is not stored here.</summary>
public readonly record struct ShieldLoadout(ShieldTier Tier, int DurabilityCenti)
{
    public bool HasShield => Tier != ShieldTier.None;
    public bool IsBroken => HasShield && DurabilityCenti <= 0;
    public static ShieldLoadout Unshielded => new(ShieldTier.None, 0);
}

/// <summary>Read-only home projection of the Raider-owned shield.</summary>
public sealed record OwnedShieldView(
    string Tier,
    string? CatalogId,
    string? Name,
    string? Color,
    double Durability,
    int MaxDurability,
    int MaxCharge,
    double MitigationPercent);

/// <summary>
/// BAL-1S experimental shield constants and integer math. BAL-4 owns tuning; nothing here is validated balance.
/// </summary>
public static class ShieldRules
{
    public const int MaxDurabilityCenti = 10_000;
    public const int MaxDurabilityPoints = 100;
    /// <summary>At or below every trait's repair threshold; used for the "started weak" metric.</summary>
    public const int WeakDurabilityCenti = 2_500;
    public const int WearPointsPerFullBar = 20;
    public const int MinimumRepairBlockPoints = 10;

    public static ShieldLoadout InitialLoadout => new(ShieldTier.Light, MaxDurabilityCenti);

    public static int RechargeTrigger(int maxCharge) => maxCharge * 3 / 8;

    /// <summary>Danger multiplier in halves: Low x1, Medium x1.5, High x2.</summary>
    public static int DangerHalves(string danger) => danger switch
    {
        "Low" => 2,
        "Medium" => 3,
        "High" => 4,
        _ => throw new ArgumentException($"Unknown danger '{danger}' for shield wear.", nameof(danger))
    };

    /// <summary>Durability lost (centi-points) when a hit spends charge: chargeSpent / maxCharge * 20 points * danger.</summary>
    public static int WearCenti(int chargeSpent, int maxCharge, string danger)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chargeSpent);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCharge);
        return checked((int)((long)chargeSpent * WearPointsPerFullBar * 100 * DangerHalves(danger) / (maxCharge * 2L)));
    }

    /// <summary>
    /// Full-hit mitigation with a per-raid permille remainder carry, so small hits realize the nominal rate on average.
    /// Returns HP prevented; never more than the hit.
    /// </summary>
    public static int Mitigate(int damage, int permille, ref int carryPermille)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);
        if (permille is < 0 or >= 1000) throw new ArgumentOutOfRangeException(nameof(permille));
        var accumulated = checked(carryPermille + damage * permille);
        carryPermille = accumulated % 1000;
        return accumulated / 1000;
    }

    /// <summary>Cost in coins to restore <paramref name="points"/> whole points: ceil(price * (0.25s + 0.35s^2)), s = points/100.</summary>
    public static int RepairCost(int price, int points)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(price);
        if (points is < 0 or > MaxDurabilityPoints) throw new ArgumentOutOfRangeException(nameof(points));
        var scaled = checked((long)price * (2_500L * points + 35L * points * points));
        return checked((int)((scaled + 999_999) / 1_000_000));
    }

    /// <summary>Missing durability rounded up to whole points.</summary>
    public static int MissingPoints(int durabilityCenti) =>
        (MaxDurabilityCenti - Math.Clamp(durabilityCenti, 0, MaxDurabilityCenti) + 99) / 100;

    public static int ApplyRepair(int durabilityCenti, int points) =>
        Math.Min(MaxDurabilityCenti, durabilityCenti + points * 100);

    public static OwnedShieldView Project(ShieldLoadout shield)
    {
        if (!shield.HasShield)
            return new OwnedShieldView(nameof(ShieldTier.None), null, null, null, 0, MaxDurabilityPoints, 0, 0);
        var definition = ShopCatalog.GetShield(shield.Tier);
        return new OwnedShieldView(shield.Tier.ToString(), definition.Id, definition.Name, definition.Color,
            shield.DurabilityCenti / 100.0, MaxDurabilityPoints, definition.MaxCharge, definition.MitigationPermille / 10.0);
    }
}
