using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AFKRaiders.Server.AI;
using AFKRaiders.Server.Content;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class DiaryVarietyTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-11-08T00:00:00Z");

    [Fact]
    public void ExhaustionUsesLeastRecentEligibleLineWithStableTies()
    {
        var memory = new NarrationSelectionMemory();
        Assert.Equal("a", NarrationSelectionMemory.Select(["a", "b"], [memory]));
        Assert.Equal("b", NarrationSelectionMemory.Select(["a", "b"], [memory]));
        Assert.Equal("a", NarrationSelectionMemory.Select(["a", "b"], [memory]));
        Assert.Equal("b", NarrationSelectionMemory.Select(["a", "b"], [memory]));
        Assert.Throws<InvalidOperationException>(() => NarrationSelectionMemory.Select([], [memory]));
        Assert.Equal("only", NarrationSelectionMemory.Select(["only"], [memory]));
        Assert.Equal("only", NarrationSelectionMemory.Select(["only"], [memory]));
    }

    [Fact]
    public void EligibilityPrecedesRecencyAndCheckpointPreservesHubMemory()
    {
        var memory = new NarrationSelectionMemory();
        NarrationSelectionMemory.Select(["eligible"], [memory]);
        Assert.Equal("eligible", NarrationSelectionMemory.Select(["eligible"], [memory]));
        var restored = new NarrationSelectionMemory();
        restored.Restore(memory.Snapshot());
        var hub = NarrationContent.Default.Pool("hub.activities");
        for (var sequence = 0; sequence < hub.Count * 3; sequence++)
            Assert.Equal(NarrationSelectionMemory.Select(hub, [memory]),
                NarrationSelectionMemory.Select(hub, [restored]));
        Assert.DoesNotContain("never-eligible", memory.Snapshot().LineUses!.Keys);
    }

    [Fact]
    public void SharedSelectionUsesEachRecipientsRecencyNotTheirDifferentQuoteCounts()
    {
        var one = new NarrationSelectionMemory();
        var two = new NarrationSelectionMemory();
        NarrationSelectionMemory.Select(["b"], [one]);
        for (var i = 0; i < 100; i++) NarrationSelectionMemory.Select(["other"], [one]);
        NarrationSelectionMemory.Select(["a"], [one]);
        NarrationSelectionMemory.Select(["b"], [two]);
        Assert.Equal("a", NarrationSelectionMemory.Select(["a", "b"], [one, two]));
    }

    [Theory]
    [InlineData(101)]
    [InlineData(202)]
    [InlineData(303)]
    public void OrderedCommittedStreamReplaysExactlyIncludingSharedEvents(int seed)
    {
        var sim = new RaidSimulation("replay", Start, seed, danger: "Medium");
        sim.AddAutonomousRaider("one", "raider-one", Start);
        sim.AddAutonomousRaider("two", "raider-two", Start.AddSeconds(30));
        sim.Advance(Start.AddHours(1));
        var one = sim.Project("one", Start.AddHours(1));
        var two = sim.Project("two", Start.AddHours(1));
        var events = one.Events.Concat(two.Events).DistinctBy(evt => evt.Sequence).OrderBy(evt => evt.Sequence).ToArray();
        Assert.Contains(events, evt => evt.ParticipantIds.Count == 2);
        var memories = new Dictionary<string, NarrationSelectionMemory>
        {
            ["one"] = new(), ["two"] = new()
        };
        var traits = one.Players.ToDictionary(player => player.PlayerId, player => player.PrimaryTrait);
        foreach (var evt in events.Where(evt => evt.ParticipantIds.Count > 0))
        {
            var primaryTrait = evt.ParticipantIds.Count == 1 ? traits[evt.ParticipantIds[0]] : null;
            var selected = NarrationSelectionMemory.Select(
                RaiderCommsContent.EligibleLines(evt.Kind, evt.Activity, primaryTrait),
                evt.ParticipantIds.Select(id => memories[id]).ToArray());
            Assert.Equal($"{evt.FactsText}\nRaider: {selected}", evt.Text);
            var formatted = new TemplateRaidNarrator().FormatEvent(
                new(evt.Sequence, evt.At, evt.Kind, evt.ParticipantIds, OriginalText: evt.FactsText,
                    Details: new Dictionary<string, string> { ["SelectedLine"] = selected }), null);
            Assert.Equal(evt.Text, formatted);
        }
    }

    [Fact]
    public void EventFitReuseDoesNotSelectLeaveOrCombatClaimsForIncompatibleOutcomes()
    {
        foreach (var trait in Enum.GetValues<RaiderPrimaryTrait>())
        {
            var decisions = RaiderCommsContent.EligibleLines("ExtractionDecision", null, trait.ToString());
            Assert.DoesNotContain(decisions, line => line.Contains("I'm ready") || line.Contains("I will board"));
            var aftermath = RaiderCommsContent.EligibleLines("RobotDefeated", null, trait.ToString());
            Assert.DoesNotContain(aftermath, line => line.Contains("arguing") || line.Contains("warning tone"));
            Assert.Equal("voice.arrival", RaiderCommsContent.PoolKey("Arrived", null));
            Assert.Equal("voice.packing", RaiderCommsContent.PoolKey("PackingCompleted", null));
        }
    }

    [Theory]
    [InlineData(RaiderPrimaryTrait.Cautious)]
    [InlineData(RaiderPrimaryTrait.Hoarder)]
    [InlineData(RaiderPrimaryTrait.Brawler)]
    [InlineData(RaiderPrimaryTrait.Gadgeteer)]
    public void AbsentAndRepeatedlyPolledHandlerHaveIdenticalQuotesAndOutcomes(RaiderPrimaryTrait trait)
    {
        var absent = Run(trait, 101, false, 1);
        var polled = Run(trait, 101, true, 1);
        Assert.Equal(absent.FullJournal!.Select(evt => evt.Text), polled.FullJournal!.Select(evt => evt.Text));
        Assert.Equal(AuthoritativeJson(absent), AuthoritativeJson(polled));
    }

    [Theory]
    [InlineData(RaiderPrimaryTrait.Cautious, 101, "779BF840A4B9EEDE74C4CBA4B8DEA217AF2A83EE5FB37124CDCAD4732D801744")]
    [InlineData(RaiderPrimaryTrait.Cautious, 202, "1B5DD5DEC6ABBFC966A903D4D752E3DDFA3A8536E92231570610E44F191C7728")]
    [InlineData(RaiderPrimaryTrait.Cautious, 303, "3E35BD2469BA194C34889E457BB7F6E47B48EB37ABF83B489A96D466B51A486B")]
    [InlineData(RaiderPrimaryTrait.Hoarder, 101, "897B6CE33C6C4245E06944A1DDF112BC0234F2F6A71DB161966B2D4CC8229AE5")]
    [InlineData(RaiderPrimaryTrait.Hoarder, 202, "8DA9D514FE47482B3A6EB24169964702B089EDDF7589F78CD9E3588EAD5EFA8B")]
    [InlineData(RaiderPrimaryTrait.Hoarder, 303, "ACDF75C2BC66803C914A76D8D703C2BB3B1A8987787792F9B0D7D13569317958")]
    [InlineData(RaiderPrimaryTrait.Brawler, 101, "C6B745A63428214B3B46494F3A0B9403BE405AB0A58E6AA62DCF375F799CD1F0")]
    [InlineData(RaiderPrimaryTrait.Brawler, 202, "52266B94E71CC5BF58857BD9D2E430F344EBFE4BB215EF8F2B246BF9E379ABC0")]
    [InlineData(RaiderPrimaryTrait.Brawler, 303, "030DAC439CE90DFB2566048DA5D757FD0BBAA691C2E3EB01202D22FD6C6F1560")]
    [InlineData(RaiderPrimaryTrait.Gadgeteer, 101, "0EA6F95A27311D82EF151346152A3E8EAA0461D3403D44BA78FE9F448AAD17E9")]
    [InlineData(RaiderPrimaryTrait.Gadgeteer, 202, "A32B6E1B4C1AFDDD0EED03780217F4257A45F17335E636187BFAC9E12974529C")]
    [InlineData(RaiderPrimaryTrait.Gadgeteer, 303, "A0882EB97A080B2D7FC30F4006F9720E0C01CAA6CF6D1FBA5C354655E5865BFB")]
    public void EightHourGameplayOutcomesMatchPreSliceBaseline(RaiderPrimaryTrait trait, int seed, string expected)
    {
        // Captured from untouched e1dfa85 with the same clock/cohort and canonicalization.
        var actual = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes(AuthoritativeJson(Run(trait, seed, false, 8)))));
        Assert.Equal(expected, actual);
    }

    internal static HandlerProjection Run(RaiderPrimaryTrait trait, int seed, bool poll, int hours)
    {
        var clock = new FixedClock(Start);
        var nextSeed = 0;
        var nextId = 0;
        var coordinator = new RaidCoordinator(clock, narrator: new TemplateRaidNarrator(),
            narrativeGenerator: new TemplateNarrativeGenerator(),
            nextSeed: () => nextSeed++ == 0 ? (int)trait : seed * 1000 + nextSeed,
            traderSeed: seed, nextId: () => $"analysis-{seed}-{nextId++:D4}");
        var enrolled = coordinator.EnrollHandler("raider-one", $"diary-variety-{trait}-{seed}");
        for (var seconds = 0; seconds <= hours * 3600; seconds += 5)
        {
            clock.Now = Start.AddSeconds(seconds);
            coordinator.AdvanceRaids();
            if (!poll) continue;
            coordinator.GetHandlerProjection(enrolled.AccessToken);
            coordinator.GetHandlerProjection(enrolled.AccessToken);
        }
        return coordinator.GetHandlerProjection(enrolled.AccessToken);
    }

    internal static string AuthoritativeJson(HandlerProjection projection)
    {
        var node = JsonSerializer.SerializeToNode(projection)!;
        RemovePresentation(node);
        // Only free starter IDs are GUID-derived; retain their reference relationships.
        var weaponIds = new Dictionary<string, string>(StringComparer.Ordinal);
        return Regex.Replace(node.ToJsonString(), @"wpn_tea_kettle_[a-f0-9]{5}(?![a-z0-9])", match =>
        {
            if (!weaponIds.TryGetValue(match.Value, out var id))
                weaponIds.Add(match.Value, id = $"starter-kettle-{weaponIds.Count}");
            return id;
        });
    }

    private static void RemovePresentation(JsonNode node)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            var index = text.IndexOf("\nRaider: ", StringComparison.Ordinal);
            if (index >= 0) node.ReplaceWith(text[..index]);
        }
        else if (node is JsonObject obj)
        {
            foreach (var key in new[] { "Text", "Flavor", "NarrativeSource", "EnrichmentStatus",
                "EnrichmentReason", "AccessToken", "DevelopmentAccessToken", "GeneratedAt",
                "DebriefSummary", "Highlights", "AarDebrief", "TopItemAppraisal" }) obj.Remove(key);
            foreach (var child in obj.ToArray()) if (child.Value is not null) RemovePresentation(child.Value);
        }
        else if (node is JsonArray array)
            foreach (var child in array.ToArray()) if (child is not null) RemovePresentation(child);
    }

    private sealed class FixedClock(DateTimeOffset initial) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = initial;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
