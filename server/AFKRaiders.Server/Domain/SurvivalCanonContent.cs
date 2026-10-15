using AFKRaiders.Server.Content;

namespace AFKRaiders.Server.Domain;

/// <summary>
/// JSON-backed online adaptations of original survival content; provenance is recorded in README.md.
/// Conforms to docs/lore/content-guidelines/TONE_VOICE.md.
/// </summary>
public static class SurvivalCanonContent
{
    public static readonly IReadOnlyList<string> DownedLines =
        NarrationContent.Default.Pool("survival.downed");

    public static readonly IReadOnlyList<string> KnockedOutLines =
        NarrationContent.Default.Pool("survival.knocked-out");

    public static readonly IReadOnlyList<string> RobotCombatProgress =
        NarrationContent.Default.Pool("survival.robot-progress");

    public static string GetDownedLine(int index) =>
        DownedLines[Math.Abs(index) % DownedLines.Count];

    public static string GetKnockedOutLine(int index) =>
        KnockedOutLines[Math.Abs(index) % KnockedOutLines.Count];

    public static string GetRandomDownedLine(long seed) =>
        DownedLines[(int)(Math.Abs(seed) % DownedLines.Count)];

    public static string GetRandomKnockedOutLine(long seed) =>
        KnockedOutLines[(int)(Math.Abs(seed) % KnockedOutLines.Count)];
}
