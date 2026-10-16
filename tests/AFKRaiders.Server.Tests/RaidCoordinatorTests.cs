using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class RaidCoordinatorTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-11T12:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void TwoPlayersShareTheWorldButHavePrivateInventoriesAndDeploymentEvents()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var host = coordinator.CreateRaid("raider-one");
        var guest = coordinator.JoinRaid(host.InviteCode, "raider-two");
        clock.Now += TimeSpan.FromSeconds(45);
        coordinator.AdvanceRaids();
        var one = coordinator.GetProjection(host.RaidId, host.AccessToken);
        var two = coordinator.GetProjection(guest.RaidId, guest.AccessToken);

        Assert.Equal(host.RaidId, guest.RaidId);
        Assert.Equal(one.Crate, two.Crate);
        Assert.Equal(one.Robot.Health, two.Robot.Health);
        Assert.Equal(one.Robot.Status, two.Robot.Status);
        Assert.Equal(one.Revision, two.Revision);
        Assert.Equal(2, one.Players.Count);
        Assert.Single(one.MyInventory);
        Assert.Single(two.MyInventory);
        Assert.NotEqual(one.MyInventory[0].InstanceId, two.MyInventory[0].InstanceId);
        Assert.DoesNotContain(one.Events, item => item.Kind == "Deployment" && item.ParticipantIds.Contains(guest.PlayerId));
    }

    [Fact]
    public void ServerClockRunsThroughExtractionWithoutReadsOrConnectedBrowsers()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var host = coordinator.CreateRaid("raider-one");
        var guest = coordinator.JoinRaid(host.InviteCode, "raider-two");
        clock.Now += TimeSpan.FromMinutes(3);

        Assert.Single(coordinator.AdvanceRaids());
        var one = coordinator.GetProjection(host.RaidId, host.AccessToken);
        var two = coordinator.GetProjection(guest.RaidId, guest.AccessToken);
        Assert.Equal("Completed", one.Status);
        Assert.Empty(one.MyInventory);
        Assert.Empty(two.MyInventory);
        var extracted = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
        var expectedLootCount = one.Robot.Status == "Defeated" ? 6 : 4;
        Assert.Equal(expectedLootCount, extracted.Length);
        Assert.Equal(expectedLootCount, extracted.Select(item => item.InstanceId).Distinct().Count());
        Assert.Equal(one.Robot.Health, two.Robot.Health);
        Assert.Equal(one.Robot.Status, two.Robot.Status);
        Assert.Empty(coordinator.AdvanceRaids());
    }

    [Fact]
    public void ReadsDoNotDriveSimulationAndConcurrentClockCallsDoNotRepeatSettlement()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var host = coordinator.CreateRaid("raider-one");
        clock.Now += TimeSpan.FromMinutes(3);
        Assert.Equal("Preparing", coordinator.GetProjection(host.RaidId, host.AccessToken).Status);
        Parallel.For(0, 10, _ => coordinator.AdvanceRaids());
        var result = coordinator.GetProjection(host.RaidId, host.AccessToken);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(result.Robot.Status == "Defeated" ? 6 : 4, result.MyExtractedLoot.Count);
        Assert.Single(result.Events, item => item.Kind == "Extracted");
    }

    [Fact]
    public void APlayerCannotReadOrSubscribeToAnotherRaid()
    {
        var coordinator = new RaidCoordinator(new ManualTime(), fastPacing: true);
        var first = coordinator.CreateRaid("raider-one");
        var second = coordinator.CreateRaid("raider-two");
        Assert.Throws<RaidAccessException>(() => coordinator.GetProjection(second.RaidId, first.AccessToken));
        Assert.Throws<RaidAccessException>(() => coordinator.GetRaidForConnection(first.AccessToken, second.RaidId));
        Assert.Throws<RaidAccessException>(() => coordinator.GetProjection(first.RaidId, "invalid-token"));
        Assert.Throws<RaidAccessException>(() => coordinator.GetProjection(first.RaidId, ""));
    }

    [Fact]
    public void InvalidInvitesIdentitiesAndDuplicateSlotsAreRejected()
    {
        var coordinator = new RaidCoordinator(new ManualTime(), fastPacing: true);
        var host = coordinator.CreateRaid("raider-one");
        Assert.Throws<RaidCommandException>(() => coordinator.CreateRaid("pretend-raider"));
        Assert.Throws<RaidAccessException>(() => coordinator.JoinRaid("incorrect", "raider-two"));
        Assert.Throws<RaidAccessException>(() => coordinator.JoinRaid(host.InviteCode, "raider-one"));
        coordinator.JoinRaid(host.InviteCode.ToLowerInvariant(), "raider-two");
        Assert.Throws<RaidAccessException>(() => coordinator.JoinRaid(host.InviteCode, "raider-two"));
    }

    [Fact]
    public void JoiningAtDepartureIsRejectedEvenBeforeTheClockServiceRuns()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var host = coordinator.CreateRaid("raider-one");
        clock.Now += RaidSimulation.AdmissionWindow;
        var exception = Assert.Throws<RaidAccessException>(() => coordinator.JoinRaid(host.InviteCode, "raider-two"));
        Assert.Equal("RaidAlreadyDeparted", exception.Reason);
        coordinator.AdvanceRaids();
        Assert.Single(coordinator.GetProjection(host.RaidId, host.AccessToken).Players);
    }

    [Fact]
    public void EnrollmentRetriesReturnTheSameHandlerWithoutDuplicatingDuty()
    {
        var coordinator = new RaidCoordinator(new ManualTime(), fastPacing: true);

        var first = coordinator.EnrollHandler("raider-one", "enrollment-1");
        var retry = coordinator.EnrollHandler("raider-one", "enrollment-1");

        Assert.Equal(first.AccessToken, retry.AccessToken);
        Assert.Equal(first.Projection.CurrentRaidId, retry.Projection.CurrentRaidId);
        Assert.Single(retry.Projection.Comms, item => item.Kind == "DutyEnabled");
        Assert.Throws<RaidAccessException>(() => coordinator.EnrollHandler("raider-one", "different-request"));
    }

    [Fact]
    public void AdmissionPulseAddsLateActualRaiderToOngoingWorldAndIsIdempotent()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var first = coordinator.EnrollHandler("raider-one", "first-enrollment");
        var second = coordinator.EnrollHandler("raider-two", "second-enrollment");
        var raidId = Assert.IsType<string>(first.Projection.CurrentRaidId);

        Assert.Null(second.Projection.CurrentRaidId);
        var homeResidents = Assert.Single(second.Projection.HubResidents!);
        Assert.Equal("Dev \"Wet Socks\" Kowalski", homeResidents.DisplayName);
        Assert.Contains("Desperanza", homeResidents.Status);
        clock.Now += TimeSpan.FromSeconds(29);
        coordinator.AdvanceRaids();
        Assert.Null(coordinator.GetHandlerProjection(second.AccessToken).CurrentRaidId);

        clock.Now += TimeSpan.FromSeconds(1);
        var revisions = coordinator.AdvanceRaids();
        var firstView = coordinator.GetHandlerProjection(first.AccessToken);
        var secondView = coordinator.GetHandlerProjection(second.AccessToken);

        Assert.Contains(revisions, revision => revision.RaidId == raidId);
        Assert.Equal(raidId, secondView.CurrentRaidId);
        Assert.Equal(raidId, firstView.Raid?.RaidId);
        Assert.Equal(firstView.Raid?.Revision, secondView.Raid?.Revision);
        Assert.Equal(2, secondView.Raid?.Players.Count);
        Assert.Contains(secondView.Comms, item => item.Kind == "RaidAdmission" && item.At == clock.Now);
        var repeated = coordinator.AdvanceRaids();
        Assert.True(repeated.Count == 0, string.Join(" | ",
            coordinator.GetHandlerProjection(first.AccessToken).Raid?.Events.Select(item =>
                $"{item.At:O}:{item.Kind}:{item.Text}") ?? []));
    }

    [Fact]
    public void DutyPauseOnlyAffectsFutureAdmissionAndHandlerCanMonitorWithoutDrivingRaid()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var first = coordinator.EnrollHandler("raider-one", "first-enrollment");
        var second = coordinator.EnrollHandler("raider-two", "second-enrollment");
        coordinator.SetDuty(second.AccessToken, false);
        var initialRaidId = Assert.IsType<string>(first.Projection.CurrentRaidId);

        clock.Now += TimeSpan.FromSeconds(30);
        coordinator.AdvanceRaids();
        var paused = coordinator.GetHandlerProjection(second.AccessToken);
        Assert.Null(paused.CurrentRaidId);
        Assert.Equal("At Home / Paused", paused.Status);

        coordinator.SetDuty(second.AccessToken, true);
        Assert.Null(coordinator.GetHandlerProjection(second.AccessToken).CurrentRaidId);
        clock.Now += TimeSpan.FromSeconds(30);
        var revisions = coordinator.AdvanceRaids();

        var firstView = coordinator.GetHandlerProjection(first.AccessToken);
        var secondView = coordinator.GetHandlerProjection(second.AccessToken);
        Assert.Contains(revisions, revision => revision.RaidId == initialRaidId);
        Assert.Equal(initialRaidId, secondView.CurrentRaidId);
        Assert.Equal(2, firstView.Raid?.Players.Count);
    }

    [Fact]
    public void ReturnedRaiderReentersAfterHomeIntervalOnAdmissionPulse()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var handler = coordinator.EnrollHandler("raider-one", "first-enrollment");
        var firstRaidId = Assert.IsType<string>(handler.Projection.CurrentRaidId);

        DateTimeOffset? homeAvailableAt = null;
        for (var index = 0; index < 240 && !homeAvailableAt.HasValue; index++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            homeAvailableAt = coordinator.GetHandlerProjection(handler.AccessToken).HomeAvailableAt;
        }

        Assert.NotNull(homeAvailableAt);
        Assert.Null(coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId);
        Assert.Equal("At Home / Cooling Down",
            coordinator.GetHandlerProjection(handler.AccessToken).Status);
        Assert.Contains(coordinator.GetHandlerProjection(handler.AccessToken).HubResidents!,
            resident => resident.Slot == "raider-one" && resident.Status == "Cooling down in Desperanza");

        clock.Now = homeAvailableAt.Value - TimeSpan.FromSeconds(1);
        coordinator.AdvanceRaids();
        Assert.Null(coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId);

        string? nextRaidId = null;
        for (var index = 0; index < 31 && nextRaidId is null; index++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            nextRaidId = coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId;
        }

        Assert.NotNull(nextRaidId);
        Assert.NotEqual(firstRaidId, nextRaidId);
        Assert.Contains(coordinator.GetHandlerProjection(handler.AccessToken).Comms,
            item => item.Kind == "RaidAdmission" && item.Text.Contains("autonomously joins", StringComparison.Ordinal));
        Assert.Contains(coordinator.GetHandlerProjection(handler.AccessToken).Comms,
            item => item.Kind == "HubReady" && item.At == homeAvailableAt.Value);
    }

    [Fact]
    public void OfflineHandlerDoesNotPauseSettlementAndSecuredLootRemainsPrivateAfterReturn()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var handler = coordinator.EnrollHandler("raider-one", "first-enrollment");

        for (var index = 0; index < 24; index++)
        {
            clock.Now += TimeSpan.FromSeconds(5);
            coordinator.AdvanceRaids();
        }

        var returned = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.Contains(returned.Comms, item => item.Kind == "ReturnedHome");
        Assert.NotEmpty(returned.MyLastExtractedLoot);
        Assert.Equal(returned.MyLastExtractedLoot.Select(item => item.InstanceId).Distinct().Count(),
            returned.MyLastExtractedLoot.Count);
    }

    [Fact]
    public void HandlerCannotReadAnotherIdentityOrUseHandlerTokenForLegacyRaid()
    {
        var coordinator = new RaidCoordinator(new ManualTime(), fastPacing: true);
        var first = coordinator.EnrollHandler("raider-one", "first-enrollment");
        var otherRaid = coordinator.CreateRaid("raider-two");

        Assert.Throws<RaidAccessException>(() => coordinator.GetProjection(otherRaid.RaidId, first.AccessToken));
        Assert.Throws<RaidAccessException>(() => coordinator.GetRaidForConnection(first.AccessToken, otherRaid.RaidId));
        Assert.Throws<RaidAccessException>(() => coordinator.GetHandlerProjection(otherRaid.AccessToken));
    }

    [Fact]
    public void CrossRaidCommsJournalPersistsAcrossMultiRaidLifecyclesWithoutDuplicates()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var handler = coordinator.EnrollHandler("raider-one", "first-enrollment");
        var firstRaidId = Assert.IsType<string>(handler.Projection.CurrentRaidId);

        // Advance through first raid until extracted and cooled down
        DateTimeOffset? homeAvailableAt = null;
        for (var index = 0; index < 240 && !homeAvailableAt.HasValue; index++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            homeAvailableAt = coordinator.GetHandlerProjection(handler.AccessToken).HomeAvailableAt;
        }

        Assert.NotNull(homeAvailableAt);
        var projAfterRaid1 = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.NotNull(projAfterRaid1.FullJournal);
        var raid1JournalCount = projAfterRaid1.FullJournal.Count;
        Assert.True(raid1JournalCount > 0);
        Assert.Contains(projAfterRaid1.FullJournal, e => e.RaidId == firstRaidId);
        Assert.Contains(projAfterRaid1.FullJournal, e => e.Kind == "ReturnedHome");

        // Advance past cooldown into second raid
        clock.Now = homeAvailableAt.Value;
        string? secondRaidId = null;
        for (var index = 0; index < 40 && secondRaidId is null; index++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            secondRaidId = coordinator.GetHandlerProjection(handler.AccessToken).CurrentRaidId;
        }

        Assert.NotNull(secondRaidId);
        Assert.NotEqual(firstRaidId, secondRaidId);

        // Advance 25 seconds into second raid
        clock.Now += TimeSpan.FromSeconds(25);
        coordinator.AdvanceRaids();

        var projDuringRaid2 = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.NotNull(projDuringRaid2.FullJournal);
        var journal = projDuringRaid2.FullJournal;

        // Verify multi-raid persistence
        Assert.True(journal.Count > raid1JournalCount);
        Assert.Contains(journal, e => e.RaidId == firstRaidId);
        Assert.Contains(journal, e => e.RaidId == secondRaidId);
        Assert.Contains(journal, e => e.Kind == "RaidAdmission");

        // Verify strict monotonicity and unique source IDs (no duplicates)
        for (var i = 1; i < journal.Count; i++)
        {
            Assert.Equal(i + 1, journal[i].Sequence);
            Assert.True(journal[i].At >= journal[i - 1].At);
        }
        var sourceIds = journal.Select(e => e.SourceEventId).ToList();
        Assert.Equal(sourceIds.Distinct().Count(), sourceIds.Count);

        // Polling multiple times should not add duplicates
        var pollCountBefore = journal.Count;
        var polled1 = coordinator.GetHandlerProjection(handler.AccessToken);
        var polled2 = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.Equal(pollCountBefore, polled1.FullJournal?.Count);
        Assert.Equal(pollCountBefore, polled2.FullJournal?.Count);
    }

    [Fact]
    public void GetHandlerCommsSupportsIncrementalAndPagedReads()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var handler = coordinator.EnrollHandler("raider-one", "first-enrollment");

        // Advance 35 seconds to generate several raid events
        clock.Now += TimeSpan.FromSeconds(35);
        coordinator.AdvanceRaids();

        var fullProj = coordinator.GetHandlerProjection(handler.AccessToken);
        var totalEvents = fullProj.FullJournal!.Count;
        Assert.True(totalEvents >= 6);

        // Page 1: limit 3
        var page1 = coordinator.GetHandlerComms(handler.AccessToken, sinceSequence: 0, limit: 3);
        Assert.Equal(3, page1.Entries.Count);
        Assert.Equal(totalEvents, page1.TotalCount);
        Assert.Equal(3, page1.NextSequence);
        Assert.True(page1.HasMore);

        // Page 2: sinceSequence = 3, limit 3
        var page2 = coordinator.GetHandlerComms(handler.AccessToken, sinceSequence: 3, limit: 3);
        Assert.Equal(3, page2.Entries.Count);
        Assert.Equal(4, page2.Entries[0].Sequence);
        Assert.Equal(6, page2.NextSequence);

        // Read remaining
        var pageRemaining = coordinator.GetHandlerComms(handler.AccessToken, sinceSequence: 6, limit: 100);
        Assert.Equal(totalEvents - 6, pageRemaining.Entries.Count);
        Assert.False(pageRemaining.HasMore);
    }

    [Fact]
    public void PostRaidDebriefSummaryMatchesAuthoritativeExtractedLoot()
    {
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var handler = coordinator.EnrollHandler("raider-one", "first-enrollment");

        // Advance until returned home
        for (var index = 0; index < 240; index++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            if (coordinator.GetHandlerProjection(handler.AccessToken).LastReturnSummary is not null)
                break;
        }

        var projection = coordinator.GetHandlerProjection(handler.AccessToken);
        Assert.NotNull(projection.LastReturnSummary);
        var summary = projection.LastReturnSummary;

        Assert.Equal(projection.MyLastExtractedLoot.Count, summary.SecuredLootCount);
        Assert.Equal(projection.MyLastExtractedLoot.Sum(i => i.Value), summary.TotalValue);
        Assert.Equal(projection.MyLastExtractedLoot.Count, summary.Items.Count);
        Assert.Contains("Mira \"Refund\" Malone returned to Desperanza", summary.DebriefSummary);
        Assert.NotEmpty(summary.Highlights);
    }
}
