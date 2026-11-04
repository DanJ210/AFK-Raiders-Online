namespace AFKRaiders.Server.Domain;

internal enum BalanceExperimentArm
{
    Arm3Baseline,
    Arm4A,
    Arm4B,
    Arm5
}

internal static class BalanceExperimentRules
{
    private static readonly IReadOnlyList<RobotDefeatEntry> Arm4AHighCargo =
    [
        new("circuit_bundle_messy", 30),
        new("arc_data_shard", 30),
        new("energy_core_overcharged", 20),
        new("arc_data_shard_legendary", 20)
    ];

    private static readonly IReadOnlyList<RobotDefeatEntry> Arm4BHighCargo =
    [
        new("circuit_bundle_messy", 25),
        new("arc_data_shard", 30),
        new("energy_core_overcharged", 20),
        new("arc_data_shard_legendary", 25)
    ];

    public static string RulesVersion(BalanceExperimentArm arm) => arm switch
    {
        BalanceExperimentArm.Arm3Baseline => "desperanza-bal-4-arm3-v1",
        BalanceExperimentArm.Arm4A => "desperanza-bal-4-arm4a-v1",
        BalanceExperimentArm.Arm4B => "desperanza-bal-4-arm4b-v1",
        BalanceExperimentArm.Arm5 => "desperanza-bal-4-arm5-v1",
        _ => throw new ArgumentOutOfRangeException(nameof(arm))
    };

    public static string RewardsVersion(BalanceExperimentArm arm) => arm switch
    {
        BalanceExperimentArm.Arm3Baseline => "bal-4-defeat-v1",
        BalanceExperimentArm.Arm4A => "bal-4-defeat-arm4a-v1",
        BalanceExperimentArm.Arm4B => "bal-4-defeat-arm4b-v1",
        BalanceExperimentArm.Arm5 => "bal-4-defeat-v1",
        _ => throw new ArgumentOutOfRangeException(nameof(arm))
    };

    public static IReadOnlyList<RobotDefeatEntry> CargoTable(
        string danger,
        BalanceExperimentArm arm)
    {
        if (danger == "High")
        {
            return arm switch
            {
                BalanceExperimentArm.Arm4A => Arm4AHighCargo,
                BalanceExperimentArm.Arm4B => Arm4BHighCargo,
                BalanceExperimentArm.Arm3Baseline or BalanceExperimentArm.Arm5 =>
                    RobotDefeatRewards.CargoByDanger[danger],
                _ => throw new ArgumentOutOfRangeException(nameof(arm))
            };
        }

        return arm switch
        {
            BalanceExperimentArm.Arm3Baseline or BalanceExperimentArm.Arm4A
                or BalanceExperimentArm.Arm4B or BalanceExperimentArm.Arm5 =>
                RobotDefeatRewards.CargoByDanger.TryGetValue(danger, out var table)
                    ? table
                    : throw new InvalidOperationException($"No robot defeat cargo table exists for danger '{danger}'."),
            _ => throw new ArgumentOutOfRangeException(nameof(arm))
        };
    }

    public static bool BlocksHigh(BalanceExperimentArm arm, TimeSpan age, bool highReady)
    {
        if (arm == BalanceExperimentArm.Arm5 && age < TimeSpan.FromHours(8))
            return true;

        return age < TimeSpan.FromHours(24) && !highReady;
    }
}
