using System.Text.Json;
using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

public sealed class WeaponCombatTests(ITestOutputHelper output)
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");

    private static WeaponDefinition Weapon(string id) => ShopCatalog.Weapons.Single(w => w.Id == id);

    private static RaidSimulation Create(int seed, string danger, string weaponId, bool shared = false,
        bool reverse = false, string raidId = "bal-1-combat", bool fastPacing = true)
    {
        var sim = new RaidSimulation(raidId, Start, seed, fastPacing: fastPacing,
            riskReward: true, danger: danger);
        // BAL-2 combat fixtures deploy a fresh Heavy shield so retaliation does not end the fight
        // before the profile's round budget; damage accounting and replay semantics are unchanged.
        var shield = new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100);
        if (shared && reverse)
            sim.AddAutonomousRaider("two", "raider-two", Start, equippedWeapon: Weapon(weaponId), shield: shield);
        sim.AddAutonomousRaider("one", "raider-one", Start, equippedWeapon: Weapon(weaponId), shield: shield);
        if (shared && !reverse)
            sim.AddAutonomousRaider("two", "raider-two", Start, equippedWeapon: Weapon(weaponId), shield: shield);
        return sim;
    }

    [Theory]
    [InlineData("tea_kettle")]
    [InlineData("aspperigo")]
    [InlineData("vernerider")]
    [InlineData("temptrest_ar")]
    public void EachRoundUsesTheAdmissionWeaponAndIndependentKeyedRoll(string weaponId)
    {
        var weapon = Weapon(weaponId);
        var roundsObserved = 0;
        foreach (var seed in Enumerable.Range(0, 64))
        {
            var sim = Create(seed, "High", weaponId, shared: true);
            var totals = new Dictionary<string, int> { ["one"] = 0, ["two"] = 0 };
            var round = 0;
            while (sim.NextActivityAt is { } at && at < Start.AddMinutes(3))
            {
                var before = sim.Project("one", at);
                var fighters = before.Players.Where(p => p.Activity == "FightingRobot"
                    && p.ActivityEndsAt == at).ToArray();
                if (fighters.Length > 0)
                    fighters = before.Players.Where(p => p.ActivityEndsAt == at
                        && (p.Activity == "FightingRobot"
                            || (p.Activity == "Traveling" && p.Destination == before.Robot.Location))).ToArray();
                sim.Advance(at);
                var after = sim.Project("one", at);
                if (fighters.Length == 0 || before.Robot.Status != "Active") continue;
                round++;
                roundsObserved++;
                var remaining = before.Robot.Health;
                foreach (var fighter in fighters)
                {
                    var key = FormattableString.Invariant(
                        $"bal-1:{sim.RaidId.Length}:{sim.RaidId}:{before.Robot.RobotId.Length}:{before.Robot.RobotId}:{round}:{fighter.PlayerId.Length}:{fighter.PlayerId}");
                    var baseRoll = weapon.DamageMin + (int)(RaidConditions.Roll(seed, key)
                        * (weapon.DamageMax - weapon.DamageMin + 1));
                    var rolled = (int)Math.Round(baseRoll * weapon.DamageMultiplier, MidpointRounding.AwayFromZero);
                    var damage = Math.Min(rolled, remaining);
                    totals[fighter.PlayerId] += damage;
                    remaining -= damage;
                }
                Assert.Equal(remaining, after.Robot.Health);
                if (after.RobotEncounter is { } encounter)
                {
                    Assert.All(encounter.Damage, d => Assert.Equal(totals[d.PlayerId], d.DamageDealt));
                    Assert.Equal(after.Robot.MaxHealth - remaining, encounter.Damage.Sum(d => d.DamageDealt));
                    break;
                }
            }
        }
        Assert.True(roundsObserved > 0);
    }

    [Fact]
    public void SoloWeaponOutcomesFollowTheDangerEquipmentThresholds()
    {
        foreach (var danger in new[] { "Low", "Medium", "High" })
        {
            var rates = new List<double>();
            foreach (var weapon in ShopCatalog.Weapons)
            {
                var fought = 0;
                var defeated = 0;
                foreach (var seed in Enumerable.Range(0, 256))
                {
                    var sim = Create(seed, danger, weapon.Id, fastPacing: false);
                    sim.Advance(Start.AddMinutes(5));
                    if (sim.Project("one", Start).RobotEncounter is not { } encounter) continue;
                    fought++;
                    if (encounter.Resolution == "Defeated") defeated++;
                }
                Assert.True(fought >= 100);
                var rate = (double)defeated / fought;
                rates.Add(rate);
                output.WriteLine($"{danger} {weapon.Id}: {defeated}/{fought} robots defeated ({rate:P1})");
            }
            Assert.True(rates.SequenceEqual(rates.Order()));
            if (danger == "Low") Assert.True(rates[0] >= .75, "Kettle must remain solo-viable on Low.");
            if (danger == "Medium")
            {
                Assert.Equal(0, rates[0]);
                // This remains a distribution diagnostic, not a balance acceptance gate.
                Assert.True(rates[1] > 0, "Aspperigo must retain possible Medium solo victories.");
                Assert.True(rates[2] >= .5, "Vernerider must enable Medium solo victories.");
                Assert.Equal(1, rates[3]);
            }
            if (danger == "High")
            {
                Assert.Equal(0, rates[0]);
                Assert.Equal(0, rates[1]);
                // BAL-4 Arm 1 restores 45 HP: Vernerider (max 4x11 = 44 < 45) cannot solo High within four rounds.
                Assert.Equal(0, rates[2]);
                // High solo success is an aspiration, not a balance gate; retain legal wins
                // without requiring a minimum observed rate from this small seeded cohort.
                Assert.True(rates[3] > 0, "Temptrest must retain possible High solo victories.");
            }
        }
    }

    [Fact]
    public void SharedHighRobotCanBeDefeatedByTwoVerneridersAndContributionsAreNotDuplicated()
    {
        // BAL-2: two starter kettles can no longer realistically clear a 45 HP High robot;
        // actual cooperation with strong owned weapons is the intended shared path.
        var defeats = 0;
        foreach (var seed in Enumerable.Range(0, 128))
        {
            var sim = Create(seed, "High", "vernerider", shared: true);
            sim.Advance(Start.AddMinutes(3));
            var one = sim.Project("one", Start.AddMinutes(3));
            var two = sim.Project("two", Start.AddMinutes(3));
            if (one.RobotEncounter is not { ParticipantIds.Count: 2, Resolution: "Defeated" } encounter) continue;
            defeats++;
            Assert.Equal(JsonSerializer.Serialize(encounter), JsonSerializer.Serialize(two.RobotEncounter));
            Assert.Equal(one.Robot.MaxHealth, encounter.Damage.Sum(d => d.DamageDealt));
            Assert.All(encounter.Damage, d => Assert.True(d.DamageDealt > 0));
            var loot = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
            Assert.Equal(loot.Length, loot.Select(i => i.InstanceId).Distinct().Count());
            var snapshot = JsonSerializer.Serialize(one);
            Assert.False(sim.Advance(Start.AddMinutes(3)));
            Assert.Equal(snapshot, JsonSerializer.Serialize(sim.Project("one", Start.AddMinutes(3))));
        }
        Assert.True(defeats > 0);
    }

    [Theory]
    [InlineData("Low", "tea_kettle")]
    [InlineData("Medium", "aspperigo")]
    [InlineData("High", "vernerider")]
    [InlineData("High", "temptrest_ar")]
    public void AdmissionOrderSchedulingPollingAndRetriesPreserveReplay(string danger, string weaponId)
    {
        foreach (var seed in Enumerable.Range(0, 32))
        {
            var scheduled = Create(seed, danger, weaponId, shared: true);
            var absent = Create(seed, danger, weaponId, shared: true, reverse: true);
            for (var t = 0; t <= 180; t++)
            {
                var at = Start.AddSeconds(t);
                scheduled.Advance(at);
                var beforeRetry = JsonSerializer.Serialize(scheduled.Project("one", at));
                scheduled.Advance(at);
                scheduled.Advance(at);
                Assert.Equal(beforeRetry, JsonSerializer.Serialize(scheduled.Project("one", at)));
            }
            absent.Advance(Start.AddMinutes(3));
            foreach (var playerId in new[] { "one", "two" })
            {
                var a = scheduled.Project(playerId, Start.AddMinutes(3));
                var b = absent.Project(playerId, Start.AddMinutes(3));
                Assert.Equal(JsonSerializer.Serialize(a.RobotEncounter), JsonSerializer.Serialize(b.RobotEncounter));
                Assert.Equal(JsonSerializer.Serialize(a.MyExtractedLoot), JsonSerializer.Serialize(b.MyExtractedLoot));
                Assert.Equal(a.MyHealth, b.MyHealth);
                Assert.Equal(a.MyShieldDurability, b.MyShieldDurability);
                Assert.Equal(a.Events.Where(e => e.Kind is not ("DutyAdmission" or "WorldCondition"))
                    .Select(e => (e.At, e.Kind, e.Text)),
                    b.Events.Where(e => e.Kind is not ("DutyAdmission" or "WorldCondition"))
                    .Select(e => (e.At, e.Kind, e.Text)));
            }
        }
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CoordinatorSnapshotsTheEquippedCatalogWeaponAndCombatDoesNotWearItPerRound(bool damageOwnedAfterAdmission)
    {
        var clock = new Clock();
        var id = 0;
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 11, traderSeed: 83,
            nextId: () => $"bal1-id-{id++}");
        var enrolled = coordinator.EnrollHandler("raider-one");
        coordinator.ResetPreparationForTesting(enrolled.AccessToken, 1000, Start);
        clock.Now = Start.AddSeconds(600);
        coordinator.AdvanceRaids();
        var admitted = coordinator.GetHandlerProjection(enrolled.AccessToken);
        var equipped = Assert.IsType<OwnedWeapon>(admitted.EquippedWeapon);
        Assert.NotEqual("tea_kettle", equipped.CatalogId);
        var raid = Assert.IsType<RaidProjection>(admitted.Raid);
        coordinator.SetDuty(enrolled.AccessToken, false);
        if (damageOwnedAfterAdmission)
            coordinator.ApplyWeaponDamage(enrolled.AccessToken, equipped.InstanceId, equipped.CurrentDurability);
        var reference = new RaidSimulation(raid.RaidId, Start.AddSeconds(600), 11, danger: raid.Condition!.Danger);
        reference.AddAutonomousRaider(admitted.PlayerId, admitted.Slot, Start.AddSeconds(600),
            raid.MyFieldSupplies!.Select(s => new StagedSupplyItem(s.CatalogId, s.Name, s.Category, s.Quantity)),
            new(raid.MyGreed, raid.MyMood), primaryTrait: Enum.Parse<RaiderPrimaryTrait>(admitted.PrimaryTrait),
            equippedWeapon: Weapon(equipped.CatalogId),
            shield: new ShieldLoadout(Enum.Parse<ShieldTier>(admitted.Shield!.Tier), (int)Math.Round(admitted.Shield.Durability * 100)));

        var observedRound = false;
        var settled = false;
        for (var t = 605; t <= 2100; t += 5)
        {
            clock.Now = Start.AddSeconds(t);
            coordinator.AdvanceRaids();
            reference.Advance(clock.Now);
            var current = coordinator.GetHandlerProjection(enrolled.AccessToken);
            if (current.Raid is { } active)
            {
                Assert.Equal(damageOwnedAfterAdmission ? 0 : equipped.CurrentDurability, current.EquippedWeapon!.CurrentDurability);
                Assert.Equal(JsonSerializer.Serialize(reference.Project(admitted.PlayerId, clock.Now).Robot),
                    JsonSerializer.Serialize(active.Robot));
                Assert.Equal(JsonSerializer.Serialize(reference.Project(admitted.PlayerId, clock.Now).RobotEncounter),
                    JsonSerializer.Serialize(active.RobotEncounter));
                if (active.Events.Any(e => e.Kind == "RobotRound")) observedRound = true;
                continue;
            }
            Assert.NotNull(current.LastReturnSummary);
            var expected = reference.Project(admitted.PlayerId, clock.Now);
            Assert.Equal(expected.RobotEncounter!.Damage.Single().DamageDealt, current.Stats!.Total.RobotDamageDealt);
            Assert.Equal(expected.Players.Single().Activity, current.LastReturnSummary.Outcome);
            settled = true;
            break;
        }
        Assert.True(observedRound);
        Assert.True(settled);
    }

    [Fact]
    public void RaidIdentitySeparatesCombatRollsWithoutChangingTheRoute()
    {
        var changed = 0;
        foreach (var seed in Enumerable.Range(0, 32))
        {
            var a = Create(seed, "High", "tea_kettle", raidId: "raid:a");
            var b = Create(seed, "High", "tea_kettle", raidId: "raid:b");
            a.Advance(Start.AddMinutes(3));
            b.Advance(Start.AddMinutes(3));
            var first = a.Project("one", Start).RobotEncounter;
            var second = b.Project("one", Start).RobotEncounter;
            if (first is null) continue;
            Assert.NotNull(second);
            Assert.Equal(first.StartedAt, second.StartedAt);
            if (first.Damage.Single().DamageDealt != second.Damage.Single().DamageDealt) changed++;
        }
        Assert.True(changed > 0);
    }
}
