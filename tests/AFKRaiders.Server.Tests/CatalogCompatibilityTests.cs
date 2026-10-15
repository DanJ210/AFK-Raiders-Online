using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Tests;

public sealed class CatalogCompatibilityTests
{
    private const string LegacyC1Fingerprint =
        "52392C0BB009F79A680F03AAC5C9C46BF2387CB4AD98B7F7D9A39EDD3A4540C0";

    [Fact]
    public void SharedLootV3RemainsDistinctFromTheExplicitC1CompatibilityBaseline()
    {
        var catalog = LootCatalog.Default;
        var outputs = Enumerable.Range(0, 100).Select(seed => new
        {
            Table = catalog.CreateTableBackedWorldItems("compatibility", seed),
            Opportunities = catalog.Opportunities.SelectMany(opportunity =>
                new[] { "Low", "Medium", "High" }.SelectMany(danger =>
                    new[] { 0.0, 0.5, 1.0 }.Select(exposure =>
                        catalog.CreateOpportunityItems("compatibility", seed, opportunity.Id,
                            danger, exposure, 0.75)))).ToArray()
        }).ToArray();
        var snapshot = JsonSerializer.Serialize(new
        {
            catalog.ContentVersion,
            catalog.Items,
            catalog.Opportunities,
            catalog.Pool,
            ShopCatalog.Weapons,
            ShopCatalog.Supplies,
            Starter = ShopCatalog.GetStarterWeapon(),
            Fixed = catalog.CreateFixedWorldItems("compatibility"),
            Outputs = outputs
        });
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot)));
        // C1's exact pre-migration fingerprint remains frozen; later content changes are deliberate.
        Assert.NotEqual(LegacyC1Fingerprint, hash);
        Assert.Equal("desperanza-shared-loot-v3", catalog.ContentVersion);
    }
}
