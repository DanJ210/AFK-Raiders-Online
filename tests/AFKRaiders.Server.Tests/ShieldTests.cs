using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

/// <summary>BAL-1S: shield tiers, per-raid charge, persistent wear and paid autonomous repair.</summary>
public sealed class ShieldTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-20T12:00:00Z");
    private static readonly ShieldTier[] Tiers = [ShieldTier.Light, ShieldTier.Medium, ShieldTier.Heavy];

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static int Permille(ShieldTier tier) => ShopCatalog.GetShield(tier).MitigationPermille;

    private static int CarryTotal(int damage, int permille, int hits)
    {
        var carry = 0;
        var total = 0;
        for (var i = 0; i < hits; i++)
        {
            var prevented = ShieldRules.Mitigate(damage, permille, ref carry);
            Assert.InRange(prevented, 0, damage);
            Assert.InRange(carry, 0, 999);
            total += prevented;
        }
        return total;
    }

    private static int FloorOnlyTotal(int damage, int permille, int hits) => hits * (damage * permille / 1000);

    private static void AssertTierOrder(int damage, Func<int, int, int, int> total)
    {
        var totals = Tiers.Select(tier => total(damage, Permille(tier), 1000)).ToArray();
        Assert.True(totals[0] < totals[1] && totals[1] < totals[2],
            $"Prevented HP for {damage}-damage hits must rise strictly by tier: {string.Join(" / ", totals)}.");
    }

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(22)]
    [InlineData(30)]
    public void RemainderCarryRealizesNominalMitigationAndStrictTierOrder(int damage)
    {
        foreach (var tier in Tiers)
        {
            // 1000 hits x damage x permille / 1000 is an integer, so the carry realizes it exactly.
            Assert.Equal(damage * Permille(tier), CarryTotal(damage, Permille(tier), 1000));
            for (var hits = 1; hits <= 50; hits++)
                Assert.InRange(CarryTotal(damage, Permille(tier), hits) - hits * damage * Permille(tier) / 1000.0, -1.0, 0.0);
        }
        AssertTierOrder(damage, CarryTotal);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    public void FloorOnlyMitigationFailsTheLowDamageTierOrder(int damage)
    {
        // Guard against regressing to a per-hit floor: Light and Medium would prevent identical HP.
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => AssertTierOrder(damage, FloorOnlyTotal));
    }

    [Fact]
    public void CarryIsOrderedAcrossDifferentHitSizesAndNeverExceedsTheHit()
    {
        var carry = 0;
        Assert.Equal(3, ShieldRules.Mitigate(8, 421, ref carry));   // 3368
        Assert.Equal(368, carry);
        Assert.Equal(4, ShieldRules.Mitigate(10, 421, ref carry));  // 368 + 4210 = 4578
        Assert.Equal(578, carry);
        Assert.Equal(0, ShieldRules.Mitigate(1, 421, ref carry));   // 578 + 421 = 999
        Assert.Equal(999, carry);
        Assert.Equal(1, ShieldRules.Mitigate(1, 421, ref carry));   // 999 + 421 = 1420
        Assert.Equal(420, carry);
        Assert.Equal(0, ShieldRules.Mitigate(0, 421, ref carry));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShieldRules.Mitigate(-1, 421, ref carry));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShieldRules.Mitigate(5, 1000, ref carry));
    }

    [Fact]
    public void RechargeTriggerIsThreeEighthsOfMaxCharge()
    {
        Assert.Equal([15, 26, 37], Tiers.Select(t => ShieldRules.RechargeTrigger(ShopCatalog.GetShield(t).MaxCharge)).ToArray());
    }

    [Fact]
    public void WearScalesWithChargeSpentFractionAndDanger()
    {
        Assert.Equal(2000, ShieldRules.WearCenti(40, 40, "Low"));
        Assert.Equal(3000, ShieldRules.WearCenti(40, 40, "Medium"));
        Assert.Equal(4000, ShieldRules.WearCenti(40, 40, "High"));
        Assert.Equal(2000, ShieldRules.WearCenti(100, 100, "Low"));
        Assert.Equal(50, ShieldRules.WearCenti(1, 40, "Low"));
        Assert.Equal(42, ShieldRules.WearCenti(1, 70, "Medium"));  // 1 * 2000 * 3 / 140 = 42.86 -> 42
        Assert.Equal(0, ShieldRules.WearCenti(0, 100, "High"));
        Assert.Throws<ArgumentException>(() => ShieldRules.WearCenti(1, 40, "Unknown"));
    }

    [Theory]
    [InlineData(30, new[] { 1, 3, 7, 12, 18 })]
    [InlineData(90, new[] { 3, 8, 20, 35, 54 })]
    [InlineData(300, new[] { 9, 26, 64, 116, 180 })]
    public void RepairCostMatchesExactIntegerFixtures(int price, int[] expected)
    {
        Assert.Equal(expected, new[] { 10, 25, 50, 75, 100 }.Select(m => ShieldRules.RepairCost(price, m)).ToArray());
        Assert.Equal(0, ShieldRules.RepairCost(price, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ShieldRules.RepairCost(price, 101));
    }

    [Fact]
    public void MissingPointsRoundUpAndRepairCapsAtFull()
    {
        Assert.Equal(0, ShieldRules.MissingPoints(10_000));
        Assert.Equal(1, ShieldRules.MissingPoints(9_950));
        Assert.Equal(100, ShieldRules.MissingPoints(0));
        Assert.Equal(10_000, ShieldRules.ApplyRepair(9_950, 1));
        Assert.Equal(3_500, ShieldRules.ApplyRepair(2_500, 10));
    }

    [Theory]
    [InlineData(RaiderPrimaryTrait.Cautious, 75)]
    [InlineData(RaiderPrimaryTrait.Hoarder, 50)]
    [InlineData(RaiderPrimaryTrait.Gadgeteer, 50)]
    [InlineData(RaiderPrimaryTrait.Brawler, 25)]
    public void RepairStartsExactlyAtTheTraitThreshold(RaiderPrimaryTrait trait, int threshold)
    {
        Assert.Equal(threshold, RaiderEconomyPolicy.ShieldRepairThresholdPercent(trait));
        // 80 coins cannot reach Medium above any reserve, so the decision is repair-or-nothing.
        var above = RaiderEconomyPolicy.DecideShield(trait, 80, 0, new(ShieldTier.Light, threshold * 100 + 1));
        Assert.Equal(ShieldPlanAction.None, above.Action);
        var at = RaiderEconomyPolicy.DecideShield(trait, 80, 0, new(ShieldTier.Light, threshold * 100));
        Assert.Equal(ShieldPlanAction.Repair, at.Action);
        Assert.Equal(100 - threshold, at.RepairPoints);
        Assert.Equal(ShieldRules.MaxDurabilityCenti, at.DurabilityCentiAfter);
        Assert.Equal(ShieldRules.RepairCost(30, 100 - threshold), at.Cost);
    }

    [Fact]
    public void PartialRepairKeepsTheReserveAndUsesTheLargestAffordableBlock()
    {
        var plan = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Cautious, 70, 50, new(ShieldTier.Medium, 2_000));
        Assert.Equal(ShieldPlanAction.Repair, plan.Action);
        Assert.InRange(plan.RepairPoints, ShieldRules.MinimumRepairBlockPoints, 80);
        Assert.True(70 - plan.Cost >= 50);
        Assert.True(plan.RepairPoints == 80 || ShieldRules.RepairCost(90, plan.RepairPoints + 1) > 20);
        Assert.Equal(2_000 + plan.RepairPoints * 100, plan.DurabilityCentiAfter);
    }

    [Fact]
    public void NoRepairWhenEvenTheMinimumBlockWouldBreachTheReserve()
    {
        // Medium 10-point block costs 3; 2 spendable coins are not enough and sub-10 blocks are not allowed.
        var plan = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Cautious, 52, 50, new(ShieldTier.Medium, 2_000));
        Assert.Equal(ShieldPlanAction.None, plan.Action);
        Assert.Equal(0, plan.Cost);
    }

    [Fact]
    public void ZeroDurabilityRepairMayWaiveTheReserve()
    {
        var plan = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Brawler, 20, 100, new(ShieldTier.Light, 0));
        Assert.Equal(ShieldPlanAction.Repair, plan.Action);
        Assert.True(plan.Cost <= 20);
        Assert.True(plan.RepairPoints == 100 || ShieldRules.RepairCost(30, plan.RepairPoints + 1) > 20);
    }

    [Fact]
    public void AffordableUpgradeIsEvaluatedBeforeRepairAndStartsAtFullDurability()
    {
        var plan = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Cautious, 200, 50, new(ShieldTier.Light, 1_000));
        Assert.Equal(ShieldPlanAction.Upgrade, plan.Action);
        Assert.Equal(ShieldTier.Medium, plan.Tier);
        Assert.Equal(90, plan.Cost);
        Assert.Equal(ShieldRules.MaxDurabilityCenti, plan.DurabilityCentiAfter);

        var heavy = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Brawler, 1_000, 100, new(ShieldTier.Medium, 9_000));
        Assert.Equal((ShieldPlanAction.Upgrade, ShieldTier.Heavy, 300), (heavy.Action, heavy.Tier, heavy.Cost));
        Assert.Equal(ShieldPlanAction.None,
            RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Brawler, 1_000, 100, new(ShieldTier.Heavy, 9_000)).Action);
    }

    [Fact]
    public void MissingShieldBuysBestAboveReserveThenEmergencyLightThenDeploysUnshielded()
    {
        var best = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Hoarder, 1_000, 100, ShieldLoadout.Unshielded);
        Assert.Equal((ShieldPlanAction.Buy, ShieldTier.Heavy, 300), (best.Action, best.Tier, best.Cost));
        var medium = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Hoarder, 150, 60, ShieldLoadout.Unshielded);
        Assert.Equal((ShieldPlanAction.Buy, ShieldTier.Medium, 90), (medium.Action, medium.Tier, medium.Cost));
        var emergency = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Hoarder, 40, 100, ShieldLoadout.Unshielded);
        Assert.Equal((ShieldPlanAction.EmergencyBuy, ShieldTier.Light, 30), (emergency.Action, emergency.Tier, emergency.Cost));
        var none = RaiderEconomyPolicy.DecideShield(RaiderPrimaryTrait.Hoarder, 29, 0, ShieldLoadout.Unshielded);
        Assert.Equal((ShieldPlanAction.DeployUnshielded, 0), (none.Action, none.Cost));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PreparationPlansWeaponThenShieldThenSuppliesWithoutOverdraw(int seed)
    {
        foreach (var coins in new[] { 0, 29, 45, 120, 400, 1_000 })
        {
            var plan = RaiderEconomyPolicy.Decide("owner", 1, seed, Start, coins, [],
                [ShopCatalog.CreateWeaponInstance(ShopCatalog.GetStarterWeapon(), "starter")], "starter",
                new Dictionary<string, int>(), [], LootCatalog.Default, ShopCatalog.Weapons, ShopCatalog.Supplies,
                shield: ShieldLoadout.Unshielded);
            var shield = Assert.IsType<ShieldPreparationPlan>(plan.Shield);
            var weaponCost = plan.WeaponCatalogIdToBuy is { } id ? ShopCatalog.Weapons.Single(w => w.Id == id).Value : 0;
            var supplyCost = plan.SupplyPurchases.Sum(p => p.Quantity * ShopCatalog.Supplies.Single(s => s.Id == p.CatalogId).Value);
            Assert.True(weaponCost + shield.Cost + supplyCost <= coins,
                $"seed {seed} coins {coins}: weapon {weaponCost} + shield {shield.Cost} + supplies {supplyCost}");
            if (coins - weaponCost < 30)
                Assert.Equal(ShieldPlanAction.DeployUnshielded, shield.Action);
            else
                Assert.NotEqual(ShieldPlanAction.DeployUnshielded, shield.Action);
            if (shield.Action == ShieldPlanAction.Buy)
                Assert.True(coins - weaponCost - shield.Cost >= plan.ReserveFloor);
        }
    }

    [Theory]
    [InlineData("Medium", 3_000)]
    [InlineData("Light", 0)]
    [InlineData("None", 0)]
    public void AdmissionRefillsChargeOnlyAndNeverRepairsOrRecreatesTheShield(string tierName, int durabilityCenti)
    {
        var tier = Enum.Parse<ShieldTier>(tierName);
        var clock = new Clock();
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 5, traderSeed: 83);
        var token = coordinator.EnrollHandler("raider-one", $"shield-admit-{tierName}").AccessToken;
        coordinator.SetShieldForTesting(token, new ShieldLoadout(tier, durabilityCenti));
        // Zero coins: no purchase, upgrade or repair is affordable, so admission alone acts on the shield.
        coordinator.ResetPreparationForTesting(token, 0, Start);
        clock.Now = Start.AddSeconds(600);
        Parallel.For(0, 4, _ => coordinator.AdvanceRaids());

        var admitted = coordinator.GetHandlerProjection(token);
        var raid = Assert.IsType<RaidProjection>(admitted.Raid);
        Assert.Equal(0, admitted.Coins);
        Assert.Equal(tierName, raid.MyShieldTier);
        Assert.Equal(tierName, admitted.Shield!.Tier);
        Assert.Equal(durabilityCenti / 100.0, raid.MyShieldDurability);
        Assert.Equal(durabilityCenti / 100.0, admitted.Shield.Durability);
        var maxCharge = tier == ShieldTier.None ? 0 : ShopCatalog.GetShield(tier).MaxCharge;
        Assert.Equal(maxCharge, raid.MyShieldMaxCharge);
        Assert.Equal(maxCharge, raid.MyShieldCharge);
        Assert.Equal(tier == ShieldTier.None ? 0 : Permille(tier), raid.MyShieldMitigationPermille);
        Assert.Equal(tier == ShieldTier.None ? 1 : 0,
            admitted.FullJournal!.Count(e => e.Kind == "PreparationShieldUnaffordable"));
        Assert.DoesNotContain(admitted.FullJournal!, e => e.Kind is "PreparationShieldPurchase" or "PreparationShieldRepair");
    }

    [Fact]
    public void ShieldPurchaseCommitsOnceWithoutOverdrawAndProjectsReadOnly()
    {
        var clock = new Clock();
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 5, traderSeed: 83);
        var token = coordinator.EnrollHandler("raider-one", "shield-buy").AccessToken;
        coordinator.SetShieldForTesting(token, ShieldLoadout.Unshielded);
        coordinator.ResetPreparationForTesting(token, 1_000, Start);
        clock.Now = Start.AddSeconds(590);
        Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
        var prepared = coordinator.GetHandlerProjection(token);
        Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
        var retried = coordinator.GetHandlerProjection(token);

        var purchase = Assert.Single(retried.FullJournal!, e => e.Kind == "PreparationShieldPurchase");
        Assert.StartsWith("hub:preparation:", purchase.SourceEventId);
        Assert.Contains(":shield:buy:", purchase.SourceEventId);
        Assert.Equal(prepared.Coins, retried.Coins);
        Assert.True(retried.Coins >= 0);
        var shield = Assert.IsType<OwnedShieldView>(retried.Shield);
        Assert.NotEqual("None", shield.Tier);
        Assert.Equal(100, shield.Durability);
        var definition = ShopCatalog.GetShield(Enum.Parse<ShieldTier>(shield.Tier));
        Assert.Equal((definition.Id, definition.Name, definition.Color), (shield.CatalogId!, shield.Name!, shield.Color!));
        Assert.Equal(definition.Value, retried.Stats!.Economy.CoinsOnShields);
        Assert.Equal(1, retried.Stats.Economy.LightShieldsBought + retried.Stats.Economy.MediumShieldsBought
            + retried.Stats.Economy.HeavyShieldsBought);
        Assert.Contains(definition.Name, purchase.Text);
        Assert.Equal(3, retried.ShopCatalog!.Shields!.Count);
    }

    [Fact]
    public void KnockoutRemovesTheShieldOnceAndExtractionPersistsWear()
    {
        var knockouts = 0;
        var extractions = 0;
        for (var seed = 0; seed < 32; seed++)
        {
            var clock = new Clock();
            var coordinator = new RaidCoordinator(clock, nextSeed: () => seed, traderSeed: 83);
            var token = coordinator.EnrollHandler("raider-one", $"shield-ko-{seed}").AccessToken;
            coordinator.ResetPreparationForTesting(token, coins: seed % 2 == 0 ? 30 : 120, arrivedAt: clock.Now);
            clock.Now += TimeSpan.FromSeconds(600);
            coordinator.AdvanceRaids();
            var admitted = coordinator.GetHandlerProjection(token);
            var raid = Assert.IsType<RaidProjection>(admitted.Raid);
            var deployed = Assert.IsType<OwnedShieldView>(admitted.Shield);
            coordinator.SetDuty(token, false);
            for (var elapsed = 5; elapsed <= 1500; elapsed += 5)
            {
                clock.Now += TimeSpan.FromSeconds(5);
                coordinator.AdvanceRaids();
                if (coordinator.GetHandlerProjection(token).LastReturnSummary is not null) break;
            }
            var returned = coordinator.GetHandlerProjection(token);
            var summary = Assert.IsType<ReturnSummaryView>(returned.LastReturnSummary);
            Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
            var retried = coordinator.GetHandlerProjection(token);
            var stats = retried.Stats!.Total;
            Assert.Equal(1, stats.Raids);
            Assert.Equal(1, stats.LightShieldRaids + stats.MediumShieldRaids + stats.HeavyShieldRaids + stats.UnshieldedRaids);
            Assert.Equal(returned.Shield, retried.Shield);
            var lost = retried.FullJournal!.Where(e => e.Kind == "ShieldLost").ToArray();
            if (summary.Outcome == "KnockedOut" && deployed.Tier != "None")
            {
                knockouts++;
                Assert.Equal("None", retried.Shield!.Tier);
                var entry = Assert.Single(lost);
                Assert.Equal($"hub:shield-lost:{raid.RaidId}:{admitted.Slot}", entry.SourceEventId);
                Assert.Contains(deployed.Name!, entry.Text);
                Assert.Equal(1, stats.ShieldsLost);

                // The next preparation replaces, emergency-buys or deploys unshielded exactly once; it never loans one.
                clock.Now += TimeSpan.FromMinutes(10);
                Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
                var next = coordinator.GetHandlerProjection(token);
                Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
                var nextRetried = coordinator.GetHandlerProjection(token);
                Assert.True(next.Coins >= 0);
                Assert.Equal(next.Coins, nextRetried.Coins);
                var settledThrough = retried.FullJournal!.Max(e => e.Sequence);
                var decisions = nextRetried.FullJournal!.Where(e => e.Sequence > settledThrough &&
                    e.Kind is "PreparationShieldPurchase" or "PreparationShieldUnaffordable").ToArray();
                var decision = Assert.Single(decisions);
                Assert.Equal(nextRetried.Shield!.Tier == "None", decision.Kind == "PreparationShieldUnaffordable");
                if (nextRetried.Shield.Tier != "None") Assert.Equal(100, nextRetried.Shield.Durability);
                Assert.Single(nextRetried.FullJournal!, e => e.Kind == "ShieldLost");
            }
            else if (summary.Outcome == "Extracted")
            {
                extractions++;
                Assert.Empty(lost);
                Assert.Equal(deployed.Tier, retried.Shield!.Tier);
                Assert.True(retried.Shield.Durability <= deployed.Durability);
                Assert.Equal((int)Math.Round((deployed.Durability - retried.Shield.Durability) * 100),
                    stats.ShieldDurabilityLostCenti);
                Assert.Equal(0, stats.ShieldsLost);
            }
        }
        Assert.True(knockouts > 0, "The seeded cohort must exercise shield loss on knockout.");
        Assert.True(extractions > 0, "The seeded cohort must exercise worn-shield extraction.");
    }

    [Fact]
    public void RulesVersionAdvertisesCurrentBalanceSlice()
    {
        // BAL-1S shield rules are unchanged; the advertised version moved forward with BAL-2.
        Assert.Equal("desperanza-bal-4-v1", RaidSimulation.RulesVersion);
    }
}
