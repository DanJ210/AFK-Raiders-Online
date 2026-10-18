using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Content;

internal sealed record LootCatalogDocument(
    int SchemaVersion,
    string ContentVersion,
    IReadOnlyList<LootCatalogItem> Items,
    IReadOnlyList<OpportunityDefinition> Opportunities,
    IReadOnlyList<WeightedLootEntry> Pool);

internal sealed record ShopCatalogDocument(
    int SchemaVersion,
    IReadOnlyList<WeaponDefinition> Weapons,
    IReadOnlyList<SupplyDefinition> Supplies);

internal static class CatalogContent
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver();
        resolver.Modifiers.Add(type =>
        {
            foreach (var property in type.Properties)
                property.IsRequired = true;
        });
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            RespectRequiredConstructorParameters = true,
            RespectNullableAnnotations = true,
            AllowDuplicateProperties = false,
            TypeInfoResolver = resolver
        };
    }

    internal static Stream OpenBundled(string fileName) =>
        typeof(CatalogContent).Assembly.GetManifestResourceStream($"AFKRaiders.Server.Content.{fileName}")
        ?? throw new InvalidDataException($"Bundled catalog '{fileName}' is missing. Rebuild and redeploy the server.");

    internal static LootCatalog LoadLoot()
    {
        using var stream = OpenBundled("loot-catalog.json");
        return ReadLoot(stream);
    }

    internal static ShopCatalogDocument LoadShop()
    {
        using var stream = OpenBundled("shop-catalog.json");
        return ReadShop(stream);
    }

    internal static LootCatalog ReadLoot(Stream stream)
    {
        const string source = "loot-catalog.json";
        var document = Read<LootCatalogDocument>(stream, source);
        Require(document.SchemaVersion == 4, source, "Unsupported schemaVersion; expected 4.");
        Require(document.Items is { Count: > 0 } && document.Opportunities is { Count: > 0 },
            source, "Items and opportunities must be nonempty arrays.");
        foreach (var item in document.Items)
        {
            Require(item is not null, source, "Items cannot contain null.");
            Require(!string.IsNullOrWhiteSpace(item.Category), source, $"Item '{item.Id}' requires a category.");
            Require(item.EligibilityCategories is { Count: > 0 }, source,
                $"Item '{item.Id}' requires eligibilityCategories.");
        }
        foreach (var opportunity in document.Opportunities)
        {
            Require(opportunity is not null, source, "Opportunities cannot contain null.");
            Require(opportunity.FixedItemIds is not null, source, $"Opportunity '{opportunity.Id}' requires fixedItemIds.");
            Require(opportunity.FixedItemIds.All(id => !string.IsNullOrWhiteSpace(id)), source,
                $"Opportunity '{opportunity.Id}' contains an empty fixed item reference.");
            Require(opportunity.EligibleCategories is { Count: > 0 }, source,
                $"Opportunity '{opportunity.Id}' requires eligibleCategories.");
        }
        try
        {
            return new LootCatalog(document.ContentVersion, document.Items, document.Opportunities, document.Pool);
        }
        catch (LootCatalogValidationException exception)
        {
            throw new InvalidDataException($"{source}: {exception.Message}", exception);
        }
    }

    internal static ShopCatalogDocument ReadShop(Stream stream)
    {
        const string source = "shop-catalog.json";
        var document = Read<ShopCatalogDocument>(stream, source);
        Require(document.SchemaVersion == 1, source, "Unsupported schemaVersion; expected 1.");
        Require(document.Weapons is { Count: > 0 } && document.Supplies is { Count: > 0 },
            source, "Weapons and supplies must be nonempty arrays.");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var weapon in document.Weapons)
        {
            Require(weapon is not null, source, "Weapons cannot contain null.");
            ValidateEntry(weapon.Id, weapon.Name, weapon.Flavor, weapon.Value, weapon.Rarity, ids, source);
            Require(weapon.Damage > 0 && weapon.DamageMin > 0 && weapon.DamageMax >= weapon.DamageMin
                && double.IsFinite(weapon.DamageMultiplier) && weapon.DamageMultiplier > 0
                && weapon.DurabilityMax > 0 && weapon.RepairCost >= 0,
                source, $"Weapon '{weapon.Id}' has invalid damage, durability or repair fields.");
        }
        Require(document.Weapons.Any(weapon => string.Equals(weapon.Id, "tea_kettle", StringComparison.OrdinalIgnoreCase)),
            source, "Starter weapon 'tea_kettle' is missing.");
        foreach (var supply in document.Supplies)
        {
            Require(supply is not null, source, "Supplies cannot contain null.");
            ValidateEntry(supply.Id, supply.Name, supply.Flavor, supply.Value, supply.Rarity, ids, source);
            Require(supply.HealAmount >= 0 && supply.ReviveAmount >= 0 && supply.ChargeAmount >= 0,
                source, $"Supply '{supply.Id}' has a negative effect amount.");
            Require(supply.Category switch
            {
                "Medical" => (supply.HealAmount > 0 || supply.ReviveAmount > 0) && supply.ChargeAmount == 0,
                "ShieldRecharger" => supply.ChargeAmount > 0 && supply.HealAmount == 0 && supply.ReviveAmount == 0,
                _ => false
            }, source, $"Supply '{supply.Id}' has an unsupported category or incompatible effects.");
        }
        return document with
        {
            Weapons = Array.AsReadOnly(document.Weapons.ToArray()),
            Supplies = Array.AsReadOnly(document.Supplies.ToArray())
        };
    }

    private static void ValidateEntry(string id, string name, string flavor, int value, int rarity,
        HashSet<string> ids, string source)
    {
        Require(!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name)
            && !string.IsNullOrWhiteSpace(flavor), source, "Shop entries require id, name and flavor.");
        Require(ids.Add(id), source, $"Duplicate shop ID '{id}' (case-insensitive).");
        Require(value >= 0 && rarity > 0, source, $"Shop entry '{id}' has an invalid value or rarity.");
    }

    internal static T Read<T>(Stream stream, string source)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(stream, Options)
                ?? throw new InvalidDataException($"{source}: Catalog cannot be null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"{source}: {exception.Message}", exception);
        }
    }

    private static void Require(
        [System.Diagnostics.CodeAnalysis.DoesNotReturnIf(false)] bool condition, string source, string message)
    {
        if (!condition)
            throw new InvalidDataException($"{source}: {message}");
    }
}
