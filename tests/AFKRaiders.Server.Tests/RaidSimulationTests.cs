using System.Text.Json;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class RaidSimulationTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-11T12:00:00Z");

    private static (RaidSimulation Simulation, RaidProjection One, RaidProjection Two) FindTwoPlayerRobotResult(
        Func<RobotEncounterView, bool> predicate)
    {
        for (var seed = 0; seed < 512; seed++)
        {
            var simulation = Create(seed);
            simulation.Advance(Start.AddMinutes(3));
            var one = simulation.Project("one", Start.AddMinutes(3));
            var two = simulation.Project("two", Start.AddMinutes(3));
            if (one.RobotEncounter is { ParticipantIds.Count: 2 } encounter && predicate(encounter))
                return (simulation, one, two);
        }
        throw new InvalidOperationException("No deterministic seed produced the requested two-player robot encounter.");
    }

    private static RaidSimulation Create(int seed, bool reverseOrder = false, bool solo = false)
    {
        var simulation = new RaidSimulation("test-raid", Start, seed, fastPacing: true);
        if (reverseOrder) simulation.AddRaider("two", "raider-two", Start);
        simulation.AddRaider("one", "raider-one", Start);
        if (!reverseOrder && !solo) simulation.AddRaider("two", "raider-two", Start);
        return simulation;
    }

    [Fact]
    public void PerRaiderDeadlinesProduceDistinctDestinationsWithoutHandlerInput()
    {
        var simulation = Create(2);
        Assert.False(simulation.Advance(Start.AddSeconds(19)));
        simulation.Advance(Start.AddSeconds(20));
        Assert.All(simulation.Project("one", Start).Players, player =>
        {
            Assert.Equal("Traveling", player.Activity);
            Assert.Equal("Market", player.Destination);
            Assert.Equal(Start.AddSeconds(28), player.ActivityEndsAt);
        });
        simulation.Advance(Start.AddSeconds(28));
        Assert.All(simulation.Project("one", Start).Players, player => Assert.Equal("Searching", player.Activity));
        simulation.Advance(Start.AddSeconds(38));
        Assert.Single(simulation.Project("one", Start).MyInventory);
        simulation.Advance(Start.AddSeconds(40));
        var players = simulation.Project("one", Start).Players;
        Assert.Equal("Salvage Yard", players[0].Destination);
        Assert.Equal("Service Depot", players[1].Destination);
        Assert.NotEqual(players[0].ActivityEndsAt, players[1].ActivityEndsAt);
        Assert.Empty(simulation.Project("one", Start).Encounters);
    }

    [Fact]
    public void SimultaneousArrivalsJoinOneWindowBeforeResolution()
    {
        var simulation = Create(0);
        simulation.Advance(Start.AddSeconds(32));
        var market = simulation.Project("one", Start).Opportunities.Single(item => item.Location == "Market");
        Assert.Equal("Searching", market.Status);
        Assert.Equal(Start.AddSeconds(42), market.ClosesAt);
        Assert.Equal(["one", "two"], market.ParticipantIds);
        Assert.All(simulation.Project("one", Start).Players, player => Assert.Equal(market.ClosesAt, player.ActivityEndsAt));
        simulation.Advance(Start.AddSeconds(42));
        Assert.Single(simulation.Project("one", Start).MyInventory);
        Assert.Single(simulation.Project("two", Start).MyInventory);
    }

    [Fact]
    public void LateAutonomousAdmissionJoinsTheSameWorldAndSettlesOnlyActualUniqueLoot()
    {
        var simulation = new RaidSimulation("ongoing", Start, 11, fastPacing: true);
        simulation.AddAutonomousRaider("one", "raider-one", Start);
        simulation.Advance(Start);
        simulation.AddAutonomousRaider("two", "raider-two", Start.AddSeconds(1));
        simulation.Advance(Start.AddSeconds(1));
        simulation.Advance(Start.AddMinutes(3));

        var one = simulation.Project("one", Start.AddMinutes(3));
        var two = simulation.Project("two", Start.AddMinutes(3));
        var actualIds = one.Players.Select(player => player.PlayerId).ToHashSet(StringComparer.Ordinal);
        var sharedEncounters = one.Encounters.Where(encounter => encounter.ParticipantIds.Contains("two")).ToArray();
        var settledLoot = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();

        Assert.Equal(2, simulation.ParticipantCount);
        Assert.Equal("ongoing", one.RaidId);
        Assert.Equal(one.RaidId, two.RaidId);
        Assert.Contains(one.Players, player => player.PlayerId == "two");
        Assert.All(sharedEncounters, encounter =>
        {
            Assert.Equal(JsonSerializer.Serialize(encounter),
                JsonSerializer.Serialize(two.Encounters.Single(item => item.EncounterId == encounter.EncounterId)));
            Assert.All(encounter.ParticipantIds, participantId => Assert.Contains(participantId, actualIds));
        });
        Assert.Equal(settledLoot.Length, settledLoot.Select(item => item.InstanceId).Distinct().Count());
        Assert.DoesNotContain(settledLoot.Select(item => item.InstanceId),
            itemId => one.MyExtractedLoot.Any(item => item.InstanceId == itemId)
                && two.MyExtractedLoot.Any(item => item.InstanceId == itemId));
    }

    [Fact]
    public void SearchWindowIncludesExactDeadlineButNotOneTickAfterAndNeverResets()
    {
        var search = new RaidSimulation.Opportunity(new OpportunityDefinition(
            "test", "Salvage Yard", 12, [], 2, [LootEligibilityCategory.Salvage],
            OpportunityActivity.Searching, "Test activity", "A test activity begins.",
            "Test activity complete.", true));
        search.Admit("one", Start);
        search.Admit("two", Start.AddSeconds(12));
        Assert.Equal(Start.AddSeconds(12), search.ClosesAt);
        Assert.Equal(2, search.ParticipantIds.Count);
        Assert.Throws<InvalidOperationException>(() => search.Admit("late", Start.AddSeconds(12).AddTicks(1)));
        Assert.DoesNotContain("late", search.ParticipantIds);
        Assert.Throws<InvalidOperationException>(() => search.Admit("two", Start.AddSeconds(12)));
        search.Resolved = true;
        Assert.Throws<InvalidOperationException>(() => search.Admit("after-resolution", Start.AddSeconds(12)));
    }

    [Fact]
    public void CrossingCautiousRaidersShareReceiptAndDivideUniqueSalvage()
    {
        var simulation = Create(11);
        simulation.Advance(Start.AddMinutes(3));
        var one = simulation.Project("one", Start);
        var two = simulation.Project("two", Start);
        var receipt = Assert.Single(one.Encounters);
        Assert.Equal("Cooperation", receipt.Resolution);
        Assert.Equal(["one", "two"], receipt.ParticipantIds);
        Assert.Equal(JsonSerializer.Serialize(one.Encounters), JsonSerializer.Serialize(two.Encounters));
        Assert.Equal(2, receipt.Awards.Select(item => item.RecipientId).Distinct().Count());
        Assert.All(receipt.Factors, factor => Assert.Equal("Cautious", factor.Disposition));
        var expectedPerRaider = one.Robot.Status == "Defeated" ? 3 : 2;
        Assert.Equal(expectedPerRaider, one.MyExtractedLoot.Count);
        Assert.Equal(expectedPerRaider, two.MyExtractedLoot.Count);
    }

    [Fact]
    public void NonCrossingBranchesProduceSoloSalvageAndExhaustedVisitsNotInventedOpponents()
    {
        var simulation = Create(2);
        simulation.Advance(Start.AddMinutes(3));
        var one = simulation.Project("one", Start);
        var two = simulation.Project("two", Start);
        var receipt = Assert.Single(one.Encounters);
        Assert.Equal("SoloSearch", receipt.Resolution);
        Assert.Equal(["one"], receipt.ParticipantIds);
        Assert.Single(receipt.Factors);
        Assert.Empty(two.Encounters);
        Assert.Contains(two.Events, item => item.Kind == "OpportunityExhausted" && item.Text.Contains("humming-crate"));
        Assert.Contains(one.Events, item => item.Kind == "OpportunityExhausted" && item.Text.Contains("depot-locker"));
        Assert.DoesNotContain(one.Events, item => item.Kind == "CrossedPaths" && item.Text.Contains("Salvage Yard"));
        Assert.All(receipt.Awards, award => Assert.Equal("one", award.RecipientId));
        var extracted = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
        Assert.Equal("Escaped", one.Robot.Status);
        Assert.Equal(4, extracted.Length);
        Assert.Equal(4, extracted.Select(item => item.InstanceId).Distinct().Count());
    }

    [Fact]
    public void ContestKeepsOriginalFactorsAndRecoveryCost()
    {
        var simulation = Create(0);
        while (simulation.Project("one", Start).Encounters.Count == 0)
        {
            simulation.Advance(simulation.NextActivityAt!.Value);
        }
        var projection = simulation.Project("one", Start);
        var receipt = Assert.Single(projection.Encounters);
        Assert.Equal("Contest", receipt.Resolution);
        Assert.All(receipt.Factors, factor => Assert.Equal(
            factor.Greed + factor.Aptitude * 3 + factor.Initiative - factor.BackpackValue / 5, factor.Score));
        var winner = receipt.Factors.OrderByDescending(factor => factor.Score).First();
        Assert.All(receipt.Awards, award => Assert.Equal(winner.PlayerId, award.RecipientId));
        Assert.All(projection.Players, player =>
        {
            Assert.Equal("Recovering", player.Activity);
            Assert.Equal(receipt.ResolvedAt.AddSeconds(8), player.ActivityEndsAt);
            Assert.Equal("Salvage Yard", player.Location);
        });
    }

    [Fact]
    public void ExactScoreTieUsesStableIdentityOrderNotJoinOrder()
    {
        var simulation = Create(193, reverseOrder: true);
        simulation.Advance(Start.AddMinutes(3));
        var receipt = Assert.Single(simulation.Project("one", Start).Encounters);
        Assert.Equal("Contest", receipt.Resolution);
        Assert.Equal(receipt.Factors[0].Score, receipt.Factors[1].Score);
        Assert.All(receipt.Awards, award => Assert.Equal("one", award.RecipientId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(11)]
    [InlineData(193)]
    public void JoinOrderPollingAndAbsentHandlersDoNotChangeOutcomes(int seed)
    {
        var frequent = Create(seed);
        var delayed = Create(seed, reverseOrder: true);
        for (var seconds = 0; seconds <= 180; seconds++) frequent.Advance(Start.AddSeconds(seconds));
        delayed.Advance(Start.AddSeconds(180));
        foreach (var playerId in new[] { "one", "two" })
        {
            var one = frequent.Project(playerId, Start.AddSeconds(180));
            var two = delayed.Project(playerId, Start.AddSeconds(180));
            Assert.Equal(JsonSerializer.Serialize(one.MyExtractedLoot), JsonSerializer.Serialize(two.MyExtractedLoot));
            Assert.Equal(JsonSerializer.Serialize(one.Encounters), JsonSerializer.Serialize(two.Encounters));
            Assert.Equal(JsonSerializer.Serialize(one.Robot), JsonSerializer.Serialize(two.Robot));
            Assert.Equal(JsonSerializer.Serialize(one.RobotEncounter), JsonSerializer.Serialize(two.RobotEncounter));
            Assert.Equal(one.Revision, two.Revision);
            Assert.Equal(one.Events.Where(item => item.Kind != "Deployment").Select(item => (item.At, item.Kind, item.Text)),
                two.Events.Where(item => item.Kind != "Deployment").Select(item => (item.At, item.Kind, item.Text)));
        }
    }

    [Fact]
    public void SoloRaiderTraversesConnectedGraphAndSettlesExactlyOnce()
    {
        var simulation = Create(11, solo: true);
        simulation.Advance(Start.AddMinutes(3));
        var result = simulation.Project("one", Start);
        Assert.Equal("Completed", result.Status);
        Assert.Equal("SoloSearch", Assert.Single(result.Encounters).Resolution);
        Assert.DoesNotContain(result.Events, item => item.Kind == "CrossedPaths");
        Assert.Empty(result.MyInventory);
        Assert.Equal("Escaped", result.Robot.Status);
        Assert.Equal(10, result.Robot.Health);
        Assert.Equal(22, result.MyHealth);
        Assert.Equal(10, result.MyShieldCharge);
        Assert.Equal(4, result.MyExtractedLoot.Count);
        Assert.Null(result.NextActivityAt);
        Assert.Null(result.Players[0].ActivityEndsAt);
        Assert.False(simulation.Advance(Start.AddDays(1)));
        Assert.Single(result.Events, item => item.Kind == "Extracted");
    }

    [Fact]
    public void CooperativeRaidersDefeatOneSharedRobotBeforeDepotLootIsAwarded()
    {
        var (simulation, one, two) = FindTwoPlayerRobotResult(encounter => encounter.Resolution == "Defeated");

        Assert.Equal("Defeated", one.Robot.Status);
        Assert.Equal(0, one.Robot.Health);
        Assert.Equal(0, two.Robot.Health);
        Assert.Equal(JsonSerializer.Serialize(one.Robot), JsonSerializer.Serialize(two.Robot));
        Assert.Equal(JsonSerializer.Serialize(one.RobotEncounter), JsonSerializer.Serialize(two.RobotEncounter));
        Assert.Equal(["one", "two"], one.RobotEncounter!.ParticipantIds);
        Assert.Equal(2, one.RobotEncounter.Damage.Count);
        Assert.All(one.RobotEncounter.Damage, entry => Assert.True(entry.DamageTaken < 40));
        var robotDefeated = one.Events.Single(item => item.Kind == "RobotDefeated");
        var depotSearch = one.Events.Single(item => item.Kind == "SearchCompleted" && item.Text.Contains("Service Depot"));
        Assert.True(robotDefeated.At < depotSearch.At);
        var allLoot = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
        Assert.Equal(6, allLoot.Length);
        Assert.Equal(6, allLoot.Select(item => item.InstanceId).Distinct().Count());
        Assert.Equal(2, allLoot.Count(item => item.InstanceId.Contains("depot-", StringComparison.Ordinal)));
        Assert.False(simulation.Advance(Start.AddDays(1)));
        Assert.Single(one.Events, item => item.Kind == "Extracted");
        Assert.Single(two.Events, item => item.Kind == "Extracted");
    }

    [Fact]
    public void AutonomousSharedRobotUnlocksOnlyItsFiniteConfiguredDepotPoolOnce()
    {
        RaidSimulation? simulation = null;
        RaidProjection? one = null;
        RaidProjection? two = null;
        for (var seed = 0; seed < 512; seed++)
        {
            var candidate = new RaidSimulation("c2-robot", Start, seed,
                catalog: LootCatalog.Default, useTableBackedLoot: true, fastPacing: true, riskReward: true);
            candidate.AddAutonomousRaider("one", "raider-one", Start);
            candidate.AddAutonomousRaider("two", "raider-two", Start);
            candidate.Advance(Start.AddMinutes(3));
            var first = candidate.Project("one", Start.AddMinutes(3));
            var second = candidate.Project("two", Start.AddMinutes(3));
            if (first.RobotEncounter?.Resolution == "Defeated"
                && first.Events.Any(raidEvent => raidEvent.Kind == "SearchCompleted"
                    && raidEvent.OpportunityId == "depot-locker"))
            {
                simulation = candidate;
                one = first;
                two = second;
                break;
            }
        }

        Assert.NotNull(simulation);
        Assert.NotNull(one);
        Assert.NotNull(two);
        Assert.Equal("Defeated", one.Robot.Status);
        Assert.Equal(["one", "two"], one.RobotEncounter!.ParticipantIds);
        var allEvents = one.Events.Concat(two.Events).ToArray();
        var robotDefeated = Assert.Single(one.Events, raidEvent => raidEvent.Kind == "RobotDefeated");
        var depotSearch = Assert.Single(one.Events, raidEvent => raidEvent.Kind == "SearchCompleted"
            && raidEvent.OpportunityId == "depot-locker");
        Assert.True(robotDefeated.At < depotSearch.At);
        var depotAwards = allEvents.Where(raidEvent => raidEvent.Kind == "ItemAllocated"
            && raidEvent.OpportunityId == "depot-locker").ToArray();
        Assert.Equal(2, depotAwards.Length);
        Assert.All(depotAwards, award => Assert.Contains(award.ParticipantIds.Single(), new[] { "one", "two" }));

        var awards = allEvents.Where(raidEvent => raidEvent.Kind == "ItemAllocated").ToArray();
        Assert.Equal(6, awards.Length);
        Assert.Equal(awards.Length, awards.Select(award => award.ItemInstanceId).Distinct().Count());
        Assert.All(awards, award => Assert.Single(award.ParticipantIds));
        foreach (var award in depotAwards)
        {
            var catalogItem = Assert.Single(simulation.Catalog.Items, item => item.Name == award.ItemName);
            Assert.Contains(catalogItem.EligibilityCategories,
                category => category is LootEligibilityCategory.Robot or LootEligibilityCategory.Technology);
        }

        simulation.Advance(Start.AddDays(1));
        var settledEvents = simulation.Project("one", Start.AddDays(1)).Events
            .Concat(simulation.Project("two", Start.AddDays(1)).Events).ToArray();
        var settledAwards = settledEvents.Where(raidEvent => raidEvent.Kind == "ItemAllocated").ToArray();
        Assert.Equal(6, settledAwards.Length);
        Assert.Equal(settledAwards.Length, settledAwards.Select(award => award.ItemInstanceId).Distinct().Count());
        Assert.False(simulation.Advance(Start.AddDays(1)));
        var repeatedEvents = simulation.Project("one", Start.AddDays(1)).Events
            .Concat(simulation.Project("two", Start.AddDays(1)).Events).ToArray();
        Assert.Equal(settledEvents.Length, repeatedEvents.Length);
    }

    [Fact]
    public void LateActualArrivalJoinsOnlyFutureRobotRounds()
    {
        var (_, one, two) = FindTwoPlayerRobotResult(encounter =>
        {
            var early = encounter.Damage.MinBy(entry => entry.JoinedAt)!;
            var late = encounter.Damage.MaxBy(entry => entry.JoinedAt)!;
            return late.JoinedAt > encounter.StartedAt
                && early.DamageDealt > late.DamageDealt
                && early.DamageTaken > late.DamageTaken;
        });
        var encounter = one.RobotEncounter!;
        var early = encounter.Damage.MinBy(entry => entry.JoinedAt)!;
        var late = encounter.Damage.MaxBy(entry => entry.JoinedAt)!;

        Assert.True(late.JoinedAt > encounter.StartedAt);
        Assert.True(early.DamageDealt > late.DamageDealt);
        Assert.True(early.DamageTaken > late.DamageTaken);
        Assert.Equal(JsonSerializer.Serialize(encounter), JsonSerializer.Serialize(two.RobotEncounter));
        Assert.Contains(one.Events, item => item.Kind == "RobotParticipantJoined");
        Assert.DoesNotContain(one.Events, item => item.Kind == "Deployment" && item.ParticipantIds.Contains("two"));
        Assert.Contains(one.Events, item => item.Kind == "Arrived" && item.Text.Contains("Service Depot"));
    }

    [Fact]
    public void RaidersExtractIndependentlyAndCompletedOpportunityNeverAwardsAgain()
    {
        var simulation = Create(2);
        while (!simulation.Project("one", Start).Players.Any(player => player.Activity == "Extracted"))
        {
            simulation.Advance(simulation.NextActivityAt!.Value);
        }
        var first = simulation.Project("one", Start);
        Assert.Equal("Raiding", first.Status);
        Assert.Single(first.Players, player => player.Activity == "Extracted");
        var extractedPlayer = first.Players.Single(player => player.Activity == "Extracted").PlayerId;
        var settledIds = simulation.Project(extractedPlayer, Start).MyExtractedLoot.Select(item => item.InstanceId).ToArray();
        Assert.NotEmpty(settledIds);
        simulation.Advance(Start.AddMinutes(3));
        Assert.True(simulation.Completed);
        Assert.Equal(settledIds, simulation.Project(extractedPlayer, Start).MyExtractedLoot.Select(item => item.InstanceId));
        Assert.False(simulation.Advance(Start.AddDays(1)));
    }

    [Fact]
    public void SeedSweepPreservesPresenceOwnershipOrderingAndFiniteCompletion()
    {
        for (var seed = 0; seed < 256; seed++)
        {
            var simulation = Create(seed);
            var encountered = false;
            while (simulation.NextActivityAt is { } at)
            {
                Assert.True(at < Start.AddMinutes(3), $"Seed {seed} exceeded the bounded journey.");
                simulation.Advance(at);
                var snapshot = simulation.Project("one", at);
                if (!encountered && snapshot.Encounters.Count > 0)
                {
                    var receipt = snapshot.Encounters[0];
                    Assert.All(receipt.ParticipantIds, id =>
                    {
                        var player = snapshot.Players.Single(player => player.PlayerId == id);
                        Assert.Equal(receipt.Location, player.Location);
                        Assert.Contains(player.Activity, new[] { "Packing", "Recovering" });
                    });
                    encountered = true;
                }
            }
            var one = simulation.Project("one", Start);
            var two = simulation.Project("two", Start);
            Assert.True(simulation.Completed);
            Assert.All(one.Opportunities, opportunity =>
            {
                Assert.Equal("Resolved", opportunity.Status);
                Assert.Equal(0, opportunity.RemainingItems);
            });
            var allItems = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
            var expectedLootCount = one.Robot.Status == "Defeated" ? 6 : 4;
            Assert.Equal(expectedLootCount, allItems.Length);
            Assert.Equal(expectedLootCount, allItems.Select(item => item.InstanceId).Distinct().Count());
            Assert.Empty(one.MyInventory);
            Assert.Empty(two.MyInventory);
            Assert.Single(one.Events, item => item.Kind == "Extracted");
            Assert.Single(two.Events, item => item.Kind == "Extracted");
            Assert.True(one.Events.Zip(one.Events.Skip(1)).All(pair => pair.First.At <= pair.Second.At));
            Assert.True(one.Events.Zip(one.Events.Skip(1)).All(pair => pair.First.Sequence < pair.Second.Sequence));
        }
    }

    [Fact]
    public void PreparationJoinDoesNotResetDepartureOrCreateDuplicateParticipants()
    {
        var simulation = Create(11, solo: true);
        simulation.AddRaider("two", "raider-two", Start.AddSeconds(19));
        Assert.Equal(Start.AddSeconds(20), simulation.Project("two", Start).DepartsAt);
        Assert.Throws<RaidAccessException>(() => simulation.AddRaider("extra", "raider-two", Start.AddSeconds(19)));
        Assert.Throws<RaidAccessException>(() => simulation.AddRaider("one", "raider-two", Start.AddSeconds(19)));
    }

    [Fact]
    public void AllRequiredTransitionEventsEmittedWithMonotonicSequenceAndParodyNarrative()
    {
        var (simulation, one, two) = FindTwoPlayerRobotResult(encounter => encounter.Resolution == "Defeated");
        var allEvents = one.Events.Concat(two.Events).ToList();

        // Check required event kinds across the raid
        var kinds = allEvents.Select(e => e.Kind).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("Departure", kinds);
        Assert.Contains("Travel", kinds);
        Assert.Contains("Arrived", kinds);
        Assert.Contains("Search", kinds);
        Assert.Contains("ItemAllocated", kinds);
        Assert.Contains("SearchCompleted", kinds);
        Assert.Contains("RobotCombatStarted", kinds);
        Assert.Contains("RobotRound", kinds);
        Assert.Contains("RobotDefeated", kinds);
        Assert.Contains("ExtractionStarted", kinds);
        Assert.Contains("Extracted", kinds);

        // Monotonic sequence and timestamp ordering on individual participant streams
        foreach (var playerEvents in new[] { one.Events, two.Events })
        {
            for (var i = 1; i < playerEvents.Count; i++)
            {
                Assert.True(playerEvents[i].Sequence > playerEvents[i - 1].Sequence, $"Sequence must strictly increase at index {i}");
                Assert.True(playerEvents[i].At >= playerEvents[i - 1].At, $"Timestamp must be monotonic non-decreasing at index {i}");
            }

            // Parody text and channels populated
            Assert.All(playerEvents, evt =>
            {
                Assert.False(string.IsNullOrWhiteSpace(evt.Text), $"Event {evt.Kind} must have non-empty text");
                Assert.False(string.IsNullOrWhiteSpace(evt.Channel), $"Event {evt.Kind} must have non-empty channel");
            });
        }
    }

    [Fact]
    public void ActiveThreadProjectsCurrentActivityAndRetiresToRecent()
    {
        var simulation = Create(2);
        // Advance into first travel activity
        simulation.Advance(Start.AddSeconds(21));
        var projTravel = simulation.Project("one", Start.AddSeconds(21));
        Assert.NotNull(projTravel.CurrentThread);
        Assert.Equal("Travel", projTravel.CurrentThread.Kind);
        Assert.Equal("Entry Tunnel", projTravel.CurrentThread.Location);
        Assert.Equal("Travel to Market", projTravel.CurrentThread.Title);
        Assert.True(projTravel.CurrentThread.TotalSeconds > 0);
        Assert.NotEmpty(projTravel.CurrentThread.Logs);

        // Advance into search activity
        simulation.Advance(Start.AddSeconds(30));
        var projSearch = simulation.Project("one", Start.AddSeconds(30));
        Assert.NotNull(projSearch.CurrentThread);
        Assert.Equal("Search", projSearch.CurrentThread.Kind);
        Assert.NotNull(projSearch.RecentThreads);
        Assert.Contains(projSearch.RecentThreads!, t => t.Kind == "Travel");

        // Advance to full completion
        simulation.Advance(Start.AddMinutes(3));
        var projCompleted = simulation.Project("one", Start.AddMinutes(3));
        Assert.Null(projCompleted.CurrentThread);
        Assert.NotNull(projCompleted.RecentThreads);
        Assert.NotEmpty(projCompleted.RecentThreads!);
        Assert.Contains(projCompleted.RecentThreads!, t => t.Kind == "Extraction");
    }

    [Fact]
    public void PrivateInventoryEventsAreFilteredToOwningRaider()
    {
        var simulation = Create(0);
        simulation.Advance(Start.AddMinutes(3));
        var one = simulation.Project("one", Start.AddMinutes(3));
        var two = simulation.Project("two", Start.AddMinutes(3));

        var oneItemAllocations = one.Events.Where(e => e.Kind == "ItemAllocated").ToArray();
        var twoItemAllocations = two.Events.Where(e => e.Kind == "ItemAllocated").ToArray();

        Assert.NotEmpty(oneItemAllocations);
        Assert.NotEmpty(twoItemAllocations);

        // Raider one's item allocation events should only reference items in one's inventory/extracted loot
        var oneExtractedNames = one.MyExtractedLoot.Select(i => i.Name).ToHashSet(StringComparer.Ordinal);
        Assert.All(oneItemAllocations, evt => Assert.Contains(evt.ItemName!, oneExtractedNames));

        // Raider one's private allocations should not appear in raider two's event stream
        var oneInstanceIds = one.MyExtractedLoot.Select(i => i.InstanceId).ToHashSet(StringComparer.Ordinal);
        foreach (var evt in twoItemAllocations)
        {
            Assert.DoesNotContain(evt.ItemInstanceId!, oneInstanceIds);
        }
    }
}
