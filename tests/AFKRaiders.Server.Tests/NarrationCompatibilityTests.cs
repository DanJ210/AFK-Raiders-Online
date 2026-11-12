using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AFKRaiders.Server.Domain;
using Xunit.Abstractions;

namespace AFKRaiders.Server.Tests;

public sealed class NarrationCompatibilityTests(ITestOutputHelper output)
{
    [Fact]
    public void EachBalanceArmHasAStableCompatibilityFingerprint()
    {
        var fingerprints = Enum.GetValues<BalanceExperimentArm>()
            .ToDictionary(arm => arm, ComputeFingerprint);
        foreach (var (arm, fingerprint) in fingerprints)
            output.WriteLine($"{arm} ({BalanceExperimentRules.RulesVersion(arm)}): {fingerprint}");

        // DIARY VARIETY changes authored routing/selection only; gameplay has a separate matched gate.
        Assert.Equal("5B1952FBA537B825876C72B31472E6BEC949287B171723F0D3114FAB44E523A4",
            fingerprints[BalanceExperimentArm.Arm3Baseline]);
        Assert.Equal("7DF539331C26D1DBD89BCB3C499AD7BFC4DB8C0C322641BDE842EAA6F24880CC",
            fingerprints[BalanceExperimentArm.Arm4A]);
        Assert.Equal("5CC0D89687CD23C9868E213D1E5A84FE680BC31D648CE7C19E18CFE35CE756B9",
            fingerprints[BalanceExperimentArm.Arm4B]);
        Assert.Equal("30D651C3F659502A7FBD23177EB75CFA3ED8E761BD934E246E7F7AC0E234B1CE",
            fingerprints[BalanceExperimentArm.Arm5]);
        Assert.Equal(4, fingerprints.Values.Distinct(StringComparer.Ordinal).Count());

        string ComputeFingerprint(BalanceExperimentArm arm)
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
                    narrator: null,
                    catalog: CatalogCompatibilityTests.C3Baseline(),
                    useTableBackedLoot: false,
                    fastPacing: false,
                    riskReward: risk,
                    danger: null,
                    conditionMomentum: 0,
                    balanceExperimentArm: arm);
                sim.AddAutonomousRaider("one", "raider-one", start);
                sim.AddAutonomousRaider("two", "raider-two", start);
                sim.Advance(start.AddHours(1));
                lines.Add(JsonSerializer.Serialize(sim.Project("one", start.AddHours(1))));
                lines.Add(JsonSerializer.Serialize(sim.Project("two", start.AddHours(1))));
            }
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", lines))));
        }
    }
}
