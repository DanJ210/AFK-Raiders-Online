namespace AFKRaiders.Server.Domain;

/// <summary>
/// One settled raid's committed outcomes for a single Raider, derived from the owner's projection.
/// </summary>
public sealed record RaidStatsSample(
    string Danger,
    bool KnockedOut,
    int HpLostToRobot,
    int HpLostToHazards,
    int MedicalUsed,
    int RevivesUsed,
    int ShieldRechargersUsed,
    int MedicalCarried,
    bool RobotFought,
    string? RobotResolution,
    int RobotDamageDealt,
    bool SharedRobotFight,
    int CrossedPaths,
    int SharedSearches,
    int ContestsWon,
    int ContestsLost,
    int ItemsSecured,
    int ValueSecured,
    int ItemsLost,
    int ValueLost,
    int WeaponsFound,
    bool WeaponLost,
    string? DeployShieldTier = null,
    int DeployShieldDurabilityCenti = 0,
    int ShieldDurabilityLostCenti = 0,
    bool ShieldLost = false,
    int RobotCargoSecured = 0,
    int RobotCargoValueSecured = 0,
    int RobotCargoAllocated = 0,
    int RobotCargoValueAllocated = 0,
    int RobotWeaponsAllocated = 0,
    int RobotWeaponsSecured = 0,
    int RobotWeaponsLost = 0,
    int RobotRoundsResolved = 0,
    string? KnockOutCause = null,
    double? TimeToTerminalSeconds = null,
    int MedicalHpRestored = 0,
    int MedicalHpWasted = 0,
    bool RobotJackpotSecured = false)
{
    public const string UnratedDanger = "Unrated";

    public static RaidStatsSample FromProjection(
        RaidProjection projection,
        DateTimeOffset raidStartedAt,
        string playerId,
        bool knockedOut,
        int weaponsFound,
        bool weaponLost,
        ShieldLoadout? deployShield = null,
        int shieldDurabilityLostCenti = 0,
        bool shieldLost = false)
    {
        var hpRobot = 0;
        var hpHazard = 0;
        var medical = 0;
        var revives = 0;
        var rechargers = 0;
        var medicalHpRestored = 0;
        var medicalHpWasted = 0;
        var robotFought = false;
        var crossedPaths = 0;
        foreach (var raidEvent in projection.Events)
        {
            switch (raidEvent.Kind)
            {
                case "ShieldAbsorbed" or "UnmitigatedDamage" when raidEvent.HealthDelta is < 0:
                    if (raidEvent.ThreadId?.StartsWith("robot:", StringComparison.Ordinal) == true)
                        hpRobot -= raidEvent.HealthDelta.Value;
                    else
                        hpHazard -= raidEvent.HealthDelta.Value;
                    break;
                case "ConsumableUsed":
                    var supply = ShopCatalog.Supplies.FirstOrDefault(s => s.Name == raidEvent.ItemName)
                        ?? throw new InvalidOperationException($"Consumed supply '{raidEvent.ItemName}' is not in the catalog.");
                    if (supply.ReviveAmount > 0) revives++;
                    else if (supply.Category == "ShieldRecharger") rechargers++;
                    else
                    {
                        medical++;
                        var actualHeal = raidEvent.HealthDelta ?? 0;
                        medicalHpRestored += actualHeal;
                        medicalHpWasted += Math.Max(0, supply.HealAmount - actualHeal);
                    }
                    break;
                case "RobotCombatStarted" or "RobotParticipantJoined" when raidEvent.ParticipantIds.Contains(playerId):
                    robotFought = true;
                    break;
                case "CrossedPaths":
                    crossedPaths++;
                    break;
            }
        }

        var remainingMedical = (projection.MyFieldSupplies ?? [])
            .Where(s => s.Category == "Medical")
            .Sum(s => s.Quantity);
        var robot = projection.RobotEncounter;
        robotFought |= robot is not null;
        var mine = robot?.Damage.FirstOrDefault(d => d.PlayerId == playerId);
        var contests = projection.Encounters.Where(e => e.Resolution == "Contest").ToArray();
        var contestsWon = contests.Count(e => e.Awards.Any(a => a.RecipientId == playerId));

        var robotCargo = projection.MyExtractedLoot
            .Where(item => item.InstanceId.Contains(":defeat:", StringComparison.Ordinal))
            .ToArray();
        var robotCargoAwards = projection.Events
            .Where(raidEvent => raidEvent.Kind == "ItemAllocated"
                && raidEvent.ItemInstanceId?.Contains(":defeat:", StringComparison.Ordinal) == true
                && raidEvent.ParticipantIds.Contains(playerId))
            .ToArray();
        var robotWeaponsAllocated = robotCargoAwards.Count(raidEvent =>
            ShopCatalog.Weapons.Any(weapon => weapon.Name.Equals(raidEvent.ItemName, StringComparison.Ordinal)));
        var robotWeaponsSecured = robotCargo.Count(item =>
            ShopCatalog.TryGetWeapon(item.CatalogId, out _));
        var terminal = projection.Events.LastOrDefault(raidEvent => raidEvent.Kind is "Extracted" or "KnockedOut")
            ?? throw new InvalidOperationException($"Settled raid '{projection.RaidId}' has no terminal event.");
        return new RaidStatsSample(
            Danger: projection.Condition?.Danger ?? UnratedDanger,
            KnockedOut: knockedOut,
            HpLostToRobot: hpRobot,
            HpLostToHazards: hpHazard,
            MedicalUsed: medical,
            RevivesUsed: revives,
            ShieldRechargersUsed: rechargers,
            MedicalCarried: medical + revives + remainingMedical,
            RobotFought: robotFought,
            RobotResolution: robotFought ? robot?.Resolution ?? "Unresolved" : null,
            RobotDamageDealt: mine?.DamageDealt ?? 0,
            SharedRobotFight: robot is not null && robot.ParticipantIds.Count > 1,
            CrossedPaths: crossedPaths,
            SharedSearches: projection.Encounters.Count(e => e.Resolution == "Cooperation"),
            ContestsWon: contestsWon,
            ContestsLost: contests.Length - contestsWon,
            ItemsSecured: projection.MyExtractedLoot.Count,
            ValueSecured: projection.MyExtractedLoot.Sum(i => i.Value),
            ItemsLost: projection.MyLostHaulCount,
            ValueLost: projection.MyLostHaulValue,
            WeaponsFound: weaponsFound,
            WeaponLost: weaponLost,
            DeployShieldTier: deployShield?.Tier.ToString(),
            DeployShieldDurabilityCenti: deployShield?.DurabilityCenti ?? 0,
            ShieldDurabilityLostCenti: shieldDurabilityLostCenti,
            ShieldLost: shieldLost,
            RobotCargoSecured: robotCargo.Length,
            RobotCargoValueSecured: robotCargo.Sum(item => item.Value),
            RobotCargoAllocated: robotCargoAwards.Length,
            RobotCargoValueAllocated: robotCargoAwards.Sum(raidEvent => raidEvent.ItemValue ?? 0),
            RobotWeaponsAllocated: robotWeaponsAllocated,
            RobotWeaponsSecured: robotWeaponsSecured,
            RobotWeaponsLost: robotWeaponsAllocated - robotWeaponsSecured,
            RobotRoundsResolved: projection.Robot.RoundsResolved,
            KnockOutCause: knockedOut ? terminal.Reason : null,
            TimeToTerminalSeconds: (terminal.At - raidStartedAt).TotalSeconds,
            MedicalHpRestored: medicalHpRestored,
            MedicalHpWasted: medicalHpWasted,
            RobotJackpotSecured: robotCargo.Any(item => item.Value == 900));
    }
}

public sealed record DangerStatsView(
    string Danger,
    int Raids,
    int Extractions,
    int KnockOuts,
    int HpLostToRobot,
    int HpLostToHazards,
    int MedicalUsed,
    int RevivesUsed,
    int ShieldRechargersUsed,
    int MedicalCarried,
    int RobotFights,
    int RobotsDefeated,
    int RobotsEscaped,
    int RobotFightsUnresolved,
    int RobotDamageDealt,
    int SharedRobotFights,
    int CrossedPaths,
    int SharedSearches,
    int ContestsWon,
    int ContestsLost,
    int ItemsSecured,
    int ValueSecured,
    int ItemsLost,
    int ValueLost,
    int WeaponsFound,
    int WeaponsLost,
    int ShieldDurabilityLostCenti = 0,
    int UnshieldedRaids = 0,
    int LightShieldRaids = 0,
    int MediumShieldRaids = 0,
    int HeavyShieldRaids = 0,
    int WeakShieldRaids = 0,
    int BrokenShieldRaids = 0,
    int ShieldsLost = 0)
{
    public static DangerStatsView Empty(string danger) =>
        new(danger, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public DangerStatsView Add(RaidStatsSample s) => this with
    {
        Raids = Raids + 1,
        Extractions = Extractions + (s.KnockedOut ? 0 : 1),
        KnockOuts = KnockOuts + (s.KnockedOut ? 1 : 0),
        HpLostToRobot = HpLostToRobot + s.HpLostToRobot,
        HpLostToHazards = HpLostToHazards + s.HpLostToHazards,
        MedicalUsed = MedicalUsed + s.MedicalUsed,
        RevivesUsed = RevivesUsed + s.RevivesUsed,
        ShieldRechargersUsed = ShieldRechargersUsed + s.ShieldRechargersUsed,
        MedicalCarried = MedicalCarried + s.MedicalCarried,
        RobotFights = RobotFights + (s.RobotFought ? 1 : 0),
        RobotsDefeated = RobotsDefeated + (s.RobotResolution == "Defeated" ? 1 : 0),
        RobotsEscaped = RobotsEscaped + (s.RobotResolution == "Escaped" ? 1 : 0),
        RobotFightsUnresolved = RobotFightsUnresolved + (s.RobotResolution == "Unresolved" ? 1 : 0),
        RobotDamageDealt = RobotDamageDealt + s.RobotDamageDealt,
        SharedRobotFights = SharedRobotFights + (s.SharedRobotFight ? 1 : 0),
        CrossedPaths = CrossedPaths + s.CrossedPaths,
        SharedSearches = SharedSearches + s.SharedSearches,
        ContestsWon = ContestsWon + s.ContestsWon,
        ContestsLost = ContestsLost + s.ContestsLost,
        ItemsSecured = ItemsSecured + s.ItemsSecured,
        ValueSecured = ValueSecured + s.ValueSecured,
        ItemsLost = ItemsLost + s.ItemsLost,
        ValueLost = ValueLost + s.ValueLost,
        WeaponsFound = WeaponsFound + s.WeaponsFound,
        WeaponsLost = WeaponsLost + (s.WeaponLost ? 1 : 0),
        ShieldDurabilityLostCenti = ShieldDurabilityLostCenti + s.ShieldDurabilityLostCenti,
        UnshieldedRaids = UnshieldedRaids + (s.DeployShieldTier == nameof(ShieldTier.None) ? 1 : 0),
        LightShieldRaids = LightShieldRaids + (s.DeployShieldTier == nameof(ShieldTier.Light) ? 1 : 0),
        MediumShieldRaids = MediumShieldRaids + (s.DeployShieldTier == nameof(ShieldTier.Medium) ? 1 : 0),
        HeavyShieldRaids = HeavyShieldRaids + (s.DeployShieldTier == nameof(ShieldTier.Heavy) ? 1 : 0),
        WeakShieldRaids = WeakShieldRaids + (IsShielded(s) && s.DeployShieldDurabilityCenti is > 0 and <= ShieldRules.WeakDurabilityCenti ? 1 : 0),
        BrokenShieldRaids = BrokenShieldRaids + (IsShielded(s) && s.DeployShieldDurabilityCenti <= 0 ? 1 : 0),
        ShieldsLost = ShieldsLost + (s.ShieldLost ? 1 : 0)
    };

    private static bool IsShielded(RaidStatsSample s) =>
        s.DeployShieldTier is not null && s.DeployShieldTier != nameof(ShieldTier.None);

    public DangerStatsView Merge(DangerStatsView o) => this with
    {
        Raids = Raids + o.Raids,
        Extractions = Extractions + o.Extractions,
        KnockOuts = KnockOuts + o.KnockOuts,
        HpLostToRobot = HpLostToRobot + o.HpLostToRobot,
        HpLostToHazards = HpLostToHazards + o.HpLostToHazards,
        MedicalUsed = MedicalUsed + o.MedicalUsed,
        RevivesUsed = RevivesUsed + o.RevivesUsed,
        ShieldRechargersUsed = ShieldRechargersUsed + o.ShieldRechargersUsed,
        MedicalCarried = MedicalCarried + o.MedicalCarried,
        RobotFights = RobotFights + o.RobotFights,
        RobotsDefeated = RobotsDefeated + o.RobotsDefeated,
        RobotsEscaped = RobotsEscaped + o.RobotsEscaped,
        RobotFightsUnresolved = RobotFightsUnresolved + o.RobotFightsUnresolved,
        RobotDamageDealt = RobotDamageDealt + o.RobotDamageDealt,
        SharedRobotFights = SharedRobotFights + o.SharedRobotFights,
        CrossedPaths = CrossedPaths + o.CrossedPaths,
        SharedSearches = SharedSearches + o.SharedSearches,
        ContestsWon = ContestsWon + o.ContestsWon,
        ContestsLost = ContestsLost + o.ContestsLost,
        ItemsSecured = ItemsSecured + o.ItemsSecured,
        ValueSecured = ValueSecured + o.ValueSecured,
        ItemsLost = ItemsLost + o.ItemsLost,
        ValueLost = ValueLost + o.ValueLost,
        WeaponsFound = WeaponsFound + o.WeaponsFound,
        WeaponsLost = WeaponsLost + o.WeaponsLost,
        ShieldDurabilityLostCenti = ShieldDurabilityLostCenti + o.ShieldDurabilityLostCenti,
        UnshieldedRaids = UnshieldedRaids + o.UnshieldedRaids,
        LightShieldRaids = LightShieldRaids + o.LightShieldRaids,
        MediumShieldRaids = MediumShieldRaids + o.MediumShieldRaids,
        HeavyShieldRaids = HeavyShieldRaids + o.HeavyShieldRaids,
        WeakShieldRaids = WeakShieldRaids + o.WeakShieldRaids,
        BrokenShieldRaids = BrokenShieldRaids + o.BrokenShieldRaids,
        ShieldsLost = ShieldsLost + o.ShieldsLost
    };
}

public sealed record EconomyStatsView(
    int WeaponsBought,
    int CoinsOnWeapons,
    int Repairs,
    int CoinsOnRepairs,
    int LoanerKettles,
    int SuppliesBought,
    int CoinsOnSupplies,
    int ItemsSold,
    int CoinsFromSales,
    int LightShieldsBought = 0,
    int MediumShieldsBought = 0,
    int HeavyShieldsBought = 0,
    int EmergencyShieldsBought = 0,
    int ShieldUpgrades = 0,
    int CoinsOnShields = 0,
    int ShieldRepairs = 0,
    int CoinsOnShieldRepairs = 0,
    int ShieldPointsRepaired = 0,
    int UnshieldedDeployDecisions = 0,
    int WeaponInstancesLiquidated = 0,
    int CoinsFromWeaponLiquidations = 0)
{
    public static readonly EconomyStatsView Empty = new(0, 0, 0, 0, 0, 0, 0, 0, 0);

    public EconomyStatsView Merge(EconomyStatsView o) => new(
        WeaponsBought + o.WeaponsBought, CoinsOnWeapons + o.CoinsOnWeapons, Repairs + o.Repairs,
        CoinsOnRepairs + o.CoinsOnRepairs, LoanerKettles + o.LoanerKettles, SuppliesBought + o.SuppliesBought,
        CoinsOnSupplies + o.CoinsOnSupplies, ItemsSold + o.ItemsSold, CoinsFromSales + o.CoinsFromSales,
        LightShieldsBought + o.LightShieldsBought, MediumShieldsBought + o.MediumShieldsBought,
        HeavyShieldsBought + o.HeavyShieldsBought, EmergencyShieldsBought + o.EmergencyShieldsBought,
        ShieldUpgrades + o.ShieldUpgrades, CoinsOnShields + o.CoinsOnShields, ShieldRepairs + o.ShieldRepairs,
        CoinsOnShieldRepairs + o.CoinsOnShieldRepairs, ShieldPointsRepaired + o.ShieldPointsRepaired,
        UnshieldedDeployDecisions + o.UnshieldedDeployDecisions,
        WeaponInstancesLiquidated + o.WeaponInstancesLiquidated,
        CoinsFromWeaponLiquidations + o.CoinsFromWeaponLiquidations);
}

/// <summary>
/// Session-scope statistics for one Raider. Counts committed settlements and economy commits once.
/// Held in server memory; cleared on server restart until persistence is chosen.
/// </summary>
public sealed record RaiderStatsView(
    string Scope,
    DateTimeOffset? Since,
    DangerStatsView Total,
    IReadOnlyList<DangerStatsView> ByDanger,
    EconomyStatsView Economy,
    ShieldStreakView? ShieldStreaks = null);

/// <summary>Consecutive settled raids deployed without any shield.</summary>
public sealed record ShieldStreakView(int Current, int Longest, int Streaks);

/// <summary>
/// BAL-2 measurement telemetry: one committed settlement with its true terminal-event timestamp and
/// post-settlement owner snapshots. Recorded once per settled raid alongside the aggregate statistics;
/// consecutive <see cref="AssetValueAfter"/> values attribute inter-raid preparation to the following raid.
/// </summary>
public sealed record SettledRaidRecord(
    string RaidId,
    DateTimeOffset SettledAt,
    string PrimaryTrait,
    RaidStatsSample Sample,
    int CoinsAfter,
    int AssetValueAfter,
    int StashValueAfter,
    int SuppliesValueAfter,
    int OwnedWeaponCatalogValue,
    int OwnedWeaponDurabilityCenti,
    int OwnedShieldDurabilityCenti,
    int UsableWeaponsOwned,
    int BrokenWeaponsOwned,
    int OwnedWeaponsAfter,
    string OwnedShieldTierAfter,
    string DeployedWeaponCatalogId,
    int WeaponsBoughtCumulative,
    int LoanerKettlesCumulative,
    int WeaponsLostCumulative,
    EconomyStatsView EconomyAfter);

public sealed record RaiderRunTelemetry(
    IReadOnlyList<SettledRaidRecord> Records,
    int InitialAssetValue,
    int InitialWeaponCount,
    int FinalAssetValue,
    int FinalWeaponCount,
    int FinalCoins,
    string FinalShieldTier);

public sealed class RaiderStatistics
{
    public const string SessionScope = "ServerSession";
    private static readonly string[] DangerOrder = ["Low", "Medium", "High"];
    private readonly Dictionary<string, DangerStatsView> byDanger = new(StringComparer.Ordinal);
    private readonly List<SettledRaidRecord> settledRaids = [];

    /// <summary>Measurement-only per-raid telemetry in settlement order; not part of the projected view.</summary>
    public IReadOnlyList<SettledRaidRecord> SettledRaids => settledRaids;

    /// <summary>Asset value snapshot at enrollment, before any raid's admission preparation.</summary>
    public int InitialAssetValue { get; private set; }

    /// <summary>Owned weapon stock at enrollment, before any raid's admission preparation.</summary>
    public int InitialWeaponCount { get; private set; }

    public void RecordInitialAssets(int assetValue, int weaponCount)
    {
        InitialAssetValue = assetValue;
        InitialWeaponCount = weaponCount;
    }

    public DateTimeOffset? Since { get; private set; }
    public EconomyStatsView Economy { get; private set; } = EconomyStatsView.Empty;

    public void Start(DateTimeOffset at) => Since ??= at;

    public ShieldStreakView ShieldStreaks { get; private set; } = new(0, 0, 0);

    public void RecordRaid(RaidStatsSample sample)
    {
        byDanger[sample.Danger] = byDanger.GetValueOrDefault(sample.Danger, DangerStatsView.Empty(sample.Danger)).Add(sample);
        if (sample.DeployShieldTier is null) return;
        ShieldStreaks = sample.DeployShieldTier == nameof(ShieldTier.None)
            ? new(ShieldStreaks.Current + 1, Math.Max(ShieldStreaks.Longest, ShieldStreaks.Current + 1),
                ShieldStreaks.Streaks + (ShieldStreaks.Current == 0 ? 1 : 0))
            : ShieldStreaks with { Current = 0 };
    }

    /// <summary>Appends measurement telemetry for the settlement just recorded via <see cref="RecordRaid"/>.</summary>
    public void RecordSettlementTelemetry(SettledRaidRecord record) => settledRaids.Add(record);

    public void RecordEconomy(Func<EconomyStatsView, EconomyStatsView> change) => Economy = change(Economy);

    public RaiderStatsView Project()
    {
        var ordered = byDanger.Values
            .OrderBy(d => Array.IndexOf(DangerOrder, d.Danger) is var i && i >= 0 ? i : DangerOrder.Length)
            .ThenBy(d => d.Danger, StringComparer.Ordinal)
            .ToArray();
        var total = ordered.Aggregate(DangerStatsView.Empty("All"), (sum, d) => sum.Merge(d));
        return new RaiderStatsView(SessionScope, Since, total, ordered, Economy, ShieldStreaks);
    }
}
