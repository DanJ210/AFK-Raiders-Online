using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class RaiderEconomyTests
{
    private static readonly DateTimeOffset Arrival = DateTimeOffset.Parse("2026-10-18T10:00:00Z");

    private sealed class ManualTime : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = Arrival;
        public override DateTimeOffset GetUtcNow() => Now;
        public void Advance(TimeSpan duration) => Now += duration;
    }

    [Fact]
    public void SameSeedStateAndTimeProduceIdenticalDecisions()
    {
        var first = Decide(seed: 3, coins: 300, stash: SampleStash());
        var second = Decide(seed: 3, coins: 300, stash: SampleStash());

        Assert.Equal(first.PrimaryTrait, second.PrimaryTrait);
        Assert.Equal(first.ArrivedAt, second.ArrivedAt);
        Assert.Equal(first.ItemInstanceIdsToSell, second.ItemInstanceIdsToSell);
        Assert.Equal(first.WeaponInstanceIdToRepair, second.WeaponInstanceIdToRepair);
        Assert.Equal(first.WeaponCatalogIdToBuy, second.WeaponCatalogIdToBuy);
        Assert.Equal(first.WeaponInstanceIdToBuy, second.WeaponInstanceIdToBuy);
        Assert.Equal(first.WeaponInstanceIdToEquip, second.WeaponInstanceIdToEquip);
        Assert.Equal(first.CreateTeaKettleFallback, second.CreateTeaKettleFallback);
        Assert.Equal(first.SupplyPurchases, second.SupplyPurchases);
        Assert.Equal(first.StagedSupplies, second.StagedSupplies);
        Assert.Equal(first.ReserveFloor, second.ReserveFloor);
    }

    [Fact]
    public void SeedSelectsOneOfTheFourPrimaryTraitsDeterministically()
    {
        Assert.Equal(RaiderPrimaryTrait.Cautious, RaiderEconomyPolicy.TraitFromSeed(0));
        Assert.Equal(RaiderPrimaryTrait.Hoarder, RaiderEconomyPolicy.TraitFromSeed(1));
        Assert.Equal(RaiderPrimaryTrait.Brawler, RaiderEconomyPolicy.TraitFromSeed(2));
        Assert.Equal(RaiderPrimaryTrait.Gadgeteer, RaiderEconomyPolicy.TraitFromSeed(3));
        Assert.Equal(RaiderPrimaryTrait.Gadgeteer, RaiderEconomyPolicy.TraitFromSeed(-1));
    }

    [Fact]
    public void TraitReserveThresholdsProduceDifferentWeaponChoicesFromTheSameCatalog()
    {
        var weapons = new[]
        {
            ShopCatalog.GetStarterWeapon(),
            TestWeapon("blade_50", 50, 10),
            TestWeapon("blade_60", 60, 20),
            TestWeapon("blade_70", 70, 30),
            TestWeapon("blade_80", 80, 40)
        };
        var choices = Enumerable.Range(0, 4)
            .Select(seed => Decide(seed, coins: 100, weaponCatalog: weapons).WeaponCatalogIdToBuy)
            .ToArray();

        Assert.Equal(new string?[] { "blade_60", "blade_50", "blade_80", "blade_70" }, choices);
    }

    [Fact]
    public void HoarderKeepsExactlyItsThreeMostValuableItemsWithStableTies()
    {
        var stash = new[]
        {
            new ItemView("scrap-z", "scrap_metal_basic", "Scrap", 100, ""),
            new ItemView("scrap-a", "scrap_metal_basic", "Scrap", 100, ""),
            new ItemView("valuable", "robot_finger", "Finger", 100, ""),
            new ItemView("cheap-z", "scrap_metal_basic", "Scrap", 20, ""),
            new ItemView("cheap-a", "robot_finger", "Finger", 20, "")
        };

        var plan = Decide(seed: 1, stash: stash);
        var kept = stash.Select(item => item.InstanceId)
            .Except(plan.ItemInstanceIdsToSell)
            .OrderBy(id => id, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["scrap-a", "scrap-z", "valuable"], kept);
        Assert.Equal(2, plan.ItemInstanceIdsToSell.Count);
    }

    [Fact]
    public void BrokenWeaponsUseRepairOrFreeFallbackAndAlwaysSelectOneUsableWeapon()
    {
        var aspperigo = ShopCatalog.CreateWeaponInstance(ShopCatalog.Weapons[1], "broken-asp")
            with { CurrentDurability = 0 };
        var fallbackPlan = Decide(seed: 2, coins: 0, weapons: [aspperigo], equippedWeaponId: aspperigo.InstanceId);

        Assert.True(fallbackPlan.CreateTeaKettleFallback);
        Assert.Equal(RaiderEconomyPolicy.CreateWeaponInstanceId("owner", 1, "tea_kettle", "fallback"),
            fallbackPlan.WeaponInstanceIdToEquip);

        var brokenKettle = ShopCatalog.CreateWeaponInstance(ShopCatalog.GetStarterWeapon(), "broken-kettle")
            with { CurrentDurability = 0 };
        var repairPlan = Decide(seed: 0, coins: 0, weapons: [brokenKettle], equippedWeaponId: brokenKettle.InstanceId);

        Assert.False(repairPlan.CreateTeaKettleFallback);
        Assert.Equal(brokenKettle.InstanceId, repairPlan.WeaponInstanceIdToRepair);
        Assert.Null(repairPlan.WeaponInstanceIdToEquip);
    }

    [Fact]
    public void PreparationRespectsReserveAndSupplyLimitsWithoutNegativeBalances()
    {
        foreach (var seed in Enumerable.Range(0, 4))
        {
            var owned = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["bandage_white"] = 10,
                ["fizz_cell"] = 10
            };
            var plan = Decide(seed, coins: 90, supplyStock: owned);
            var purchases = plan.SupplyPurchases.Sum(purchase =>
                ShopCatalog.Supplies.Single(item => item.Id == purchase.CatalogId).Value * purchase.Quantity);
            var weaponCost = plan.WeaponCatalogIdToBuy is { } weaponId
                ? ShopCatalog.Weapons.Single(item => item.Id == weaponId).Value
                : 0;
            var finalCoins = 90 - weaponCost - purchases;
            var medical = plan.StagedSupplies.Where(item => item.Category == "Medical").Sum(item => item.Quantity);
            var rechargers = plan.StagedSupplies.Where(item => item.Category == "ShieldRecharger").Sum(item => item.Quantity);

            Assert.True(finalCoins >= plan.ReserveFloor, $"Trait {plan.PrimaryTrait} violated its reserve.");
            Assert.InRange(medical, 0, ShopCatalog.MaxStagedMedicalCount);
            Assert.InRange(rechargers, 0, ShopCatalog.MaxStagedRechargerCount);
            Assert.All(plan.SupplyPurchases, purchase => Assert.True(purchase.Quantity > 0));
        }
    }

    [Fact]
    public void AutonomousArrivalPreparationAndAdmissionRetriesDoNotRepeatTransactions()
    {
        var time = new ManualTime();
        var coordinator = new RaidCoordinator(time, fastPacing: true, nextSeed: () => 0);
        var enrolled = coordinator.EnrollHandler("raider-one", "econ-autonomous");
        Assert.Contains(coordinator.GetHandlerProjection(enrolled.AccessToken).LatestPreparationDecisions!,
            decision => decision.Kind == "PreparationReady");

        time.Advance(TimeSpan.FromSeconds(120));
        Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
        var prepared = coordinator.GetHandlerProjection(enrolled.AccessToken);
        Assert.Contains(prepared.LatestPreparationDecisions!, decision => decision.Kind == "PreparationSale");
        Assert.All(prepared.LatestPreparationDecisions!, decision =>
            Assert.Contains(prepared.Comms, entry => entry.Kind == decision.Kind && entry.Text == decision.Text));
        var decisionCount = prepared.LatestPreparationDecisions!.Count;
        var coins = prepared.Coins;
        var priorAdmissions = prepared.Comms.Count(item => item.Kind == "RaidAdmission");
        var stockBeforeAdmission = prepared.SupplyStock!.ToDictionary(item => item.CatalogId, item => item.Quantity);
        var stagedBeforeAdmission = prepared.StagedSupplies!.ToDictionary(item => item.CatalogId, item => item.Quantity);

        Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
        var afterRetry = coordinator.GetHandlerProjection(enrolled.AccessToken);

        Assert.Equal(decisionCount, afterRetry.LatestPreparationDecisions!.Count);
        Assert.Equal(coins, afterRetry.Coins);
        Assert.All(afterRetry.SupplyStock!, item => Assert.True(item.Quantity >= 0));

        time.Now = (prepared.HomeAvailableAt ?? throw new InvalidOperationException("The Raider did not return home."))
            + TimeSpan.FromSeconds(30);
        Parallel.For(0, 8, _ => coordinator.AdvanceRaids());
        var admitted = coordinator.GetHandlerProjection(enrolled.AccessToken);

        Assert.NotNull(admitted.CurrentRaidId);
        Assert.Equal(priorAdmissions + 1, admitted.Comms.Count(item => item.Kind == "RaidAdmission"));
        Assert.Equal(coins, admitted.Coins);
        Assert.All(stagedBeforeAdmission, entry =>
        {
            var remaining = admitted.SupplyStock!.FirstOrDefault(item => item.CatalogId == entry.Key)?.Quantity ?? 0;
            Assert.Equal(stockBeforeAdmission.GetValueOrDefault(entry.Key) - entry.Value, remaining);
        });
        Assert.All(admitted.SupplyStock!, item => Assert.True(item.Quantity >= 0));
        Assert.Equal(admitted.OwnedWeapons!.Count,
            admitted.OwnedWeapons.Select(weapon => weapon.InstanceId).Distinct().Count());
    }

    [Fact]
    public void HandlerCommsContainCommittedPreparationEventsOnly()
    {
        var coordinator = new RaidCoordinator(new ManualTime(), fastPacing: true, nextSeed: () => 0);
        var enrolled = coordinator.EnrollHandler("raider-one", "econ-comms");
        var projection = coordinator.GetHandlerProjection(enrolled.AccessToken);

        Assert.All(projection.LatestPreparationDecisions!, decision =>
            Assert.Contains(projection.Comms, entry =>
                entry.Kind == decision.Kind && entry.Text == decision.Text));
        Assert.DoesNotContain(projection.LatestPreparationDecisions!, decision =>
            decision.Kind.Contains("Purchase", StringComparison.Ordinal));
    }

    [Fact]
    public void ShopCatalogRemainsAvailableAsTheAutonomousPreparationCatalog()
    {
        Assert.True(ShopCatalog.TryGetWeapon("tea_kettle", out var kettle));
        Assert.NotNull(kettle);
        Assert.Equal(0, kettle.Value);
        Assert.True(ShopCatalog.TryGetSupply("bandage_white", out var bandage));
        Assert.NotNull(bandage);
        Assert.Equal(8, bandage.Value);
    }

    [Fact]
    public void FiniteTraderStockFiltersWeaponAndSupplyChoicesAndCapsQuantities()
    {
        var weaponPlan = Decide(
            seed: 0,
            coins: 500,
            traderStock: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["vernerider"] = 1,
                ["temptrest_ar"] = 1
            });
        Assert.Equal("vernerider", weaponPlan.WeaponCatalogIdToBuy);

        var supplyPlan = Decide(
            seed: 0,
            coins: 500,
            traderStock: new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["bandage_white"] = 1
            });
        Assert.Equal(1, Assert.Single(supplyPlan.SupplyPurchases).Quantity);
        Assert.Equal("bandage_white", supplyPlan.SupplyPurchases[0].CatalogId);
    }

    private static RaiderPreparationPlan Decide(
        int seed,
        int coins = 0,
        IReadOnlyList<ItemView>? stash = null,
        IReadOnlyList<OwnedWeapon>? weapons = null,
        string? equippedWeaponId = "starter",
        IReadOnlyDictionary<string, int>? supplyStock = null,
        IReadOnlyList<WeaponDefinition>? weaponCatalog = null,
        IReadOnlyList<SupplyDefinition>? supplyCatalog = null,
        IReadOnlyDictionary<string, int>? traderStock = null)
    {
        var ownedWeapons = weapons ?? [ShopCatalog.CreateWeaponInstance(ShopCatalog.GetStarterWeapon(), "starter")];
        var equipped = ownedWeapons.Any(weapon => weapon.InstanceId == equippedWeaponId)
            ? equippedWeaponId
            : ownedWeapons.FirstOrDefault()?.InstanceId;
        return RaiderEconomyPolicy.Decide(
            "owner",
            1,
            seed,
            Arrival,
            coins,
            stash ?? [],
            ownedWeapons,
            equipped,
            supplyStock ?? new Dictionary<string, int>(),
            [],
            LootCatalog.Default,
            weaponCatalog ?? ShopCatalog.Weapons,
            supplyCatalog ?? ShopCatalog.Supplies,
            traderStock);
    }

    private static IReadOnlyList<ItemView> SampleStash() =>
    [
        new("valuable-1", "robot_finger", "Robot Finger", 35, ""),
        new("valuable-2", "scrap_metal_basic", "Scrap", 25, ""),
        new("valuable-3", "robot_finger", "Robot Finger", 20, ""),
        new("cheap", "scrap_metal_basic", "Scrap", 5, "")
    ];

    private static WeaponDefinition TestWeapon(string id, int value, int damage) =>
        new(id, id, damage, 1, damage, damage, value, 1, 10, 0, "");
}
