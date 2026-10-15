using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace AFKRaiders.Server.Domain;

public sealed record ZoneConditionView(string Id, string Name, string Description, string Danger);
public sealed record DangerProfile(int MinimumExtractSeconds, double HazardChance, int HazardDamage,
    int RobotHealth, int RobotDamage, string RobotId, string RobotName, string RobotFlavor);
public sealed record RaiderPersonality(int Greed, int Mood);
public sealed record RaiderVoiceState(IReadOnlyDictionary<string, long> Counts, string? LastLine);
public sealed record HazardWarningView(string Location, DateTimeOffset ResolvesAt, int RawDamage);

public static class RaidConditions
{
    public const int DeadlineSeconds = 1320;
    public const int LastAdmissionSeconds = 300;
    public static readonly IReadOnlyList<ZoneConditionView> Conditions =
    [
        new("light_fog", "Light Fog", "A gentle haze. My confidence has requested brighter working conditions.", "Low"),
        new("apology_weather", "Apology Weather", "The drizzle appears apologetic. The footing is not accepting apologies.", "Medium"),
        new("overtime_red", "Overtime Red", "Even the exit signs look like they stayed past closing and want compensation.", "High")
    ];
    public static readonly IReadOnlyList<string> Zones =
        ["Damp Battlegrounds", "Chassis Graveyard", "The Staycation Celebration"];

    public static DangerProfile Profile(string danger) => danger switch
    {
        "Low" => new(900, 0.025, 10, 50, 8, "anxietick", "Anxietick",
            "It approaches, reconsiders, then files a worried metronome report."),
        "Medium" => new(960, 0.16, 22, 70, 10, "passive_aggressor", "Passive-Aggressor Drone",
            "It emits a disappointed sigh at industrial efficiency."),
        "High" => new(1020, 0.20, 30, 90, 10, "roomba_prime", "Roomba Prime",
            "Its cleaning mandate has escalated into a workplace grievance."),
        _ => throw new ArgumentException("Unknown danger profile.", nameof(danger))
    };

    public static double Roll(int seed, string key) =>
        BinaryPrimitives.ReadUInt32LittleEndian(SHA256.HashData(Encoding.UTF8.GetBytes($"g6:{seed}:{key}"))) /
        ((double)uint.MaxValue + 1);

    public static double Exposure(DateTimeOffset startsAt, DateTimeOffset at) =>
        Math.Clamp((at - startsAt).TotalSeconds / DeadlineSeconds, 0, 1);

    public static double HazardOdds(string danger, double exposure) =>
        Profile(danger).HazardChance * (1 + Math.Clamp(exposure, 0, 1) * 0.5);

    public static int Resilience(int mood) => Math.Clamp(mood, 0, 10) / 2;

    public static double Appetite(int greed, int mood) =>
        Math.Clamp(greed, 0, 100) / 100.0 + Math.Clamp(mood, -10, 10) / 50.0;

    public static double ExtractChance(int greed, int health, bool urgent, double urgency)
    {
        var chance = urgent ? 0.25 + Math.Clamp(urgency, 0, 1) * 0.65 : 0.45;
        chance -= Math.Clamp(greed, 0, 100) * 0.003;
        if (health <= 18) chance += 0.10;
        return Math.Clamp(chance, 0.05, 0.95);
    }
}
