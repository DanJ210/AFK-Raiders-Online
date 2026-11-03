using System.Globalization;
using System.Text;
using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

/// <summary>
/// BAL-0: committed-event Raider statistics and the opt-in per-danger balance report.
/// </summary>
public sealed class BalanceMeasurementTests(ITestOutputHelper output)
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
    private static readonly string[] Slots = ["raider-one", "raider-two"];
    public const string ReportPathVariable = "AFK_BALANCE_REPORT_PATH";

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed record CohortResult(
        IReadOnlyList<RaiderStatsView> Stats,
        IReadOnlyDictionary<string, (int Extracted, int KnockedOut)> ObservedSettlements,
        IReadOnlyList<RaiderTelemetry> Telemetry);

    private static CohortResult RunRecurringCohort(int seed, int hours, int pollSeconds)
    {
        var clock = new Clock();
        var nextSeed = seed;
        var nextId = 0;
        // Pinned trader stock and IDs: player IDs are the deterministic tie-break for shared trader stock.
        var coordinator = new RaidCoordinator(clock, nextSeed: () => nextSeed++, traderSeed: seed,
            nextId: () => $"id{seed}x{nextId++:D4}");
        var enrolled = Slots.Select(coordinator.EnrollHandler).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var observed = enrolled.ToDictionary(h => h.Projection.Slot, _ => (Extracted: 0, KnockedOut: 0));
        for (var t = 0; t <= hours * 3600; t += 5)
        {
            clock.Now = Start.AddSeconds(t);
            coordinator.AdvanceRaids();
            if (t % pollSeconds != 0) continue;
            foreach (var handler in enrolled)
            {
                var p = coordinator.GetHandlerProjection(handler.AccessToken);
                if (p.LastReturnSummary is { Payoff: { } payoff } summary && seen.Add(payoff.SettlementId))
                {
                    var current = observed[p.Slot];
                    observed[p.Slot] = summary.Outcome == "Extracted"
                        ? current with { Extracted = current.Extracted + 1 }
                        : current with { KnockedOut = current.KnockedOut + 1 };
                }
            }
        }
        // Repeated scheduling at the same instant and repeated reads must not change committed stats.
        var first = enrolled.Select(h => coordinator.GetHandlerProjection(h.AccessToken).Stats!).ToArray();
        coordinator.AdvanceRaids();
        coordinator.AdvanceRaids();
        var second = enrolled.Select(h => coordinator.GetHandlerProjection(h.AccessToken).Stats!).ToArray();
        for (var i = 0; i < first.Length; i++)
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(first[i]), System.Text.Json.JsonSerializer.Serialize(second[i]));
        var telemetry = enrolled.Select((handler, index) =>
        {
            var run = coordinator.GetSettlementTelemetry(handler.AccessToken);
            return new RaiderTelemetry(seed, Slots[index], run);
        }).ToArray();
        return new CohortResult(second, observed, telemetry);
    }

    [Fact]
    public void RaiderStatsCountEachSettlementOnceAndStayConsistent()
    {
        var result = RunRecurringCohort(seed: 7, hours: 3, pollSeconds: 30);
        var slotsInOrder = Slots;
        for (var i = 0; i < result.Stats.Count; i++)
        {
            var stats = result.Stats[i];
            var observed = result.ObservedSettlements[slotsInOrder[i]];
            Assert.Equal(RaiderStatistics.SessionScope, stats.Scope);
            Assert.Equal(Start, stats.Since);
            Assert.True(stats.Total.Raids > 0, "The cohort must settle at least one raid per Raider.");
            Assert.Equal(observed.Extracted, stats.Total.Extractions);
            Assert.Equal(observed.KnockedOut, stats.Total.KnockOuts);
            Assert.Equal(stats.Total.Raids, stats.Total.Extractions + stats.Total.KnockOuts);
            Assert.Equal(stats.Total.Raids, stats.ByDanger.Sum(d => d.Raids));
            Assert.Equal(stats.Total.HpLostToRobot + stats.Total.HpLostToHazards,
                stats.ByDanger.Sum(d => d.HpLostToRobot + d.HpLostToHazards));
            Assert.Equal(stats.Total.KnockOuts, stats.Total.WeaponsLost);
            Assert.Equal(stats.Total.RobotFights,
                stats.Total.RobotsDefeated + stats.Total.RobotsEscaped + stats.Total.RobotFightsUnresolved);
            Assert.True(stats.Total.MedicalCarried >= stats.Total.MedicalUsed + stats.Total.RevivesUsed);
            Assert.True(stats.Total.ContestsWon + stats.Total.ContestsLost <= stats.Total.CrossedPaths);
            Assert.All(stats.ByDanger, d => Assert.Contains(d.Danger, new[] { "Low", "Medium", "High" }));
            Assert.Equal(stats.Total.Raids, stats.Total.UnshieldedRaids + stats.Total.LightShieldRaids
                + stats.Total.MediumShieldRaids + stats.Total.HeavyShieldRaids);
            Assert.InRange(stats.Total.ShieldsLost, 0, stats.Total.KnockOuts);
            Assert.InRange(stats.Total.BrokenShieldRaids, 0, stats.Total.Raids - stats.Total.UnshieldedRaids);
            Assert.Equal(stats.Economy.UnshieldedDeployDecisions, stats.Total.UnshieldedRaids);
        }
    }

    [Fact]
    public void AbsentHandlersAccumulateTheSameStatsAsPolledHandlers()
    {
        var polled = RunRecurringCohort(seed: 11, hours: 2, pollSeconds: 5);
        var absent = RunRecurringCohort(seed: 11, hours: 2, pollSeconds: int.MaxValue);
        Assert.Contains(polled.Stats, s => s.Total.ShieldDurabilityLostCenti > 0);

        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(polled.Stats),
            System.Text.Json.JsonSerializer.Serialize(absent.Stats));
        // Per-raid telemetry (timestamps, snapshots and robot cargo) must also be identical.
        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(polled.Telemetry),
            System.Text.Json.JsonSerializer.Serialize(absent.Telemetry));
    }

    [Fact]
    public void SampleSeparatesDamageSourcesAndCountsCarriedMedical()
    {
        StagedSupplyItem[] supplies =
        [
            new("bandage_white", "White Bandage", "Medical", 2),
            new("bandage_green", "Green Bandage", "Medical", 1),
            new("fizz_cell", "Fizz Cell", "ShieldRecharger", 1)
        ];
        var sawRobotDamage = false;
        var sawHazardDamage = false;
        foreach (var seed in Enumerable.Range(1, 24))
        {
            var sim = new RaidSimulation("bal0", Start, seed, danger: "High");
            sim.AddAutonomousRaider("one", "raider-one", Start, supplies);
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5) sim.Advance(Start.AddSeconds(t));
            var final = sim.Project("one", Start.AddSeconds(1500));
            var knockedOut = final.Events.Any(e => e.Kind == "KnockedOut");
            var sample = RaidStatsSample.FromProjection(final, Start, "one", knockedOut, weaponsFound: 0,
                weaponLost: knockedOut);

            var totalHpLost = final.Events
                .Where(e => e.Kind is "ShieldAbsorbed" or "UnmitigatedDamage" && e.HealthDelta < 0)
                .Sum(e => -e.HealthDelta!.Value);
            Assert.Equal("High", sample.Danger);
            Assert.Equal(totalHpLost, sample.HpLostToRobot + sample.HpLostToHazards);
            Assert.Equal(3, sample.MedicalCarried);
            Assert.InRange(sample.MedicalUsed, 0, 3);
            Assert.InRange(sample.ShieldRechargersUsed, 0, 1);
            Assert.Equal(final.MyExtractedLoot.Count, sample.ItemsSecured);
            sawRobotDamage |= sample.HpLostToRobot > 0;
            sawHazardDamage |= sample.HpLostToHazards > 0;
        }
        Assert.True(sawRobotDamage && sawHazardDamage, "The seeded cohort should exercise both damage sources.");
    }

    /// <summary>
    /// Writes reviewable current-rule measurements using the BAL-0 harness when AFK_BALANCE_REPORT_PATH is set.
    /// Optional: AFK_BALANCE_SEEDS (default 24), AFK_BALANCE_HOURS (default 8),
    /// AFK_BALANCE_RAW_PATH (writes raw per-raid settlement telemetry as JSON).
    /// </summary>
    [Fact]
    public void WriteBalanceReportWhenRequested()
    {
        var path = Environment.GetEnvironmentVariable(ReportPathVariable);
        if (string.IsNullOrWhiteSpace(path))
        {
            output.WriteLine($"Set {ReportPathVariable} to generate the current balance report.");
            return;
        }
        var seeds = int.Parse(Environment.GetEnvironmentVariable("AFK_BALANCE_SEEDS") ?? "24", CultureInfo.InvariantCulture);
        var hours = int.Parse(Environment.GetEnvironmentVariable("AFK_BALANCE_HOURS") ?? "8", CultureInfo.InvariantCulture);
        var cohorts = Enumerable.Range(1, seeds)
            .AsParallel().AsOrdered()
            .Select(seed => RunRecurringCohort(seed, hours, pollSeconds: int.MaxValue))
            .ToArray();
        var all = cohorts.SelectMany(cohort => cohort.Stats).ToArray();
        var telemetry = cohorts.SelectMany(cohort => cohort.Telemetry).ToArray();
        var starter = RunStarterLowCohort(seeds);
        var report = BalanceReport.Render(all, seeds, hours, RaidSimulation.RulesVersion)
            + BalanceReport.RenderWindows(telemetry, Start, hours)
            + starter;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, report);
        var rawPath = Environment.GetEnvironmentVariable("AFK_BALANCE_RAW_PATH");
        if (!string.IsNullOrWhiteSpace(rawPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(rawPath))!);
            File.WriteAllText(rawPath, System.Text.Json.JsonSerializer.Serialize(new
            {
                RaidSimulation.RulesVersion,
                Seeds = seeds,
                Hours = hours,
                Start,
                Telemetry = telemetry
            }));
        }
        output.WriteLine(report);
    }

    /// <summary>
    /// Fresh solo Low starter cohort: Tea Kettle plus a fresh initial Light shield, no Handler Signals.
    /// Mature Low raids are not starter evidence; this isolates the first-raid experience.
    /// </summary>
    private static string RunStarterLowCohort(int seeds)
    {
        var knockouts = 0;
        var robotDefeats = 0;
        var fights = 0;
        for (var seed = 1; seed <= seeds; seed++)
        {
            var sim = new RaidSimulation($"starter{seed}", Start, seed, danger: "Low");
            sim.AddAutonomousRaider("one", "raider-one", Start,
                equippedWeapon: ShopCatalog.GetStarterWeapon(),
                shield: new ShieldLoadout(ShieldTier.Light, ShieldRules.MaxDurabilityPoints * 100));
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5) sim.Advance(Start.AddSeconds(t));
            var final = sim.Project("one", Start.AddSeconds(1500));
            if (final.Events.Any(e => e.Kind == "KnockedOut")) knockouts++;
            if (final.RobotEncounter is not null) fights++;
            if (final.RobotEncounter?.Resolution == "Defeated") robotDefeats++;
        }
        return "\n## Fresh solo Low starter cohort (Kettle + initial Light, no Signals)\n\n"
            + $"| Seeds | KO rate | Robot fights | Robot defeats |\n|---:|---:|---:|---:|\n"
            + $"| {seeds} | {(100.0 * knockouts / seeds).ToString("F1", CultureInfo.InvariantCulture)}% | {fights} | {robotDefeats} |\n";
    }
}

/// <summary>Per-raider measurement telemetry for one cohort run.</summary>
public sealed record RaiderTelemetry(int Seed, string Slot, RaiderRunTelemetry Run);

public static class BalanceReport
{
    public static string Render(IReadOnlyList<RaiderStatsView> raiders, int seeds, int hours, string rulesVersion)
    {
        var dangers = raiders.SelectMany(r => r.ByDanger)
            .GroupBy(d => d.Danger)
            .Select(g => g.Aggregate(DangerStatsView.Empty(g.Key), (sum, d) => sum.Merge(d)))
            .OrderBy(d => Array.IndexOf(new[] { "Low", "Medium", "High" }, d.Danger))
            .ToList();
        var total = dangers.Aggregate(DangerStatsView.Empty("All"), (sum, d) => sum.Merge(d));
        dangers.Add(total);
        var economy = raiders.Select(r => r.Economy).Aggregate(EconomyStatsView.Empty, (a, b) => a.Merge(b));


        static string Pct(int n, int d) => d == 0 ? "-" : (100.0 * n / d).ToString("F1", CultureInfo.InvariantCulture) + "%";
        static string Per(int n, int d) => d == 0 ? "-" : ((double)n / d).ToString("F2", CultureInfo.InvariantCulture);

        var sb = new StringBuilder();
        sb.AppendLine("# Balance measurements (BAL-0 harness)");
        sb.AppendLine();
        sb.AppendLine("Generated by `BalanceMeasurementTests.WriteBalanceReportWhenRequested`. Do not edit by hand; regenerate after rule changes.");
        sb.AppendLine();
        sb.AppendLine($"- Rules version: `{rulesVersion}`");
        sb.AppendLine($"- Cohort: {seeds} seeds x 2 unattended Raiders x {hours} simulated hours (recurring coordinator, autonomous economy, no Handler Signal use)");
        sb.AppendLine($"- Settled raids: {total.Raids}");
        sb.AppendLine("- Targets (experimental, BAL-4): KO Low <=10%, Medium 15-25%, High 25-40%; medical used per raid Low 0-1, Medium 1-2, High 2-4.");
        sb.AppendLine("- Danger is chosen by the coordinator per world, so per-danger sample sizes differ. Small samples are noisy.");
        sb.AppendLine();
        sb.AppendLine("## Survival and healing");
        sb.AppendLine();
        sb.AppendLine("| Danger | Raids | KO rate | HP lost/raid (robot) | HP lost/raid (hazard) | Medical used/raid | Medical carried/raid | Revives/raid | Shield rechargers/raid |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var d in dangers)
            sb.AppendLine($"| {d.Danger} | {d.Raids} | {Pct(d.KnockOuts, d.Raids)} | {Per(d.HpLostToRobot, d.Raids)} | {Per(d.HpLostToHazards, d.Raids)} | {Per(d.MedicalUsed, d.Raids)} | {Per(d.MedicalCarried, d.Raids)} | {Per(d.RevivesUsed, d.Raids)} | {Per(d.ShieldRechargersUsed, d.Raids)} |");
        sb.AppendLine();
        sb.AppendLine("## Robots and other Raiders");
        sb.AppendLine();
        sb.AppendLine("| Danger | Robot fights | Defeated | Escaped | Unresolved at settlement | Shared robot fights | Robot dmg dealt/fight | Crossed paths | Shared searches | Contests won | Contests lost |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var d in dangers)
            sb.AppendLine($"| {d.Danger} | {d.RobotFights} | {d.RobotsDefeated} | {d.RobotsEscaped} | {d.RobotFightsUnresolved} | {d.SharedRobotFights} | {Per(d.RobotDamageDealt, d.RobotFights)} | {d.CrossedPaths} | {d.SharedSearches} | {d.ContestsWon} | {d.ContestsLost} |");
        sb.AppendLine();
        sb.AppendLine("## Loot and weapons");
        sb.AppendLine();
        sb.AppendLine("| Danger | Items secured/raid | Value secured/raid | Items lost/raid | Value lost/raid | Weapons found | Weapons lost |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var d in dangers)
            sb.AppendLine($"| {d.Danger} | {Per(d.ItemsSecured, d.Raids)} | {Per(d.ValueSecured, d.Raids)} | {Per(d.ItemsLost, d.Raids)} | {Per(d.ValueLost, d.Raids)} | {d.WeaponsFound} | {d.WeaponsLost} |");
        sb.AppendLine();
        sb.AppendLine("## Autonomous economy (all dangers)");
        sb.AppendLine();
        sb.AppendLine("| Weapons bought | Coins on weapons | Repairs | Coins on repairs | Loaner Kettles | Supplies bought | Coins on supplies | Items sold | Coins from sales | Weapons liquidated | Coins from weapon liquidation |");
        sb.AppendLine("|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        sb.AppendLine($"| {economy.WeaponsBought} | {economy.CoinsOnWeapons} | {economy.Repairs} | {economy.CoinsOnRepairs} | {economy.LoanerKettles} | {economy.SuppliesBought} | {economy.CoinsOnSupplies} | {economy.ItemsSold} | {economy.CoinsFromSales} | {economy.WeaponInstancesLiquidated} | {economy.CoinsFromWeaponLiquidations} |");
        sb.AppendLine();
        sb.AppendLine($"Weapon flow (found + bought + loaner) vs outflow (KO loss + cap liquidation): {total.WeaponsFound + economy.WeaponsBought + economy.LoanerKettles} in / {total.WeaponsLost} KO losses / {economy.WeaponInstancesLiquidated} cap liquidations.");
        sb.AppendLine();
        sb.AppendLine("## Shields (BAL-1S, experimental)");
        sb.AppendLine();
        sb.AppendLine("| Danger | Raids | Deployed none | Light | Medium | Heavy | Started weak (<=25) | Started broken | Durability lost/raid | Shields lost on KO |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var d in dangers)
            sb.AppendLine($"| {d.Danger} | {d.Raids} | {Pct(d.UnshieldedRaids, d.Raids)} | {Pct(d.LightShieldRaids, d.Raids)} | {Pct(d.MediumShieldRaids, d.Raids)} | {Pct(d.HeavyShieldRaids, d.Raids)} | {Pct(d.WeakShieldRaids, d.Raids)} | {Pct(d.BrokenShieldRaids, d.Raids)} | {(d.Raids == 0 ? "-" : (d.ShieldDurabilityLostCenti / 100.0 / d.Raids).ToString("F2", CultureInfo.InvariantCulture))} | {d.ShieldsLost} |");
        sb.AppendLine();
        var shieldSpend = economy.CoinsOnShields + economy.CoinsOnShieldRepairs;
        var streaks = raiders.Select(r => r.ShieldStreaks ?? new ShieldStreakView(0, 0, 0)).ToArray();
        var streakCount = streaks.Sum(s => s.Streaks);
        sb.AppendLine("| Light / Medium / Heavy bought | Emergency Lights | Upgrades | Coins on shields | Shield repairs (points) | Coins on shield repairs | Shield spend/settled raid | Unshielded deploy decisions | Unshielded streaks (mean / max) |");
        sb.AppendLine("|---|---:|---:|---:|---|---:|---:|---:|---|");
        sb.AppendLine($"| {economy.LightShieldsBought} / {economy.MediumShieldsBought} / {economy.HeavyShieldsBought} | {economy.EmergencyShieldsBought} | {economy.ShieldUpgrades} | {economy.CoinsOnShields} | {economy.ShieldRepairs} ({economy.ShieldPointsRepaired}) | {economy.CoinsOnShieldRepairs} | {Per(shieldSpend, total.Raids)} | {economy.UnshieldedDeployDecisions} | {streakCount} streaks, {(streakCount == 0 ? "-" : ((double)total.UnshieldedRaids / streakCount).ToString("F2", CultureInfo.InvariantCulture))} / {(streaks.Length == 0 ? 0 : streaks.Max(s => s.Longest))} |");
        return sb.ToString();
    }

    private static readonly (double FromHours, double ToHours)[] SettlementWindows =
        [(0, 8), (8, 24), (24, 48), (48, 96)];

    /// <summary>
    /// BAL-2 windowed telemetry over half-open settlement-time windows [0,8), [8,24), [24,48), [48,96) hours.
    /// Uses true terminal-event timestamps. Net asset change per raid diffs consecutive post-settlement
    /// snapshots, attributing inter-raid preparation to the following raid; events at exactly 96h are excluded.
    /// </summary>
    public static string RenderWindows(IReadOnlyList<RaiderTelemetry> raiders, DateTimeOffset start, int hours)
    {
        static string F(double v) => v.ToString("F2", CultureInfo.InvariantCulture);
        static string PctF(int n, int d) => d == 0 ? "-" : (100.0 * n / d).ToString("F1", CultureInfo.InvariantCulture) + "%";
        static double Percentile(IReadOnlyList<double> sorted, double p)
        {
            if (sorted.Count == 0) return 0;
            var rank = (sorted.Count - 1) * p;
            var low = (int)Math.Floor(rank);
            var high = (int)Math.Ceiling(rank);
            return sorted[low] + (sorted[high] - sorted[low]) * (rank - low);
        }

        static EconomyStatsView Difference(EconomyStatsView current, EconomyStatsView previous) => new(
            current.WeaponsBought - previous.WeaponsBought,
            current.CoinsOnWeapons - previous.CoinsOnWeapons,
            current.Repairs - previous.Repairs,
            current.CoinsOnRepairs - previous.CoinsOnRepairs,
            current.LoanerKettles - previous.LoanerKettles,
            current.SuppliesBought - previous.SuppliesBought,
            current.CoinsOnSupplies - previous.CoinsOnSupplies,
            current.ItemsSold - previous.ItemsSold,
            current.CoinsFromSales - previous.CoinsFromSales,
            current.LightShieldsBought - previous.LightShieldsBought,
            current.MediumShieldsBought - previous.MediumShieldsBought,
            current.HeavyShieldsBought - previous.HeavyShieldsBought,
            current.EmergencyShieldsBought - previous.EmergencyShieldsBought,
            current.ShieldUpgrades - previous.ShieldUpgrades,
            current.CoinsOnShields - previous.CoinsOnShields,
            current.ShieldRepairs - previous.ShieldRepairs,
            current.CoinsOnShieldRepairs - previous.CoinsOnShieldRepairs,
            current.ShieldPointsRepaired - previous.ShieldPointsRepaired,
            current.UnshieldedDeployDecisions - previous.UnshieldedDeployDecisions,
            current.WeaponInstancesLiquidated - previous.WeaponInstancesLiquidated,
            current.CoinsFromWeaponLiquidations - previous.CoinsFromWeaponLiquidations);

        var rows = new List<(int Window, int Seed, string Slot, string Danger, string Trait, RaidStatsSample Sample,
            SettledRaidRecord Record, int Net, int WeaponsBought, int LoanerKettles, int WeaponLosses,
            EconomyStatsView EconomyDelta)>();
        foreach (var raider in raiders)
        {
            if (raider.Run.InitialWeaponCount > WeaponInventoryCap.MaximumOwnedWeapons
                || raider.Run.FinalWeaponCount > WeaponInventoryCap.MaximumOwnedWeapons)
            {
                throw new InvalidOperationException(
                    $"Weapon cap exceeded for seed {raider.Seed}, {raider.Slot}: "
                    + $"initial {raider.Run.InitialWeaponCount}, final {raider.Run.FinalWeaponCount}.");
            }
            var previousAssets = raider.Run.InitialAssetValue;
            var previousWeapons = raider.Run.InitialWeaponCount;
            var previousBought = 0;
            var previousLoaners = 0;
            var previousLosses = 0;
            var previousEconomy = EconomyStatsView.Empty;
            foreach (var record in raider.Run.Records)
            {
                var atHours = (record.SettledAt - start).TotalHours;
                var window = Array.FindIndex(SettlementWindows, w => atHours >= w.FromHours && atHours < w.ToHours);
                var net = record.AssetValueAfter - previousAssets;
                var bought = record.WeaponsBoughtCumulative - previousBought;
                var loaners = record.LoanerKettlesCumulative - previousLoaners;
                var losses = record.WeaponsLostCumulative - previousLosses;
                var economyDelta = Difference(record.EconomyAfter, previousEconomy);
                var liquidated = economyDelta.WeaponInstancesLiquidated;
                var actualStockChange = record.OwnedWeaponsAfter - previousWeapons;
                var expectedStockChange = record.Sample.WeaponsFound + bought + loaners - losses - liquidated;
                if (record.OwnedWeaponsAfter > WeaponInventoryCap.MaximumOwnedWeapons)
                {
                    throw new InvalidOperationException(
                        $"Weapon cap exceeded for seed {raider.Seed}, {raider.Slot}, raid {record.RaidId}: "
                        + $"{record.OwnedWeaponsAfter} owned.");
                }
                if (actualStockChange != expectedStockChange)
                {
                    throw new InvalidOperationException(
                        $"Weapon stock did not conserve for seed {raider.Seed}, {raider.Slot}, raid {record.RaidId}: "
                        + $"actual {actualStockChange}, expected {expectedStockChange} "
                        + $"(secured {record.Sample.WeaponsFound}, bought {bought}, loaner {loaners}, "
                        + $"lost {losses}, liquidated {liquidated}).");
                }
                previousAssets = record.AssetValueAfter;
                previousWeapons = record.OwnedWeaponsAfter;
                previousBought = record.WeaponsBoughtCumulative;
                previousLoaners = record.LoanerKettlesCumulative;
                previousLosses = record.WeaponsLostCumulative;
                previousEconomy = record.EconomyAfter;
                if (window < 0) continue;
                rows.Add((window, raider.Seed, raider.Slot, record.Sample.Danger, record.PrimaryTrait, record.Sample,
                    record, net, bought, loaners, losses, economyDelta));
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("## Settlement-time windows (BAL-2 telemetry)");
        sb.AppendLine();
        sb.AppendLine("Half-open windows by true terminal-event timestamp. Robot counts are per settled participant,");
        sb.AppendLine("not distinct worlds; world counts are separately deduplicated by RaidId. Net asset change is");
        sb.AppendLine("coins + stash + owned weapons/shield at catalog value + supplies at unit cost, diffed against");
        sb.AppendLine("the previous settlement (first raid diffs against enrollment), so inter-raid preparation is");
        sb.AppendLine("attributed to the following raid. Catalog marking ignores durability; the endpoint table isolates");
        sb.AppendLine("unsettled end-of-run changes. Weapon stock conservation is checked for each owner/settlement.");
        sb.AppendLine();
        foreach (var (windowIndex, window) in SettlementWindows.Index())
        {
            if (window.FromHours >= hours) continue;
            var inWindow = rows.Where(row => row.Window == windowIndex).ToArray();
            sb.AppendLine($"### Window [{window.FromHours},{window.ToHours})h");
            sb.AppendLine();
            if (inWindow.Length == 0)
            {
                sb.AppendLine("No settlements in this window.");
                sb.AppendLine();
                continue;
            }
            sb.AppendLine("| Danger | Settlements | KO rate | KO cause count(s) | Mean time-to-KO | Robot worlds | Participant fights | Defeated / escaped / unresolved | Mean rounds/fight | Shared fights | Medical used/raid | Carried/raid | HP restored/wasted | Secured value/raid | Robot items secured/allocated | Robot value secured/raid | Robot guns secured/allocated/lost | Weapons found/lost | Bought / loaner / liquidated | Weapon liquidation coins | Shield spend | Net asset change/raid (mean / p50 / p90 / p99) |");
            sb.AppendLine("|---|---:|---:|---|---:|---:|---:|---|---:|---:|---:|---:|---|---:|---|---:|---|---|---:|---:|---|");
            foreach (var danger in new[] { "Low", "Medium", "High", "All" })
            {
                var subset = danger == "All" ? inWindow : inWindow.Where(row => row.Danger == danger).ToArray();
                if (subset.Length == 0) continue;
                var nets = subset.Select(row => (double)row.Net).OrderBy(v => v).ToArray();
                var fights = subset.Where(r => r.Sample.RobotFought).ToArray();
                var ko = subset.Where(r => r.Sample.KnockedOut).ToArray();
                var causes = string.Join(", ", ko.GroupBy(r => r.Sample.KnockOutCause ?? "Unknown")
                    .OrderByDescending(group => group.Count())
                    .ThenBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => $"{group.Key}:{group.Count()}"));
                var distinctWorlds = fights.Select(row => row.Record.RaidId).Distinct(StringComparer.Ordinal).Count();
                var shieldSpend = subset.Sum(row => row.EconomyDelta.CoinsOnShields + row.EconomyDelta.CoinsOnShieldRepairs);
                var bought = subset.Sum(row => row.WeaponsBought);
                var loaners = subset.Sum(row => row.LoanerKettles);
                var lost = subset.Sum(row => row.WeaponLosses);
                var liquidated = subset.Sum(row => row.EconomyDelta.WeaponInstancesLiquidated);
                var liquidationCoins = subset.Sum(row => row.EconomyDelta.CoinsFromWeaponLiquidations);
                sb.AppendLine($"| {danger} | {subset.Length} | {PctF(ko.Length, subset.Length)} | {causes} "
                    + $"| {(ko.Length == 0 ? "-" : F(ko.Average(row => row.Sample.TimeToTerminalSeconds ?? 0)))} "
                    + $"| {distinctWorlds} | {fights.Length} | "
                    + $"{fights.Count(r => r.Sample.RobotResolution == "Defeated")} / "
                    + $"{fights.Count(r => r.Sample.RobotResolution == "Escaped")} / "
                    + $"{fights.Count(r => r.Sample.RobotResolution == "Unresolved")} "
                    + $"| {(fights.Length == 0 ? "-" : F(fights.Average(row => (double)row.Sample.RobotRoundsResolved)))} "
                    + $"| {fights.Count(r => r.Sample.SharedRobotFight)} "
                    + $"| {F(subset.Average(r => (double)r.Sample.MedicalUsed))} "
                    + $"| {F(subset.Average(r => (double)r.Sample.MedicalCarried))} "
                    + $"| {subset.Sum(r => r.Sample.MedicalHpRestored)}/{subset.Sum(r => r.Sample.MedicalHpWasted)} "
                    + $"| {F(subset.Average(r => (double)r.Sample.ValueSecured))} "
                    + $"| {subset.Sum(r => r.Sample.RobotCargoSecured)}/{subset.Sum(r => r.Sample.RobotCargoAllocated)} "
                    + $"| {F(subset.Average(r => (double)r.Sample.RobotCargoValueSecured))} "
                    + $"| {subset.Sum(r => r.Sample.RobotWeaponsSecured)}/{subset.Sum(r => r.Sample.RobotWeaponsAllocated)}/{subset.Sum(r => r.Sample.RobotWeaponsLost)} "
                    + $"| {subset.Sum(r => r.Sample.WeaponsFound)}/{lost} "
                    + $"| {bought}/{loaners}/{liquidated} "
                    + $"| {liquidationCoins} "
                    + $"| {shieldSpend} "
                    + $"| {F(nets.Average())} / {F(Percentile(nets, 0.50))} / {F(Percentile(nets, 0.90))} / {F(Percentile(nets, 0.99))} |");
            }
            sb.AppendLine();
            sb.AppendLine("Trait-level outcomes by danger:");
            sb.AppendLine();
            sb.AppendLine("| Danger | Trait | Settlements | KO rate | Medical/raid | Secured value/raid | Weapons liquidated |");
            sb.AppendLine("|---|---|---:|---:|---:|---:|---:|");
            foreach (var group in inWindow.GroupBy(row => (row.Danger, row.Trait))
                .OrderBy(group => group.Key.Danger, StringComparer.Ordinal)
                .ThenBy(group => group.Key.Trait, StringComparer.Ordinal))
            {
                var traitKOs = group.Count(row => row.Sample.KnockedOut);
                sb.AppendLine($"| {group.Key.Danger} | {group.Key.Trait} | {group.Count()} "
                    + $"| {PctF(traitKOs, group.Count())} "
                    + $"| {F(group.Average(row => (double)row.Sample.MedicalUsed))} "
                    + $"| {F(group.Average(row => (double)row.Sample.ValueSecured))} "
                    + $"| {group.Sum(row => row.EconomyDelta.WeaponInstancesLiquidated)} |");
            }
            sb.AppendLine();
            sb.AppendLine("Robot outcomes by deployed weapon/shield (participant-fight counts):");
            sb.AppendLine();
            sb.AppendLine("| Weapon | Shield | Fights | Defeated | Escaped | Mean rounds |");
            sb.AppendLine("|---|---|---:|---:|---:|---:|");
            foreach (var group in inWindow.Where(row => row.Sample.RobotFought)
                .GroupBy(row => (row.Record.DeployedWeaponCatalogId, row.Sample.DeployShieldTier))
                .OrderBy(group => group.Key.DeployedWeaponCatalogId, StringComparer.Ordinal)
                .ThenBy(group => group.Key.DeployShieldTier, StringComparer.Ordinal))
            {
                sb.AppendLine($"| {group.Key.DeployedWeaponCatalogId} | {group.Key.DeployShieldTier} | {group.Count()} "
                    + $"| {group.Count(row => row.Sample.RobotResolution == "Defeated")} "
                    + $"| {group.Count(row => row.Sample.RobotResolution == "Escaped")} "
                    + $"| {F(group.Average(row => (double)row.Sample.RobotRoundsResolved))} |");
            }
            // High jackpot sensitivity: exclude raids that secured a 900-value prime-archive shard.
            var high = inWindow.Where(row => row.Danger == "High").ToArray();
            var highNoJackpot = high.Where(row => !row.Sample.RobotJackpotSecured).ToArray();
            if (high.Length > 0 && highNoJackpot.Length != high.Length)
                sb.AppendLine($"\nHigh excluding secured 900-value jackpots: n {highNoJackpot.Length}, "
                    + $"net mean {F(highNoJackpot.Average(row => (double)row.Net))} "
                    + $"(all High: {F(high.Average(row => (double)row.Net))}).");
            // End-of-window owned stock from each raider's last settlement in or before this window.
            var closing = raiders.Select(raider => raider.Run.Records.LastOrDefault(record =>
                    (record.SettledAt - start).TotalHours < window.ToHours))
                .Where(record => record is not null)
                .Select(record => record!)
                .ToArray();
            if (closing.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Closing stock at {window.ToHours}h (per raider with any settlement): "
                    + $"coins {F(closing.Average(r => (double)r.CoinsAfter))}, stash value {F(closing.Average(r => (double)r.StashValueAfter))}, "
                    + $"supply value {F(closing.Average(r => (double)r.SuppliesValueAfter))}, "
                    + $"usable weapons {F(closing.Average(r => (double)r.UsableWeaponsOwned))}, "
                    + $"broken weapons {F(closing.Average(r => (double)r.BrokenWeaponsOwned))}, shield none/L/M/H "
                    + $"{closing.Count(r => r.OwnedShieldTierAfter == "None")}/{closing.Count(r => r.OwnedShieldTierAfter == "Light")}/"
                    + $"{closing.Count(r => r.OwnedShieldTierAfter == "Medium")}/{closing.Count(r => r.OwnedShieldTierAfter == "Heavy")}.");
            }
            sb.AppendLine();
        }
        sb.AppendLine("### End-of-run unsettled endpoint");
        sb.AppendLine();
        sb.AppendLine("This separates costs/assets after the last settled raid, rather than silently dropping an unfinished raid or preparation.");
        sb.AppendLine();
        sb.AppendLine("| Raiders | Mean final-minus-last-settlement assets | Mean final assets | Mean final coins | Mean final weapons | Mean final shield |");
        sb.AppendLine("|---:|---:|---:|---:|---:|---|");
        var endpoint = raiders.Select(raider =>
        {
            var last = raider.Run.Records.LastOrDefault();
            return (raider.Run.FinalAssetValue - (last?.AssetValueAfter ?? raider.Run.InitialAssetValue),
                raider.Run.FinalAssetValue, raider.Run.FinalCoins, raider.Run.FinalWeaponCount, raider.Run.FinalShieldTier);
        }).ToArray();
        sb.AppendLine($"| {endpoint.Length} | {F(endpoint.Average(row => (double)row.Item1))} "
            + $"| {F(endpoint.Average(row => (double)row.FinalAssetValue))} "
            + $"| {F(endpoint.Average(row => (double)row.FinalCoins))} "
            + $"| {F(endpoint.Average(row => (double)row.FinalWeaponCount))} "
            + $"| {string.Join(", ", endpoint.GroupBy(row => row.FinalShieldTier).OrderBy(group => group.Key, StringComparer.Ordinal).Select(group => $"{group.Key}:{group.Count()}"))} |");
        sb.AppendLine();
        // Shield progression: first settlement at which a Medium/Heavy shield is owned.
        var mediumTimes = new List<double>();
        var heavyTimes = new List<double>();
        foreach (var raider in raiders)
        {
            var firstMedium = raider.Run.Records.FirstOrDefault(r => r.OwnedShieldTierAfter is "Medium" or "Heavy");
            if (firstMedium is not null) mediumTimes.Add((firstMedium.SettledAt - start).TotalHours);
            var firstHeavy = raider.Run.Records.FirstOrDefault(r => r.OwnedShieldTierAfter is "Heavy");
            if (firstHeavy is not null) heavyTimes.Add((firstHeavy.SettledAt - start).TotalHours);
        }
        mediumTimes.Sort();
        heavyTimes.Sort();
        sb.AppendLine("### Shield progression");
        sb.AppendLine();
        sb.AppendLine($"Raiders reaching Medium-or-better: {mediumTimes.Count}/{raiders.Count}"
            + (mediumTimes.Count > 0 ? $", median {F(Percentile(mediumTimes, 0.5))}h" : "")
            + $". Reaching Heavy: {heavyTimes.Count}/{raiders.Count}"
            + (heavyTimes.Count > 0 ? $", median {F(Percentile(heavyTimes, 0.5))}h" : "") + ".");
        return sb.ToString();
    }
}
