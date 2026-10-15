using System.Text.Json;
using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

public sealed class G6RiskRewardTests(ITestOutputHelper output)
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
    private static readonly string[] Dangers = ["Low", "Medium", "High"];
    private static readonly string[] Slots = ["raider-one", "raider-two"];
    private static readonly StagedSupplyItem[] Supplies =
    [
        new("bandage_white", "White Bandage", "Medical", 2),
        new("bandage_green", "Green Bandage", "Medical", 1),
        new("fizz_cell", "Fizz Cell", "ShieldRecharger", 1)
    ];

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Start;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed record Run(bool Extracted, int Net, double Seconds, bool Early, string Mechanics);

    private static Run FirstRaid(int seed, string slot, string danger, bool attended, bool staged)
    {
        var sim = new RaidSimulation("audit", Start, seed, danger: danger);
        sim.AddAutonomousRaider("one", slot, Start, staged ? Supplies : null);
        var lastCalm = -60;
        var called = false;
        var signal = 3;
        var signalAt = 0;
        for (var t = 5; t <= 1500 && !sim.Completed; t += 5)
        {
            sim.Advance(Start.AddSeconds(t));
            if (!attended || sim.Completed) continue;
            var p = sim.Project("one", Start.AddSeconds(t));
            if (t - signalAt >= 30)
            {
                var points = (t - signalAt) / 30;
                signal = Math.Min(5, signal + points);
                signalAt += points * 30;
            }
            if (!called && p.MyInventory.Count > 0 && (p.MyHealth <= 18 || t >= 1080) && signal >= 3)
            {
                if (sim.QueueHandlerAction($"exit:{t}", "CallExtract", "one", Start.AddSeconds(t)))
                {
                    signal -= 3;
                    called = true;
                }
            }
            else if (!called && p.MyGreed >= 35 && t - lastCalm >= 60 && signal >= 1)
            {
                if (sim.QueueHandlerAction($"calm:{t}", "Calm", "one", Start.AddSeconds(t)))
                {
                    signal--;
                    lastCalm = t;
                }
            }
        }
        Assert.True(sim.Completed);
        var final = sim.Project("one", Start.AddSeconds(1500));
        var terminal = Assert.Single(final.Events, e => e.Kind is "Extracted" or "KnockedOut");
        var extracted = terminal.Kind == "Extracted";
        var supplyCost = staged ? Supplies.Sum(s =>
            (s.Quantity - (final.MyFieldSupplies?.SingleOrDefault(f => f.CatalogId == s.CatalogId)?.Quantity ?? 0)) *
            ShopCatalog.Supplies.Single(d => d.Id == s.CatalogId).Value) : 0;
        return new(extracted, final.MyExtractedLoot.Sum(i => i.Value) - supplyCost,
            (terminal.At - Start).TotalSeconds,
            final.Events.Any(e => e.Kind == "HandlerAction" && e.Reason == "HandlerSupport:CallExtract") ||
                (terminal.At - Start).TotalSeconds < RaidConditions.Profile(danger).MinimumExtractSeconds,
            JsonSerializer.Serialize(new
            {
                final.MyHealth,
                Loot = final.MyExtractedLoot.Select(i => i.CatalogId),
                terminal.Kind,
                terminal.At
            }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FrozenFirstRaidCohortMeetsDangerSupportAndNetThresholds(bool staged)
    {
        var results = new Dictionary<(string Danger, bool Attended), Run[]>();
        foreach (var danger in Dangers)
            foreach (var attended in new[] { false, true })
            {
                var runs = Enumerable.Range(1, 120).SelectMany(seed => Slots.Select(slot =>
                    FirstRaid(seed, slot, danger, attended, staged))).ToArray();
                results[(danger, attended)] = runs;
                var ordinary = runs.Where(r => r.Extracted && !r.Early).OrderBy(r => r.Seconds).ToArray();
                output.WriteLine($"{(staged ? "Staged" : "Starter")} {danger} {(attended ? "attended" : "unattended")}: " +
                    $"extract={runs.Count(r => r.Extracted)}/{runs.Length} ({runs.Count(r => r.Extracted) / 2.4:F2}%), " +
                    $"mean net={runs.Average(r => r.Net):F2}, ordinary={ordinary.Length}, " +
                    $"ordinary median={(ordinary.Length == 0 ? 0 : ordinary[ordinary.Length / 2].Seconds):F0}s, " +
                    $"early/support={runs.Count(r => r.Extracted && r.Early)}, " +
                    $"before deadline={runs.Count(r => r.Extracted && r.Seconds < RaidConditions.DeadlineSeconds)}");
            }
        double Rate(string danger, bool attended) => results[(danger, attended)].Count(r => r.Extracted) / 240.0;
        double Net(string danger, bool attended) => results[(danger, attended)].Average(r => r.Net);
        Assert.True(Rate("Low", false) >= .8, "Low unattended extraction must be >=80%.");
        Assert.True(Net("Low", false) > 0, "Low unattended mean net must be positive.");
        Assert.True(Rate("Low", false) - Rate("Medium", false) >= .05, "Medium failure must rise >=5pp.");
        Assert.True(Rate("Medium", false) - Rate("High", false) >= .05, "High failure must rise >=5pp.");
        foreach (var danger in new[] { "Medium", "High" })
            Assert.True(Rate(danger, true) - Rate(danger, false) >= .05 ||
                Net(danger, true) >= Net(danger, false) * 1.1, $"{danger} support must improve extraction >=5pp or net >=10%.");
        foreach (var runs in results.Values)
        {
            var successful = runs.Where(r => r.Extracted).ToArray();
            Assert.True(successful.Count(r => r.Seconds < RaidConditions.DeadlineSeconds) >= successful.Length * .95);
            var ordinary = successful.Where(r => !r.Early).OrderBy(r => r.Seconds).ToArray();
            Assert.NotEmpty(ordinary);
            Assert.InRange(ordinary[ordinary.Length / 2].Seconds, 900, 1200);
        }
    }

    [Fact]
    public void LateLootIsMoreValuableWithoutInflationOrMoreInstances()
    {
        var catalog = LootCatalog.Default;
        foreach (var danger in Dangers)
        {
            var early = new List<ItemView>();
            var late = new List<ItemView>();
            foreach (var seed in Enumerable.Range(1, 120))
                foreach (var opp in catalog.Opportunities)
                {
                    var a = catalog.CreateOpportunityItems("loot", seed, opp.Id, danger, 0, .35);
                    var b = catalog.CreateOpportunityItems("loot", seed, opp.Id, danger, 1, .35);
                    Assert.Equal(a.Count, b.Count);
                    Assert.Equal(a.Select(i => i.Item.InstanceId), b.Select(i => i.Item.InstanceId));
                    early.AddRange(a.Select(i => i.Item));
                    late.AddRange(b.Select(i => i.Item));
                }
            output.WriteLine($"{danger}: early={early.Average(i => i.Value):F2}, late={late.Average(i => i.Value):F2}");
            Assert.True(late.Average(i => i.Value) >= early.Average(i => i.Value) * 1.05);
            Assert.All(early.Concat(late), i => Assert.Equal(catalog.GetItem(i.CatalogId).Value, i.Value));
            Assert.True(RaidConditions.HazardOdds(danger, 1) > RaidConditions.HazardOdds(danger, 0));
        }
    }

    [Fact]
    public void CalmPressureHavePrivateMonotonicMechanismsAndRealizedEffects()
    {
        RaidProjection AtMinute(string action)
        {
            var sim = new RaidSimulation("support", Start, 1, danger: "Medium");
            sim.AddAutonomousRaider("one", "raider-one", Start);
            sim.AddAutonomousRaider("two", "raider-two", Start);
            sim.Advance(Start.AddSeconds(40));
            Assert.True(sim.QueueHandlerAction("same-id", action, "one", Start.AddSeconds(40)));
            sim.Advance(Start.AddSeconds(60));
            var p = sim.Project("one", Start.AddSeconds(60));
            Assert.DoesNotContain(sim.Project("two", Start.AddSeconds(60)).Events,
                e => e.Kind == "HandlerAction");
            return p;
        }
        var calm = AtMinute("Calm");
        var pressure = AtMinute("Pressure");
        Assert.True(calm.MyGreed < pressure.MyGreed);
        Assert.True(calm.MyMood > pressure.MyMood);
        Assert.True(calm.MyResilience > pressure.MyResilience);
        Assert.True(calm.MyAppetite < pressure.MyAppetite);
        Assert.True(RaidConditions.ExtractChance(calm.MyGreed, 30, false, 0) >
            RaidConditions.ExtractChance(pressure.MyGreed, 30, false, 0));
        var differences = Enumerable.Range(1, 24).Count(seed =>
            FirstRaid(seed, "raider-one", "Medium", false, false).Mechanics !=
            FirstRaid(seed, "raider-one", "Medium", true, false).Mechanics);
        Assert.True(differences > 0);
        output.WriteLine($"Matched attended/unattended outcome differences: {differences}/24.");
        string ForcedOutcome(int seed, string action)
        {
            var sim = new RaidSimulation("forced", Start, seed, danger: "Medium");
            sim.AddAutonomousRaider("one", "raider-one", Start);
            sim.Advance(Start.AddSeconds(40));
            sim.QueueHandlerAction("support", action, "one", Start.AddSeconds(40));
            sim.Advance(Start.AddSeconds(1500));
            var p = sim.Project("one", Start.AddSeconds(1500));
            return JsonSerializer.Serialize(new
            {
                p.MyHealth,
                Loot = p.MyExtractedLoot.Select(i => i.CatalogId),
                Terminal = p.Events.Single(e => e.Kind is "Extracted" or "KnockedOut").At
            });
        }
        var pressureDifferences = Enumerable.Range(1, 24).Count(seed =>
            ForcedOutcome(seed, "Calm") != ForcedOutcome(seed, "Pressure"));
        Assert.True(pressureDifferences > 0);
        output.WriteLine($"Matched Calm/Pressure realized outcome differences: {pressureDifferences}/24.");
    }

    [Fact]
    public void MarketResidenceHasDistinctTruthfulDiaryWithoutNewRewards()
    {
        var sim = new RaidSimulation("diary", Start, 1, danger: "Low");
        sim.AddAutonomousRaider("one", "raider-one", Start);
        sim.Advance(Start.AddSeconds(200));
        var p = sim.Project("one", Start.AddSeconds(200));
        Assert.Equal("Market", p.Players.Single().Location);
        var diary = p.Events.Where(e => e.Kind == "AreaDiary" && e.Location == "Market").ToArray();
        Assert.True(diary.Length >= 6);
        Assert.All(diary, e =>
        {
            Assert.Null(e.ItemInstanceId);
            Assert.Null(e.HealthDelta);
            Assert.Contains("No loot, damage or new actor created.", e.FactsText);
        });
        Assert.Equal(diary.Length, diary.Select(e => e.Text).Distinct().Count());
        sim.Advance(Start.AddSeconds(1500));
        var lines = sim.Project("one", Start.AddSeconds(1500)).Events
            .Select(e => e.Text.Split("Raider: ").Last()).ToArray();
        Assert.All(lines.Zip(lines.Skip(1)), pair => Assert.NotEqual(pair.First, pair.Second));
        Assert.All(p.Events.Where(e => e.Kind == "AreaDiary").GroupBy(e => e.Activity),
            group => Assert.Equal(group.Count(), group.Select(e => e.Text.Split("Raider: ").Last()).Distinct().Count()));
    }

    [Fact]
    public void SharedTimelineCommandsConditionsAndOwnershipIgnorePolling()
    {
        foreach (var seed in Enumerable.Range(1, 24))
        {
            RaidProjection[] Play(bool frequent)
            {
                var sim = new RaidSimulation("shared", Start, seed, danger: "Medium");
                sim.AddAutonomousRaider("one", "raider-one", Start);
                sim.AddAutonomousRaider("two", "raider-two", Start);
                if (frequent)
                    for (var t = 1; t <= 100; t++)
                    {
                        sim.Advance(Start.AddSeconds(t));
                        _ = sim.Project("one", Start.AddSeconds(t));
                        _ = sim.Project("two", Start.AddSeconds(t));
                    }
                else sim.Advance(Start.AddSeconds(100));
                Assert.True(sim.QueueHandlerAction("shared-id", "Calm", "one", Start.AddSeconds(100)));
                Assert.True(sim.QueueHandlerAction("shared-id", "Pressure", "two", Start.AddSeconds(100)));
                if (frequent)
                    for (var t = 101; t <= 1500; t++) sim.Advance(Start.AddSeconds(t));
                else sim.Advance(Start.AddSeconds(1500));
                var a = sim.Project("one", Start.AddSeconds(1500));
                var b = sim.Project("two", Start.AddSeconds(1500));
                Assert.Equal(a.Condition, b.Condition);
                Assert.Equal(JsonSerializer.Serialize(a.Robot), JsonSerializer.Serialize(b.Robot));
                Assert.Equal(a.MyExtractedLoot.Count + b.MyExtractedLoot.Count,
                    a.MyExtractedLoot.Concat(b.MyExtractedLoot).Select(i => i.InstanceId).Distinct().Count());
                foreach (var impact in a.Events.Where(e => e.Kind == "ConditionImpact" && e.ParticipantIds.Count == 2))
                    Assert.Equal(impact, b.Events.Single(e => e.Sequence == impact.Sequence));
                var revision = sim.Revision;
                sim.Advance(Start.AddHours(2));
                Assert.Equal(revision, sim.Revision);
                return [a, b];
            }
            Assert.Equal(JsonSerializer.Serialize(Play(false)), JsonSerializer.Serialize(Play(true)));
        }
    }

    [Fact]
    public void CallExtractInterruptsResidenceButNeverTeleportsOrGrantsUnfinishedRewards()
    {
        var sim = new RaidSimulation("exit", Start, 1, danger: "Low");
        sim.AddAutonomousRaider("one", "raider-one", Start);
        sim.Advance(Start.AddSeconds(40));
        Assert.True(sim.QueueHandlerAction("extract", "CallExtract", "one", Start.AddSeconds(40)));
        sim.Advance(Start.AddSeconds(100));
        var p = sim.Project("one", Start.AddSeconds(100));
        Assert.Equal("Extracted", p.Players.Single().Activity);
        Assert.Contains(p.Events, e => e.Kind == "ActivityInterrupted");
        Assert.All(p.Events.Where(e => e.Kind == "Travel"),
            e => Assert.Contains(e.Destination, p.Locations.Single(l => l.Name == e.Location).Neighbors));
        Assert.Equal(2, p.MyExtractedLoot.Count);
        Assert.DoesNotContain(p.Events, e => e.Kind == "AreaActivityCompleted");
    }

    [Fact]
    public void AdmissionCutoffAndFutureCommandTimeAreAuthoritative()
    {
        var sim = new RaidSimulation("late", Start, 1, danger: "Low");
        sim.AddAutonomousRaider("one", "raider-one", Start);
        Assert.False(sim.CanAdmit(Start.AddSeconds(301)));
        Assert.Throws<RaidAccessException>(() => sim.AddAutonomousRaider("two", "raider-two", Start.AddSeconds(301)));
        Assert.True(sim.QueueHandlerAction("future", "Pressure", "one", Start.AddSeconds(200)));
        sim.Advance(Start.AddSeconds(100));
        Assert.DoesNotContain(sim.Project("one", Start.AddSeconds(100)).Events, e => e.Kind == "HandlerAction");
        sim.Advance(Start.AddSeconds(240));
        Assert.All(sim.Project("one", Start.AddSeconds(240)).Events.Where(e => e.Kind == "HandlerAction"),
            e => Assert.True(e.At >= Start.AddSeconds(200)));
    }

    [Fact]
    public void SignalRegenAtCapacityDoesNotDependOnReads()
    {
        int Play(bool reads)
        {
            var clock = new Clock();
            var coordinator = new RaidCoordinator(clock, nextSeed: () => 1);
            var handler = coordinator.EnrollHandler("raider-one");
            coordinator.AdvanceRaids();
            clock.Now = Start.AddSeconds(65);
            coordinator.AdvanceRaids();
            if (reads) _ = coordinator.GetHandlerProjection(handler.AccessToken);
            clock.Now = Start.AddSeconds(75);
            coordinator.ExecuteSupportAction(handler.AccessToken, new("calm", "Calm"));
            clock.Now = Start.AddSeconds(90);
            return coordinator.GetHandlerProjection(handler.AccessToken).SignalCurrent;
        }
        Assert.Equal(5, Play(false));
        Assert.Equal(Play(false), Play(true));
    }

    [Fact]
    public void FrozenEightHourRecurringCohortMeasuresNetAndDistinctReturns()
    {
        foreach (var attended in new[] { false, true })
        {
            var extracted = 0;
            var knockedOut = 0;
            var net = 0;
            var conditions = new HashSet<string>();
            foreach (var seed in Enumerable.Range(1, 24))
            {
                var clock = new Clock();
                var nextSeed = seed;
                var coordinator = new RaidCoordinator(clock, nextSeed: () => nextSeed++);
                var enrolled = Slots.Select(coordinator.EnrollHandler).ToArray();
                var settlements = new HashSet<string>();
                var lastCalm = new Dictionary<string, DateTimeOffset>();
                var calledWorlds = new HashSet<string>();
                var securedBySlot = Slots.ToDictionary(s => s, _ => 0);
                for (var t = 0; t <= 8 * 3600; t += 5)
                {
                    clock.Now = Start.AddSeconds(t);
                    coordinator.AdvanceRaids();
                    if (!attended && t % 30 != 0) continue;
                    foreach (var handler in enrolled)
                    {
                        var p = coordinator.GetHandlerProjection(handler.AccessToken);
                        if (p.LastReturnSummary is { Payoff: { } payoff } summary && settlements.Add(payoff.SettlementId))
                        {
                            if (summary.Outcome == "Extracted") extracted++; else knockedOut++;
                            net += payoff.GrossSecuredValue;
                            securedBySlot[p.Slot] += payoff.GrossSecuredValue;
                        }
                        if (p.Raid is not { } raid) continue;
                        conditions.Add(raid.Condition!.Danger);
                        if (!attended) continue;
                        var key = $"{p.Slot}:{raid.RaidId}";
                        if (!calledWorlds.Contains(key) && p.SignalCurrent >= 3 && raid.MyInventory.Count > 0 &&
                            (raid.MyHealth <= 18 || (clock.Now - raid.Events.First(e => e.Kind == "DutyAdmission").At).TotalSeconds >= 1080))
                        {
                            coordinator.ExecuteSupportAction(handler.AccessToken, new($"exit:{t}", "CallExtract"));
                            calledWorlds.Add(key);
                        }
                        else if (!calledWorlds.Contains(key) && p.SignalCurrent >= 1 && raid.MyGreed >= 35 &&
                            (!lastCalm.TryGetValue(p.Slot, out var last) || clock.Now - last >= TimeSpan.FromSeconds(60)))
                        {
                            coordinator.ExecuteSupportAction(handler.AccessToken, new($"calm:{t}", "Calm"));
                            lastCalm[p.Slot] = clock.Now;
                        }
                    }
                }
                foreach (var handler in enrolled)
                {
                    var p = coordinator.GetHandlerProjection(handler.AccessToken);
                    Assert.Equal(p.FullJournal!.Count, p.FullJournal.Select(e => e.SourceEventId).Distinct().Count());
                    Assert.Equal(p.Stash!.Count, p.Stash.Select(i => i.InstanceId).Distinct().Count());
                    Assert.Equal(securedBySlot[p.Slot], p.StashValue + p.Coins);
                }
            }
            output.WriteLine($"8h recurring {(attended ? "attended" : "unattended")}: " +
                $"returns={extracted + knockedOut}, extracts={extracted}, KO={knockedOut}, " +
                $"net secured={net}, mean per return={net / (double)(extracted + knockedOut):F2}, " +
                $"observed danger={string.Join(",", conditions.Order())}. Starter gear/supplies cost=0; no purchases.");
            Assert.True(net > 0);
            Assert.True(extracted > 0 && knockedOut > 0);
            if (!attended) Assert.Contains("High", conditions);
        }
    }

    [Fact]
    public void UrgencyRemainsFallibleAndMissedDeadlineSettlesAsFailure()
    {
        var urgent = 0;
        var missed = 0;
        foreach (var seed in Enumerable.Range(1, 120))
        {
            var sim = new RaidSimulation("deadline", Start, seed, danger: "Low");
            sim.AddAutonomousRaider("one", "raider-one", Start, personality: new(100, 10));
            sim.Advance(Start.AddSeconds(1500));
            var p = sim.Project("one", Start.AddSeconds(1500));
            urgent += p.Events.Count(e => e.Kind == "ExtractionDecision" && e.Reason == "DeadlineUrgency");
            missed += p.Events.Count(e => e.Kind == "DeadlineExpired");
            Assert.All(p.Events.Where(e => e.Kind == "Extracted"), e => Assert.True(e.At <= p.DeadlineAt));
            if (p.Events.Any(e => e.Kind == "DeadlineExpired"))
            {
                Assert.Equal("KnockedOut", p.Players.Single().Activity);
                Assert.Empty(p.MyExtractedLoot);
                Assert.Contains("World deadline", p.MyCauseOfDefeat);
            }
        }
        output.WriteLine($"Max-greed stress fixture: urgent decisions={urgent}, missed deadlines={missed}/120.");
        Assert.True(urgent > 0);
        Assert.True(missed > 0);
    }

    [Fact]
    public void InterruptedSearchBurnsUnfinishedLootAndKeepsOtherRaiderSearching()
    {
        var sim = new RaidSimulation("interrupt-search", Start, 1, danger: "Low");
        sim.AddAutonomousRaider("one", "raider-one", Start);
        sim.AddAutonomousRaider("two", "raider-two", Start);
        var t = 0;
        while (sim.Project("one", Start.AddSeconds(t)).Players.Single(p => p.PlayerId == "one").Activity != "Searching")
        {
            t++;
            sim.Advance(Start.AddSeconds(t));
            Assert.InRange(t, 1, 15);
        }
        Assert.True(sim.QueueHandlerAction("exit", "CallExtract", "one", Start.AddSeconds(t)));
        sim.Advance(Start.AddSeconds(50));
        var one = sim.Project("one", Start.AddSeconds(50));
        var two = sim.Project("two", Start.AddSeconds(50));
        Assert.Empty(one.MyInventory);
        Assert.Empty(one.MyExtractedLoot);
        Assert.Equal(2, two.MyInventory.Count);
        Assert.DoesNotContain(one.Events, e => e.Kind == "ItemAllocated");
        Assert.DoesNotContain(two.Events, e => e.Kind == "HandlerAction");
        Assert.Contains(one.Events, e => e.Kind == "ActivityInterrupted");
    }

    [Fact]
    public void StaggeredEnvironmentalParticipantsMatchActualLocationAndAliveState()
    {
        var impacts = 0;
        var shared = 0;
        foreach (var seed in Enumerable.Range(1, 24))
        {
            var sim = new RaidSimulation("occupancy", Start, seed, danger: "High");
            sim.AddAutonomousRaider("one", "raider-one", Start);
            sim.Advance(Start.AddSeconds(100));
            sim.AddAutonomousRaider("two", "raider-two", Start.AddSeconds(100));
            sim.Advance(Start.AddSeconds(1500));
            var one = sim.Project("one", Start.AddSeconds(1500));
            var two = sim.Project("two", Start.AddSeconds(1500));
            var all = one.Events.Concat(two.Events).GroupBy(e => e.Sequence).Select(g => g.First()).OrderBy(e => e.Sequence);
            var location = new Dictionary<string, string> { ["one"] = "Entry Tunnel", ["two"] = "Entry Tunnel" };
            var absent = new HashSet<string> { "two" };
            foreach (var evt in all)
            {
                foreach (var player in evt.ParticipantIds)
                {
                    if (evt.Kind is "Travel" or "Downed" or "Extracted" or "KnockedOut") absent.Add(player);
                    if (evt.Kind == "DutyAdmission") absent.Remove(player);
                    if (evt.Kind == "Arrived")
                    {
                        location[player] = evt.Location!;
                        absent.Remove(player);
                    }
                }
                if (evt.Kind != "ConditionImpact") continue;
                impacts++;
                if (evt.ParticipantIds.Count == 2) shared++;
                var present = location.Keys.Where(p => !absent.Contains(p) && location[p] == evt.Location).Order().ToArray();
                Assert.Equal(present, evt.ParticipantIds.Order());
                foreach (var p in present)
                    Assert.Contains((p == "one" ? one : two).Events, e => e.Sequence == evt.Sequence);
                foreach (var p in location.Keys.Except(present))
                    Assert.DoesNotContain((p == "one" ? one : two).Events, e => e.Sequence == evt.Sequence);
            }
        }
        output.WriteLine($"Staggered real-occupant impacts={impacts}, co-present impacts={shared}.");
        Assert.True(impacts > 0 && shared > 0);
    }

    [Fact]
    public void RecurringPersonalityCoolsOrResetsAndVoiceDoesNotRestart()
    {
        foreach (var seed in Enumerable.Range(1, 24))
        {
            var sim = new RaidSimulation("old", Start, seed, danger: "Medium");
            sim.AddAutonomousRaider("one", "raider-one", Start);
            sim.Advance(Start.AddSeconds(1500));
            var p = sim.Project("one", Start.AddSeconds(1500));
            var personality = sim.Personality("one");
            var cooled = p.Players.Single().Activity == "KnockedOut" ? new RaiderPersonality(0, 0) :
                new((int)(personality.Greed * .65), Math.Max(0, personality.Mood));
            var next = new RaidSimulation("new", Start.AddSeconds(1600), seed + 1, danger: "Low");
            next.AddAutonomousRaider("one", "raider-one", Start.AddSeconds(1600),
                personality: cooled, voiceState: sim.VoiceState("one"));
            var resumed = next.Project("one", Start.AddSeconds(1600));
            Assert.Equal(cooled.Greed, resumed.MyGreed);
            Assert.Equal(cooled.Mood, resumed.MyMood);
            Assert.NotEqual(p.Events.Last().Text.Split("Raider: ").Last(), resumed.Events.First().Text.Split("Raider: ").Last());
        }
    }
}
