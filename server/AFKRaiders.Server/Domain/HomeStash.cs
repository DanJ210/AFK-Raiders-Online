namespace AFKRaiders.Server.Domain;

public sealed class HomeStash
{
    public const int MaxCapacity = 120;
    private readonly List<ItemView> items = [];

    public HomeStash() { }

    public HomeStash(IEnumerable<ItemView> initialItems)
    {
        items.AddRange(initialItems);
    }

    public int Count => items.Count;
    public int TotalValue => items.Sum(item => item.Value);
    public IReadOnlyList<ItemView> Items => items.AsReadOnly();

    public HaulTransferResult TransferHaul(IReadOnlyList<ItemView> haul)
    {
        var incoming = haul.ToArray();
        var candidateItems = new List<ItemView>(items.Count + incoming.Length);
        candidateItems.AddRange(items);
        candidateItems.AddRange(incoming);

        var autoSold = new List<ItemView>();
        var overflowCount = Math.Max(0, candidateItems.Count - MaxCapacity);

        if (overflowCount > 0)
        {
            // Auto-sell the lowest-value items first.
            // Equal-value ties are broken deterministically by InstanceId ascending.
            var sortedCandidates = candidateItems
                .OrderBy(item => item.Value)
                .ThenBy(item => item.InstanceId, StringComparer.Ordinal)
                .ToList();

            autoSold.AddRange(sortedCandidates.Take(overflowCount));
            var retained = sortedCandidates.Skip(overflowCount).ToList();

            // Reconstruct the stash preserving a clean, stable order (e.g. by CatalogId then InstanceId)
            items.Clear();
            items.AddRange(retained.OrderBy(item => item.CatalogId, StringComparer.Ordinal)
                .ThenBy(item => item.InstanceId, StringComparer.Ordinal));
        }
        else
        {
            items.Clear();
            items.AddRange(candidateItems.OrderBy(item => item.CatalogId, StringComparer.Ordinal)
                .ThenBy(item => item.InstanceId, StringComparer.Ordinal));
        }

        var overflowCoins = autoSold.Sum(item => item.Value);
        var autoSoldIds = new HashSet<string>(autoSold.Select(item => item.InstanceId), StringComparer.Ordinal);
        var incomingRetainedCount = incoming.Count(item => !autoSoldIds.Contains(item.InstanceId));

        return new HaulTransferResult(
            incoming,
            incomingRetainedCount,
            autoSold,
            overflowCoins
        );
    }

    public SellStashResult SellByInstanceIds(IReadOnlyList<string> instanceIds)
    {
        if (instanceIds.Count == 0)
        {
            throw new RaidCommandException("EmptySellRequest");
        }

        var requestedIdSet = new HashSet<string>(instanceIds, StringComparer.Ordinal);
        if (requestedIdSet.Count != instanceIds.Count)
        {
            throw new RaidCommandException("DuplicateItemInSellRequest");
        }

        var toSell = new List<ItemView>();
        foreach (var id in instanceIds)
        {
            var match = items.FirstOrDefault(item => item.InstanceId == id);
            if (match is null)
            {
                throw new RaidCommandException($"ItemNotFound:{id}");
            }
            toSell.Add(match);
        }

        foreach (var item in toSell)
        {
            items.Remove(item);
        }

        var coinsGained = toSell.Sum(item => item.Value);
        return new SellStashResult(toSell, coinsGained);
    }

    public SellStashResult SellByCatalogId(string catalogId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(catalogId))
        {
            throw new RaidCommandException("InvalidCatalogId");
        }
        if (quantity <= 0)
        {
            throw new RaidCommandException("InvalidQuantity");
        }

        var matches = items.Where(item => item.CatalogId == catalogId)
            .OrderBy(item => item.Value)
            .ThenBy(item => item.InstanceId, StringComparer.Ordinal)
            .ToList();

        if (matches.Count < quantity)
        {
            throw new RaidCommandException($"InsufficientQuantity:Requested {quantity}, available {matches.Count}");
        }

        var toSell = matches.Take(quantity).ToList();
        foreach (var item in toSell)
        {
            items.Remove(item);
        }

        var coinsGained = toSell.Sum(item => item.Value);
        return new SellStashResult(toSell, coinsGained);
    }

    public IReadOnlyList<StashGroupView> GetGroups()
    {
        return items
            .GroupBy(item => item.CatalogId, StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                var ordered = group.OrderBy(item => item.InstanceId, StringComparer.Ordinal).ToList();
                return new StashGroupView(
                    CatalogId: group.Key,
                    Name: first.Name,
                    UnitValue: first.Value,
                    Quantity: ordered.Count,
                    TotalValue: ordered.Sum(item => item.Value),
                    Flavor: first.Flavor,
                    InstanceIds: ordered.Select(item => item.InstanceId).ToArray()
                );
            })
            .OrderByDescending(group => group.TotalValue)
            .ThenBy(group => group.Name, StringComparer.Ordinal)
            .ToArray();
    }
}

public sealed record HaulTransferResult(
    IReadOnlyList<ItemView> IncomingHaul,
    int IncomingRetainedCount,
    IReadOnlyList<ItemView> AutoSoldItems,
    int OverflowCoinsGained
);

public sealed record SellStashResult(
    IReadOnlyList<ItemView> SoldItems,
    int CoinsGained
);

public sealed record StashGroupView(
    string CatalogId,
    string Name,
    int UnitValue,
    int Quantity,
    int TotalValue,
    string Flavor,
    IReadOnlyList<string> InstanceIds
);

public sealed record HaulPayoffView(
    string SettlementId,
    int GrossSecuredCount,
    int GrossSecuredValue,
    IReadOnlyList<ItemView> SecuredItems,
    int UnitsRetainedInStash,
    int UnitsAutoSold,
    IReadOnlyList<ItemView> AutoSoldItems,
    int OverflowCoinsEarned,
    int NetStashValueChange,
    int StashUnitsAfter,
    int StashValueAfter,
    int CoinsBalanceAfter
);

public sealed record SellStashItemsRequest(
    string CommandId,
    IReadOnlyList<string>? InstanceIds = null,
    string? CatalogId = null,
    int? Quantity = null
);

public sealed record SellStashResponse(
    string CommandId,
    int ItemsSoldCount,
    int CoinsGained,
    int NewCoinsBalance,
    int NewStashCount,
    int NewStashValue,
    IReadOnlyList<ItemView> SoldItems
);
