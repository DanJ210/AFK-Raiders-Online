# Shared Story System — Design Proposal

**Status: Proposal, awaiting designer approval. No story-system implementation is active.**

This is a design document only. It does not authorize code, runtime content, gameplay, balance, rules-version, or fingerprint changes. The counts and limits suggested below are proposals, not validated targets. Quantitative runtime evidence is marked **Measured (docs)**; projections are **Estimates**; future choices are **Proposals**.

The former assumptions that AI only narrated and inference had to remain local/offline were removed on 2026-11-06; see the ledger's [decision-authority row](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint) and [Assumptions changed on 2026-11-06](#assumptions-changed-on-2026-11-06). This revision considers the newly allowed options, but the proposal remains unapproved.

## 1. Summary and recommendation

Make the shared event the unchanging part of the story, and make each Raider's reaction a personal layer on top of it. The server first records what actually happened once. Every participant sees that same event and outcome; their own Handler may then see a separate reaction shaped by that Raider's traits and remembered encounters.

Start with **one shared server narrator**, immediate authored reactions for every participant, and **one optional AI reaction per admitted moment for a single focal Raider (proposed first-slice default)**. Pass only the common committed facts and that Raider's authorized private context. Measure before widening to several AI reactions. The shared event itself remains server-authored and authoritative. Do not load one model per player. With the current CPU evidence, separate models multiply memory use without evidence of better story quality or acceptable concurrent latency.

This system should create continuity from a small authored beat library and structured memory, not an unbounded collection of hand-written storylines. Deterministic beat selection is the proposed first proof, not a project-wide restriction. An AI director or labelled AI-controlled participant is now an option for a separately approved design.

## 2. Current-state audit

**Measured (docs)** runtime limits and native samples below come from [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md#current-bounded-implementation-g6-2026-10-14) and the [implementation ledger](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint). Identity and persistence limits are also documented in the [README](../README.md#current-implementation-desperanza-duty-and-shared-autonomous-raids).

| What exists | What it provides | Exact limit or distinction |
|---|---|---|
| Server-owned shared events | A shared event is queued once, for actual participants. The runtime keeps original receipts separate from immediate authored narration. | A common event receipt exists; a persistent story-memory system and personal reactions to shared moments are not established. |
| Comms journal and active/recent threads | The Comms Log already separates diary from active multi-tick threads. Optional AI updates are delivered through callbacks, polling/snapshots, and the journal. | Existing shared outcome facts remain authoritative; this proposal adds a story layer, not a second event or gameplay log. |
| Source-ID upserts | A generated update can replace/update the matching authored journal entry rather than adding a second event. Updates propagate to private journals, snapshots, and threads. | Existing UI and callback behavior is event/enrichment delivery, not cross-raid memory or relationships. |
| AI spine and authored fallback | Background inference is outside domain mutation; authored text is immediate. The optional generator can fail without blocking the simulation. | **Measured (docs):** current optional mood has a **25-second** total deadline, including model-gate wait; one model gate serializes Comms/AAR/appraisal access. An eight-slot queue exists. |
| Live admission | At most one eligible live request may be pending per world; other eligible events keep authored narration as `NotRequested / LiveBudgetBusy` and are not retried later. | This is admission control, not parallel inference. The queue also serves terminal debriefs. Live requests expire after **90 seconds** or world completion (**Measured (docs)**). |
| Native CPU samples | The docs record several isolated Phi-3.5 mini runs. | **Measured (docs):** one record reports **70.04 seconds** initialization and a **31-token** response in **13.42 seconds cold / 8.29 seconds warm**. Other isolated runs report **32.58 seconds** initialization with **6/6** accepted samples at **12.60–23.48 seconds**, and **20.22 seconds** initialization with **6/6** accepted at **5.99–9.29 seconds**. These are different small smoke samples, not a concurrency benchmark or a latency guarantee. |
| Acceptance samples and validators | The current guard accepts a single first-person sentence of **8–400 characters** and at least **4 words**, with punctuation/character checks, actor/outcome denylist terms, and context-allowed proper nouns; it revalidates on delivery. The completion budget is **96 output tokens**. Unknown proper nouns fail. | **Measured (docs):** samples include **0/6**, **1/6**, **6/6**, and an earlier **12/12** isolated prompt set (only **8 distinct** outputs). Small, isolated samples do not establish an acceptance rate, humor quality, or factual safety. The lexical guard can false-positive and is not a complete semantic fact checker. |
| Traits and current economy implementation | The current four primary traits are **Brawler, Cautious, Gadgeteer, and Hoarder**. The current economy uses deterministic utility scoring. | **Current implementation**, not a permanent constraint on future AI decisions. Traits are not a validated narrative voice model; do not infer hidden motives from a trait. |
| Shared-world and persistence state | Shared conditions, robots, loot, event ordering, and settlement are server-owned. | **Current limit (docs):** Development identities and the raid/duty state are in-memory, limited to **two hard-coded development identities**, and erased by restart. There are no durable accounts, persistent home assets, restart recovery, or established event-retention policy. S6 is design-only and is a prerequisite for production continuity. |

The AI design doc's small-model RAM and short-line CPU claims are unmeasured candidate claims, excluded from capacity planning here. **RAM for the installed Phi model is unmeasured**; CPU utilization is also unmeasured. The tables below count model copies, not invented byte budgets.

## 3. Shared Moment record

**Proposal:** Persist one canonical `SharedMoment` per committed shared story event. The example is a conceptual shape, not an approved schema:

```json
{
  "schemaVersion": 1,
  "eventId": "stable-server-event-id",
  "worldId": "server-world-id",
  "sequence": 184,
  "beatId": "robot-encounter-mixed-stances",
  "participants": [
    {
      "raiderId": "raider-a",
      "role": "engaged",
      "stance": "continued",
      "trait": "Brawler"
    },
    {
      "raiderId": "raider-b",
      "role": "participant",
      "stance": "withdrew",
      "trait": "Cautious"
    },
    {
      "raiderId": "raider-c",
      "role": "engaged",
      "stance": "continued",
      "trait": "Gadgeteer"
    },
    {
      "raiderId": "raider-d",
      "role": "participant",
      "stance": "withdrew",
      "trait": "Hoarder"
    }
  ],
  "canonicalFacts": {
    "eventType": "SharedRobotEncounter",
    "robotId": "receipt_printer_doom",
    "robotName": "Receipt Printer of Doom",
    "locationId": "committed-location-id",
    "actions": [
      "robot attacked the group",
      "raider-a and raider-c remained engaged",
      "raider-b and raider-d withdrew"
    ],
    "outcome": "two raiders remained engaged; two withdrew; robot remained active",
    "ownershipChanges": []
  }
}
```

In the real record, participant entries cover every actual participant. `role` and `stance` come only from recorded simulation actions or explicitly visible event facts. If the simulation did not record them, use a neutral value such as `participant` or `not-recorded`; neither the narrator nor the director guesses. Private health, inventory, Handler activity, and other owner-only facts do not enter the common projection unless the product explicitly makes them public.

**How sameness is enforced:** the coordinator commits the canonical record and outcome once, assigns a stable event ID and per-world sequence, then fans out the same common payload to all authorized participants. Each journal entry and generated reaction references that event ID. The common text is not separately regenerated per player. The server can hash or compare the canonical payload in tests to verify that all participants received the same version. Idempotent event IDs prevent retries or callbacks from creating duplicate moments or settlement.

**How reactions differ safely:** render the shared facts once, then add a separate owner-private reaction for each Raider. Each reaction receives only that Raider's trait, recorded role/stance, and approved memory summary. It may express attitude, embarrassment, pride, or a running gag, but it may not alter the common facts or announce a new actor, item, winner, injury, or outcome. A reaction is a projection of the record, never a new record.

**Worked example — proposed four-participant fixture, not current capacity or new canon:** the record above covers every participant. The Brawler and Gadgeteer remained engaged; the Cautious Raider and Hoarder withdrew. All were present when the existing Receipt Printer of Doom attacked the group; the robot remained active and no loot changed ownership at this recorded moment. This is not a prediction of the fight's eventual winner. These common facts are identical for all participants:

> **Shared moment:** The Receipt Printer of Doom attacked the group; the Brawler and Gadgeteer remained engaged, while the Cautious Raider and Hoarder withdrew. The robot remained active and no loot changed ownership. The committee split evenly on the value of staying.

Possible private reactions to those same facts:

- **Brawler (raider-a, continued):** “I stayed engaged with the active robot; apparently I consider that a conversation.”
- **Cautious (raider-b, withdrew):** “I withdrew while the robot remained active; my risk assessment has legs.”
- **Gadgeteer (raider-c, continued):** “I stayed engaged with the active robot; this is a regrettably hands-on review.”
- **Hoarder (raider-d, withdrew):** “I withdrew without any loot changing ownership; inventory movement remains theoretical.”

The lines above are **illustrative proposals**. They do not add a new robot ability, item, reward, or game rule. Any factual clause in a future line must be supported by that participant's authorized record.

## 4. Memory and relationships

**Proposal:** maintain a compact, versioned memory projection for each Raider, derived only from committed moments. Keep the durable event archive separate from this small prompt context.

Per-Raider memory may contain:

- Stable, privacy-safe IDs for Raiders met, encounter count, most recent sequence, and whether meetings were shared or separate.
- Relationship tags such as familiar, friendly, or rival **only when a deterministic rule has evidence**. For example, repeated shared encounters may support familiarity; rivalry must not be inferred from mere co-presence.
- Favors only after a real, explicitly supported favor event. Do not invent a favor ledger while favor mechanics are absent.
- Notable wins, losses, and escapes as references to their actual event IDs, not generated summaries of uncertain outcomes.
- Running-gag tags tied to repeated, committed event types or approved authored beats.
- A short rolling summary assembled from those structured facts, never free-written by a model.

**Proposed initial size limits (estimates for a pilot, not measured storage requirements):** keep at most **32** active relationship entries, **8** recent moment references for callbacks, and a rolling summary of at most **500 characters** per Raider. Supply no more than **4** relevant relationship entries and **8** moment references to a single prompt, with a proposed **256-token** memory-context ceiling. The durable event archive is not truncated by these prompt limits; it follows a separately approved retention and deletion policy. A cap eviction must preserve deterministic aggregate counters or a documented archival reference, not silently rewrite history.

**Deterministic and replayable reduction:** process moments in ascending server sequence using a versioned reducer. Derive counts, allowed relationship tags, callback eligibility, and the rolling summary from structured facts and stable ordering. Persist the reducer version and last processed sequence. Replaying the same event stream must produce the same memory state. Store each chosen beat, reaction input version, accepted output or authored fallback, and terminal reason; replay reads the recorded choice/result rather than asking a model to regenerate history.

**Privacy:** each Handler may see their Raider's memory and private reaction, plus common facts authorized for that shared moment. Do not expose another Handler's private memory, inventory, health, hidden history, or model prompt. A common beat must not leak a private fact merely because the narrator received it for another participant. Define account export/deletion and retention before durable relationship memory ships.

## 5. Deterministic story director

**First-slice proposal, not a restriction on future AI:** use a server-owned deterministic rule set to examine committed events and memory, select an eligible authored beat, and record that choice. A model may later pick among server-ranked legal beats under the option in [Roles AI could now play](#roles-ai-could-now-play); that requires separate approval.

Suggested selection order:

1. Consider only a committed event with actual participants and an eligible, authorized shared projection.
2. Derive first-slice candidate beats only from existing committed events: first or repeated shared participation, documented loss followed by return, recorded activity results, or a shared robot outcome. Rescue, favor, and betrayal are future candidates only after approved mechanics produce supporting events.
3. Reject beats whose required facts are absent, whose participants lack visibility, or whose per-Raider/per-pair/world cooldown is active.
4. Choose the highest-priority eligible beat using a fixed priority table; break ties by stable beat ID. If randomized selection is later approved, use a recorded server seed and persist the selected beat.
5. Commit the beat ID and participant set with the Shared Moment. A retry or replay returns that same selection.

**Proposed pacing:** permit no more than one director beat for a shared moment, and require **two intervening eligible shared moments** for the same Raider pair before another pair-specific callback. This is a proposed pacing rule, not a measured fun or balance value. A world-level cooldown may also be needed after playtests; set it from observed repetition, not guesswork.

A compact authored library can vary by recorded role, existing trait, and history state. As a **design-space estimate only**, a proposed **10** beats × **4** existing traits × **3** proposed recorded roles × **4** proposed history states yields **480** combinations. This includes future beats, not first-slice scope, and is not a promise of unique or funny outputs.

**The following proposed 10 examples are not approved runtime content or canon.** Each has a mechanical fact and one deadpan joke, following [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md). Receipt Printer of Doom and its ID are verified in the [robot roster](lore/entities/robots.md#current-roster); A.R.C. is verified in [PARODY_MAPPING.md](lore/PARODY_MAPPING.md). No new named robot, location, or phenomenon is introduced.

### (a) Supported by existing committed event families

These are candidate interpretations of existing search, robot, defeat, and extraction receipts, not existing story beats. Memory-based labels are proposed derived state. A template remains ineligible unless every stated fact is actually recorded; never guess a stance or rivalry.

| Proposed beat | Example line (only when the stated facts are committed) |
|---|---|
| First shared meeting | “These Raiders completed their first recorded shared search. Introductions were handled by standing near the same problem.” |
| Repeat encounter | “The same Raiders participated in another shared robot encounter. Attendance is becoming a habit.” |
| Comeback | “After a recorded knockout, the Raider returned and extracted successfully. The comeback required a second draft.” |
| Unrewarded activity streak ends | “After repeated nonreward activities, a search allocated loot to the Raider. Inventory has ended its silence.” |
| Reunion | “Raiders who shared an earlier search later boarded the same extraction vehicle. The reunion had a departure time.” |
| Shared robot escape | “The Receipt Printer of Doom escaped the shared fight without being defeated; no defeat loot was awarded. The machine declined to become inventory.” |
| Shared robot defeat | “The robot was defeated in the shared fight and its recorded rewards were allocated to eligible contributors. Teamwork has acquired a receipt.” |

### (b) Future: mechanics do not exist; excluded from the first slice

| Future beat | Example line (only after an approved mechanic records these facts) |
|---|---|
| Betrayal | “A Raider broke the recorded agreement to remain for the shared activity and left before it finished. The agreement had a shorter life than the activity.” |
| Favor callback | “The Raider completed the favor recorded in the earlier shared moment. The debt has finally stopped attending meetings.” |
| Rescue | “One Raider's recorded rescue action enabled another Raider to escape. The exit required a referral.” |

An ordinary early exit is not evidence of betrayal, co-presence is not a favor, and surviving together is not a rescue. These examples do not authorize agreements, debts, rescue actions, transfers, or any new mechanics.

## 6. AI role and architecture options

AI decision-making, cloud models, and paid APIs are permitted design options, not approved implementations. Every option still needs authorized inputs, server validation before any gameplay commitment, and recorded results rather than model regeneration during replay.

### How often shared moments actually occur

**Measured (docs):** [BAL-2 robot outcomes by window](balance/BAL-2-measurements.md#robot-outcomes-and-rewards-by-window) reports the following shared robot fights alongside participant-fight counts. These are the report's counts, not unique story moments per hour or the percentage of raids with any meeting:

| Measured cohort window | Shared fights under BAL-2 | Participant fights under BAL-2 | Shared fights in matched BAL-1S baseline |
|---|---:|---:|---:|
| 0–8h | 114 | 1,661 | 94 |
| 8–24h | 22 | 3,481 | 36 |
| 24–48h | 58 | 5,278 | 50 |
| 48–96h | 112 | 10,649 | 48 |

The [measurement method](balance/BAL-2-measurements.md#method-and-interpretation) uses **64 seeds and two unattended Raiders per cohort (measured setup)**. Participant fights count settled Raiders, so a shared fight is not a separate unique-world story count. **General co-presence is not currently measured**: shared searches, passing meetings, and shared extraction need their own frequency telemetry. Do not extrapolate this historical robot-only sample into current all-meeting frequency or a public population estimate.

The designer's loose aim is **about 45% of raids including a meeting with another Raider (proposed, not measured or an acceptance threshold)**. Frequency-raising designs and labelled AI-controlled participants belong in **Prompt 2**, the [world-activity proposal (pending document)](https://github.com/DanJ210/AFK-Raiders-Online/blob/main/docs/WORLD_ACTIVITY_PROPOSAL.md). That document was not present in this checkout when this revision was made; its filename/link is provisional until Prompt 2 supplies it. This story proposal does not authorize changing admission, routes, population, or encounter frequency.

### Batched-reaction latency and the first-slice default

**Measured (docs):** the isolated local Phi sample returned **31 tokens in 8.29 seconds warm / 13.42 seconds cold**, and the current live deadline is **25 seconds including gate wait**; see [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md#current-bounded-implementation-g6-2026-10-14).

**Estimate:** one structured reply carrying several reactions will be several times longer than that short sample, with participant IDs and structure adding output too. A proposed **four-reaction batch** will probably miss the current deadline. A deliberately crude length-only projection is **4 × 31 = 124 reaction tokens**, before structure, and **4 × 8.29–13.42 = 33.16–53.68 seconds**. This is not measured batch latency: prompt processing, stopping, and throughput are not guaranteed to scale linearly. The current **96-token completion cap (measured configuration, same source)** also does not establish capacity for such a batch.

**Proposed default:** immediate authored reactions for everyone; at most **one AI reaction per admitted moment**, for a single focal Raider chosen in stable participant order with recorded rotation. Use AI only on an authored-first subset of moments. Record the focal ID and selection rule/version so retries do not choose someone else. Measure end-to-end latency, token counts, acceptance, and fallback before widening. Do not increase deadlines or completion budgets in this documentation task.

Newly allowed alternatives, all **proposals needing measurements**:

| Alternative | Value and latency question | Trade-offs |
|---|---|---|
| Cloud model with a measured faster response | May fit richer reactions into the live deadline; benchmark actual prompts, network time, queueing, and acceptance before choosing it. No cloud timing is measured here. | Paid token/API usage and quotas; server-side secret storage and API-key rotation, never browser keys; provider outages and network availability; minimize player data and decide provider retention, region, consent/disclosure, and privacy before sending it. |
| Parallel per-Raider requests | Shorter individual outputs may finish sooner than a long batch if real parallel capacity exists. | Today's shared gate serializes inference. Separate slots need an approved admission/pool design, compute or cloud spending, partial-result handling, and measurement under contention. Keep one pending moment/job per world rather than bypassing the rule with child requests. Cloud use adds the same key, availability, and player-data privacy duties; local use still costs hardware and energy. |
| Faster local model | Could lower latency without sending player data to a provider; compare factual acceptance and voice quality on identical fixtures. | Hardware/energy and model maintenance cost, unproven quality and latency, machine availability, and deployment/licensing checks. No cloud API key is needed; protect local prompts and logs. Do not plan from unmeasured candidate RAM claims. |

### Scaling by moments, not simultaneous players

**Estimated workload model; inputs to measure, not asserted rates:**

- `H` = unique eligible shared moments per deployment hour, summed across worlds; `p` = mean actual participants per moment.
- `q` = proposed fraction of eligible moments selected for optional AI before admission; busy admission can reduce actual use further.
- `A` = admitted AI moment jobs per hour, with `A ≤ qH`. Each world retains the **one-pending-live-request rule (measured current behavior)**; busy arrivals keep authored output and are not retried. A job counts as pending through queued/running work.
- `r` = proposed AI reactions per admitted moment, with `r = 1` for the first slice and `r ≤ p` if widened. All participant reactions number approximately `Hp`; AI reactions number approximately `Ar`; the rest stay authored.
- `t` = the measured isolated short-response time above, used only as a proxy for estimated service work. Queue wait, other Comms/AAR/appraisal work, prompt growth, bursts, and deadlines can cause fallback even when average hourly demand looks small. Service work is elapsed inference demand, not CPU-seconds.

Do not derive `H` from connected player count alone. Instrument unique moments, participant counts, per-world arrival gaps, admission/busy reasons, and peak as well as average demand.

| Architecture / proposed policy | Estimated workload using H, p, q and A | Resource and consistency implications |
|---|---|---|
| **(a) One local model per player** | Up to `Ar` personal requests/hour after admission, not a request for every connected player on every tick. | Model copies scale with resident player sessions. Not recommended: startup/compute pressure without evidence of better voice or concurrency. Common facts still come from the server. |
| **(b) Shared local narrator, single focal reaction** | `A` requests/hour and approximately `At` short-response service work; remaining `Hp − A` reactions authored. | Proposed starting architecture: a single model copy and the current shared gate. Admission limits demand, not guarantees of timely completion. |
| **(c) Small shared local pool** | Per-Raider calls require about `Ar` requests/hour; ideal spare-capacity service work is `Art`, not a measured speedup. | A proposed pool of **2–4 copies** needs measured parallel capacity and revised deployment admission. Retain the per-world pending-job rule. |
| **(d) Client-side private decoration** | Up to `Ar` client requests/hour if server eligibility/admission is retained. | No server inference for this path (estimated architecture). Client performance varies; client prose is not trusted server history. Not a first-slice choice. |
| **(e) Authored-first subset** | Reduces offered jobs from `H` to `qH`; admitted `A` can be smaller. With the proposed focal default, `r = 1`. | Rollout policy for a shared local or cloud narrator, not another model copy. Choose `q` from measured latency, acceptance, fallback, and cost. |
| **(f) Shared cloud narrator** | Focal default: `A` calls/hour; per-Raider widening: about `Ar` calls/hour; batching: `A` longer calls/hour. **Estimated cost per proposed 1,000 reactions:** `1,000 × (I × P_in + O × P_out)` when prices are per token. If quoted per million tokens, divide by **1,000,000**. | `I` and `O` are measured billed input/output tokens per delivered AI reaction, including apportioned prompt/structure and failed paid attempts; `P_in` and `P_out` are the selected provider's published prices. No prices are assumed. Separate request fees, storage, and other charges if applicable. Provider capacity, quota, availability, privacy, and keys still need approval. |

**Labelled stress case, not expected demand:** assume every player needs an independent short reaction at once, ignoring shared moments and admission. At proposed loads of **1 / 10 / 100 / 1,000 players**, a single serialized model would finish the last reply in an estimated **8.29–13.42 / 82.9–134.2 / 829–1,342 / 8,290–13,420 seconds**, respectively, using the measured short-response proxy. With the authored-first subset, offered calls would be approximately `qN` for `N` players. The real admission/deadline policy falls back instead of waiting out that burst; these figures are stress arithmetic, not measured concurrency or a capacity claim.

**Trait voice recommendation:** use the existing Brawler, Cautious, Gadgeteer, and Hoarder as proposed lightweight voice profiles, not separate models. Test whether framing is distinguishable without inventing facts; existing decision weights are not proof of established narrative personalities.

### Roles AI could now play

**Options only; none activates story-system implementation.** The first row extends the current narration role. The others require separately approved designs. For every runtime option, retain authoritative event IDs, ordered server inputs, visibility rules, validation and terminal reasons. Replays consume recorded choices/results, not fresh model calls. Only the coordinator may commit gameplay or settle items/coins once.

| Option | Value | Cost | Latency tolerance | Risk | Record for replay and once-only settlement |
|---|---|---|---|---|---|
| Voice an authored beat (current role; personal reactions proposed) | Variety without changing the chosen mechanical event. | Local compute/energy or provider token fees; authored editing and review. | Optional live enrichment within the current measured deadline; authored text is immediate. | False implied facts, tone drift, private-data leakage, rejection. | Source event/beat/focal IDs, authorized input and prompt/model versions, accepted text or fallback, status/reason, and idempotent journal upsert. No new settlement from prose. |
| Pick among server-ranked legal beats: slow story director | More varied pacing and callbacks within a validated candidate set. | An extra inference and evaluation workload. | Asynchronous, at a proposed story checkpoint; never wait on a simulation tick. Expire stale inputs and fall back to the top deterministic legal beat. | Invalid/stale pick, manipulative pacing, inconsistent memory or visibility. | Candidate IDs/ranks, input sequence and policy/model/prompt versions, selected ID, validation, fallback, and stable decision ID. Record before commitment; replay the pick and deduplicate it, never settle again. |
| Draft authored beat variants offline for human review | Helps build a reviewed, reusable library. “Offline” here means outside live play, not a cloud ban. | Batch compute/provider fees plus human editorial time. | Long-running authoring work; no live deadline. | Off-lore names, copied prose, factual or tone mistakes entering content. | Draft provenance/model/prompt, review approval and final content revision. Runtime records the approved variant ID/version; drafts never create rewards or events. |
| Analyse telemetry for designers | Finds repetition, latency, meeting-rate, and balance questions for human investigation. | Analysis/model fees, data preparation, human verification. | After a run or periodic report, not in the gameplay path. | Misread statistics, invented conclusions, sensitive data disclosure. | Dataset/run references, queries/aggregation versions, model/prompt, output and human conclusions. Advice does not mutate worlds or settle anything; retain approved change provenance separately. |
| Act as a labelled AI-controlled participant (Prompt 2) | Tests/populates real server-owned encounters with clearly disclosed AI presence. | Repeated decision calls plus hosting and evaluation; workload must be measured. | Approved decision windows with legal deterministic fallback on delay/unavailability; not tied to Handler presence. | Unfair behavior, mistaken human identity, illegal actions, economy/ownership exploits, privacy exposure. | AI label and controller version, actual world/participant ID (never a saved opponent shown as live), legal choices, authorized state, decision ID/pick/time, validation, fallback and committed effects. Deduplicate commands and settle only through normal server ownership/settlement rules. See the pending world-activity proposal above. |

## 7. Guardrails

- **Committed facts remain factual:** commit the Shared Moment from server state before narrating it. A director or model cannot rewrite already-committed participants, roles, actions, results, timestamps, item ownership, or settlement. An approved AI decision must be recorded and validated before it takes effect.
- **Structured reaction only:** for the proposed focal default, require exactly the selected focal participant ID and a single reaction. Other participants retain authored reactions. If a wider batch is later approved, require exactly its recorded selected subset, not every participant automatically. Reject unknown IDs, missing/duplicate selected reactions, extra fields, malformed output, or any factual name/item/outcome not present in the authorized fact set. The shared moment itself remains authored from facts.
- **Reuse current checks, do not overclaim:** retain sentence/character checks, actor/outcome denial, allowed proper-noun vocabulary, delivery-time revalidation, and explicit failure reasons. Add exact participant and fact allowlists. The current lexical checks are not a semantic proof that prose contains no implied claim; keep reactions short and subjective, and keep the mechanically complete shared line visible independently.
- **Fallback:** an authored line is immediately available. On timeout, busy/full queue, model unavailability, stale event, rejection, or validation error, show it and preserve the reason/status. No silent “generated” success, no retry that duplicates or rewrites a committed moment.
- **Proposed rate limits:** at most **one AI reaction per Raider per 5 minutes** and **one AI moment job per world per 60 seconds**, with the existing **one-pending-live-request-per-world** admission and **25-second** deadline (**measured configuration**, [AI design](AI_SYSTEMS_DESIGN.md#current-bounded-implementation-g6-2026-10-14)) retained unless a measured review approves a change. The proposed focal default is not a batch. These initial safeguards are not performance or balance findings. Deployment quotas and a cost budget need workload measurement and, for cloud use, current provider prices before public use.
- **Player-influenced text:** free-form Handler or player text is out of the first slice. Use server-owned IDs, traits, and enumerated Signal actions only; escape display names for presentation and do not interpolate arbitrary names or text into a prompt. If free text is considered later, it needs input length limits, moderation before storage and prompt use, output validation, abuse/rate controls, privacy review, and an authored-only fallback when moderation is unavailable. It never becomes a gameplay command or fact.

## 8. Prerequisites and ordering

**Must exist before persistent player-facing story memory:**

1. **Durable identity and persistence (S6).** The current Development service is in-memory and restart clears identities/access and raid state. It is not safe to treat its memory as durable player continuity.
2. **Event retention and deletion policy.** Decide which shared facts are retained, for how long, how account deletion/export works, and how private memory references are removed without changing other players' canonical records.
3. **Memory storage and replay contract.** Persist versioned reducer state, last processed sequence, relationship projections, beat choices, and narration/fallback status alongside or reconstructable from committed events. Recovery must not duplicate settlement or re-run a model to recreate the past.
4. **Visibility contract.** Define what is public to participants versus private to one Handler before prompts or callbacks can carry those fields.

**Can be designed now, without implementation:** the Shared Moment schema candidate, authored-only beat rules using existing committed events, in-memory memory reducer and privacy requirements, model workload measurement plan, and deterministic fixture-based acceptance tests. After separate approval, an in-memory experiment can prove a within-session callback without S6. Only the cross-restart callback and durable player continuity require the relevant persistence/identity proof; do not mark the whole slice blocked on storage.

Suggested order after approval: settle visibility and fixture scope; prove an authored-only shared moment and within-session callback using in-memory memory; measure the focal AI reaction on the same fixtures and use the interim observer plan below. Separately settle identity/retention decisions and complete the narrow S6 persistence proof before cross-restart acceptance or the multi-account human playtest. Expand beat eligibility only after review.

## 9. Vertical-slice plan

**Proposed smallest proof:** **two to four actual Raiders (proposed fixture size, not current development capacity)** in a server-owned event encounter the same existing robot. Record one Shared Moment, select one authored beat, and provide an authored trait/history-shaped reaction per Raider from the identical fact record. On an admitted subset, optionally enrich only the focal Raider's reaction. Retain a compact memory entry in memory; a later event in the same session should call back to it. Cross-restart validation is a separate persistence extension after S6, not a blocker for the within-session proof.

**Acceptance criteria (proposed):**

- Every participant projection references the same event ID, sequence, canonical-facts version, and outcome; automated comparison finds no fact or ownership differences.
- The result is produced once under retries/replay. Reprocessing does not re-settle gameplay, duplicate the beat, or call generation to reconstruct an already committed result.
- Each participant receives exactly one allowed reaction or the authored fallback. Different wording never changes shared facts; no private data crosses Handler boundaries.
- Deterministic beat/memory reduction produces the same selected beat and memory projection from the same ordered event stream.
- **Within-session callback:** the later callback refers to an earlier committed event retained in in-memory memory and states only facts supported by it. This criterion can pass without durable storage; it proves session continuity, not restart recovery.
- **Cross-restart callback (separate S6-dependent criterion):** after restart, restore the earlier event and versioned memory and deliver a factual callback without repeated generation or settlement. Mark only this extension blocked until durable identity/storage is available.
- Narration remains asynchronous and cannot block an authoritative simulation tick. AI failure produces immediate authored output with an explicit recorded failure/fallback reason.

**Human playtest (proposed, 3–5 independently signed-in testers): blocked on durable identity (S6).** The current [development limit](../README.md#current-implementation-desperanza-duty-and-shared-autonomous-raids) is **two hard-coded identities (measured current configuration)**, so this plan cannot currently give each tester an independent human-owned Raider. Do not share access tokens, claim extra accounts exist, or treat the proposed fixture size as current live capacity.

**Interim observer plan (proposed, after separate harness approval):** run an autonomous multi-Raider population in an isolated harness on the server-owned timeline. Have each tester watch one Raider's owner-filtered Handler view from the same recorded run, through harness-backed views or captured view fixtures rather than new live accounts. Ensure the population contains actual simulated participants, not saved opponents represented as live humans. This requires a harness/view fixture capable of exposing the selected projections; it is not a claim that today's development login supports extra observers. No backend restart or harness implementation is requested here.

Show a shared event and a later within-session callback. Ask: What happened mechanically? Did other Raiders experience the same event? What did your Raider personally think? Did the callback feel earned or intrusive? Did any line claim something you did not observe? Which Raider voice was recognizable? Would you want this kind of shared story again? Record individual answers before testers compare notes. Score **factual agreement separately from reaction variety**; the latter is not proof of the former. Observer results do not prove multi-account authorization, human encounter frequency, or durable continuity. Evidence of fun is spontaneous recall, clear distinction between shared facts and personal reaction, recognition of a recurring Raider, and a stated desire to see another encounter, not merely different text.

**Measurements to capture (proposed instrumentation; no targets asserted):** unique shared moments/hour, participants/moment and general co-presence/raids-with-meetings; offered `q` and actual admitted jobs; event-to-authored display latency; queue wait and end-to-end latency; accepted/rejected/timeout/busy/stale counts; authored fallback rate and reason; per-trait/per-beat acceptance; within-session callback eligibility/frequency separately from cross-restart results; duplicate/stale upserts; input/output and billed tokens, cost including failures; installed-model RAM and CPU; memory entries/bytes; privacy/fact violations. Report sample size, model/provider/device/configuration, and limitations of small playtests.

## 10. Risks and what could go wrong

| Risk | What it looks like | Mitigation or evidence needed |
|---|---|---|
| Repetition | The same beat or joke returns despite different events. | Deterministic cooldowns, authored variants, callback caps, and playtest repetition notes; do not claim model randomness solves it. |
| Tone drift | A line is dramatic, mean, or too explanatory for the deadpan voice. | Review against the existing tone guide; keep authored baseline; sample human ratings, not only validator pass rates. |
| Latency and cost | The global gate backs up or a long batch misses the deadline; cloud usage exceeds its budget. | Start authored-first with a single focal AI reaction; measure moments, queue wait, token/billing demand, and compute before widening or choosing a provider. |
| Cross-player inconsistency | Participants see different outcomes or different “shared” prose. | One canonical event ID/payload, one common fact line, idempotent upserts, and hash/equality tests. |
| Unsafe or off-lore output | Unknown proper nouns, made-up items, insults, or unsupported outcomes. | Narrow inputs, original AFK names, current validators plus fact allowlists, authored fallback, and human content review. |
| Memory growth or distortion | Relationship records grow without bound or compressed summaries change what happened. | Bounded prompt projection, archived event references, deterministic versioned reducer, retention policy, and replay checks. |
| Privacy leakage | One Handler receives another Raider's private history or personal reaction. | Separate common and private payloads, per-owner authorization, private memory inputs, and multi-client access tests. |
| Notes reveal contradictions | Players compare screens and find a “shared” robot, loot, or outcome differs. | Shared facts are authored once and rendered identically; only explicitly private subjective reactions may differ. Test with side-by-side client projections. |
| Reactions imply false facts | A subjective aside quietly claims an unrecorded past event or hidden item. | Restrict reaction form, validate names/outcomes, keep factual event line separate, and treat semantic validation as imperfect. |

## 11. Decisions for the designer

| Decision | Recommendation | Reason |
|---|---|---|
| Approve the core contract: same canonical facts/outcome, different private reactions? | **Approve.** | It directly expresses the intended “shared event, personal story” experience while preserving server authority. |
| Which facts are visible to all participants? | **Start with the event, visible participants, recorded roles/actions, and common outcome only.** Keep health, inventory, Handler actions, and memories private unless explicitly designated public. | Prevents a shared story from leaking another person's private state. |
| Relationship meaning and memory privacy? | **Start with mechanical counters and neutral tags; do not infer friendship, rivalry, or favors without a recorded supporting event.** Keep personal memory private. | Avoids claiming emotional or social intent the game did not actually establish. |
| Persistence, retention, and deletion policy? | **Require S6 durable identity/storage plus an explicit retention/deletion policy before shipping saved memory.** | Current data disappears on restart; long-term memory creates a new privacy and recovery obligation. |
| Story director's initial beat set and cooldown? | **Start with one existing-event-backed beat and sequence-based cooldowns; expand only after playtest.** | Small scope lets the team test continuity without enabling unimplemented social mechanics or overproducing callbacks. |
| Narrator architecture and provider? | **Start with a shared narrator, not a model per player. Measure local and cloud candidates before choosing or widening.** | Existing local samples do not establish batch/concurrent performance. Cloud selection needs a spending budget, key handling, availability and player-data policy. |
| AI rollout policy? | **Use authored-first on a measured subset, with one focal AI reaction per admitted moment (proposed), authored reactions for the rest.** | Longer batches probably miss the current deadline. Select `q` from measurements rather than player count. |
| Future AI roles? | **Consider the options in §6 separately; approve any director/participant design explicitly.** | New permission does not approve legal-choice policy, timing, controller identity, or once-only decision/settlement contracts. |
| Meeting frequency? | **Use the loose proposed aim of about 45% of raids with a meeting as a question for Prompt 2, not a certified target.** | Historical shared robot fights do not measure general co-presence; the world-activity document is pending here. |
| Per-trait voice tuning? | **Try lightweight authored profiles for the four existing traits after the shared-fact slice passes.** | It may make Raiders recognizable without multiplying models, but the current traits are decision weights, not established voices. |
| Player-supplied free text? | **Exclude it from the first slice.** | It adds moderation, privacy, and prompt-injection risk without being necessary to prove the shared-story idea. |
| Human pilot and acceptance? | **Use harness-based observers first; defer the proposed 3–5 independent human-account testers until S6. Accept the within-session callback separately.** | The measured current two-identity limit blocks that live pilot, not an in-memory continuity proof. Observer evidence does not establish live account isolation or overall fun. |

No balance values or rules changes are requested by these decisions.

## 12. Out of scope

- Implementing any code, runtime content, tests, balance tuning, rules version/fingerprint changes, or model deployment.
- Changing simulation outcomes, autonomous economy, Handler economy controls, ownership, or settlement.
- PvP, theft, or any new favor, rescue, or betrayal mechanic.
- Implementing AI-directed choices is outside the first slice. Their possible roles are listed in §6 for design consideration; the former project-wide prohibition was removed on 2026-11-06.
- Treating generated prose as canon, or adding any proposed example as approved AFK lore.
- Replacing authored narration as the entertainment baseline.
- Implementing a model per player, freeform player chat, public matchmaking, production accounts, world-activity changes, or claiming current tests prove production scale.
- Writing millions of hand-crafted storylines or promising limitless generated variety.

## Assumptions changed on 2026-11-06

- The former rule that AI could only voice an already selected beat, and could not decide or direct systems, was removed. Future AI decision-making and AI-controlled agents are allowed design options, not implementation approvals.
- The former offline-only, zero-cost, no-API-key constraint was removed. Cloud-hosted models and paid APIs may be considered.
- This revision keeps a deterministic authored beat as the proposed first proof, not a permanent AI restriction. §6 now lists voicing, a slow legal-beat director, reviewed offline drafting, telemetry analysis, and labelled AI-controlled participation as options with costs, timing, risks and replay records.
- The worked example now records mixed stances consistently. Beat examples separate existing event families from future favor, betrayal and rescue mechanics; no new canon or gameplay is approved.
- The measured local timings support a proposed single focal AI reaction, not an assumed timely multi-reaction batch. Faster cloud, parallel requests and faster local models need measured comparisons, provider/key/privacy decisions and explicit budgets.
- Capacity now depends on moments, participants, the authored-first subset and per-world admission, with the simultaneous-player burst retained only as an estimated stress case. Unmeasured small-model RAM claims are not planning inputs, and no provider prices are invented.
- The cited BAL-2 robot-fight counts are historical measured evidence, not general co-presence measurements. The loose proposed meeting aim and labelled AI participation refer to Prompt 2's pending world-activity proposal, not approved frequency changes.
- The proposed multi-account human pilot waits for S6; autonomous harness observers can assess factual agreement and variety separately. Within-session callbacks can be tested with in-memory memory; only cross-restart continuity requires persistence.
- The measured local ONNX timings and authored fallback describe current evidence and implementation, not restrictions on model/provider choice. No story work is active and no code, content, tests, balance, rules versions or fingerprints change through this proposal.

## Source documents consulted

- [START-HERE.md](START-HERE.md), [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md), [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md), and [GAME_DESIGN.md](GAME_DESIGN.md).
- [README](../README.md) for the development identity limit and [BAL-2 measurements](balance/BAL-2-measurements.md) for historical shared robot fights. Prompt 2's world-activity proposal is pending, not a consulted measurement source.
- [Lore README](lore/README.md), [PARODY_MAPPING.md](lore/PARODY_MAPPING.md), [BACKLOG.md](lore/BACKLOG.md), [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md), and [LEGAL_GUARDRAILS.md](lore/content-guidelines/LEGAL_GUARDRAILS.md).
- The lore entity references for [Desperanza](lore/entities/desperanza.md), [factions and brands](lore/entities/factions-and-brands.md), [field phenomena](lore/entities/field-phenomena.md), [robots](lore/entities/robots.md), [skills](lore/entities/skills.md), [traders](lore/entities/traders.md), [weapons](lore/entities/weapons.md), [world history](lore/entities/world-history.md), and [zones](lore/entities/zones.md).
