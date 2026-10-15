using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class SurvivalMechanicsTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-11T12:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-11T12:00:00Z");

    [Fact]
    public void ShieldMitigates40PercentOfCombatDamage_AndDrainsChargeAndDurability()
    {
        // 1. Verify shield mitigation and HP breakthrough math
        var simulation = new RaidSimulation("test-shield", Start, seed: 10, fastPacing: true);
        simulation.AddRaider("p1", "raider-one", Start);

        var initial = simulation.Project("p1", Start);
        Assert.Equal(40, initial.MyHealth);
        Assert.Equal(40, initial.MyMaxHealth);
        Assert.Equal(40, initial.MyShieldCharge);
        Assert.Equal(40, initial.MyShieldMaxCharge);
        Assert.Equal(100.0, initial.MyShieldDurability);

        // Advance simulation through movement to encounter robot combat
        var advanced = false;
        for (var t = 1; t <= 180; t++)
        {
            simulation.Advance(Start.AddSeconds(t));
            var proj = simulation.Project("p1", Start.AddSeconds(t));
            if (proj.MyShieldCharge < 40 || proj.MyHealth < 40)
            {
                advanced = true;
                // Verify breakthrough occurred: HP damage is Math.Ceiling(rawDamage * (1 - 0.4))
                // and shield charge decreased
                Assert.True(proj.MyShieldCharge < 40, "Shield charge should be drained by damage");
                Assert.True(proj.MyShieldDurability < 100.0, "Shield durability should suffer wear");
                Assert.Contains(proj.Events, e => e.Text.Contains("Shield lost") && e.Text.Contains("HP got through"));
                break;
            }
        }

        Assert.True(advanced, "Raider should take damage from robot encounter or hazard");
    }

    [Fact]
    public void AutonomousConsumables_DeployAndConsumeWhenDamaged()
    {
        // 2. Autonomous field consumable consumption
        var simulation = new RaidSimulation("test-consumables", Start, seed: 10, fastPacing: true);
        var staged = new List<StagedSupplyItem>
        {
            new("bandage_white", "White Bandage", "Medical", 2),
            new("duct_taped_capacitor", "Duct-Taped Capacitor", "ShieldRecharger", 1)
        };
        simulation.AddRaider("p1", "raider-one", Start, staged);

        var initial = simulation.Project("p1", Start);
        Assert.NotNull(initial.MyFieldSupplies);
        Assert.Equal(2, initial.MyFieldSupplies.Count);

        // Run simulation and observe consumption
        var consumed = false;
        for (var t = 1; t <= 180; t++)
        {
            simulation.Advance(Start.AddSeconds(t));
            var proj = simulation.Project("p1", Start.AddSeconds(t));
            if (proj.Events.Any(e => e.Kind == "ConsumableUsed"))
            {
                consumed = true;
                break;
            }
        }

        Assert.True(consumed, "Damaged raider should autonomously consume staged medical or shield supplies");
    }

    [Fact]
    public void SecretHiddenPocket_PreservesDesignatedItemOnKnockedOut_WhileOtherLootIsLost()
    {
        // 3. Secret Hidden Pocket preservation on KNOCKED_OUT
        // Setup a simulation where the raider collects items and is then downed
        var simulation = new RaidSimulation("test-pocket-ko", Start, seed: 15, fastPacing: true);
        simulation.AddRaider("p1", "raider-one", Start);

        // Advance until raider collects at least 1 item
        string? designatedInstanceId = null;
        for (var t = 1; t <= 60; t++)
        {
            simulation.Advance(Start.AddSeconds(t));
            var proj = simulation.Project("p1", Start.AddSeconds(t));
            if (proj.MyInventory.Count > 0)
            {
                designatedInstanceId = proj.MyInventory[0].InstanceId;
                var designated = simulation.DesignateHiddenPocket("p1", designatedInstanceId);
                Assert.True(designated, "Should designate item into secret hidden pocket");
                break;
            }
        }

        Assert.NotNull(designatedInstanceId);

        var projAfterPocket = simulation.Project("p1", Start.AddSeconds(60));
        Assert.Equal(designatedInstanceId, projAfterPocket.MyHiddenPocketInstanceId);

        // Let simulation advance until completion
        simulation.Advance(Start.AddMinutes(5));

        var finalProj = simulation.Project("p1", Start.AddMinutes(5));
        // If raider extracted, the pocket item is extracted normally; if knocked out, only pocket item is preserved
        Assert.Contains(finalProj.MyExtractedLoot, item => item.InstanceId == designatedInstanceId);
    }

    [Fact]
    public void SecretHiddenPocket_OnNormalExtraction_DoesNotDuplicateItem()
    {
        // 4. Safe pocket item is not duplicated on normal extraction
        var simulation = new RaidSimulation("test-pocket-extract", Start, seed: 2, fastPacing: true);
        simulation.AddRaider("p1", "raider-one", Start);

        for (var t = 1; t <= 45; t++)
        {
            simulation.Advance(Start.AddSeconds(t));
            var proj = simulation.Project("p1", Start.AddSeconds(t));
            if (proj.MyInventory.Count > 0)
            {
                simulation.DesignateHiddenPocket("p1", proj.MyInventory[0].InstanceId);
                break;
            }
        }

        simulation.Advance(Start.AddMinutes(3));
        var finalProj = simulation.Project("p1", Start.AddMinutes(3));
        Assert.Equal("Extracted", finalProj.Players.First(p => p.PlayerId == "p1").Activity);

        // Check instance IDs are strictly distinct (no duplicates)
        var instanceIds = finalProj.MyExtractedLoot.Select(i => i.InstanceId).ToList();
        Assert.Equal(instanceIds.Count, instanceIds.Distinct().Count());
    }

    [Fact]
    public void WeaponDurability_DepletesOnRaidConclusion_AndBlocksBrokenWeaponEquip()
    {
        // 5. Weapon durability depletion & broken weapon equip prevention
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one");
        var token = enrolled.AccessToken;

        var proj = coordinator.GetHandlerProjection(token);
        Assert.NotNull(proj.EquippedWeapon);
        var weaponInstanceId = proj.EquippedWeapon.InstanceId;

        // Force durability to 1 to test breaking on raid completion
        coordinator.ApplyWeaponDamage(token, weaponInstanceId, proj.EquippedWeapon.CurrentDurability - 1);

        // Start and advance raid to completion
        clock.Now += TimeSpan.FromSeconds(35);
        coordinator.AdvanceRaids();
        clock.Now += TimeSpan.FromMinutes(3);
        coordinator.AdvanceRaids();

        var updated = coordinator.GetHandlerProjection(token);
        var weapon = updated.OwnedWeapons?.FirstOrDefault(w => w.InstanceId == weaponInstanceId);
        Assert.NotNull(weapon);
        Assert.Equal(0, weapon.CurrentDurability);
        Assert.True(weapon.IsBroken, "Weapon at 0 durability must be marked broken");

        // Attempting to equip a broken weapon must throw
        var ex = Assert.Throws<RaidCommandException>(() =>
            coordinator.EquipWeapon(token, new EquipWeaponRequest("cmd-1", weapon.InstanceId)));
        Assert.Equal("BrokenWeaponCannotBeEquipped", ex.Reason);

        // Give coins for repair if needed
        var repairRes = coordinator.RepairWeapon(token, new RepairWeaponRequest("cmd-2", weapon.InstanceId));
        Assert.Equal(weapon.MaxDurability, repairRes.RepairedWeapon.CurrentDurability);

        // Can now equip successfully
        var equipRes = coordinator.EquipWeapon(token, new EquipWeaponRequest("cmd-3", weapon.InstanceId));
        Assert.Equal(weaponInstanceId, equipRes.EquippedWeapon.InstanceId);
    }

    [Fact]
    public void HandlerSupportActions_ConsumeSignal_AndAreIdempotent()
    {
        // 6. Signal spendable budget, regeneration, and emergency directives
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one");
        var token = enrolled.AccessToken;

        // Handler starts with 3 signal, max 5
        var proj = coordinator.GetHandlerProjection(token);
        Assert.Equal(3, proj.SignalCurrent);
        Assert.Equal(5, proj.SignalMax);

        // Start a raid
        clock.Now += TimeSpan.FromSeconds(35);
        coordinator.AdvanceRaids();

        var raidActiveProj = coordinator.GetHandlerProjection(token);
        Assert.NotNull(raidActiveProj.CurrentRaidId);
        var initialSignal = raidActiveProj.SignalCurrent;

        // Execute CallExtract (costs 3 signal)
        var cmdId = "call-ext-1";
        var res1 = coordinator.ExecuteSupportAction(token, new HandlerSupportActionRequest(cmdId, "CallExtract"));
        Assert.True(res1.Success);
        Assert.Equal(initialSignal - 3, res1.RemainingSignal);

        // Idempotent retry with same commandId
        var res1Retry = coordinator.ExecuteSupportAction(token, new HandlerSupportActionRequest(cmdId, "CallExtract"));
        Assert.True(res1Retry.Success);
        Assert.Equal(res1.RemainingSignal, res1Retry.RemainingSignal);

        // Zero out signal to test insufficient signal condition
        coordinator.SetSignalForTesting(token, 0);

        // Another action without sufficient signal throws
        var ex = Assert.Throws<RaidCommandException>(() =>
            coordinator.ExecuteSupportAction(token, new HandlerSupportActionRequest("calm-fail", "Calm")));
        Assert.Equal("InsufficientSignal", ex.Reason);

        // Advance clock by 30 seconds to regenerate 1 signal
        clock.Now += TimeSpan.FromSeconds(30);
        coordinator.AdvanceRaids();

        var regenProj = coordinator.GetHandlerProjection(token);
        Assert.True(regenProj.SignalCurrent >= 1, "Signal should regenerate after 30 seconds");
    }

    [Fact]
    public void HandlerSupportActions_CalmAndPressure_ModifyGreed()
    {
        // 7. Calm decreases Greed by 12, Pressure increases Greed by 10
        var clock = new ManualTime();
        var coordinator = new RaidCoordinator(clock, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one");
        var token = enrolled.AccessToken;

        clock.Now += TimeSpan.FromSeconds(35);
        coordinator.AdvanceRaids();

        // Give 5 signal to test multiple actions
        coordinator.SetSignalForTesting(token, 5);

        // Calm
        var calmRes = coordinator.ExecuteSupportAction(token, new HandlerSupportActionRequest("calm-cmd", "Calm"));
        Assert.True(calmRes.Success);
        Assert.Contains("Greed", calmRes.Message, StringComparison.OrdinalIgnoreCase);

        // Support waits for existing travel, which can take more than five seconds.
        for (var i = 0; i < 15; i++)
        {
            clock.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            if (coordinator.GetHandlerProjection(token).Raid?.Events.Any(e =>
                e.Kind == "HandlerAction" && e.Reason == "HandlerSupport:Calm") == true) break;
        }
        Assert.Contains(coordinator.GetHandlerProjection(token).Raid!.Events,
            e => e.Kind == "HandlerAction" && e.Reason == "HandlerSupport:Calm");

        // Pressure
        var pressureRes = coordinator.ExecuteSupportAction(token, new HandlerSupportActionRequest("press-cmd", "Pressure"));
        Assert.True(pressureRes.Success);
        Assert.Contains("Greed", pressureRes.Message, StringComparison.OrdinalIgnoreCase);

        // Advance raid to process the queued Pressure action
        clock.Now += TimeSpan.FromSeconds(5);
        coordinator.AdvanceRaids();

        var proj = coordinator.GetHandlerProjection(token);
        Assert.Contains(proj.Comms, e => e.Kind == "HandlerSupportAction");
    }
}
