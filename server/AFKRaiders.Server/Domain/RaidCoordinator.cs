using System.Security.Cryptography;
using System.Text;
using AFKRaiders.Server.AI;

namespace AFKRaiders.Server.Domain;

public sealed class RaidCoordinator
{
    private static readonly TimeSpan AdmissionPulse = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan HomeInterval = TimeSpan.FromSeconds(60);
    private readonly TimeProvider time;
    private readonly IRaidNarrator narrator;
    private readonly INarrativeGenerator narrativeGenerator;
    private readonly object gate = new();
    private readonly Dictionary<string, RaidState> raids = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PlayerPrincipal> credentials = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HandlerState> handlers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> handlerCredentials = new(StringComparer.Ordinal);
    private readonly ILootCatalog catalog;
    private readonly bool fastPacing;
    private readonly Func<int> nextSeed;
    private DateTimeOffset nextAdmissionAt;
    private string? activeAutonomousRaidId;

    public RaidCoordinator(TimeProvider time, IRaidNarrator? narrator = null, ILootCatalog? catalog = null, INarrativeGenerator? narrativeGenerator = null, bool fastPacing = false, Func<int>? nextSeed = null)
    {
        this.time = time;
        this.narrator = narrator ?? new TemplateRaidNarrator();
        this.catalog = catalog ?? LootCatalog.Default;
        this.fastPacing = fastPacing;
        this.nextSeed = nextSeed ?? (() => RandomNumberGenerator.GetInt32(int.MaxValue));
        this.narrativeGenerator = narrativeGenerator ?? new TemplateNarrativeGenerator();
        nextAdmissionAt = time.GetUtcNow() + AdmissionPulse;
    }

    public EnrolledHandler EnrollHandler(string slot)
        => EnrollHandler(slot, CreateSecret(24));

    public EnrolledHandler EnrollHandler(string slot, string requestId)
    {
        ValidateSlot(slot);
        if (string.IsNullOrWhiteSpace(requestId) || requestId.Length > 100)
        {
            throw new RaidCommandException("InvalidEnrollmentRequest");
        }
        lock (gate)
        {
            if (handlers.TryGetValue(slot, out var existing))
            {
                if (!string.Equals(existing.EnrollmentRequestId, requestId, StringComparison.Ordinal))
                {
                    throw new RaidAccessException("HandlerAlreadyEnrolled");
                }
                return new EnrolledHandler(existing.AccessToken, ProjectHandler(existing, time.GetUtcNow()));
            }

            var accessToken = CreateSecret(32);
            var state = new HandlerState(slot, Guid.NewGuid().ToString("N"), requestId, accessToken);
            state.EquippedWeaponInstanceId = state.OwnedWeapons[0].InstanceId;
            state.LastSignalRegenAt = time.GetUtcNow();
            handlers.Add(slot, state);
            handlerCredentials.Add(Hash(accessToken), slot);
            var initMsg = $"{DisplayName(slot)} starts autonomous duty from Desperanza. Standard-issue {state.OwnedWeapons[0].Name} assigned.";
            state.Events.Add(new HandlerEventView(1, time.GetUtcNow(), "DutyEnabled", initMsg));
            AddJournalEntry(state, null, time.GetUtcNow(), "DutyEnabled", "Hub", initMsg, $"hub:enroll:{state.Slot}");
            TryStartAutonomousWorld(time.GetUtcNow());
            return new EnrolledHandler(accessToken, ProjectHandler(state, time.GetUtcNow()));
        }
    }

    public HandlerProjection GetHandlerProjection(string accessToken)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            return ProjectHandler(state, time.GetUtcNow());
        }
    }

    public HandlerProjection SetDuty(string accessToken, bool enabled)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.DutyEnabled != enabled)
            {
                state.DutyEnabled = enabled;
                var text = enabled
                    ? "Autonomous duty is enabled. The Raider will join an eligible raid without a Handler deployment."
                    : "Duty paused. The Raider will finish any current raid and skip future admissions.";
                var kind = enabled ? "DutyEnabled" : "DutyPaused";
                state.Events.Add(new HandlerEventView(state.Events.Count + 1, time.GetUtcNow(), kind, text));
                AddJournalEntry(state, null, time.GetUtcNow(), kind, "Hub", text, $"hub:duty:{state.Slot}:{state.Events.Count}");
            }
            return ProjectHandler(state, time.GetUtcNow());
        }
    }

    public CreatedRaid CreateRaid(string slot)
    {
        ValidateSlot(slot);
        lock (gate)
        {
            var now = time.GetUtcNow();
            var raidId = Guid.NewGuid().ToString("N");
            var inviteCode = CreateSecret(9);
            var playerId = Guid.NewGuid().ToString("N");
            var accessToken = CreateSecret(32);
            var simulation = new RaidSimulation(raidId, now, nextSeed(), narrator, fastPacing: fastPacing);
            simulation.AddRaider(playerId, slot, now);
            raids.Add(raidId, new RaidState(Hash(inviteCode), simulation));
            credentials.Add(Hash(accessToken), new PlayerPrincipal(raidId, playerId));
            return new CreatedRaid(raidId, inviteCode, playerId, accessToken, simulation.Project(playerId, now));
        }
    }

    public JoinedRaid JoinRaid(string inviteCode, string slot)
    {
        ValidateSlot(slot);
        if (string.IsNullOrWhiteSpace(inviteCode))
        {
            throw new RaidAccessException("InvalidInvite");
        }
        lock (gate)
        {
            var inviteHash = Hash(inviteCode.Trim().ToUpperInvariant());
            var state = raids.Values.SingleOrDefault(raid => raid.InviteHash is not null
                && FixedTimeEquals(raid.InviteHash, inviteHash));
            if (state is null)
            {
                throw new RaidAccessException("InvalidInvite");
            }
            var now = time.GetUtcNow();
            var playerId = Guid.NewGuid().ToString("N");
            var accessToken = CreateSecret(32);
            state.Simulation.AddRaider(playerId, slot, now);
            credentials.Add(Hash(accessToken), new PlayerPrincipal(state.Simulation.RaidId, playerId));
            return new JoinedRaid(state.Simulation.RaidId, playerId, accessToken,
                state.Simulation.Project(playerId, now));
        }
    }

    public RaidProjection GetProjection(string raidId, string accessToken)
    {
        lock (gate)
        {
            var (state, playerId) = GetAuthorizedPlayer(raidId, accessToken);
            return state.Simulation.Project(playerId, time.GetUtcNow());
        }
    }

    public IReadOnlyList<RaidRevision> AdvanceRaids()
    {
        lock (gate)
        {
            var now = time.GetUtcNow();
            var revisions = new Dictionary<string, RaidRevision>(StringComparer.Ordinal);
            while (nextAdmissionAt <= now)
            {
                AdvanceWorlds(nextAdmissionAt, revisions);
                RefreshAutonomousMembership(nextAdmissionAt);
                TryCloseAutonomousWorld(nextAdmissionAt);
                AdmitEligibleRaiders(nextAdmissionAt, revisions);
                nextAdmissionAt += AdmissionPulse;
            }
            AdvanceWorlds(now, revisions);
            RefreshAutonomousMembership(now);
            TryCloseAutonomousWorld(now);
            return revisions.Values.OrderBy(revision => revision.RaidId, StringComparer.Ordinal).ToArray();
        }
    }

    public string GetRaidForConnection(string accessToken, string raidId)
    {
        lock (gate)
        {
            GetAuthorizedPlayer(raidId, accessToken);
            return raidId;
        }
    }

    private (RaidState State, string PlayerId) GetAuthorizedPlayer(string raidId, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(raidId) || string.IsNullOrWhiteSpace(accessToken))
        {
            throw new RaidAccessException("Unauthorized");
        }
        var tokenHash = Hash(accessToken);
        if (credentials.TryGetValue(tokenHash, out var principal)
            && string.Equals(principal.RaidId, raidId, StringComparison.Ordinal)
            && raids.TryGetValue(raidId, out var legacyState))
        {
            return (legacyState, principal.PlayerId);
        }
        if (handlerCredentials.TryGetValue(tokenHash, out var slot)
            && handlers.TryGetValue(slot, out var handler)
            && string.Equals(handler.CurrentRaidId, raidId, StringComparison.Ordinal)
            && raids.TryGetValue(raidId, out var autonomousState)
            && autonomousState.Simulation.Project(handler.PlayerId, time.GetUtcNow()).Players
                .Any(player => player.PlayerId == handler.PlayerId))
        {
            return (autonomousState, handler.PlayerId);
        }
        throw new RaidAccessException("Unauthorized");
    }

    private HandlerState GetAuthorizedHandler(string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken)
            || !handlerCredentials.TryGetValue(Hash(accessToken), out var slot)
            || !handlers.TryGetValue(slot, out var state))
        {
            throw new RaidAccessException("Unauthorized");
        }
        return state;
    }

    private void AdvanceWorlds(DateTimeOffset at, Dictionary<string, RaidRevision> revisions)
    {
        foreach (var raid in raids.Values.OrderBy(raid => raid.Simulation.RaidId, StringComparer.Ordinal))
        {
            if (raid.Simulation.Advance(at))
            {
                revisions[raid.Simulation.RaidId] =
                    new RaidRevision(raid.Simulation.RaidId, raid.Simulation.Revision);
            }
        }
    }

    private void AdmitEligibleRaiders(DateTimeOffset at, Dictionary<string, RaidRevision> revisions)
    {
        foreach (var handler in EligibleHandlers(at))
        {
            if (activeAutonomousRaidId is null)
            {
                CreateAutonomousWorld(at);
            }
            if (activeAutonomousRaidId is null) return;

            var world = raids[activeAutonomousRaidId];
            if (!world.Simulation.CanAdmit(at)) return;
            var participantCount = world.Simulation.ParticipantCount;
            if (participantCount >= RaidSimulation.MaxRaiders) return;
            AdmitToAutonomousWorld(handler, world, at);
            revisions[world.Simulation.RaidId] =
                new RaidRevision(world.Simulation.RaidId, world.Simulation.Revision);
        }
    }

    private void TryStartAutonomousWorld(DateTimeOffset at)
    {
        if (activeAutonomousRaidId is not null) return;
        var first = EligibleHandlers(at).FirstOrDefault();
        if (first is null) return;
        CreateAutonomousWorld(at);
        AdmitToAutonomousWorld(first, raids[activeAutonomousRaidId!], at);
    }

    private void CreateAutonomousWorld(DateTimeOffset at)
    {
        var raidId = Guid.NewGuid().ToString("N");
        var momentum = EligibleHandlers(at).Select(h => h.Personality?.Greed ?? 0).DefaultIfEmpty(0).Max();
        var simulation = new RaidSimulation(raidId, at, nextSeed(), narrator, catalog, fastPacing: fastPacing,
            conditionMomentum: momentum);
        raids.Add(raidId, new RaidState(null, simulation));
        activeAutonomousRaidId = raidId;
    }

    private void AdmitToAutonomousWorld(HandlerState handler, RaidState world, DateTimeOffset at)
    {
        var staged = handler.StagedSupplies.Select(kv => new StagedSupplyItem(
            kv.Key,
            Domain.ShopCatalog.TryGetSupply(kv.Key, out var def) && def is not null ? def.Name : kv.Key,
            def?.Category ?? "Medical",
            Math.Min(kv.Value, handler.SupplyStock.GetValueOrDefault(kv.Key, 0))
        )).Where(s => s.Quantity > 0).ToList();

        foreach (var item in staged)
        {
            handler.SupplyStock[item.CatalogId] = Math.Max(0, handler.SupplyStock.GetValueOrDefault(item.CatalogId, 0) - item.Quantity);
        }

        world.Simulation.AddAutonomousRaider(handler.PlayerId, handler.Slot, at, staged, handler.Personality, handler.VoiceState);
        handler.CurrentRaidId = world.Simulation.RaidId;
        handler.IsHome = false;
        handler.HomeAvailableAt = null;
        world.Simulation.Advance(at);
        var msg = $"{DisplayName(handler.Slot)} autonomously joins an ongoing raid from Desperanza.";
        handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, at, "RaidAdmission", msg));
        AddJournalEntry(handler, world.Simulation.RaidId, at, "RaidAdmission", "Hub", msg, $"hub:admit:{world.Simulation.RaidId}:{handler.Slot}");
    }

    private IEnumerable<HandlerState> EligibleHandlers(DateTimeOffset at) =>
        handlers.Values
            .Where(handler => handler.DutyEnabled && handler.IsHome && handler.CurrentRaidId is null
                && (!handler.HomeAvailableAt.HasValue || handler.HomeAvailableAt.Value <= at))
            .OrderBy(handler => handler.Slot, StringComparer.Ordinal);

    private void RefreshAutonomousMembership(DateTimeOffset at)
    {
        if (activeAutonomousRaidId is null || !raids.TryGetValue(activeAutonomousRaidId, out var world)) return;
        foreach (var handler in handlers.Values.Where(handler =>
            handler.CurrentRaidId == activeAutonomousRaidId && !handler.IsHome))
        {
            var projection = world.Simulation.Project(handler.PlayerId, at);
            SyncRaidEventsToHandlerJournal(handler, world.Simulation);
            var playerActivity = projection.Players.Single(player => player.PlayerId == handler.PlayerId).Activity;
            if (playerActivity != "Extracted" && playerActivity != "KnockedOut")
                continue;

            var isKnockedOut = playerActivity == "KnockedOut";
            handler.VoiceState = world.Simulation.VoiceState(handler.PlayerId);
            if (!fastPacing)
            {
                var personality = world.Simulation.Personality(handler.PlayerId);
                handler.Personality = isKnockedOut ? new(0, 0) : new((int)(personality.Greed * 0.65), Math.Max(0, personality.Mood));
            }
            var terminalEvent = projection.Events.Last(item => item.Kind == "Extracted" || item.Kind == "KnockedOut");
            var extractedAt = terminalEvent.At;
            handler.IsHome = true;
            handler.HomeAvailableAt = extractedAt + HomeInterval;
            handler.LastExtractedLoot = projection.MyExtractedLoot.ToArray();

            if (handler.SettledRaidIds.Add(world.Simulation.RaidId))
            {
                // Weapon durability wear upon raid completion
                if (handler.EquippedWeaponInstanceId is not null)
                {
                    var wIdx = handler.OwnedWeapons.FindIndex(w => w.InstanceId == handler.EquippedWeaponInstanceId);
                    if (wIdx >= 0)
                    {
                        var w = handler.OwnedWeapons[wIdx];
                        handler.OwnedWeapons[wIdx] = w with { CurrentDurability = Math.Max(0, w.CurrentDurability - 1) };
                    }
                }

                // Return unused staged supplies if extracted safely
                if (!isKnockedOut && projection.MyFieldSupplies is not null)
                {
                    foreach (var supply in projection.MyFieldSupplies)
                    {
                        handler.SupplyStock[supply.CatalogId] = handler.SupplyStock.GetValueOrDefault(supply.CatalogId, 0) + supply.Quantity;
                    }
                }
                foreach (var key in handler.StagedSupplies.Keys.ToArray())
                {
                    handler.StagedSupplies[key] = Math.Min(handler.StagedSupplies[key], handler.SupplyStock.GetValueOrDefault(key, 0));
                }

                var previousStashValue = handler.Stash.TotalValue;
                var transfer = handler.Stash.TransferHaul(handler.LastExtractedLoot);
                handler.Coins += transfer.OverflowCoinsGained;

                var payoff = new HaulPayoffView(
                    SettlementId: $"settlement:{world.Simulation.RaidId}:{handler.PlayerId}",
                    GrossSecuredCount: transfer.IncomingHaul.Count,
                    GrossSecuredValue: transfer.IncomingHaul.Sum(i => i.Value),
                    SecuredItems: transfer.IncomingHaul,
                    UnitsRetainedInStash: transfer.IncomingRetainedCount,
                    UnitsAutoSold: transfer.AutoSoldItems.Count,
                    AutoSoldItems: transfer.AutoSoldItems,
                    OverflowCoinsEarned: transfer.OverflowCoinsGained,
                    NetStashValueChange: handler.Stash.TotalValue - previousStashValue,
                    StashUnitsAfter: handler.Stash.Count,
                    StashValueAfter: handler.Stash.TotalValue,
                    CoinsBalanceAfter: handler.Coins
                );

                var totalValue = handler.LastExtractedLoot.Sum(item => item.Value);
                var highlights = new List<string>();
                var damageTaken = Math.Max(0, projection.MyMaxHealth - projection.MyHealth);
                var robotRes = projection.RobotEncounter?.Resolution;
                var playerView = projection.Players.FirstOrDefault(p => p.PlayerId == handler.PlayerId);
                var finalGreed = playerView?.Disposition == "Hoarder" ? 35 : 15;

                if (isKnockedOut)
                {
                    var cause = projection.MyCauseOfDefeat ?? "Combat trauma";
                    highlights.Add($"TERMINAL DEFEAT: {cause}");
                    var pocketItem = handler.LastExtractedLoot.FirstOrDefault();
                    if (pocketItem is not null)
                    {
                        highlights.Add($"Secret Hidden Pocket salvaged: {pocketItem.Name} (Val: {pocketItem.Value})");
                    }
                    else
                    {
                        highlights.Add($"All field salvage lost ({projection.MyLostHaulCount} items, {projection.MyLostHaulValue} value)");
                    }
                    if (transfer.AutoSoldItems.Count > 0)
                    {
                        highlights.Add($"Stash overflow: Auto-sold {transfer.AutoSoldItems.Count} items for {transfer.OverflowCoinsGained} coins");
                    }

                    var debrief = $"{DisplayName(handler.Slot)} was KNOCKED OUT in Sector 4 ({cause}). {(pocketItem is not null ? $"Secret Hidden Pocket preserved {pocketItem.Name}." : "All field salvage lost.")}";

                    var aarContext = new AarDebriefContext(
                        RaidId: world.Simulation.RaidId,
                        RaiderName: DisplayName(handler.Slot),
                        Outcome: "KnockedOut",
                        SecuredItemCount: handler.LastExtractedLoot.Count,
                        TotalValue: totalValue,
                        TopItem: pocketItem,
                        DamageTaken: damageTaken,
                        RobotEncounterResolution: robotRes,
                        FinalGreed: finalGreed,
                        JournalHighlights: highlights,
                        CauseOfDefeat: cause,
                        PocketSalvagedItem: pocketItem,
                        LostHaulValue: projection.MyLostHaulValue
                    );

                    var aarDebrief = narrativeGenerator.GenerateAarDebrief(aarContext);

                    handler.LastReturnSummary = new ReturnSummaryView(
                        world.Simulation.RaidId,
                        extractedAt,
                        handler.LastExtractedLoot.Count,
                        totalValue,
                        handler.LastExtractedLoot,
                        debrief,
                        highlights,
                        payoff,
                        aarDebrief,
                        null,
                        "KnockedOut",
                        cause,
                        projection.MyLostHaulValue,
                        pocketItem
                    );

                    var raidIdForCallback = world.Simulation.RaidId;
                    _ = narrator.EnqueueEnrichmentAsync(new NarrativeEnrichmentRequest(
                        RaidId: raidIdForCallback,
                        RaiderId: handler.PlayerId,
                        EventSequence: handler.Journal.Count,
                        EventType: "AarDebrief",
                        EventPayload: aarContext,
                        OnGenerated: text => ApplyAarResult(handler, raidIdForCallback, new(text, "ArchivalLog"))
                    ) with { OnResult = result => ApplyAarResult(handler, raidIdForCallback, result) });

                    var msg = $"{DisplayName(handler.Slot)} chassis recovered by medbay drones. Cause: {cause}. Secured salvage: {(pocketItem is not null ? pocketItem.Name : "None")}.";
                    handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, extractedAt, "KnockedOutReturn", msg));
                    AddJournalEntry(handler, world.Simulation.RaidId, extractedAt, "KnockedOutReturn", "Hub", msg, $"hub:ko:{world.Simulation.RaidId}:{handler.Slot}");
                }
                else
                {
                    foreach (var item in handler.LastExtractedLoot)
                    {
                        highlights.Add($"Secured {item.Name} (Val: {item.Value})");
                    }
                    if (projection.RobotEncounter is not null)
                    {
                        highlights.Add($"Robot Encounter: {projection.RobotEncounter.Resolution} ({projection.RobotEncounter.Location})");
                    }
                    if (transfer.AutoSoldItems.Count > 0)
                    {
                        highlights.Add($"Stash overflow: Auto-sold {transfer.AutoSoldItems.Count} items for {transfer.OverflowCoinsGained} coins");
                    }
                    var debrief = $"{DisplayName(handler.Slot)} returned to Desperanza with {handler.LastExtractedLoot.Count} secured items (Val: {totalValue}). Paperwork awaiting Handler stamp.";
                    
                    var topItem = handler.LastExtractedLoot.OrderByDescending(i => i.Value).FirstOrDefault();
                    ItemAppraisalView? topAppraisal = null;
                    if (topItem is not null)
                    {
                        var rarity = catalog.TryGetItem(topItem.CatalogId, out var catItem) && catItem is not null ? catItem.Rarity : 1;
                        topAppraisal = narrativeGenerator.GenerateItemAppraisal(new ItemAppraisalContext(
                            CatalogId: topItem.CatalogId,
                            ItemName: topItem.Name,
                            ItemValue: topItem.Value,
                            Rarity: rarity,
                            Category: "Scrap",
                            ItemFlavor: topItem.Flavor,
                            RaiderName: DisplayName(handler.Slot)
                        ));
                        highlights.Add($"Appraisal: {topAppraisal.StrategicClassification} - \"{topAppraisal.Appraisal}\"");
                    }

                    var aarContext = new AarDebriefContext(
                        RaidId: world.Simulation.RaidId,
                        RaiderName: DisplayName(handler.Slot),
                        Outcome: "Extracted",
                        SecuredItemCount: handler.LastExtractedLoot.Count,
                        TotalValue: totalValue,
                        TopItem: topItem,
                        DamageTaken: damageTaken,
                        RobotEncounterResolution: robotRes,
                        FinalGreed: finalGreed,
                        JournalHighlights: highlights
                    );

                    var aarDebrief = narrativeGenerator.GenerateAarDebrief(aarContext);

                    handler.LastReturnSummary = new ReturnSummaryView(
                        world.Simulation.RaidId,
                        extractedAt,
                        handler.LastExtractedLoot.Count,
                        totalValue,
                        handler.LastExtractedLoot,
                        debrief,
                        highlights,
                        payoff,
                        aarDebrief,
                        topAppraisal,
                        "Extracted"
                    );

                    var raidIdForCallback = world.Simulation.RaidId;
                    _ = narrator.EnqueueEnrichmentAsync(new NarrativeEnrichmentRequest(
                        RaidId: raidIdForCallback,
                        RaiderId: handler.PlayerId,
                        EventSequence: handler.Journal.Count,
                        EventType: "AarDebrief",
                        EventPayload: aarContext,
                        OnGenerated: text => ApplyAarResult(handler, raidIdForCallback, new(text, "ArchivalLog"))
                    ) with { OnResult = result => ApplyAarResult(handler, raidIdForCallback, result) });
                    var msg = $"{DisplayName(handler.Slot)} returns to Desperanza with {handler.LastExtractedLoot.Count} secured items.";
                    handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, extractedAt, "ReturnedHome", msg));
                    AddJournalEntry(handler, world.Simulation.RaidId, extractedAt, "ReturnedHome", "Hub", msg, $"hub:returned:{world.Simulation.RaidId}:{handler.Slot}");
                }

                if (transfer.AutoSoldItems.Count > 0)
                {
                    var overflowMsg = $"Home Stash exceeded {HomeStash.MaxCapacity}-unit capacity. Auto-sold {transfer.AutoSoldItems.Count} lowest-value items for {transfer.OverflowCoinsGained} spendable coins.";
                    handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, extractedAt, "StashOverflow", overflowMsg));
                    AddJournalEntry(handler, world.Simulation.RaidId, extractedAt, "StashOverflow", "Hub", overflowMsg, $"hub:overflow:{world.Simulation.RaidId}:{handler.Slot}");
                }
            }
        }
    }

    private void TryCloseAutonomousWorld(DateTimeOffset at)
    {
        if (activeAutonomousRaidId is null || !raids.TryGetValue(activeAutonomousRaidId, out var world)
            || !world.Simulation.Completed) return;
        var closedRaidId = activeAutonomousRaidId;
        foreach (var handler in handlers.Values.Where(handler => handler.CurrentRaidId == closedRaidId))
        {
            SyncRaidEventsToHandlerJournal(handler, world.Simulation);
            handler.CurrentRaidId = null;
            var msg = "The ongoing raid closes after its last Raider returns to Desperanza.";
            handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, at, "RaidWorldClosed", msg));
            AddJournalEntry(handler, closedRaidId, at, "RaidWorldClosed", "Hub", msg, $"hub:closed:{closedRaidId}:{handler.Slot}");
        }
        raids.Remove(closedRaidId);
        activeAutonomousRaidId = null;
    }

    private HandlerProjection ProjectHandler(HandlerState handler, DateTimeOffset at)
    {
        RaidProjection? raidProjection = null;
        if (handler.CurrentRaidId is { } raidId && raids.TryGetValue(raidId, out var world))
        {
            SyncRaidEventsToHandlerJournal(handler, world.Simulation);
            raidProjection = world.Simulation.Project(handler.PlayerId, at);
        }
        var status = !handler.IsHome ? "In Raid"
            : handler.CurrentRaidId is not null ? "Returned to Desperanza"
            : handler.HomeAvailableAt > at ? "At Home / Cooling Down"
            : handler.DutyEnabled ? "At Home / On Duty" : "At Home / Paused";

        var equippedWeapon = handler.OwnedWeapons.FirstOrDefault(w => w.InstanceId == handler.EquippedWeaponInstanceId)
                             ?? handler.OwnedWeapons.FirstOrDefault();

        var supplyList = Domain.ShopCatalog.Supplies.Select(s => new OwnedSupplyStack(
            CatalogId: s.Id,
            Name: s.Name,
            Category: s.Category,
            Quantity: handler.SupplyStock.GetValueOrDefault(s.Id, 0),
            CostPerUnit: s.Value,
            HealAmount: s.HealAmount,
            ReviveAmount: s.ReviveAmount,
            ChargeAmount: s.ChargeAmount,
            Rarity: s.Rarity,
            Flavor: s.Flavor
        )).Where(s => s.Quantity > 0).ToArray();

        var stagedList = handler.StagedSupplies.Select(kv =>
        {
            var def = Domain.ShopCatalog.Supplies.FirstOrDefault(s => s.Id.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));
            return new StagedSupplyItem(
                CatalogId: kv.Key,
                Name: def?.Name ?? kv.Key,
                Category: def?.Category ?? "Consumable",
                Quantity: kv.Value
            );
        }).Where(s => s.Quantity > 0).ToArray();

        var shopCatalogView = new ShopCatalogView(
            Weapons: Domain.ShopCatalog.Weapons,
            Supplies: Domain.ShopCatalog.Supplies
        );

        RegenerateSignal(handler, at);

        return new HandlerProjection(handler.Slot, handler.PlayerId, DisplayName(handler.Slot), handler.DutyEnabled,
            status, handler.HomeAvailableAt, handler.CurrentRaidId, raidProjection,
            handler.LastExtractedLoot, handler.Events.ToArray(), at,
            handler.LastReturnSummary, handler.Journal.ToArray(),
            handler.Coins, handler.Stash.TotalValue, handler.Stash.Count, HomeStash.MaxCapacity,
            handler.Stash.Items, handler.Stash.GetGroups(),
            equippedWeapon, handler.OwnedWeapons.ToArray(), supplyList, stagedList, shopCatalogView,
            handler.SignalCurrent, handler.SignalMax, raidProjection?.MyHiddenPocketInstanceId);
    }

    public PurchaseShopItemResponse PurchaseShopItem(string accessToken, PurchaseShopItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }
        if (request.Quantity <= 0 || request.Quantity > 99)
        {
            throw new RaidCommandException("InvalidQuantity");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.ProcessedShopCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            int totalCost;
            List<OwnedWeapon>? newWeapons = null;
            List<OwnedSupplyStack>? updatedSupplies = null;
            string purchaseSummary;

            if (Domain.ShopCatalog.TryGetWeapon(request.CatalogId, out var weaponDef) && weaponDef is not null)
            {
                totalCost = weaponDef.Value * request.Quantity;
                if (state.Coins < totalCost)
                {
                    throw new RaidCommandException("InsufficientCoins");
                }

                state.Coins -= totalCost;
                newWeapons = new List<OwnedWeapon>();
                for (var i = 0; i < request.Quantity; i++)
                {
                    var instance = Domain.ShopCatalog.CreateWeaponInstance(weaponDef);
                    state.OwnedWeapons.Add(instance);
                    newWeapons.Add(instance);
                }

                if (string.IsNullOrWhiteSpace(state.EquippedWeaponInstanceId) && newWeapons.Count > 0)
                {
                    state.EquippedWeaponInstanceId = newWeapons[0].InstanceId;
                }

                purchaseSummary = $"Purchased {request.Quantity}x {weaponDef.Name} for {totalCost} coins.";
            }
            else if (Domain.ShopCatalog.TryGetSupply(request.CatalogId, out var supplyDef) && supplyDef is not null)
            {
                totalCost = supplyDef.Value * request.Quantity;
                if (state.Coins < totalCost)
                {
                    throw new RaidCommandException("InsufficientCoins");
                }

                state.Coins -= totalCost;
                state.SupplyStock[supplyDef.Id] = state.SupplyStock.GetValueOrDefault(supplyDef.Id, 0) + request.Quantity;
                updatedSupplies = [new OwnedSupplyStack(
                    CatalogId: supplyDef.Id,
                    Name: supplyDef.Name,
                    Category: supplyDef.Category,
                    Quantity: state.SupplyStock[supplyDef.Id],
                    CostPerUnit: supplyDef.Value,
                    HealAmount: supplyDef.HealAmount,
                    ReviveAmount: supplyDef.ReviveAmount,
                    ChargeAmount: supplyDef.ChargeAmount,
                    Rarity: supplyDef.Rarity,
                    Flavor: supplyDef.Flavor
                )];

                purchaseSummary = $"Purchased {request.Quantity}x {supplyDef.Name} for {totalCost} coins into prep stock.";
            }
            else
            {
                throw new RaidCommandException("InvalidCatalogItem");
            }

            var response = new PurchaseShopItemResponse(
                CommandId: request.CommandId,
                CatalogId: request.CatalogId,
                Quantity: request.Quantity,
                TotalCost: totalCost,
                RemainingCoins: state.Coins,
                NewWeapons: newWeapons,
                UpdatedSupplies: updatedSupplies
            );

            state.ProcessedShopCommands[request.CommandId] = response;

            var now = time.GetUtcNow();
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "ShopPurchase", purchaseSummary));
            AddJournalEntry(state, null, now, "ShopPurchase", "Hub", purchaseSummary, $"hub:shop:{request.CommandId}");

            return response;
        }
    }

    public EquipWeaponResponse EquipWeapon(string accessToken, EquipWeaponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.ProcessedEquipCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            var target = state.OwnedWeapons.FirstOrDefault(w => w.InstanceId == request.WeaponInstanceId);
            if (target is null)
            {
                throw new RaidCommandException("WeaponNotOwned");
            }

            if (target.CurrentDurability <= 0)
            {
                throw new RaidCommandException("BrokenWeaponCannotBeEquipped");
            }

            state.EquippedWeaponInstanceId = target.InstanceId;

            var response = new EquipWeaponResponse(
                CommandId: request.CommandId,
                WeaponInstanceId: target.InstanceId,
                EquippedWeapon: target
            );

            state.ProcessedEquipCommands[request.CommandId] = response;

            var now = time.GetUtcNow();
            var msg = $"Equipped {target.Name} (Durability: {target.CurrentDurability}/{target.MaxDurability}) for field operations.";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "WeaponEquipped", msg));
            AddJournalEntry(state, null, now, "WeaponEquipped", "Hub", msg, $"hub:equip:{request.CommandId}");

            return response;
        }
    }

    public RepairWeaponResponse RepairWeapon(string accessToken, RepairWeaponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.ProcessedRepairCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            var index = state.OwnedWeapons.FindIndex(w => w.InstanceId == request.WeaponInstanceId);
            if (index < 0)
            {
                throw new RaidCommandException("WeaponNotOwned");
            }

            var weapon = state.OwnedWeapons[index];
            if (weapon.CurrentDurability >= weapon.MaxDurability)
            {
                throw new RaidCommandException("WeaponAlreadyAtMaxDurability");
            }

            if (state.Coins < weapon.RepairCost)
            {
                throw new RaidCommandException("InsufficientCoins");
            }

            state.Coins -= weapon.RepairCost;
            var repaired = weapon with { CurrentDurability = weapon.MaxDurability };
            state.OwnedWeapons[index] = repaired;

            var response = new RepairWeaponResponse(
                CommandId: request.CommandId,
                WeaponInstanceId: repaired.InstanceId,
                RepairCostPaid: weapon.RepairCost,
                RemainingCoins: state.Coins,
                RepairedWeapon: repaired
            );

            state.ProcessedRepairCommands[request.CommandId] = response;

            var now = time.GetUtcNow();
            var msg = $"Repaired {repaired.Name} to full durability ({repaired.MaxDurability}/{repaired.MaxDurability}) for {weapon.RepairCost} coins.";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "WeaponRepaired", msg));
            AddJournalEntry(state, null, now, "WeaponRepaired", "Hub", msg, $"hub:repair:{request.CommandId}");

            return response;
        }
    }

    internal void DamageWeaponForTesting(string accessToken, string instanceId, int damage)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            var index = state.OwnedWeapons.FindIndex(w => w.InstanceId == instanceId);
            if (index >= 0)
            {
                var current = state.OwnedWeapons[index];
                state.OwnedWeapons[index] = current with { CurrentDurability = Math.Max(0, current.CurrentDurability - damage) };
            }
        }
    }

    internal void ApplyWeaponDamage(string accessToken, string instanceId, int damage) =>
        DamageWeaponForTesting(accessToken, instanceId, damage);

    public StageLoadoutResponse StageLoadout(string accessToken, StageLoadoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.ProcessedStageCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            var stagedItems = new List<StagedSupplyItem>();
            var medCount = 0;
            var rechCount = 0;

            foreach (var item in request.StagedSupplies)
            {
                if (item.Quantity <= 0) continue;
                if (!Domain.ShopCatalog.TryGetSupply(item.CatalogId, out var def) || def is null)
                {
                    throw new RaidCommandException($"InvalidSupplyItem:{item.CatalogId}");
                }

                var available = state.SupplyStock.GetValueOrDefault(item.CatalogId, 0);
                if (available < item.Quantity)
                {
                    throw new RaidCommandException($"InsufficientStock:{item.CatalogId}");
                }

                if (string.Equals(def.Category, "Medical", StringComparison.OrdinalIgnoreCase))
                {
                    medCount += item.Quantity;
                }
                else if (string.Equals(def.Category, "ShieldRecharger", StringComparison.OrdinalIgnoreCase))
                {
                    rechCount += item.Quantity;
                }

                stagedItems.Add(new StagedSupplyItem(
                    CatalogId: def.Id,
                    Name: def.Name,
                    Category: def.Category,
                    Quantity: item.Quantity
                ));
            }

            if (medCount > Domain.ShopCatalog.MaxStagedMedicalCount)
            {
                throw new RaidCommandException("ExceededMedicalStageLimit");
            }
            if (rechCount > Domain.ShopCatalog.MaxStagedRechargerCount)
            {
                throw new RaidCommandException("ExceededRechargerStageLimit");
            }

            state.StagedSupplies.Clear();
            foreach (var staged in stagedItems)
            {
                state.StagedSupplies[staged.CatalogId] = staged.Quantity;
            }

            var response = new StageLoadoutResponse(
                CommandId: request.CommandId,
                StagedSupplies: stagedItems
            );

            state.ProcessedStageCommands[request.CommandId] = response;

            var now = time.GetUtcNow();
            var totalCount = medCount + rechCount;
            var msg = $"Staged field loadout with {totalCount} consumables ({medCount} Medical, {rechCount} Rechargers) for next raid (Active in G5).";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "LoadoutStaged", msg));
            AddJournalEntry(state, null, now, "LoadoutStaged", "Hub", msg, $"hub:stage:{request.CommandId}");

            return response;
        }
    }

    public ItemAppraisalView AppraiseItem(string accessToken, string catalogId, string? itemName = null)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);

            string name = itemName ?? catalogId;
            int value = 10;
            int rarity = 1;
            string category = "Scrap";
            string flavor = "Recovered from the surface ruins.";

            if (catalog.TryGetItem(catalogId, out var lootItem) && lootItem is not null)
            {
                name = lootItem.Name;
                value = lootItem.Value;
                rarity = lootItem.Rarity;
                category = lootItem.Category;
                flavor = lootItem.Flavor;
            }
            else if (Domain.ShopCatalog.TryGetWeapon(catalogId, out var weaponDef) && weaponDef is not null)
            {
                name = weaponDef.Name;
                value = weaponDef.Value;
                rarity = weaponDef.Rarity;
                category = "Weapon";
                flavor = weaponDef.Flavor;
            }
            else if (Domain.ShopCatalog.TryGetSupply(catalogId, out var supplyDef) && supplyDef is not null)
            {
                name = supplyDef.Name;
                value = supplyDef.Value;
                rarity = supplyDef.Rarity;
                category = supplyDef.Category;
                flavor = supplyDef.Flavor;
            }

            var context = new ItemAppraisalContext(
                CatalogId: catalogId,
                ItemName: name,
                ItemValue: value,
                Rarity: rarity,
                Category: category,
                ItemFlavor: flavor,
                RaiderName: DisplayName(state.Slot)
            );

            var appraisal = narrativeGenerator.GenerateItemAppraisal(context);

            var now = time.GetUtcNow();
            var logText = $"Appraisal for {name}: {appraisal.StrategicClassification} — \"{appraisal.Appraisal}\"";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "ItemAppraisal", logText));
            AddJournalEntry(state, null, now, "ItemAppraisal", "Hub", logText, $"hub:appraise:{catalogId}:{DateTimeOffset.UtcNow.Ticks}");

            return appraisal;
        }
    }

    public SellStashResponse SellStash(string accessToken, SellStashItemsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);

            if (state.ProcessedSellCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            SellStashResult result;
            if (request.InstanceIds is { Count: > 0 })
            {
                result = state.Stash.SellByInstanceIds(request.InstanceIds);
            }
            else if (!string.IsNullOrWhiteSpace(request.CatalogId) && request.Quantity.HasValue)
            {
                result = state.Stash.SellByCatalogId(request.CatalogId, request.Quantity.Value);
            }
            else
            {
                throw new RaidCommandException("InvalidSellRequest");
            }

            state.Coins += result.CoinsGained;
            var response = new SellStashResponse(
                request.CommandId,
                result.SoldItems.Count,
                result.CoinsGained,
                state.Coins,
                state.Stash.Count,
                state.Stash.TotalValue,
                result.SoldItems
            );

            state.ProcessedSellCommands[request.CommandId] = response;

            var now = time.GetUtcNow();
            var msg = $"Sold {result.SoldItems.Count} items from Home Stash for {result.CoinsGained} coins.";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "StashSale", msg));
            AddJournalEntry(state, null, now, "StashSale", "Hub", msg, $"hub:sell:{request.CommandId}");

            return response;
        }
    }

    public PagedCommsResponse GetHandlerComms(string accessToken, int sinceSequence = 0, int limit = 50)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (state.CurrentRaidId is { } raidId && raids.TryGetValue(raidId, out var world))
            {
                SyncRaidEventsToHandlerJournal(state, world.Simulation);
            }
            var entries = state.Journal.Where(e => e.Sequence > sinceSequence)
                .OrderBy(e => e.Sequence)
                .Take(Math.Clamp(limit, 1, 200))
                .ToArray();
            var total = state.Journal.Count;
            var nextSeq = entries.Length > 0 ? entries.Max(e => e.Sequence) : sinceSequence;
            var hasMore = state.Journal.Any(e => e.Sequence > nextSeq);
            return new PagedCommsResponse(entries, total, nextSeq, hasMore);
        }
    }

    private void RegenerateSignal(HandlerState handler, DateTimeOffset now)
    {
        if (handler.LastSignalRegenAt == default)
        {
            handler.LastSignalRegenAt = now;
            return;
        }

        var elapsedSeconds = (now - handler.LastSignalRegenAt).TotalSeconds;
        var pointsToAdd = (int)(elapsedSeconds / 30);
        if (pointsToAdd > 0)
        {
            handler.SignalCurrent = Math.Min(handler.SignalMax, handler.SignalCurrent + pointsToAdd);
            handler.LastSignalRegenAt = handler.LastSignalRegenAt.AddSeconds(pointsToAdd * 30);
        }
    }

    internal void SetSignalForTesting(string accessToken, int signal)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            state.SignalCurrent = Math.Clamp(signal, 0, state.SignalMax);
            state.LastSignalRegenAt = time.GetUtcNow();
        }
    }

    public HandlerSupportActionResponse ExecuteSupportAction(string accessToken, HandlerSupportActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        if (request.ActionType is not ("CallExtract" or "Calm" or "Pressure"))
        {
            throw new RaidCommandException("InvalidActionType");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            var now = time.GetUtcNow();
            RegenerateSignal(state, now);

            if (state.ProcessedSupportCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            if (state.CurrentRaidId is null || !raids.TryGetValue(state.CurrentRaidId, out var world))
            {
                throw new RaidCommandException("RaiderNotInRaid");
            }

            var cost = request.ActionType switch
            {
                "CallExtract" => 3,
                "Calm" => 1,
                "Pressure" => 1,
                _ => 1
            };

            if (state.SignalCurrent < cost)
            {
                throw new RaidCommandException("InsufficientSignal");
            }

            var queued = world.Simulation.QueueHandlerAction(request.CommandId, request.ActionType, state.PlayerId, now);
            if (!queued)
            {
                throw new RaidCommandException("HandlerActionPending");
            }

            state.SignalCurrent -= cost;
            world.Simulation.Advance(now);

            var msg = request.ActionType switch
            {
                "CallExtract" => "Extraction order accepted (3 Signal spent). It bypasses natural checks when consumed; travel and extraction still take time.",
                "Calm" => "Calm accepted (1 Signal spent). Greed -12, Mood +3: stronger exit willingness and positive-mood resilience.",
                "Pressure" => "Pressure accepted (1 Signal spent). Greed +10, Mood -2: stronger valuable-item appetite, weaker exit willingness and resilience.",
                _ => "Support action executed."
            };

            var response = new HandlerSupportActionResponse(
                CommandId: request.CommandId,
                ActionType: request.ActionType,
                Success: true,
                RemainingSignal: state.SignalCurrent,
                Message: msg
            );

            state.ProcessedSupportCommands[request.CommandId] = response;
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "HandlerSupportAction", msg));
            AddJournalEntry(state, world.Simulation.RaidId, now, "HandlerSupportAction", "Support", msg, $"hub:support:{request.CommandId}");

            return response;
        }
    }

    public DesignateHiddenPocketResponse DesignateHiddenPocket(string accessToken, DesignateHiddenPocketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
        {
            throw new RaidCommandException("InvalidCommandId");
        }

        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            var now = time.GetUtcNow();

            if (state.ProcessedPocketCommands.TryGetValue(request.CommandId, out var existing))
            {
                return existing;
            }

            if (state.CurrentRaidId is null || !raids.TryGetValue(state.CurrentRaidId, out var world))
            {
                throw new RaidCommandException("RaiderNotInRaid");
            }

            var success = world.Simulation.DesignateHiddenPocket(state.PlayerId, request.ItemInstanceId);
            if (!success && !string.IsNullOrEmpty(request.ItemInstanceId))
            {
                throw new RaidCommandException("ItemNotFoundInBackpack");
            }

            world.Simulation.Advance(now);

            var msg = request.ItemInstanceId is not null
                ? $"Item designated into Secret Hidden Pocket for emergency salvage."
                : "Secret Hidden Pocket designation cleared.";

            var response = new DesignateHiddenPocketResponse(
                CommandId: request.CommandId,
                ItemInstanceId: request.ItemInstanceId,
                Success: true,
                Message: msg
            );

            state.ProcessedPocketCommands[request.CommandId] = response;
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "HiddenPocketDesignated", msg));
            AddJournalEntry(state, world.Simulation.RaidId, now, "HiddenPocketDesignated", "Support", msg, $"hub:pocket:{request.CommandId}");

            return response;
        }
    }

    private void ApplyAarResult(HandlerState handler, string raidId, NarrativeResult result)
    {
        lock (gate)
        {
            if (handler.LastReturnSummary is not { } summary || summary.RaidId != raidId) return;
            var bullets = OnnxGenAiNarrativeGenerator.ParseBullets(result.Text);
            if (bullets.Count != 3) return;
            handler.LastReturnSummary = summary with
            {
                AarDebrief = new AarDebriefView(bullets, result.Source, time.GetUtcNow())
            };
        }
    }

    private void QueueLiveNarration(RaidSimulation simulation, RaidEventView evt)
    {
        if (!narrativeGenerator.IsAvailable || narrativeGenerator is TemplateNarrativeGenerator ||
            simulation.Completed || time.GetUtcNow() - evt.At > TimeSpan.FromSeconds(90) ||
            !Content.NarrationContent.Default.CanEnrich(evt.Kind) ||
            !simulation.TryQueueNarration(evt.Sequence)) return;
        bool Current()
        {
            lock (gate)
                return !simulation.Completed && time.GetUtcNow() - evt.At <= TimeSpan.FromSeconds(90);
        }
        void Apply(NarrativeResult result)
        {
            lock (gate)
            {
                string? rejectionReason = null;
                var flavor = result.Source == "Generated" ? OnnxGenAiNarrativeGenerator.ValidateAmbient(result.Text, out rejectionReason) : null;
                if (Current() && flavor is not null)
                    simulation.ApplyNarration(evt.Sequence, evt.Text, flavor);
                else simulation.FinishNarration(evt.Sequence, Current() ? "Fallback" : "Expired",
                    !Current() ? (simulation.Completed ? "WorldCompleted" : "StaleAfterGeneration") :
                    result.FailureReason ?? (rejectionReason is null ? "NoAcceptedAside" : $"RejectedAtDelivery:{rejectionReason}"));
                foreach (var state in handlers.Values)
                    if (simulation.HasRaider(state.PlayerId))
                        SyncRaidEventsToHandlerJournal(state, simulation);
            }
        }
        _ = narrator.EnqueueEnrichmentAsync(new(simulation.RaidId, evt.ParticipantIds[0], evt.Sequence, evt.Kind,
            new AmbientCommsContext(evt.Location ?? "Desperanza", evt.Activity ?? evt.Kind),
            text => Apply(new(text, "Generated")), Apply, Current));
    }

    private void SyncRaidEventsToHandlerJournal(HandlerState handler, RaidSimulation simulation)
    {
        foreach (var evt in simulation.Project(handler.PlayerId, time.GetUtcNow()).Events)
            QueueLiveNarration(simulation, evt);
        var raidProj = simulation.Project(handler.PlayerId, time.GetUtcNow());
        foreach (var evt in raidProj.Events)
        {
            var sourceId = $"raid:{simulation.RaidId}:{evt.Sequence}";
            AddJournalEntry(
                handler,
                simulation.RaidId,
                evt.At,
                evt.Kind,
                evt.Channel,
                evt.Text,
                sourceId,
                evt.Location,
                evt.Activity,
                evt.Destination,
                evt.DurationSeconds,
                evt.HealthDelta,
                evt.RobotDamageDealt,
                evt.RobotDamageTaken,
                evt.RobotRemainingHealth,
                evt.ItemName,
                evt.ItemValue,
                evt.ItemInstanceId,
                evt.OpportunityId,
                evt.Reason,
                evt.ThreadId,
                evt.FactsText,
                evt.Flavor,
                evt.NarrativeSource,
                evt.EnrichmentStatus,
                evt.EnrichmentReason
            );
        }
    }

    private static void AddJournalEntry(
        HandlerState handler,
        string? raidId,
        DateTimeOffset at,
        string kind,
        string channel,
        string text,
        string sourceEventId,
        string? location = null,
        string? activity = null,
        string? destination = null,
        int? durationSeconds = null,
        int? healthDelta = null,
        int? robotDamageDealt = null,
        int? robotDamageTaken = null,
        int? robotRemainingHealth = null,
        string? itemName = null,
        int? itemValue = null,
        string? itemInstanceId = null,
        string? opportunityId = null,
        string? reason = null,
        string? threadId = null,
        string? factsText = null,
        string? flavor = null,
        string narrativeSource = "CanonFallback",
        string enrichmentStatus = "NotRequested",
        string? enrichmentReason = null)
    {
        if (handler.JournalIndexes.TryGetValue(sourceEventId, out var index))
        {
            var previous = handler.Journal[index];
            if (previous.Text == text && previous.FactsText == factsText && previous.Flavor == flavor &&
                previous.NarrativeSource == narrativeSource && previous.EnrichmentStatus == enrichmentStatus &&
                previous.EnrichmentReason == enrichmentReason) return;
            handler.Journal[index] = previous with
            {
                Text = text, FactsText = factsText, Flavor = flavor,
                NarrativeSource = narrativeSource, EnrichmentStatus = enrichmentStatus, EnrichmentReason = enrichmentReason
            };
            return;
        }
        var entry = new HandlerJournalEntryView(
            handler.Journal.Count + 1,
            sourceEventId,
            raidId,
            at,
            kind,
            channel,
            text,
            location,
            activity,
            destination,
            durationSeconds,
            healthDelta,
            robotDamageDealt,
            robotDamageTaken,
            robotRemainingHealth,
            itemName,
            itemValue,
            itemInstanceId,
            opportunityId,
            reason,
            threadId,
            factsText,
            flavor,
            narrativeSource,
            enrichmentStatus,
            enrichmentReason
        );
        handler.JournalIndexes.Add(sourceEventId, handler.Journal.Count);
        handler.Journal.Add(entry);
    }

    private static string DisplayName(string slot) =>
        slot == "raider-one" ? "Mira \"Refund\" Malone" : "Dev \"Wet Socks\" Kowalski";

    private static void ValidateSlot(string slot)
    {
        if (slot is not ("raider-one" or "raider-two"))
        {
            throw new RaidCommandException("InvalidDevelopmentIdentity");
        }
    }

    private static string CreateSecret(int byteCount) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_').ToUpperInvariant();
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Convert.FromHexString(left), Convert.FromHexString(right));
    private sealed record RaidState(string? InviteHash, RaidSimulation Simulation);
    private sealed record PlayerPrincipal(string RaidId, string PlayerId);

    private sealed class HandlerState(string slot, string playerId, string enrollmentRequestId, string accessToken)
    {
        public string Slot { get; } = slot;
        public string PlayerId { get; } = playerId;
        public string EnrollmentRequestId { get; } = enrollmentRequestId;
        public string AccessToken { get; } = accessToken;
        public bool DutyEnabled { get; set; } = true;
        public bool IsHome { get; set; } = true;
        public DateTimeOffset? HomeAvailableAt { get; set; }
        public string? CurrentRaidId { get; set; }
        public IReadOnlyList<ItemView> LastExtractedLoot { get; set; } = [];
        public List<HandlerEventView> Events { get; } = [];
        public List<HandlerJournalEntryView> Journal { get; } = [];
        public Dictionary<string, int> JournalIndexes { get; } = new(StringComparer.Ordinal);
        public ReturnSummaryView? LastReturnSummary { get; set; }
        public RaiderPersonality? Personality { get; set; }
        public int Coins { get; set; }
        public HomeStash Stash { get; } = new();
        public HashSet<string> SettledRaidIds { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, SellStashResponse> ProcessedSellCommands { get; } = new(StringComparer.Ordinal);

        public List<OwnedWeapon> OwnedWeapons { get; } = [Domain.ShopCatalog.CreateWeaponInstance(Domain.ShopCatalog.GetStarterWeapon())];
        public string? EquippedWeaponInstanceId { get; set; }
        public Dictionary<string, int> SupplyStock { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> StagedSupplies { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, PurchaseShopItemResponse> ProcessedShopCommands { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, EquipWeaponResponse> ProcessedEquipCommands { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, RepairWeaponResponse> ProcessedRepairCommands { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, StageLoadoutResponse> ProcessedStageCommands { get; } = new(StringComparer.Ordinal);
        public int SignalCurrent { get; set; } = 3;
        public RaiderVoiceState? VoiceState { get; set; }
        public int SignalMax { get; set; } = 5;
        public DateTimeOffset LastSignalRegenAt { get; set; }
        public Dictionary<string, HandlerSupportActionResponse> ProcessedSupportCommands { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, DesignateHiddenPocketResponse> ProcessedPocketCommands { get; } = new(StringComparer.Ordinal);
    }
}

public sealed record RaidRevision(string RaidId, int Revision);
public sealed record CreatedRaid(string RaidId, string InviteCode, string PlayerId, string AccessToken, RaidProjection Projection);
public sealed record JoinedRaid(string RaidId, string PlayerId, string AccessToken, RaidProjection Projection);
public sealed record EnrolledHandler(string AccessToken, HandlerProjection Projection);
public sealed record HandlerProjection(
    string Slot,
    string PlayerId,
    string DisplayName,
    bool DutyEnabled,
    string Status,
    DateTimeOffset? HomeAvailableAt,
    string? CurrentRaidId,
    RaidProjection? Raid,
    IReadOnlyList<ItemView> MyLastExtractedLoot,
    IReadOnlyList<HandlerEventView> Comms,
    DateTimeOffset ServerTime,
    ReturnSummaryView? LastReturnSummary = null,
    IReadOnlyList<HandlerJournalEntryView>? FullJournal = null,
    int Coins = 0,
    int StashValue = 0,
    int StashCount = 0,
    int StashCapacity = HomeStash.MaxCapacity,
    IReadOnlyList<ItemView>? Stash = null,
    IReadOnlyList<StashGroupView>? StashGroups = null,
    OwnedWeapon? EquippedWeapon = null,
    IReadOnlyList<OwnedWeapon>? OwnedWeapons = null,
    IReadOnlyList<OwnedSupplyStack>? SupplyStock = null,
    IReadOnlyList<StagedSupplyItem>? StagedSupplies = null,
    ShopCatalogView? ShopCatalog = null,
    int SignalCurrent = 3,
    int SignalMax = 5,
    string? SecretHiddenPocketInstanceId = null);
public sealed record HandlerEventView(int Sequence, DateTimeOffset At, string Kind, string Text);

public sealed record ReturnSummaryView(
    string RaidId,
    DateTimeOffset ExtractedAt,
    int SecuredLootCount,
    int TotalValue,
    IReadOnlyList<ItemView> Items,
    string DebriefSummary,
    IReadOnlyList<string> Highlights,
    HaulPayoffView? Payoff = null,
    AarDebriefView? AarDebrief = null,
    ItemAppraisalView? TopItemAppraisal = null,
    string Outcome = "Extracted",
    string? CauseOfDefeat = null,
    int LostHaulValue = 0,
    ItemView? PocketSalvagedItem = null
);

public sealed record HandlerJournalEntryView(
    int Sequence,
    string SourceEventId,
    string? RaidId,
    DateTimeOffset At,
    string Kind,
    string Channel,
    string Text,
    string? Location = null,
    string? Activity = null,
    string? Destination = null,
    int? DurationSeconds = null,
    int? HealthDelta = null,
    int? RobotDamageDealt = null,
    int? RobotDamageTaken = null,
    int? RobotRemainingHealth = null,
    string? ItemName = null,
    int? ItemValue = null,
    string? ItemInstanceId = null,
    string? OpportunityId = null,
    string? Reason = null,
    string? ThreadId = null,
    string? FactsText = null,
    string? Flavor = null,
    string NarrativeSource = "CanonFallback",
    string EnrichmentStatus = "NotRequested",
    string? EnrichmentReason = null
);

public sealed record PagedCommsResponse(
    IReadOnlyList<HandlerJournalEntryView> Entries,
    int TotalCount,
    int NextSequence,
    bool HasMore
);

public sealed class RaidAccessException(string reason) : Exception(reason)
{
    public string Reason { get; } = reason;
}
public sealed class RaidCommandException(string reason) : Exception(reason)
{
    public string Reason { get; } = reason;
}
