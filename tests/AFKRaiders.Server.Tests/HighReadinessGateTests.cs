using System.Text.Json;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

/// <summary>
/// BAL-4 Arm 3: the first-24h new-Raider High readiness gate. The age clock is the Raider's
/// deterministic server-owned enrollment time. A young Raider deploys into High only with a usable
/// equipped weapon, at least one carried Medical supply, and an unbroken shield; blocked Raiders
/// fall back to the autonomous policy's next-best danger and lose nothing.
/// </summary>
public sealed class HighReadinessGateTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
    private static readonly string[] Slots = ["raider-one", "raider-two"];

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, true, true)]
    public void ReadinessRequiresWeaponMedicalAndUnbrokenShieldTogether(
        bool usableWeapon, bool carriesMedical, bool unbrokenShield, bool expected)
    {
        Assert.Equal(expected,
            RaidCoordinator.IsHighReadyForTesting(usableWeapon, carriesMedical, unbrokenShield));
    }

    private static (RaiderTelemetry[] Telemetry, int Settled) RunCohort(
        int seed,
        int hours,
        int pollSeconds,
        BalanceExperimentArm arm = BalanceExperimentArm.Arm3Baseline)
    {
        var clock = new Clock();
        var nextSeed = seed;
        var nextId = 0;
        var coordinator = new RaidCoordinator(
            clock, narrator: null, catalog: null, narrativeGenerator: null, fastPacing: false,
            nextSeed: () => nextSeed++, traderSeed: seed,
            nextId: () => $"id{seed}x{nextId++:D4}", balanceExperimentArm: arm);
        var enrolled = Slots.Select(coordinator.EnrollHandler).ToArray();
        for (var t = 0; t <= hours * 3600; t += 5)
        {
            clock.Now = Start.AddSeconds(t);
            coordinator.AdvanceRaids();
            if (pollSeconds != int.MaxValue && t % pollSeconds == 0)
                foreach (var handler in enrolled)
                    coordinator.GetHandlerProjection(handler.AccessToken);
        }
        var telemetry = enrolled.Select((handler, index) =>
            new RaiderTelemetry(seed, Slots[index], coordinator.GetSettlementTelemetry(handler.AccessToken))).ToArray();
        return (telemetry, telemetry.Sum(raider => raider.Run.Records.Count));
    }

    [Fact]
    public void YoungRaidersNeverDeployToHighWithoutTheReadinessKit()
    {
        var sawYoungHigh = false;
        foreach (var seed in new[] { 1, 2, 3, 5, 8 })
        {
            var (telemetry, settled) = RunCohort(seed, hours: 30, pollSeconds: int.MaxValue);
            Assert.True(settled > 0, $"Seed {seed} settled no raids.");
            foreach (var raider in telemetry)
            {
                var enrolledAt = raider.Run.EnrolledAt
                    ?? throw new InvalidOperationException("Telemetry must record the enrollment age clock.");
                // Non-starvation: the gate must not keep a Raider out of every danger.
                Assert.Contains(raider.Run.Records, record =>
                    (record.SettledAt - enrolledAt).TotalHours < 24);
                foreach (var record in raider.Run.Records.Where(record => record.Sample.Danger == "High"))
                {
                    var admittedAt = record.AdmittedAt
                        ?? throw new InvalidOperationException("Settled raids must record their admission time.");
                    if ((admittedAt - enrolledAt).TotalHours >= 24) continue;
                    sawYoungHigh = true;
                    // The gate's kit at admission: unbroken shield, carried medical, usable weapon
                    // (admission itself already requires the usable equipped weapon).
                    Assert.NotEqual(nameof(ShieldTier.None), record.Sample.DeployShieldTier);
                    Assert.True(record.Sample.DeployShieldDurabilityCenti > 0,
                        $"Seed {seed} {raider.Slot}: young High deploy with broken shield.");
                    Assert.True(record.Sample.MedicalCarried >= 1,
                        $"Seed {seed} {raider.Slot}: young High deploy without medical.");
                }
                // Funnel consistency: every block was an offer; every start was an offer.
                Assert.True(raider.Run.HighGateBlocks <= raider.Run.HighWorldsOffered);
                Assert.True(raider.Run.HighAdmissionsStarted <= raider.Run.HighWorldsOffered);
                var events = raider.Run.HighAdmissionEvents
                    ?? throw new InvalidOperationException("Admission events are required for funnel checks.");
                Assert.Equal(raider.Run.HighWorldsOffered, events.Count(item => item.Kind == "Offered"));
                Assert.Equal(raider.Run.HighAdmissionsStarted, events.Count(item => item.Kind == "Started"));
                Assert.Equal(raider.Run.HighGateBlocks, events.Count(item => item.Kind == "ReadinessBlocked"));
                Assert.Equal(raider.Run.HighAgeBlocks, events.Count(item => item.Kind == "AgeBlocked"));
                Assert.True(raider.Run.Records.Count(record => record.Sample.Danger == "High")
                    <= raider.Run.HighAdmissionsStarted);
            }
        }
        Assert.True(sawYoungHigh, "The seeded cohorts must exercise at least one young High admission.");
    }

    [Theory]
    [InlineData("arm3", 7, true, false)]
    [InlineData("arm3", 7, false, true)]
    [InlineData("arm3", 8, true, false)]
    [InlineData("arm3", 8, false, true)]
    [InlineData("arm3", 23.999, true, false)]
    [InlineData("arm3", 23.999, false, true)]
    [InlineData("arm3", 24, false, false)]
    [InlineData("arm3", 25, false, false)]
    [InlineData("arm5", 7.999, true, true)]
    [InlineData("arm5", 7.999, false, true)]
    [InlineData("arm5", 8, true, false)]
    [InlineData("arm5", 8, false, true)]
    [InlineData("arm5", 23.999, true, false)]
    [InlineData("arm5", 23.999, false, true)]
    [InlineData("arm5", 24, false, false)]
    [InlineData("arm5", 25, false, false)]
    public void HighAdmissionAgeRuleHasExactBoundaries(
        string armName, double ageHours, bool ready, bool expectedBlocked)
    {
        var arm = armName switch
        {
            "arm3" => BalanceExperimentArm.Arm3Baseline,
            "arm5" => BalanceExperimentArm.Arm5,
            _ => throw new ArgumentOutOfRangeException(nameof(armName))
        };
        Assert.Equal(expectedBlocked,
            BalanceExperimentRules.BlocksHigh(arm, TimeSpan.FromHours(ageHours), ready));
    }

    [Fact]
    public void Arm5ClampsYoungHighWorldsAndStillSettlesEveryRaider()
    {
        var (telemetry, settled) = RunCohort(
            seed: 2, hours: 8, pollSeconds: int.MaxValue, arm: BalanceExperimentArm.Arm5);
        Assert.True(settled > 0);
        foreach (var raider in telemetry)
        {
            Assert.NotEmpty(raider.Run.Records);
            Assert.All(raider.Run.Records, record =>
            {
                Assert.True((record.SettledAt - Start).TotalHours < 8);
                Assert.NotEqual("High", record.Sample.Danger);
            });
            var events = raider.Run.HighAdmissionEvents
                ?? throw new InvalidOperationException("Admission events are required.");
            Assert.All(events.Where(item => item.Kind is "Offered" or "Started"), item =>
                Assert.True((item.At - raider.Run.EnrolledAt!.Value).TotalHours >= 8));
            Assert.Equal(0, raider.Run.HighAgeBlocks);
        }
    }

    [Fact]
    public void Arm5BlockedRaidersReachAnEligibleAdmissionWithoutStarvation()
    {
        var (telemetry, _) = RunCohort(
            seed: 3, hours: 30, pollSeconds: int.MaxValue, arm: BalanceExperimentArm.Arm5);
        foreach (var raider in telemetry)
        {
            var events = raider.Run.HighAdmissionEvents
                ?? throw new InvalidOperationException("Admission events are required.");
            foreach (var blocked in events.Where(item =>
                item.Kind is "AgeBlocked" or "ReadinessBlocked"))
            {
                var next = raider.Run.Records.FirstOrDefault(record => record.AdmittedAt >= blocked.At);
                Assert.NotNull(next);
                Assert.InRange((next!.AdmittedAt!.Value - blocked.At).TotalSeconds, 0, 1350);
            }
        }
    }

    [Fact]
    public void GateOutcomesAreDeterministicAndIdenticalForAbsentAndPolledHandlers()
    {
        var absent = RunCohort(seed: 3, hours: 26, pollSeconds: int.MaxValue);
        var polled = RunCohort(seed: 3, hours: 26, pollSeconds: 30);
        var repeat = RunCohort(seed: 3, hours: 26, pollSeconds: int.MaxValue);
        Assert.Equal(JsonSerializer.Serialize(absent.Telemetry), JsonSerializer.Serialize(repeat.Telemetry));
        Assert.Equal(JsonSerializer.Serialize(absent.Telemetry), JsonSerializer.Serialize(polled.Telemetry));
    }

    [Fact]
    public void Arm5OutcomesAreDeterministicAndIdenticalForAbsentAndPolledHandlers()
    {
        var absent = RunCohort(seed: 3, hours: 26, pollSeconds: int.MaxValue,
            arm: BalanceExperimentArm.Arm5);
        var polled = RunCohort(seed: 3, hours: 26, pollSeconds: 30, arm: BalanceExperimentArm.Arm5);
        var repeat = RunCohort(seed: 3, hours: 26, pollSeconds: int.MaxValue,
            arm: BalanceExperimentArm.Arm5);
        Assert.Equal(JsonSerializer.Serialize(absent.Telemetry), JsonSerializer.Serialize(repeat.Telemetry));
        Assert.Equal(JsonSerializer.Serialize(absent.Telemetry), JsonSerializer.Serialize(polled.Telemetry));
    }
}
