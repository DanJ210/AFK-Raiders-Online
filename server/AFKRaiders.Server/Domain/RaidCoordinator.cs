using System.Security.Cryptography;
using System.Text;
using AFKRaiders.Server.AI;

namespace AFKRaiders.Server.Domain;

public sealed class RaidCoordinator
{
    private static readonly TimeSpan AdmissionPulse = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan HomeInterval = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan HubActivityInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ErrandRecovery = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ErrandStopDuration = TimeSpan.FromSeconds(45);
    private static readonly TimeSpan ErrandRestMargin = TimeSpan.FromMinutes(2);
    private static readonly string[] SaleDesks = ["Pawn Desk", "Coupon Barons", "OopsAll Claims Window"];
    private const int ReadyUpSignalCost = 2;
    private readonly TimeProvider time;
    private readonly IRaidNarrator narrator;
    private readonly INarrativeGenerator narrativeGenerator;
    private readonly object gate = new();
    private readonly Dictionary<string, RaidState> raids = new(StringComparer.Ordinal);
    private readonly Dictionary<string, PlayerPrincipal> credentials = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HandlerState> handlers = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> handlerCredentials = new(StringComparer.Ordinal);
    private readonly ILootCatalog catalog;
    private readonly TraderStockGenerator traderStockGenerator;
    private readonly Dictionary<long, TraderCycleStock> traderStocks = [];
    private readonly bool fastPacing;
    private bool committingPriorityPredecessors;
    private readonly Func<int> nextSeed;
    private DateTimeOffset nextAdmissionAt;
    private string? activeAutonomousRaidId;

    public RaidCoordinator(TimeProvider time, IRaidNarrator? narrator = null, ILootCatalog? catalog = null, INarrativeGenerator? narrativeGenerator = null, bool fastPacing = false, Func<int>? nextSeed = null, int? traderSeed = null)
    {
        this.time = time;
        this.narrator = narrator ?? new TemplateRaidNarrator();
        this.catalog = catalog ?? LootCatalog.Default;
        traderStockGenerator = new TraderStockGenerator(traderSeed ?? RandomNumberGenerator.GetInt32(int.MaxValue), time);
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
            var state = new HandlerState(slot, Guid.NewGuid().ToString("N"), requestId, accessToken)
            {
                TraitSeed = nextSeed()
            };
            state.EquippedWeaponInstanceId = state.OwnedWeapons[0].InstanceId;
            state.LastSignalRegenAt = time.GetUtcNow();
            BeginHomeStay(state, time.GetUtcNow(), initial: true);
            handlers.Add(slot, state);
            handlerCredentials.Add(Hash(accessToken), slot);
            var initMsg = $"{DisplayName(slot)} starts autonomous duty from Desperanza. Standard-issue {state.OwnedWeapons[0].Name} assigned.";
            state.Events.Add(new HandlerEventView(1, time.GetUtcNow(), "DutyEnabled", initMsg));
            AddJournalEntry(state, null, time.GetUtcNow(), "DutyEnabled", "Hub", initMsg, $"hub:enroll:{state.Slot}");
            PrepareForArrival(state);
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

    public HandlerProjection ReadyUp(string accessToken, ReadyUpRequest request)
    {
        lock (gate)
        {
            var state = GetAuthorizedHandler(accessToken);
            if (string.IsNullOrWhiteSpace(request.CommandId) || request.CommandId.Length > 100)
                throw new RaidCommandException("InvalidCommandId");
            if (state.ProcessedReadyCommands.Contains(request.CommandId))
                return ProjectHandler(state, time.GetUtcNow());

            AdvanceRaids();
            if (!state.IsHome)
                throw new RaidCommandException("RaiderNotAtHome");
            if (state.PreparationCompletedVisit != state.HomeVisit && state.PendingErrands.Count == 0)
                throw new RaidCommandException("PreparationIncomplete");

            var now = time.GetUtcNow();
            if (state.DutyEnabled && state.HomeAvailableAt <= now)
                throw new RaidCommandException("RaiderAlreadyQueued");
            RegenerateSignal(state, now);
            if (state.SignalCurrent < ReadyUpSignalCost)
                throw new RaidCommandException("InsufficientSignal");
            if (state.PreparationCompletedVisit != state.HomeVisit)
                HurryRemainingErrands(state, now);
            state.SignalCurrent -= ReadyUpSignalCost;
            state.DutyEnabled = true;
            state.HomeAvailableAt = now;
            state.HomeReadinessLogged = true;
            state.ProcessedReadyCommands.Add(request.CommandId);
            var message = $"{DisplayName(state.Slot)} readies up in Desperanza for {ReadyUpSignalCost} Signal. Queued for the next eligible admission; Hatch Authority has misplaced the waiting form.";
            state.Events.Add(new HandlerEventView(state.Events.Count + 1, now, "HandlerReadyUp", message));
            AddJournalEntry(state, null, now, "HandlerReadyUp", "Hub", message,
                $"hub:ready-up:{request.CommandId}", location: "Desperanza");
            return ProjectHandler(state, now);
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
                PrepareUncompletedHomeVisits();
                ProcessDueErrands(nextAdmissionAt);
                TryCloseAutonomousWorld(nextAdmissionAt);
                RecordHubActivities(nextAdmissionAt);
                RecordHomeReadiness(nextAdmissionAt);
                AdmitEligibleRaiders(nextAdmissionAt, revisions);
                nextAdmissionAt += AdmissionPulse;
            }
            AdvanceWorlds(now, revisions);
            RefreshAutonomousMembership(now);
            PrepareUncompletedHomeVisits();
            ProcessDueErrands(now);
            TryCloseAutonomousWorld(now);
            RecordHubActivities(now);
            RecordHomeReadiness(now);
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

        world.Simulation.AddAutonomousRaider(handler.PlayerId, handler.Slot, at, staged, handler.Personality,
            handler.VoiceState, handler.PrimaryTrait);
        handler.CurrentRaidId = world.Simulation.RaidId;
        handler.IsHome = false;
        handler.HomeAvailableAt = null;
        handler.NextHubActivityAt = null;
        handler.HubErrandLabel = null;
        handler.PendingErrands.Clear();
        world.Simulation.Advance(at);
        var msg = $"{DisplayName(handler.Slot)} autonomously joins an ongoing raid from Desperanza.";
        handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, at, "RaidAdmission", msg));
        AddJournalEntry(handler, world.Simulation.RaidId, at, "RaidAdmission", "Hub", msg, $"hub:admit:{world.Simulation.RaidId}:{handler.Slot}");
    }

    private IEnumerable<HandlerState> EligibleHandlers(DateTimeOffset at) =>
        handlers.Values
            .Where(handler => handler.DutyEnabled && handler.IsHome && handler.CurrentRaidId is null
                && handler.PreparationCompletedVisit == handler.HomeVisit
                && (!handler.HomeAvailableAt.HasValue || handler.HomeAvailableAt.Value <= at))
            .OrderBy(handler => handler.Slot, StringComparer.Ordinal);

    private void PrepareUncompletedHomeVisits()
    {
        foreach (var handler in handlers.Values
            .Where(handler => handler.IsHome && handler.PreparationCompletedVisit != handler.HomeVisit)
            .OrderBy(handler => handler.Slot, StringComparer.Ordinal))
        {
            PrepareForArrival(handler);
        }
    }

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
            BeginHomeStay(handler, extractedAt);
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
                var settledItems = FoundWeaponSettlement.Separate(
                    handler.LastExtractedLoot, handler.OwnedWeapons, catalog);
                handler.OwnedWeapons.AddRange(settledItems.WeaponsToKeep);
                var transfer = handler.Stash.TransferHaul(settledItems.StashItems);
                handler.Coins += transfer.OverflowCoinsGained;

                var payoff = new HaulPayoffView(
                    SettlementId: $"settlement:{world.Simulation.RaidId}:{handler.PlayerId}",
                    GrossSecuredCount: handler.LastExtractedLoot.Count,
                    GrossSecuredValue: handler.LastExtractedLoot.Sum(i => i.Value),
                    SecuredItems: handler.LastExtractedLoot,
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
                var finalGreed = playerView?.PrimaryTrait == "Hoarder" ? 35 : 15;

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

                PrepareForArrival(handler, paced: true);
                foreach (var weapon in settledItems.WeaponsToKeep)
                {
                    var text = Content.NarrationContent.Default.Format(
                        "hub.economy-found-weapon", weapon.Name);
                    RecordPreparationDecision(handler, extractedAt, "FoundWeapon",
                        $"found-weapon:{weapon.InstanceId}", text);
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

    private void PrepareForArrival(HandlerState handler, bool paced = false)
    {
        if (handler.PreparationCompletedVisit == handler.HomeVisit)
            return;

        if (handler.PreparationStartedVisit != handler.HomeVisit)
        {
            handler.PreparationStartedVisit = handler.HomeVisit;
            handler.LatestPreparationDecisions.Clear();
            handler.PendingErrands.Clear();
            handler.HubErrandLabel = null;
            ScheduleArrivalErrands(handler, paced && !fastPacing);
        }

        if (handler.PendingErrands.Count > 0 && !handler.PendingErrands[0].Paced)
            CommitDueErrands(handler, DateTimeOffset.MaxValue);
    }

    private RaiderPreparationPlan DecidePreparation(
        HandlerState handler,
        DateTimeOffset at,
        int? reserveFloorOverride = null,
        IReadOnlyDictionary<string, int>? availableStock = null)
    {
        var currentStaging = handler.StagedSupplies.Select(entry =>
        {
            var definition = Domain.ShopCatalog.Supplies.FirstOrDefault(supply =>
                supply.Id.Equals(entry.Key, StringComparison.OrdinalIgnoreCase));
            return new StagedSupplyItem(entry.Key, definition?.Name ?? entry.Key,
                definition?.Category ?? "Consumable", entry.Value);
        }).Where(item => item.Quantity > 0).ToArray();
        return RaiderEconomyPolicy.Decide(
            handler.PlayerId,
            handler.HomeVisit,
            handler.TraitSeed,
            at,
            handler.Coins,
            handler.Stash.Items,
            handler.OwnedWeapons,
            handler.EquippedWeaponInstanceId,
            handler.SupplyStock,
            currentStaging,
            catalog,
            Domain.ShopCatalog.Weapons,
            Domain.ShopCatalog.Supplies,
            availableStock ?? TraderStockAt(at).AvailableQuantities,
            reserveFloorOverride);
    }

    private TraderCycleStock TraderStockAt(DateTimeOffset at)
    {
        var cycleIndex = traderStockGenerator.CycleIndexAt(at);
        if (!traderStocks.TryGetValue(cycleIndex, out var stock))
        {
            stock = new TraderCycleStock(traderStockGenerator.Generate(
                at, Domain.ShopCatalog.Weapons, Domain.ShopCatalog.Supplies));
            traderStocks.Add(cycleIndex, stock);
        }
        return stock;
    }

    private static bool CanBuyWeapon(
        HandlerState handler,
        TraderCycleStock stock,
        string catalogId,
        int reserveFloor) =>
        Domain.ShopCatalog.TryGetWeapon(catalogId, out var definition)
        && definition is not null
        && definition.Value > 0
        && stock.Remaining(catalogId) > 0
        && handler.Coins >= definition.Value
        && handler.Coins - definition.Value >= reserveFloor;

    private static bool CanBuySupply(
        HandlerState handler,
        TraderCycleStock stock,
        string catalogId,
        int reserveFloor) =>
        Domain.ShopCatalog.TryGetSupply(catalogId, out var definition)
        && definition is not null
        && stock.Remaining(catalogId) > 0
        && handler.Coins >= definition.Value
        && handler.Coins - definition.Value >= reserveFloor;

    private static void EnsureTeaKettleFallback(HandlerState handler, DateTimeOffset at)
    {
        if (RaiderEconomyPolicy.BestUsableWeapon(handler.OwnedWeapons, Domain.ShopCatalog.Weapons) is not null)
            return;
        var fallbackId = RaiderEconomyPolicy.CreateWeaponInstanceId(
            handler.PlayerId, handler.HomeVisit, "tea_kettle", "fallback");
        if (!handler.OwnedWeapons.Any(weapon => weapon.InstanceId == fallbackId))
        {
            handler.OwnedWeapons.Add(Domain.ShopCatalog.CreateWeaponInstance(
                Domain.ShopCatalog.GetStarterWeapon(), fallbackId));
        }
        RecordPreparationDecision(handler, at, "PreparationWeaponFallback", "fallback",
            "Issued a free Tea Kettle at the Hatch Authority loaner desk because no usable owned weapon remained.");
    }

    private static void RecordUnavailablePurchase(
        HandlerState handler,
        DateTimeOffset at,
        string catalogId,
        bool soldOut,
        string? action = null)
    {
        var name = Domain.ShopCatalog.TryGetWeapon(catalogId, out var weapon) && weapon is not null
            ? weapon.Name
            : Domain.ShopCatalog.TryGetSupply(catalogId, out var supply) && supply is not null
                ? supply.Name
                : catalogId;
        if (soldOut)
        {
            var text = Content.NarrationContent.Default.Format("hub.economy-sold-out", name);
            RecordPreparationDecision(handler, at, "PreparationPurchaseUnavailable",
                action ?? $"unavailable:{catalogId}", text);
        }
    }

    private static void RecordSubstitutePurchase(
        HandlerState handler,
        DateTimeOffset at,
        string requestedCatalogId,
        string purchasedCatalogId,
        string desk)
    {
        var requestedName = CatalogItemName(requestedCatalogId);
        var purchasedName = CatalogItemName(purchasedCatalogId);
        var text = Content.NarrationContent.Default.Format(
            "hub.economy-substitute", requestedName, purchasedName, desk);
        RecordPreparationDecision(handler, at, "PreparationPurchaseSubstitute",
            $"substitute:{requestedCatalogId}:{purchasedCatalogId}", text);
    }

    private static string CatalogItemName(string catalogId) =>
        Domain.ShopCatalog.TryGetWeapon(catalogId, out var weapon) && weapon is not null
            ? weapon.Name
            : Domain.ShopCatalog.TryGetSupply(catalogId, out var supply) && supply is not null
                ? supply.Name
                : catalogId;

    private void ScheduleArrivalErrands(HandlerState handler, bool paced)
    {
        var arrivedAt = handler.HomeArrivedAt
            ?? throw new InvalidOperationException("A home visit must have an arrival time before preparation.");
        var plan = DecidePreparation(handler, arrivedAt);

        var stops = new List<(string Label, Action<DateTimeOffset> Commit)>();

        if (plan.ItemInstanceIdsToSell.Count > 0)
        {
            var chunkCount = Math.Min(SaleDesks.Length, plan.ItemInstanceIdsToSell.Count);
            var chunkSize = (plan.ItemInstanceIdsToSell.Count + chunkCount - 1) / chunkCount;
            var chunks = plan.ItemInstanceIdsToSell.Chunk(chunkSize).ToArray();
            for (var index = 0; index < chunks.Length; index++)
            {
                var chunk = chunks[index];
                var desk = SaleDesks[index];
                var action = chunks.Length == 1 ? "sale" : $"sale:{index}";
                stops.Add(($"Selling salvage at the {desk}", at =>
                {
                    var sale = handler.Stash.SellByInstanceIds(chunk);
                    handler.Coins = checked(handler.Coins + sale.CoinsGained);
                    RecordPreparationDecision(handler, at, "PreparationSale", action,
                        $"Sold {sale.SoldItems.Count} Home Stash items at the {desk} for {sale.CoinsGained} coins. Balance: {handler.Coins}.");
                }));
            }
        }

        if (plan.WeaponInstanceIdToRepair is { } repairId)
        {
            stops.Add(("At the Gear Weirdos repair bench", at =>
            {
                var index = handler.OwnedWeapons.FindIndex(weapon => weapon.InstanceId == repairId);
                if (index < 0)
                    throw new InvalidOperationException("Preparation selected a weapon the Raider does not own.");
                var weapon = handler.OwnedWeapons[index];
                if (weapon.CurrentDurability >= weapon.MaxDurability
                    || handler.Coins < weapon.RepairCost
                    || handler.Coins - weapon.RepairCost < plan.ReserveFloor)
                    throw new InvalidOperationException("Preparation repair failed coordinator validation.");
                handler.Coins -= weapon.RepairCost;
                var repaired = weapon with { CurrentDurability = weapon.MaxDurability };
                handler.OwnedWeapons[index] = repaired;
                RecordPreparationDecision(handler, at, "PreparationRepair", $"repair:{repairId}",
                    $"Repaired {repaired.Name} at the Gear Weirdos repair bench for {weapon.RepairCost} coins. Balance: {handler.Coins}.");
            }));
        }

        if (plan.WeaponCatalogIdToBuy is { } weaponCatalogId
            && plan.WeaponInstanceIdToBuy is not null)
        {
            stops.Add(("Browsing the Gear Weirdos weapon counter", at =>
            {
                CommitEarlierPriorityTraderErrands(handler, at);
                var stock = TraderStockAt(at);
                var selectedId = CanBuyWeapon(handler, stock, weaponCatalogId, plan.ReserveFloor)
                    ? weaponCatalogId
                    : DecidePreparation(handler, at, plan.ReserveFloor, stock.AvailableQuantities)
                        .WeaponCatalogIdToBuy;
                if (selectedId is null
                    || !CanBuyWeapon(handler, stock, selectedId, plan.ReserveFloor))
                {
                    RecordUnavailablePurchase(handler, at, weaponCatalogId,
                        stock.Remaining(weaponCatalogId) == 0);
                    EnsureTeaKettleFallback(handler, at);
                    return;
                }

                var definition = Domain.ShopCatalog.Weapons.Single(weapon =>
                    weapon.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
                var purchased = Domain.ShopCatalog.CreateWeaponInstance(definition,
                    RaiderEconomyPolicy.CreateWeaponInstanceId(handler.PlayerId, handler.HomeVisit, selectedId));
                if (!stock.TryBuy(selectedId, 1))
                    throw new InvalidOperationException("Trader weapon stock changed during a serialized purchase.");
                handler.Coins -= definition.Value;
                handler.OwnedWeapons.Add(purchased);
                RecordPreparationDecision(handler, at, "PreparationWeaponPurchase", $"weapon:{selectedId}",
                    $"Bought {purchased.Name} at the Gear Weirdos weapon counter for {definition.Value} coins. Balance: {handler.Coins}.");
                if (!selectedId.Equals(weaponCatalogId, StringComparison.OrdinalIgnoreCase))
                    RecordSubstitutePurchase(handler, at, weaponCatalogId, selectedId, "Gear Weirdos");
            }));
        }

        if (plan.CreateTeaKettleFallback)
        {
            stops.Add(("At the Hatch Authority loaner desk", at =>
            {
                EnsureTeaKettleFallback(handler, at);
            }));
        }

        if (plan.WeaponInstanceIdToEquip is { } equipId)
        {
            stops.Add(("Checking gear at the locker", at =>
            {
                var equipped = RaiderEconomyPolicy.BestUsableWeapon(
                    handler.OwnedWeapons, Domain.ShopCatalog.Weapons)
                    ?? throw new InvalidOperationException("Preparation has no usable owned weapon after fallback.");
                handler.EquippedWeaponInstanceId = equipped.InstanceId;
                RecordPreparationDecision(handler, at, "PreparationWeaponEquipped", $"equip:{equipped.InstanceId}",
                    $"Equipped {equipped.Name} ({equipped.CurrentDurability}/{equipped.MaxDurability} durability).");
            }));
        }

        foreach (var (purchase, purchaseIndex) in plan.SupplyPurchases.Select((purchase, index) => (purchase, index)))
        {
            if (!Domain.ShopCatalog.TryGetSupply(purchase.CatalogId, out var definition) || definition is null
                || purchase.Quantity <= 0)
                throw new InvalidOperationException("Preparation selected an invalid supply purchase.");
            var desk = SupplyDesk(definition.Category);
            stops.Add(($"Shopping at the {desk}", at =>
            {
                CommitEarlierPriorityTraderErrands(handler, at);
                var stock = TraderStockAt(at);
                var acquired = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var unit = 0; unit < purchase.Quantity; unit++)
                {
                    var selectedId = CanBuySupply(handler, stock, purchase.CatalogId, plan.ReserveFloor)
                        ? purchase.CatalogId
                        : DecidePreparation(handler, at, plan.ReserveFloor, stock.AvailableQuantities)
                            .SupplyPurchases.FirstOrDefault()?.CatalogId;
                    if (selectedId is null || !CanBuySupply(handler, stock, selectedId, plan.ReserveFloor))
                        break;

                    var selected = Domain.ShopCatalog.Supplies.Single(supply =>
                        supply.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
                    if (!stock.TryBuy(selectedId, 1))
                        throw new InvalidOperationException("Trader supply stock changed during a serialized purchase.");
                    handler.Coins -= selected.Value;
                    handler.SupplyStock[selected.Id] = checked(
                        handler.SupplyStock.GetValueOrDefault(selected.Id) + 1);
                    acquired[selected.Id] = acquired.GetValueOrDefault(selected.Id) + 1;
                    if (!selectedId.Equals(purchase.CatalogId, StringComparison.OrdinalIgnoreCase))
                        RecordSubstitutePurchase(handler, at, purchase.CatalogId, selectedId, desk);
                }

                foreach (var (catalogId, quantity) in acquired)
                {
                    var bought = Domain.ShopCatalog.Supplies.Single(supply =>
                        supply.Id.Equals(catalogId, StringComparison.OrdinalIgnoreCase));
                    var totalCost = checked(bought.Value * quantity);
                    RecordPreparationDecision(handler, at, "PreparationSupplyPurchase",
                        $"supply:{purchaseIndex}:{catalogId}",
                        $"Bought {quantity}x {bought.Name} at the {SupplyDesk(bought.Category)} for {totalCost} coins. Balance: {handler.Coins}.");
                }
                var purchasedCount = acquired.Values.Sum();
                if (purchasedCount < purchase.Quantity)
                    RecordUnavailablePurchase(handler, at, purchase.CatalogId,
                        stock.Remaining(purchase.CatalogId) == 0,
                        $"supply:{purchaseIndex}");
            }));
        }

        if (plan.UpdateStagedSupplies || plan.SupplyPurchases.Count > 0)
        {
            stops.Add(("Packing supplies at the Field Depot Counter", at =>
            {
                var actualStaging = DecidePreparation(handler, at, plan.ReserveFloor,
                    new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase));
                var medicalCount = 0;
                var rechargerCount = 0;
                foreach (var item in actualStaging.StagedSupplies)
                {
                    if (item.Quantity <= 0
                        || !Domain.ShopCatalog.TryGetSupply(item.CatalogId, out var definition)
                        || definition is null
                        || handler.SupplyStock.GetValueOrDefault(definition.Id) < item.Quantity)
                        throw new InvalidOperationException("Preparation staging failed coordinator validation.");
                    if (definition.Category == "Medical")
                        medicalCount += item.Quantity;
                    else if (definition.Category == "ShieldRecharger")
                        rechargerCount += item.Quantity;
                }
                if (medicalCount > ShopCatalog.MaxStagedMedicalCount
                    || rechargerCount > ShopCatalog.MaxStagedRechargerCount)
                    throw new InvalidOperationException("Preparation staging exceeded a category limit.");

                handler.StagedSupplies.Clear();
                foreach (var item in actualStaging.StagedSupplies)
                    handler.StagedSupplies[item.CatalogId] = item.Quantity;
                RecordPreparationDecision(handler, at, "PreparationSuppliesStaged", "stage",
                    $"Staged {medicalCount} medical supplies and {rechargerCount} rechargers from Raider-owned stock at the Field Depot Counter.");
            }));
        }

        stops.Add(("Wrapping up errands", at =>
        {
            RecordPreparationDecision(handler, at, "PreparationReady", "ready",
                $"{DisplayName(handler.Slot)} is ready in Desperanza as a {plan.PrimaryTrait}; {handler.Coins} coins remain.");
            handler.PreparationCompletedVisit = handler.HomeVisit;
        }));

        if (!paced)
        {
            foreach (var (label, commit) in stops)
                handler.PendingErrands.Add(new PreparationErrand(arrivedAt, label, commit, Paced: false, Optional: false));
            return;
        }

        var recoverAt = arrivedAt + ErrandRecovery;
        var restIndex = (int)(StableHash($"{handler.Slot}:{handler.HomeVisit}:rest") % 2);
        var restPool = restIndex == 0 ? "hub.errand-rest-canteen" : "hub.errand-rest-outpost";
        var restPlace = restIndex == 0 ? "Last Call Canteen" : "Hatch Outpost";
        var budget = HomeInterval - ErrandRecovery - ErrandRestMargin;
        var stopDuration = TimeSpan.FromTicks(Math.Min(ErrandStopDuration.Ticks, budget.Ticks / (stops.Count + 1)));

        handler.PendingErrands.Add(new PreparationErrand(recoverAt, "Collecting themselves after the raid", at =>
        {
            var lines = Content.NarrationContent.Default.Pool("hub.errand-recover");
            RecordPreparationDecision(handler, at, "PreparationRecover", "recover",
                lines[handler.HomeVisit % lines.Count]);
        }, Paced: true, Optional: false));
        for (var index = 0; index < stops.Count; index++)
        {
            handler.PendingErrands.Add(new PreparationErrand(recoverAt + stopDuration * (index + 1),
                stops[index].Label, stops[index].Commit, Paced: true, Optional: false));
        }
        var restAt = recoverAt + stopDuration * (stops.Count + 1);
        handler.PendingErrands.Add(new PreparationErrand(restAt, $"Resting at the {restPlace}", at =>
        {
            var lines = Content.NarrationContent.Default.Pool(restPool);
            RecordPreparationDecision(handler, at, "PreparationRest", "rest",
                lines[handler.HomeVisit % lines.Count]);
        }, Paced: true, Optional: true));
        handler.NextHubActivityAt = restAt + HubActivityInterval;
    }

    private void CommitDueErrands(HandlerState handler, DateTimeOffset upTo)
    {
        while (handler.PendingErrands.Count > 0 && handler.PendingErrands[0].DueAt <= upTo)
        {
            var errand = handler.PendingErrands[0];
            handler.PendingErrands.RemoveAt(0);
            errand.Commit(errand.DueAt);
            handler.HubErrandLabel = errand.Paced ? errand.Label : null;
        }
    }

    private void CommitEarlierPriorityTraderErrands(HandlerState current, DateTimeOffset at)
    {
        if (committingPriorityPredecessors)
            return;
        committingPriorityPredecessors = true;
        try
        {
            foreach (var earlier in handlers.Values
                .Where(handler => handler != current
                    && handler.IsHome
                    && handler.HomeArrivedAt <= current.HomeArrivedAt
                    && (handler.HomeArrivedAt < current.HomeArrivedAt
                        || string.CompareOrdinal(handler.PlayerId, current.PlayerId) < 0)
                    && handler.PendingErrands.Any(IsTraderPurchaseErrand))
                .OrderBy(handler => handler.HomeArrivedAt)
                .ThenBy(handler => handler.PlayerId, StringComparer.Ordinal))
            {
                while (earlier.PendingErrands.Count > 0 && !earlier.PendingErrands[0].Optional)
                {
                    var errand = earlier.PendingErrands[0];
                    earlier.PendingErrands.RemoveAt(0);
                    errand.Commit(at);
                    earlier.HubErrandLabel = errand.Paced ? errand.Label : null;
                }
            }
        }
        finally
        {
            committingPriorityPredecessors = false;
        }
    }

    private static bool IsTraderPurchaseErrand(PreparationErrand errand) =>
        errand.Label == "Browsing the Gear Weirdos weapon counter"
        || errand.Label.StartsWith("Shopping at the ", StringComparison.Ordinal);

    private void HurryRemainingErrands(HandlerState handler, DateTimeOffset at)
    {
        handler.PendingErrands.RemoveAll(errand => errand.Optional);
        while (handler.PendingErrands.Count > 0)
        {
            var errand = handler.PendingErrands[0];
            handler.PendingErrands.RemoveAt(0);
            errand.Commit(errand.DueAt < at ? errand.DueAt : at);
        }
        handler.HubErrandLabel = "Hurrying to the Hatch";
    }

    private void ProcessDueErrands(DateTimeOffset at)
    {
        foreach (var handler in handlers.Values
            .Where(handler => handler.IsHome && handler.PendingErrands.Count > 0)
            .OrderBy(handler => handler.HomeArrivedAt)
            .ThenBy(handler => handler.PlayerId, StringComparer.Ordinal)
            .ThenBy(handler => handler.PendingErrands[0].DueAt))
        {
            CommitDueErrands(handler, at);
        }
    }

    private static string SupplyDesk(string category) => category switch
    {
        "Medical" => "Stitch-It Aid Window",
        "ShieldRecharger" => "Bravado Battery Kiosk",
        _ => "Field Depot Counter"
    };

    private static uint StableHash(string value) =>
        BitConverter.ToUInt32(SHA256.HashData(Encoding.UTF8.GetBytes(value)), 0);

    private static void RecordPreparationDecision(
        HandlerState handler,
        DateTimeOffset at,
        string kind,
        string action,
        string text)
    {
        var sourceEventId = $"hub:preparation:{handler.HomeVisit}:{action}";
        if (!handler.PreparationEventIds.Add(sourceEventId))
            return;
        handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, at, kind, text));
        handler.LatestPreparationDecisions.Add(new PreparationDecisionView(at, kind, text));
        AddJournalEntry(handler, null, at, kind, "Hub", text, sourceEventId, location: "Desperanza");
    }

    private void BeginHomeStay(HandlerState handler, DateTimeOffset at, bool initial = false)
    {
        handler.HomeVisit++;
        handler.HomeArrivedAt = at;
        handler.HomeAvailableAt = at + (fastPacing
            ? initial ? TimeSpan.Zero : TimeSpan.FromSeconds(60)
            : HomeInterval);
        handler.HomeReadinessLogged = false;
        handler.NextHubActivityAt = at + HubActivityInterval;
    }

    private void RecordHubActivities(DateTimeOffset at)
    {
        foreach (var handler in handlers.Values.Where(handler => handler.IsHome)
            .OrderBy(handler => handler.Slot, StringComparer.Ordinal))
        {
            while (handler.NextHubActivityAt is { } activityAt && activityAt <= at)
            {
                var lines = Content.NarrationContent.Default.Pool("hub.activities");
                var flavor = lines[handler.HubActivitySequence % lines.Count];
                handler.HubActivitySequence++;
                handler.NextHubActivityAt = activityAt + HubActivityInterval;
                AddJournalEntry(handler, null, activityAt, "HubActivity", "Hub", flavor,
                    $"hub:activity:{handler.Slot}:{handler.HubActivitySequence}",
                    location: "Desperanza", activity: "HangingOut",
                    factsText: "Spending time in Desperanza. No gameplay effects.");
            }
        }
    }

    private void RecordHomeReadiness(DateTimeOffset at)
    {
        foreach (var handler in handlers.Values.Where(handler =>
            handler.IsHome && !handler.HomeReadinessLogged &&
            handler.HomeAvailableAt is { } availableAt && availableAt <= at))
        {
            var readyAt = handler.HomeAvailableAt!.Value;
            handler.HomeReadinessLogged = true;
            var message = $"{DisplayName(handler.Slot)}'s home interval is complete; ready for another eligible admission in Desperanza.";
            handler.Events.Add(new HandlerEventView(handler.Events.Count + 1, readyAt, "HubReady", message));
            AddJournalEntry(handler, null, readyAt, "HubReady", "Hub", message,
                $"hub:ready:{handler.LastReturnSummary?.RaidId}:{handler.Slot}");
        }
    }

    private HubResidentView[] GetHubResidents(DateTimeOffset at) =>
        handlers.Values
            .Where(handler => handler.IsHome)
            .OrderBy(handler => handler.Slot, StringComparer.Ordinal)
            .Select(handler => new HubResidentView(
                handler.Slot,
                DisplayName(handler.Slot),
                handler.HomeAvailableAt is { } availableAt && availableAt > at
                    ? handler.HubErrandLabel ?? "Hanging out in Desperanza"
                    : handler.DutyEnabled ? "Ready for duty in Desperanza" : "At home in Desperanza / duty paused",
                handler.HomeAvailableAt))
            .ToArray();

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
            : !handler.DutyEnabled ? "At Home / Paused"
            : handler.HomeAvailableAt > at ? "At Home / Hanging Out"
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
        var traderStockView = TraderStockAt(at).Project();

        RegenerateSignal(handler, at);

        return new HandlerProjection(handler.Slot, handler.PlayerId, DisplayName(handler.Slot), handler.DutyEnabled,
            status, handler.HomeAvailableAt, handler.CurrentRaidId, raidProjection,
            handler.LastExtractedLoot, handler.Events.ToArray(), at,
            handler.LastReturnSummary, handler.Journal.ToArray(),
            handler.Coins, handler.Stash.TotalValue, handler.Stash.Count, HomeStash.MaxCapacity,
            handler.Stash.Items, handler.Stash.GetGroups(),
            equippedWeapon, handler.OwnedWeapons.ToArray(), supplyList, stagedList, shopCatalogView,
            handler.SignalCurrent, handler.SignalMax, raidProjection?.MyHiddenPocketInstanceId,
            GetHubResidents(at), handler.IsHome,
            handler.IsHome && (!handler.DutyEnabled || handler.HomeAvailableAt > at), ReadyUpSignalCost,
            handler.PrimaryTrait.ToString(), handler.LatestPreparationDecisions.ToArray(),
            handler.IsHome ? handler.HubErrandLabel : null,
            traderStockView);
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

    internal void ResetPreparationForTesting(
        string accessToken,
        int coins,
        DateTimeOffset arrivedAt)
    {
        lock (gate)
        {
            var handler = GetAuthorizedHandler(accessToken);
            handler.HomeVisit++;
            handler.IsHome = true;
            handler.CurrentRaidId = null;
            handler.Coins = coins;
            handler.HomeArrivedAt = arrivedAt;
            handler.HomeAvailableAt = arrivedAt + HomeInterval;
            handler.PreparationStartedVisit = 0;
            handler.PreparationCompletedVisit = 0;
            handler.PreparationEventIds.Clear();
            handler.LatestPreparationDecisions.Clear();
            handler.PendingErrands.Clear();
            handler.HubErrandLabel = null;
            PrepareForArrival(handler, paced: true);
        }
    }

    internal void ApplyWeaponDamage(string accessToken, string instanceId, int damage) =>
        DamageWeaponForTesting(accessToken, instanceId, damage);

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

        if (request.ActionType is not ("CallExtract" or "DepartExtraction" or "Calm" or "Pressure"))
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

            if (request.ActionType is "CallExtract" or "DepartExtraction")
            {
                world.Simulation.Advance(now);
                if (request.ActionType == "CallExtract" && !world.Simulation.CanCallExtract(state.PlayerId))
                {
                    throw new RaidCommandException("ExtractionAlreadyInProgress");
                }
                if (request.ActionType == "DepartExtraction" && !world.Simulation.CanDepartExtraction(state.PlayerId))
                {
                    throw new RaidCommandException("ExtractionVehicleNotArrived");
                }
            }

            var cost = request.ActionType switch
            {
                "CallExtract" => 3,
                "DepartExtraction" => 0,
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
                "CallExtract" => "Extraction vehicle called (3 Signal spent). It arrives after 60 seconds; your Raider departs automatically after a 30-second boarding window if you do not confirm.",
                "DepartExtraction" => "Departure confirmed. Your Raider boards the arrived vehicle and returns to Desperanza.",
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
        if (narrativeGenerator is TemplateNarrativeGenerator || simulation.Completed ||
            time.GetUtcNow() - evt.At > TimeSpan.FromSeconds(90) ||
            !Content.NarrationContent.Default.CanEnrich(evt.Kind)) return;
        if (!narrativeGenerator.IsAvailable)
        {
            simulation.MarkNarrationUnavailable(evt.Sequence);
            return;
        }
        if (!simulation.TryQueueNarration(evt.Sequence)) return;
        var location = evt.Location ?? "Desperanza";
        var isRobot = evt.Kind == "RobotRound";
        var context = new AmbientCommsContext(location, evt.Activity ?? evt.Kind,
            EventKind: evt.Kind,
            Condition: simulation.Condition.Name,
            Danger: simulation.Condition.Danger.ToString(),
            RobotName: isRobot ? simulation.RobotName : null,
            NearbyRaiders: isRobot ? Math.Max(0, evt.ParticipantIds.Count - 1)
                : simulation.OtherActiveRaidersAt(location, evt.ParticipantIds[0]));
        var allowedTerms = OnnxGenAiNarrativeGenerator.AllowedTermsFor(context);
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
                var flavor = result.Source == "Generated" ? OnnxGenAiNarrativeGenerator.ValidateAmbient(result.Text, out rejectionReason, allowedTerms) : null;
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
            context,
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
    private sealed record PreparationErrand(
        DateTimeOffset DueAt, string Label, Action<DateTimeOffset> Commit, bool Paced, bool Optional);

    private sealed class TraderCycleStock(TraderStockSnapshot snapshot)
    {
        private readonly Dictionary<string, int> remaining = snapshot.Items
            .Where(item => item.Quantity is not null)
            .ToDictionary(item => item.CatalogId, item => item.Quantity!.Value,
                StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, int> AvailableQuantities => remaining
            .Where(entry => entry.Value > 0)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.OrdinalIgnoreCase);

        public int Remaining(string catalogId) => remaining.GetValueOrDefault(catalogId);

        public bool TryBuy(string catalogId, int quantity)
        {
            if (quantity <= 0 || Remaining(catalogId) < quantity)
                return false;
            remaining[catalogId] -= quantity;
            return true;
        }

        public TraderStockView Project() => new(
            snapshot.CycleIndex,
            snapshot.Items.Select(item => new TraderStockItemView(
                item.CatalogId,
                item.Name,
                item.Category,
                item.Quantity is null ? null : Remaining(item.CatalogId),
                item.UnitPrice)).ToArray());
    }

    private sealed class HandlerState(string slot, string playerId, string enrollmentRequestId, string accessToken)
    {
        public string Slot { get; } = slot;
        public string PlayerId { get; } = playerId;
        public string EnrollmentRequestId { get; } = enrollmentRequestId;
        public string AccessToken { get; } = accessToken;
        public int TraitSeed { get; init; }
        public RaiderPrimaryTrait PrimaryTrait => RaiderEconomyPolicy.TraitFromSeed(TraitSeed);
        public bool DutyEnabled { get; set; } = true;
        public bool IsHome { get; set; } = true;
        public bool HomeReadinessLogged { get; set; }
        public DateTimeOffset? HomeAvailableAt { get; set; }
        public DateTimeOffset? HomeArrivedAt { get; set; }
        public int HomeVisit { get; set; }
        public int PreparationStartedVisit { get; set; }
        public int PreparationCompletedVisit { get; set; }
        public HashSet<string> PreparationEventIds { get; } = new(StringComparer.Ordinal);
        public List<PreparationDecisionView> LatestPreparationDecisions { get; } = [];
        public List<PreparationErrand> PendingErrands { get; } = [];
        public string? HubErrandLabel { get; set; }
        public DateTimeOffset? NextHubActivityAt { get; set; }
        public int HubActivitySequence { get; set; }
        public HashSet<string> ProcessedReadyCommands { get; } = new(StringComparer.Ordinal);
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

        public List<OwnedWeapon> OwnedWeapons { get; } = [Domain.ShopCatalog.CreateWeaponInstance(Domain.ShopCatalog.GetStarterWeapon())];
        public string? EquippedWeaponInstanceId { get; set; }
        public Dictionary<string, int> SupplyStock { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> StagedSupplies { get; } = new(StringComparer.OrdinalIgnoreCase);
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
public sealed record ReadyUpRequest(string CommandId);
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
    string? SecretHiddenPocketInstanceId = null,
    IReadOnlyList<HubResidentView>? HubResidents = null,
    bool IsHome = false,
    bool CanReadyUp = false,
    int ReadyUpSignalCost = 2,
    string PrimaryTrait = "Cautious",
    IReadOnlyList<PreparationDecisionView>? LatestPreparationDecisions = null,
    string? HubErrand = null,
    TraderStockView? TraderStock = null);
public sealed record TraderStockView(long CycleIndex, IReadOnlyList<TraderStockItemView> Items);
public sealed record TraderStockItemView(string CatalogId, string Name, string Category, int? Quantity, int UnitPrice);
public sealed record HubResidentView(string Slot, string DisplayName, string Status, DateTimeOffset? HomeAvailableAt);
public sealed record HandlerEventView(int Sequence, DateTimeOffset At, string Kind, string Text);
public sealed record PreparationDecisionView(DateTimeOffset At, string Kind, string Text);

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
