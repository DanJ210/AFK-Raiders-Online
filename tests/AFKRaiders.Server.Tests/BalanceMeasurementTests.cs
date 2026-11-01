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
        IReadOnlyDictionary<string, (int Extracted, int KnockedOut)> ObservedSettlements);

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
        return new CohortResult(second, observed);
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
        }
    }

    [Fact]
    public void AbsentHandlersAccumulateTheSameStatsAsPolledHandlers()
    {
        var polled = RunRecurringCohort(seed: 11, hours: 2, pollSeconds: 5);
        var absent = RunRecurringCohort(seed: 11, hours: 2, pollSeconds: int.MaxValue);
        Assert.Equal(
            System.Text.Json.JsonSerializer.Serialize(polled.Stats),
            System.Text.Json.JsonSerializer.Serialize(absent.Stats));
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
            var sample = RaidStatsSample.FromProjection(final, "one", knockedOut, weaponsFound: 0, weaponLost: knockedOut);

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
    /// Optional: AFK_BALANCE_SEEDS (default 24), AFK_BALANCE_HOURS (default 8).
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
        var all = Enumerable.Range(1, seeds)
            .AsParallel().AsOrdered()
            .SelectMany(seed => RunRecurringCohort(seed, hours, pollSeconds: int.MaxValue).Stats)
            .ToArray();
        var report = BalanceReport.Render(all, seeds, hours, RaidSimulation.RulesVersion);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, report);
        output.WriteLine(report);
    }
}

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
        var economy = raiders.Select(r => r.Economy).Aggregate(EconomyStatsView.Empty, (a, b) => new EconomyStatsView(
            a.WeaponsBought + b.WeaponsBought, a.CoinsOnWeapons + b.CoinsOnWeapons, a.Repairs + b.Repairs,
            a.CoinsOnRepairs + b.CoinsOnRepairs, a.LoanerKettles + b.LoanerKettles, a.SuppliesBought + b.SuppliesBought,
            a.CoinsOnSupplies + b.CoinsOnSupplies, a.ItemsSold + b.ItemsSold, a.CoinsFromSales + b.CoinsFromSales));

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
        sb.AppendLine("| Weapons bought | Coins on weapons | Repairs | Coins on repairs | Loaner Kettles | Supplies bought | Coins on supplies | Items sold | Coins from sales |");
        sb.AppendLine("|---:|---:|---:|---:|---:|---:|---:|---:|---:|");
        sb.AppendLine($"| {economy.WeaponsBought} | {economy.CoinsOnWeapons} | {economy.Repairs} | {economy.CoinsOnRepairs} | {economy.LoanerKettles} | {economy.SuppliesBought} | {economy.CoinsOnSupplies} | {economy.ItemsSold} | {economy.CoinsFromSales} |");
        sb.AppendLine();
        sb.AppendLine($"Weapon inflow (found + bought + loaner) vs outflow (lost on KO): {total.WeaponsFound + economy.WeaponsBought + economy.LoanerKettles} in / {total.WeaponsLost} out.");
        return sb.ToString();
    }
}
