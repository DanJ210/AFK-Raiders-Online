using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class HubResidenceTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-16T12:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void EnrollmentStaysHomeForExactlyTenMinutesThenQueuesWithoutHandlerInput()
    {
        var clock = new ManualTime();
        var start = clock.Now;
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 0);
        var handler = coordinator.EnrollHandler("raider-one", "enroll");
        Assert.Null(handler.Projection.CurrentRaidId);
        Assert.True(handler.Projection.IsHome);
        Assert.True(handler.Projection.CanReadyUp);
        Assert.Equal(start.AddMinutes(10), handler.Projection.HomeAvailableAt);

        clock.Now = start.AddSeconds(599);
        coordinator.AdvanceRaids();
        Assert.Null(coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId);
        clock.Now = start.AddMinutes(10);
        Assert.Null(coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId);
        coordinator.AdvanceRaids();
        var admitted = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.NotNull(admitted.CurrentRaidId);
        Assert.False(admitted.IsHome);
        Assert.False(admitted.CanReadyUp);
        Assert.Single(admitted.Comms, e => e.Kind == "RaidAdmission" && e.At == clock.Now);
        var count = admitted.FullJournal!.Count;
        coordinator.AdvanceRaids();
        Assert.Equal(count, coordinator.GetHandlerProjection(handler.AccessToken).FullJournal!.Count);
    }

    [Fact]
    public void PausedOfflineResidentsHavePrivateDeterministicCommsWithoutRewardsOrReadDrivenEvents()
    {
        var clock = new ManualTime();
        var start = clock.Now;
        var coordinator = new RaidCoordinator(clock);
        var one = coordinator.EnrollHandler("raider-one", "one");
        var two = coordinator.EnrollHandler("raider-two", "two");
        coordinator.SetDuty(one.AccessToken, false);
        coordinator.SetDuty(two.AccessToken, false);
        clock.Now = start.AddMinutes(11);
        Assert.DoesNotContain(coordinator.GetHandlerProjection(one.AccessToken).FullJournal!, e => e.Kind == "HubActivity");
        coordinator.AdvanceRaids();
        var view = coordinator.GetHandlerProjection(one.AccessToken);
        var activities = view.FullJournal!.Where(e => e.Kind == "HubActivity").ToArray();
        Assert.Equal(11, activities.Length);
        Assert.Equal(10, activities.Take(10).Select(e => e.Text).Distinct().Count());
        Assert.Equal(Enumerable.Range(1, 11).Select(i => start.AddMinutes(i)), activities.Select(e => e.At));
        Assert.All(activities, e =>
        {
            Assert.Null(e.RaidId);
            Assert.Equal("Hub", e.Channel);
            Assert.Equal("Desperanza", e.Location);
            Assert.Null(e.ItemInstanceId);
            Assert.Null(e.HealthDelta);
            Assert.Null(e.Flavor);
            Assert.Equal("CanonFallback", e.NarrativeSource);
        });
        Assert.Equal(0, view.Coins);
        Assert.Empty(view.Stash!);
        Assert.Null(view.CurrentRaidId);
        Assert.Equal(2, view.HubResidents!.Count);
        var otherJournal = coordinator.GetHandlerProjection(two.AccessToken).FullJournal!;
        Assert.DoesNotContain(otherJournal, e => activities.Any(a => a.SourceEventId == e.SourceEventId));
        coordinator.AdvanceRaids();
        Assert.Equal(view.FullJournal!.Count, coordinator.GetHandlerProjection(one.AccessToken).FullJournal!.Count);
    }

    [Fact]
    public void ReadyUpResumesDutyQueuesOnPulseAndRetriesCannotSkipANewStay()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, nextSeed: () => 0);
        var one = coordinator.EnrollHandler("raider-one", "one");
        var two = coordinator.EnrollHandler("raider-two", "two");
        coordinator.SetDuty(one.AccessToken, false);
        clock.Now += TimeSpan.FromSeconds(7);
        var ready = coordinator.ReadyUp(one.AccessToken, new("ready-one"));
        Assert.True(ready.DutyEnabled);
        Assert.False(ready.CanReadyUp);
        Assert.Null(ready.CurrentRaidId);
        Assert.Equal(clock.Now, ready.HomeAvailableAt);
        Assert.Equal(0, ready.Coins);
        Assert.Equal(2, ready.ReadyUpSignalCost);
        Assert.Equal(one.Projection.SignalCurrent - 2, ready.SignalCurrent);
        var retry = coordinator.ReadyUp(one.AccessToken, new("ready-one"));
        Assert.Single(retry.Comms, e => e.Kind == "HandlerReadyUp");
        Assert.Equal(ready.SignalCurrent, retry.SignalCurrent);
        Assert.Throws<RaidCommandException>(() => coordinator.ReadyUp(one.AccessToken, new("already-queued")));
        Assert.True(coordinator.GetHandlerProjection(two.AccessToken).CanReadyUp);
        coordinator.ReadyUp(two.AccessToken, new("ready-two"));
        clock.Now += TimeSpan.FromSeconds(23);
        coordinator.AdvanceRaids();
        var admitted = coordinator.GetHandlerProjection(one.AccessToken);
        Assert.Equal(admitted.CurrentRaidId, coordinator.GetHandlerProjection(two.AccessToken).CurrentRaidId);
        Assert.Equal(2, admitted.Raid!.Players.Count);
        Assert.Throws<RaidCommandException>(() => coordinator.ReadyUp(one.AccessToken, new("active")));
        coordinator.SetDuty(one.AccessToken, false);

        HandlerProjection returned = admitted;
        for (var i = 0; i < 1500 && !returned.IsHome; i++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            returned = coordinator.GetHandlerProjection(one.AccessToken);
        }
        Assert.True(returned.IsHome);
        Assert.Equal(returned.LastReturnSummary!.ExtractedAt.AddMinutes(10), returned.HomeAvailableAt);
        var deadline = returned.HomeAvailableAt;
        Assert.Equal(deadline, coordinator.ReadyUp(one.AccessToken, new("ready-one")).HomeAvailableAt);
        Assert.False(coordinator.GetHandlerProjection(one.AccessToken).DutyEnabled);
        clock.Now = deadline!.Value.AddSeconds(-1);
        coordinator.AdvanceRaids();
        Assert.True(coordinator.GetHandlerProjection(one.AccessToken).IsHome);
        Assert.DoesNotContain(coordinator.GetHandlerProjection(one.AccessToken).FullJournal!,
            e => e.Kind == "HubActivity" && e.At > admitted.ServerTime && e.At < returned.LastReturnSummary.ExtractedAt);
        coordinator.SetDuty(one.AccessToken, true);
        clock.Now = deadline.Value;
        coordinator.AdvanceRaids();
        clock.Now += TimeSpan.FromSeconds(30);
        coordinator.AdvanceRaids();
        Assert.False(coordinator.GetHandlerProjection(one.AccessToken).IsHome);
    }

    [Fact]
    public void InsufficientSignalDoesNotChangeDutyOrDeadlineAndRegenerationAllowsRetry()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock);
        var handler = coordinator.EnrollHandler("raider-one", "one");
        coordinator.SetDuty(handler.AccessToken, false);
        coordinator.SetSignalForTesting(handler.AccessToken, 1);
        var deadline = handler.Projection.HomeAvailableAt;
        var failure = Assert.Throws<RaidCommandException>(() =>
            coordinator.ReadyUp(handler.AccessToken, new("cost")));
        Assert.Equal("InsufficientSignal", failure.Reason);
        var rejected = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.Equal(1, rejected.SignalCurrent);
        Assert.False(rejected.DutyEnabled);
        Assert.Equal(deadline, rejected.HomeAvailableAt);
        Assert.DoesNotContain(rejected.Comms, e => e.Kind == "HandlerReadyUp");

        clock.Now += TimeSpan.FromSeconds(30);
        var ready = coordinator.ReadyUp(handler.AccessToken, new("cost"));
        Assert.Equal(0, ready.SignalCurrent);
        Assert.True(ready.DutyEnabled);
        Assert.Equal(clock.Now, ready.HomeAvailableAt);
        Assert.Equal(0, coordinator.ReadyUp(handler.AccessToken, new("cost")).SignalCurrent);
        Assert.Equal("RaiderAlreadyQueued", Assert.Throws<RaidCommandException>(() =>
            coordinator.ReadyUp(handler.AccessToken, new("second"))).Reason);
    }

    [Fact]
    public void ConcurrentRetriesSpendSignalOnlyOnce()
    {
        var coordinator = new RaidCoordinator(new ManualTime());
        var handler = coordinator.EnrollHandler("raider-one", "one");
        Parallel.For(0, 10, _ => coordinator.ReadyUp(handler.AccessToken, new("same")));
        var result = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.Equal(1, result.SignalCurrent);
        Assert.Single(result.Comms, e => e.Kind == "HandlerReadyUp");
    }

    [Fact]
    public void ReadyUpRejectsInvalidCredentialsAndCommandIds()
    {
        var coordinator = new RaidCoordinator(new ManualTime());
        var handler = coordinator.EnrollHandler("raider-one", "one");
        Assert.Throws<RaidAccessException>(() => coordinator.ReadyUp("invalid", new("ready")));
        Assert.Throws<RaidCommandException>(() => coordinator.ReadyUp(handler.AccessToken, new("")));
        Assert.Throws<RaidCommandException>(() => coordinator.ReadyUp(handler.AccessToken, new(new string('x', 101))));
        Assert.True(coordinator.GetHandlerProjection(handler.AccessToken).CanReadyUp);
    }
}
