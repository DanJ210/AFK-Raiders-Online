# Shared Story System — Design Proposal

**Status: Proposal, awaiting designer approval. No story-system implementation is active.**

This is a design document only. It does not authorize code, runtime content, gameplay, balance, rules-version, or fingerprint changes. The counts and limits suggested below are proposals, not validated targets. Quantitative runtime evidence is marked **Measured (docs)**; projections are **Estimates**; future choices are **Proposals**.

## 1. Summary and recommendation

Make the shared event the unchanging part of the story, and make each Raider's reaction a personal layer on top of it. The server first records what actually happened once. Every participant sees that same event and outcome; their own Handler may then see a separate reaction shaped by that Raider's traits and remembered encounters.

Start with **one shared server narrator** and pass it a committed event plus a small, private context for each participant. It can return a set of reactions for that moment, while the shared event itself remains server-authored and authoritative. Keep authored narration as the immediate baseline and fallback; initially use AI only for a measured subset of eligible moments. Do not load one model per player. With the current CPU evidence, separate models multiply memory use without evidence of better story quality or acceptable concurrent latency.

This system should create continuity from a small authored beat library, structured relationships, and deterministic selection—not from an unbounded collection of hand-written storylines. It is an optional narration layer; it never changes the simulation.

## 2. Current-state audit

| What exists | What it provides | Exact limit or distinction |
|---|---|---|
| Server-owned shared events | A shared event is queued once, for actual participants. The runtime keeps original receipts separate from immediate authored narration. | A common event receipt exists; a persistent story-memory system and personal reactions to shared moments are not established. |
| Comms journal and active/recent threads | The Comms Log already separates diary from active multi-tick threads. Optional AI updates are delivered through callbacks, polling/snapshots, and the journal. | Existing shared outcome facts remain authoritative; this proposal adds a story layer, not a second event or gameplay log. |
| Source-ID upserts | A generated update can replace/update the matching authored journal entry rather than adding a second event. Updates propagate to private journals, snapshots, and threads. | Existing UI and callback behavior is event/enrichment delivery, not cross-raid memory or relationships. |
| AI spine and authored fallback | Background inference is outside domain mutation; authored text is immediate. The optional generator can fail without blocking the simulation. | **Measured (docs):** current optional mood has a **25-second** total deadline, including model-gate wait; one model gate serializes Comms/AAR/appraisal access. An eight-slot queue exists. |
| Live admission | At most one eligible live request may be pending per world; other eligible events keep authored narration as `NotRequested / LiveBudgetBusy` and are not retried later. | This is admission control, not parallel inference. The queue also serves terminal debriefs. Live requests expire after **90 seconds** or world completion (**Measured (docs)**). |
| Native CPU samples | The docs record several isolated Phi-3.5 mini runs. | **Measured (docs):** one record reports **70.04 seconds** initialization and a **31-token** response in **13.42 seconds cold / 8.29 seconds warm**. Other isolated runs report **32.58 seconds** initialization with **6/6** accepted samples at **12.60–23.48 seconds**, and **20.22 seconds** initialization with **6/6** accepted at **5.99–9.29 seconds**. These are different small smoke samples, not a concurrency benchmark or a latency guarantee. |
| Acceptance samples and validators | The current guard accepts a single first-person sentence of **8–400 characters** and at least **4 words**, with punctuation/character checks, actor/outcome denylist terms, and context-allowed proper nouns; it revalidates on delivery. The completion budget is **96 output tokens**. Unknown proper nouns fail. | **Measured (docs):** samples include **0/6**, **1/6**, **6/6**, and an earlier **12/12** isolated prompt set (only **8 distinct** outputs). Small, isolated samples do not establish an acceptance rate, humor quality, or factual safety. The lexical guard can false-positive and is not a complete semantic fact checker. |
| Traits and deterministic economy | The current four primary traits are **Brawler, Cautious, Gadgeteer, and Hoarder**. They already affect deterministic utility scoring. | Traits currently tune decisions, not a validated narrative voice model. Do not infer hidden motives from a trait. |
| Shared-world and persistence state | Shared conditions, robots, loot, event ordering, and settlement are server-owned. | **Current limit (docs):** Development identities and the raid/duty state are in-memory, limited to **two hard-coded development identities**, and erased by restart. There are no durable accounts, persistent home assets, restart recovery, or established event-retention policy. S6 is design-only and is a prerequisite for production continuity. |

The AI design doc's suggested **250–400 MB** SmolLM2 footprint and **sub-50 ms** short-line CPU generation are **unmeasured candidate claims in that document**, not observed runtime performance. They must not be used as planning guarantees. Current Phi initialization and response timings above are the relevant measured evidence; model RAM and CPU utilization are not reported.

## 3. Shared Moment record

**Proposal:** Persist one canonical `SharedMoment` per committed shared story event. The example is a conceptual shape, not an approved schema:

```json
{
  "schemaVersion": 1,
  "eventId": "stable-server-event-id",
  "worldId": "server-world-id",
  "sequence": 184,
  "beatId": "robot-encounter-withdrawal",
  "participants": [
    {
      "raiderId": "raider-a",
      "role": "engaged",
      "stance": "continued",
      "trait": "Brawler"
    },
    {
      "raiderId": "raider-b",
      "role": "warned",
      "stance": "withdrew",
      "trait": "Cautious"
    }
  ],
  "canonicalFacts": {
    "eventType": "SharedRobotEncounter",
    "robotId": "receipt_printer_doom",
    "robotName": "Receipt Printer of Doom",
    "locationId": "committed-location-id",
    "actions": ["robot attacked the group", "participants withdrew"],
    "outcome": "raiders withdrew; robot remained active",
    "ownershipChanges": []
  }
}
```

In the real record, participant entries cover every actual participant. `role` and `stance` come only from recorded simulation actions or explicitly visible event facts. If the simulation did not record them, use a neutral value such as `participant` or `not-recorded`; neither the narrator nor the director guesses. Private health, inventory, Handler activity, and other owner-only facts do not enter the common projection unless the product explicitly makes them public.

**How sameness is enforced:** the coordinator commits the canonical record and outcome once, assigns a stable event ID and per-world sequence, then fans out the same common payload to all authorized participants. Each journal entry and generated reaction references that event ID. The common text is not separately regenerated per player. The server can hash or compare the canonical payload in tests to verify that all participants received the same version. Idempotent event IDs prevent retries or callbacks from creating duplicate moments or settlement.

**How reactions differ safely:** render the shared facts once, then add a separate owner-private reaction for each Raider. Each reaction receives only that Raider's trait, recorded role/stance, and approved memory summary. It may express attitude, embarrassment, pride, or a running gag, but it may not alter the common facts or announce a new actor, item, winner, injury, or outcome. A reaction is a projection of the record, never a new record.

**Worked example — proposal, not canon:** four Raiders encounter the existing Receipt Printer of Doom. The committed facts are: all four were present; the robot attacked the group; all four withdrew; the robot remained active; no loot changed ownership. These facts are identical for all four:

> **Shared moment:** Four Raiders withdrew from the Receipt Printer of Doom; the robot remained active and no loot changed ownership. The retreat was unanimous, which is the closest thing to a committee decision.

Possible private reactions to those same facts:

- **Brawler:** “I advanced far enough to confirm the robot was still unpleasant. The withdrawal was a tactical footnote.”
- **Cautious:** “All four of us withdrew, and the robot stayed active. The exit has passed my only test: it exists.”
- **Gadgeteer:** “The robot attacked; we all withdrew. I have identified a promising improvement: fewer meetings with printers.”
- **Hoarder:** “We left without transferring loot, and the robot remained. I have filed the lack of ownership changes under acceptable inventory movement.”

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

The story director is a server-owned, deterministic rule set. It examines committed event and memory state, selects an eligible authored beat, and records that choice. It does not ask a model what should happen.

Suggested selection order:

1. Consider only a committed event with actual participants and an eligible, authorized shared projection.
2. Derive candidate beats from observable state: first meeting, repeated meeting, documented loss followed by return, a run of failed activities, reunion, a supported rescue/favor, a shared robot outcome, or a world condition that actually changed what participants did.
3. Reject beats whose required facts are absent, whose participants lack visibility, or whose per-Raider/per-pair/world cooldown is active.
4. Choose the highest-priority eligible beat using a fixed priority table; break ties by stable beat ID. If randomized selection is later approved, use a recorded server seed and persist the selected beat.
5. Commit the beat ID and participant set with the Shared Moment. A retry or replay returns that same selection.

**Proposed pacing:** permit no more than one director beat for a shared moment, and require **two intervening eligible shared moments** for the same Raider pair before another pair-specific callback. This is a proposed pacing rule, not a measured fun or balance value. A world-level cooldown may also be needed after playtests; set it from observed repetition, not guesswork.

A compact authored library can be multiplied by participant count, role, one of the four existing traits, and a few history states. As a **design-space estimate only**, **10** beats × **4** traits × **3** recorded roles × **4** history states yields **480** combinations; this is not a promise of 480 unique or funny outputs.

**The following 10 lines are proposals for beat templates, not approved runtime content or canon. Emit a line only when its stated mechanical facts are present.** They use existing AFK-original names and the tone guide's consequence-first deadpan:

| Proposed beat | Example line (only when the stated facts are committed) |
|---|---|
| First shared meeting | “Two Raiders met for the first recorded shared moment during the same field search and both completed it. A first meeting has been filed under ‘we were both already standing here.’” |
| Repeat encounter / rivalry-lite | “The same Raiders met again and each kept their own secured loot. Their rivalry remains strictly parallel and fully insured.” |
| Comeback | “After a recorded defeat, the Raider returned and extracted successfully. The comeback has been entered in the department of things that took more than one attempt.” |
| Bad-luck streak ends | “After two recorded failed searches, the next search succeeded and produced its recorded result. Luck has resumed at the minimum level required for a receipt.” |
| Reunion | “Two Raiders who shared an earlier moment met again and both took part in this event. Reunited at last, they continued not to exchange property.” |
| Shared robot retreat | “Four Raiders withdrew from the Receipt Printer of Doom; the robot remained active and no loot changed hands. The retreat was unanimous, which is the closest thing to a committee decision.” |
| Complementary roles | “The Brawler engaged while the Cautious Raider withdrew; the shared encounter ended with the robot still active. Teamwork has produced two different opinions and one exit.” |
| Shared catastrophe | “The recorded Tremor of Bad Priorities interrupted the shared search; no loot was secured from that activity. The ground submitted its own request to end the meeting.” |
| Betrayal-lite / early exit | “One Raider withdrew before the other finished the shared activity; no item transferred between them. The departure was abrupt, but the ownership ledger remained calm.” |
| Favor callback | “The Raider completed the favor recorded in the earlier shared moment; the recorded outcome was delivered. The favor is now closed, unlike the filing cabinet.” |

Several rows require event types or favor mechanics that may not currently exist in the online runtime. Those lines are not authorization to add them. For the first slice, select a beat backed by an existing, committed event, and omit unsupported examples.

## 6. AI role and architecture options

AI may voice an already selected beat; it does not select beats, decide actions, set prices, grant rewards, establish relationships, or determine outcomes. Keep a single common fact block and pass small per-Raider context for reactions. A shared narrator can generate a structured set of reactions for a shared event, while each Handler sees the same committed moment and only their own private line.

| Option | Latency and memory/CPU | Consistency, trade-offs, and fallback |
|---|---|---|
| **(a) One model per player session** | **Estimate:** one model copy and one independent inference slot per player. With **N** players, memory scales as **N** copies; the AI design doc's **250–400 MB** candidate estimate would imply **0.25–0.4 GB / 2.5–4 GB / 25–40 GB / 250–400 GB** for **1 / 10 / 100 / 1,000** copies, before runtime overhead. That footprint is an unmeasured candidate claim. Per-player timing could resemble the isolated **8.29–13.42 second** cold/warm observation only with adequate independent CPU capacity; contention is unmeasured. | **Not recommended.** More model instances increase memory/startup pressure and each model may phrase the same facts differently. Share the canonical event and fall back per Raider to their authored reaction. |
| **(b) One shared server narrator with per-character context** **(recommended starting architecture)** | **Estimate:** one model copy; memory stays approximately one loaded model as player count grows. With one independent request per player in a burst and today's single gate, the idealized time for the last serialized response is **8.29–13.42 seconds at 1; 82.9–134.2 seconds at 10; 829–1,342 seconds at 100; and 8,290–13,420 seconds at 1,000**. These are arithmetic service-time projections, not measured multi-user performance; the current deadline/admission policy would cause fallback rather than wait that long. If one moment contains several participants, one structured request can return their reaction set and reduce calls, but longer output latency is unmeasured. | **Recommended.** One shared event input makes consistency easiest to reason about; individual reactions can still differ by trait/history. Preserve identical authored common facts and use authored per-Raider reactions on timeout, rejection, busy admission, or model failure. |
| **(c) A small pool of shared models** | **Estimate:** a proposed **2–4** instances multiply model memory by that count. For one independent request per player and ideal parallel gates, the last response in a burst would take about **8–67 seconds at 10; 3.5–11 minutes at 100; and 34.5 minutes–1 hour 52 minutes at 1,000**, using the isolated response-time range and assuming no contention. Real CPU contention and longer structured outputs will change this. | Can reduce queue waits only if hardware has spare compute and the pool is measured. Adds model lifecycle, pool admission, and capacity risks; it does not improve fact correctness. On saturation, keep the shared facts and use authored reactions. |
| **(d) Client-side generation for private reactions only** | Server inference/memory is **0** for this path. Each active client runs its own model; aggregate client CPU and memory scale with connected clients. Client hardware, model footprint, initialization, and latency are **not measured** here. | Server can keep canonical facts consistent, but clients may show different or fabricated private claims, and output cannot be trusted or replayed as server history. Only plausible for clearly non-authoritative decoration; authored private reactions remain fallback. Not a starting choice. |
| **(e) Authored-first; AI on a subset of beats** | Reuses **(b)** but sends only a fraction **q** of eligible moments to inference. In the one-request-per-player stress case, request counts are **q / 10q / 100q / 1,000q** at the four listed player loads; memory remains one shared model. **q** and actual CPU usage are unmeasured and must be chosen from pilot telemetry. | **Recommended rollout policy with (b).** Authored text is always available, keeps humor predictable, and limits overload. At a busy gate, deadline, rejection, or invalid output, retain the authored line and record the reason; never queue indefinitely or retry a stale moment. |

**How to read the capacity estimates:** The **1 / 10 / 100 / 1,000** loads are the requested concurrency scenarios, not measurements. The arithmetic assumes one independent, eligible, short reaction per player at once and uses the isolated **31-token** response range above; it ignores prompt growth, contention, other AI work, and structured multi-reaction output. In practice, one shared event may include multiple participants and need only one narrator request; a busy shared world may also generate no request and use authored output. CPU utilization and model memory for the installed Phi model have not been measured. The model-pool projection is an idealized batching estimate, not a promise that a small pool meets the current **25-second** deadline.

**Explicit scale-out comparison (all projections are estimates):** `t` is the **measured** isolated **8.29–13.42 second** response range used as a planning proxy; `M` is the AI design doc's **unmeasured** **0.25–0.4 GB** candidate model footprint; `q` is the **unmeasured** fraction of eligible moments sent to AI. Each cell assumes one independent eligible reaction per player at once. “Service work” is aggregate inference wall-time demand, **not CPU-seconds or CPU utilization**.

| Architecture | 1 concurrent player | 10 concurrent players | 100 concurrent players | 1,000 concurrent players |
|---|---|---|---|---|
| **(a) One model/player** | **1** copy; **M** RAM; **t** per player only with spare compute; **1×t** service work. | **10** copies; **10×M** RAM; **t** per player only with **10** independent compute slots; **10×t** service work. | **100** copies; **100×M** RAM; **t** per player only with **100** independent slots; **100×t** work. | **1,000** copies; **1,000×M** RAM; **t** per player only with **1,000** independent slots; **1,000×t** work. Actual RAM/latency under contention is unknown. |
| **(b) One shared model** | **1** copy; current Phi RAM unknown; last response **t**; **1×t** work. | **1** copy; RAM unknown; last response **10×t = 82.9–134.2 seconds**; **10×t** work. | **1** copy; RAM unknown; last response **100×t = 829–1,342 seconds**; **100×t** work. | **1** copy; RAM unknown; last response **1,000×t = 8,290–13,420 seconds**; **1,000×t** work. |
| **(c) Pool of 2–4 models** | **2–4** copies; **2–4×M** candidate RAM; **t**; **1×t** work. | **2–4** copies; same RAM; **3–5 batches × t = 24.9–67.1 seconds**; **10×t** work. | **2–4** copies; same RAM; **25–50 batches × t = 3.45–11.18 minutes**; **100×t** work. | **2–4** copies; same RAM; **250–500 batches × t = 34.5 minutes–1 hour 52 minutes**; **1,000×t** work. |
| **(d) Client-side** | **0** server copies/CPU; client RAM and latency unknown; **1** client request. | **0** server copies/CPU; **10** clients each use unknown local RAM/latency; **10** client requests. | **0** server copies/CPU; **100** clients each use unknown local RAM/latency; **100** client requests. | **0** server copies/CPU; **1,000** clients each use unknown local RAM/latency; **1,000** client requests. |
| **(e) Authored-first subset** | **1** shared copy; current Phi RAM unknown; about **q×t** expected work. | **1** copy; RAM unknown; about **10q** requests / **10q×t** service work. | **1** copy; RAM unknown; about **100q** requests / **100q×t** work. | **1** copy; RAM unknown; about **1,000q** requests / **1,000q×t** work. Burst latency follows (b) for the admitted requests. |

For **(a)**, the candidate RAM figures work out to **0.25–0.4 / 2.5–4 / 25–40 / 250–400 GB** across the four loads. For **(c)** they are **0.5–1.6 GB** for the pool at every load. These use the unmeasured SmolLM2 candidate estimate, not a measurement of the installed Phi model. For **(a)** the per-player latency is an optimistic independent-capacity assumption; for **(b)** and **(c)** queueing is explicit; for **(d)** hardware performance is unknown; and for **(e)** the admitted fraction is unknown. The service-work figures for server inference are estimates from measured elapsed times, not measured CPU consumption. Contention, longer prompts/outputs, queue admission, and the current deadline can cause fallback well before the idealized burst finishes.

**Trait voice recommendation:** Use Brawler, Cautious, Gadgeteer, and Hoarder as four small voice archetypes, not four separate models. Tune their reaction framing (e.g. confrontation, risk accounting, systems curiosity, object valuation) only after authored examples and blind playtests show that the voices are distinguishable without creating new facts. The traits are existing deterministic choice weights; treating them as established narrative personalities is a proposal, not current behavior.

The “sub-50 ms” small-model latency and “250–400 MB” memory statements in the AI design doc are **not measured**. No current evidence justifies choosing a per-player model or claiming any stated player count will meet a latency objective.

## 7. Guardrails

- **Facts first:** commit the Shared Moment from server state before narration. The director and model cannot alter participants, roles, actions, results, timestamps, item ownership, or settlement.
- **Structured reaction only:** require exactly the expected participant IDs and one reaction per participant. Reject unknown IDs, missing/duplicate reactions, extra fields, malformed output, or any factual name/item/outcome not present in the authorized fact set. The shared moment itself remains authored from facts.
- **Reuse current checks, do not overclaim:** retain sentence/character checks, actor/outcome denial, allowed proper-noun vocabulary, delivery-time revalidation, and explicit failure reasons. Add exact participant and fact allowlists. The current lexical checks are not a semantic proof that prose contains no implied claim; keep reactions short and subjective, and keep the mechanically complete shared line visible independently.
- **Fallback:** an authored line is immediately available. On timeout, busy/full queue, model unavailability, stale event, rejection, or validation error, show it and preserve the reason/status. No silent “generated” success, no retry that duplicates or rewrites a committed moment.
- **Proposed rate limits:** at most **one AI reaction per Raider per 5 minutes** and **one AI batch per world per 60 seconds**, with the existing **one-pending-live-request-per-world** admission and **25-second** deadline retained unless a measured review approves a change. These are initial safeguards to test, not performance or balance findings. A deployment-wide quota/cost budget is not established by the current local model evidence and must be measured before public use.
- **Player-influenced text:** free-form Handler or player text is out of the first slice. Use server-owned IDs, traits, and enumerated Signal actions only; escape display names for presentation and do not interpolate arbitrary names or text into a prompt. If free text is considered later, it needs input length limits, moderation before storage and prompt use, output validation, abuse/rate controls, privacy review, and an authored-only fallback when moderation is unavailable. It never becomes a gameplay command or fact.

## 8. Prerequisites and ordering

**Must exist before persistent player-facing story memory:**

1. **Durable identity and persistence (S6).** The current Development service is in-memory and restart clears identities/access and raid state. It is not safe to treat its memory as durable player continuity.
2. **Event retention and deletion policy.** Decide which shared facts are retained, for how long, how account deletion/export works, and how private memory references are removed without changing other players' canonical records.
3. **Memory storage and replay contract.** Persist versioned reducer state, last processed sequence, relationship projections, beat choices, and narration/fallback status alongside or reconstructable from committed events. Recovery must not duplicate settlement or re-run a model to recreate the past.
4. **Visibility contract.** Define what is public to participants versus private to one Handler before prompts or callbacks can carry those fields.

**Can be designed now, without implementation:** the Shared Moment schema candidate, authored-only beat rules using existing committed events, memory reducer and privacy requirements, model workload measurement plan, and a small set of deterministic fixture-based acceptance tests. This proposal does not activate that work. A true saved-memory callback across restarts must wait for the relevant durable persistence and identity proof; the current in-memory experiment cannot validate that requirement.

Suggested order after approval: settle the identity/retention/visibility decisions; complete the narrow S6 persistence proof; implement and verify an authored-only shared-moment slice; then measure optional AI reactions on the same fixtures; expand beat eligibility only after human review.

## 9. Vertical-slice plan

**Proposed smallest proof:** two to four actual Raiders in one server-owned event encounter the same existing robot. Record one Shared Moment, select one authored beat, and produce one trait/history-shaped reaction per Raider from that identical fact record. Persist a compact memory entry; a later shared event should call back to the earlier moment. Cross-restart memory validation is part of the slice only after durable S6 storage exists.

**Acceptance criteria (proposed):**

- Every participant projection references the same event ID, sequence, canonical-facts version, and outcome; automated comparison finds no fact or ownership differences.
- The result is produced once under retries/replay. Reprocessing does not re-settle gameplay, duplicate the beat, or call generation to reconstruct an already committed result.
- Each participant receives exactly one allowed reaction or the authored fallback. Different wording never changes shared facts; no private data crosses Handler boundaries.
- Deterministic beat/memory reduction produces the same selected beat and memory projection from the same ordered event stream.
- The later callback refers to a persisted earlier event and only states facts supported by it. If durable storage is not yet available, mark that criterion blocked rather than claiming the in-memory prototype proved persistence.
- Narration remains asynchronous and cannot block an authoritative simulation tick. AI failure produces immediate authored output with an explicit recorded failure/fallback reason.

**Human playtest (proposed, 3–5 testers):** show each tester their own Handler view after a two-to-four-Raider shared encounter, then show a later callback. Ask: What happened mechanically? Did other Raiders experience the same event? What did your Raider personally think? Did the callback feel earned or intrusive? Did any line claim something you did not observe? Which Raider voice was recognizable? Would you want this kind of shared story again? Ask participants to compare notes only after recording individual answers; compare factual agreement separately from reaction variety. Evidence of fun is spontaneous recall, clear distinction between shared facts and personal reaction, recognition of the recurring Raider/relationship, and a stated desire to see another encounter—not merely “the text was different.”

**Measurements to capture (proposed instrumentation; no targets asserted):** event-to-authored display latency; model queue wait and end-to-end latency; accepted/rejected/timeout/busy/stale counts; authored fallback rate and reason; per-trait and per-beat acceptance; callback eligibility and actual callback frequency; duplicate/stale upsert attempts; prompt and output token counts; memory entries and bytes; privacy/fact-validation violations. Report sample size, model/device/configuration, and the fact that small playtests do not establish population-level enjoyment.

## 10. Risks and what could go wrong

| Risk | What it looks like | Mitigation or evidence needed |
|---|---|---|
| Repetition | The same beat or joke returns despite different events. | Deterministic cooldowns, authored variants, callback caps, and playtest repetition notes; do not claim model randomness solves it. |
| Tone drift | A line is dramatic, mean, or too explanatory for the deadpan voice. | Review against the existing tone guide; keep authored baseline; sample human ratings, not only validator pass rates. |
| Latency and cost | The global gate backs up, so most live reactions miss the deadline. | Start authored-first; batch reactions per shared moment; measure queue wait, workload, and CPU before increasing eligibility. |
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
| Narrator architecture? | **Approve one shared server narrator with per-character context; no model per player.** | It reuses one service/model, centralizes shared facts, and is the architecture most compatible with limited CPU evidence. |
| AI rollout policy? | **Use authored-first; AI only on a measured subset, always with an authored line ready.** | The authored baseline is already trusted operationally; current native samples are small and not concurrent. |
| Per-trait voice tuning? | **Try four lightweight authored voice profiles for the existing traits after the shared-fact slice passes.** | It may make Raiders recognizable without multiplying models, but the current traits are decision weights, not established voices. |
| Player-supplied free text? | **Exclude it from the first slice.** | It adds moderation, privacy, and prompt-injection risk without being necessary to prove the shared-story idea. |
| Human pilot and acceptance? | **Use the proposed 3–5 tester pilot; approve broader targets only after seeing the evidence.** | This yields initial qualitative evidence without pretending a small sample establishes overall fun or model reliability. |

No balance values or rules changes are requested by these decisions.

## 12. Out of scope

- Implementing any code, runtime content, tests, balance tuning, rules version/fingerprint changes, or model deployment.
- Changing simulation outcomes, autonomous economy, Handler economy controls, ownership, or settlement.
- PvP, theft, or any new favor, rescue, or betrayal mechanic.
- Letting AI choose beats, actions, relationship state, loot, prices, or outcomes.
- Treating generated prose as canon, or adding any proposed example as approved AFK lore.
- Replacing authored narration as the entertainment baseline.
- One model per player, freeform player chat, public matchmaking, production accounts, or a claim that current tests prove production scale.
- Writing millions of hand-crafted storylines or promising limitless generated variety.

## Source documents consulted

- [START-HERE.md](START-HERE.md), [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md), [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md), and [GAME_DESIGN.md](GAME_DESIGN.md).
- [Lore README](lore/README.md), [PARODY_MAPPING.md](lore/PARODY_MAPPING.md), [BACKLOG.md](lore/BACKLOG.md), [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md), and [LEGAL_GUARDRAILS.md](lore/content-guidelines/LEGAL_GUARDRAILS.md).
- The lore entity references for [Desperanza](lore/entities/desperanza.md), [factions and brands](lore/entities/factions-and-brands.md), [field phenomena](lore/entities/field-phenomena.md), [robots](lore/entities/robots.md), [skills](lore/entities/skills.md), [traders](lore/entities/traders.md), [weapons](lore/entities/weapons.md), [world history](lore/entities/world-history.md), and [zones](lore/entities/zones.md).
