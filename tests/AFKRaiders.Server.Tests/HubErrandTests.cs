using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class HubErrandTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-18T12:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly string[] ErrandKinds =
    [
        "PreparationRecover", "PreparationSale", "PreparationRepair", "PreparationWeaponPurchase",
        "PreparationWeaponFallback", "PreparationWeaponEquipped", "PreparationSupplyPurchase",
        "PreparationSuppliesStaged", "PreparationReady", "PreparationRest"
    ];

    private static (RaidCoordinator Coordinator, ManualTime Clock, string Token, HandlerProjection Returned) RunToReturn()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 0, traderSeed: 0);
        var handler = coordinator.EnrollHandler("raider-one", "enroll");
        coordinator.ReadyUp(handler.AccessToken, new("go"));
        HandlerProjection view = coordinator.GetHandlerProjection(handler.AccessToken);
        for (var i = 0; i < 3000 && (view.CurrentRaidId is null || view.IsHome); i++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            view = coordinator.GetHandlerProjection(handler.AccessToken);
        }
        Assert.False(view.IsHome);
        for (var i = 0; i < 3000 && !view.IsHome; i++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            view = coordinator.GetHandlerProjection(handler.AccessToken);
        }
        Assert.True(view.IsHome);
        return (coordinator, clock, handler.AccessToken, view);
    }

    private static HandlerJournalEntryView[] Errands(HandlerProjection view, DateTimeOffset after) =>
        view.FullJournal!.Where(e => e.At >= after && ErrandKinds.Contains(e.Kind)).ToArray();

    [Fact]
    public void ReturnErrandsCommitAtScheduledTimesNotWhenPolled()
    {
        var (coordinator, clock, token, returned) = RunToReturn();
        var arrivedAt = returned.LastReturnSummary!.ExtractedAt;
        var deadline = returned.HomeAvailableAt!.Value;
        Assert.Equal(arrivedAt.AddMinutes(10), deadline);

        // Polling the projection must never advance errands.
        var before = Errands(returned, arrivedAt).Length;
        clock.Now = arrivedAt.AddMinutes(9);
        Assert.Equal(before, Errands(coordinator.GetHandlerProjection(token), arrivedAt).Length);

        coordinator.AdvanceRaids();
        var view = coordinator.GetHandlerProjection(token);
        var errands = Errands(view, arrivedAt);
        Assert.Equal("PreparationRecover", errands[0].Kind);
        Assert.Equal(arrivedAt.AddMinutes(1), errands[0].At);
        Assert.Equal("PreparationRest", errands[^1].Kind);
        Assert.Equal("PreparationReady", errands[^2].Kind);
        Assert.True(errands[^1].At <= deadline.AddMinutes(-2));
        for (var i = 1; i < errands.Length; i++)
        {
            Assert.True(errands[i].At > errands[i - 1].At, $"{errands[i].Kind} must follow {errands[i - 1].Kind}");
            Assert.True(errands[i].At - errands[i - 1].At <= TimeSpan.FromSeconds(45));
        }
        Assert.Single(errands, e => e.Kind == "PreparationReady");
        Assert.StartsWith("Resting at the ", view.HubErrand);
        Assert.Contains(view.HubResidents!, r => r.Slot == "raider-one" && r.Status == view.HubErrand);
        Assert.DoesNotContain(view.FullJournal!, e => e.Kind == "HubActivity" && e.At > arrivedAt && e.At <= errands[^1].At);

        var count = view.FullJournal!.Count;
        coordinator.AdvanceRaids();
        Assert.Equal(count, coordinator.GetHandlerProjection(token).FullJournal!.Count);
    }

    [Fact]
    public void ErrandsAreIncrementalAndShowOnlyCommittedStops()
    {
        var (coordinator, clock, token, returned) = RunToReturn();
        var arrivedAt = returned.LastReturnSummary!.ExtractedAt;
        clock.Now = arrivedAt.AddSeconds(59);
        coordinator.AdvanceRaids();
        var early = coordinator.GetHandlerProjection(token);
        Assert.Empty(Errands(early, arrivedAt));
        Assert.Empty(early.LatestPreparationDecisions!);
        Assert.Null(early.HubErrand);
        var coins = early.Coins;
        var stash = early.StashCount;

        clock.Now = arrivedAt.AddMinutes(1);
        coordinator.AdvanceRaids();
        var recovering = coordinator.GetHandlerProjection(token);
        Assert.Equal("Collecting themselves after the raid", recovering.HubErrand);
        Assert.Equal("PreparationRecover", Assert.Single(recovering.LatestPreparationDecisions!).Kind);
        Assert.Equal(coins, recovering.Coins);
        Assert.Equal(stash, recovering.StashCount);
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalErrandTimelines()
    {
        static (string Kind, TimeSpan Offset, string Text)[] Timeline()
        {
            var (coordinator, clock, token, returned) = RunToReturn();
            var arrivedAt = returned.LastReturnSummary!.ExtractedAt;
            clock.Now = arrivedAt.AddMinutes(9);
            coordinator.AdvanceRaids();
            return Errands(coordinator.GetHandlerProjection(token), arrivedAt)
                .Select(e => (e.Kind, e.At - arrivedAt, e.Text)).ToArray();
        }

        Assert.Equal(Timeline(), Timeline());
    }

    [Fact]
    public void ReadyUpHurriesRemainingErrandsOnceAndSkipsRest()
    {
        var (coordinator, clock, token, returned) = RunToReturn();
        var arrivedAt = returned.LastReturnSummary!.ExtractedAt;
        coordinator.SetSignalForTesting(token, 5);
        clock.Now = arrivedAt.AddSeconds(70);
        coordinator.AdvanceRaids();
        var ready = coordinator.ReadyUp(token, new("hurry"));
        Assert.Equal("Hurrying to the Hatch", ready.HubErrand);
        var errands = Errands(ready, arrivedAt);
        Assert.Single(errands, e => e.Kind == "PreparationReady");
        Assert.DoesNotContain(errands, e => e.Kind == "PreparationRest");
        Assert.All(errands.Where(e => e.Kind != "PreparationRecover"), e => Assert.True(e.At <= clock.Now));
        Assert.True(ready.Coins >= 0);

        var retry = coordinator.ReadyUp(token, new("hurry"));
        Assert.Equal(ready.Coins, retry.Coins);
        Assert.Equal(errands.Length, Errands(retry, arrivedAt).Length);
        Assert.Single(retry.Comms, e => e.Kind == "HandlerReadyUp" && e.At == clock.Now);

        clock.Now += TimeSpan.FromSeconds(30);
        coordinator.AdvanceRaids();
        var admitted = coordinator.GetHandlerProjection(token);
        Assert.False(admitted.IsHome);
        Assert.Null(admitted.HubErrand);
        Assert.Equal(errands.Length, Errands(admitted, arrivedAt).Length);
    }

    [Fact]
    public void AbsentHandlerRaiderStillFinishesErrandsAndDeploysAfterDeadline()
    {
        var (coordinator, clock, token, returned) = RunToReturn();
        var deadline = returned.HomeAvailableAt!.Value;
        clock.Now = deadline.AddSeconds(-1);
        coordinator.AdvanceRaids();
        var waiting = coordinator.GetHandlerProjection(token);
        Assert.True(waiting.IsHome);
        Assert.Contains(waiting.LatestPreparationDecisions!, d => d.Kind == "PreparationReady");
        clock.Now = deadline.AddSeconds(30);
        coordinator.AdvanceRaids();
        Assert.False(coordinator.GetHandlerProjection(token).IsHome);
    }
}
