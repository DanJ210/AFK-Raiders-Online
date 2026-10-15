using AFKRaiders.Server.Domain;
using Xunit;

namespace AFKRaiders.Server.Tests;

public sealed class LootCatalogTests
{
    private static LootCatalogItem Item(string id, string name = "Test Item", int value = 10, int rarity = 1,
        IReadOnlyList<string>? categories = null) =>
        new(id, name, value, rarity, "test", "Flavor", categories ?? [LootEligibilityCategory.Salvage]);

    private static OpportunityDefinition Opportunity(string id, string location, IReadOnlyList<string> fixedItems) =>
        new(id, location, 10, fixedItems, 1, [LootEligibilityCategory.Salvage]);

    [Fact]
    public void DefaultCatalogIsValidAndContainsReviewedScrapItems()
    {
        var catalog = LootCatalog.Default;

        Assert.Equal("desperanza-shared-loot-v4", catalog.ContentVersion);
        Assert.NotEmpty(catalog.Items);
        Assert.NotEmpty(catalog.Opportunities);

        var basicScrap = catalog.GetItem("scrap_metal_basic");
        Assert.Equal("Basic Scrap Metal", basicScrap.Name);
        Assert.Equal(8, basicScrap.Value);
        Assert.Equal(1, basicScrap.Rarity);
        Assert.Equal("scrap_components", basicScrap.Category);
        Assert.Equal([LootEligibilityCategory.Salvage, LootEligibilityCategory.Residential], basicScrap.EligibilityCategories);
        Assert.Equal("Sharp, rusty, and eager to give you tetanus.", basicScrap.Flavor);

        var robotToe = catalog.GetItem("robot_toe");
        Assert.Equal("Robot Toe", robotToe.Name);
        Assert.Equal(15, robotToe.Value);
        Assert.Equal(2, robotToe.Rarity);

        var circuits = catalog.GetItem("circuit_bundle_messy");
        Assert.Equal("Messy Circuit Bundle", circuits.Name);
        Assert.Equal(35, circuits.Value);
        Assert.Equal(3, circuits.Rarity);

        var robotFinger = catalog.GetItem("robot_finger");
        Assert.Equal("Robot Finger", robotFinger.Name);
        Assert.Equal(18, robotFinger.Value);
        Assert.Equal(2, robotFinger.Rarity);
    }

    [Fact]
    public void FixedWorldItemsPreserveExactS5FixtureAndInstanceIds()
    {
        var catalog = LootCatalog.Default;
        var raidId = "raid-test-123";

        var items = catalog.CreateFixedWorldItems(raidId);

        Assert.Equal(6, items.Count);

        var marketItems = items.Where(i => i.OpportunityId == "market-locker").Select(i => i.Item).ToArray();
        Assert.Equal(2, marketItems.Length);
        Assert.Equal($"{raidId}:scrap", marketItems[0].InstanceId);
        Assert.Equal("scrap_metal_basic", marketItems[0].CatalogId);
        Assert.Equal(8, marketItems[0].Value);
        Assert.Equal($"{raidId}:toe", marketItems[1].InstanceId);
        Assert.Equal("robot_toe", marketItems[1].CatalogId);
        Assert.Equal(15, marketItems[1].Value);

        var yardItems = items.Where(i => i.OpportunityId == "humming-crate").Select(i => i.Item).ToArray();
        Assert.Equal(2, yardItems.Length);
        Assert.Equal($"{raidId}:circuits", yardItems[0].InstanceId);
        Assert.Equal("circuit_bundle_messy", yardItems[0].CatalogId);
        Assert.Equal(35, yardItems[0].Value);
        Assert.Equal($"{raidId}:finger", yardItems[1].InstanceId);
        Assert.Equal("robot_finger", yardItems[1].CatalogId);
        Assert.Equal(18, yardItems[1].Value);

        var depotItems = items.Where(i => i.OpportunityId == "depot-locker").Select(i => i.Item).ToArray();
        Assert.Equal(2, depotItems.Length);
        Assert.Equal($"{raidId}:depot-scrap", depotItems[0].InstanceId);
        Assert.Equal("scrap_metal_basic", depotItems[0].CatalogId);
        Assert.Equal(8, depotItems[0].Value);
        Assert.Equal($"{raidId}:depot-toe", depotItems[1].InstanceId);
        Assert.Equal("robot_toe", depotItems[1].CatalogId);
        Assert.Equal(15, depotItems[1].Value);

        // Instance IDs must be strictly unique across the world
        Assert.Equal(6, items.Select(i => i.Item.InstanceId).Distinct().Count());
    }

    [Fact]
    public void ValidationRejectsDuplicateCatalogIds()
    {
        var items = new List<LootCatalogItem>
        {
            Item("dup_id", "Item A"),
            Item("dup_id", "Item B", 20, 2)
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-1", "Market", ["dup_id"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps, [new("dup_id", 1)]));
        Assert.Contains("Duplicate item CatalogId", ex.Message);
    }

    [Fact]
    public void ValidationRejectsNegativeItemValue()
    {
        var items = new List<LootCatalogItem>
        {
            Item("item_neg", "Negative Item", -5)
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-1", "Market", ["item_neg"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps, [new("item_neg", 1)]));
        Assert.Contains("cannot have negative value", ex.Message);
    }

    [Fact]
    public void ValidationRejectsUnknownItemInFixedOpportunity()
    {
        var items = new List<LootCatalogItem>
        {
            Item("item_valid")
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-1", "Market", ["item_valid", "item_nonexistent"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps, [new("item_valid", 1)]));
        Assert.Contains("references unknown fixed item", ex.Message);
    }

    [Fact]
    public void ValidationRejectsUnknownItemInWeightedPool()
    {
        var items = new List<LootCatalogItem>
        {
            Item("item_valid")
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-1", "Market", ["item_valid"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps,
            [new("item_valid", 10), new("item_missing", 5)]));
        Assert.Contains("pool references unknown item", ex.Message);
    }

    [Fact]
    public void ValidationRejectsInvalidPoolWeights()
    {
        var items = new List<LootCatalogItem>
        {
            Item("item_valid")
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-1", "Market", ["item_valid"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps, [new("item_valid", 0)]));
        Assert.Contains("has non-positive dropWeight", ex.Message);
    }

    [Fact]
    public void ValidationRejectsDuplicateOpportunityIds()
    {
        var items = new List<LootCatalogItem>
        {
            Item("item_valid")
        };
        var opps = new List<OpportunityDefinition>
        {
            Opportunity("opp-same", "Market", ["item_valid"]),
            Opportunity("opp-same", "Service Depot", ["item_valid"])
        };

        var ex = Assert.Throws<LootCatalogValidationException>(() => new LootCatalog("v1", items, opps, [new("item_valid", 1)]));
        Assert.Contains("Duplicate opportunity Id", ex.Message);
    }

    [Fact]
    public void CategoryEligibilityFiltersConfiguredPoolOnceAndPreservesSeededOrdering()
    {
        var residentialSalvage = Item("residential-salvage", categories:
            [LootEligibilityCategory.Residential, LootEligibilityCategory.Salvage]);
        var salvage = Item("salvage", categories: [LootEligibilityCategory.Salvage]);
        var robot = Item("robot", categories: [LootEligibilityCategory.Robot]);
        var table = new OpportunityDefinition(
            "configured-table",
            "A Location Not Used For Eligibility",
            10,
            ["residential-salvage"],
            1,
            [LootEligibilityCategory.Residential, LootEligibilityCategory.Salvage]);
        var catalog = new LootCatalog("category-test-v1",
            [residentialSalvage, salvage, robot], [table],
            [new("residential-salvage", 1), new("salvage", 1), new("robot", 1)]);

        var repeated = Enumerable.Range(0, 512)
            .SelectMany(seed => catalog.CreateTableBackedWorldItems("category-raid", seed))
            .ToArray();
        var selectedOverlappingItemCount = repeated.Count(item => item.Item.CatalogId == "residential-salvage");

        Assert.Equal(512, repeated.Length);
        Assert.All(repeated, item => Assert.Contains(item.Item.CatalogId,
            new[] { "residential-salvage", "salvage" }));
        Assert.InRange(selectedOverlappingItemCount, 230, 282);

        var ordered = catalog.CreateTableBackedWorldItems("category-raid", 42);
        Assert.Equal(ordered, catalog.CreateTableBackedWorldItems("category-raid", 42));
        Assert.All(ordered, item => Assert.Equal("configured-table", item.OpportunityId));
        Assert.Equal(ordered.Select(item => item.Item.InstanceId).Distinct().Count(), ordered.Count);
    }

    [Fact]
    public void SharedPoolOrderAndOverlappingCategoriesDetermineExactDrawsInBothGenerators()
    {
        var first = Item("first", categories: [LootEligibilityCategory.Residential, LootEligibilityCategory.Salvage]);
        var second = Item("second");
        var excluded = Item("excluded", categories: [LootEligibilityCategory.Robot]);
        var opportunity = new OpportunityDefinition("shared", "Unrelated Placement", 10, [], 3,
            [LootEligibilityCategory.Salvage, LootEligibilityCategory.Residential]);
        var catalog = new LootCatalog("shared-test", [first, second, excluded], [opportunity],
            [new("second", 3), new("excluded", 100), new("first", 1)]);

        for (var seed = 0; seed < 64; seed++)
        {
            var expected = Enumerable.Range(0, 3).Select(draw =>
                RaidConditions.Roll(seed, $"loot:shared:{draw}") * 4 < 3 ? "second" : "first").ToArray();
            var table = catalog.CreateTableBackedWorldItems("raid", seed);
            var activity = catalog.CreateOpportunityItems("raid", seed, "shared", "Low", 0, 0);
            Assert.Equal(expected, table.Select(instance => instance.Item.CatalogId));
            Assert.Equal(expected, activity.Select(instance => instance.Item.CatalogId));
            Assert.Equal(activity, catalog.CreateOpportunityItems("raid", seed, "shared", "Low", 0, 0));
            Assert.Equal(3, activity.Select(instance => instance.Item.InstanceId).Distinct().Count());
        }
    }

    [Fact]
    public void AddingOneSharedEntryMakesItAvailableToAllMatchingOpportunitiesWithoutEditingThem()
    {
        var old = Item("old");
        var added = Item("added", categories: [LootEligibilityCategory.Salvage, LootEligibilityCategory.Technology]);
        var opportunities = new[]
        {
            new OpportunityDefinition("locker", "Market", 10, [], 1, [LootEligibilityCategory.Salvage]),
            new OpportunityDefinition("robot-salvage", "Anywhere", 10, [], 1, [LootEligibilityCategory.Technology])
        };
        var catalog = new LootCatalog("expanded", [old, added], opportunities,
            [new("old", 1), new("added", 1)]);
        var renamed = new LootCatalog("expanded", catalog.Items,
            opportunities.Select(opportunity => opportunity with { Location = "Different Location" }), catalog.Pool);

        var sampled = Enumerable.Range(0, 64)
            .SelectMany(seed => catalog.CreateTableBackedWorldItems("raid", seed)).ToArray();
        foreach (var opportunity in opportunities)
        {
            Assert.Contains(sampled, instance => instance.OpportunityId == opportunity.Id && instance.Item.CatalogId == added.Id);
            Assert.Contains(Enumerable.Range(0, 64).SelectMany(seed =>
                catalog.CreateOpportunityItems("raid", seed, opportunity.Id, "Low", 0, 0)),
                instance => instance.Item.CatalogId == added.Id);
        }
        for (var seed = 0; seed < 64; seed++)
        {
            Assert.Equal(catalog.CreateTableBackedWorldItems("raid", seed), renamed.CreateTableBackedWorldItems("raid", seed));
            foreach (var opportunity in opportunities)
                Assert.Equal(catalog.CreateOpportunityItems("raid", seed, opportunity.Id, "High", 0.5, 0.75),
                    renamed.CreateOpportunityItems("raid", seed, opportunity.Id, "High", 0.5, 0.75));
        }
    }

    [Fact]
    public void DefaultSharedPoolMakesEveryMatchingItemReachableInBothGenerators()
    {
        var catalog = LootCatalog.Default;
        foreach (var opportunity in catalog.Opportunities)
        {
            var expected = catalog.Pool.Where(entry => catalog.GetItem(entry.CatalogId).EligibilityCategories
                .Any(opportunity.EligibleCategories.Contains)).Select(entry => entry.CatalogId).Order().ToArray();
            var tableItems = Enumerable.Range(0, 512)
                .SelectMany(seed => catalog.CreateTableBackedWorldItems("raid", seed))
                .Where(instance => instance.OpportunityId == opportunity.Id)
                .Select(instance => instance.Item.CatalogId).Distinct().Order().ToArray();
            var activityItems = Enumerable.Range(0, 512)
                .SelectMany(seed => catalog.CreateOpportunityItems("raid", seed, opportunity.Id, "Low", 0, 0))
                .Select(instance => instance.Item.CatalogId).Distinct().Order().ToArray();
            Assert.Equal(expected, tableItems);
            Assert.Equal(expected, activityItems);
        }
    }

    [Fact]
    public void CategoryEligibilityAndDropWeightAreSeparateFromRarityAndValue()
    {
        var cheapCommon = Item("cheap-common", value: 2, rarity: 1, categories: [LootEligibilityCategory.Residential]);
        var expensiveCommon = Item("expensive-common", value: 100, rarity: 1, categories: [LootEligibilityCategory.Residential]);
        var highRarity = Item("high-rarity", value: 2, rarity: 5, categories: [LootEligibilityCategory.Technology]);
        var table = new OpportunityDefinition(
            "residential-table",
            "Market",
            10,
            ["cheap-common"],
            1,
            [LootEligibilityCategory.Residential]);
        var catalog = new LootCatalog("weights-test-v1", [cheapCommon, expensiveCommon, highRarity], [table],
            [new("cheap-common", 1), new("expensive-common", 3), new("high-rarity", 100)]);

        var generated = Enumerable.Range(0, 512)
            .Select(seed => catalog.CreateOpportunityItems("weights-raid", seed, table.Id, "Low", 0, 0).Single().Item)
            .ToArray();

        Assert.DoesNotContain(generated, item => item.CatalogId == highRarity.Id);
        Assert.InRange(generated.Count(item => item.CatalogId == expensiveCommon.Id), 330, 430);
        Assert.All(generated, item => Assert.Equal(item.CatalogId, catalog.GetItem(item.CatalogId).Id));
    }

    [Theory]
    [InlineData(42)]
    [InlineData(12345)]
    [InlineData(99999)]
    public void TableBackedGenerationIsDeterministicForSameSeed(int seed)
    {
        var catalog = LootCatalog.Default;
        var raidId = "raid-seed-test";

        var runA = catalog.CreateTableBackedWorldItems(raidId, seed);
        var runB = catalog.CreateTableBackedWorldItems(raidId, seed);

        Assert.Equal(runA.Count, runB.Count);
        for (var i = 0; i < runA.Count; i++)
        {
            Assert.Equal(runA[i].OpportunityId, runB[i].OpportunityId);
            Assert.Equal(runA[i].Item.InstanceId, runB[i].Item.InstanceId);
            Assert.Equal(runA[i].Item.CatalogId, runB[i].Item.CatalogId);
            Assert.Equal(runA[i].Item.Name, runB[i].Item.Name);
            Assert.Equal(runA[i].Item.Value, runB[i].Item.Value);
            Assert.Equal(runA[i].Item.Flavor, runB[i].Item.Flavor);
        }

        // Each instance within the run must have a distinct InstanceId
        Assert.Equal(runA.Count, runA.Select(i => i.Item.InstanceId).Distinct().Count());
    }

    [Fact]
    public void TableBackedGenerationDrawsValidCatalogItems()
    {
        var catalog = LootCatalog.Default;
        var raidId = "raid-check-catalog";

        for (var seed = 0; seed < 50; seed++)
        {
            var items = catalog.CreateTableBackedWorldItems(raidId, seed);
            Assert.Equal(6, items.Count); // 3 opportunities * 2 draws each

            foreach (var instance in items)
            {
                Assert.True(catalog.TryGetItem(instance.Item.CatalogId, out var catalogItem));
                Assert.NotNull(catalogItem);
                Assert.Equal(catalogItem.Name, instance.Item.Name);
                Assert.Equal(catalogItem.Value, instance.Item.Value);
                Assert.Equal(catalogItem.Flavor, instance.Item.Flavor);
            }
        }
    }

    [Fact]
    public void TableBackedGenerationRetainsConfiguredOpportunityAndDrawOrder()
    {
        var catalog = LootCatalog.Default;
        var generated = catalog.CreateTableBackedWorldItems("ordered", 725);
        var expectedOrder = catalog.Opportunities
            .SelectMany(opportunity => Enumerable.Repeat(opportunity.Id, opportunity.DrawCount));

        Assert.Equal(expectedOrder, generated.Select(item => item.OpportunityId));
        Assert.Equal(generated, catalog.CreateTableBackedWorldItems("ordered", 725));
    }

    [Fact]
    public void RaidSimulationWithTableBackedLootRunsToCompletion()
    {
        var start = DateTimeOffset.Parse("2026-10-12T00:00:00Z");
        var simulation = new RaidSimulation(
            "table-sim-1",
            start,
            123,
            useTableBackedLoot: true, fastPacing: true);

        simulation.AddAutonomousRaider("p1", "raider-one", start);
        simulation.AddAutonomousRaider("p2", "raider-two", start);

        simulation.Advance(start.AddMinutes(3));

        var p1 = simulation.Project("p1", start.AddMinutes(3));
        var p2 = simulation.Project("p2", start.AddMinutes(3));

        Assert.Equal("Completed", p1.Status);
        Assert.Equal("Completed", p2.Status);

        var totalExtracted = p1.MyExtractedLoot.Concat(p2.MyExtractedLoot).ToArray();
        // Unique instance IDs across all extracted items
        Assert.Equal(totalExtracted.Length, totalExtracted.Select(i => i.InstanceId).Distinct().Count());

        // Every extracted item has positive value and valid catalog item
        foreach (var item in totalExtracted)
        {
            Assert.True(item.Value > 0);
            Assert.True(simulation.Catalog.TryGetItem(item.CatalogId, out _));
        }
        var marketItems = totalExtracted.Where(item =>
            p1.Events.Concat(p2.Events).Any(raidEvent =>
                raidEvent.Kind == "ItemAllocated" &&
                raidEvent.OpportunityId == "market-locker" &&
                raidEvent.ItemInstanceId == item.InstanceId)).ToArray();
        Assert.NotEmpty(marketItems);
        Assert.All(marketItems, item => Assert.Contains(
            simulation.Catalog.GetItem(item.CatalogId).EligibilityCategories,
            category => category is LootEligibilityCategory.Residential or LootEligibilityCategory.Salvage));
    }
}
