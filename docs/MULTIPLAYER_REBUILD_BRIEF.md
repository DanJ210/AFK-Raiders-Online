# AFK Raiders: Live Multiplayer Rebuild Brief

Date: 2026-10-10
Product alignment confirmed: 2026-10-11
Product direction clarified: 2026-10-12
Autonomous Raider economy confirmed: 2026-10-17

## Purpose and status

This is a portable handoff for starting a new repository and development conversation. It records the direction reached while evaluating the AFK Raiders prototype, without making that prototype an implementation dependency.

The existing game is an unpublished experiment. Its gameplay design remains the baseline for the online game; its client-side architecture, saves, runtime, and migration history are not implementation dependencies.

This brief governs the shared-world rebuild, not the current prototype. The full [GAME_DESIGN.md](GAME_DESIGN.md) is the main-game design baseline, including its autonomous loop, parody Comms Log, loot/value, home stash, preparation, survival, progression, and Handler interactions; §8 adds social encounter context. Adapt that design to server authority rather than treating all non-social features as optional historical material. The recurring Desperanza lifecycle supersedes invite-first entry. Exact balance remains unvalidated, explicitly planned features remain planned, and multiplayer conflicts require explicit reconciliation rather than wholesale copying or silent removal.

This workspace contains the M1 networking proof, independent server-owned journeys, one experimental shared robot, and a bounded S5 Desperanza duty loop. S5 exercises recurring admission and actual-Raider encounters only for two Development identities in memory; it is not the production Desperanza service, a public matching pool, or the full Handler influence loop. Experimental rules and verification evidence are documented in the README and implementation ledger; they are not validated balance. Hosting and the remaining operational/mechanical details below still require design.

## 1. Product direction

**G6 runtime clarification (2026-10-15):** The two-identity in-memory experiment now has one seeded condition/zone/danger profile per world, one shared danger-specific robot, finite unique loot, timed residence diaries and autonomous risk/extraction. Raiders live at Desperanza between raids and deploy in single-person pods; the initial tunnel-to-surface trip takes 60 seconds. At extraction, a train or service elevator arrives after 60 seconds with doors opening on all four sides. Each Raider must independently commit to extracting and reach the platform during the shared 30-second boarding window to join that vehicle; departures and haul settlement remain individual. A Handler may confirm their Raider's departure when it arrives; otherwise that Raider leaves autonomously at the window's end. Admission closes at world age 300 seconds; the world expires at 1320 seconds. Forecasts and impacts use actual location/time, and extracting interrupts only the owner's unfinished activities/search membership. Decisions run on server deadlines; projection polling and optional narration cannot drive outcomes. Calm/Pressure and CallExtract are bounded spendable-Signal actions, not mandatory input. Private vitals, appetite/exit telemetry and loot remain owner-filtered. The Hub roster shows the two enrolled Development identities' home presence only; transition comms are event-driven, with no fabricated idle chatter. The normal Development server uses full-speed timing; fast pacing is enabled only by tests. The new shared-extraction behavior and travel timings are experimental and require fresh balance verification. G6 does not complete M2/M3, public matching, persistence, direct PvP or theft. Frozen technical acceptance and severe High-balance limitations are recorded in the [implementation ledger](IMPLEMENTATION_PROGRESS.md#g6-autonomous-riskreward-conditions-and-broader-parody-content); human comedy/pacing approval remains pending.

- The game needs meaningful PvP alongside environmental robot threats.
- Interactions must be real and live: players should feel they are meeting another person's actual Raider, whether friend or enemy.
- The selected stakes direction is contested prizes plus bounded backpack theft, rather than unrestricted full-haul PvP loss.
- Previous client-side implementation code need not remain intact. Retain the original main-game design and parody identity while rebuilding its execution on the authoritative server.
- The core product is an autonomous zero-player RPG that plays itself, expanding the original solo concept with live social integration.
- Crates provide events or contests when Raiders cross paths during a server-owned raid. Gameplay factors resolve those encounters; Handler click speed is not the intended competition.
- All Raiders reside in **Desperanza**, the persistent home hub, before a raid. Raiders autonomously join recurring, ongoing server-run raids; private invites are not the core participation or matchmaking loop.
- Raiders organically encounter other actual Raiders while sharing those raids. Their autonomous raid cycle repeats without requiring a Handler to redeploy them or wait for an invite.
- The Handler is the player: they monitor comms and influence their Raider **only through spendable Signal actions**. The Handler does not shop, sell, equip, repair, or stage loadouts. Exact Signal actions, costs, timing, and offline safeguards remain to be designed; they must not turn play into mandatory moment-to-moment control.
- **Autonomous Raider economy (confirmed 2026-10-17):** Like Godville, the Raider manages its own economy. See [Autonomous Raider economy](#autonomous-raider-economy-confirmed-2026-10-17) below.
- **Main-game continuity (confirmed 2026-10-12):** The Handler stays in Desperanza while the Raider reports every gameplay action and encounter through the Comms Log, including parody actions and commentary. Diary and active-thread presentation are core gameplay surfaces, not decorative additions to multiplayer receipts. The original design remains valid beyond social encounters; the S1-S5 slices establish engine groundwork, not the full game.
- Encounters are not asynchronous fights against saved Raider snapshots. Every participant in a shared encounter is an actual Raider in the same authoritative world and uses its server clock.

The original [game design document](GAME_DESIGN.md) is the main-game baseline for this rebuild, including the Handler experience and the loot/survival/home loop as well as §8 Social Encounters and PvP. The server engine enables these systems to operate in a shared world instead of isolated browser simulations. Any retained invite-based development endpoints are a test harness, not the intended product loop.

## 2. Product definition

Build an autonomous zero-player RPG with live, recurring social PvPvE extraction gameplay organized around Desperanza as the shared home hub.

The player is a Handler, not the Raider. Their Raider moves, searches, fights, resolves encounters, and prepares between raids autonomously. The intended gameplay continues without a Handler responding to each event. Handler involvement is limited to Signal actions (support, nudges, and any later approved standing intentions or social actions); their exact rules remain proposals requiring confirmation. Preparation is the Raider's decision, not a Handler task.

### Autonomous Raider economy (confirmed 2026-10-17)

This is confirmed product direction, not an experimental proposal. Exact traits, weights, prices, stock, and Signal costs still need tuning and remain unvalidated balance.

- **The Raider makes its own economic decisions.** In Desperanza it sells loot, buys weapons and supplies, equips, repairs, stashes, and decides when it is ready to depart. Weapons come mainly from raid loot and sometimes from Desperanza traders. The Handler has no shop, sell, equip, repair, or staging controls.
- **Deterministic, server-owned decisions.** The C# domain decides with a utility (scoring) policy: every legal option (sell, buy, equip, repair, stash, depart) gets a score, for example power gain × trait weight − price − cash-reserve risk. Inputs are only what the Raider should know: inventory, coins, durability, current trader stock, recent outcomes, and its traits. The coordinator validates and commits each choice as a journaled event. Same seed, state, and time give the same decisions, so replay, retries, tests, and recovery stay deterministic.
- **Personality through traits.** Each Raider has seeded traits (examples: Greedy, Cautious, Hoarder, Brawler, Gadgeteer) that weight its choices and drift slowly with committed experience, such as becoming more cautious after a failed extraction. Different Raiders choose differently from the same stock.
- **Signals as nudges, not commands.** Signal actions shift scoring weights temporarily (for example "Spend!", "Play it safe", "Hoard"), like the existing Calm/Pressure greed nudges. They never name a specific purchase or bypass validation, and an absent Handler forfeits nothing owed.
- **Desperanza traders.** Trader stock is generated from a seed for each cycle and is shared and finite. When Raiders want the same scarce item, a deterministic order decides, never request arrival or reflexes. Each purchase or sale commits exactly once: retries and repeated scheduling cannot double-charge, double-pay, or duplicate items.
- **AI models do not decide.** No language model chooses actions, prices, or outcomes. The reasons are nondeterminism, latency, cost, prompt injection through player-controlled text, and the need to validate every move anyway. AI enriches the voice of committed events (diary and Comms explaining *why* the Raider bought that ridiculous gun), with authored templates as fallback, as the [AI systems design](AI_SYSTEMS_DESIGN.md) already describes.
- **Possible later hybrid (not approved):** at most once per Desperanza visit, a model selects among the top few options that the utility policy already ranked as legal. The selection is journaled as an event so replay reads it instead of re-asking the model, and the utility policy's top choice is used if the model fails. This is a deferred experiment that needs separate approval.

The implemented G4/OUTFITTER Handler shop, equip, repair, and staging controls are transitional. They are superseded by this direction and will be replaced, not extended. See the [implementation ledger](IMPLEMENTATION_PROGRESS.md) for the proposed replacement step.

#### Initial economy values (settled 2026-10-17)

These are the starting contract for ECON implementation. They were chosen autonomously because the user was unavailable, to fit the existing catalog (weapons 0/65/140/280 coins, supplies 8-90 coins, loot 5-35 coins, averaging about 16). They are tunable and **not validated balance**. Change them through a recorded decision, not silently.

1. **Traits.** Each Raider has one seeded primary trait. The existing **Cautious** and **Hoarder** are kept, and **Brawler** and **Gadgeteer** are added. A separate "Greedy" trait is not added, because the existing evolving greed stat already covers it. Trait drift is deferred to a later step.

   | Trait | Weapon weight | Medical weight | Recharger weight | Cash reserve | Repair at durability ≤ | Sells |
   |---|---|---|---|---|---|---|
   | Cautious | 0.8 | 1.5 | 1.2 | 40% | 75% | All loot |
   | Hoarder | 0.8 | 1.0 | 1.0 | 50% | 50% | All except its 3 most valuable items |
   | Brawler | 1.5 | 0.8 | 0.8 | 15% | 25% | All loot |
   | Gadgeteer | 1.0 | 1.0 | 1.5 | 25% | 50% | All loot |

2. **Decision pass.** It runs once when the Raider arrives in Desperanza, as fixed ordered phases: sell → repair → buy/equip weapon → buy/stage supplies → ready.
   - **Reserve:** after selling, the Raider keeps at least `max(reserve% × coins, repair cost of its equipped weapon)` unspent.
   - **Scoring:** a candidate's score is `benefit × trait weight ÷ price`. Weapon benefit is the gain in average damage × multiplier over the best usable owned weapon. Supply benefit is its heal, revive, or charge amount. Only affordable candidates with a positive benefit that respect the reserve qualify. The Raider buys the highest score.
   - **Ties:** ties go to catalog order, then instance ID.
   - **Equipping:** the Raider always equips its best usable weapon. A broken weapon is repaired if affordable. Otherwise the Raider equips the next usable weapon, or the free Tea Kettle fallback.
   - **Supply limits:** existing limits stay at 4 medical and 2 rechargers. The Raider fills its loadout from owned stock first and buys only to cover a shortfall.
3. **Selling.** On return, the Raider sells eligible loot at the existing G3 sale value. The Hoarder exception keeps its 3 most valuable items. The 120-unit stash cap and lowest-value auto-sale remain. Every sale is one event that commits once.
4. **Desperanza traders.** Trader stock rotates every 10-minute Hub cycle, seeded by cycle index, and is shared by all residents.
   - **Stock:** the Tea Kettle is always free. Each cycle offers one non-starter weapon (quantity 1) and 3 random supply types (quantity 2-4 each).
   - **Prices:** catalog value.
   - **Contested stock:** when stock is scarce, the earliest committed Hub arrival buys first, then Raider ID. Request arrival order never decides.
   - **Scope:** ECON-1 may use the full catalog as unlimited stock; finite rotation is ECON-2.
5. **Weapons as loot (ECON-2).** Aspperigo and Vernerider join field opportunity pools at about 5% of draws combined. Temptrest AR is trader-only. Found weapons start at full durability and are kept as owned equipment, not sold as loot.
6. **Signal nudges (ECON-3).** Two nudges are added, **Spend** and **Save**, each costing 1 Signal like Calm and Pressure. Each shifts the cash reserve by ±20 percentage points (clamped to 0-80%) for the next decision pass only. Only one nudge can be pending; it doesn't stack and is deduplicated on retries. A nudge never names an item.
7. **Outfitter retirement (ECON-1).** Remove the Handler purchase, equip, repair, and staging mutation endpoints and buttons in the same step that adds autonomous preparation, so there is no period with both manual and autonomous preparation. Keep a read-only Loadout/Inventory view that shows the Raider's committed decisions. Supplies the Handler already bought stay as Raider-owned stock.

"AFK" means no constant movement, aiming, reflex claiming, or mandatory encounter input. Live social integration connects actual human-owned Raiders; it does not require their Handlers to be continuously present. It also does not mean each browser independently invents a shared multiplayer world. Raiders return to Desperanza and re-enter the autonomous raid cycle; the world does not depend on a friend creating a private session.

### Crates and autonomous encounters

A crate is a shared-world opportunity for an event or contest, not simply a button that awards loot to the first Handler to click. When Raiders cross paths during a server raid, the server should resolve the resulting encounter through gameplay factors and autonomous Raider behavior. Exact factors, contest types, and reward rules are not yet confirmed.

The original [solo concept](https://github.com/DanJ210/afk-raiders) establishes that the Raider plays itself. Retain that product identity while redesigning the simulation for a shared server-owned world; preserving autonomy does not require importing prototype code, saves, balance, or offline catch-up.

The emotional loop:

1. Find loot worth protecting.
2. Meet another human-owned Raider who may help, compete, or threaten.
3. Let the Raider's autonomous behavior determine what is worth risking, with any approved Handler influence remaining optional.
4. Survive shared robot threats.
5. Extract the outcome.
6. Remember the person involved and care about meeting them again.

Comedy makes consequences memorable; funny logs are not a substitute for meaningful autonomous outcomes and social consequences.

### Definition of a real encounter

- Both players' actual Raiders occupy the same server-owned ongoing raid world/session at the same time.
- Both interact with the same crate, robot, or extraction opportunity.
- Intent, preset communication, and public actions reach the other participant live.
- Both receive compatible consequences, although private information and narration may differ.
- A stolen item changes actual ownership. It is not independently awarded in two simulations.
- Saved opponent snapshots and NPCs must never be represented as live human opponents.

### Shared zones, autonomous Raiders, live Handlers

Raiders reside in Desperanza between raids and autonomously enter ongoing server-run raids where actual human-owned Raiders share locations and a world clock. They return to the hub and continue the cycle without waiting for an invite or a Handler deployment command. Production population and account-backed lifecycle remain to be specified; the current technical harness supports at most two development identities.

**Bounded S5 experiment (approved 2026-10-12, Hub residence superseded by HUB-3 on 2026-10-16 and Ready Up cost by HUB-4 on 2026-10-17):** one-time duty opt-in; 30-second admission pulses; at most two actual development Raiders per world; solo admission when no other eligible Raider is available; worlds stay open while any admitted Raider remains active and close after the last returns. Raiders now spend ten minutes in Desperanza both on enrollment and after every return, with server-committed, owner-private, nonreward parody Hub activities every minute, including during paused duty or Handler absence. Optional Ready Up spends 2 Signal to skip the remaining stay and resume duty, immediately entering the queue without bypassing admission cadence, world availability or capacity; it never interrupts an active raid. The server rejects insufficient Signal without queue/duty changes and retries never spend twice. Without input, enabled duty queues automatically for free when the stay ends. Browser/Handler absence does not withdraw or pause an enrolled Raider. Pause affects future admission only. These settings are slice decisions, not public matchmaking policy, fairness guarantees, or validated balance. The runtime details and verification evidence are in the [README](../README.md) and [implementation ledger](IMPLEMENTATION_PROGRESS.md).

- Raiders search, travel, and fight autonomously through connected locations.
- Locations contain shared loot opportunities and robot threats.
- Raiders encounter each other when their paths overlap in the same location and time, not through a separate simulation of an opponent's saved state.
- Connected Handlers see encounters involving their actual current Raiders and may respond through limited commands. Exact commands and limits still require confirmation.
- The server resolves encounters even when neither Handler responds.
- Every participant uses the same server clock and receives a projection of the same committed world outcomes.

Meeting a friend means meeting their actual Raider in the same current raid instance. A theft transfers one real item: it disappears from their backpack and appears in yours. Different private views or narration must not create contradictory ownership or independently simulated stories.

Illustrative zone topology:

```text
                  Salvage Yard
                 /            \
Entry Tunnel -- Market -- Extraction Platform
                 \            /
                  Service Depot
```

This graph is an example, not an approved map, route-selection algorithm, travel schedule, or extraction rule. Its purpose is to show connected locations where autonomous paths can overlap and produce shared encounters.

Handler absence is distinct from asynchronous opponent simulation: a Raider continues in the actual server-owned world under autonomous behavior, returns to Desperanza, and can continue into later raids according to the approved bounded S5 experiment. Production outage recovery, account-backed opt-out and admission, public-pool offline exposure, and larger-population fairness remain open.

## 3. Recommended first playable rules

These are concrete starting proposals, not previously approved balance requirements. Confirm them before implementing dependent mechanics. The product direction is autonomous recurring participation from Desperanza, not private-invite-first entry. S5 verifies one bounded in-memory admission/lifecycle experiment; production account eligibility, population, and outage policy still need design. Any adopted live choices or support actions must remain optional: an encounter must resolve meaningfully when no Handler responds.

| Area | Proposed baseline |
|---|---|
| Entry | Desperanza residents autonomously join recurring shared raids; no invite is required |
| Population | Actual Raiders share ongoing raids; production capacity/concurrency remain open. S5 uses two Development identities only |
| Social entry | Autonomous admission from Desperanza creates organic encounters; private invites are not the core loop |
| World | One small location graph with shared loot, robots, and extraction |
| Preparation | Raider autonomously sells, buys, equips, repairs, and departs in Desperanza through a deterministic trait-weighted utility policy (confirmed direction 2026-10-17); no Handler shop or loadout controls |
| Equipment sources | Weapons mainly from raid loot; Desperanza traders sell from seeded, finite, shared per-cycle stock |
| Handler influence | Spendable Signal actions only, including temporary scoring nudges; never specific purchases |
| Standing orders | Avoid confrontation, deal first, or claim the objective |
| Optional live choices | Offer a split, stand ground, or break contact; autonomous behavior resolves the encounter without a response |
| Communication | Preset messages; no freeform chat initially |
| Combat | Autonomous rounds; support and intent matter without reflex aiming |
| PvP loss | Contested prize plus at most one eligible ordinary loot unit |
| Theft exposure | Ordinary-loot theft against a Raider limited to once per raid |
| Protection | Home assets, equipped gear, pocketed items, and support supplies are not theft targets |
| PvP defeat | Forced retreat rather than automatic loss of the whole raid |
| Post-defeat | A bounded separation/protection window prevents immediate repeated attacks |
| Reward | Contested and stolen loot must still be extracted |
| Disconnect | Current Raider continues under standing orders; no disconnect escape |
| Recurring participation | Handler presence must not gate autonomous return from Desperanza or raid re-entry. S5 experiments with 30-second admission, a 60-second home interval, and future-admission pause; production opt-out, offline exposure, and repeat-encounter safeguards remain to be designed |
| Offline mode | Outside the first multiplayer slice; no local-save import into its economy |

Standing orders are conditional behavior, not an attack/defense bonus system with one obvious best choice.

A shared robot and noise escalation should make fighting costly enough that cooperation or escape can be sensible. Do not reduce PvPvE to unrelated player combat plus occasional random damage.

### Timing hypotheses to playtest

- Command acknowledgement and preset messages: immediate live delivery.
- Opening encounter response window: roughly 10-15 seconds.
- Combat rounds: roughly 3-5 seconds.
- Travel and searches: slower scheduled activities.
- Initial raid duration: roughly 8-12 minutes.

These are hypotheses, not measured acceptance thresholds. A response window is an opportunity for optional intervention, not a requirement to play. Do not inherit the prototype's 30-second tick or 30-minute raid simply because it already exists.

Use one server timeline. Extraction, incapacitation, movement, and encounters need explicit ordering and interruption rules; faster encounter scheduling must not accidentally freeze unrelated deadlines.

## 4. Architecture recommendation

### Stack

- Frontend: Vue 3, TypeScript, Vite, Pinia, Tailwind.
- Backend: ASP.NET Core on a supported .NET LTS SDK selected at scaffolding.
- Live transport: SignalR.
- Persistence: SQL database.
- Deployment: separate frontend and backend hosting; choose the concrete services later.

The prototype's static frontend deployment does not provide an authoritative live simulation backend.

Start with one modular backend application. Do not introduce microservices, distributed simulation, or a broker before the small version requires them.

### Suggested repository layout

```text
web/
server/
  API/
  Realtime/
  Application/
  Domain/
  Persistence/
content/
tests/
docs/
```

This illustrates responsibility boundaries, not a requirement for a separate project per directory.

### Domain ownership

| Model | Responsibility |
|---|---|
| Account | Identity, secured inventory, currency |
| Raid session | Authoritative time, membership, locations, shared world |
| Raider | Position, health, equipment, carried items, standing orders |
| World entity | Shared crate, robot, extraction point |
| Encounter | Participants, decisions, deadlines, combat state |
| Ownership ledger | Item creation, transfer, consumption, extraction, loss |
| Player projection | Only the state and events that player may see |

The world exists independently of a particular Raider. Robots have one health pool and crates have one inventory.

Items need server-owned identity or provenance sufficient to trace creation and transfers. Catalog IDs alone do not identify a specific contested loot unit.

### Authority and command flow

The server owns equipment eligibility, resources, loot generation, movement, encounters, extraction, and account rewards. The client submits intent, not resulting state.

Examples:

```text
OfferSplit(encounterId, commandId)
StandGround(encounterId, commandId)
BreakContact(encounterId, commandId)
CallExtract(raidId, commandId)
```

One authoritative coordinator serializes mutation of each raid:

1. Authenticate the actor and validate membership, timing, costs, and eligibility.
2. Deduplicate commands and record accepted intent.
3. Collect competing decisions until a server-owned deadline.
4. Resolve using explicit action-order rules and seeded randomness.
5. Atomically persist resulting state, item transfers, and ordered durable events.
6. Deliver filtered updates to each participant.

Acknowledging an accepted command is distinct from announcing its resolved outcome. Late or invalid commands receive explicit rejection reasons.

SignalR is delivery infrastructure, not the simulation authority. Connection groups alone are not authorization.

Use revision/event sequences, idempotent commands and settlement, and reconnect snapshots with missed-event recovery. Do not promise exactly-once network delivery; guarantee no duplicate gameplay effects despite retries.

Persist committed events so interrupted delivery can recover after a crash. Protect account inventory transitions against concurrent deployments; an account cannot stage the same resources into multiple active raids.

### Simulation implementation

Implement one authoritative gameplay rules module in pure C# for the recommended .NET backend. Inject simulation time and randomness. Keep transport, database, and UI behavior outside rules.

Do not port the old TypeScript engine line-for-line or maintain parallel C# and TypeScript gameplay implementations. Reuse its ideas and small fixtures only when they fit the redesigned rules.

The browser renders server projections and can animate countdowns using server-time estimates. It never commits combat, theft, or extraction outcomes.

Narration follows committed domain events. Templates cannot claim a victory, theft, or rescue that did not happen. Pin content/rules versions for active sessions so deployments cannot silently change an ongoing raid.

### Disconnect and outage semantics

- Autonomous raid admission and repeat deployment must not require an active Handler connection; exact account eligibility and safeguards remain to be designed.
- Once deployed, a disconnected Raider remains in the authoritative session.
- Standing orders handle missed decisions.
- Reconnect restores the actual session, not local catch-up.
- Handler presence is distinct from Raider participation.
- A server outage is not a player disconnect: define a recovery/pause policy so downtime does not silently expire player decisions.
- Recover persisted sessions without awarding loot again or re-running committed outcomes.

## 5. Prototype salvage assessment

The inspected prototype contains about 5,896 nonblank engine lines, 2,926 component lines, 1,720 composable lines, and 12,452 content lines. Footprint is not an estimate of reusable effort.

Its combined single-player state, local browser ticker, catch-up, and Raider-owned encounters do not provide the required shared-world model.

| Asset | Treatment |
|---|---|
| Original world, names, item flavor, robot personalities | Selectively reuse after review |
| Handler-console visual identity | Inspiration, not a locked screen design |
| Responsive layout and log-scrolling patterns | Extract only when useful |
| Seeded simulation, explicit damage accounting, content validation | Retain practices, redesign interfaces |
| Shields, loot, equipment mechanics | Reference material, not approved multiplayer balance |
| Local saves, migrations, ticker, offline catch-up | Do not import |
| Combined game-state and activity orchestration | Replace |
| Economy, progression, durations, weights | Unvalidated prototype experiments |
| Old tests | Reference for useful invariants, not a wholesale regression contract |
| Old docs and agent instructions | Historical; do not copy as new-product requirements |

Do not build account-backed upload of arbitrary client saves as an intermediate step. Start with server-owned gameplay and inventory.

## 6. Implementation milestones and verification

Use the [implementation progress ledger](IMPLEMENTATION_PROGRESS.md) for current status, recorded checks, and proposed next slices. This section defines milestone scope; future mechanics remain proposals even when M1 provides partial groundwork. [Copilot instructions](../.github/copilot-instructions.md) require keeping the handoff current.

### Main-game delivery alongside multiplayer foundations

The [main-game coverage map and sequence](IMPLEMENTATION_PROGRESS.md#main-game-design-coverage-map-2026-10-12) maps the full original design to current server/UI evidence, including explicit shared-world conflicts and acceptance gates. S1-S5 supplied bounded engine groundwork, not the complete game.

Recommended proposed delivery order is G1 complete Comms/diary/active threads; G2 reviewed catalog and finite loot tables; G3 cumulative stash/coins/return receipts; G4 preparation and equipment/supply custody; G5 survival and Handler support together; G6 autonomous greed/extraction, world conditions and expanded content; G7 outcome-driven progression. Each slice needs bounded implementation authorization. G1-G3 initially remain Development-only/in-memory; durable assets and recovery remain S6 requirements before external release. S6 can resume once custody and outcome contracts are defined, without waiting for every progression/content feature.

Preserve the original gameplay intent while adapting it: one shared world owns conditions and finite loot; UI stacks retain unique item units; automatic admission is serialized against home preparation; personal extraction/support interrupts only the eligible Raider; Comms describes actual server actions with owner-private effects. The original manual-versus-autonomous controls, loadout selection semantics, Ready Up's admission effect and shared-world timing conflicts require explicit decisions, not silent feature removal. This sequence neither approves theft/public matching nor completes M2-M4.

### M1 infrastructure: One shared crate, two real clients (completed technical proof)

Implement development identities, two real browser clients, one shared location/objective, live presence, and server projections.

This was a technical proof only, not the intended participation model. The initial implementation used a manual claim and awarded the reward to the first valid server-serialized request; the autonomous slice removed that button and endpoint. The earlier invite/explicit-deploy interface was superseded by the S5 duty console; any retained legacy development endpoints are not product requirements.

Verify:

- Two independently connected browsers recognize each other.
- Both reference the same session and crate.
- Competing claims cannot duplicate the crate reward.
- Unauthorized clients cannot observe or command the session.
- Projections do not leak private inventory or hidden decisions.

Development identities must be explicitly development-only and unavailable in production.

### M1 gameplay: One autonomous salvage journey (verified earlier slice; itinerary superseded)

Following approval to begin the recommended autonomous slice on 2026-10-11, the implementation uses a small experimental `salvage-v1` rule set:

- The current harness admits two private development identities during a 20-second preparation window. Either can host; a solo Raider proceeds without waiting for another participant. Joins at or after departure are rejected. This is not the target Desperanza/autonomous admission flow.
- The server schedules travel, searching, and extraction on one timeline, independently of browser connections or requests.
- All admitted Raiders follow the same fixed Entry Tunnel -> Market -> Salvage Yard -> Extraction Platform itinerary. This guarantees a real co-location encounter; it does not yet implement independent path selection or a full zone graph simulation.
- The Market locker allocates two unique items. At the humming crate, two cautious Raiders cooperate and divide two salvage items by backpack need; a hoarder triggers a contest. A solo Raider searches without an invented opponent.
- Contest score is greed + salvage aptitude * 3 + seeded initiative - backpack value / 5 (integer division). Highest score wins both crate items. Exact ties use stable development identity order, not arrival of Handler requests. Disposition, greed, aptitude, initiative, backpack value, and allocations appear in a shared receipt.
- Cooperation/solo packing takes two seconds; contest recovery takes eight. With the fixed travel/search/extraction schedule, the slice ends at 78 or 84 seconds after creation. The extra contest time is not yet exposed to robot danger and must not be presented as validated risk/reward balance.
- Extraction moves the actual backpack items into an in-memory raid result once. No account economy or durable settlement is implied.
- The Handler console shows activities, the shared route, private carried/extracted loot, and committed event narration. Refresh restores tab-scoped access; disconnecting does not remove the Raider.

Verify with deterministic simulation time and seeded fixtures:

- Exact departure, travel, search, encounter, and extraction deadlines.
- Solo completion and two-participant cooperation/competition without Handler input.
- Identical encounter receipts and unique ownership across both projections.
- Results independent of admission order and scheduler polling frequency, including exact-score ties.
- Repeated/concurrent scheduling cannot repeat allocation or extraction.
- Unauthorized reads/subscriptions and invalid or late invitations are rejected.

The [README](../README.md) records the selected original content's source revision and current runtime limitations. Implementing this slice does not approve all proposed mechanics in section 3.

### Independent autonomous journeys (M1 extension)

The next bounded slice replaces the raid-wide itinerary with per-Raider activity deadlines and reproducible branch selection in a small connected graph. The same server coordinator, finite ownership, solo completion, authorization, and private projections remain.

- Market and two salvage branches provide finite shared searches; only co-present eligible searchers participate. A first arrival opens a fixed search window, later arrivals through its deadline join it, and exhausted opportunities never award again.
- Same-time arrivals are processed before due searches; search/encounter resolution precedes packing/recovery and extraction completion, followed by subsequent activity selection.
- The existing humming-crate cooperation/contest formula remains experimental. Market and Depot allocate ordinary salvage without new combat or theft mechanics.
- Raiders extract independently; completion is a property of the whole session only after every admitted Raider has settled.
- Both crossing and non-crossing branch journeys are required fixtures. The map, travel/search timing, branch order policy, and participation cutoff are experimental assumptions under authorized autonomous execution.

Use the [README](../README.md) for exact implemented rules and the [progress ledger](IMPLEMENTATION_PROGRESS.md) for verification evidence. Acceptance includes actual co-presence, exact search cutoff behavior, no duplicate finite loot, identical shared receipts, independent settlement, absent-Handler progression, stable simultaneous ordering, and results independent of scheduler frequency or admission order.

The current runtime does not yet implement the target Desperanza cycle: recurring autonomous admission from the hub, an ongoing organic real-Raider population, optional Handler influence, or the associated repeat/offline safeguards. These are future stages requiring a bounded design. Richer social choices, bounded theft, and durable accounts also remain unimplemented; S4 supplies only one experimental environmental threat and does not substitute for validating social encounters with actual Raiders.

### M2: Consequential encounters and organic social participation

S5 implements a bounded Development-only Desperanza-to-raid-to-Desperanza loop and autonomous admission experiment for two hard-coded identities. This does not meet the broader M2 population, fairness, or social-choice acceptance criteria below. Before expanding population or adding consequential PvP, confirm encounter factors and any optional Handler actions. Split, claim, retreat, autonomous combat, support costs, and bounded theft remain candidate mechanics, not newly approved rules.

Verify:

- Actual Raiders from a production autonomous Desperanza population share ongoing raid worlds without requiring a private invitation or simultaneous Handler presence. The current two-identity Development harness is only a bounded rehearsal.
- A Raider returns to Desperanza and continues its recurring raid cycle according to an explicit, safe admission policy. S5 verifies its experimental in-memory policy only; it does not establish public-pool safety or repeat-opponent fairness.
- Encounters arise from shared location/time in the authoritative world; no saved opponents or invented player proxies are used.
- Raiders carry out activities and encounters resolve when no Handler responds; an idle browser does not stall gameplay.
- Outcomes follow confirmed gameplay factors and autonomous behavior, not which Handler clicks first.
- If optional Handler actions are adopted, both responding Handlers and either single responding Handler can influence an encounter without making intervention mandatory.
- Simultaneous decisions resolve independently of packet arrival advantage.
- Duplicate commands do not repeat spending or transfers.
- Theft limits and retreat protection hold across repeat encounters.
- Receipts show understandable, compatible outcomes.

### M3: Actual PvPvE

Implement one shared robot, noise escalation, and limited cooperation.

Verify:

- Both Raiders act on one robot health pool.
- Robot participation changes the sensible encounter choices.
- Combat damage and loot ownership have explicit deterministic ordering.
- Cooperative play has useful payoff rather than only flavor.

### M4: Extraction and durable accounts

Implement production authentication, secured inventory, extraction settlement, reconnection, and restart recovery before external release.

Verify:

- Stolen loot remains exposed until extraction.
- Extraction and failure cannot both settle the same Raider.
- Inventory cannot be spent or staged twice through concurrent requests.
- A disconnect cannot cancel unfavorable outcomes.
- Server restart restores committed state and missed updates without duplicated rewards.
- Outage recovery follows the documented deadline policy.

### M5: Social validation and expansion

Test organic encounters among friends and strangers in the ongoing autonomous raid pool. Expand population/capacity only when validated by encounter quality and fairness; invites are not a prerequisite for meeting other Raiders.

Record whether players:

- Recognize and remember the other person.
- Explain what was at stake and why they chose an action.
- Consider losses understandable rather than arbitrary.
- Change behavior because of robot danger or encounter history.
- Want another raid with or against that person.

Before formal playtesting, agree participant count and numerical pacing/decision targets. Do not invent passing balance metrics afterward.

Do not expand squads, seasons, territory, rankings, or a large progression economy until this loop demonstrates value.

## 7. Decisions to confirm at the relevant milestone

- New repository name, ownership, visibility, and local workspace.
- Acceptance of the proposed .NET/SignalR stack and initial gameplay rules.
- Autonomous economy tuning: initial values were settled 2026-10-17 (see [Initial economy values](#initial-economy-values-settled-2026-10-17)). Trait drift rules and balance validation of those values remain open.
- Production raid lifecycle and capacity: how account-backed worlds are created, populated, retained, and closed while Raiders enter and extract at different times. S5's two-identity in-memory lifecycle is a bounded experiment.
- Production autonomous admission cadence, population/capacity, eligibility, opt-out/pause, and solo behavior when no other actual Raider is available. S5's cadence and solo-start rule do not establish the production policy.
- Repeated-encounter fairness, target avoidance/limits, and exposure for unattended Raiders in a larger always-on population. S5 introduces no target selection or pair-cooldown system.
- Autonomous crate contest factors, event types, and resolution rules.
- Whether and how Handlers may optionally influence encounters or communicate.
- Handler-facing monitoring and influence surfaces, including which consequential choices can be made without requiring continuous attention.
- Offline participation, loss, and recovery limits; zero-player autonomy must not be confused with importing prototype offline replay.
- Exact split allocation, theft eligibility/value ceiling, and retreat protection.
- Extraction interruption and combat/incapacitation ordering.
- Signal costs, regeneration, and intervention limits.
- Definition of cooperative membership and friendly fire.
- Authentication provider and deployment environment before external release.
- Repeat-opponent limits and population fairness for the autonomous shared-raid pool.

Ask focused questions when a decision blocks implementation. Do not stall the two-client experiment behind future-scale infrastructure.

## 8. Prototype references and provenance

Prototype repository: [DanJ210/afk-raiders](https://github.com/DanJ210/afk-raiders).

Inspected HEAD: `ee4d909c83299503e377ec496756cc602fdde6ea`.

The assessment also read uncommitted working-tree changes. That commit alone is not an exact snapshot of everything reviewed. If an asset is later imported, record its actual source revision or reviewed working-tree provenance.

Useful reference paths:

- [Game design](C:\Users\DanJ2\source\repos\afk-raiders\docs\GAME_DESIGN.md): original concept and prior social direction.
- [Architecture](C:\Users\DanJ2\source\repos\afk-raiders\docs\ARCHITECTURE.md): pure-engine and narration practices.
- [Activity tracker](C:\Users\DanJ2\source\repos\afk-raiders\docs\ACTIVE_RAID_ACTIVITY_PLAN.md): prototype-only activity contracts.
- [Storage/accounts proposal](C:\Users\DanJ2\source\repos\afk-raiders\docs\SERVER_STORAGE_AND_ACCOUNTS.md): previous backend preference, not the rebuild's authority model.
- [Lore index](C:\Users\DanJ2\source\repos\afk-raiders\docs\lore\README.md): original parody canon.
- [Legal guardrails](C:\Users\DanJ2\source\repos\afk-raiders\docs\lore\content-guidelines\LEGAL_GUARDRAILS.md): content boundaries.
- [Prototype state model](C:\Users\DanJ2\source\repos\afk-raiders\src\engine\types.ts): evidence of single-player ownership.
- [Prototype ticker](C:\Users\DanJ2\source\repos\afk-raiders\src\composables\useGameTicker.ts): local-only timing model.
- [Damage accounting](C:\Users\DanJ2\source\repos\afk-raiders\src\engine\shields.ts): reference implementation, not PvP balance.
- [Comms presentation](C:\Users\DanJ2\source\repos\afk-raiders\src\components\CommsLog.vue): visual patterns.

All imported content must be original, legally distinct parody. Do not import source-game names, copied prose, trademarks as player-facing branding, or third-party assets without appropriate rights. A.R.C. means "Aggressively Roaming Chassis" within AFK's original world.

## 9. Recommended handoff workflow

1. Read this brief, [GAME_DESIGN.md](GAME_DESIGN.md), [implementation progress](IMPLEMENTATION_PROGRESS.md), and [README](../README.md); these separate target product direction from the current development harness.
2. Preserve the confirmed target: Raiders live in Desperanza between raids, autonomously enter recurring ongoing raids, meet actual Raiders organically, and return to repeat; the Handler monitors comms and has optional influence.
3. Treat the S5 two-identity in-memory duty loop as a bounded experiment, not a production population or matching policy.
4. Before expanding population or implementing PvP/Handler encounter actions, confirm the production identity/lifecycle choices, encounter rules, Handler influence, and unattended/repeat-encounter safeguards without requiring constant player attention.
5. Keep all encounters authoritative and between actual Raiders; never represent saved snapshots or NPCs as live human opponents.
6. Update the progress ledger with the bounded scope, decision authority, tests, limitations, and concrete resume point.

A fresh chat is recommended for a clean workspace and product contract. Continuing the current chat is also possible once its workspace is explicitly switched; the document, not conversation memory, should be the durable source of direction.

### Continuation prompt for this implementation

> Read the Live Multiplayer Rebuild Brief, GAME_DESIGN.md, implementation progress ledger, README, and project Copilot instructions as the handoff. This is an autonomous zero-player RPG expanding the original solo concept with live social integration, not a migration of the prototype implementation. Product target: Raiders reside in Desperanza between raids, autonomously join recurring ongoing server-run raids, organically meet actual Raiders, return to the hub, and repeat; the Handler monitors comms and can influence selected aspects without mandatory live input. S5 implements a bounded Development-only, in-memory version of this loop for two identities with experimental 30-second admission pulses, capacity two, solo start, 60-second home interval, and pause for future admissions only. Treat those values as slice-specific, not a public policy or validated balance. Do not represent saved opponent snapshots or NPCs as live Raiders. Preserve server authority, privacy, finite item ownership, deterministic ordering, and exactly-once settlement; import no prototype saves, balance, or runtime behavior by default. S1-S5 verify bounded technical slices only; M2/M3, public population, production accounts/recovery, broader social PvP, and fairness remain incomplete. Before expanding population or implementing PvP/Handler encounter actions, confirm the relevant mechanics and safeguards for absent Handlers and repeat encounters; keep encounters meaningful with no Handler response. Do not build offline replay, extensive progression, or distributed infrastructure first. Update the handoff with verified outcomes, limitations, open decisions, and a concrete resume point.
