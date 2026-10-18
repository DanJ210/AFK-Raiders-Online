using System.Text;
using System.Text.Json.Nodes;
using AFKRaiders.Server.Content;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class CatalogContentTests
{
    [Theory]
    [InlineData("loot-catalog.json")]
    [InlineData("shop-catalog.json")]
    public void StrictParsingRejectsMalformedNullUnknownAndDuplicateProperties(string file)
    {
        foreach (var json in new[] { "{", "null", "{}", "{\"schemaVersion\":1,\"schemaVersion\":1}" })
        {
            var exception = Assert.Throws<InvalidDataException>(() => Read(file, json));
            Assert.Contains(file, exception.Message);
        }
        var document = BundledDocument(file);
        document["unexpected"] = true;
        Assert.Throws<InvalidDataException>(() => Read(file, document.ToJsonString()));
    }

    [Theory]
    [InlineData("loot-catalog.json", "items")]
    [InlineData("loot-catalog.json", "opportunities")]
    [InlineData("loot-catalog.json", "pool")]
    [InlineData("shop-catalog.json", "weapons")]
    [InlineData("shop-catalog.json", "supplies")]
    public void CollectionsAreRequiredNonemptyAndCannotContainNull(string file, string collection)
    {
        foreach (var mutation in new[] { "missing", "null", "empty", "nullEntry" })
        {
            var document = BundledDocument(file);
            switch (mutation)
            {
                case "missing": document.Remove(collection); break;
                case "null": document[collection] = null; break;
                case "empty": document[collection] = new JsonArray(); break;
                case "nullEntry": document[collection] = new JsonArray((JsonNode?)null); break;
            }
            var exception = Assert.Throws<InvalidDataException>(() => Read(file, document.ToJsonString()));
            Assert.Contains(file, exception.Message);
        }
    }

    [Theory]
    [InlineData("loot-catalog.json")]
    [InlineData("shop-catalog.json")]
    public void UnsupportedSchemaVersionIsRejected(string file)
    {
        var document = BundledDocument(file);
        document["schemaVersion"] = file == "loot-catalog.json" ? 1 : 2;
        var exception = Assert.Throws<InvalidDataException>(() => Read(file, document.ToJsonString()));
        Assert.Contains("schemaVersion", exception.Message);
    }

    [Fact]
    public void PreviousPerOpportunityPoolSchemaIsRejected()
    {
        var document = BundledDocument("loot-catalog.json");
        document["schemaVersion"] = 2;
        var exception = Assert.Throws<InvalidDataException>(() => Read("loot-catalog.json", document.ToJsonString()));
        Assert.Contains("expected 4", exception.Message);
    }

    [Theory]
    [InlineData("loot-catalog.json", "items", "value")]
    [InlineData("loot-catalog.json", "items", "category")]
    [InlineData("loot-catalog.json", "items", "eligibilityCategories")]
    [InlineData("loot-catalog.json", "opportunities", "drawCount")]
    [InlineData("loot-catalog.json", "pool", "catalogId")]
    [InlineData("loot-catalog.json", "pool", "dropWeight")]
    [InlineData("loot-catalog.json", "opportunities", "fixedItemIds")]
    [InlineData("loot-catalog.json", "opportunities", "eligibleCategories")]
    [InlineData("loot-catalog.json", "opportunities", "activity")]
    [InlineData("loot-catalog.json", "opportunities", "title")]
    [InlineData("loot-catalog.json", "opportunities", "startText")]
    [InlineData("loot-catalog.json", "opportunities", "completedText")]
    [InlineData("loot-catalog.json", "opportunities", "contestable")]
    [InlineData("shop-catalog.json", "weapons", "damageMultiplier")]
    [InlineData("shop-catalog.json", "supplies", "reviveAmount")]
    public void DefinitionFieldsCannotBeSilentlyDefaulted(string file, string collection, string field)
    {
        var document = BundledDocument(file);
        document[collection]![0]!.AsObject().Remove(field);
        Assert.Throws<InvalidDataException>(() => Read(file, document.ToJsonString()));
    }

    [Theory]
    [InlineData("nullFixed")]
    [InlineData("nullPoolEntry")]
    [InlineData("unknownFixed")]
    [InlineData("unknownPool")]
    [InlineData("emptyPool")]
    [InlineData("noLoot")]
    [InlineData("zeroWeight")]
    [InlineData("overflowWeights")]
    [InlineData("blankCategory")]
    [InlineData("unknownEligibilityCategory")]
    [InlineData("emptyEligibilityCategories")]
    [InlineData("duplicateEligibilityCategory")]
    [InlineData("unknownTableCategory")]
    [InlineData("emptyTableCategories")]
    [InlineData("duplicatePoolEntry")]
    [InlineData("duplicateFixedEntry")]
    [InlineData("emptyEligiblePool")]
    [InlineData("zeroDrawCount")]
    [InlineData("duplicateItem")]
    [InlineData("duplicateOpportunity")]
    [InlineData("legacyOpportunityPool")]
    [InlineData("duplicateTableCategory")]
    [InlineData("negativeWeight")]
    [InlineData("blankPoolReference")]
    [InlineData("unknownActivity")]
    [InlineData("blankActivityText")]
    public void InvalidLootDefinitionsAreRejectedWithSourceContext(string mutation)
    {
        var document = BundledDocument("loot-catalog.json");
        var items = document["items"]!.AsArray();
        var opportunities = document["opportunities"]!.AsArray();
        var opportunity = opportunities[0]!.AsObject();
        var pool = document["pool"]!.AsArray();
        switch (mutation)
        {
            case "nullFixed": opportunity["fixedItemIds"] = null; break;
            case "nullPoolEntry": pool.Add((JsonNode?)null); break;
            case "unknownFixed": opportunity["fixedItemIds"]![0] = "unknown"; break;
            case "unknownPool": pool[0]!["catalogId"] = "unknown"; break;
            case "blankPoolReference": pool[0]!["catalogId"] = " "; break;
            case "emptyPool": pool.Clear(); break;
            case "noLoot":
                document["pool"] = null;
                opportunity["fixedItemIds"] = new JsonArray();
                break;
            case "zeroWeight": pool[0]!["dropWeight"] = 0; break;
            case "negativeWeight": pool[0]!["dropWeight"] = -1; break;
            case "overflowWeights": pool[0]!["dropWeight"] = int.MaxValue; break;
            case "blankCategory": items[0]!["category"] = " "; break;
            case "unknownEligibilityCategory": items[0]!["eligibilityCategories"]![0] = "Unknown"; break;
            case "emptyEligibilityCategories": items[0]!["eligibilityCategories"] = new JsonArray(); break;
            case "duplicateEligibilityCategory": items[0]!["eligibilityCategories"]!.AsArray().Add("Salvage"); break;
            case "unknownTableCategory": opportunity["eligibleCategories"]![0] = "Unknown"; break;
            case "emptyTableCategories": opportunity["eligibleCategories"] = new JsonArray(); break;
            case "duplicatePoolEntry": pool.Add(pool[0]!.DeepClone()); break;
            case "duplicateFixedEntry": opportunity["fixedItemIds"]!.AsArray().Add(opportunity["fixedItemIds"]![0]!.DeepClone()); break;
            case "emptyEligiblePool":
                foreach (var item in items)
                    item!["eligibilityCategories"] = new JsonArray(JsonValue.Create("Salvage"));
                opportunities[1]!["eligibleCategories"] = new JsonArray(JsonValue.Create("Residential"));
                break;
            case "zeroDrawCount": opportunity["drawCount"] = 0; break;
            case "duplicateItem": items.Add(items[0]!.DeepClone()); break;
            case "duplicateOpportunity": opportunities.Add(opportunity.DeepClone()); break;
            case "legacyOpportunityPool": opportunity["pool"] = pool.DeepClone(); break;
            case "duplicateTableCategory": opportunity["eligibleCategories"]!.AsArray().Add("Salvage"); break;
            case "unknownActivity": opportunity["activity"] = "Unreviewed"; break;
            case "blankActivityText": opportunity["startText"] = " "; break;
        }
        var exception = Assert.Throws<InvalidDataException>(() => Read("loot-catalog.json", document.ToJsonString()));
        Assert.Contains("loot-catalog.json", exception.Message);
    }

    [Theory]
    [InlineData("duplicateWeapon")]
    [InlineData("duplicateSupply")]
    [InlineData("crossTypeDuplicate")]
    [InlineData("missingStarter")]
    [InlineData("blankName")]
    [InlineData("negativePrice")]
    [InlineData("zeroRarity")]
    [InlineData("zeroDamage")]
    [InlineData("zeroMultiplier")]
    [InlineData("invertedDamage")]
    [InlineData("zeroDurability")]
    [InlineData("negativeRepair")]
    [InlineData("unknownCategory")]
    [InlineData("negativeEffect")]
    [InlineData("noEffect")]
    [InlineData("incompatibleEffect")]
    public void InvalidShopDefinitionsAreRejectedWithSourceContext(string mutation)
    {
        var document = BundledDocument("shop-catalog.json");
        var weapons = document["weapons"]!.AsArray();
        var supplies = document["supplies"]!.AsArray();
        var weapon = weapons[0]!.AsObject();
        var supply = supplies[0]!.AsObject();
        switch (mutation)
        {
            case "duplicateWeapon":
                weapons.Add(weapon.DeepClone());
                weapons[^1]!["id"] = "TEA_KETTLE";
                break;
            case "duplicateSupply": supplies.Add(supply.DeepClone()); break;
            case "crossTypeDuplicate": supply["id"] = "tea_kettle"; break;
            case "missingStarter": weapons.RemoveAt(0); break;
            case "blankName": supply["name"] = " "; break;
            case "negativePrice": weapon["value"] = -1; break;
            case "zeroRarity": supply["rarity"] = 0; break;
            case "zeroDamage": weapon["damage"] = 0; break;
            case "zeroMultiplier": weapon["damageMultiplier"] = 0; break;
            case "invertedDamage": weapon["damageMin"] = 100; break;
            case "zeroDurability": weapon["durabilityMax"] = 0; break;
            case "negativeRepair": weapon["repairCost"] = -1; break;
            case "unknownCategory": supply["category"] = "Unknown"; break;
            case "negativeEffect": supply["healAmount"] = -1; break;
            case "noEffect": supply["healAmount"] = 0; break;
            case "incompatibleEffect": supply["chargeAmount"] = 10; break;
        }
        var exception = Assert.Throws<InvalidDataException>(() => Read("shop-catalog.json", document.ToJsonString()));
        Assert.Contains("shop-catalog.json", exception.Message);
    }

    [Fact]
    public void MissingBundledResourceFailsExplicitly()
    {
        var exception = Assert.Throws<InvalidDataException>(() => CatalogContent.OpenBundled("missing.json"));
        Assert.Contains("missing.json", exception.Message);
    }

    [Fact]
    public void ExistingLookupComparisonsAndReadOnlyShopCollectionsArePreserved()
    {
        Assert.True(ShopCatalog.TryGetWeapon("TEA_KETTLE", out var weapon));
        Assert.Equal(ShopCatalog.GetStarterWeapon(), weapon);
        Assert.True(ShopCatalog.TryGetSupply("BANDAGE_WHITE", out _));
        Assert.False(LootCatalog.Default.TryGetItem("SCRAP_METAL_BASIC", out _));
        Assert.True(Assert.IsAssignableFrom<IList<WeaponDefinition>>(ShopCatalog.Weapons).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<IList<SupplyDefinition>>(ShopCatalog.Supplies).IsReadOnly);
    }

    private static JsonObject BundledDocument(string file)
    {
        using var stream = CatalogContent.OpenBundled(file);
        return JsonNode.Parse(stream)!.AsObject();
    }

    private static void Read(string file, string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        if (file == "loot-catalog.json")
            CatalogContent.ReadLoot(stream);
        else
            CatalogContent.ReadShop(stream);
    }
}
