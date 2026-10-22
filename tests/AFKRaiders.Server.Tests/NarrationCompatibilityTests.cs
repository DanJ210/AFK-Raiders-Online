using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

public sealed class NarrationCompatibilityTests(ITestOutputHelper output)
{
    [Fact]
    public void BundledNarrationAndCurrentSimulationHaveAStableCompatibilityFingerprint()
    {
        var kinds = new[] { "Deployment", "DutyAdmission", "Departure", "Travel", "Arrived", "Search",
            "SearchStarted", "CrossedPaths", "SearchJoined", "OpportunityExhausted", "SearchCompleted",
            "EncounterResolved", "ItemAllocated", "ItemAwarded", "PackingStarted", "PackingCompleted",
            "RecoveryStarted", "RecoveryCompleted", "RobotCombatStarted", "RobotJoined", "RobotParticipantJoined",
            "RobotRound", "RobotDefeated", "RobotEscaped", "ExtractionStarted", "ExtractionVehicleArrived", "Extracted", "ExtractionCompleted",
            "SharedExtractionCalled", "ExtractionJoined",
            "Downed", "KnockedOut", "HiddenPocketSalvaged", "HiddenPocketEmpty", "ConsumableUsed", "HandlerAction",
            "CallExtract", "Calm", "Pressure", "DutyEnabled", "DutyPaused", "ReturnedHome", "RaidWorldClosed",
            "AreaDiary", "AreaActivityStarted", "AreaActivityCompleted", "WorldCondition", "Unknown" };
        var activities = new[] { "Surveying", "Checking", "Rummaging", "Searching", "FightingRobot", "Extracting", "Hub", "Unknown" };
        var narrator = new TemplateRaidNarrator();
        var start = DateTimeOffset.Parse("2026-10-14T00:00:00Z");
        var lines = new List<string>();
        lines.AddRange(SurvivalCanonContent.DownedLines);
        lines.AddRange(SurvivalCanonContent.KnockedOutLines);
        lines.AddRange(SurvivalCanonContent.RobotCombatProgress);
        foreach (var activity in activities)
        {
            foreach (var location in new[] { "Market", "Service Depot" })
                lines.AddRange(RaiderCommsContent.ResidenceTasks(activity, location));
            for (var sequence = -8; sequence < 32; sequence++)
                lines.Add(RaiderCommsContent.StyleHint(activity, sequence));
        }
        foreach (var kind in kinds)
            foreach (var activity in activities)
                for (var sequence = -8; sequence < 32; sequence++)
                {
                    lines.Add(RaiderCommsContent.Line(kind, activity, sequence, "Hoarder"));
                    lines.Add(RaiderCommsContent.Line(kind, activity, sequence, rotate: true));
                }
        foreach (var kind in kinds)
            foreach (var opportunity in new[] { "market-locker", "humming-crate", "depot-locker", "unknown" })
                foreach (var resolution in new[] { "SoloSearch", "Cooperation", "Contest", "Unknown" })
                    lines.Add(narrator.FormatEvent(new(5, start, kind, ["one"], Location: "Market",
                        Destination: "Salvage Yard", DurationSeconds: 12, ItemName: "Basic Scrap",
                        ItemValue: 8, OpportunityId: opportunity, Resolution: resolution,
                        Awards: [new("item", "Basic Scrap", "one", "Raider One")]), null));
        foreach (var risk in new[] { false, true })
            for (var seed = 0; seed < 16; seed++)
            {
                // Freeze C4's original loot inputs; later loot expansions deliberately change outcomes.
                var sim = new RaidSimulation("compatibility", start, seed,
                    catalog: CatalogCompatibilityTests.C3Baseline(), fastPacing: false, riskReward: risk);
                sim.AddAutonomousRaider("one", "raider-one", start);
                sim.AddAutonomousRaider("two", "raider-two", start);
                sim.Advance(start.AddHours(1));
                lines.Add(JsonSerializer.Serialize(sim.Project("one", start.AddHours(1))));
                lines.Add(JsonSerializer.Serialize(sim.Project("two", start.AddHours(1))));
            }
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", lines))));
        output.WriteLine($"Current narration and simulation fingerprint: {hash}");
        // ECON-2 v2 intentionally changes the rules version for damaged field weapon finds.
        Assert.Equal("2F08B779B7D6147A46A18D9EEC2852BE753216180A249FA80225F2DBFA76541D", hash);
    }
}
