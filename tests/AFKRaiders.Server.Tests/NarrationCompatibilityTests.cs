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

        Assert.Equal("4C3D0A00E28B25D579E8AE72D3EDC2422532088463BC3A87DB1BA3BF386FE4AC",
            fingerprints[BalanceExperimentArm.Arm3Baseline]);
        Assert.Equal("6BCECA6CE60AC87DAE8EB5D53FDD3635C1104E0A27F8615AFBEE78CC53F70C87",
            fingerprints[BalanceExperimentArm.Arm4A]);
        Assert.Equal("6285BA484563A01061EFC86D38060166A8AADF646C991D57837858AC8C94F5F5",
            fingerprints[BalanceExperimentArm.Arm4B]);
        Assert.Equal("A26935F4A799EA192DC3999489C0A6161CF1D1011A1BEF564FDB160D0C8D6744",
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
