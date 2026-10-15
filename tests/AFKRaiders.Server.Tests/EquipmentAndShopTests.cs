using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class EquipmentAndShopTests
{
    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = DateTimeOffset.Parse("2026-10-12T10:00:00Z");
        public override DateTimeOffset GetUtcNow() => Now;
        public void Advance(TimeSpan duration) => Now += duration;
    }

    [Fact]
    public void EnrolledHandler_ReceivesStarterWeapon_EquippedWithMaxDurability()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);

        var enrolled = coordinator.EnrollHandler("raider-one", "req-start-wpn");
        var projection = enrolled.Projection;

        Assert.NotNull(projection.EquippedWeapon);
        Assert.Equal("tea_kettle", projection.EquippedWeapon.CatalogId);
        Assert.Equal("Tea Kettle", projection.EquippedWeapon.Name);
        Assert.Equal(8, projection.EquippedWeapon.CurrentDurability);
        Assert.Equal(8, projection.EquippedWeapon.MaxDurability);
        Assert.NotEmpty(projection.OwnedWeapons ?? []);
        Assert.Single(projection.OwnedWeapons!);
    }

    [Fact]
    public void PurchaseShopItem_ValidatesCoinsAndAppliesIdempotency()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one", "req-shop-1");

        // Attempting to buy weapon without coins fails
        var failEx = Assert.Throws<RaidCommandException>(() =>
            coordinator.PurchaseShopItem(enrolled.AccessToken, new PurchaseShopItemRequest("cmd-buy-fail", "aspperigo", 1)));
        Assert.Equal("InsufficientCoins", failEx.Reason);

        // Grant coins by running an autonomous raid and selling secured loot
        DateTimeOffset? homeAvailableAt = null;
        for (var index = 0; index < 240 && !homeAvailableAt.HasValue; index++)
        {
            time.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
            homeAvailableAt = coordinator.GetHandlerProjection(enrolled.AccessToken).HomeAvailableAt;
        }

        var returnStatus = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.NotNull(homeAvailableAt);
        Assert.True(returnStatus.StashCount > 0);

        // Sell all items in stash to earn coins
        var sellRes = coordinator.SellStash(enrolled.AccessToken, new SellStashItemsRequest(
            CommandId: "cmd-sell-all",
            InstanceIds: returnStatus.Stash!.Select(i => i.InstanceId).ToArray()
        ));
        Assert.True(sellRes.CoinsGained > 0);
        Assert.True(sellRes.NewCoinsBalance >= 65);

        // Buy Aspperigo weapon
        var buyRes = coordinator.PurchaseShopItem(enrolled.AccessToken, new PurchaseShopItemRequest("cmd-buy-aspp", "aspperigo", 1));
        Assert.Equal("aspperigo", buyRes.CatalogId);
        Assert.Equal(65, buyRes.TotalCost);
        Assert.NotNull(buyRes.NewWeapons);
        Assert.Single(buyRes.NewWeapons);

        // Idempotency: replaying same command returns cached response without charging twice
        var coinsBeforeReplay = coordinator.GetHandlerProjection(enrolled.AccessToken).Coins;
        var replayRes = coordinator.PurchaseShopItem(enrolled.AccessToken, new PurchaseShopItemRequest("cmd-buy-aspp", "aspperigo", 1));
        Assert.Equal(buyRes.NewWeapons[0].InstanceId, replayRes.NewWeapons![0].InstanceId);
        Assert.Equal(coinsBeforeReplay, coordinator.GetHandlerProjection(enrolled.AccessToken).Coins);

        // Equip the newly purchased Aspperigo
        var newWeaponId = buyRes.NewWeapons[0].InstanceId;
        var equipRes = coordinator.EquipWeapon(enrolled.AccessToken, new EquipWeaponRequest("cmd-equip-1", newWeaponId));
        Assert.Equal(newWeaponId, equipRes.WeaponInstanceId);
        Assert.Equal("aspperigo", equipRes.EquippedWeapon.CatalogId);

        // Replay equip command is idempotent
        var replayEquip = coordinator.EquipWeapon(enrolled.AccessToken, new EquipWeaponRequest("cmd-equip-1", newWeaponId));
        Assert.Equal(newWeaponId, replayEquip.WeaponInstanceId);

        // Buy supplies (White Bandage)
        var buyMeds = coordinator.PurchaseShopItem(enrolled.AccessToken, new PurchaseShopItemRequest("cmd-buy-meds", "bandage_white", 1));
        Assert.Equal(8, buyMeds.TotalCost);
        Assert.NotNull(buyMeds.UpdatedSupplies);
        Assert.Equal(1, buyMeds.UpdatedSupplies[0].Quantity);

        // Stage 1 bandage for field loadout
        var stageRes = coordinator.StageLoadout(enrolled.AccessToken, new StageLoadoutRequest("cmd-stage-meds", [
            new StagedSupplyRequestItem("bandage_white", 1)
        ]));
        Assert.Single(stageRes.StagedSupplies);
        Assert.Equal(1, stageRes.StagedSupplies[0].Quantity);

        // Replay stage command is idempotent
        var replayStage = coordinator.StageLoadout(enrolled.AccessToken, new StageLoadoutRequest("cmd-stage-meds", [
            new StagedSupplyRequestItem("bandage_white", 1)
        ]));
        Assert.Single(replayStage.StagedSupplies);

        // Projection reflects equipped weapon and staged loadout
        var updatedProj = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.Equal("aspperigo", updatedProj.EquippedWeapon?.CatalogId);
        Assert.Single(updatedProj.StagedSupplies ?? []);
        Assert.Equal("bandage_white", updatedProj.StagedSupplies![0].CatalogId);
    }

    [Fact]
    public void ShopCatalog_ProvidesCanonicalWeaponsAndSupplies()
    {
        Assert.True(ShopCatalog.TryGetWeapon("tea_kettle", out var kettle));
        Assert.NotNull(kettle);
        Assert.Equal(0, kettle.Value);
        Assert.Equal(8, kettle.DurabilityMax);

        Assert.True(ShopCatalog.TryGetWeapon("aspperigo", out var aspperigo));
        Assert.NotNull(aspperigo);
        Assert.Equal(65, aspperigo.Value);
        Assert.Equal(18, aspperigo.RepairCost);

        Assert.True(ShopCatalog.TryGetWeapon("vernerider", out var vernerider));
        Assert.NotNull(vernerider);
        Assert.Equal(140, vernerider.Value);

        Assert.True(ShopCatalog.TryGetWeapon("temptrest_ar", out var temptrest));
        Assert.NotNull(temptrest);
        Assert.Equal(280, temptrest.Value);
        Assert.Equal(55, temptrest.RepairCost);

        Assert.True(ShopCatalog.TryGetSupply("bandage_white", out var whiteBandage));
        Assert.NotNull(whiteBandage);
        Assert.Equal(8, whiteBandage.Value);
        Assert.Equal(5, whiteBandage.HealAmount);

        Assert.True(ShopCatalog.TryGetSupply("panic_paddles", out var paddles));
        Assert.NotNull(paddles);
        Assert.Equal(90, paddles.Value);
        Assert.Equal(25, paddles.ReviveAmount);

        Assert.True(ShopCatalog.TryGetSupply("fizz_cell", out var fizzCell));
        Assert.NotNull(fizzCell);
        Assert.Equal(12, fizzCell.Value);
        Assert.Equal(20, fizzCell.ChargeAmount);
    }

    [Fact]
    public void Equipment_EquipAndRepair_EnforcesOwnershipAndDurability()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one", "req-equip-1");

        // Equipping unowned weapon throws WeaponNotOwned
        var ex = Assert.Throws<RaidCommandException>(() =>
            coordinator.EquipWeapon(enrolled.AccessToken, new EquipWeaponRequest("cmd-eq-bad", "wpn_fake_123")));
        Assert.Equal("WeaponNotOwned", ex.Reason);

        // Repairing weapon already at max durability throws WeaponAlreadyAtMaxDurability
        var starterWeapon = enrolled.Projection.EquippedWeapon!;
        var repEx = Assert.Throws<RaidCommandException>(() =>
            coordinator.RepairWeapon(enrolled.AccessToken, new RepairWeaponRequest("cmd-rep-max", starterWeapon.InstanceId)));
        Assert.Equal("WeaponAlreadyAtMaxDurability", repEx.Reason);
    }

    [Fact]
    public void Equipment_RepairDamagedWeapon_RestoresDurabilityAndDeductsCoins()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one", "req-rep-1");

        // Advance multiple raids to collect plenty of stash
        for (var index = 0; index < 360; index++)
        {
            time.Now += TimeSpan.FromSeconds(1);
            coordinator.AdvanceRaids();
        }

        var proj = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.True(proj.Stash!.Count > 4);

        coordinator.SellStash(enrolled.AccessToken, new SellStashItemsRequest(
            CommandId: "cmd-sell-rep",
            InstanceIds: proj.Stash!.Select(i => i.InstanceId).ToArray()
        ));

        // Buy Aspperigo (cost 65, repairCost 18, maxDurability 10)
        var buy = coordinator.PurchaseShopItem(enrolled.AccessToken, new PurchaseShopItemRequest("cmd-buy-asp-rep", "aspperigo", 1));
        var weaponInstanceId = buy.NewWeapons![0].InstanceId;
        var coinsBeforeRepair = buy.RemainingCoins;

        // Simulate durability wear down to 3
        coordinator.DamageWeaponForTesting(enrolled.AccessToken, weaponInstanceId, 7);

        var damagedProj = coordinator.GetHandlerProjection(enrolled.AccessToken);
        var damagedWeapon = damagedProj.OwnedWeapons!.First(w => w.InstanceId == weaponInstanceId);
        Assert.Equal(3, damagedWeapon.CurrentDurability);

        // Repair weapon
        var repair = coordinator.RepairWeapon(enrolled.AccessToken, new RepairWeaponRequest("cmd-repair-asp", weaponInstanceId));
        Assert.Equal(18, repair.RepairCostPaid);
        Assert.Equal(coinsBeforeRepair - 18, repair.RemainingCoins);
        Assert.Equal(10, repair.RepairedWeapon.CurrentDurability);

        // Verify projection state
        var afterProj = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.Equal(repair.RemainingCoins, afterProj.Coins);
        var repairedInProj = afterProj.OwnedWeapons!.First(w => w.InstanceId == weaponInstanceId);
        Assert.Equal(10, repairedInProj.CurrentDurability);

        // Idempotent duplicate check
        var dupRepair = coordinator.RepairWeapon(enrolled.AccessToken, new RepairWeaponRequest("cmd-repair-asp", weaponInstanceId));
        Assert.Equal("cmd-repair-asp", dupRepair.CommandId);
        Assert.Equal(repair.RemainingCoins, dupRepair.RemainingCoins);
    }

    [Fact]
    public void StageLoadout_EnforcesStockAndCategoryLimits()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one", "req-stage-1");

        // Staging items not in stock throws InsufficientStock
        var ex = Assert.Throws<RaidCommandException>(() =>
            coordinator.StageLoadout(enrolled.AccessToken, new StageLoadoutRequest("cmd-st-1", [
                new StagedSupplyRequestItem("bandage_white", 1)
            ])));
        Assert.StartsWith("InsufficientStock", ex.Reason);

        // Staging invalid item throws InvalidSupplyItem
        var invEx = Assert.Throws<RaidCommandException>(() =>
            coordinator.StageLoadout(enrolled.AccessToken, new StageLoadoutRequest("cmd-st-2", [
                new StagedSupplyRequestItem("quantum_nuke", 1)
            ])));
        Assert.StartsWith("InvalidSupplyItem", invEx.Reason);
    }

    [Fact]
    public void ItemAppraisal_GeneratesDeadpanClassificationAndAppraisal()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true);
        var enrolled = coordinator.EnrollHandler("raider-one", "req-appraise-1");

        var appraisal = coordinator.AppraiseItem(enrolled.AccessToken, "tea_kettle");
        Assert.NotNull(appraisal);
        Assert.Equal("tea_kettle", appraisal.CatalogId);
        Assert.Contains("Steam-Pressurized", appraisal.StrategicClassification);
        Assert.NotEmpty(appraisal.Appraisal);

        var scrapAppraisal = coordinator.AppraiseItem(enrolled.AccessToken, "scrap_copper_pipe", "Crooked Pipe");
        Assert.NotNull(scrapAppraisal);
        Assert.NotEmpty(scrapAppraisal.StrategicClassification);
        Assert.NotEmpty(scrapAppraisal.Appraisal);
    }
}
