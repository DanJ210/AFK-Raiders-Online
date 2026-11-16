using AFKRaiders.Server.Content;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class DiarySlotExtractorTests
{
    private static readonly DateTimeOffset At = DateTimeOffset.Parse("2026-11-08T00:00:00Z");

    [Fact]
    public void ExtractsOnlyCommittedOwnerVisibleFacts()
    {
        var item = Event("HiddenPocketSalvaged", "Secret Hidden Pocket preserved Raider's Widget, Mk. II (Value: 3)! Medbay drones recovered the concealed salvage.",
            item: "Raider's Widget, Mk. II") with { ItemValue = 3, ItemInstanceId = "item-one" };
        Assert.Equal("Raider's Widget, Mk. II", DiarySlotExtractor.Extract(item, "one").Item);
        Assert.Equal(new DiarySlots(), DiarySlotExtractor.Extract(item, "other"));

        var damage = Event("UnmitigatedDamage", "Shield depleted! Chassis takes 4 unmitigated kinetic damage from Captain's robot, Mk. II.");
        Assert.Equal("Captain's robot, Mk. II", DiarySlotExtractor.Extract(damage, "one").Source);
        Assert.Equal("Market", DiarySlotExtractor.Extract(damage, "one").Location);
        Assert.Null(DiarySlotExtractor.Extract(Event("Downed", "One is downed.", reason: "secret attacker"), "one").Source);
        Assert.Null(DiarySlotExtractor.Extract(Event("Downed", "One is downed. The floor is judgmental.", reason: "floor"), "one").Source);
        Assert.Null(DiarySlotExtractor.Extract(Event("KnockedOut", "One was knocked out. floor is mentioned.", reason: "floor"), "one").Source);
        Assert.Equal("Captain's robot, Mk. II", DiarySlotExtractor.Extract(Event("KnockedOut",
            "One was KNOCKED OUT. A line. Cause: Captain's robot, Mk. II.", reason: "Captain's robot, Mk. II"), "one").Source);
    }

    [Fact]
    public void SharedAwardsRequireVisibleMatchingRecipientAndUnambiguousItem()
    {
        var evt = new RaidEvent(1, At, "SearchCompleted", ["one", "two"], Location: "Market",
            Awards: [new("id", "Widget's End.", "two", "Two")],
            OriginalText: "Staycation locker search complete. Two recovered Widget's End..");
        Assert.Null(DiarySlotExtractor.Extract(evt, "one").Item);
        Assert.Equal("Widget's End.", DiarySlotExtractor.Extract(evt, "two").Item);
        Assert.Null(DiarySlotExtractor.Extract(evt with { Awards = null }, "two").Item);
        Assert.Null(DiarySlotExtractor.Extract(evt with { OriginalText = "No awards." }, "two").Item);
    }

    [Fact]
    public void ParsesExactConditionRobotAndActivityReceipts()
    {
        var hazard = Event("HazardWarning", "Apology Weather: exposed footing at Market. Impact in 10s; 12 raw damage before resilience/shields.");
        Assert.Equal("Apology Weather", DiarySlotExtractor.Extract(hazard, "one").Condition);
        Assert.Null(DiarySlotExtractor.Extract(hazard with { Location = "Salvage Yard" }, "one").Condition);
        var robot = Event("RobotDefeated", "Roomba Prime is disabled. Its salvage cargo is claimed on the spot; the Service Depot locker has nothing further to offer.");
        Assert.Equal("Roomba Prime", DiarySlotExtractor.Extract(robot, "one").Robot);
        var encounter = Event("RobotCombatStarted", "One encounters Captain's Bot, Mk. II at Market. It beeps.");
        Assert.Equal("Captain's Bot, Mk. II", DiarySlotExtractor.Extract(encounter, "one").Robot);
        Assert.Null(DiarySlotExtractor.Extract(encounter with
        {
            OriginalText = "One encounters x encounters Bot at Market. It beeps."
        }, "one").Robot);
        Assert.Equal("Captain's Bot, Mk. II", DiarySlotExtractor.Extract(Event("RobotParticipantJoined",
            "One joins the fight against Captain's Bot, Mk. II. The shared health pool is 12/12."), "one").Robot);
        var interrupted = Event("ActivityInterrupted", "Rummaging interrupted for extraction; shared deadlines and other Raiders continue.", activity: "Rummaging");
        Assert.Equal("rummaging", DiarySlotExtractor.Extract(interrupted, "one").Activity);
        Assert.Equal("fighting a robot", DiarySlotExtractor.Extract(interrupted with
        {
            Activity = "FightingRobot",
            OriginalText = "FightingRobot interrupted for extraction; shared deadlines and other Raiders continue."
        }, "one").Activity);
        Assert.Equal("waiting for an exit decision", DiarySlotExtractor.Extract(interrupted with
        {
            Activity = "WaitingForExitDecision",
            OriginalText = "WaitingForExitDecision interrupted for extraction; shared deadlines and other Raiders continue."
        }, "one").Activity);
        Assert.Null(DiarySlotExtractor.Extract(interrupted with { Activity = "Ready" }, "one").Activity);
        Assert.Null(DiarySlotExtractor.Extract(interrupted with { OriginalText = "not a receipt" }, "one").Activity);
        Assert.Equal("surveying", DiarySlotExtractor.Extract(Event("AreaDiary", "One watches the area.", activity: "Surveying"), "one").Activity);
    }

    [Fact]
    public void PersonalityDirectionsUseReceiptAndRequireUnequalValues()
    {
        var evt = Event("PersonalityChanged", "Completed searching: Greed 3 -> 4; Mood 2 -> 1; resilience 0.", reason: "Completed searching");
        var slots = DiarySlotExtractor.Extract(evt, "one");
        Assert.Equal("searching", slots.Activity);
        Assert.Equal(DiaryDirection.Increased, slots.GreedDirection);
        Assert.Equal(DiaryDirection.Decreased, slots.MoodDirection);
        var equal = evt with { OriginalText = "Completed searching: Greed 4 -> 4; Mood 1 -> 1; resilience 0." };
        Assert.Null(DiarySlotExtractor.Extract(equal, "one").GreedDirection);
        Assert.Null(DiarySlotExtractor.Extract(equal, "one").MoodDirection);
        Assert.Null(DiarySlotExtractor.Extract(evt with { Reason = "Staying deeper" }, "one").GreedDirection);
        Assert.Null(DiarySlotExtractor.Extract(evt with { OriginalText = "malformed" }, "one").Activity);
    }

    [Fact]
    public void NoMappingOrEmptyFactsNeverProduceGuessedSlotsAndShieldNeedsPositiveLoss()
    {
        var evt = new RaidEvent(1, At, "OpportunityExhausted", ["one"], OpportunityId: "market-locker");
        Assert.Null(DiarySlotExtractor.Extract(evt, "one").Opportunity);
        IReadOnlyDictionary<string, string> fixtureNames = new Dictionary<string, string>
        {
            ["market-locker"] = "Staycation Locker",
            ["raw"] = "raw"
        };
        Assert.Equal("Staycation Locker", DiarySlotExtractor.Extract(evt, "one", fixtureNames).Opportunity);
        Assert.Null(DiarySlotExtractor.Extract(evt with { OpportunityId = "raw" }, "one", fixtureNames).Opportunity);
        Assert.Null(DiarySlotExtractor.Extract(evt with { OpportunityId = "unknown" }, "one", fixtureNames).Opportunity);
        Assert.Equal(new DiarySlots(), DiarySlotExtractor.Extract(evt with { OpportunityId = "unknown" }, "one"));
        var shield = Event("ShieldAbsorbed", "Shield lost 0 charge, 0 HP got through. Raider called that 'a learning invoice.'");
        Assert.False(DiarySlotExtractor.Extract(shield, "one").PositiveShieldLoss);
        Assert.True(DiarySlotExtractor.Extract(shield with { OriginalText = "Shield lost 2 charge, 0 HP got through. Raider called that 'a learning invoice.'" }, "one").PositiveShieldLoss);
        Assert.False(DiarySlotExtractor.Extract(shield with { OriginalText = "Shield lost 2 charge." }, "one").PositiveShieldLoss);
    }

    private static RaidEvent Event(string kind, string text, string? reason = null,
        string? activity = null, string? item = null) =>
        new(1, At, kind, ["one"], Location: "Market", Activity: activity,
            ItemName: item, Reason: reason, OriginalText: text);
}
