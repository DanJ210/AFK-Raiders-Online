using System.Text.Json;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class ReviewedLootIntakeTests
{
    private static readonly string[] ResidentialIds =
    [
        "lost_diary_page", "crumpled_memo", "left_boot", "fingerless_gloves",
        "rusted_spork", "vintage_comic_book"
    ];
    private static readonly string[] RobotIds = ["energy_core_spent", "prototype_chip_faulty"];

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
    public void SelectedSpecializedSourceDefinitionsArePreservedAsSellableLoot(
        string id, string name, int value, int rarity, string category, int weight, string flavor)
    {
        var catalog = LootCatalog.Default;
        var item = catalog.GetItem(id);
        Assert.Equal(new LootCatalogItem(id, name, value, rarity, category, flavor,
            item.EligibilityCategories), item);
        Assert.Equal(weight, Assert.Single(catalog.Pool, entry => entry.CatalogId == id).DropWeight);
        Assert.Equal(ResidentialIds.Contains(id) ? [LootEligibilityCategory.Residential] :
            new[] { LootEligibilityCategory.Robot, LootEligibilityCategory.Technology },
            item.EligibilityCategories);
        Assert.DoesNotContain(ShopCatalog.Weapons, weapon => weapon.Id == id);
        Assert.DoesNotContain(ShopCatalog.Supplies, supply => supply.Id == id);
    }

    [Fact]
    public void IntakeOnlyAppendsEightEntriesAndLeavesOpportunityConfigurationAndFiniteCountsIntact()
    {
        var catalog = LootCatalog.Default;
        var expectedIds = CatalogCompatibilityTests.C3ItemIds.Concat(ResidentialIds).Concat(RobotIds).ToArray();
        Assert.Equal("desperanza-shared-loot-v4", catalog.ContentVersion);
        Assert.Equal(expectedIds, catalog.Items.Select(item => item.Id));
        Assert.Equal(expectedIds, catalog.Pool.Select(entry => entry.CatalogId));
        Assert.Equal(new[] { 35, 30, 25, 20, 18, 15, 22, 18, 25, 18, 20 },
            catalog.Pool.Take(11).Select(entry => entry.DropWeight));
        Assert.Equal(new[] { "market-locker", "humming-crate", "depot-locker" },
            catalog.Opportunities.Select(opportunity => opportunity.Id));
        Assert.Equal(new[] { 10, 12, 10 }, catalog.Opportunities.Select(opportunity => opportunity.DurationSeconds));
        Assert.Equal(new[] { "Market", "Salvage Yard", "Service Depot" },
            catalog.Opportunities.Select(opportunity => opportunity.Location));
        Assert.Equal(new[] { 16, 12, 9 }, catalog.Opportunities.Select(opportunity =>
            catalog.Pool.Count(entry => catalog.GetItem(entry.CatalogId).EligibilityCategories
                .Any(opportunity.EligibleCategories.Contains))));
        Assert.All(catalog.Opportunities, opportunity => Assert.Equal(2, opportunity.DrawCount));
        Assert.All(catalog.Items, item => Assert.InRange(item.Value, 5, 35));
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
