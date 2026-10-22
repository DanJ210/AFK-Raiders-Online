using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class HomeStashTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-12T10:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void StashUnderCapacityRetainsAllItemsWithoutOverflow()
    {
        var stash = new HomeStash();
        var haul = new[]
        {
            new ItemView("item-1", "scrap_metal_basic", "Basic Scrap Metal", 8, "Bent metal"),
            new ItemView("item-2", "robot_finger", "Precision Actuator Finger", 18, "Articulated copper")
        };

        var result = stash.TransferHaul(haul);

        Assert.Equal(2, stash.Count);
        Assert.Equal(26, stash.TotalValue);
        Assert.Equal(2, result.IncomingRetainedCount);
        Assert.Empty(result.AutoSoldItems);
        Assert.Equal(0, result.OverflowCoinsGained);
    }

    [Fact]
    public void StashBoundaryAt119And120RetainsAllUnits()
    {
        var initial = Enumerable.Range(1, 115)
            .Select(i => new ItemView($"item-{i:D3}", "scrap_metal_basic", "Basic Scrap Metal", 10, "Flavor"))
            .ToArray();
        var stash = new HomeStash(initial);

        // Add 4 items -> total 119
        var haul4 = Enumerable.Range(116, 4)
            .Select(i => new ItemView($"item-{i:D3}", "scrap_metal_bent", "Bent Scrap Metal", 10, "Flavor"))
            .ToArray();
        var result119 = stash.TransferHaul(haul4);

        Assert.Equal(119, stash.Count);
        Assert.Equal(1190, stash.TotalValue);
        Assert.Equal(4, result119.IncomingRetainedCount);
        Assert.Empty(result119.AutoSoldItems);
        Assert.Equal(0, result119.OverflowCoinsGained);

        // Add 1 item -> total exactly 120
        var haul1 = new[] { new ItemView("item-120", "robot_toe", "Standard Tread Sprocket", 15, "Flavor") };
        var result120 = stash.TransferHaul(haul1);

        Assert.Equal(120, stash.Count);
        Assert.Equal(1205, stash.TotalValue);
        Assert.Equal(1, result120.IncomingRetainedCount);
        Assert.Empty(result120.AutoSoldItems);
        Assert.Equal(0, result120.OverflowCoinsGained);
    }

    [Fact]
    public void StashBoundaryAt121SellsLowestValueItemAndMaintainsConservation()
    {
        // 120 items: 10 of value 8, 110 of value 20
        var initial = new List<ItemView>();
        for (int i = 1; i <= 10; i++)
        {
            initial.Add(new ItemView($"cheap-{i:D2}", "scrap_metal_basic", "Cheap Scrap", 8, "Flavor"));
        }
        for (int i = 11; i <= 120; i++)
        {
            initial.Add(new ItemView($"valuable-{i:D3}", "robot_knuckle_joint", "Valuable Part", 20, "Flavor"));
        }
        var stash = new HomeStash(initial);
        var initialValue = stash.TotalValue; // 10*8 + 110*20 = 80 + 2200 = 2280

        // Incoming high-value item (value 35) pushes count to 121
        var incoming = new[] { new ItemView("incoming-1", "circuit_bundle_messy", "Bundle", 35, "Rare") };
        var incomingValue = 35;

        var result = stash.TransferHaul(incoming);

        // Assert capacity
        Assert.Equal(HomeStash.MaxCapacity, stash.Count);
        // Exactly 1 item auto-sold
        Assert.Single(result.AutoSoldItems);
        var sold = result.AutoSoldItems[0];
        // Must be the lowest value item (cheap-01 because value 8 < 20 < 35, and cheap-01 tiebreak by InstanceId)
        Assert.Equal("cheap-01", sold.InstanceId);
        Assert.Equal(8, sold.Value);
        Assert.Equal(8, result.OverflowCoinsGained);

        // Unit conservation: retained + sold == previous + incoming
        Assert.Equal(120 + 1, stash.Count + result.AutoSoldItems.Count);

        // Value conservation: stash value after + overflow coins == initial stash value + incoming value
        Assert.Equal(initialValue + incomingValue, stash.TotalValue + result.OverflowCoinsGained);
        Assert.Equal(2280 + 35 - 8, stash.TotalValue);
    }

    [Fact]
    public void EqualValueTiesResolveUsingStableInstanceIdOrder()
    {
        // 119 items of value 10
        var initial = Enumerable.Range(1, 119)
            .Select(i => new ItemView($"fixed-{i:D3}", "scrap", "Scrap", 10, "F"))
            .ToList();
        var stash = new HomeStash(initial);

        // 3 incoming items of value 5: "z-cheap", "a-cheap", "m-cheap"
        var haul = new[]
        {
            new ItemView("z-cheap", "scrap", "Scrap", 5, "F"),
            new ItemView("a-cheap", "scrap", "Scrap", 5, "F"),
            new ItemView("m-cheap", "scrap", "Scrap", 5, "F")
        };

        // Total 122 items. Overflow = 2.
        // The 3 items of value 5 are all lower than the 119 items of value 10.
        // Sorted by Value (5) then InstanceId ("a-cheap", "m-cheap", "z-cheap").
        // Take 2: "a-cheap" and "m-cheap" are sold! "z-cheap" is retained.
        var result = stash.TransferHaul(haul);

        Assert.Equal(HomeStash.MaxCapacity, stash.Count);
        Assert.Equal(2, result.AutoSoldItems.Count);
        Assert.Equal("a-cheap", result.AutoSoldItems[0].InstanceId);
        Assert.Equal("m-cheap", result.AutoSoldItems[1].InstanceId);
        Assert.Equal(10, result.OverflowCoinsGained);
        Assert.Contains(stash.Items, item => item.InstanceId == "z-cheap");
    }

    [Fact]
    public void IncomingLootCanBeSoldIfItIsLowestValue()
    {
        // Stash full of 120 high-value items (value 50)
        var initial = Enumerable.Range(1, 120)
            .Select(i => new ItemView($"rich-{i:D3}", "gold", "Gold", 50, "F"))
            .ToList();
        var stash = new HomeStash(initial);

        // Incoming item is low-value (value 8)
        var haul = new[] { new ItemView("junk-1", "junk", "Junk", 8, "F") };
        var result = stash.TransferHaul(haul);

        Assert.Equal(HomeStash.MaxCapacity, stash.Count);
        Assert.Single(result.AutoSoldItems);
        Assert.Equal("junk-1", result.AutoSoldItems[0].InstanceId);
        Assert.Equal(8, result.OverflowCoinsGained);
        Assert.Equal(0, result.IncomingRetainedCount);
        Assert.DoesNotContain(stash.Items, item => item.InstanceId == "junk-1");
    }

    [Fact]
    public void ManualSaleByInstanceIdsDeductsUnitsAndCreditsCoins()
    {
        var stash = new HomeStash([
            new ItemView("item-1", "scrap_basic", "Basic", 8, "F"),
            new ItemView("item-2", "robot_finger", "Finger", 18, "F"),
            new ItemView("item-3", "robot_toe", "Toe", 15, "F")
        ]);

        var sellResult = stash.SellByInstanceIds(["item-1", "item-3"]);

        Assert.Equal(2, sellResult.SoldItems.Count);
        Assert.Equal(23, sellResult.CoinsGained);
        Assert.Single(stash.Items);
        Assert.Equal("item-2", stash.Items[0].InstanceId);
        Assert.Equal(18, stash.TotalValue);
    }

    [Fact]
    public void ManualSaleByInstanceIdsRejectsUnownedOrDuplicateItems()
    {
        var stash = new HomeStash([
            new ItemView("item-1", "scrap_basic", "Basic", 8, "F")
        ]);

        Assert.Throws<RaidCommandException>(() => stash.SellByInstanceIds([]));
        Assert.Throws<RaidCommandException>(() => stash.SellByInstanceIds(["item-1", "item-1"]));
        Assert.Throws<RaidCommandException>(() => stash.SellByInstanceIds(["item-999"]));
    }

    [Fact]
    public void ManualSaleByCatalogIdAndQuantityDeductsUnitsAndCreditsCoins()
    {
        var stash = new HomeStash([
            new ItemView("scrap-01", "scrap_basic", "Basic Scrap", 8, "F"),
            new ItemView("scrap-02", "scrap_basic", "Basic Scrap", 8, "F"),
            new ItemView("scrap-03", "scrap_basic", "Basic Scrap", 8, "F"),
            new ItemView("other-01", "other", "Other", 20, "F")
        ]);

        var sellResult = stash.SellByCatalogId("scrap_basic", 2);

        Assert.Equal(2, sellResult.SoldItems.Count);
        Assert.Equal(16, sellResult.CoinsGained);
        Assert.Equal(2, stash.Count);
        Assert.Equal("scrap-03", stash.Items.Single(item => item.CatalogId == "scrap_basic").InstanceId);
    }

    [Fact]
    public void ManualSaleByCatalogIdRejectsExcessQuantityOrInvalidCatalog()
    {
        var stash = new HomeStash([
            new ItemView("scrap-01", "scrap_basic", "Basic Scrap", 8, "F")
        ]);

        Assert.Throws<RaidCommandException>(() => stash.SellByCatalogId("", 1));
        Assert.Throws<RaidCommandException>(() => stash.SellByCatalogId("scrap_basic", 0));
        Assert.Throws<RaidCommandException>(() => stash.SellByCatalogId("scrap_basic", -1));
        Assert.Throws<RaidCommandException>(() => stash.SellByCatalogId("scrap_basic", 2));
        Assert.Throws<RaidCommandException>(() => stash.SellByCatalogId("nonexistent", 1));
    }

    [Fact]
    public void StashGroupsAggregateQuantitiesAndValuesForUI()
    {
        var stash = new HomeStash([
            new ItemView("scrap-1", "scrap_basic", "Basic Scrap", 8, "F1"),
            new ItemView("scrap-2", "scrap_basic", "Basic Scrap", 8, "F2"),
            new ItemView("rare-1", "rare_circuit", "Rare Circuit", 35, "F3")
        ]);

        var groups = stash.GetGroups();

        Assert.Equal(2, groups.Count);
        var rareGroup = groups.First();
        Assert.Equal("rare_circuit", rareGroup.CatalogId);
        Assert.Equal(1, rareGroup.Quantity);
        Assert.Equal(35, rareGroup.TotalValue);

        var scrapGroup = groups.Last();
        Assert.Equal("scrap_basic", scrapGroup.CatalogId);
        Assert.Equal(2, scrapGroup.Quantity);
        Assert.Equal(16, scrapGroup.TotalValue);
        Assert.Equal(2, scrapGroup.InstanceIds.Count);
    }

    [Fact]
    public void CoordinatorSellsEligibleLootDuringArrivalPreparationAndTracksCoins()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true, nextSeed: () => 1);
        var handler = coordinator.EnrollHandler("raider-one", "enrollment-g3");

        // First raid runs to completion (approx 90-110s)
        clock.Now += TimeSpan.FromSeconds(120);
        coordinator.AdvanceRaids();

        var view1 = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.Equal("Hoarder", view1.PrimaryTrait);
        Assert.InRange(view1.StashCount, 0, 3);
        Assert.True(view1.Coins >= 0);
        Assert.NotNull(view1.LastReturnSummary?.Payoff);
        Assert.True(view1.LastReturnSummary.Payoff.GrossSecuredCount > 0);
        Assert.Equal(0, view1.LastReturnSummary.Payoff.UnitsAutoSold);
        Assert.Contains(view1.LatestPreparationDecisions!, decision =>
            decision.Kind == "PreparationSale");

        // Wait out home cooldown (60s) and next admission pulse (30s) -> 90s
        clock.Now += TimeSpan.FromSeconds(90);
        coordinator.AdvanceRaids();

        var viewAdmitted = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.NotNull(viewAdmitted.CurrentRaidId);

        // Second raid runs to completion
        clock.Now += TimeSpan.FromSeconds(120);
        coordinator.AdvanceRaids();

        var view2 = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.InRange(view2.StashCount, 0, 3);
        Assert.Equal(view2.StashCount, view2.Stash?.Count);
        Assert.Equal(view2.StashValue, view2.Stash?.Sum(i => i.Value));
    }

    [Fact]
    public void PollingAndAdvancingDoesNotDoubleSettleExtractedLoot()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true, nextSeed: () => 0);
        var handler = coordinator.EnrollHandler("raider-one", "enrollment-poll");

        clock.Now += TimeSpan.FromSeconds(300);
        coordinator.AdvanceRaids();

        var initialView = coordinator.GetHandlerProjection(handler.AccessToken);
        var stashCount = initialView.StashCount;
        var coins = initialView.Coins;

        // Multiple subsequent calls to AdvanceRaids and GetHandlerProjection
        for (int i = 0; i < 5; i++)
        {
            clock.Now += TimeSpan.FromSeconds(5);
            coordinator.AdvanceRaids();
            var pollView = coordinator.GetHandlerProjection(handler.AccessToken);
            Assert.Equal(stashCount, pollView.StashCount);
            Assert.Equal(coins, pollView.Coins);
        }
    }

    [Fact]
    public void IndependentEarlyExtractorSettlesImmediatelyWithoutWaitingForOtherRaiders()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true, nextSeed: () => 0);
        var first = coordinator.EnrollHandler("raider-one", "early-1");
        var second = coordinator.EnrollHandler("raider-two", "early-2");

        // Both admitted to the same world
        clock.Now += TimeSpan.FromSeconds(30);
        coordinator.AdvanceRaids();

        var firstAdmitted = coordinator.GetHandlerProjection(first.AccessToken);
        var secondAdmitted = coordinator.GetHandlerProjection(second.AccessToken);
        Assert.NotNull(firstAdmitted.CurrentRaidId);
        Assert.Equal(firstAdmitted.CurrentRaidId, secondAdmitted.CurrentRaidId);

        // Advance to a point where the first raider has extracted but the second hasn't,
        // or check step-by-step
        for (int step = 0; step < 20; step++)
        {
            clock.Now += TimeSpan.FromSeconds(5);
            coordinator.AdvanceRaids();
            var v1 = coordinator.GetHandlerProjection(first.AccessToken);
            var v2 = coordinator.GetHandlerProjection(second.AccessToken);

            if (v1.Status.StartsWith("Returned", StringComparison.Ordinal) && v2.Status == "In Raid")
            {
                // Raider one extracted and settled immediately!
                Assert.NotNull(v1.LastReturnSummary?.Payoff);
                Assert.True(v1.LastReturnSummary.Payoff.GrossSecuredCount > 0);
                Assert.Contains(v1.LatestPreparationDecisions!, decision => decision.Kind == "PreparationSale");

                // Raider two remains in the world; its private stash is not part of this settlement.
                Assert.Null(v2.LastReturnSummary);
                break;
            }
        }

        // Advance fully to completion
        clock.Now += TimeSpan.FromSeconds(120);
        coordinator.AdvanceRaids();

        var v1Final = coordinator.GetHandlerProjection(first.AccessToken);
        var v2Final = coordinator.GetHandlerProjection(second.AccessToken);

        Assert.NotNull(v2Final.LastReturnSummary);
        Assert.NotNull(v2Final.LastReturnSummary.Payoff);
        Assert.Equal(v2Final.LastReturnSummary.Payoff.GrossSecuredCount,
            v2Final.LastReturnSummary.Payoff.UnitsRetainedInStash + v2Final.LastReturnSummary.Payoff.UnitsAutoSold);
        var v1Ids = v1Final.LastReturnSummary!.Payoff!.SecuredItems.Select(item => item.InstanceId).ToHashSet();
        var v2Ids = v2Final.LastReturnSummary.Payoff!.SecuredItems.Select(item => item.InstanceId).ToHashSet();
        Assert.Empty(v1Ids.Intersect(v2Ids));
    }

}
