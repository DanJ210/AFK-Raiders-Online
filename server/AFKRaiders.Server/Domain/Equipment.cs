namespace AFKRaiders.Server.Domain;

/// <summary>
/// A weapon definition available for autonomous purchase or standard issue in Desperanza.
/// Provenance: afk-raiders/src/content/weapons.json
/// </summary>
public sealed record WeaponDefinition(
    string Id,
    string Name,
    int Damage,
    double DamageMultiplier,
    int DamageMin,
    int DamageMax,
    int Value,
    int Rarity,
    int DurabilityMax,
    int RepairCost,
    string Flavor
);

/// <summary>
/// A medical or shield-recharger consumable available in the Desperanza catalog.
/// Provenance: afk-raiders/src/content/healing_items.json and shield_rechargers.json
/// </summary>
public sealed record SupplyDefinition(
    string Id,
    string Name,
    string Category, // "Medical" or "ShieldRecharger"
    int Value,       // Purchase cost in coins
    int HealAmount,
    int ReviveAmount,
    int ChargeAmount,
    int Rarity,
    string Flavor
);

/// <summary>
/// An instance of a Raider-owned weapon.
/// </summary>
public sealed record OwnedWeapon(
    string InstanceId,
    string CatalogId,
    string Name,
    int CurrentDurability,
    int MaxDurability,
    int RepairCost,
    int Rarity,
    string Flavor
)
{
    public bool IsBroken => CurrentDurability <= 0;
}

/// <summary>
/// A stack of Raider-owned consumables in Desperanza.
/// </summary>
public sealed record OwnedSupplyStack(
    string CatalogId,
    string Name,
    string Category,
    int Quantity,
    int CostPerUnit,
    int HealAmount,
    int ReviveAmount,
    int ChargeAmount,
    int Rarity,
    string Flavor
);

/// <summary>
/// A staged supply item prepared for field deployment.
/// </summary>
public sealed record StagedSupplyItem(
    string CatalogId,
    string Name,
    string Category,
    int Quantity
);

/// <summary>
/// Full Desperanza shop catalog view.
/// </summary>
public sealed record ShopCatalogView(
    IReadOnlyList<WeaponDefinition> Weapons,
    IReadOnlyList<SupplyDefinition> Supplies
);

/// <summary>
/// Request for Handler spendable Signal support action in an active raid.
/// </summary>
public sealed record HandlerSupportActionRequest(
    string CommandId,
    string ActionType
);

/// <summary>
/// Response after executing a Handler support action.
/// </summary>
public sealed record HandlerSupportActionResponse(
    string CommandId,
    string ActionType,
    bool Success,
    int RemainingSignal,
    string Message
);

/// <summary>
/// Request to designate an item in backpack for Secret Hidden Pocket protection.
/// </summary>
public sealed record DesignateHiddenPocketRequest(
    string CommandId,
    string? ItemInstanceId
);

/// <summary>
/// Response after designating an item into Secret Hidden Pocket.
/// </summary>
public sealed record DesignateHiddenPocketResponse(
    string CommandId,
    string? ItemInstanceId,
    bool Success,
    string Message
);

/// <summary>
/// Authoritative Desperanza catalog for weapons, medical items, and shield rechargers.
/// </summary>
public static class ShopCatalog
{
    public const int MaxStagedMedicalCount = 4;
    public const int MaxStagedRechargerCount = 2;

    private static readonly Content.ShopCatalogDocument Definitions = Content.CatalogContent.LoadShop();

    public static readonly IReadOnlyList<WeaponDefinition> Weapons = Definitions.Weapons;

    public static readonly IReadOnlyList<SupplyDefinition> Supplies = Definitions.Supplies;

    private static readonly Dictionary<string, WeaponDefinition> WeaponsById =
        Weapons.ToDictionary(w => w.Id, StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, SupplyDefinition> SuppliesById =
        Supplies.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);

    public static bool TryGetWeapon(string id, out WeaponDefinition? weapon) =>
        WeaponsById.TryGetValue(id, out weapon);

    public static bool TryGetSupply(string id, out SupplyDefinition? supply) =>
        SuppliesById.TryGetValue(id, out supply);

    public static WeaponDefinition GetStarterWeapon() =>
        WeaponsById["tea_kettle"];

    public static OwnedWeapon CreateWeaponInstance(WeaponDefinition def) =>
        CreateWeaponInstance(def, $"wpn_{def.Id}_{Guid.NewGuid():N}"[..20]);

    public static OwnedWeapon CreateWeaponInstance(WeaponDefinition def, string instanceId) =>
        new(
            InstanceId: instanceId,
            CatalogId: def.Id,
            Name: def.Name,
            CurrentDurability: def.DurabilityMax,
            MaxDurability: def.DurabilityMax,
            RepairCost: def.RepairCost,
            Rarity: def.Rarity,
            Flavor: def.Flavor
        );
}
