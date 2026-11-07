# World Activity and Crossings — Design Proposal

**Status: Proposal, awaiting designer approval. Documentation only; no implementation is active.**

This proposal does not authorize code, runtime content, tests, balance changes, rules-version or fingerprint changes, a backend restart, or a change to any frozen rule. It does not add canon: every sample line is a proposal, not approved runtime content. Numbers are labeled **Measured (docs)**, **Estimate**, or **Proposed**; no aim below is validated.

The 2026-11-06 decision allows AI to direct systems and act as an agent, and allows cloud models and paid APIs. That is permission to design options, not implementation approval. Current experimental balance remains at the Arm 4B + Arm 5 candidates; this proposal makes no balance recommendation.

## 1. Summary and recommendation

Keep one shared world timeline and make more Raiders likely to be present for one another's committed events, without turning every meeting into combat or a loot contest. First, measure real human Raider presence and increase low-stakes ambient and individual diary moments without changing health, items, coins, timing, or decision order. Separately test shared outcomes and story beats; do not count a shared joke as a consequential encounter.

**Recommendation:** begin with one map and the existing shared-world model; tune admission overlap and event visibility only after telemetry is in place. Keep shared ambient events cosmetic, make solo events owner-private, and give both clear repetition limits. Target roughly 45% co-presence, 15-25% raids with a shared committed outcome, and 20-25% with a story beat as **experimental goals**, not acceptance gates. Under the illustrative population model in section 3, 45% co-presence is not reached with only two naturally overlapping Raiders unless scheduling deliberately increases their overlap. A low-population simulator should test the formulas and telemetry before any public population or map change.

Do not open separate maps until there is sustained human population to support them. Prefer a hub-and-routes structure later so new zones can be added without splitting the population into isolated queues. Keep AI residents or players explicitly labeled and out of human crossing and balance cohorts unless a test intentionally measures them.

## 2. Definition, metrics, and current-state audit

### What counts as crossing paths

For these metrics, a **raid** is one actual Raider's server-owned raid participation, from admission to that Raider's return or terminal settlement. A **crossing** is a committed event during that raid at which at least one other actual Raider is present in the same live server-owned world at that event time. Record co-location separately: participants at the same location are a stronger form of proximity than merely sharing a world. A queued Raider, a Raider in Desperanza, an NPC, an AI agent, or a saved opponent snapshot is not a human crossing.

Count each raid at most once in each tier's numerator, regardless of how many qualifying events occurred. Use all eligible started human raids as the denominator, including raids with no crossing. Report unique event counts and participants separately so one large event does not masquerade as many independent meetings.

| Tier | Raid-level numerator | Experimental aim | What it is not |
|---|---|---:|---|
| **(a) Co-presence** | Raids with at least one committed event while at least one other actual Raider is in that same live world. Also report the stricter same-location share. | **About 45%** — designer's loose proposed aim, not measured or validated. | Does not imply interaction, shared outcome, or direct physical proximity when the Raiders are in different locations of the same world. |
| **(b) Shared committed event** | Raids with at least one committed event whose recorded, real outcome involves another actual Raider, such as a shared robot fight or shared activity result. | **15-25%** — proposed lower experimental aim. | A shared ambient line with no outcome, a nearby count, or separate private events are not enough. |
| **(c) Story beat** | Raids with at least one story-system beat committed for that Raider and linked to an eligible, recorded event or memory. | **20-25%** — proposed, roughly one in four or five raids. | Ordinary narration, AI asides, or an authored joke without a story-system beat ID do not count. |

Show human-only results first. If AI participants are tested later, report them separately and state whether each tier counts actual human-human, human-AI, or AI-AI participation. Do not mix those populations silently.

### What exists today

| Surface | Current documented behavior | What is not established or measured |
|---|---|---|
| World admission | The Development loop admits eligible actual Raiders on a 30-second pulse, has a two-Raider per-world cap, admits a solo world if no other eligible Raider is available, and retains the world until its participants return. The ten-minute Desperanza stay and optional Ready Up are current experimental slice settings. | Production population, public matching, raid-level crossing rate, and the number of human Raiders simultaneously available to a world. The two development identities are not a population test. |
| Shared worlds and locations | The server owns a shared timeline, location graph, conditions, robots, loot, and outcomes. Actual participants can share the humming-crate search/contest, robot fights, and an extraction vehicle when their actions and locations overlap. | General co-presence rate and same-location rate across all events. No telemetry currently establishes either tier (a) denominator. |
| Shared robot fights | BAL-2 has historical shared-fight telemetry. **Measured (docs):** under BAL-2, the `[0,8)` window had 114 shared participant fights among 1,661 participant fights (114 / 1,661 = **6.9%**); `[8,24)` had 22 / 3,481 (**0.6%**); `[24,48)` 58 / 5,278 (**1.1%**); `[48,96)` 112 / 10,649 (**1.1%**). Source: [BAL-2 measurements](balance/BAL-2-measurements.md#robot-outcomes-and-rewards-by-window). | Those are shares of participant fights, not the share of raids with a shared fight, not all shared committed events, and not a current-population forecast. No tier (b) raid-level baseline is reported. |
| Shared activities | The current bounded world has shared searches and a Salvage Yard contest with committed allocation; solo search remains solo. | No raid-level rate for shared activities or their outcomes. Current balance harnesses do not represent large populations. |
| Desperanza residence | Raiders reside in the Hub for ten minutes on enrollment and return; current nonreward activities commit privately for the individual Raider. The Hub roster reports presence. Ready Up spends 2 Signal. | No designed Hub encounter between two Raiders, and no Hub crossing rate. Residence does not currently imply a shared social activity. |
| Story beats | A separate story-system design exists as a proposal. Current authored narration and optional AI event asides are not a story-beat implementation. | No story-beat baseline or telemetry. |

**Measured (docs):** the BAL-2 and BAL-4 harnesses run two unattended Raiders per seed. They can test the existing two-Raider rules and outcomes; they cannot establish population effects, natural matchmaking rates, a 45% co-presence result, or how map fragmentation behaves. BAL-2 windows also use settled participant fights as their denominator, which differs from the raid-level metrics proposed here.

### Instrumentation needed before comparing aims

For each committed event, record stable event and raid IDs, world/map/location IDs, server sequence/time, event kind, actual participant IDs, participant type (human, test-simulated, or AI), event scope, and whether there was a committed shared outcome. Record story beat ID and version separately. Record admission, arrival, departure, and settlement times. Use authorized IDs, not display names, for aggregation.

Derive all three numerator flags per raid from that event stream; deduplicate by raid and tier. Keep unknown/missing participant or event data visible as an instrumentation failure, not a solo raid. Retention, privacy, and deletion rules must be approved before durable event history is added.

## 3. Levers and population effects

### Proposed levers

| Lever | Expected effect | Cost | Risk and balance impact |
|---|---|---|---|
| **Admission pacing and world size** | Longer admission overlap or a modestly larger shared world lets more actual Raiders enter before routes diverge. Current 30-second pulse and cap of two are useful test settings, not public policy. | More concurrent world state and more demanding fairness, projection, and settlement testing. | Larger groups may crowd locations and change encounter frequency. Simply increasing capacity is not proof of more same-location events. No balance change if it only alters admission and does not change mechanical event frequency; any event-frequency shift needs matched measurements. |
| **Shared ambient events** | Makes an already co-present moment visible and gives a quiet shared world more texture. | Authored catalog, eligibility/cooldown rules, journal projection, and repetition telemetry. | **Safe for current balance measurements only if cosmetic:** no loot transfer, damage, coins, mood/greed/Signal changes, timers, movement, RNG consumption, or altered decision ordering. It does not count toward tier (b) unless a real outcome exists. |
| **More individual (solo) events** | Adds variety during solo travel, searches, and Hub residence; provides authored fallback even in empty worlds. | Authored variants, trait review, private journal handling, repetition testing. | No balance effect only if it is presentation-only and consumes no gameplay RNG or time. Events that alter behavior, rewards, or pacing need their own approval and measurement. |
| **Hub encounters between raids** | Uses the existing ten-minute Desperanza residence to show actual residents in the same Hub moment, independent of raid population. | New shared Hub presence/event scope, identity and privacy rules, and Hub event logging. | Hub presence must not be mislabeled as raid co-presence. Keep Hub actions cosmetic in this proposal; no trader stock, economy, Handler controls, or Signal changes. |
| **Location-overlap rules** | Routes or ambient-event eligibility can make actual Raiders more likely to share a location at an event. | Route schedule and path-overlap measurement; careful handling of simultaneous deadlines. | Deterministic path changes can alter search, combat, extraction exposure, and loot opportunities. A cosmetic shared event is safe; changing routes or event eligibility in a way that changes mechanics is not and requires separate balance measurement. |
| **Scheduling Raiders into the same world windows** | Raises co-presence most directly at low population by delaying one admission briefly for another eligible Raider. | Queue visibility, bounded wait, consent/eligibility, and a deterministic solo fallback. | Forced waiting can make autonomous play feel broken and strand quiet worlds. At low population the 45% aim is otherwise unreachable under the illustrative model below. Do not block a raid indefinitely. |
| **Later: matchmaking by activity** | Can match Raiders whose route, danger, or timing preferences create more useful encounters. | Durable identity, history, privacy, fairness, and population telemetry. | Can fragment queues, create repeat-pair or gear unfairness, and change danger/event mix. Match by broad activity only after S6 and measured population; do not expose private inventory or history. |

### Transparent estimate model

The following is a deliberately small **Estimate**, not current behavior or a forecast. It shows why population, world assignment, raid duration, and route overlap all matter.

Let:

- `N` = eligible human Raiders concurrently available to the same admission/matching window; they may still be in the Hub or in different raid phases.
- `C` = world capacity; use **8 only as a proposed illustration**, not a current or approved capacity.
- `W = ceil(N / C)` = number of worlds receiving that cohort, assuming even, independent assignment.
- `D` = target raid duration.
- `T` = typical interval between a peer raid's start and the target raid's start in that pool.
- `u(D) = min(1, (D + D_other) / T)` = simple phase-overlap estimate for a target raid and a peer raid of duration `D_other`; it is a rough uniform-start approximation, not an observed distribution.
- `g` = probability that raids overlapping in time also have a qualifying committed event while both are in the same world. Let `h` be the additional probability that they are in the same location at that event.
- `q(D) = u(D) × g` = chance one peer assigned to the same world creates a qualifying event-time crossing during the target raid.
- `r = 0.35` = **Proposed illustrative** chance a co-present pair has a real shared committed outcome in the raid, after ambient-only meetings are excluded.

With independent assignments, the illustration is:

```text
P(co-presence) = 1 - (1 - q(D) / W)^(N - 1)
P(same-location crossing) = 1 - (1 - q(D) × h / W)^(N - 1)
P(shared outcome) = 1 - (1 - q(D) × r / W)^(N - 1)
P(story beat) = s_solo × (1 - P(shared outcome))
               + s_shared × P(shared outcome)
```

For the labelled table only, set `q(D) = 0.25`, `r = 0.35`, `s_solo = 0.18`, and `s_shared = 0.35`; these are **illustrative assumptions**, not measurements or recommended runtime rates. They represent one effective same-world crossing opportunity per peer, a proposed solo story opportunity, and a proposed shared-event beat opportunity. `h` is not used in that table; same-location overlap is a stricter submetric that needs location telemetry. Real event cadence, cooldowns, route overlap, active raid duration, and arrival patterns must replace these assumptions with telemetry. The formula ignores group dependence, changing populations, return cycles, and correlation between events.

| Concurrent eligible Raiders `N` (**scenario input**) | Illustrative worlds `W` at capacity 8 | Co-presence per raid (**Estimate**) | Shared committed outcome per raid (**Estimate**) | Story beat per raid (**Estimate**) |
|---:|---:|---:|---:|---:|
| 2 | 1 | 25% | 9% | 19% |
| 4 | 1 | 58% | 24% | 22% |
| 8 | 1 | 87% | 47% | 26% |
| 16 | 2 | 87% | 49% | 26% |
| 64 | 8 | 86% | 50% | 27% |

These are not forecasts. In particular, the table is sensitive to `q(D)`: long overlapping raids or forced scheduling raise it; brief/disjoint raid windows, lower occupancy, or path separation lower it. If two Raiders are not in the same time window, no increase in map count or ambient narration can make a real crossing. At two eligible Raiders, the model's 25% co-presence is below the 45% aim; reaching it requires a higher observed `q(D)` (for example, deliberate bounded scheduling). If a population is too small to produce a second eligible Raider during the wait limit, that aim is unreachable without a labeled simulated participant, which must not be counted as a human crossing. The 15-25% shared-outcome aim is near the illustrative estimate at four Raiders but is not attainable merely by adding cosmetic ambient events. The story estimate uses both solo and shared opportunities; its rate must be adjusted with cooldowns to keep beats meaningful.

Use actual admission timestamps and per-raid durations to fit or replace these assumptions. Report both natural and deliberately scheduled cohorts; do not claim a target from the formula.

## 4. Shared ambient event catalogue

### Record and eligibility proposal

An ambient event is a **shared, committed, presentation-only observation**. It may make co-presence legible, but it has no mechanical outcome and therefore does not count as a tier (b) shared committed event.

Conceptual record (field names are proposals, not a schema):

```json
{
  "eventId": "stable-server-event-id",
  "worldId": "server-world-id",
  "mapId": "map-id",
  "locationId": "location-id",
  "sequence": 123,
  "kind": "SharedAmbient",
  "catalogId": "ambient-example-id",
  "participantIds": ["actual-raider-a", "actual-raider-b"],
  "facts": ["both participants were present at this location"],
  "mechanicalEffects": [],
  "committedAt": "server-sequence-or-time",
  "contentVersion": "approved-catalog-version"
}
```

**Eligibility, proposed:** at least two actual current participants must be present in the same location at the committed event time; nobody is added from a snapshot. Participants must be eligible to see that location's event. Do not reveal private health, inventory, Handler activity, or hidden decisions. Ambient events do not change positions, loot, condition, damage, mood, greed, coins, Signal, deadlines, or other state. A hub ambient event uses a distinct `HubAmbient` scope and does not count as a raid tier (a) event.

**Cooldowns, proposed for a pilot:** at most one ambient event per world every two minutes; at most one occurrence of the same catalog ID per world in 30 minutes; at most one ambient line per Raider per five minutes. These are repetition safeguards, not validated cadence. The event selector must not consume gameplay RNG or delay a gameplay deadline; use a stable, separate event-selection stream or a nonrandom ordered rotation.

**Participant and journal rules:** select only Raiders actually present when the server commits the event. Write one shared canonical line from committed facts and fan it out under the same event ID to each authorized participant. Each Handler's private journal may add an owner-only reaction (authored first, optional AI later) keyed to that same event ID; it cannot change the shared line or reveal another Raider's private state. Retries upsert the same event; they never create a second event or settlement. If a participant leaves before commit, they are not added retrospectively.

**Example proposals below are not canon or runtime content.** The named locations, robots, and phenomena are listed in [zones](lore/entities/zones.md), [robots](lore/entities/robots.md), or [field phenomena](lore/entities/field-phenomena.md). Each is cosmetic: no loot transferred; damage was 0.

| # | Proposed shared ambient line |
|---:|---|
| 1 | At Damp Battlegrounds, both Raiders watched Apology Weather arrive; no loot transferred and damage was 0. The forecast has apologized to the puddle. |
| 2 | At Buried City (Now 30% more buried), both Raiders heard Humming Crate Migration nearby; no loot transferred and damage was 0. The crate has relocated without submitting a forwarding address. |
| 3 | At Chassis Graveyard, a Tattletale drone looked at both Raiders; no loot transferred and damage was 0. The report was filed under "still here." |
| 4 | At The Staycation Celebration, both Raiders watched a tent flap once; no loot transferred and damage was 0. The shelter has completed its daily exercise. |
| 5 | At Forgotten Fields, Polite Glyphs appeared beside both Raiders; no loot transferred and damage was 0. The warning has requested that danger lower its voice. |
| 6 | At Sunken Highrise, both Raiders heard a floorboard complain; no loot transferred and damage was 0. The building has submitted a maintenance request to gravity. |
| 7 | At The Breach, both Raiders saw Signal Shadow pass over the route; no loot transferred and damage was 0. The radio remained quiet with unusual confidence. |
| 8 | At Damp Battlegrounds, the Receipt Printer of Doom printed an empty receipt near both Raiders; no loot transferred and damage was 0. The machine has audited the absence of a purchase. |
| 9 | At Buried City (Now 30% more buried), Tremor of Bad Priorities made the signage wobble; no loot transferred and damage was 0. The directions are now uncertain in two dimensions. |
| 10 | At Chassis Graveyard, an Anxietick paused beside both Raiders; no loot transferred and damage was 0. The machine has requested a second opinion from itself. |
| 11 | At Sunken Highrise, a Passive-Aggressor Drone hovered over both Raiders; no loot transferred and damage was 0. Its silence has been escalated to management. |
| 12 | At The Staycation Celebration, both Raiders heard the wind move through an empty tent; no loot transferred and damage was 0. The attendance policy remains unenforced. |

If a future line needs a proper noun not present in the cited entity tables, add and review its mapping row before using it. Do not treat this proposal's examples as that mapping approval.

## 5. Individual (solo) event catalogue

Solo events are private, committed diary moments for one actual Raider. They may occur in a raid or during Desperanza residence, but the event record identifies its scope. They do not create an opponent, a story beat by themselves, or a gameplay effect. The shared-world record must not disclose a private solo line to another Raider.

**Eligibility, proposed:** one eligible Raider is at the recorded location/state; the event's factual prerequisites are present; its cooldown has elapsed. Avoid sensitive or unrecorded inferences. Use the same event ID, event version, and upsert rule for authored text and any optional later AI voice. Presentation-only events have an empty mechanical-effects list and do not consume gameplay RNG or time.

**Cooldowns, proposed:** at most one solo ambient event per Raider every two minutes; do not repeat the same catalog ID for that Raider within 30 minutes; suppress an event when its factual predicate is unknown. These are initial safeguards to test, not cadence commitments.

The trait is a voice lens, not a rules input. Do not add or remove items, alter navigation, or change damage, danger, loot, or timing by trait. The sample lines below are proposals, not canon; all locations and phenomena are in the lore entities linked above.

| # | Proposed individual event line |
|---:|---|
| 1 | At Damp Battlegrounds, the Raider watched Apology Weather cross the water; no loot moved and damage was 0. The clouds have accepted responsibility without changing the forecast. |
| 2 | At Buried City (Now 30% more buried), the Raider heard Humming Crate Migration; no loot moved and damage was 0. The sound has been added to the list of things not carried home. |
| 3 | At Chassis Graveyard, the Raider saw a Tattletale drone turn away; no loot moved and damage was 0. The machine has scheduled its judgment for later. |
| 4 | At The Staycation Celebration, the Raider stood beside a tent; no loot moved and damage was 0. The camping strategy remains indistinguishable from waiting. |
| 5 | At Forgotten Fields, Polite Glyphs appeared on a sign; no loot moved and damage was 0. The sign has asked the Raider to reconsider the sign. |
| 6 | At Sunken Highrise, the Raider listened to the stairs settle; no loot moved and damage was 0. The building has completed a small administrative sigh. |
| 7 | At The Breach, Signal Shadow crossed the radio; no loot moved and damage was 0. The silence arrived with excellent reception. |
| 8 | At Damp Battlegrounds, the Raider noticed a dry patch; no loot moved and damage was 0. It has not yet been assigned a strategic value. |
| 9 | At Buried City (Now 30% more buried), the Raider read a faded direction sign; no loot moved and damage was 0. The sign is confident about a street that is no longer available. |
| 10 | At Chassis Graveyard, the Raider heard metal settle; no loot moved and damage was 0. The ruins have filed the noise as routine maintenance. |
| 11 | At Forgotten Fields, the Raider watched a shadow pass across the grass; no loot moved and damage was 0. The field has declined to identify its supervisor. |
| 12 | At The Staycation Celebration, the Raider inspected an empty chair; no loot moved and damage was 0. The chair appears to have secured the best seat. |

**Trait variation proposal — same facts, no mechanical change:** for event 8, choose only one authored phrasing variant from the Raider's existing trait. These are examples of voice, not new personality canon:

| Trait | Proposed private reaction |
|---|---|
| Brawler | “I inspected the dry patch. It has declined to make this a contest.” |
| Cautious | “I noted the dry patch from a prudent distance. The distance was sufficient.” |
| Gadgeteer | “I assessed the dry patch for technical potential. It remains mostly dry.” |
| Hoarder | “I considered keeping the dry patch. Storage capacity remains a concern.” |

All variants describe the same observed facts, state no new outcome, and change no simulation state. If traits do not fit playtest feedback, keep a neutral line rather than inventing additional trait mechanics.

## 6. Multi-map design

The proposal must work with one map now and allow several later. Reuse existing AFK-original zones; do not create new canon or assume every lore zone is currently a playable map. A map is a route/location set. A world is one authoritative live instance with actual participants and one server timeline.

| Option | Low-population crossing effect | Content cost | Loot/balance coupling | Migration from current single map |
|---|---|---|---|---|
| **(a) Fully separate maps, separate worlds** | Lowest natural cross-map probability; each map fragments the same eligible population. Nearly empty maps are likely at low population. | Highest: separate active world pools, route/event coverage, observability, and map-specific testing. | Strongest coupling: each world has its own loot opportunities, danger/condition mix, and participant composition. Comparisons need map-stratified cohorts. | Keep the current world as the default. Add another world pool only after a population gate; never silently move a live Raider or copy its state. |
| **(b) Districts of one shared world** | Co-presence can remain high at world scope, but same-location meetings fall as Raiders spread across districts. | Moderate: each district needs locations and event coverage, but one timeline and population. | Shared economy/world resources couple districts; ensure any finite stock or shared rewards are still allocated exactly once. Crossing telemetry must include district and location. | Add a district identifier and connected routes within one world; preserve existing locations and event ordering. |
| **(c) Hub-and-routes** | Keeps one population anchor in Desperanza; route selection can concentrate or disperse raid traffic. The Hub is a separate scope from raid co-presence. | Moderate and extensible: reuse zones as routes/regions and add reviewed links before new map content. | Shared world ownership remains central; danger, loot tables, and rewards stay attached to their authoritative location/opportunity. Route changes can alter exposure and need balance measurements. | **Recommendation:** keep the present single map/route graph, make map selection a route choice later, and keep Desperanza as the common home. Add routes without changing existing location IDs or settlement semantics. |

**Zone reuse, not new canon:** existing candidates include Damp Battlegrounds, Buried City (Now 30% more buried), The Breach, Chassis Graveyard, Sunken Highrise, Forgotten Fields, and The Staycation Celebration from [zones.md](lore/entities/zones.md). These are lore identities, not approval to make each a map or to change its balance.

**Opening rule, proposed:** do not open a second separate world pool until at least **16 eligible human Raiders are concurrently active on average during the same daily peak window for seven consecutive days**, and crossing telemetry shows the primary world is consistently at its approved capacity. This is a conservative proposed gate, not a measured sufficiency threshold. If these conditions are not met, keep one shared map/world. When a route or separate map is quiet, stop directing new admissions there and return eligible Raiders to the primary route after their current committed activity; do not strand them, teleport active participants, create ghost Raiders, or represent snapshots as live opponents. Review and approve the quiet-world and migration rule before implementation.

## 7. AI roles for directing systems

The 2026-11-06 decision makes these valid design options; none is approved for implementation here. In every case, the simulation server validates choices and commits facts. An AI output is not itself a committed outcome.

| Role | Value | Cost | Latency tolerance | Main risk | Required record and fallback |
|---|---|---|---|---|---|
| **(a) Offline authoring assistant** drafts event and beat variants for human review. | More candidate lines and trait variants without hand-writing every first draft. | Model/API cost, editorial and legal review, catalog maintenance. | Minutes or hours; never in a gameplay path. | Imported phrases, off-tone content, unsupported facts, or a draft being mistaken for approved canon. | Keep prompt/model and draft provenance in review workflow; only approved text gets a catalog ID/version. No runtime event or gameplay state is created from a draft. Human-reviewed authored fallback is always available. |
| **(b) Slow event director** selects among server-ranked legal events every few minutes. | Can vary timing and event choice while the server constrains facts and eligibility. | Inference cost, candidate ranking, validation, observability, and replay storage. | Seconds to minutes, outside gameplay deadlines; stale choices expire. | Invalid or stale selections, repeated event patterns, provider outage, biased pacing. | Record world/event sequence, candidate IDs and server ranks, eligible participants, input facts, model/prompt/policy versions, selected candidate, decision ID, validation result, and fallback. If late or invalid, use the top deterministic server-ranked option. Deduplicate before commit. |
| **(c) AI telemetry and balance analyst** summarizes measured crossing, cadence, and playtest data for designers. | Finds patterns and suggests questions for human review. | Data pipeline, model cost, privacy review, analyst verification. | Post-run or scheduled batch; no gameplay deadline. | Misread denominators, fabricated causality, or sensitive player data being sent to a provider. | Record dataset/query/run versions, fields sent, model/prompt, outputs, and human conclusions. Advice cannot change worlds, rules, or settlement. Use minimized/pseudonymous data and approved provider/privacy terms. |
| **(d) AI voicing reactions** to committed events (current optional role). | Adds optional variation while authored narration remains immediate. | Existing model gate or provider/API expense and content validation. | Bounded asynchronous enrichment; never block simulation. | False claims, privacy leaks, latency, rejection, and cost. | Record source event ID, exact authorized facts, model/prompt/content versions, accepted output or authored fallback, status/reason, and idempotent journal upsert. No prose can alter a shared fact or settle an item. |
| **(e) Learned or AI-assisted Raider decision policy.** | Could select among legal navigation, activity, extraction, or economy actions using state and trait context. | Training/evaluation, model calls or inference compute, decision logging, adversarial and fairness tests. | A decision deadline must remain server-owned; use the existing deterministic policy if the model times out, is unavailable, or returns an invalid action. | Opaque policy, unstable outcomes, hidden balance shifts, runaway spending or risk, and replay divergence. | Record state snapshot/version, legal candidate list and ranking, model/policy version, request/seed or provider response ID, selected action, validation, fallback, server sequence, command/idempotency key, and committed effect. Never bypass stock, price, ownership, authorization, or settlement invariants. |

**Recommendation order:** (1) use AI offline to draft proposals for human review; (2) improve the current optional event-voicing role only after refreshed latency, acceptance, cost, and privacy measurements; (3) run AI telemetry analysis on minimized reports; (4) test a slow event director in a deterministic sandbox; (5) test a learned Raider policy only after the recorded-decision and fallback path is proven. This order buys learning before live decision risk.

**Repeatable tests and balance runs:** the primary matched-seed balance harness should use AI off or a deterministic stub with fixed, versioned choices. If a real AI decision is studied, record its full input, candidate set, output, validation, and fallback once; replay that trace for paired comparisons where the same choice remains legal. If it is no longer legal in a comparison arm, take the recorded deterministic fallback and report the divergence rather than silently asking the model again. Never include a live, nondeterministic provider call in the reference matched-seed run. Decision records and idempotency keys must prevent a replay or retry from applying a choice or settlement twice.

## 8. AI agents in the world — experimental options

An agent is not a human participant. An AI controlled entity must be labeled in the UI, roster, event record, exports, and telemetry. It must not impersonate a human Raider, use a human identity, or make a saved snapshot look live.

| Experiment | Benefit | Limits and safeguards | Where it may help |
|---|---|---|---|
| **(i) AI-controlled Handler using only existing Signal actions** | Tests bounded advice or timing around existing signals without adding a player action type. | Use the same Signal balance, authorization, deadlines, and validation as a human Handler. No buying, selling, equipping, repairs, or loadout controls. Label the Handler as AI; rate-limit, log, and allow disablement. A timeout uses the already-approved no-input behavior. | A separate Development/simulation cohort can exercise limited actions. It does not add a Raider or make a quiet raid socially populated. |
| **(ii) AI-controlled Raider in a separated, flagged sandbox** | Exercises actual server-owned paths, shared events, and decision/replay logging when human populations are unavailable. | Give it an explicit AI/test identity and controller version. Keep it in an isolated sandbox or clearly labeled test world; apply normal ownership and exactly-once settlement rules. Measure AI behavior separately. Do not count it toward human-human crossing or balance cohorts. | Can test load and encounter flows; only deliberately configured test runs may use it to explore sparse-population behavior. It must never be shown as a live human. |
| **(iii) Clearly labeled AI resident/NPC** | Adds visible Hub activity and tests whether the world feels inhabited without implying that a human is online. | Mark it “AI” or “NPC” wherever shown; do not claim it is an actual player Raider. Keep it non-mechanical and out of human population metrics. If it enters raids, it becomes a labeled AI participant with the stricter sandbox rules above. | Can add Hub flavor in a quiet Desperanza without manufacturing human raid meetings. |

**Identity and anti-abuse:** there are only two hard-coded Development identities today; there is no production identity system. Do not mint extra human-looking accounts or reuse those identities for agents. Reserve a separate agent identity namespace, limit agent count/actions, validate every command as an ordinary server command, and retain an operator kill switch. Durable identity, ownership, and recovery are prerequisites before any public agent participation.

**Local versus cloud model choice:** a local model avoids sending prompts to an external provider and can have predictable per-call fees, but it consumes host compute and may be slow or unavailable. Current local CPU narration samples are not decision-agent or concurrency benchmarks. A cloud model may offer different latency/capacity and simpler operations, but adds usage fees, variable latency, provider availability/quota risk, and a need to decide exactly which player/world data may leave the service. Store API keys in a managed secret facility, never in the repo, client, or logs. For every provider, review retention/training terms, data location, access control, and failure handling. Do not send private inventory, health, identity, or conversation history unless the approved design requires it and consent/privacy rules permit it.

**Cohort separation:** tag every entity and event with `human`, `ai-agent`, `npc`, or `test-simulated` provenance. Default reports use `human` only. If the experiment intentionally measures AI, publish separate human-human, human-AI, and AI-only results. No AI-generated event counts as a human crossing, even if its simulation entity occupies a real world.

**Smallest first experiment — recommendation:** after approval, run an **isolated, non-production AI Handler shadow test** over a deterministic replay or test simulation. Let it propose only existing limited Signal actions; initially record the proposal but do not apply it. Compare its legal choices, latency, fallbacks, and costs against a deterministic baseline. Only after that succeeds should a separate approval allow the same constrained Handler to apply actions in a labeled Development world. This needs no fake human identity, creates no fake live opponent, and does not change the balance harness. A later AI Raider or resident experiment must stay labeled and separated as above.

**Success means:** humans can always distinguish agent/NPC from human; the agent makes only legal, logged choices; failures fall back cleanly; repeated requests do not duplicate actions or settlement; human-only metrics exclude agents; and a playtest shows a specific benefit (for example, better sandbox coverage or useful Hub texture) without confusion or unfairness. “More entities on screen” alone is not success.

## 9. Prerequisites and ordering

| Order | Prerequisite or work | Why / status |
|---:|---|---|
| 1 | **Approval of this proposal's decisions** | No implementation or gameplay change is authorized by this document. Keep the current experimental balance candidate and frozen rules unchanged. |
| 2 | **S6 durable identity, persistence, and recovery design/implementation** | Current Development identities, worlds, and event state are in memory and disappear on restart. Durable identity and safe recovery are needed for persistent human presence, reliable multi-map admission, agent identity, and cross-raid memory. S6 is design-only in the current ledger. |
| 3 | **Event retention, privacy, and deletion policy** | All three metrics depend on event-level history; story memory and AI inputs increase sensitivity. Define retention and privacy before durable logs or provider calls. |
| 4 | **Crossing instrumentation** | Instrument raids, world/location occupancy at committed events, shared outcomes, story beat IDs, and entity provenance before tuning an aim. Do not infer the metric from shared robot fight counts. |
| 5 | **Small deterministic population harness** | The existing balance harness has two Raiders per seed. A separate population/overlap harness is needed to explore 2/4/8/16/64, varied raid durations, world capacities, and route overlap without altering the balance cohort. |
| 6 | **Story-system vertical slice** | The story design is a proposal. Approve an event-backed record, authored fallback, idempotency, privacy, and story-beat measurement before claiming a tier (c) baseline. |
| 7 | **Cosmetic ambient and solo event pilot** | Only after event eligibility, cooldowns, non-mechanical invariants, catalog review, and presentation are approved. Use authored lines first and compare exact balance telemetry. |
| 8 | **Shared-outcome, map, or live-agent experiments** | Separate approval and measurement required if changing paths, encounter rates, rewards, danger, or population rules. Public use additionally needs durable identity and operational readiness. |

**Can be done now:** this proposal, its decision review, and (after approval) a separate test design for instrumentation/harness acceptance. **Must wait:** implementation, runtime content/catalog changes, additional AI models or API keys, public matching, new maps/world pools, AI participant deployment, and any mechanically consequential activity. No work in this table is active.

## 10. Measurement plan

Every target and cadence below is experimental until measured and approved. Use current harness inputs where applicable, but keep the existing two-Raider Arm 4B + Arm 5 balance cohort unchanged as a control.

| Measure | Definition and report | Method |
|---|---|---|
| Tier (a), co-presence | Distinct human raids with another actual human present at a committed event / all eligible human raids. Also report same-location share, Hub separately, median participants, and event-time overlap. | Record occupancy and participant IDs at event commit. Segment by population size, world size, map, raid duration, and natural versus scheduled admission. |
| Tier (b), shared committed event | Distinct human raids with a real shared outcome / all eligible human raids. Split robot fights, shared searches/activities, extraction, and other approved kinds. | Use authoritative outcome/event IDs and participant sets; report unique worlds/events as well as participant counts. Do not count ambient events. |
| Tier (c), story beat | Distinct human raids with a committed story beat ID / all eligible human raids. | Record beat ID/version, source event, recipients, fallback/AI status, and cooldown decision. Do not count ordinary Comms lines or AI asides. |
| Ambient and solo cadence | Shared ambient and solo events per raid and per Raider-hour; Hub counts separate. | Report eligible opportunities, emitted events, suppression reasons, and participant counts. Confirm no change to simulation state or gameplay RNG. |
| Cooldown compliance | Attempts, eligible events, blocked repeats, and any duplicate event IDs by world/Raider/catalog ID. | Deterministic tests plus live telemetry; any duplicate commit or cooldown violation blocks pilot expansion. |
| Repetition | Exact catalog ID repetition by Raider, pair, world, and recent event sequence; human playtest reports of recognizable/repetitive lines. | Compare against cooldown limits and review over multiple sessions; do not call a low raw duplicate rate proof of good writing. |
| Balance invariance | Existing Arm 4B + Arm 5 outputs: KO, healing, loot value, coins/assets, weapons, shields, and admission funnels. | Cosmetic-only event runs must match control state and outcome outputs exactly for matched seeds. If event selection changes RNG, timing, route, admission, or an existing mechanic, it is not a cosmetic-only test; stop and define a separate BAL-3/reward measurement. |
| Population formula | Compare measured crossing rates with the illustrative model for 2/4/8/16/64 concurrent eligible Raiders. | Run a separate simulator across matched population/raid-duration/arrival scenarios. State each assumption and do not treat simulated population as real-player evidence. |
| AI options | Latency distribution, cost, availability, invalid/stale choices, fallback rates, privacy fields sent, and decision/log consistency. | Begin in AI-off or deterministic-stub mode. If testing live AI, capture the decision trace and replay it; report provider and model versions. |
| Playtest questions | Did participants notice the crossing? Did ambient events feel shared without confusing or overstating outcomes? Were solo lines varied and trait distinctions readable? Did meetings remain meaningful at the proposed rate? Could humans immediately identify AI/NPCs? | Ask after sessions, not from click counts alone. Include quiet/solo sessions, repeated pairs, different locations, and both Handler-present/absent cases when the prerequisite systems exist. |

Use the existing balance harness's seed IDs **1-64** for matched mechanical regression checks when its inputs remain comparable. That harness's two Raiders per seed are not the proposed population experiment. A separate 2/4/8/16/64 population harness should use common arrival and event seeds across scenarios and record its own version and assumptions. For any measured mechanical change, follow the applicable balance process rather than claiming the ambient-only invariance check is sufficient.

## 11. Risks and what could go wrong

| Risk | What could go wrong | Mitigation or decision gate |
|---|---|---|
| Repetition | More lines make the same joke feel automated. | Per-world/per-Raider cooldowns, stable selection rotation, repetition telemetry, and human review. |
| Tone drift | AI or authored variants over-explain, imitate another game's prose, or add canon accidentally. | Original AFK names, tone/legal review, proposals not canon, human-approved catalog, and factual validation. |
| Crowding | Larger shared worlds overload a location or make route choice meaningless. | Measure same-location density and wait times; increase one variable at a time; preserve safe solo progress. |
| Empty worlds | Dividing the population across maps or queues creates silent instances. | One map until a sustained population threshold; quiet worlds stop receiving new admissions; no fake humans. |
| Cross-map fragmentation | A new map splits meeting opportunities and makes every map feel empty. | Hub-and-routes recommendation; use measured population and map-stratified metrics before opening a pool. |
| Meetings lose meaning | If every raid has several meetings, shared events become background noise. | Keep shared outcomes and story beats rarer than co-presence; test the proposed tier gap and pair repetition. |
| AI cost and latency | Provider bills, queue delays, rate limits, or outages arrive at peak load. | Authored fallback, strict budget/quotas, bounded async calls, offline/stub tests, and explicit cost/privacy approval. |
| AI confusion | A player mistakes a bot, NPC, or old snapshot for a human. | Persistent labels and distinct identities in UI, logs, telemetry, and support tools; never label a snapshot as live. |
| Unfairness | A model's choices or schedule gives an agent better information or repeated advantage. | Same legal actions/visibility, no private-data advantage, recorded decisions, sandbox cohort, and human-only reports. |
| Privacy | World history or private Raider facts are sent to a model or retained too long. | Minimize and pseudonymize; define retention/deletion; review provider data terms; send no private fields by default. |
| False balance confidence | A two-Raider harness or cosmetic-only test is misread as proof at 64 or public scale. | Keep the harness limitation next to every result; separate simulated population from human evidence and mechanical tests. |

## 12. Decisions for the designer

Recommendations are proposals, not presumed approvals.

| Decision | Recommendation | Reason |
|---|---|---|
| **1. Co-presence aim** | Keep **about 45%** as a loose experimental aim, not a launch gate. | It is measurable once occupancy events are logged. The illustrative estimate is 25% at two Raiders and 58% at four with `q(D)=0.25`; low population needs deliberate bounded scheduling. |
| **2. Shared committed event aim** | Use **15-25% of raids** as a lower experimental aim. | This keeps a real shared outcome less common than merely seeing another Raider. Under the illustrative assumptions, the estimate is about 9% at two, 24% at four, and 47% at eight; eight is a warning that frequency or world size may need tuning. |
| **3. Story-beat aim** | Use **20-25% of raids** as an experimental range. | About one in four or five can feel notable. The estimate includes solo and shared beat opportunities; cooldowns and playtests must prevent ambient jokes from becoming constant. |
| **4. First experiment: simulated population or wait for real players?** | Start with **simulated population in a separate deterministic harness**, then validate with consenting real players only after S6 and privacy/identity prerequisites. Do not put simulated Raiders in a human-facing world. | It can test population math and instrumentation at 2-64 without inventing production accounts or claiming human social validation. It cannot answer whether real meetings feel good. |
| **5. First activity slice** | Approve a cosmetic shared/solo diary pilot only after instrumentation; keep every mechanical effect at zero. | It is the lowest-risk way to increase ambient and individual activity without invalidating the frozen balance candidates. |
| **6. Map model** | Keep one map now; prefer a hub-and-routes model later; defer separate world pools until the proposed sustained-population gate is met and reviewed. | This minimizes low-population fragmentation and reuses existing zone lore without asserting new maps are already playable. |
| **7. World size and admission wait** | Keep the current two-Raider setting as a Development constraint until a separate population experiment compares capacities and a bounded wait/fallback. | The table's capacity eight is an illustration, not evidence or approval to raise the current cap. |
| **8. AI order and first agent** | Approve offline authoring/telemetry analysis first; if an agent experiment is desired, start in shadow mode as an AI Handler using only existing Signal choices. | It creates no fake opponent, new action, or additional development identity and gives measurable decision/fallback evidence before an agent can affect play. |
| **9. Event retention and privacy** | Require a retention/deletion and provider-data policy before persistent event history or cloud AI uses player-specific context. | Cross-raid memory and telemetry make privacy a product requirement, not merely an implementation detail. |
| **10. Separate balance work** | Keep chip damage, rewards, loot changes, route mechanics, and any new mechanical shared activity out of this approval; route them through BAL-3/reward review. | Those effects can invalidate existing balance evidence and need explicit, focused measurements. |

## 13. Out of scope

- Implementing any proposal in this document, adding runtime content, changing tests, or changing balance, rules versions, or compatibility fingerprints.
- Changing the experimental Arm 4B + Arm 5 candidate or starting a new balance arm.
- Changing trader-only Temptrest, once-per-raid weapon wear, deterministic ownership or once-only settlement.
- Handler economy controls; direct PvP, theft, or item transfers; presenting saved opponents as live; changing extraction, KO, or Secret Hidden Pocket semantics.
- Adding damage, rewards, loot opportunities, mechanical hazards, or other BAL-3/reward effects.
- Enabling additional maps, public matchmaking, production accounts, or increasing current Development capacity.
- Deploying an AI Handler, Raider, NPC, model provider, API key, or cloud service.
- Treating examples as canon, adopting any named character/zone/robot/phenomenon beyond the existing lore entities, or reusing source-game prose.
- Restarting a backend, committing, or pushing.

## Sources consulted

- [START-HERE.md](START-HERE.md), [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md), [GAME_DESIGN.md](GAME_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md), [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md), and [STORY_SYSTEM_PROPOSAL.md](STORY_SYSTEM_PROPOSAL.md).
- [BAL-4 arms](balance/BAL-4-arms.md), [BAL-2 measurements](balance/BAL-2-measurements.md), and the [current README](../README.md).
- Lore [README](lore/README.md), [PARODY_MAPPING.md](lore/PARODY_MAPPING.md), [BACKLOG.md](lore/BACKLOG.md), [legal guardrails](lore/content-guidelines/LEGAL_GUARDRAILS.md), [tone guide](lore/content-guidelines/TONE_VOICE.md), and entity references for [Desperanza](lore/entities/desperanza.md), [factions and brands](lore/entities/factions-and-brands.md), [field phenomena](lore/entities/field-phenomena.md), [robots](lore/entities/robots.md), [skills](lore/entities/skills.md), [traders](lore/entities/traders.md), [weapons](lore/entities/weapons.md), [world history](lore/entities/world-history.md), and [zones](lore/entities/zones.md).
