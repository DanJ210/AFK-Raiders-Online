using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

public sealed class TraderStockTests(ITestOutputHelper output)
{
    private sealed class ManualTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void SeedAndCycleDetermineFiniteCatalogPricedStock()
    {
        var at = DateTimeOffset.Parse("2026-10-19T19:40:00Z");
        var generator = new TraderStockGenerator(seed: 2718, new ManualTime(at));
        var first = generator.Generate(at, ShopCatalog.Weapons, ShopCatalog.Supplies);
        var sameCycle = generator.Generate(at.AddMinutes(9), ShopCatalog.Weapons, ShopCatalog.Supplies);
        var nextCycle = generator.Generate(at.AddMinutes(10), ShopCatalog.Weapons, ShopCatalog.Supplies);
        var sameSeed = new TraderStockGenerator(seed: 2718, new ManualTime(at))
            .Generate(at, ShopCatalog.Weapons, ShopCatalog.Supplies);

        Assert.Equal(first.CycleIndex, sameCycle.CycleIndex);
        Assert.Equal(first.Items, sameCycle.Items);
        Assert.Equal(first.CycleIndex, sameSeed.CycleIndex);
        Assert.Equal(first.Items, sameSeed.Items);
        Assert.Equal(first.CycleIndex + 1, nextCycle.CycleIndex);
        Assert.NotEqual(
            first.Items.Single(item => item.Category == "Weapon" && item.Quantity == 1).CatalogId,
            nextCycle.Items.Single(item => item.Category == "Weapon" && item.Quantity == 1).CatalogId);
        Assert.Equal(5, first.Items.Count);

        var kettle = Assert.Single(first.Items, item => item.CatalogId == "tea_kettle");
        Assert.Null(kettle.Quantity);
        Assert.Equal(0, kettle.UnitPrice);

        var weapon = Assert.Single(first.Items, item => item.Category == "Weapon" && item.Quantity == 1);
        Assert.NotEqual("tea_kettle", weapon.CatalogId);
        Assert.Equal(ShopCatalog.Weapons.Single(item => item.Id == weapon.CatalogId).Value, weapon.UnitPrice);

        var supplies = first.Items.Where(item => item.Category != "Weapon").ToArray();
        Assert.Equal(3, supplies.Length);
        Assert.Equal(3, supplies.Select(item => item.CatalogId).Distinct(StringComparer.Ordinal).Count());
        foreach (var supply in supplies)
        {
            Assert.InRange(supply.Quantity!.Value, 2, 4);
            Assert.Equal(ShopCatalog.Supplies.Single(item => item.Id == supply.CatalogId).Value, supply.UnitPrice);
        }
    }

    [Fact]
    public void CycleIndexUsesTheUnixEpochAndTenMinuteFloor()
    {
        var generator = new TraderStockGenerator(0, new ManualTime(DateTimeOffset.UnixEpoch));

        Assert.Equal(-1, generator.CycleIndexAt(DateTimeOffset.UnixEpoch.AddTicks(-1)));
        Assert.Equal(0, generator.CycleIndexAt(DateTimeOffset.UnixEpoch));
        Assert.Equal(1, generator.CycleIndexAt(DateTimeOffset.UnixEpoch.AddMinutes(10)));
        Assert.Equal(1, generator.CycleIndexAt(DateTimeOffset.UnixEpoch.AddMinutes(19).AddSeconds(59)));
    }

    [Fact]
    public void ScarceWeaponContestUsesArrivalThenRaiderIdNotProjectionOrder()
    {
        var arrivedAt = DateTimeOffset.Parse("2026-10-19T19:40:00Z");
        var (coordinator, clock, raiderOne, raiderTwo, offeredWeapon) =
            StartTraderContest(arrivedAt, traderSeed: 83);
        var one = coordinator.GetHandlerProjection(raiderOne);
        var two = coordinator.GetHandlerProjection(raiderTwo);
        var winnerToken = string.CompareOrdinal(one.PlayerId, two.PlayerId) < 0 ? raiderOne : raiderTwo;
        var loserToken = winnerToken == raiderOne ? raiderTwo : raiderOne;

        clock.Now = arrivedAt.AddMinutes(9);
        _ = coordinator.GetHandlerProjection(loserToken);
        _ = coordinator.GetHandlerProjection(winnerToken);
        coordinator.AdvanceRaids();

        var winner = coordinator.GetHandlerProjection(winnerToken);
        var loser = coordinator.GetHandlerProjection(loserToken);
        Assert.Contains(winner.OwnedWeapons!, weapon => weapon.CatalogId == offeredWeapon);
        Assert.DoesNotContain(loser.OwnedWeapons!, weapon => weapon.CatalogId == offeredWeapon);
        Assert.Contains(winner.LatestPreparationDecisions!, decision =>
            decision.Kind == "PreparationWeaponPurchase");
        Assert.Contains(loser.LatestPreparationDecisions!, decision =>
            decision.Kind == "PreparationPurchaseUnavailable");
        Assert.Equal(0, winner.TraderStock!.Items.Single(item => item.CatalogId == offeredWeapon).Quantity);
        Assert.All(new[] { winner, loser }, projection =>
        {
            Assert.True(projection.Coins >= 0);
            Assert.Equal(projection.OwnedWeapons!.Count,
                projection.OwnedWeapons.Select(weapon => weapon.InstanceId).Distinct(StringComparer.Ordinal).Count());
            Assert.All(projection.SupplyStock!, supply => Assert.True(supply.Quantity >= 0));
        });

        var decisionCount = winner.LatestPreparationDecisions!.Count + loser.LatestPreparationDecisions!.Count;
        coordinator.AdvanceRaids();
        coordinator.AdvanceRaids();
        Assert.Equal(decisionCount,
            coordinator.GetHandlerProjection(winnerToken).LatestPreparationDecisions!.Count
            + coordinator.GetHandlerProjection(loserToken).LatestPreparationDecisions!.Count);
    }

    [Fact]
    public void EarlierArrivalWinsEvenWhenItsRaiderIdSortsLater()
    {
        var arrivedAt = DateTimeOffset.Parse("2026-10-19T19:40:00Z");
        var (coordinator, clock, raiderOne, raiderTwo, offeredWeapon) =
            StartTraderContest(arrivedAt, traderSeed: 83);
        var one = coordinator.GetHandlerProjection(raiderOne);
        var two = coordinator.GetHandlerProjection(raiderTwo);
        var higherIdToken = string.CompareOrdinal(one.PlayerId, two.PlayerId) > 0 ? raiderOne : raiderTwo;
        var lowerIdToken = higherIdToken == raiderOne ? raiderTwo : raiderOne;

        coordinator.ResetPreparationForTesting(lowerIdToken, 1_000, arrivedAt.AddSeconds(10));
        coordinator.ResetPreparationForTesting(higherIdToken, 1_000, arrivedAt);
        clock.Now = arrivedAt.AddMinutes(9);
        coordinator.AdvanceRaids();

        Assert.Contains(coordinator.GetHandlerProjection(higherIdToken).OwnedWeapons!,
            weapon => weapon.CatalogId == offeredWeapon);
        Assert.DoesNotContain(coordinator.GetHandlerProjection(lowerIdToken).OwnedWeapons!,
            weapon => weapon.CatalogId == offeredWeapon);
    }

    [Fact]
    public void ReadyUpHurryCannotLetALaterArrivalTakeAnEarlierRaidersReservedOpportunity()
    {
        var arrivedAt = DateTimeOffset.Parse("2026-10-19T19:40:00Z");
        var (coordinator, clock, raiderOne, raiderTwo, offeredWeapon) =
            StartTraderContest(arrivedAt, traderSeed: 83);
        var one = coordinator.GetHandlerProjection(raiderOne);
        var two = coordinator.GetHandlerProjection(raiderTwo);
        var earlierToken = string.CompareOrdinal(one.PlayerId, two.PlayerId) < 0 ? raiderOne : raiderTwo;
        var laterToken = earlierToken == raiderOne ? raiderTwo : raiderOne;
        coordinator.SetSignalForTesting(laterToken, 5);
        clock.Now = arrivedAt.AddSeconds(70);

        var hurried = coordinator.ReadyUp(laterToken, new("hurry-contested-stock"));
        var retry = coordinator.ReadyUp(laterToken, new("hurry-contested-stock"));

        Assert.Contains(coordinator.GetHandlerProjection(earlierToken).OwnedWeapons!,
            weapon => weapon.CatalogId == offeredWeapon);
        Assert.DoesNotContain(hurried.OwnedWeapons!, weapon => weapon.CatalogId == offeredWeapon);
        Assert.Equal(hurried.Coins, retry.Coins);
        var offeredWeaponName = ShopCatalog.Weapons.Single(weapon => weapon.Id == offeredWeapon).Name;
        Assert.Single(retry.LatestPreparationDecisions!, decision =>
            decision.Kind == "PreparationPurchaseUnavailable"
            && decision.Text.Contains(offeredWeaponName, StringComparison.Ordinal));
    }

    [Fact]
    public void ErrandAfterCycleBoundaryRevalidatesAgainstTheNewCycle()
    {
        const int seed = 83;
        var epoch = DateTimeOffset.UnixEpoch;
        var boundary = epoch.AddTicks(TraderStockGenerator.CycleDuration.Ticks * 100);
        var arrivedAt = boundary.AddMinutes(-1);
        var clock = new ManualTime(arrivedAt);
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 0, traderSeed: seed);
        var enrolled = coordinator.EnrollHandler("raider-one", "boundary");
        var oldCycleWeapon = new TraderStockGenerator(seed, clock)
            .Generate(arrivedAt, ShopCatalog.Weapons, ShopCatalog.Supplies)
            .Items.Single(item => item.Category == "Weapon" && item.Quantity == 1).CatalogId;
        var nextCycleWeapon = new TraderStockGenerator(seed, clock)
            .Generate(boundary, ShopCatalog.Weapons, ShopCatalog.Supplies)
            .Items.Single(item => item.Category == "Weapon" && item.Quantity == 1).CatalogId;
        Assert.NotEqual(oldCycleWeapon, nextCycleWeapon);

        coordinator.ResetPreparationForTesting(enrolled.AccessToken, 1_000, arrivedAt);
        clock.Now = arrivedAt.AddMinutes(9);
        coordinator.AdvanceRaids();

        var projection = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.Equal(100, projection.TraderStock!.CycleIndex);
        Assert.Contains(projection.OwnedWeapons!, weapon => weapon.CatalogId == nextCycleWeapon);
        Assert.DoesNotContain(projection.OwnedWeapons!, weapon => weapon.CatalogId == oldCycleWeapon);
    }

    [Fact]
    public void FieldWeaponDropsStayTraderOnlyForTemptrestAndNearFivePercentCombined()
    {
        var catalog = LootCatalog.Default;
        var sampleSeeds = 10_000;
        var totalDraws = 0;
        var aspperigoDrops = 0;
        var verneriderDrops = 0;
        var temptrestDrops = 0;
        foreach (var seed in Enumerable.Range(0, sampleSeeds))
        {
            foreach (var opportunity in catalog.Opportunities)
            {
                var generated = catalog.CreateOpportunityItems(
                    $"sample-{seed}", seed, opportunity.Id, "High", 0.9, 0.8);
                totalDraws += generated.Count;
                aspperigoDrops += generated.Count(item => item.Item.CatalogId == "aspperigo");
                verneriderDrops += generated.Count(item => item.Item.CatalogId == "vernerider");
                temptrestDrops += generated.Count(item => item.Item.CatalogId == "temptrest_ar");
            }
        }

        var combinedRate = (aspperigoDrops + verneriderDrops) / (double)totalDraws;
        output.WriteLine(
            $"Measured combined field weapon rate: {combinedRate:P2} ({totalDraws:N0} draws, {sampleSeeds:N0} seeds).");
        Assert.InRange(totalDraws, 100_000, int.MaxValue);
        Assert.InRange(combinedRate, 0.04, 0.07);
        Assert.True(aspperigoDrops > 0);
        Assert.True(verneriderDrops > 0);
        Assert.Equal(0, temptrestDrops);
    }

    private static (RaidCoordinator Coordinator, ManualTime Clock, string One, string Two, string WeaponId)
        StartTraderContest(DateTimeOffset arrivedAt, int traderSeed)
    {
        var clock = new ManualTime(arrivedAt);
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 0, traderSeed: traderSeed);
        var one = coordinator.EnrollHandler("raider-one", "one");
        var two = coordinator.EnrollHandler("raider-two", "two");
        var offeredWeapon = new TraderStockGenerator(traderSeed, clock)
            .Generate(arrivedAt, ShopCatalog.Weapons, ShopCatalog.Supplies)
            .Items.Single(item => item.Category == "Weapon" && item.Quantity == 1).CatalogId;
        coordinator.ResetPreparationForTesting(one.AccessToken, 1_000, arrivedAt);
        coordinator.ResetPreparationForTesting(two.AccessToken, 1_000, arrivedAt);
        return (coordinator, clock, one.AccessToken, two.AccessToken, offeredWeapon);
    }
}
