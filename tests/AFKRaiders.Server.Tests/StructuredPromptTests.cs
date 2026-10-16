using AFKRaiders.Server.AI;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class StructuredPromptTests
{
    private static AmbientCommsContext Robot(int nearby = 0) => new("Salvage Yard", "RobotRound",
        EventKind: "RobotRound", Condition: "Overtime Red", Danger: "High", RobotName: "Roomba Prime", NearbyRaiders: nearby);

    [Theory]
    [InlineData("RobotRound", "RobotRound", 0, "Robot")]
    [InlineData("RobotRound", "RobotRound", 2, "SharedRobot")]
    [InlineData("Search", "Searching", 0, "Search")]
    [InlineData("Travel", "Travel", 0, "Travel")]
    [InlineData("ExtractionStarted", "Extracting", 0, "Exit")]
    [InlineData("AreaDiary", "Lingering", 1, "Diary")]
    [InlineData("Downed", "Downed", 0, "Floor")]
    [InlineData("KnockedOut", "KnockedOut", 0, "Floor")]
    [InlineData("AreaActivityStarted", "Surveying", 0, "AreaActivity")]
    [InlineData(null, "Idle", 0, "General")]
    public void Categories_follow_the_committed_event_kind(string? kind, string activity, int nearby, string expected)
    {
        var context = new AmbientCommsContext("Market", activity, EventKind: kind, NearbyRaiders: nearby);
        Assert.Equal(expected, OnnxGenAiNarrativeGenerator.CategoryFor(context).ToString());
    }

    [Fact]
    public void Robot_prompt_carries_server_facts_and_lore_without_identities()
    {
        var prompt = OnnxGenAiNarrativeGenerator.BuildAmbientPrompt(Robot(), 3);

        Assert.Contains("Place: Salvage Yard, humming, vibrating crates.", prompt);
        Assert.Contains("facing a hostile robot", prompt);
        Assert.Contains("Weather: Overtime Red, high danger.", prompt);
        Assert.Contains("Robot: Roomba Prime", prompt);
        Assert.Contains("never say who wins or gets hurt", prompt);
        Assert.DoesNotContain("player", prompt, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Company nearby", prompt);
        Assert.EndsWith("I consider", prompt);
    }

    [Fact]
    public void Shared_robot_and_company_cues_reflect_nearby_raiders()
    {
        Assert.Contains("robot with company nearby", OnnxGenAiNarrativeGenerator.BuildAmbientPrompt(Robot(nearby: 1)));
        var search = OnnxGenAiNarrativeGenerator.BuildAmbientPrompt(
            new AmbientCommsContext("Market", "Searching", EventKind: "Search", Condition: "Light Fog", Danger: "Low", RobotName: "Anxietick", NearbyRaiders: 2));
        Assert.Contains("searching through containers", search);
        Assert.Contains("Company nearby.", search);
        Assert.Contains("No combat.", search);
        Assert.DoesNotContain("Anxietick", search);
    }

    [Theory]
    [InlineData("I consider Roomba Prime a loud apology.")]
    [InlineData("I distrust that robot and its tiny brushes.")]
    [InlineData("I consider the Salvage Yard a humming opinion.")]
    [InlineData("I consider Overtime Red rather theatrical weather.")]
    public void Robot_context_accepts_grounded_mockery(string text)
    {
        var terms = OnnxGenAiNarrativeGenerator.AllowedTermsFor(Robot());
        Assert.Equal(text, OnnxGenAiNarrativeGenerator.ValidateAmbient(text, out var reason, terms));
        Assert.Null(reason);
    }

    [Theory]
    [InlineData("I won against the robot today.", "ForbiddenOutcomeOrActorWord:won")]
    [InlineData("I consider that robot badly hurt now.", "ForbiddenOutcomeOrActorWord:hurt")]
    [InlineData("I consider Anxietick a nervous kettle.", "UnknownProperNoun:Anxietick")]
    public void Robot_context_still_rejects_outcomes_and_unapproved_names(string text, string expected)
    {
        Assert.Null(OnnxGenAiNarrativeGenerator.ValidateAmbient(text, out var reason, OnnxGenAiNarrativeGenerator.AllowedTermsFor(Robot())));
        Assert.Equal(expected, reason);
    }

    [Fact]
    public void Non_robot_contexts_reject_robot_words_and_names()
    {
        var terms = OnnxGenAiNarrativeGenerator.AllowedTermsFor(
            new AmbientCommsContext("Market", "Searching", EventKind: "Search", RobotName: "Roomba Prime"));
        OnnxGenAiNarrativeGenerator.ValidateAmbient("I distrust that robot in the Market.", out var reason, terms);
        Assert.Equal("ForbiddenOutcomeOrActorWord:robot", reason);
        OnnxGenAiNarrativeGenerator.ValidateAmbient("I consider Roomba Prime a dusty rumour.", out reason, terms);
        Assert.Equal("UnknownProperNoun:Roomba", reason);
    }

    [Fact]
    public void Hyphenated_robot_names_are_allowed_whole_and_in_parts()
    {
        var terms = OnnxGenAiNarrativeGenerator.AllowedTermsFor(new AmbientCommsContext("Service Depot", "RobotRound",
            EventKind: "RobotRound", Condition: "Apology Weather", Danger: "Medium", RobotName: "Passive-Aggressor Drone"));
        Assert.NotNull(OnnxGenAiNarrativeGenerator.ValidateAmbient("I consider the Passive-Aggressor Drone overly sincere.", out _, terms));
        Assert.NotNull(OnnxGenAiNarrativeGenerator.ValidateAmbient("I distrust drones during Apology Weather.", out _, terms));
    }
}
