using System.Text.Json;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class ReviewedLootIntakeTests
{
    private static readonly string[] ResidentialIds =
    [
        "lost_diary_page", "crumpled_memo", "left_boot", "fingerless_gloves",
        "rusted_spork", "vintage_comic_book", "right_boot", "single_sock_fancy",
        "old_photograph", "collector_plate_shattered"
    ];
    private static readonly string[] RobotIds =
        ["energy_core_spent", "prototype_chip_faulty", "broken_servo", "arc_encrypted_drive_broken"];

    [Theory]
    [InlineData("lost_diary_page", "Lost Diary Page", 10, 2, "personal_junk", 20,
        "Reads: 'Day 14: Still no snacks.'")]
    [InlineData("crumpled_memo", "Crumpled Memo", 10, 2, "personal_junk", 20,
        "Subject: MANDATORY FUN. The irony is not lost on you.")]
    [InlineData("left_boot", "Left Boot", 15, 2, "apparel_accessories", 20,
        "Someone out there has the right one. The pair would be worth something.")]
    [InlineData("fingerless_gloves", "Fingerless Gloves", 20, 2, "apparel_accessories", 20,
        "For when you want to be edgy AND cold.")]
    [InlineData("rusted_spork", "Rusted Spork", 10, 1, "valuables", 20,
        "A relic of simpler, tetanus-filled times.")]
    [InlineData("vintage_comic_book", "Vintage Comic Book", 20, 2, "valuables", 15,
        "Features a hero who fights robots. The cover is torn but the nostalgia is intact.")]
    [InlineData("energy_core_spent", "Spent Energy Core", 10, 1, "arc_tech", 25,
        "Completely drained. Same.")]
    [InlineData("prototype_chip_faulty", "Prototype Chip (Faulty)", 20, 2, "arc_tech", 20,
        "It sparks occasionally. You pretend not to notice.")]
    [InlineData("right_boot", "Right Boot", 15, 2, "apparel_accessories", 20,
        "Finally. The prophecy is fulfilled.")]
    [InlineData("single_sock_fancy", "Single Sock (Fancy)", 20, 2, "personal_junk", 15,
        "Embroidered with initials you don't recognize.")]
    [InlineData("old_photograph", "Old Photograph", 15, 2, "personal_junk", 18,
        "A smiling family. You hope they're okay.")]
    [InlineData("broken_servo", "Broken Servo", 20, 2, "scrap_components", 25,
        "It tries to move. It fails.")]
    [InlineData("arc_encrypted_drive_broken", "Chassis Encrypted Drive (Broken)", 18, 2, "arc_tech", 20,
        "Contains exactly one intact file: 'notes.txt'. It's empty.")]
    [InlineData("collector_plate_shattered", "Shattered Collector Plate", 8, 1, "valuables", 20,
        "Still collectible. Technically.")]
    public void SelectedSpecializedSourceDefinitionsArePreservedAsSellableLoot(
        string id, string name, int value, int rarity, string category, int weight, string flavor)
    {
        var catalog = LootCatalog.Default;
        var item = catalog.GetItem(id);
        Assert.Equal(new LootCatalogItem(id, name, value, rarity, category, flavor,
            item.EligibilityCategories), item);
        Assert.Equal(weight, Assert.Single(catalog.Pool, entry => entry.CatalogId == id).DropWeight);
        var expectedCategories = id switch
        {
            "broken_servo" => new[] { LootEligibilityCategory.Robot, LootEligibilityCategory.Salvage },
            "arc_encrypted_drive_broken" => new[] { LootEligibilityCategory.Robot, LootEligibilityCategory.Technology },
            "collector_plate_shattered" => new[] { LootEligibilityCategory.Residential, LootEligibilityCategory.Salvage },
            _ when ResidentialIds.Contains(id) => new[] { LootEligibilityCategory.Residential },
            _ => new[] { LootEligibilityCategory.Robot, LootEligibilityCategory.Technology }
        };
        Assert.Equal(expectedCategories, item.EligibilityCategories);
        Assert.DoesNotContain(ShopCatalog.Weapons, weapon => weapon.Id == id);
        Assert.DoesNotContain(ShopCatalog.Supplies, supply => supply.Id == id);
    }

    [Fact]
    public void IntakeKeepsReviewedEntriesAddsFieldWeaponsAndKeepsFiniteOpportunities()
    {
        var catalog = LootCatalog.Default;
        var expectedIds = CatalogCompatibilityTests.C3ItemIds
            .Concat(["lost_diary_page", "crumpled_memo", "left_boot", "fingerless_gloves", "rusted_spork", "vintage_comic_book"])
            .Concat(["energy_core_spent", "prototype_chip_faulty"])
            .Concat(["right_boot", "single_sock_fancy", "old_photograph", "broken_servo",
                "arc_encrypted_drive_broken", "collector_plate_shattered"])
            .Concat(["aspperigo", "vernerider"])
            .ToArray();
        Assert.Equal("desperanza-shared-loot-v6", catalog.ContentVersion);
        Assert.Equal(expectedIds, catalog.Items.Select(item => item.Id));
        Assert.Equal(expectedIds, catalog.Pool.Select(entry => entry.CatalogId));
        Assert.Equal(new[] { 35, 30, 25, 20, 18, 15, 22, 18, 25, 18, 20 },
            catalog.Pool.Take(11).Select(entry => entry.DropWeight));
        Assert.Equal(9, catalog.Opportunities.Count);
        Assert.Equal(new[] { "Market", "Salvage Yard", "Service Depot" },
            catalog.Opportunities.Select(opportunity => opportunity.Location).Distinct());
        Assert.All(catalog.Opportunities.GroupBy(opportunity => opportunity.Location), location =>
        {
            Assert.Equal(3, location.Count());
            Assert.Equal(3, location.Select(opportunity => opportunity.Activity).Distinct().Count());
        });
        Assert.All(catalog.Opportunities, opportunity =>
            Assert.True(catalog.Pool.Count(entry => catalog.GetItem(entry.CatalogId).EligibilityCategories
                .Any(opportunity.EligibleCategories.Contains)) >= 5));
        Assert.All(catalog.Opportunities, opportunity => Assert.Equal(2, opportunity.DrawCount));
        Assert.All(catalog.Items.Where(item => item.Category != "weapons"),
            item => Assert.InRange(item.Value, 5, 35));
        Assert.Equal(new[] { 65, 140 },
            catalog.Items.Where(item => item.Category == "weapons").Select(item => item.Value));
    }

    [Fact]
    public void WorldSeedSelectsAndRunsOneDistinctFieldActivityPerLocation()
    {
        var catalog = LootCatalog.Default;
        var seenActivities = catalog.Opportunities
            .GroupBy(opportunity => opportunity.Location)
            .ToDictionary(group => group.Key, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        var start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");

        for (var seed = 0; seed < 128; seed++)
        {
            var selected = catalog.SelectWorldOpportunities(seed);
            Assert.Equal(3, selected.Count);
            Assert.Equal(selected, catalog.SelectWorldOpportunities(seed));
            Assert.Equal(6, catalog.CreateTableBackedWorldItems("world", seed).Count);

            foreach (var opportunity in selected)
                seenActivities[opportunity.Location].Add(opportunity.Activity);

            var simulation = new RaidSimulation("activity-seed", start, seed);
            simulation.AddAutonomousRaider("one", "raider-one", start);
            Assert.Equal(selected.Select(opportunity => opportunity.Id).Order(),
                simulation.Project("one", start).Opportunities.Select(opportunity => opportunity.OpportunityId).Order());
            simulation.Advance(start.AddSeconds(60));
            var projection = simulation.Project("one", start.AddSeconds(60));
            var market = Assert.Single(selected, opportunity => opportunity.Location == "Market");
            Assert.Equal(market.Activity, projection.Players.Single().Activity);
            Assert.Equal(market.Title, projection.CurrentThread?.Title);
        }

        Assert.All(seenActivities.Values, activities => Assert.Equal(3, activities.Count));
    }

    [Theory]
    [InlineData("market-locker")]
    [InlineData("depot-locker")]
    public void NewLootIsActuallyAllocatedByAutonomousSharedSearchesWithoutHandlersAndNeverTwice(string opportunityId)
    {
        var expectedIds = opportunityId == "market-locker" ? ResidentialIds : RobotIds;
        var observed = new HashSet<string>(StringComparer.Ordinal);
        var start = DateTimeOffset.Parse("2026-10-14T12:00:00Z");
        for (var seed = 0; seed < 512 && observed.Count < expectedIds.Length; seed++)
        {
            var simulation = new RaidSimulation("c5-intake", start, seed,
                useTableBackedLoot: true, fastPacing: true, riskReward: true);
            simulation.AddAutonomousRaider("one", "raider-one", start);
            simulation.AddAutonomousRaider("two", "raider-two", start);
            var end = start.AddDays(1);
            simulation.Advance(end);
            var one = simulation.Project("one", end);
            var two = simulation.Project("two", end);
            Assert.True(simulation.Completed);
            var search = one.Events.FirstOrDefault(evt =>
                evt.Kind == "SearchCompleted" && evt.OpportunityId == opportunityId);
            if (search is null) continue;
            if (opportunityId == "depot-locker")
            {
                if (one.RobotEncounter?.ParticipantIds.Count != 2) continue;
                Assert.Equal("Defeated", one.Robot.Status);
                Assert.Equal(new[] { "one", "two" }, one.RobotEncounter!.ParticipantIds);
                Assert.True(Assert.Single(one.Events, evt => evt.Kind == "RobotDefeated").At < search.At);
            }
            var awards = one.Events.Concat(two.Events).Where(evt => evt.Kind == "ItemAllocated").ToArray();
            Assert.Equal(awards.Length, awards.Select(evt => evt.ItemInstanceId).Distinct().Count());
            Assert.InRange(awards.Length, 2, 6);
            var opportunityAwards = awards.Where(evt => evt.OpportunityId == opportunityId).ToArray();
            Assert.Equal(2, opportunityAwards.Length);
            foreach (var award in opportunityAwards)
            {
                Assert.Single(award.ParticipantIds);
                Assert.Equal(search.At, award.At);
                var definition = Assert.Single(simulation.Catalog.Items, item => item.Name == award.ItemName);
                if (expectedIds.Contains(definition.Id)) observed.Add(definition.Id);
            }
            var extracted = one.MyExtractedLoot.Concat(two.MyExtractedLoot).ToArray();
            Assert.Equal(extracted.Length, extracted.Select(item => item.InstanceId).Distinct().Count());
            var before = JsonSerializer.Serialize(new[] { one, two });
            Assert.False(simulation.Advance(end));
            Assert.Equal(before, JsonSerializer.Serialize(new[]
            {
                simulation.Project("one", end), simulation.Project("two", end)
            }));
        }
        Assert.Equal(expectedIds.Order(), observed.Order());
    }
}
