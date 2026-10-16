using AFKRaiders.Server.Content;

using System.Security.Cryptography;
using System.Text;

namespace AFKRaiders.Server.Domain;

public sealed class RaidSimulation
{
    public const string RulesVersion = "desperanza-g6-v3";
    public static readonly TimeSpan AdmissionWindow = TimeSpan.FromSeconds(20);
    public const int MaxRaiders = 2;
    private const int PodDeploymentSeconds = 60;
    private const int ExtractionVehicleArrivalSeconds = 60;
    private const int ExtractionBoardingWindowSeconds = 30;
    private const int DecisionIntervalSeconds = 30;
    private static readonly TimeSpan RobotRoundInterval = TimeSpan.FromSeconds(5);
    private const int RobotRoundLimit = 3;
    private const int RobotDamagePerRaider = 20;
    private const int RobotDamageToRaider = 10;
    private readonly int seed;
    private readonly IRaidNarrator narrator;
    private readonly ILootCatalog catalog;
    private readonly bool fastPacing;
    private readonly bool riskReward;
    private readonly DangerProfile dangerProfile;
    private DateTimeOffset nextDecisionAt;
    private DateTimeOffset nextForecastAt;
    private DateTimeOffset? nextHazardAt;
    private bool deadlineResolved;
    private readonly List<HazardWarningView> hazardWarnings = [];
    private readonly HashSet<int> queuedNarration = [];
    private readonly List<SimRaider> raiders = [];
    private readonly List<RaidEventView> events = [];
    private readonly List<WorldItem> items;
    private readonly List<EncounterView> encounters = [];
    private readonly RobotState robot = new();
    private RobotEncounterView? robotEncounter;
    private SharedExtractionVehicle? sharedExtraction;
    private bool departed;
    private readonly Opportunity[] opportunities;
    private readonly List<PendingHandlerAction> pendingHandlerActions = [];
    private static readonly LocationView[] locations =
    [
        new("Entry Tunnel", ["Market"]),
        new("Market", ["Entry Tunnel", "Salvage Yard", "Service Depot", "Extraction Platform"]),
        new("Salvage Yard", ["Market", "Extraction Platform"]),
        new("Service Depot", ["Market", "Extraction Platform"]),
        new("Extraction Platform", ["Market", "Salvage Yard", "Service Depot"])
    ];

    public RaidSimulation(
        string raidId,
        DateTimeOffset startsAt,
        int seed,
        IRaidNarrator? narrator = null,
        ILootCatalog? catalog = null,
        bool useTableBackedLoot = false,
        bool fastPacing = false,
        bool? riskReward = null,
        string? danger = null,
        int conditionMomentum = 0)
    {
        RaidId = raidId;
        StartsAt = startsAt;
        this.seed = seed;
        this.narrator = narrator ?? new TemplateRaidNarrator();
        this.catalog = catalog ?? LootCatalog.Default;
        this.fastPacing = fastPacing;
        this.riskReward = riskReward ?? !fastPacing;
        Condition = danger is not null
            ? RaidConditions.Conditions.Single(c => c.Danger == danger)
            : RaidConditions.Conditions[(int)(RaidConditions.Roll(seed, "condition") *
                (conditionMomentum >= 30 ? 3 : 2))];
        Zone = RaidConditions.Zones[(int)(RaidConditions.Roll(seed, "zone") * RaidConditions.Zones.Count)];
        dangerProfile = RaidConditions.Profile(Condition.Danger);
        nextDecisionAt = startsAt.AddSeconds(DecisionIntervalSeconds);
        nextForecastAt = startsAt.AddSeconds(50);
        if (this.riskReward)
        {
            robot.Id = dangerProfile.RobotId;
            robot.Name = dangerProfile.RobotName;
            robot.Flavor = dangerProfile.RobotFlavor;
            robot.Health = robot.MaxHealth = dangerProfile.RobotHealth;
        }
        opportunities = this.catalog.Opportunities
            .Select(o => new Opportunity(o.Id, o.Location, o.DurationSeconds))
            .ToArray();
        var worldInstances = useTableBackedLoot
            ? this.catalog.CreateTableBackedWorldItems(raidId, seed)
            : this.catalog.CreateFixedWorldItems(raidId);
        items = worldInstances.Select(w => new WorldItem(w.OpportunityId, w.Item)).ToList();
    }

    public ILootCatalog Catalog => catalog;

    public string RaidId { get; }
    public DateTimeOffset StartsAt { get; }
    public ZoneConditionView Condition { get; }
    public string Zone { get; }
    public DateTimeOffset DeadlineAt => StartsAt.AddSeconds(RaidConditions.DeadlineSeconds);
    public bool CanAdmit(DateTimeOffset at) => !riskReward || at <= StartsAt.AddSeconds(RaidConditions.LastAdmissionSeconds);
    public RaiderPersonality Personality(string playerId)
    {
        var raider = raiders.Single(r => r.PlayerId == playerId);
        return new(raider.Greed, raider.Mood);
    }
    internal RaiderVoiceState VoiceState(string playerId)
    {
        var raider = raiders.Single(r => r.PlayerId == playerId);
        return new(new Dictionary<string, long>(raider.VoiceCounts), raider.LastVoice);
    }
    internal void RestoreVoiceState(string playerId, RaiderVoiceState? state)
    {
        if (state is null) return;
        var raider = raiders.Single(r => r.PlayerId == playerId);
        foreach (var (key, count) in state.Counts) raider.VoiceCounts[key] = count;
        raider.LastVoice = state.LastLine;
    }
    public int ParticipantCount => raiders.Count;
    public string RobotName => robot.Name;
    // Prompt context only: counts other active Raiders sharing a location; never projected with identities.
    internal int OtherActiveRaidersAt(string location, string playerId) => raiders.Count(r =>
        r.PlayerId != playerId && r.Location == location && r.Activity is not ("Extracted" or "KnockedOut"));
    internal bool HasRaider(string playerId) => raiders.Any(r => r.PlayerId == playerId);
    public int Revision { get; private set; } = 1;
    public bool Completed => departed && raiders.Count > 0 && raiders.All(raider => raider.Activity is "Extracted" or "KnockedOut");
    public DateTimeOffset? NextActivityAt
    {
        get
        {
            if (Completed) return null;
            if (!departed) return StartsAt + AdmissionWindow;
            var deadlines = raiders.SelectMany(r => new[] { r.ActivityEndsAt, r.NextDiaryAt }).ToList();
            deadlines.Add(robot.NextRoundAt);
            if (riskReward)
            {
                deadlines.Add(nextDecisionAt);
                deadlines.Add(nextForecastAt);
                deadlines.Add(nextHazardAt);
                if (!deadlineResolved) deadlines.Add(DeadlineAt);
            }
            if (sharedExtraction is { Status: "Calling" } calling)
                deadlines.Add(calling.ArrivalAt);
            if (sharedExtraction is { Status: "Boarding", BoardingClosesAt: { } boardingClosesAt })
                deadlines.Add(boardingClosesAt);
            deadlines.AddRange(opportunities
                .Where(opportunity => !opportunity.Resolved)
                .Select(opportunity => opportunity.ClosesAt));
            deadlines.AddRange(pendingHandlerActions.Select(action => (DateTimeOffset?)action.QueuedAt));
            return deadlines.Where(d => d.HasValue).Min();
        }
    }
    public string Status => Completed ? "Completed" : !departed ? "Preparing" : "Raiding";

    public void AddRaider(string playerId, string slot, DateTimeOffset now, IEnumerable<StagedSupplyItem>? stagedSupplies = null)
    {
        if (slot is not ("raider-one" or "raider-two"))
        {
            throw new RaidCommandException("InvalidDevelopmentIdentity");
        }
        if (departed || now >= StartsAt + AdmissionWindow)
        {
            throw new RaidAccessException("RaidAlreadyDeparted");
        }

        if (raiders.Any(raider => raider.Slot == slot))
        {
            throw new RaidAccessException("SlotUnavailable");
        }
        if (raiders.Any(raider => raider.PlayerId == playerId))
        {
            throw new RaidAccessException("PlayerAlreadyJoined");
        }

        raiders.Add(CreateRaider(playerId, slot, StartsAt + AdmissionWindow, stagedSupplies));
        Revision++;
        var added = raiders.Single(raider => raider.PlayerId == playerId);
        Emit(now, "Deployment", [playerId], NarrationContent.Default.Format("narrator.checks-in-the-handler-is", added.Name));
    }

    public void AddAutonomousRaider(string playerId, string slot, DateTimeOffset at, IEnumerable<StagedSupplyItem>? stagedSupplies = null,
        RaiderPersonality? personality = null, RaiderVoiceState? voiceState = null)
    {
        if (slot is not ("raider-one" or "raider-two"))
        {
            throw new RaidCommandException("InvalidDevelopmentIdentity");
        }
        if (Completed)
        {
            throw new RaidAccessException("RaidCompleted");
        }
        if (!CanAdmit(at)) throw new RaidAccessException("WorldAdmissionClosed");
        if (raiders.Count >= MaxRaiders)
        {
            throw new RaidAccessException("RaidFull");
        }
        if (raiders.Any(raider => raider.Slot == slot))
        {
            throw new RaidAccessException("RaiderAlreadyAdmitted");
        }
        if (raiders.Any(raider => raider.PlayerId == playerId))
        {
            throw new RaidAccessException("PlayerAlreadyJoined");
        }

        departed = true;
        raiders.Add(CreateRaider(playerId, slot, at, stagedSupplies, personality));
        RestoreVoiceState(playerId, voiceState);
        Revision++;
        var added = raiders.Single(raider => raider.PlayerId == playerId);
        Emit(at, "DutyAdmission", [playerId], NarrationContent.Default.Format("simulation.enters-the-ongoing-raid-from", added.Name));
        if (riskReward)
            Emit(at, "WorldCondition", [playerId],
                NarrationContent.Default.Format("simulation.shared-zone-danger-world-closes", Zone, Condition.Name, Condition.Danger, Condition.Description, DeadlineAt),
                location: added.Location, reason: Condition.Id);
    }

    public bool QueueHandlerAction(string actionId, string actionType, string playerId, DateTimeOffset now)
    {
        var raider = raiders.FirstOrDefault(r => r.PlayerId == playerId);
        if (raider is null || raider.Activity is "Extracted" or "KnockedOut") return false;
        if (actionType == "DepartExtraction" && raider.Activity != "WaitingToDepart") return false;
        if (actionType == "CallExtract" && !CanCallExtract(playerId)) return false;

        if (pendingHandlerActions.Any(a => a.ActionId == actionId && a.PlayerId == playerId)) return true;
        if (pendingHandlerActions.Any(a => a.PlayerId == playerId)) return false;

        pendingHandlerActions.Add(new PendingHandlerAction(actionId, actionType, playerId, now));
        return true;
    }

    internal bool CanDepartExtraction(string playerId) =>
        raiders.FirstOrDefault(raider => raider.PlayerId == playerId)?.Activity == "WaitingToDepart";

    internal bool CanCallExtract(string playerId) =>
        raiders.FirstOrDefault(raider => raider.PlayerId == playerId) is
            { EmergencyExtract: false, Activity: not ("Extracted" or "KnockedOut" or "Downed" or "CallingExtraction" or "WaitingToDepart" or "Extracting") };

    public bool DesignateHiddenPocket(string playerId, string? itemInstanceId)
    {
        var raider = raiders.FirstOrDefault(r => r.PlayerId == playerId);
        if (raider is null || raider.Activity is "Extracted" or "KnockedOut") return false;

        if (string.IsNullOrEmpty(itemInstanceId))
        {
            raider.HiddenPocketInstanceId = null;
            return true;
        }

        var item = raider.Backpack.FirstOrDefault(i => i.InstanceId == itemInstanceId);
        if (item is null) return false;

        raider.HiddenPocketInstanceId = itemInstanceId;
        return true;
    }

    private SimRaider CreateRaider(string playerId, string slot, DateTimeOffset readyAt, IEnumerable<StagedSupplyItem>? stagedSupplies = null,
        RaiderPersonality? personality = null)
    {
        var hoarder = Factor(slot, "disposition") % 2 == 0;
        var name = slot == "raider-one" ? "Mira \"Refund\" Malone" : "Dev \"Wet Socks\" Kowalski";
        var firstBranch = Factor(slot, "first-branch") % 2 == 0 ? "Salvage Yard" : "Service Depot";
        var secondBranch = firstBranch == "Salvage Yard" ? "Service Depot" : "Salvage Yard";
        var raider = new SimRaider(playerId, slot, name, hoarder ? "Hoarder" : "Cautious",
            hoarder ? 35 : 15, 4 + Factor(slot, "aptitude") % 5, Factor(slot, "initiative") % 6,
            new Queue<string>(["Market", firstBranch, "Market", secondBranch, "Extraction Platform"]))
        {
            ExitVehicle = Factor(slot, "exit-vehicle") % 2 == 0 ? "train" : "service elevator",
            ActivityStartedAt = readyAt,
            Greed = personality?.Greed ?? (hoarder ? 35 : 15),
            Mood = personality?.Mood ?? 2,
            AdmittedAt = readyAt
        };

        if (stagedSupplies is not null)
        {
            foreach (var staged in stagedSupplies)
            {
                if (staged.Quantity <= 0) continue;
                if (ShopCatalog.TryGetSupply(staged.CatalogId, out var def) && def is not null)
                {
                    raider.FieldSupplies.Add(new FieldSupplyItem(
                        def.Id,
                        def.Name,
                        def.Category,
                        staged.Quantity,
                        def.HealAmount,
                        def.ReviveAmount,
                        def.ChargeAmount
                    ));
                }
            }
        }

        if (departed)
        {
            raider.Activity = "Ready";
            raider.ActivityEndsAt = readyAt;
        }
        else
        {
            raider.ActivityEndsAt = readyAt;
        }
        return raider;
    }

    public bool Advance(DateTimeOffset now)
    {
        var previousRevision = Revision;
        while (NextActivityAt is { } at && at <= now)
        {
            ProcessPendingHandlerActions(at);

            if (!departed)
            {
                if (raiders.Count == 0) throw new InvalidOperationException("A raid cannot depart without a Raider.");
                departed = true;
                foreach (var raider in OrderedRaiders())
                {
                    SetActivity(raider, "Ready", null, at, at);
                    Emit(at, "Departure", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.departs-from-entry-tunnel-into", raider.Name),
                        location: "Entry Tunnel",
                        activity: "Ready");
                }
            }

            // Admit every same-time arrival before closing any shared search.
            foreach (var raider in OrderedRaiders().Where(raider => raider.Activity == "Traveling" && raider.ActivityEndsAt == at))
            {
                var origin = raider.Location;
                raider.Location = raider.Destination ?? throw new InvalidOperationException("Travel requires a destination.");
                raider.AreaBeat = 0;
                raider.AreaVisit++;
                SetActivity(raider, "Ready", null, at, at);
                CompleteThread(raider, $"travel:{origin}->{raider.Location}", $"Arrived at {raider.Location}.");
                Emit(at, "Arrived", [raider.PlayerId], NarrationContent.Default.Format("simulation.arrives-at", raider.Name, raider.Location),
                    location: raider.Location);
                BeginSearch(raider, at);
            }

            ProcessSharedExtraction(at);

            ResolveRobotRound(at);

            foreach (var opportunity in opportunities.OrderBy(opportunity => opportunity.Id, StringComparer.Ordinal)
                .Where(opportunity => !opportunity.Resolved && opportunity.ClosesAt == at))
            {
                ResolveSearch(opportunity, at);
            }

            foreach (var raider in OrderedRaiders().Where(raider => raider.ActivityEndsAt == at))
            {
                if (raider.Activity is "Surveying" or "Checking" or "Rummaging")
                {
                    Emit(at, "AreaActivityCompleted", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.completed-at-no-loot-or", raider.Name, raider.Activity.ToLowerInvariant(), raider.Location),
                        location: raider.Location, activity: raider.Activity,
                        threadId: $"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}");
                    CompleteThread(raider, $"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}", NarrationContent.Default.Format("simulation.area-check-complete-no-rewards"));
                    SetActivity(raider, "Ready", null, at, at);
                }
                else if (raider.Activity == "Extracting")
                {
                    CompleteExtraction(raider, at);
                }
                else if (raider.Activity == "WaitingToDepart")
                {
                    CompleteExtraction(raider, at);
                }
                else if (raider.Activity is "Downed")
                {
                    SetActivity(raider, "KnockedOut", null, null, at);
                    CompleteThread(raider, $"downed:{raider.PlayerId}", NarrationContent.Default.Format("simulation.succumbed-to-trauma-and-was", raider.Name));
                    var koLine = SurvivalCanonContent.GetRandomKnockedOutLine(at.ToUnixTimeMilliseconds());

                    ItemView? pocketItem = null;
                    if (!string.IsNullOrEmpty(raider.HiddenPocketInstanceId))
                    {
                        pocketItem = raider.Backpack.FirstOrDefault(i => i.InstanceId == raider.HiddenPocketInstanceId);
                    }

                    var lostCount = 0;
                    var lostValue = 0;

                    foreach (var item in raider.Backpack.ToArray())
                    {
                        if (pocketItem is not null && item.InstanceId == pocketItem.InstanceId)
                        {
                            raider.Extracted.Add(item);
                        }
                        else
                        {
                            lostCount++;
                            lostValue += item.Value;
                        }
                    }
                    raider.Backpack.Clear();
                    raider.FieldSupplies.Clear();
                    raider.LostHaulValue = lostValue;
                    raider.LostHaulCount = lostCount;

                    Emit(at, "KnockedOut", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.was-knocked-out-cause", raider.Name, koLine, raider.CauseOfDefeat ?? "Chassis failure"),
                        location: raider.Location,
                        reason: raider.CauseOfDefeat,
                        threadId: $"knockedout:{raider.PlayerId}");

                    if (pocketItem is not null)
                    {
                        Emit(at, "HiddenPocketSalvaged", [raider.PlayerId],
                            NarrationContent.Default.Format("simulation.secret-hidden-pocket-preserved-value", pocketItem.Name, pocketItem.Value),
                            location: raider.Location,
                            itemName: pocketItem.Name,
                            itemValue: pocketItem.Value,
                            itemInstanceId: pocketItem.InstanceId,
                            threadId: $"knockedout:{raider.PlayerId}");
                    }
                    else
                    {
                        Emit(at, "HiddenPocketEmpty", [raider.PlayerId],
                            NarrationContent.Default.Format("simulation.secret-hidden-pocket-was-empty", lostCount, lostValue),
                            location: raider.Location,
                            threadId: $"knockedout:{raider.PlayerId}");
                    }
                }
                else if (raider.Activity is "Packing")
                {
                    CompleteThread(raider, $"packing:{raider.Location}", $"{raider.Name} finished packing.");
                    Emit(at, "PackingCompleted", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.finished-packing-salvage", raider.Name),
                        location: raider.Location,
                        threadId: $"packing:{raider.Location}");
                    SetActivity(raider, "Ready", null, at, at);
                    TryAutonomousConsumableUse(raider, at);
                }
                else if (raider.Activity is "Recovering")
                {
                    CompleteThread(raider, $"recovery:{raider.Location}", NarrationContent.Default.Format("simulation.caught-their-breath", raider.Name));
                    Emit(at, "RecoveryCompleted", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.has-recovered-and-is-ready", raider.Name),
                        location: raider.Location,
                        threadId: $"recovery:{raider.Location}");
                    SetActivity(raider, "Ready", null, at, at);
                    TryAutonomousConsumableUse(raider, at);
                }
            }

            if (sharedExtraction is { Status: "Boarding", BoardingClosesAt: { } closesAt } && closesAt == at)
                sharedExtraction.Status = "Departed";

            if (riskReward) ProcessWorldTime(at);
            foreach (var raider in OrderedRaiders().Where(r => r.NextDiaryAt == at))
            {
                Emit(at, "AreaDiary", [raider.PlayerId],
                    ResidenceDiary(raider), location: raider.Location, activity: raider.Activity,
                    threadId: $"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}");
                raider.NextDiaryAt = raider.Activity == "WaitingForExitDecision" ||
                    at.AddSeconds(20) < raider.ActivityEndsAt ? at.AddSeconds(20) : null;
            }
            foreach (var raider in OrderedRaiders().Where(raider => raider.Activity == "Ready"))
            {
                TryAutonomousConsumableUse(raider, at);
                ContinueJourney(raider, at);
            }
            Revision++;
        }
        return Revision != previousRevision;
    }

    public RaidProjection Project(string playerId, DateTimeOffset serverTime)
    {
        var me = raiders.Single(raider => raider.PlayerId == playerId);
        var currentThread = BuildActiveThread(me, serverTime);
        var recentThreads = me.CompletedThreads.TakeLast(5).ToArray();
        return new RaidProjection(RaidId, Revision, RulesVersion, serverTime, Status,
            StartsAt + AdmissionWindow, NextActivityAt, riskReward ? Zone : "Scrapyard",
            OrderedRaiders().Select(raider => new PlayerView(raider.PlayerId, raider.Name, raider.Disposition,
                raider.Location, raider.Activity, raider.Destination, raider.ActivityEndsAt)).ToArray(),
            locations.Select(location => new LocationView(location.Name, location.Neighbors.ToArray())).ToArray(),
            new CrateView("humming-crate", "Salvage Yard", OpportunityStatus(opportunities.Single(opportunity => opportunity.Id == "humming-crate")),
                items.Count(item => item.Opportunity == "humming-crate" && item.OwnerId is null && !item.Lost)),
            new RobotView(robot.Id, robot.Name, robot.Location, robot.Status, robot.Health, robot.MaxHealth,
                robot.RoundsResolved, robot.NextRoundAt, robot.ParticipantIds.Order(StringComparer.Ordinal).ToArray()),
            me.Health, me.MaxHealth,
            me.Backpack.ToArray(), me.Extracted.ToArray(),
            encounters.Where(encounter => encounter.ParticipantIds.Contains(playerId)).ToArray(),
            robotEncounter is not null && robotEncounter.ParticipantIds.Contains(playerId) ? robotEncounter : null,
            events.Where(raidEvent => raidEvent.ParticipantIds.Contains(playerId)).ToArray(),
            opportunities.Select(opportunity => new OpportunityView(opportunity.Id, opportunity.Location,
                OpportunityStatus(opportunity), opportunity.ClosesAt,
                items.Count(item => item.Opportunity == opportunity.Id && item.OwnerId is null && !item.Lost),
                opportunity.ParticipantIds.Order(StringComparer.Ordinal).ToArray())).ToArray(),
            currentThread,
            recentThreads,
            me.ShieldCharge,
            me.ShieldMaxCharge,
            me.ShieldDurability,
            me.HiddenPocketInstanceId,
            me.FieldSupplies.Where(s => s.Quantity > 0).Select(s => s.ToView()).ToArray(),
            me.CauseOfDefeat,
            me.LostHaulValue,
            me.LostHaulCount,
            riskReward ? Condition : null,
            riskReward ? DeadlineAt : null,
            me.Greed, me.Mood, RaidConditions.Resilience(me.Mood),
            CurrentExtractChance(me, serverTime), RaidConditions.Appetite(me.Greed, me.Mood),
            riskReward ? RaidConditions.Exposure(StartsAt, serverTime) : 0,
            hazardWarnings.ToArray());
    }

    private static string OpportunityStatus(Opportunity opportunity) =>
        opportunity.Resolved ? "Resolved" : opportunity.ClosesAt.HasValue ? "Searching" : "Unsearched";

    private void BeginSearch(SimRaider raider, DateTimeOffset at)
    {
        if (raider.EmergencyExtract)
        {
            ContinueJourney(raider, at);
            return;
        }

        if (raider.Location == robot.Location && robot.Status is "Dormant" or "Active")
        {
            EngageRobot(raider, at);
            return;
        }

        var opportunity = opportunities.SingleOrDefault(opportunity => opportunity.Location == raider.Location);
        if (opportunity is null || !raider.Visited.Add(opportunity.Id)) return;
        if (opportunity.Resolved)
        {
            Emit(at, "OpportunityExhausted", [raider.PlayerId],
                NarrationContent.Default.Format("narrator.finds-already-emptied-no-consolation", raider.Name, opportunity.Id),
                location: raider.Location,
                opportunityId: opportunity.Id);
            return;
        }
        opportunity.Admit(raider.PlayerId, at);
        SetActivity(raider, "Searching", null, opportunity.ClosesAt, at);
        var text = opportunity.Id switch
        {
            "market-locker" => NarrationContent.Default.Format("simulation.a-locker-labeled-staycation-hums"),
            "humming-crate" => NarrationContent.Default.Format("simulation.a-humming-crate-sits-in"),
            _ => NarrationContent.Default.Format("simulation.the-service-depot-locker-contains")
        };
        var threadId = $"search:{opportunity.Id}";
        Emit(at, "Search", [raider.PlayerId], NarrationContent.Default.Format("simulation.searches-at", raider.Name, raider.Location, text),
            location: raider.Location,
            opportunityId: opportunity.Id,
            threadId: threadId);
        if (opportunity.ParticipantIds.Count > 1)
        {
            var participants = OrderedRaiders().Where(participant => opportunity.ParticipantIds.Contains(participant.PlayerId)).ToArray();
            Emit(at, "CrossedPaths", participants.Select(participant => participant.PlayerId).ToArray(),
                NarrationContent.Default.Format("simulation.cross-paths-at-the-existing", string.Join(" and ", participants.Select(participant => participant.Name)), opportunity.Location),
                location: opportunity.Location,
                opportunityId: opportunity.Id,
                threadId: threadId);
        }
    }

    private void EngageRobot(SimRaider raider, DateTimeOffset at)
    {
        var threadId = $"robot:{robot.Id}";
        if (robot.Status == "Dormant")
        {
            robot.Status = "Active";
            robot.StartedAt = at;
            robot.NextRoundAt = at + RobotRoundInterval;
            Emit(at, "RobotCombatStarted", [raider.PlayerId],
                NarrationContent.Default.Format("simulation.encounters-at", raider.Name, robot.Name, robot.Location, robot.Flavor),
                location: robot.Location,
                threadId: threadId);
        }

        if (!robot.ParticipantIds.Add(raider.PlayerId))
        {
            throw new InvalidOperationException("A Raider cannot join the same robot encounter twice.");
        }
        robot.DamageByPlayer.Add(raider.PlayerId, new RobotDamage(at));
        SetActivity(raider, "FightingRobot", null, robot.NextRoundAt, at);
        Emit(at, "RobotParticipantJoined", OrderedRobotParticipantIds(),
            NarrationContent.Default.Format("simulation.joins-the-fight-against-the", raider.Name, robot.Name, robot.Health, robot.MaxHealth),
            location: robot.Location,
            threadId: threadId);
    }

    private void ResolveRobotRound(DateTimeOffset at)
    {
        if (robot.Status != "Active" || robot.NextRoundAt != at) return;

        var participants = OrderedRaiders()
            .Where(raider => robot.ParticipantIds.Contains(raider.PlayerId)
                && raider.Activity == "FightingRobot"
                && raider.Location == robot.Location
                && raider.ActivityEndsAt == at)
            .ToArray();
        if (participants.Length == 0)
        {
            FinishRobotEncounter(at, "Escaped", [], NarrationContent.Default.Format("simulation.has-no-remaining-fighters-no", robot.Name));
            var abandoned = opportunities.Single(o => o.Id == "depot-locker");
            abandoned.Resolved = true;
            foreach (var item in items.Where(i => i.Opportunity == abandoned.Id && i.OwnerId is null)) item.Lost = true;
            return;
        }

        robot.RoundsResolved++;
        var remainingHealth = robot.Health;
        foreach (var participant in participants)
        {
            var damage = Math.Min(RobotDamagePerRaider, remainingHealth);
            robot.DamageByPlayer[participant.PlayerId].Dealt += damage;
            remainingHealth -= damage;
        }
        robot.Health = remainingHealth;

        if (robot.Health == 0)
        {
            FinishRobotEncounter(at, "Defeated", participants,
                NarrationContent.Default.Format("simulation.is-disabled-the-service-depot", robot.Name));
            foreach (var participant in participants) BeginSearch(participant, at);
            return;
        }

        foreach (var participant in participants)
        {
            var rawDamage = riskReward ? dangerProfile.RobotDamage : RobotDamageToRaider;
            ApplyDamageToRaider(participant, rawDamage, $"{robot.Name} kinetic swipe", at, threadId: $"robot:{robot.Id}");
            robot.DamageByPlayer[participant.PlayerId].Taken += rawDamage;
            if (participant.Activity == "Downed")
            {
                robot.ParticipantIds.Remove(participant.PlayerId);
            }
            else
            {
                TryAutonomousConsumableUse(participant, at, threadId: $"robot:{robot.Id}");
            }
        }

        var activeFighters = participants.Where(p => p.Activity == "FightingRobot").ToArray();

        Emit(at, "RobotRound", participants.Select(p => p.PlayerId).ToArray(),
            NarrationContent.Default.Format("simulation.has-health-it-deals-raw", robot.Name, robot.Health, robot.MaxHealth, (riskReward ? dangerProfile.RobotDamage : RobotDamageToRaider)),
            location: robot.Location,
            robotDamageTaken: riskReward ? dangerProfile.RobotDamage : RobotDamageToRaider,
            robotRemainingHealth: robot.Health,
            threadId: $"robot:{robot.Id}");

        if (activeFighters.Length == 0)
        {
            robot.Status = "Escaped";
            robot.NextRoundAt = null;
            var depot = opportunities.Single(opportunity => opportunity.Id == "depot-locker");
            depot.Resolved = true;
            foreach (var item in items.Where(item => item.Opportunity == depot.Id && item.OwnerId is null))
                item.Lost = true;
            FinishRobotEncounter(at, "Escaped", participants,
                NarrationContent.Default.Format("simulation.overran-all-fighters-the-service", robot.Name));
            return;
        }

        if (robot.RoundsResolved >= RobotRoundLimit)
        {
            robot.Status = "Escaped";
            robot.NextRoundAt = null;
            var depot = opportunities.Single(opportunity => opportunity.Id == "depot-locker");
            depot.Resolved = true;
            foreach (var item in items.Where(item => item.Opportunity == depot.Id && item.OwnerId is null))
                item.Lost = true;
            foreach (var participant in activeFighters) SetActivity(participant, "Ready", null, at, at);
            FinishRobotEncounter(at, "Escaped", participants,
                NarrationContent.Default.Format("simulation.escapes-after-rounds-the-service", robot.Name, robot.RoundsResolved));
            return;
        }

        robot.NextRoundAt = at + RobotRoundInterval;
        foreach (var participant in activeFighters)
            SetActivity(participant, "FightingRobot", null, robot.NextRoundAt, at);
    }

    private void FinishRobotEncounter(DateTimeOffset at, string resolution, SimRaider[] participants, string reason)
    {
        robot.Status = resolution;
        robot.NextRoundAt = null;
        foreach (var participant in participants)
        {
            if (participant.Activity is not ("Downed" or "KnockedOut"))
            {
                SetActivity(participant, "Ready", null, at, at);
            }
            CompleteThread(participant, $"robot:{robot.Id}", $"{robot.Name} {resolution}.");
        }
        var historicalParticipants = OrderedRaiders().Where(r => robot.DamageByPlayer.ContainsKey(r.PlayerId)).ToArray();
        var participantIds = historicalParticipants.Select(r => r.PlayerId).ToArray();
        var damage = historicalParticipants.Select(participant =>
        {
            var totals = robot.DamageByPlayer[participant.PlayerId];
            return new RobotDamageView(participant.PlayerId, totals.JoinedAt, totals.Dealt, totals.Taken);
        }).ToArray();
        robotEncounter = new RobotEncounterView($"{RaidId}:{robot.Id}", robot.StartedAt!.Value, at,
            robot.Location, resolution, participantIds, damage, reason);
        Emit(at, $"Robot{resolution}", participantIds, reason,
            location: robot.Location,
            threadId: $"robot:{robot.Id}");
    }

    private string[] OrderedRobotParticipantIds() =>
        OrderedRaiders().Where(raider => robot.ParticipantIds.Contains(raider.PlayerId))
            .Select(raider => raider.PlayerId).ToArray();

    private void ResolveSearch(Opportunity opportunity, DateTimeOffset at)
    {
        var participants = OrderedRaiders().Where(raider => opportunity.ParticipantIds.Contains(raider.PlayerId)).ToArray();
        if (participants.Length == 0)
        {
            opportunity.Resolved = true;
            foreach (var item in items.Where(i => i.Opportunity == opportunity.Id)) item.Lost = true;
            return;
        }
        if (participants.Any(raider =>
            raider.Location != opportunity.Location || raider.Activity != "Searching" || raider.ActivityEndsAt != at))
        {
            throw new InvalidOperationException("Search participants must be present and searching at the shared deadline.");
        }
        if (riskReward)
        {
            var generated = catalog.CreateOpportunityItems(RaidId, seed, opportunity.Id, Condition.Danger,
                RaidConditions.Exposure(StartsAt, at), participants.Average(r => RaidConditions.Appetite(r.Greed, r.Mood)));
            items.RemoveAll(i => i.Opportunity == opportunity.Id);
            items.AddRange(generated.Select(i => new WorldItem(i.OpportunityId, i.Item)));
            foreach (var participant in participants)
                ChangePersonality(participant, 2, 1, at, "Completed salvage search");
        }
        var threadId = $"search:{opportunity.Id}";
        if (opportunity.Id != "humming-crate")
        {
            var awards = Distribute(opportunity.Id, participants);
            foreach (var raider in participants)
            {
                CompleteThread(raider, threadId, $"{opportunity.Location} search completed.");
            }
            Emit(at, "SearchCompleted", participants.Select(raider => raider.PlayerId).ToArray(),
                NarrationContent.Default.Format("simulation.search-completed", opportunity.Location, string.Join("; ", awards.Select(award => $"{award.RecipientName} recovered {award.ItemName}"))),
                location: opportunity.Location,
                opportunityId: opportunity.Id,
                threadId: threadId);
            foreach (var award in awards)
            {
                var recipient = participants.First(p => p.PlayerId == award.RecipientId);
                var awardedItem = recipient.Backpack.First(i => i.InstanceId == award.InstanceId);
                Emit(at, "ItemAllocated", [award.RecipientId],
                    NarrationContent.Default.Format("narrator.recovered-value-securing-cargo-with", award.RecipientName, award.ItemName, awardedItem.Value),
                    location: opportunity.Location,
                    itemName: award.ItemName,
                    itemValue: awardedItem.Value,
                    itemInstanceId: award.InstanceId,
                    opportunityId: opportunity.Id,
                    threadId: threadId,
                    channel: "Diary");
            }
            foreach (var raider in participants)
            {
                SetActivity(raider, "Packing", null, at.AddSeconds(2), at);
                Emit(at, "PackingStarted", [raider.PlayerId],
                    NarrationContent.Default.Format("narrator.begins-packing-salvage-into-backpack", raider.Name, raider.Backpack.LastOrDefault()?.Value ?? 0),
                    location: raider.Location,
                    durationSeconds: 2,
                    threadId: $"packing:{raider.Location}");
            }
            opportunity.Resolved = true;
            return;
        }
        var factors = participants.Select(raider => new ContestFactorView(raider.PlayerId, raider.Name,
            raider.Disposition, raider.Greed, raider.Aptitude, raider.Backpack.Sum(item => item.Value),
            raider.Initiative, Score(raider))).ToArray();
        var contested = participants.Length > 1 && participants.Any(raider => raider.Disposition == "Hoarder");
        var recipients = contested
            ? participants.OrderByDescending(Score).ThenBy(raider => raider.Slot, StringComparer.Ordinal).Take(1).ToArray()
            : participants;
        var allocation = Distribute(opportunity.Id, recipients);
        var resolution = participants.Length == 1 ? "SoloSearch" : contested ? "Contest" : "Cooperation";
        var reason = contested
            ? NarrationContent.Default.Format("simulation.a-hoarder-contests-the-salvage")
            : participants.Length == 1
                ? NarrationContent.Default.Format("simulation.only-one-eligible-raider-was")
                : NarrationContent.Default.Format("simulation.both-raiders-cooperate-lower-backpack");
        encounters.Add(new EncounterView($"{RaidId}:salvage", at, "Salvage Yard", resolution,
            participants.Select(raider => raider.PlayerId).ToArray(), factors, allocation, reason));
        foreach (var raider in participants)
        {
            CompleteThread(raider, threadId, $"{resolution}: {string.Join("; ", allocation.Select(award => $"{award.RecipientName} recovered {award.ItemName}"))}.");
        }
        Emit(at, "EncounterResolved", participants.Select(raider => raider.PlayerId).ToArray(),
            $"{resolution}: {string.Join("; ", allocation.Select(award => $"{award.RecipientName} recovered {award.ItemName}"))}. {reason}",
            location: "Salvage Yard",
            opportunityId: opportunity.Id,
            reason: reason,
            threadId: threadId);
        foreach (var award in allocation)
        {
            var recipient = participants.First(p => p.PlayerId == award.RecipientId);
            var awardedItem = recipient.Backpack.First(i => i.InstanceId == award.InstanceId);
            Emit(at, "ItemAllocated", [award.RecipientId],
                NarrationContent.Default.Format("narrator.recovered-value-securing-cargo-with", award.RecipientName, award.ItemName, awardedItem.Value),
                location: opportunity.Location,
                itemName: award.ItemName,
                itemValue: awardedItem.Value,
                itemInstanceId: award.InstanceId,
                opportunityId: opportunity.Id,
                threadId: threadId,
                channel: "Diary");
        }
        foreach (var raider in participants)
        {
            var packSeconds = contested ? 8 : 2;
            SetActivity(raider, contested ? "Recovering" : "Packing", null, at.AddSeconds(packSeconds), at);
            if (contested)
            {
                Emit(at, "RecoveryStarted", [raider.PlayerId],
                    NarrationContent.Default.Format("narrator.catching-breath-and-recovering-from", raider.Name),
                    location: raider.Location,
                    durationSeconds: packSeconds,
                    threadId: $"recovery:{raider.Location}");
            }
            else
            {
                Emit(at, "PackingStarted", [raider.PlayerId],
                    NarrationContent.Default.Format("narrator.begins-packing-salvage-into-backpack", raider.Name, raider.Backpack.LastOrDefault()?.Value ?? 0),
                    location: raider.Location,
                    durationSeconds: packSeconds,
                    threadId: $"packing:{raider.Location}");
            }
        }
        opportunity.Resolved = true;
    }

    private static int Score(SimRaider raider) =>
        raider.Greed + raider.Aptitude * 3 + raider.Initiative - raider.Backpack.Sum(item => item.Value) / 5;

    private void JoinOrCallSharedExtraction(SimRaider raider, DateTimeOffset at)
    {
        if (sharedExtraction is { Status: "Calling" } calling)
        {
            SetActivity(raider, "WaitingForVehicle", null, calling.ArrivalAt, at);
            EmitExtractionJoin(raider, calling.Vehicle, at);
            return;
        }
        if (sharedExtraction is { Status: "Boarding", BoardingClosesAt: { } closesAt } boarding &&
            at < closesAt)
        {
            SetActivity(raider, "WaitingToDepart", null, closesAt, at);
            EmitExtractionJoin(raider, boarding.Vehicle, at);
            return;
        }

        var arrivalAt = at.AddSeconds(ExtractionVehicleArrivalSeconds);
        sharedExtraction = new SharedExtractionVehicle(raider.Name, raider.ExitVehicle, arrivalAt);
        SetActivity(raider, "CallingExtraction", null, arrivalAt, at);
        var threadId = $"extract:{raider.Location}";
        Emit(at, "ExtractionStarted", [raider.PlayerId],
            NarrationContent.Default.Format("simulation.calls-vehicle-with-four-sided-doors",
                raider.ExitVehicle, ExtractionVehicleArrivalSeconds),
            location: raider.Location,
            durationSeconds: ExtractionVehicleArrivalSeconds,
            threadId: threadId);
        var activeRaiders = ActiveExtractionParticipants();
        Emit(at, "SharedExtractionCalled", activeRaiders,
            NarrationContent.Default.Format("simulation.shared-extraction-vehicle-called",
                raider.Name, raider.ExitVehicle),
            location: raider.Location,
            durationSeconds: ExtractionVehicleArrivalSeconds,
            threadId: threadId);
    }

    private void ProcessSharedExtraction(DateTimeOffset at)
    {
        if (sharedExtraction is not { Status: "Calling" } vehicle || vehicle.ArrivalAt != at)
            return;

        vehicle.Status = "Boarding";
        vehicle.BoardingClosesAt = at.AddSeconds(ExtractionBoardingWindowSeconds);
        foreach (var raider in OrderedRaiders().Where(raider =>
            raider.EmergencyExtract && raider.Location == "Extraction Platform" &&
            raider.Activity is "CallingExtraction" or "WaitingForVehicle"))
        {
            SetActivity(raider, "WaitingToDepart", null, vehicle.BoardingClosesAt, at);
        }
        Emit(at, "ExtractionVehicleArrived", ActiveExtractionParticipants(),
            NarrationContent.Default.Format("simulation.extraction-vehicle-arrived",
                vehicle.CalledBy, vehicle.Vehicle, ExtractionBoardingWindowSeconds),
            location: "Extraction Platform", activity: "WaitingToDepart",
            durationSeconds: ExtractionBoardingWindowSeconds,
            threadId: "extract:Extraction Platform");
    }

    private void EmitExtractionJoin(SimRaider raider, string vehicle, DateTimeOffset at)
    {
        Emit(at, "ExtractionJoined", ActiveExtractionParticipants(),
            NarrationContent.Default.Format("simulation.joins-shared-extraction",
                raider.Name, vehicle),
            location: "Extraction Platform",
            threadId: "extract:Extraction Platform");
    }

    private string[] ActiveExtractionParticipants() =>
        OrderedRaiders()
            .Where(raider => raider.Activity is not ("Extracted" or "KnockedOut" or "Downed"))
            .Select(raider => raider.PlayerId)
            .ToArray();

    private ItemAwardView[] Distribute(string opportunity, SimRaider[] recipients)
    {
        if (recipients.Length == 0)
        {
            throw new InvalidOperationException("A loot opportunity requires a present Raider.");
        }
        var ordered = recipients.OrderBy(raider => raider.Backpack.Sum(item => item.Value))
            .ThenBy(raider => raider.Slot, StringComparer.Ordinal).ToArray();
        var awards = new List<ItemAwardView>();
        var available = items.Where(item => item.Opportunity == opportunity && item.OwnerId is null && !item.Lost)
            .OrderByDescending(item => item.Item.Value).ToArray();
        for (var index = 0; index < available.Length; index++)
        {
            var recipient = ordered[index % ordered.Length];
            var item = available[index];
            item.OwnerId = recipient.PlayerId;
            recipient.Backpack.Add(item.Item);
            awards.Add(new ItemAwardView(item.Item.InstanceId, item.Item.Name, recipient.PlayerId, recipient.Name));
        }
        return awards.ToArray();
    }

    private SimRaider[] OrderedRaiders() => raiders.OrderBy(raider => raider.Slot, StringComparer.Ordinal).ToArray();
    private void ContinueJourney(SimRaider raider, DateTimeOffset at)
    {
        if (!fastPacing && !raider.EmergencyExtract && raider.Location != "Entry Tunnel" && raider.AreaBeat < 3)
        {
            var activity = new[] { "Surveying", "Checking", "Rummaging" }[raider.AreaBeat++];
            var residenceSeconds = 60 + Factor(raider.Slot, $"area:{raider.AreaVisit}:{activity}") % 6;
            SetActivity(raider, activity, null, at.AddSeconds(residenceSeconds), at);
            var task = activity switch
            {
                "Surveying" => NarrationContent.Default.Format("simulation.surveying-the-approach-and-exit"),
                "Checking" => NarrationContent.Default.Format("simulation.checking-footing-straps-and-cover"),
                _ => NarrationContent.Default.Format("simulation.rummaging-through-empty-fittings-for")
            };
            Emit(at, "AreaActivityStarted", [raider.PlayerId],
                NarrationContent.Default.Format("simulation.begins-at-s-reconnaissance-only", raider.Name, task, raider.Location, residenceSeconds),
                location: raider.Location, activity: activity, durationSeconds: residenceSeconds,
                threadId: $"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}");
            return;
        }
        if (raider.EmergencyExtract && raider.Location != "Extraction Platform" && raider.Route.Count == 0)
        {
            var extractionPath = GetPathToExtraction(raider.Location);
            foreach (var step in extractionPath) raider.Route.Enqueue(step);
        }

        if (raider.Route.Count == 0)
        {
            if (raider.Location != "Extraction Platform") throw new InvalidOperationException("A journey must end at extraction.");
            if (riskReward && !raider.EmergencyExtract)
            {
                SetActivity(raider, "WaitingForExitDecision", null, nextDecisionAt, at);
                raider.NextDiaryAt = at.AddSeconds(20);
                return;
            }
            if (riskReward)
            {
                JoinOrCallSharedExtraction(raider, at);
            }
            else
            {
                SetActivity(raider, "Extracting", null, at.AddSeconds(10), at);
                Emit(at, "ExtractionStarted", [raider.PlayerId],
                    NarrationContent.Default.Format("narrator.calls-extraction-the-shuttle-requests", raider.Name),
                    location: raider.Location,
                    durationSeconds: 10,
                    threadId: $"extract:{raider.Location}");
            }
            return;
        }
        var destination = raider.Route.Dequeue();
        if (!locations.Single(location => location.Name == raider.Location).Neighbors.Contains(destination))
        {
            throw new InvalidOperationException("Autonomous routing must use a connected edge.");
        }
        var podDeployment = !fastPacing && raider.AreaVisit == 0 &&
            raider.Location == "Entry Tunnel" && destination == "Market";
        var duration = podDeployment
            ? PodDeploymentSeconds
            : 8 + Factor(raider.Slot, $"travel:{raider.Location}:{destination}") % 5;
        var travelThreadId = $"travel:{raider.Location}->{destination}";
        SetActivity(raider, "Traveling", destination, at.AddSeconds(duration), at);
        Emit(at, "Travel", [raider.PlayerId],
            podDeployment
                ? NarrationContent.Default.Format("simulation.pod-launches-from-desperanza", raider.Name, duration)
                : NarrationContent.Default.Format("simulation.leaves-for-eta-s-navigation", raider.Name, raider.Location, destination, duration),
            location: raider.Location,
            destination: destination,
            durationSeconds: duration,
            threadId: travelThreadId);
    }

    private double CurrentExtractChance(SimRaider raider, DateTimeOffset at)
    {
        if (!riskReward || raider.Activity is "Extracted" or "KnockedOut" or "Downed" or "Extracting" or
            "CallingExtraction" or "WaitingToDepart" ||
            raider.EmergencyExtract) return 0;
        var margin = (DeadlineAt - at).TotalSeconds - ExitReserve(raider);
        var urgent = margin <= 180;
        if (!urgent && (at - raider.AdmittedAt).TotalSeconds < dangerProfile.MinimumExtractSeconds &&
            !(raider.Health <= 18 && raider.Backpack.Count > 0)) return 0;
        return RaidConditions.ExtractChance(raider.Greed, raider.Health, urgent, 1 - margin / 180);
    }

    private static int ExitReserve(SimRaider raider) =>
        ExtractionVehicleArrivalSeconds + ExtractionBoardingWindowSeconds +
        GetPathToExtraction(raider.Destination ?? raider.Location).Length * 12 +
        DecisionIntervalSeconds / 2 +
        (raider.Activity == "Traveling" ? 12 : 0) +
        (raider.AreaVisit == 0 && raider.Location == "Entry Tunnel" ? PodDeploymentSeconds : 0);

    private void CompleteExtraction(SimRaider raider, DateTimeOffset at)
    {
        raider.Extracted.AddRange(raider.Backpack);
        raider.Backpack.Clear();
        SetActivity(raider, "Extracted", null, null, at);
        CompleteThread(raider, $"extract:{raider.Location}", $"{raider.Name} extracted successfully.");
        Emit(at, "Extracted", [raider.PlayerId],
            NarrationContent.Default.Format("narrator.extracts-the-loot-made-it", raider.Name),
            location: raider.Location,
            threadId: $"extract:{raider.Location}");
    }

    private void ChangePersonality(SimRaider raider, int greed, int mood, DateTimeOffset at, string reason)
    {
        var oldGreed = raider.Greed;
        var oldMood = raider.Mood;
        raider.Greed = Math.Clamp(raider.Greed + greed, 0, 100);
        raider.Mood = Math.Clamp(raider.Mood + mood, -10, 10);
        if (oldGreed == raider.Greed && oldMood == raider.Mood) return;
        Emit(at, "PersonalityChanged", [raider.PlayerId],
            NarrationContent.Default.Format("simulation.greed-mood-resilience", reason, oldGreed, raider.Greed, oldMood, raider.Mood, RaidConditions.Resilience(raider.Mood)),
            location: raider.Location, reason: reason);
    }

    private void ProcessWorldTime(DateTimeOffset at)
    {
        if (!deadlineResolved && at == DeadlineAt)
        {
            deadlineResolved = true;
            foreach (var raider in OrderedRaiders().Where(r => r.Activity is not ("Extracted" or "KnockedOut")))
            {
                LeaveSearch(raider);
                raider.CauseOfDefeat = NarrationContent.Default.Format("simulation.world-deadline-expired-before-extraction");
                SetActivity(raider, "Downed", null, at, at);
                Emit(at, "DeadlineExpired", [raider.PlayerId], raider.CauseOfDefeat, location: raider.Location);
            }
        }
        if (nextForecastAt == at)
        {
            nextForecastAt = at.AddSeconds(60);
            nextHazardAt = at.AddSeconds(10);
            hazardWarnings.Clear();
            foreach (var location in locations.Where(l => l.Name != "Entry Tunnel"))
                if (RaidConditions.Roll(seed, $"hazard:{at.ToUnixTimeSeconds()}:{location.Name}") <
                    RaidConditions.HazardOdds(Condition.Danger, RaidConditions.Exposure(StartsAt, at)))
                {
                    var warning = new HazardWarningView(location.Name, nextHazardAt.Value, dangerProfile.HazardDamage);
                    hazardWarnings.Add(warning);
                    var present = OrderedRaiders().Where(r => r.Location == location.Name &&
                        r.Activity is not ("Traveling" or "Extracted" or "KnockedOut" or "Downed")).ToArray();
                    if (present.Length > 0)
                        Emit(at, "HazardWarning", present.Select(r => r.PlayerId).ToArray(),
                            NarrationContent.Default.Format("simulation.exposed-footing-at-impact-in", Condition.Name, location.Name, warning.RawDamage),
                            location: location.Name, durationSeconds: 10, reason: Condition.Id);
                }
        }
        if (nextHazardAt == at)
        {
            foreach (var warning in hazardWarnings)
            {
                var present = OrderedRaiders().Where(r => r.Location == warning.Location &&
                    r.Activity is not ("Traveling" or "Downed" or "Extracted" or "KnockedOut")).ToArray();
                if (present.Length == 0) continue;
                Emit(at, "ConditionImpact", present.Select(r => r.PlayerId).ToArray(),
                    NarrationContent.Default.Format("simulation.affects-actual-occupants-of-raw", Condition.Name, warning.Location, warning.RawDamage),
                    location: warning.Location, reason: Condition.Id);
                foreach (var raider in present)
                {
                    ApplyDamageToRaider(raider, warning.RawDamage, Condition.Name, at);
                    TryAutonomousConsumableUse(raider, at);
                }
            }
            hazardWarnings.Clear();
            nextHazardAt = null;
        }
        if (nextDecisionAt != at) return;
        nextDecisionAt = at.AddSeconds(DecisionIntervalSeconds);
        foreach (var raider in OrderedRaiders().Where(r => r.Activity is not ("Extracted" or "KnockedOut" or "Downed")))
        {
            if ((at - StartsAt).TotalSeconds % 60 == 0 && !raider.EmergencyExtract && raider.Activity != "Extracting")
                ChangePersonality(raider, 1, 0, at, "Staying deeper");
            if (raider.Activity == "WaitingForExitDecision")
                raider.ActivityEndsAt = nextDecisionAt;
            var chance = CurrentExtractChance(raider, at);
            if (chance == 0) continue;
            var urgent = (DeadlineAt - at).TotalSeconds - ExitReserve(raider) <= 180;
            var exits = RaidConditions.Roll(seed, $"exit:{raider.Slot}:{at.ToUnixTimeSeconds()}") < chance;
            Emit(at, "ExtractionDecision", [raider.PlayerId],
                NarrationContent.Default.Format("simulation.exit-chance-greed", (urgent ? "Deadline urgency" : "Greed Check"), chance, (exits ? "heading for extraction" : "staying for now"), raider.Greed),
                location: raider.Location, reason: urgent ? "DeadlineUrgency" : "GreedCheck");
            if (!exits) continue;
            raider.EmergencyExtract = true;
            InterruptForExit(raider, at);
        }
    }

    private static void LeaveSearch(SimRaider raider, IEnumerable<Opportunity> opportunities)
    {
        foreach (var opportunity in opportunities.Where(o => !o.Resolved))
            opportunity.ParticipantIds.Remove(raider.PlayerId);
    }

    private void LeaveSearch(SimRaider raider) => LeaveSearch(raider, opportunities);

    private void InterruptForExit(SimRaider raider, DateTimeOffset at)
    {
        if (raider.Activity is "Downed" or "KnockedOut" or "Extracted" or "Extracting" or
            "CallingExtraction" or "WaitingToDepart") return;
        LeaveSearch(raider);
        raider.Route.Clear();
        foreach (var node in GetPathToExtraction(raider.Destination ?? raider.Location))
            raider.Route.Enqueue(node);
        if (raider.Activity == "Traveling") return;
        if (BuildActiveThread(raider, at) is { } thread)
            CompleteThread(raider, thread.ThreadId, NarrationContent.Default.Format("simulation.interrupted-by-extraction-decision-no"));
        Emit(at, "ActivityInterrupted", [raider.PlayerId],
            NarrationContent.Default.Format("simulation.interrupted-for-extraction-shared-deadlines", raider.Activity),
            location: raider.Location, activity: raider.Activity);
        SetActivity(raider, "Ready", null, at, at);
    }

    private string ResidenceDiary(SimRaider raider)
    {
        var tasks = RaiderCommsContent.ResidenceTasks(raider.Activity, raider.Location);
        var task = tasks[raider.DiaryBeat++ % tasks.Count];
        var other = OrderedRaiders().FirstOrDefault(r => r.PlayerId != raider.PlayerId &&
            r.Location == raider.Location && r.Activity is not ("Traveling" or "Extracted" or "KnockedOut"));
        var encounter = other is null ? "" : NarrationContent.Default.Format("simulation.is-also-actually-present-here", other.Name);
        return NarrationContent.Default.Format("simulation.at-no-loot-damage-or", raider.Name, task, raider.Location, encounter);
    }

    private static void SetActivity(SimRaider raider, string activity, string? destination, DateTimeOffset? endsAt, DateTimeOffset? startedAt = null)
    {
        raider.Activity = activity;
        raider.Destination = destination;
        raider.ActivityEndsAt = endsAt;
        raider.NextDiaryAt = activity is "Surveying" or "Checking" or "Rummaging" && startedAt.HasValue
            ? startedAt.Value.AddSeconds(20) : null;
        if (startedAt.HasValue)
        {
            raider.ActivityStartedAt = startedAt.Value;
        }
    }

    private void Emit(
        DateTimeOffset at,
        string kind,
        string[] participants,
        string text,
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
        string channel = "Diary")
    {
        var sequence = events.Count + 1;
        var participant = raiders.FirstOrDefault(r => r.PlayerId == participants.FirstOrDefault());
        IReadOnlyDictionary<string, string>? details = null;
        if (participant is not null)
        {
            var key = RaiderCommsContent.PoolKey(kind, activity);
            var recipients = raiders.Where(r => participants.Contains(r.PlayerId)).ToArray();
            var voiceIndex = recipients.Max(r => r.VoiceCounts.GetValueOrDefault(key));
            var disposition = participants.Length == 1 ? participant.Disposition : null;
            var voice = RaiderCommsContent.Line(kind, activity, voiceIndex, disposition, rotate: true);
            while (recipients.Any(r => r.LastVoice == voice))
                voice = RaiderCommsContent.Line(kind, activity, ++voiceIndex, disposition, rotate: true);
            foreach (var recipient in recipients)
            {
                recipient.VoiceCounts[key] = voiceIndex + 1;
                recipient.LastVoice = voice;
            }
            details = new Dictionary<string, string> { ["VoiceIndex"] = voiceIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) };
        }
        var evt = new RaidEvent(sequence, at, kind, participants, location, activity, destination,
            durationSeconds, healthDelta, robotDamageDealt, robotDamageTaken, robotRemainingHealth,
            itemName, itemValue, itemInstanceId, opportunityId, reason, ThreadId: threadId,
            Channel: channel, Details: details, OriginalText: text);
        var projection = participant is null ? null : new RaiderProjection(participant.PlayerId, participant.Slot,
            participant.Name, participant.Disposition, participant.Health, participant.MaxHealth,
            participant.Greed, participant.Aptitude, participant.Initiative, participant.Backpack.Sum(i => i.Value));
        var formatted = narrator.FormatEvent(evt, participants.Length == 1 ? projection : null);
        events.Add(new RaidEventView(
            sequence,
            at,
            kind,
            participants,
            formatted,
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
            channel,
            FactsText: text
        ));
    }

    internal void MarkNarrationUnavailable(int sequence)
    {
        var index = events.FindIndex(e => e.Sequence == sequence);
        if (index < 0 || events[index].EnrichmentStatus != "NotRequested" ||
            events[index].EnrichmentReason == "ModelUnavailable") return;
        events[index] = events[index] with { EnrichmentReason = "ModelUnavailable" };
        Revision++;
    }

    internal bool TryQueueNarration(int sequence)
    {
        if (!queuedNarration.Add(sequence)) return false;
        var index = events.FindIndex(e => e.Sequence == sequence);
        if (events.Any(e => e.EnrichmentStatus == "Pending"))
        {
            events[index] = events[index] with { EnrichmentReason = "LiveBudgetBusy" };
            Revision++;
            return false;
        }
        events[index] = events[index] with { EnrichmentStatus = "Pending", EnrichmentReason = null };
        Revision++;
        return true;
    }

    internal void FinishNarration(int sequence, string status, string? reason = null)
    {
        var index = events.FindIndex(e => e.Sequence == sequence);
        if (index < 0 || events[index].EnrichmentStatus != "Pending") return;
        events[index] = events[index] with { EnrichmentStatus = status, EnrichmentReason = reason };
        Revision++;
    }

    internal bool ApplyNarration(int sequence, string expectedText, string flavor)
    {
        var index = events.FindIndex(e => e.Sequence == sequence);
        if (index < 0 || events[index].Text != expectedText || events[index].Flavor is not null ||
            events[index].EnrichmentStatus != "Pending") return false;
        var original = events[index];
        events[index] = original with { Flavor = flavor, NarrativeSource = "Generated", EnrichmentStatus = "Ready" };
        foreach (var raider in raiders)
        {
            for (var i = 0; i < raider.CompletedThreads.Count; i++)
            {
                var thread = raider.CompletedThreads[i];
                if (thread.ThreadId != original.ThreadId &&
                    !(original.Kind.StartsWith("Robot", StringComparison.Ordinal) && thread.Location == original.Location))
                    continue;
                raider.CompletedThreads[i] = thread with
                {
                    Logs = thread.Logs.Select(l => l == original.Text ? events[index].NarrationText : l).ToArray()
                };
            }
        }
        Revision++;
        return true;
    }

    private ActiveThreadView? BuildActiveThread(SimRaider raider, DateTimeOffset serverTime)
    {
        if (raider.Activity == "WaitingForExitDecision")
            return new ActiveThreadView($"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}",
                "AreaActivity", "Weighing the exit", raider.Location, "Active",
                raider.ActivityStartedAt, raider.ActivityEndsAt, 30,
                NarrationContent.Default.Format("simulation.no-new-rewards-greed-check"),
                events.Where(e => e.ParticipantIds.Contains(raider.PlayerId) &&
                    e.At >= raider.ActivityStartedAt && e.Kind is "AreaDiary" or "ExtractionDecision")
                    .Select(e => e.NarrationText).ToArray());
        if (raider.Activity is "Surveying" or "Checking" or "Rummaging")
        {
            var threadId = $"area:{raider.PlayerId}:{raider.AreaVisit}:{raider.AreaBeat}";
            return new ActiveThreadView(threadId, "AreaActivity", raider.Activity, raider.Location, "Active",
                raider.ActivityStartedAt, raider.ActivityEndsAt,
                (int)((raider.ActivityEndsAt!.Value - raider.ActivityStartedAt).TotalSeconds),
                NarrationContent.Default.Format("simulation.nonreward-reconnaissance-shared-loot-remains"),
                events.Where(e => e.ThreadId == threadId && e.ParticipantIds.Contains(raider.PlayerId))
                    .Select(e => e.NarrationText).ToArray());
        }
        if (raider.Activity == "Traveling")
        {
            var threadId = $"travel:{raider.Location}->{raider.Destination}";
            var total = (int)Math.Max(1, ((raider.ActivityEndsAt ?? serverTime) - raider.ActivityStartedAt).TotalSeconds);
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => (e.ThreadId == threadId || e.Destination == raider.Destination) && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Travel", $"Travel to {raider.Destination}", raider.Location,
                "Active", raider.ActivityStartedAt, raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.eta-s-navigation-guided-by", remaining), logs);
        }
        if (raider.Activity == "Searching")
        {
            var opp = opportunities.FirstOrDefault(o => o.Location == raider.Location);
            var threadId = $"search:{opp?.Id ?? raider.Location}";
            var total = opp?.SearchSeconds ?? 10;
            var remaining = opp?.ClosesAt.HasValue == true
                ? Math.Max(0, (int)Math.Ceiling((opp.ClosesAt.Value - serverTime).TotalSeconds))
                : 0;
            var title = opp?.Id switch
            {
                "market-locker" => NarrationContent.Default.Format("simulation.salvage-search-staycation-locker"),
                "humming-crate" => NarrationContent.Default.Format("simulation.salvage-search-humming-crate"),
                _ => NarrationContent.Default.Format("simulation.salvage-search-service-depot-locker")
            };
            var logs = events.Where(e => (e.ThreadId == threadId || e.OpportunityId == opp?.Id) && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Search", title, raider.Location,
                "Active", raider.ActivityStartedAt, opp?.ClosesAt ?? raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.searching-container-deadline-in-s", remaining), logs);
        }
        if (raider.Activity == "FightingRobot")
        {
            var threadId = $"robot:{robot.Id}";
            var remaining = robot.NextRoundAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((robot.NextRoundAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => (e.ThreadId == threadId || e.Kind.StartsWith("Robot", StringComparison.Ordinal)) && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "RobotCombat", NarrationContent.Default.Format("simulation.combat-round", robot.Name, robot.RoundsResolved + 1), robot.Location,
                "Active", robot.StartedAt ?? raider.ActivityStartedAt, robot.NextRoundAt, 5,
                NarrationContent.Default.Format("simulation.round-in-s-hp", robot.RoundsResolved + 1, remaining, robot.Name, robot.Health, robot.MaxHealth), logs);
        }
        if (raider.Activity == "Packing")
        {
            var threadId = $"packing:{raider.Location}";
            var total = 2;
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => e.ThreadId == threadId && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Packing", "Securing Salvage", raider.Location,
                "Active", raider.ActivityStartedAt, raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.packing-backpack-complete-in-s", remaining), logs);
        }
        if (raider.Activity == "Recovering")
        {
            var threadId = $"recovery:{raider.Location}";
            var total = 8;
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => e.ThreadId == threadId && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Recovery", "Contest Recovery", raider.Location,
                "Active", raider.ActivityStartedAt, raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.recovering-breath-after-salvage-contest", remaining), logs);
        }
        if (raider.Activity is "CallingExtraction" or "WaitingForVehicle" or "WaitingToDepart")
        {
            var threadId = $"extract:{raider.Location}";
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => e.ThreadId == threadId && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            var calling = raider.Activity == "CallingExtraction";
            var waitingForVehicle = raider.Activity == "WaitingForVehicle";
            return new ActiveThreadView(threadId, "Extraction",
                calling ? "Calling Train / Service Elevator" : waitingForVehicle ? "Joining Shared Extraction" : "Vehicle Arrived / Departing",
                raider.Location, "Active", raider.ActivityStartedAt, raider.ActivityEndsAt,
                calling || waitingForVehicle ? ExtractionVehicleArrivalSeconds : ExtractionBoardingWindowSeconds,
                calling || waitingForVehicle
                    ? NarrationContent.Default.Format("simulation.extraction-vehicle-landing-in-s", remaining, raider.ExitVehicle)
                    : NarrationContent.Default.Format("simulation.extraction-departure-window-s", remaining, raider.ExitVehicle),
                logs);
        }
        if (raider.Activity == "Extracting")
        {
            var threadId = $"extract:{raider.Location}";
            var total = 10;
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => e.ThreadId == threadId && e.ParticipantIds.Contains(raider.PlayerId))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Extraction", "Extraction in Progress", raider.Location,
                "Active", raider.ActivityStartedAt, raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.extraction-shuttle-landing-in-s", remaining), logs);
        }
        if (raider.Activity == "Downed")
        {
            var threadId = $"downed:{raider.PlayerId}";
            var total = 5;
            var remaining = raider.ActivityEndsAt.HasValue
                ? Math.Max(0, (int)Math.Ceiling((raider.ActivityEndsAt.Value - serverTime).TotalSeconds))
                : 0;
            var logs = events.Where(e => e.ParticipantIds.Contains(raider.PlayerId) && (e.Kind == "Downed" || e.Kind == "UnmitigatedDamage" || e.Kind == "ShieldAbsorbed"))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Survival", "CRITICAL: Raider Downed", raider.Location,
                "Active", raider.ActivityStartedAt, raider.ActivityEndsAt, total,
                NarrationContent.Default.Format("simulation.vital-signs-failing-medbay-recovery", remaining), logs);
        }
        if (raider.Activity == "KnockedOut")
        {
            var threadId = $"knockedout:{raider.PlayerId}";
            var logs = events.Where(e => e.ParticipantIds.Contains(raider.PlayerId) && (e.Kind == "KnockedOut" || e.Kind.StartsWith("HiddenPocket")))
                .Select(e => e.NarrationText).ToArray();
            return new ActiveThreadView(threadId, "Survival", NarrationContent.Default.Format("simulation.terminal-raider-knocked-out"), raider.Location,
                "Completed", raider.ActivityStartedAt, raider.ActivityEndsAt, 0,
                NarrationContent.Default.Format("simulation.chassis-recovered-by-medbay-terminal"), logs);
        }
        return null;
    }

    private void CompleteThread(SimRaider raider, string threadId, string outcomeSummary)
    {
        var existing = raider.CompletedThreads.FirstOrDefault(t => t.ThreadId == threadId);
        if (existing is not null) return;
        var threadLogs = events.Where(e => (e.ThreadId == threadId || (e.Location == raider.Location && e.Kind.StartsWith("Robot", StringComparison.Ordinal))) && e.ParticipantIds.Contains(raider.PlayerId))
            .Select(e => e.NarrationText).ToArray();
        var kind = threadId.Split(':')[0] switch
        {
            "travel" => "Travel",
            "search" => "Search",
            "robot" => "RobotCombat",
            "packing" => "Packing",
            "recovery" => "Recovery",
            "extract" => "Extraction",
            "downed" => "Survival",
            "knockedout" => "Survival",
            "area" => "AreaActivity",
            _ => "Activity"
        };
        var title = kind switch
        {
            "Travel" => threadId.Replace("travel:", "Travel: "),
            "Search" => threadId.Replace("search:", "Search: "),
            "RobotCombat" => $"Robot Encounter: {robot.Name}",
            "Extraction" => "Extraction at Platform",
            "Packing" => "Securing Salvage",
            "Recovery" => "Contest Recovery",
            "AreaActivity" => $"{raider.Activity} at {raider.Location}",
            "Survival" => threadId.StartsWith("downed", StringComparison.Ordinal) ? "Raider Downed" : "Terminal Defeat",
            _ => threadId
        };
        raider.CompletedThreads.Add(new ActiveThreadView(threadId, kind, title, raider.Location,
            "Completed", raider.ActivityStartedAt, raider.ActivityEndsAt, 0, outcomeSummary, threadLogs));
    }

    private int Factor(string slot, string factor) =>
        SHA256.HashData(Encoding.UTF8.GetBytes($"salvage-v1:{seed}:{slot}:{factor}"))[0];

    private void ProcessPendingHandlerActions(DateTimeOffset at)
    {
        if (pendingHandlerActions.Count == 0) return;

        var actions = pendingHandlerActions.Where(a => a.QueuedAt <= at).ToArray();
        pendingHandlerActions.RemoveAll(a => a.QueuedAt <= at);

        foreach (var action in actions)
        {
            var raider = raiders.FirstOrDefault(r => r.PlayerId == action.PlayerId);
            if (raider is null || raider.Activity is "Extracted" or "KnockedOut") continue;

            switch (action.ActionType)
            {
                case "CallExtract":
                    ExecuteCallExtract(raider, at);
                    break;

                case "DepartExtraction":
                    if (raider.Activity != "WaitingToDepart") break;
                    Emit(at, "HandlerAction", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.boards-vehicle-and-confirms-departure", raider.Name, raider.ExitVehicle),
                        location: raider.Location,
                        reason: "HandlerSupport:DepartExtraction");
                    CompleteExtraction(raider, at);
                    break;

                case "Calm":
                    var oldGreedCalm = raider.Greed;
                    raider.Greed = Math.Max(0, raider.Greed - 12);
                    if (riskReward) raider.Mood = Math.Min(10, raider.Mood + 3);
                    Emit(at, "HandlerAction", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.handler-broadcast-calm-radio-transmission", oldGreedCalm, raider.Greed, raider.Mood, RaidConditions.Resilience(raider.Mood), CurrentExtractChance(raider, at)),
                        location: raider.Location,
                        reason: "HandlerSupport:Calm");
                    break;

                case "Pressure":
                    var oldGreedPressure = raider.Greed;
                    raider.Greed = Math.Min(100, raider.Greed + 10);
                    if (riskReward) raider.Mood = Math.Max(-10, raider.Mood - 2);
                    Emit(at, "HandlerAction", [raider.PlayerId],
                        NarrationContent.Default.Format("simulation.handler-broadcast-pressure-radio-transmission", oldGreedPressure, raider.Greed, raider.Mood, RaidConditions.Appetite(raider.Greed, raider.Mood), CurrentExtractChance(raider, at)),
                        location: raider.Location,
                        reason: "HandlerSupport:Pressure");
                    break;
            }
        }
    }

    private void ExecuteCallExtract(SimRaider raider, DateTimeOffset at)
    {
        if (!CanCallExtract(raider.PlayerId)) return;
        raider.EmergencyExtract = true;
        raider.Route.Clear();

        Emit(at, "HandlerAction", [raider.PlayerId],
            NarrationContent.Default.Format("simulation.handler-issued-call-extract-natural"),
            location: raider.Location,
            reason: "HandlerSupport:CallExtract");
        if (riskReward)
        {
            InterruptForExit(raider, at);
            return;
        }

        if (raider.Location == "Extraction Platform")
        {
            if (raider.Activity is "Ready")
            {
                ContinueJourney(raider, at);
            }
        }
        else
        {
            var path = GetPathToExtraction(raider.Location);
            foreach (var node in path)
            {
                raider.Route.Enqueue(node);
            }
            if (raider.Activity is "Ready")
            {
                ContinueJourney(raider, at);
            }
        }
    }

    private static string[] GetPathToExtraction(string fromLocation) => fromLocation switch
    {
        "Extraction Platform" => [],
        "Salvage Yard" => ["Extraction Platform"],
        "Service Depot" => ["Extraction Platform"],
        "Market" => ["Extraction Platform"],
        "Entry Tunnel" => ["Market", "Extraction Platform"],
        _ => ["Market", "Extraction Platform"]
    };

    private void ApplyDamageToRaider(SimRaider raider, int rawDamage, string source, DateTimeOffset at, string? threadId = null)
    {
        if (riskReward)
        {
            var trim = Math.Min(rawDamage, RaidConditions.Resilience(raider.Mood));
            if (trim > 0)
                Emit(at, "Resilience", [raider.PlayerId], NarrationContent.Default.Format("simulation.positive-mood-trimmed-raw-damage", trim, source),
                    location: raider.Location, threadId: threadId);
            rawDamage -= trim;
            ChangePersonality(raider, 0, -1, at, source);
        }
        int hpDamage;
        if (raider.ShieldCharge > 0 && raider.ShieldDurability > 0)
        {
            var chargeLost = Math.Min(raider.ShieldCharge, rawDamage);
            hpDamage = rawDamage - (int)Math.Floor(chargeLost * 0.4);
            raider.ShieldCharge -= chargeLost;

            var durabilityLost = Math.Min(raider.ShieldDurability, Math.Round(((double)chargeLost / raider.ShieldMaxCharge) * 10.0, 2));
            raider.ShieldDurability = Math.Max(0, raider.ShieldDurability - durabilityLost);

            Emit(at, "ShieldAbsorbed", [raider.PlayerId],
                NarrationContent.Default.Format("simulation.shield-lost-charge-hp-got", chargeLost, hpDamage),
                location: raider.Location,
                healthDelta: -hpDamage,
                threadId: threadId);
        }
        else
        {
            hpDamage = rawDamage;
            Emit(at, "UnmitigatedDamage", [raider.PlayerId],
                NarrationContent.Default.Format("simulation.shield-depleted-chassis-takes-unmitigated", hpDamage, source),
                location: raider.Location,
                healthDelta: -hpDamage,
                threadId: threadId);
        }

        raider.Health = Math.Max(0, raider.Health - hpDamage);

        if (raider.Health == 0)
        {
            var paddle = raider.FieldSupplies.FirstOrDefault(s => s.ReviveAmount > 0 && s.Quantity > 0);
            if (paddle is not null)
            {
                paddle.Quantity--;
                raider.Health = paddle.ReviveAmount;
                Emit(at, "ConsumableUsed", [raider.PlayerId],
                    NarrationContent.Default.Format("simulation.panic-paddle-discharged-defibrillated-back", raider.Name, raider.Health),
                    location: raider.Location,
                    healthDelta: raider.Health,
                    itemName: paddle.Name,
                    threadId: threadId);
                if (riskReward) ChangePersonality(raider, 0, 2, at, $"Used {paddle.Name}");
            }
            else
            {
                raider.CauseOfDefeat = source;
                LeaveSearch(raider);
                SetActivity(raider, "Downed", null, at + TimeSpan.FromSeconds(5), at);
                var downedLine = SurvivalCanonContent.GetRandomDownedLine(at.ToUnixTimeMilliseconds());
                Emit(at, "Downed", [raider.PlayerId],
                    NarrationContent.Default.Format("simulation.is-downed", raider.Name, downedLine),
                    location: raider.Location,
                    reason: source,
                    threadId: threadId);
            }
        }
    }

    private void TryAutonomousConsumableUse(SimRaider raider, DateTimeOffset at, string? threadId = null)
    {
        if (raider.Activity is "Downed" or "KnockedOut") return;

        if (raider.Health <= 30)
        {
            var medSupply = raider.FieldSupplies
                .Where(s => s.HealAmount > 0 && s.Quantity > 0)
                .OrderByDescending(s => s.HealAmount)
                .FirstOrDefault();

            if (medSupply is not null)
            {
                medSupply.Quantity--;
                var oldHp = raider.Health;
                raider.Health = Math.Min(raider.MaxHealth, raider.Health + medSupply.HealAmount);
                var actualHeal = raider.Health - oldHp;
                Emit(at, "ConsumableUsed", [raider.PlayerId],
                    NarrationContent.Default.Format("simulation.autonomously-applied-recovering-hp-hp", raider.Name, medSupply.Name, actualHeal, raider.Health, raider.MaxHealth),
                    location: raider.Location,
                    healthDelta: actualHeal,
                    itemName: medSupply.Name,
                    threadId: threadId);
                if (riskReward) ChangePersonality(raider, 0, 2, at, $"Applied {medSupply.Name}");
            }
        }

        if (raider.ShieldCharge <= 15)
        {
            var shieldSupply = raider.FieldSupplies
                .Where(s => s.ChargeAmount > 0 && s.Quantity > 0)
                .OrderByDescending(s => s.ChargeAmount)
                .FirstOrDefault();

            if (shieldSupply is not null)
            {
                shieldSupply.Quantity--;
                var oldCharge = raider.ShieldCharge;
                raider.ShieldCharge = Math.Min(raider.ShieldMaxCharge, raider.ShieldCharge + shieldSupply.ChargeAmount);
                var actualCharge = raider.ShieldCharge - oldCharge;
                Emit(at, "ConsumableUsed", [raider.PlayerId],
                    NarrationContent.Default.Format("simulation.autonomously-triggered-restoring-shield-charge", raider.Name, shieldSupply.Name, actualCharge, raider.ShieldCharge, raider.ShieldMaxCharge),
                    location: raider.Location,
                    itemName: shieldSupply.Name,
                    threadId: threadId);
            }
        }
    }

    internal sealed class Opportunity(string id, string location, int searchSeconds)
    {
        public string Id { get; } = id;
        public string Location { get; } = location;
        public int SearchSeconds { get; } = searchSeconds;
        public DateTimeOffset? ClosesAt { get; private set; }
        public bool Resolved { get; set; }
        public HashSet<string> ParticipantIds { get; } = new(StringComparer.Ordinal);

        public void Admit(string playerId, DateTimeOffset at)
        {
            if (Resolved || (ClosesAt.HasValue && at > ClosesAt.Value))
            {
                throw new InvalidOperationException("Cannot admit a participant after a shared search closes.");
            }
            if (!ParticipantIds.Add(playerId))
            {
                throw new InvalidOperationException("A Raider cannot join the same search twice.");
            }
            ClosesAt ??= at.AddSeconds(SearchSeconds);
        }
    }

    private sealed class WorldItem(string opportunity, ItemView item)
    {
        public string Opportunity { get; } = opportunity;
        public ItemView Item { get; } = item;
        public string? OwnerId { get; set; }
        public bool Lost { get; set; }
    }

    private sealed class RobotState
    {
        public string Id { get; set; } = "roomba-prime";
        public string Name { get; set; } = "Roomba Prime";
        public string Location { get; } = "Service Depot";
        public string Flavor { get; set; } = NarrationContent.Default.Format("simulation.it-emits-a-high-pitched");
        public int MaxHealth { get; set; } = 70;
        public int Health { get; set; } = 70;
        public string Status { get; set; } = "Dormant";
        public int RoundsResolved { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? NextRoundAt { get; set; }
        public HashSet<string> ParticipantIds { get; } = new(StringComparer.Ordinal);
        public Dictionary<string, RobotDamage> DamageByPlayer { get; } = new(StringComparer.Ordinal);
    }

    private sealed class SharedExtractionVehicle(string calledBy, string vehicle, DateTimeOffset arrivalAt)
    {
        public string CalledBy { get; } = calledBy;
        public string Vehicle { get; } = vehicle;
        public DateTimeOffset ArrivalAt { get; } = arrivalAt;
        public DateTimeOffset? BoardingClosesAt { get; set; }
        public string Status { get; set; } = "Calling";
    }

    private sealed class RobotDamage(DateTimeOffset joinedAt)
    {
        public DateTimeOffset JoinedAt { get; } = joinedAt;
        public int Dealt { get; set; }
        public int Taken { get; set; }
    }

    internal sealed class FieldSupplyItem(
        string catalogId,
        string name,
        string category,
        int quantity,
        int healAmount,
        int reviveAmount,
        int chargeAmount)
    {
        public string CatalogId { get; } = catalogId;
        public string Name { get; } = name;
        public string Category { get; } = category;
        public int Quantity { get; set; } = quantity;
        public int HealAmount { get; } = healAmount;
        public int ReviveAmount { get; } = reviveAmount;
        public int ChargeAmount { get; } = chargeAmount;
        public StagedSupplyItem ToView() => new(CatalogId, Name, Category, Quantity);
    }

    internal sealed record PendingHandlerAction(string ActionId, string ActionType, string PlayerId, DateTimeOffset QueuedAt);

    private sealed class SimRaider(string playerId, string slot, string name, string disposition, int greed, int aptitude, int initiative,
        Queue<string> route)
    {
        public string PlayerId { get; } = playerId;
        public string Slot { get; } = slot;
        public string Name { get; } = name;
        public string Disposition { get; } = disposition;
        public int Greed { get; set; } = greed;
        public int Mood { get; set; } = 2;
        public DateTimeOffset AdmittedAt { get; set; }
        public DateTimeOffset? NextDiaryAt { get; set; }
        public int DiaryBeat { get; set; }
        public Dictionary<string, long> VoiceCounts { get; } = [];
        public string? LastVoice { get; set; }
        public int Aptitude { get; } = aptitude;
        public int Initiative { get; } = initiative;
        public Queue<string> Route { get; } = route;
        public HashSet<string> Visited { get; } = new(StringComparer.Ordinal);
        public int AreaBeat { get; set; }
        public int AreaVisit { get; set; }
        public string Location { get; set; } = "Entry Tunnel";
        public string Activity { get; set; } = "Preparing";
        public string? Destination { get; set; }
        public DateTimeOffset? ActivityEndsAt { get; set; }
        public DateTimeOffset ActivityStartedAt { get; set; }
        public int MaxHealth { get; } = 40;
        public int Health { get; set; } = 40;
        public int ShieldMaxCharge { get; } = 40;
        public int ShieldCharge { get; set; } = 40;
        public double ShieldDurability { get; set; } = 100.0;
        public string? HiddenPocketInstanceId { get; set; }
        public List<FieldSupplyItem> FieldSupplies { get; } = [];
        public string? CauseOfDefeat { get; set; }
        public bool EmergencyExtract { get; set; }
        public string ExitVehicle { get; set; } = "train";
        public int LostHaulValue { get; set; }
        public int LostHaulCount { get; set; }
        public List<ItemView> Backpack { get; } = [];
        public List<ItemView> Extracted { get; } = [];
        public List<ActiveThreadView> CompletedThreads { get; } = [];
    }
}

public sealed record RaidProjection(string RaidId, int Revision, string RulesVersion, DateTimeOffset ServerTime,
    string Status, DateTimeOffset DepartsAt, DateTimeOffset? NextActivityAt, string Location,
    IReadOnlyList<PlayerView> Players, IReadOnlyList<LocationView> Locations, CrateView Crate, RobotView Robot,
    int MyHealth, int MyMaxHealth, IReadOnlyList<ItemView> MyInventory, IReadOnlyList<ItemView> MyExtractedLoot,
    IReadOnlyList<EncounterView> Encounters, RobotEncounterView? RobotEncounter, IReadOnlyList<RaidEventView> Events,
    IReadOnlyList<OpportunityView> Opportunities,
    ActiveThreadView? CurrentThread = null,
    IReadOnlyList<ActiveThreadView>? RecentThreads = null,
    int MyShieldCharge = 0,
    int MyShieldMaxCharge = 40,
    double MyShieldDurability = 100.0,
    string? MyHiddenPocketInstanceId = null,
    IReadOnlyList<StagedSupplyItem>? MyFieldSupplies = null,
    string? MyCauseOfDefeat = null,
    int MyLostHaulValue = 0,
    int MyLostHaulCount = 0,
    ZoneConditionView? Condition = null,
    DateTimeOffset? DeadlineAt = null,
    int MyGreed = 0,
    int MyMood = 0,
    int MyResilience = 0,
    double MyExtractChance = 0,
    double MyAppetite = 0,
    double Exposure = 0,
    IReadOnlyList<HazardWarningView>? HazardWarnings = null);
public sealed record OpportunityView(string OpportunityId, string Location, string Status, DateTimeOffset? ClosesAt,
    int RemainingItems, IReadOnlyList<string> ParticipantIds);
public sealed record PlayerView(string PlayerId, string DisplayName, string Disposition, string Location,
    string Activity, string? Destination, DateTimeOffset? ActivityEndsAt);
public sealed record LocationView(string Name, IReadOnlyList<string> Neighbors);
public sealed record CrateView(string CrateId, string Location, string Status, int RemainingItems);
public sealed record ItemView(string InstanceId, string CatalogId, string Name, int Value, string Flavor);
public sealed record ContestFactorView(string PlayerId, string DisplayName, string Disposition, int Greed,
    int Aptitude, int BackpackValue, int Initiative, int Score);
public sealed record ItemAwardView(string InstanceId, string ItemName, string RecipientId, string RecipientName);
public sealed record EncounterView(string EncounterId, DateTimeOffset ResolvedAt, string Location, string Resolution,
    IReadOnlyList<string> ParticipantIds, IReadOnlyList<ContestFactorView> Factors,
    IReadOnlyList<ItemAwardView> Awards, string Reason);
public sealed record RobotView(string RobotId, string Name, string Location, string Status, int Health, int MaxHealth,
    int RoundsResolved, DateTimeOffset? NextRoundAt, IReadOnlyList<string> ParticipantIds);
public sealed record RobotDamageView(string PlayerId, DateTimeOffset JoinedAt, int DamageDealt, int DamageTaken);
public sealed record RobotEncounterView(string EncounterId, DateTimeOffset StartedAt, DateTimeOffset ResolvedAt,
    string Location, string Resolution, IReadOnlyList<string> ParticipantIds,
    IReadOnlyList<RobotDamageView> Damage, string Reason);
public sealed record RaidEventView(
    int Sequence,
    DateTimeOffset At,
    string Kind,
    IReadOnlyList<string> ParticipantIds,
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
    string Channel = "Diary",
    string? FactsText = null,
    string? Flavor = null,
    string NarrativeSource = "CanonFallback",
    string EnrichmentStatus = "NotRequested",
    string? EnrichmentReason = null
)
{
    public string NarrationText => Flavor is null ? Text : $"{Text}\nOptional Raider aside [Generated]: {Flavor}";
}

public sealed record ActiveThreadView(
    string ThreadId,
    string Kind,
    string Title,
    string Location,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? EndsAt,
    int TotalSeconds,
    string Summary,
    IReadOnlyList<string> Logs
);
