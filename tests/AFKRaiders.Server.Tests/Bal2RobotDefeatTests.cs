using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

/// <summary>
/// BAL-4 experimental High profile/reward tuning over the BAL-2 shared defeat settlement contract.
/// </summary>
public sealed class Bal2RobotDefeatTests(ITestOutputHelper output)
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
    private static readonly string[] Dangers = ["Low", "Medium", "High"];

    [Fact]
    public void RulesVersionAdvertisesBal4()
    {
        Assert.Equal("desperanza-bal-4-arm3-v1", RaidSimulation.RulesVersion);
        Assert.Equal("bal-4-defeat-v1", RobotDefeatRewards.RewardsVersion);
    }

    [Theory]
    [InlineData("Low", 12, 8, 3)]
    [InlineData("Medium", 22, 12, 3)]
    [InlineData("High", 45, 18, 4)]
    public void ApprovedExperimentalProfilesAreExact(string danger, int health, int damage, int rounds)
    {
        var profile = RaidConditions.Profile(danger);
        Assert.Equal(health, profile.RobotHealth);
        Assert.Equal(damage, profile.RobotDamage);
        Assert.Equal(rounds, profile.RobotRounds);
    }

    [Fact]
    public void DefeatTablesValidateAgainstTheDefaultCatalog()
    {
        var catalog = LootCatalog.Default;
        foreach (var danger in Dangers)
        {
            var cargo = RobotDefeatRewards.CargoByDanger[danger];
            Assert.Equal(100, cargo.Sum(entry => entry.Weight));
            foreach (var entry in cargo)
            {
                var item = catalog.GetItem(entry.CatalogId);
                Assert.False(item.Category.Equals("weapons", StringComparison.OrdinalIgnoreCase));
                Assert.True(entry.Weight > 0);
            }
            var weapons = RobotDefeatRewards.WeaponsByDanger[danger];
            Assert.Equal(100, weapons.Sum(entry => entry.Weight));
            foreach (var entry in weapons)
            {
                var item = catalog.GetItem(entry.CatalogId);
                Assert.Equal("weapons", item.Category, ignoreCase: true);
                Assert.True(ShopCatalog.TryGetWeapon(entry.CatalogId, out _));
                Assert.DoesNotContain("temptrest", entry.CatalogId, StringComparison.OrdinalIgnoreCase);
            }
        }
        // Approved fixed values/rarities for the selective tech intake.
        Assert.Equal((60, 3), (catalog.GetItem("arc_data_shard").Value, catalog.GetItem("arc_data_shard").Rarity));
        Assert.Equal((150, 4), (catalog.GetItem("energy_core_overcharged").Value, catalog.GetItem("energy_core_overcharged").Rarity));
        Assert.Equal((900, 5), (catalog.GetItem("arc_data_shard_legendary").Value, catalog.GetItem("arc_data_shard_legendary").Rarity));
        // The new tech definitions are defeat-bundle-only: never in the generic weighted pool.
        Assert.DoesNotContain(catalog.Pool, entry => entry.CatalogId
            is "arc_data_shard" or "energy_core_overcharged" or "arc_data_shard_legendary");
    }

    [Fact]
    public void ExpectedGrossBundleValuesMatchTheApprovedProposalArithmetic()
    {
        var catalog = LootCatalog.Default;
        foreach (var (danger, expected) in new[] { ("Low", 25.06), ("Medium", 76.805), ("High", 695.95) })
        {
            var cargoMean = RobotDefeatRewards.CargoByDanger[danger]
                .Sum(entry => catalog.GetItem(entry.CatalogId).Value * entry.Weight / 100.0);
            var weaponMean = RobotDefeatRewards.WeaponsByDanger[danger]
                .Sum(entry => catalog.GetItem(entry.CatalogId).Value * entry.Weight / 100.0);
            var weaponChance = RobotDefeatRewards.WeaponChancePermille[danger] / 1000.0;
            var bundle = cargoMean + (1 - weaponChance) * cargoMean + weaponChance * weaponMean;
            Assert.Equal(expected, bundle, 3);
        }
    }

    [Theory]
    [InlineData("Low", 0.02)]
    [InlineData("Medium", 0.06)]
    [InlineData("High", 0.12)]
    public void WeaponGateBoundariesAreExact(string danger, double chance)
    {
        Assert.True(RobotDefeatRewards.RollsWeapon(danger, 0));
        Assert.True(RobotDefeatRewards.RollsWeapon(danger, chance - 1e-9));
        Assert.False(RobotDefeatRewards.RollsWeapon(danger, chance));
        Assert.False(RobotDefeatRewards.RollsWeapon(danger, 0.999999));
    }

    [Fact]
    public void MediumWeaponSplitBoundaryIsExact()
    {
        var weapons = RobotDefeatRewards.WeaponsByDanger["Medium"];
        Assert.Equal("aspperigo", RobotDefeatRewards.Pick(weapons, 0));
        Assert.Equal("aspperigo", RobotDefeatRewards.Pick(weapons, 0.75 - 1e-9));
        Assert.Equal("vernerider", RobotDefeatRewards.Pick(weapons, 0.75));
        Assert.Equal("vernerider", RobotDefeatRewards.Pick(weapons, 0.999999));
    }

    [Fact]
    public void CargoPickBoundariesFollowTableOrder()
    {
        var cargo = RobotDefeatRewards.CargoByDanger["High"];
        // BAL-4 High weights: arc_data_shard 25, circuit_bundle_messy 25,
        // energy_core_overcharged 15, legendary 35.
        Assert.Equal("arc_data_shard", RobotDefeatRewards.Pick(cargo, 0.249999));
        Assert.Equal("circuit_bundle_messy", RobotDefeatRewards.Pick(cargo, 0.25));
        Assert.Equal("energy_core_overcharged", RobotDefeatRewards.Pick(cargo, 0.5));
        Assert.Equal("arc_data_shard_legendary", RobotDefeatRewards.Pick(cargo, 0.65));
        Assert.Equal("arc_data_shard_legendary", RobotDefeatRewards.Pick(cargo, 0.999999));
    }

    [Fact]
    public void ContributionRankingBreaksTiesByJoinSlotThenPlayerId()
    {
        var joined = Start;
        var later = Start.AddSeconds(5);
        // Damage descending first.
        Assert.Equal(["b", "a"], RobotDefeatRewards.RankContributors(
            [("a", 3, joined, "raider-one", "p1"), ("b", 9, later, "raider-two", "p2")]));
        // Tied damage: earlier JoinedAt wins.
        Assert.Equal(["b", "a"], RobotDefeatRewards.RankContributors(
            [("a", 5, later, "raider-one", "p1"), ("b", 5, joined, "raider-two", "p2")]));
        // Tied damage and join: ordinal slot wins.
        Assert.Equal(["a", "b"], RobotDefeatRewards.RankContributors(
            [("b", 5, joined, "raider-two", "p2"), ("a", 5, joined, "raider-one", "p1")]));
        // Tied damage, join and slot: ordinal player id wins.
        Assert.Equal(["a", "b"], RobotDefeatRewards.RankContributors(
            [("b", 5, joined, "raider-one", "p2"), ("a", 5, joined, "raider-one", "p1")]));
    }

    [Fact]
    public void CreateDefeatItemsIsDeterministicUniqueAndAtMostOneWeapon()
    {
        var catalog = LootCatalog.Default;
        foreach (var danger in Dangers)
        {
            var sawWeapon = false;
            for (var seed = 1; seed <= 400; seed++)
            {
                var first = RobotDefeatRewards.CreateDefeatItems(catalog, "raid", seed, "robot", danger);
                var second = RobotDefeatRewards.CreateDefeatItems(catalog, "raid", seed, "robot", danger);
                Assert.Equal(2, first.Count);
                Assert.Equal(
                    first.Select(item => (item.InstanceId, item.CatalogId)),
                    second.Select(item => (item.InstanceId, item.CatalogId)));
                Assert.NotEqual(first[0].InstanceId, first[1].InstanceId);
                Assert.StartsWith("bal-4:4:raid:5:robot:defeat:", first[0].InstanceId, StringComparison.Ordinal);
                var weapons = first.Count(item => ShopCatalog.TryGetWeapon(item.CatalogId, out _));
                Assert.InRange(weapons, 0, 1);
                // Slot A is always nonweapon cargo.
                var slotA = first.Single(item => item.InstanceId.Contains(":defeat:a:", StringComparison.Ordinal));
                Assert.False(ShopCatalog.TryGetWeapon(slotA.CatalogId, out _), "Slot A must never carry the weapon.");
                Assert.DoesNotContain(first, item => item.CatalogId.Contains("temptrest", StringComparison.OrdinalIgnoreCase));
                sawWeapon |= weapons == 1;
            }
            if (danger != "Low") Assert.True(sawWeapon, $"{danger} should roll at least one weapon across 400 seeds.");
        }
    }

    [Fact]
    public void MissingTableReferencesFailExplicitly()
    {
        // A catalog without the referenced defeat cargo cannot produce default loot.
        var reduced = new LootCatalog("missing-defeat-refs",
            CatalogCompatibilityTests.C3ItemIds.Select(LootCatalog.Default.GetItem),
            LootCatalog.Default.Opportunities,
            CatalogCompatibilityTests.C3ItemIds.Select(id => LootCatalog.Default.Pool.Single(e => e.CatalogId == id)));
        var failures = 0;
        for (var seed = 1; seed <= 50; seed++)
        {
            try
            {
                RobotDefeatRewards.CreateDefeatItems(reduced, "raid", seed, "robot", "Low");
            }
            catch (InvalidOperationException)
            {
                failures++;
            }
        }
        Assert.True(failures > 0, "Unknown defeat-table references must throw, not fall back.");
        Assert.Throws<InvalidOperationException>(() =>
            RobotDefeatRewards.CreateDefeatItems(LootCatalog.Default, "raid", 1, "robot", "Unrated"));
    }

    private static RaidSimulation RunRaid(int seed, string danger, out RaidProjection one, out RaidProjection two,
        StagedSupplyItem[]? supplies = null, WeaponDefinition? weaponOne = null, WeaponDefinition? weaponTwo = null)
    {
        var sim = new RaidSimulation($"bal2-{danger}-{seed}", Start, seed, danger: danger);
        sim.AddAutonomousRaider("one", "raider-one", Start, supplies, equippedWeapon: weaponOne,
            shield: new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100));
        sim.AddAutonomousRaider("two", "raider-two", Start, supplies, equippedWeapon: weaponTwo,
            shield: new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100));
        for (var t = 5; t <= 1500 && !sim.Completed; t += 5) sim.Advance(Start.AddSeconds(t));
        one = sim.Project("one", Start.AddSeconds(1500));
        two = sim.Project("two", Start.AddSeconds(1500));
        return sim;
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    public void DefeatCreatesExactlyTwoOwnedItemsAndNoDepotSearch(string danger)
    {
        var defeats = 0;
        var escapes = 0;
        var sharedDefeats = 0;
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        for (var seed = 1; seed <= 48; seed++)
        {
            RunRaid(seed, danger, out var one, out var two, weaponOne: vernerider, weaponTwo: vernerider);
            var all = one.Events.Concat(two.Events).DistinctBy(e => (e.Sequence, e.Kind)).ToArray();
            var roundLimit = RaidConditions.Profile(danger).RobotRounds;
            Assert.True(all.Count(e => e.Kind == "RobotRound") <= roundLimit,
                $"{danger} rounds must respect the profile limit {roundLimit}.");
            var defeated = all.Any(e => e.Kind == "RobotDefeated");
            var allocated = all.Where(e => e.Kind == "ItemAllocated"
                && e.ItemInstanceId?.Contains(":defeat:", StringComparison.Ordinal) == true).ToArray();
            // The legacy 10-second Depot reward search must never run in risk/reward worlds.
            Assert.DoesNotContain(all, e => e.Kind == "Search" && e.OpportunityId?.StartsWith("depot", StringComparison.Ordinal) == true);
            if (!defeated)
            {
                Assert.Empty(allocated);
                if (all.Any(e => e.Kind == "RobotEscaped")) escapes++;
                continue;
            }
            defeats++;
            var robotView = (one.RobotEncounter ?? two.RobotEncounter)!;
            var eligible = robotView.Damage.Where(d => d.DamageDealt > 0).Select(d => d.PlayerId).ToHashSet(StringComparer.Ordinal);
            Assert.Equal(one.Robot.MaxHealth, robotView.Damage.Sum(d => d.DamageDealt));
            // Exactly two items; owner-private receipts only; owners must be positive-damage contributors.
            Assert.Equal(2, allocated.Length);
            foreach (var award in allocated)
            {
                var owner = Assert.Single(award.ParticipantIds);
                Assert.Contains(owner, eligible);
            }
            var myAllocations = one.Events.Where(e => e.Kind == "ItemAllocated"
                && e.ItemInstanceId?.Contains(":defeat:") == true).ToArray();
            Assert.All(myAllocations, e => Assert.Equal(["one"], e.ParticipantIds));
            if (eligible.Count == 2 && allocated.Select(a => a.ParticipantIds[0]).Distinct().Count() == 2)
            {
                sharedDefeats++;
                // Contribution ranking: the higher contributor receives the more valuable (or equal) item.
                var byOwner = allocated.ToDictionary(a => a.ParticipantIds[0], a => a.ItemValue!.Value);
                var ordered = robotView.Damage.OrderByDescending(d => d.DamageDealt)
                    .ThenBy(d => d.JoinedAt).Select(d => d.PlayerId).ToArray();
                Assert.True(byOwner[ordered[0]] >= byOwner[ordered[1]],
                    "The ranked top contributor must receive the more valuable item.");
            }
        }
        output.WriteLine($"{danger}: defeats {defeats}, escapes {escapes}, two-owner defeats {sharedDefeats} over 48 seeds.");
        Assert.True(defeats > 0, $"The {danger} cohort must defeat at least one robot.");
    }

    [Fact]
    public void PreviouslyContributingRaiderWhoWithdrawsBeforeDefeatCannotClaim()
    {
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var found = false;
        for (var seed = 1; seed <= 96 && !found; seed++)
        {
            var sim = new RaidSimulation($"bal2-withdraw-{seed}", Start, seed, danger: "Medium");
            var shield = new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100);
            sim.AddAutonomousRaider("one", "raider-one", Start, equippedWeapon: vernerider, shield: shield);
            sim.AddAutonomousRaider("two", "raider-two", Start, equippedWeapon: vernerider, shield: shield);

            var withdrewAfterContributing = false;
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5)
            {
                var at = Start.AddSeconds(t);
                sim.Advance(at);
                var projection = sim.Project("one", at);
                if (!withdrewAfterContributing && projection.Events.Any(e =>
                    e.Kind == "RobotRound" && e.ParticipantIds.Contains("two")))
                {
                    if (projection.Players.Single(p => p.PlayerId == "two").Activity == "FightingRobot")
                    {
                        var leaveAt = at.AddSeconds(1);
                        Assert.True(sim.QueueHandlerAction("withdraw", "CallExtract", "two", leaveAt));
                        sim.Advance(leaveAt);
                        withdrewAfterContributing = true;
                    }
                }
            }

            var final = sim.Project("one", Start.AddSeconds(1500));
            if (!withdrewAfterContributing || !final.Events.Any(e => e.Kind == "RobotDefeated")) continue;
            var encounter = Assert.IsType<RobotEncounterView>(final.RobotEncounter);
            Assert.Contains(encounter.Damage, d => d.PlayerId == "two" && d.DamageDealt > 0);
            var allocations = final.Events.Where(e => e.Kind == "ItemAllocated"
                && e.ItemInstanceId?.Contains(":defeat:", StringComparison.Ordinal) == true).ToArray();
            Assert.Equal(2, allocations.Length);
            Assert.All(allocations, e => Assert.Equal(["one"], e.ParticipantIds));
            found = true;
        }

        Assert.True(found, "A seeded medium defeat after one positive-damage contributor withdraws is required.");
    }

    [Fact]
    public void SoleEligibleFighterReceivesBothItemsAndRetriesCannotDuplicate()
    {
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var found = false;
        for (var seed = 1; seed <= 64 && !found; seed++)
        {
            var sim = new RaidSimulation($"bal2-solo-{seed}", Start, seed, danger: "Low");
            sim.AddAutonomousRaider("one", "raider-one", Start, equippedWeapon: vernerider,
                shield: new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100));
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5)
            {
                sim.Advance(Start.AddSeconds(t));
                // Repeated advances at the same committed instant must not regenerate or reallocate.
                sim.Advance(Start.AddSeconds(t));
            }
            var final = sim.Project("one", Start.AddSeconds(1500));
            if (!final.Events.Any(e => e.Kind == "RobotDefeated")) continue;
            found = true;
            var allocated = final.Events.Where(e => e.Kind == "ItemAllocated"
                && e.ItemInstanceId?.Contains(":defeat:") == true).ToArray();
            Assert.Equal(2, allocated.Length);
            Assert.All(allocated, e => Assert.Equal(["one"], e.ParticipantIds));
            Assert.Equal(2, allocated.Select(e => e.ItemInstanceId).Distinct().Count());
        }
        Assert.True(found, "At least one solo Low defeat is required across 64 seeds.");
    }

    [Fact]
    public void SameSeedProducesIdenticalDefeatAllocation()
    {
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        for (var seed = 1; seed <= 12; seed++)
        {
            RunRaid(seed, "Medium", out var first, out _, weaponOne: vernerider, weaponTwo: vernerider);
            RunRaid(seed, "Medium", out var second, out _, weaponOne: vernerider, weaponTwo: vernerider);
            Assert.Equal(
                first.Events.Where(e => e.Kind == "ItemAllocated").Select(e => (e.ItemInstanceId, e.ParticipantIds[0])),
                second.Events.Where(e => e.Kind == "ItemAllocated").Select(e => (e.ItemInstanceId, e.ParticipantIds[0])));
        }
    }

    [Fact]
    public void EscapeOrNoFightersExhaustsTheDepotWithoutRewards()
    {
        // Starter kettles against the BAL-2 High robot (45 HP, 4 rounds) cannot win: max 4 fighters-rounds
        // of two kettles is 2*4*6=48 only with perfect rolls; most seeds escape or down the fighters.
        var kettle = ShopCatalog.GetStarterWeapon();
        var sawEscape = false;
        for (var seed = 1; seed <= 24 && !sawEscape; seed++)
        {
            RunRaid(seed, "High", out var one, out var two, weaponOne: kettle, weaponTwo: kettle);
            var all = one.Events.Concat(two.Events).ToArray();
            if (!all.Any(e => e.Kind == "RobotEscaped")) continue;
            sawEscape = true;
            Assert.DoesNotContain(all, e => e.Kind == "ItemAllocated"
                && e.ItemInstanceId?.Contains(":defeat:") == true);
            // The depot stays terminally exhausted: anyone visiting later finds it emptied.
            Assert.DoesNotContain(all, e => e.Kind == "SearchCompleted"
                && e.OpportunityId?.StartsWith("depot", StringComparison.Ordinal) == true);
        }
        Assert.True(sawEscape, "At least one High escape is expected for starter kettles across 24 seeds.");
    }

    [Fact]
    public void HighFightsCanUseTheFourthRoundAndKillingRoundHasNoRetaliation()
    {
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var sawFourRounds = false;
        var sawDefeat = false;
        for (var seed = 1; seed <= 64; seed++)
        {
            RunRaid(seed, "High", out var one, out var two, weaponOne: vernerider, weaponTwo: vernerider);
            var all = one.Events.Concat(two.Events).DistinctBy(e => (e.Sequence, e.Kind)).ToArray();
            var rounds = all.Count(e => e.Kind == "RobotRound");
            sawFourRounds |= rounds == 4;
            if (all.Any(e => e.Kind == "RobotDefeated"))
            {
                sawDefeat = true;
                // The defeat round deals no retaliation: no robot damage event at the defeat timestamp.
                var defeatAt = all.Single(e => e.Kind == "RobotDefeated").At;
                Assert.DoesNotContain(all, e => e.Kind is "ShieldAbsorbed" or "UnmitigatedDamage"
                    && e.At == defeatAt && e.ThreadId?.StartsWith("robot:") == true);
            }
        }
        Assert.True(sawFourRounds, "High fights must be able to reach a fourth round.");
        Assert.True(sawDefeat, "Two Verneriders must defeat at least one High robot across 64 seeds.");
    }

    [Fact]
    public void HighRobotCanBeDefeatedWithoutHealing()
    {
        var noSupplies = Array.Empty<StagedSupplyItem>();
        var temptrest = ShopCatalog.Weapons.Single(w => w.Id == "temptrest_ar");
        var found = false;
        for (var seed = 0; seed < 256 && !found; seed++)
        {
            var sim = new RaidSimulation("bal-1-combat", Start, seed, danger: "High");
            sim.AddAutonomousRaider("one", "raider-one", Start, noSupplies, equippedWeapon: temptrest,
                shield: new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityCenti));
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5) sim.Advance(Start.AddSeconds(t));
            var final = sim.Project("one", Start.AddSeconds(1500));
            if (final.RobotEncounter?.Resolution != "Defeated") continue;
            Assert.DoesNotContain(final.Events, e => e.Kind == "ConsumableUsed");
            found = true;
        }

        Assert.True(found, "A seeded High victory must remain legal without any healing supplies.");
    }

    [Fact]
    public void DefeatDoesNotGrantTheDepotSearchPersonalityBonus()
    {
        // The removed 10s Depot search previously granted +2 greed / +1 mood at completion.
        // Compare adjacent ticks so earlier ordinary activity cannot mask that removed bonus.
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var verified = false;
        for (var seed = 1; seed <= 64 && !verified; seed++)
        {
            var sim = new RaidSimulation($"bal2-pers-{seed}", Start, seed, danger: "Low");
            sim.AddAutonomousRaider("one", "raider-one", Start, equippedWeapon: vernerider,
                shield: new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100));
            for (var t = 5; t <= 1500 && !sim.Completed; t += 5)
            {
                var previous = sim.Personality("one");
                var at = Start.AddSeconds(t);
                sim.Advance(at);
                var eventsAtTick = sim.Project("one", at).Events.Where(e => e.At == at).ToArray();
                if (!eventsAtTick.Any(e => e.Kind == "RobotDefeated")) continue;
                if (eventsAtTick.Any(e => e.Kind is "SearchCompleted" or "EncounterResolved"
                    && e.ItemInstanceId is null && e.OpportunityId is not null)
                    || eventsAtTick.Any(e => e.Kind == "ConsumableUsed")) continue;

                var personality = sim.Personality("one");
                var passiveDrift = (int)(t / 60) - (int)((t - 5) / 60);
                Assert.Equal(previous.Greed + passiveDrift, personality.Greed);
                Assert.Equal(previous.Mood, personality.Mood);
                verified = true;
            }
        }
        Assert.True(verified, "A solo robot defeat without a simultaneous personality-changing event is required across 64 seeds.");
    }

    [Fact]
    public void PocketedDefeatCargoSurvivesKnockoutAndUnprotectedCargoIsLost()
    {
        StagedSupplyItem[] noSupplies = [];
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var sawPocketedKo = false;
        var sawUnprotectedKo = false;
        for (var seed = 1; seed <= 96 && !(sawPocketedKo && sawUnprotectedKo); seed++)
        {
            foreach (var protect in new[] { true, false })
            {
                // No shield and no supplies: hazards after the fight regularly down the Raider.
                var sim = new RaidSimulation($"bal2-ko-{seed}", Start, seed, danger: "High");
                sim.AddAutonomousRaider("one", "raider-one", Start, noSupplies, equippedWeapon: vernerider);
                sim.AddAutonomousRaider("two", "raider-two", Start, noSupplies, equippedWeapon: vernerider);
                string? pocketed = null;
                for (var t = 5; t <= 1500 && !sim.Completed; t += 5)
                {
                    sim.Advance(Start.AddSeconds(t));
                    if (protect && pocketed is null)
                    {
                        var projection = sim.Project("one", Start.AddSeconds(t));
                        var cargo = projection.MyInventory.FirstOrDefault(i => i.InstanceId.Contains(":defeat:"));
                        if (cargo is not null && sim.DesignateHiddenPocket("one", cargo.InstanceId))
                            pocketed = cargo.InstanceId;
                    }
                }
                var final = sim.Project("one", Start.AddSeconds(1500));
                var hadCargo = final.Events.Any(e => e.Kind == "ItemAllocated"
                    && e.ItemInstanceId?.Contains(":defeat:") == true && e.ParticipantIds.Contains("one"));
                var knockedOut = final.Events.Any(e => e.Kind == "KnockedOut");
                if (!hadCargo || !knockedOut) continue;
                if (protect && pocketed?.Contains(":defeat:") == true)
                {
                    Assert.Contains(final.MyExtractedLoot, i => i.InstanceId == pocketed);
                    Assert.Single(final.MyExtractedLoot);
                    sawPocketedKo = true;
                }
                else if (!protect)
                {
                    Assert.DoesNotContain(final.MyExtractedLoot, i => i.InstanceId.Contains(":defeat:"));
                    sawUnprotectedKo = true;
                }
            }
        }
        Assert.True(sawPocketedKo, "A pocketed defeat-cargo KO case is required across the seed scan.");
        Assert.True(sawUnprotectedKo, "An unprotected defeat-cargo KO case is required across the seed scan.");
    }

    [Fact]
    public void ExtractedDefeatWeaponConvertsOnceThroughFoundWeaponSettlement()
    {
        var catalog = LootCatalog.Default;
        var cargoWeapon = new ItemView("raid:robot:defeat:b:vernerider", "vernerider", "Vernerider", 140, "No one can pronounce it right.");
        var cargoTech = new ItemView("raid:robot:defeat:a:arc_data_shard", "arc_data_shard", "Chassis Data Shard", 60, "Encrypted.");
        var result = FoundWeaponSettlement.Separate([cargoWeapon, cargoTech], [], catalog);
        var weapon = Assert.Single(result.WeaponsToKeep);
        Assert.Equal("vernerider", weapon.CatalogId);
        var definition = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        Assert.InRange(weapon.CurrentDurability,
            (int)Math.Ceiling(definition.DurabilityMax * 0.05), (int)Math.Floor(definition.DurabilityMax * 0.60));
        Assert.Equal("arc_data_shard", Assert.Single(result.StashItems).CatalogId);
        // Re-settling the same instance cannot duplicate ownership.
        var again = FoundWeaponSettlement.Separate([cargoWeapon], result.WeaponsToKeep, catalog);
        Assert.Empty(again.WeaponsToKeep);
    }

    [Fact]
    public void DefeatRewardsAppendWithoutDisplacingExistingBackpackLoot()
    {
        var vernerider = ShopCatalog.Weapons.Single(w => w.Id == "vernerider");
        var found = false;
        for (var seed = 1; seed <= 96 && !found; seed++)
        {
            var sim = new RaidSimulation($"bal2-backpack-{seed}", Start, seed, danger: "Low");
            var shield = new ShieldLoadout(ShieldTier.Heavy, ShieldRules.MaxDurabilityPoints * 100);
            sim.AddAutonomousRaider("one", "raider-one", Start, equippedWeapon: vernerider, shield: shield);
            sim.AddAutonomousRaider("two", "raider-two", Start, equippedWeapon: vernerider, shield: shield);

            for (var t = 5; t <= 1500 && !sim.Completed && !found; t += 5)
            {
                var at = Start.AddSeconds(t);
                sim.Advance(at);
                var projections = new[] { sim.Project("one", at), sim.Project("two", at) };
                if (!projections.Any(p => p.Events.Any(e => e.Kind == "RobotDefeated"))) continue;

                foreach (var (playerId, projection) in new[] { ("one", projections[0]), ("two", projections[1]) })
                {
                    var rewardIds = projection.Events
                        .Where(e => e.Kind == "ItemAllocated"
                            && e.ParticipantIds.Contains(playerId, StringComparer.Ordinal)
                            && e.ItemInstanceId?.Contains(":defeat:", StringComparison.Ordinal) == true)
                        .Select(e => e.ItemInstanceId!)
                        .ToHashSet(StringComparer.Ordinal);
                    var existingItems = projection.MyInventory
                        .Where(item => !item.InstanceId.Contains(":defeat:", StringComparison.Ordinal))
                        .ToArray();
                    if (rewardIds.Count == 0 || existingItems.Length == 0) continue;

                    Assert.All(existingItems, item =>
                        Assert.Contains(projection.MyInventory, current => current.InstanceId == item.InstanceId));
                    Assert.All(rewardIds, itemId =>
                        Assert.Contains(projection.MyInventory, item => item.InstanceId == itemId));
                    Assert.Equal(existingItems.Length + rewardIds.Count, projection.MyInventory.Count);
                    found = true;
                    break;
                }
            }
        }

        Assert.True(found, "A defeat should append cargo to a participant who already carries field loot.");
    }

    [Fact]
    public void HighValueDefeatCargoEntersTheStashWithNormalOverflowRules()
    {
        var stash = new HomeStash();
        var filler = Enumerable.Range(0, 119).Select(i =>
            new ItemView($"f{i:D3}", "scrap_metal_basic", "Basic Scrap Metal", 8, "x")).ToArray();
        Assert.Empty(stash.TransferHaul(filler).AutoSoldItems);
        var jackpot = new ItemView("raid:robot:defeat:a:arc_data_shard_legendary", "arc_data_shard_legendary",
            "Chassis Data Shard (Prime Archive)", 900, "You feel smarter just holding it. Briefly.");
        var extra = new ItemView("raid:robot:defeat:b:energy_core_overcharged", "energy_core_overcharged",
            "Overcharged Energy Core", 150, "Hums loudly. Too loudly.");
        var transfer = stash.TransferHaul([jackpot, extra]);
        // 121 units: exactly one lowest-value ordinary item is auto-sold; the tech stays.
        Assert.Equal(120, stash.Count);
        Assert.Single(transfer.AutoSoldItems);
        Assert.Equal("scrap_metal_basic", transfer.AutoSoldItems[0].CatalogId);
        Assert.Contains(stash.Items, i => i.CatalogId == "arc_data_shard_legendary");
        Assert.Contains(stash.Items, i => i.CatalogId == "energy_core_overcharged");
    }
}
