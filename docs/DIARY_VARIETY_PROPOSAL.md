# Diary Variety and Voice — Design Proposal

**Status: Original proposal retained for context. Designer authorized slice 1 Parts A/B on 2026-11-11; Part C requires review after Part B. Broader options remain proposals. See the [measured audit](DIARY_VARIETY_AUDIT.md) and [handoff](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint).**

The slice-1 decision supersedes this proposal's original blanket prohibition on
narration-only content-version/compatibility changes: only those may change, with
explanation. Gameplay rules, outcome fingerprints, state, timing and balance stay
frozen. Pilot drafting is approved only after the explicit Part B review stop;
new runtime content and live AI are not approved by this slice.

This proposal does not authorize runtime content, gameplay or simulation-state changes, balance changes, rules-version or fingerprint changes, or a backend restart. All sample lines below are proposals, not canon or approved runtime content. Numeric results are labelled **Measured**, projections are **Estimate**, and recommendations or thresholds are **Proposed**.

## 1. Summary and recommendation

The diary is strongest when it reacts to a specific committed event. It feels repetitive when several unrelated events share the same small trait fallback pool, and when numeric state changes and explanatory boilerplate compete with the story.

**Recommendation (Proposed):** first make the visible record easier to read by moving mechanical detail out of the main sentence and hiding repetitive no-effect boilerplate behind event details. Then give common event families context-fit lines and a deterministic per-Raider recent-line governor. Expand all four trait voices and the Hub pool, use history only when committed records support it, and add rarity tiers only after the basic pools read well. Use AI to draft offline under human review. Defer live AI to optional last-mile variation until an authored version meets the repetition and comedy gates.

The analysis below finds high generic-pool concentration for Brawler and Gadgeteer, while the provided short Gadgeteer sample also reports a high repeat rate. Both measures support fixing event fit and pool reuse rather than simply adding a few more generic jokes. The measured repetition rates vary substantially by trait, so no single short run should be treated as a universal player experience.

## 2. Baseline and root causes

### Designer-provided sample

The designer supplied a **Measured (provided, not independently reproduced)** sample of **290** diary entries from one Gadgeteer Raider over about **67 minutes**, with authored fallback only and AI offline:

| Metric | Designer-provided measurement |
|---|---:|
| Raider asides / distinct exact quotes | **234 / 110** |
| Share of all asides supplied by the six Gadgeteer lines listed below | **98 / 234 = 41.9%** |
| Asides repeating a quote from the previous **20** quotes | **31%** |
| Whole-entry repeats | **2%** |
| Hub activity entries / unique Hub lines | **28 / 10** |
| Mechanical “Staying deeper” entries | **37 / 290 = 12.8%** |

The six lines named in the sample are:

- “I packed the adapter. Now I need to identify what it adapts.”
- “This battery has been assigned three jobs and a supervisory title.”
- “The equipment is ready. Its instructions remain theoretical.”
- “I am optimizing the plan around the device I brought by mistake.”
- “I have a tool for this. The tool is currently evaluating me.”
- “Every spare component is essential once the correct component is known.”

The supplied sample reports those lines after unrelated shield depletion, unmitigated damage, and Greed Check events. That is a context-fit problem as well as a pool-size problem: the factual receipt differs, so whole-entry repeats stay low even when the joke repeats.

### Read-only re-measurement

**Measured (this proposal’s harness):** I added [DiaryVarietyReadOnlyAnalysis](../tools/DiaryVarietyReadOnlyAnalysis/DiaryVarietyReadOnlyAnalysis.csproj), a read-only analysis utility. It advances isolated, in-memory coordinator simulations and writes aggregate JSON to standard output; it does not persist game data or edit gameplay code. The run used **three** fixed seeds (**101, 202, 303**) for each of the **four** existing traits, for **24 simulated hours** per Raider, with the current authored template narrator and no live model generation. It sampled one autonomous Raider per cohort, with no Handler commands or human crossings. The code checks the trait selected for each seed and reads the final journal projection.

“Quote” means the exact text after `Raider: ` in a journal entry, trimmed at the ends. “Repeating within the previous **20** or **50**” means that exact quote already appeared among the immediately preceding quote entries for that Raider; the search is reset for each seed. The top-six share is the fraction of all quote occurrences accounted for by the six most frequent exact quotes. “Unique Hub lines per stay” is the mean number of distinct `HubActivity` texts between successive `RaidAdmission` journal entries, over observed stays with at least one Hub activity; the **Measured (current configuration)** Hub wait is **10 minutes**. “Staying deeper” is the exact `PersonalityChanged` reason. The broader mechanical-format figure counts every `PersonalityChanged` delta entry, including other reasons. “Distinct quotes per **100** journal entries” is total unique exact Raider quotes divided by all journal entries, multiplied by **100**; it is a whole-run variety density, not a rolling-window score.

| Trait | Mean quotes per simulated day | Unique quotes per Raider | Top-six quote share | Repeat within previous 20 | Repeat within previous 50 | Unique Hub lines per stay | Mechanical format: exact “Staying deeper”; all delta entries | Distinct quotes per 100 journal entries |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Cautious | **Measured: 6,854** | **Measured: 152** | **Measured: 20.0%** | **Measured: 5.4%** | **Measured: 40.1%** | **Measured: 3.4** | **Measured: 9.8%; 14.4%** | **Measured: 1.95** |
| Hoarder | **Measured: 6,896** | **Measured: 147** | **Measured: 34.1%** | **Measured: 26.3%** | **Measured: 40.2%** | **Measured: 3.6** | **Measured: 9.9%; 14.5%** | **Measured: 1.88** |
| Brawler | **Measured: 6,878** | **Measured: 146** | **Measured: 40.0%** | **Measured: 30.5%** | **Measured: 40.4%** | **Measured: 3.7** | **Measured: 9.8%; 14.4%** | **Measured: 1.87** |
| Gadgeteer | **Measured: 7,031** | **Measured: 146** | **Measured: 40.1%** | **Measured: 31.1%** | **Measured: 40.5%** | **Measured: 3.4** | **Measured: 9.8%; 14.5%** | **Measured: 1.84** |

**Measured cadence:** the harness recorded about **56–57 raid admissions per Raider per simulated day**. The highest-volume quote event kinds per day, pooled across traits, were `AreaDiary` (about **1,780**), `PersonalityChanged` (about **1,130**), `AreaActivityStarted` (about **655**), `AreaActivityCompleted` (about **605**), `ItemAllocated` (about **275**), and `Travel` (about **258**). These are isolated deterministic-harness counts, not a forecast of live-player behavior. The most frequent kinds include contextual pools and therefore are not all evidence of the generic fallback problem.

**Limitations:** the **12** cohorts are a small deterministic harness sample, not a population study or a comedy review. Each cohort had one Raider, so it cannot measure the co-presence suffix or actual shared encounters. The one-Raider sample does not reproduce the designer-provided **290-entry** run. The use of a full-day journal makes the whole-run “distinct per **100** entries” ratio naturally small once the finite catalog has been visited; use the rolling repetition measure for an acceptance gate. The “Staying deeper” percentage is lower than the broader delta-entry percentage because it excludes other personality-change reasons. The no-live-model setup measures only authored fallback.

### What the implementation does

- [RaidNarrator.cs](../server/AFKRaiders.Server/Domain/RaidNarrator.cs) appends a `Raider:` aside when an event has its original factual text. It passes the event kind, activity, sequence or rotating voice index, and Raider trait to `RaiderCommsContent.Line`.
- [RaiderCommsContent.cs](../server/AFKRaiders.Server/Domain/RaiderCommsContent.cs) first selects event/activity pools for recognized contexts such as diary activity, travel, search, item allocation, robots, and extraction. Unhandled kinds fall through to trait pools: `voice.hoarder`, `voice.brawler`, and `voice.gadgeteer`; every other trait, including Cautious, falls through to `voice.general`. The final choice is derived from the event sequence, or a rotating voice index when supplied. Neither path tracks recent exact lines or event truth beyond the pool key.
- [narration-catalog.json](../server/AFKRaiders.Server/Content/narration-catalog.json) contains exactly **6** Gadgeteer lines, **6** Brawler lines, **7** Hoarder lines, and **12** general lines. It has no Cautious-specific voice pool. The catalog also has context-specific pools that are materially larger and are not the same root cause.
- [NarrationContracts.cs](../server/AFKRaiders.Server/Content/NarrationContracts.cs) and [NarrationContent.cs](../server/AFKRaiders.Server/Content/NarrationContent.cs) enforce the current known pool IDs, distinct nonempty lines, supported placeholders, and content shape. They do not measure event fit or recent repetition.
- [RaidCoordinator.cs](../server/AFKRaiders.Server/Domain/RaidCoordinator.cs) selects Hub activity from the fixed `hub.activities` pool by `HubActivitySequence % lines.Count`. The pool contains **10** lines. It rotates, but has no per-stay history, event state, or variety governor.
- [RaidSimulation.cs](../server/AFKRaiders.Server/Domain/RaidSimulation.cs) emits `PersonalityChanged` entries with numeric Greed, Mood, and resilience deltas. It emits `AreaDiary` text through a fixed receipt template. [narration-catalog.json](../server/AFKRaiders.Server/Content/narration-catalog.json) supplies the recurring “No loot, damage or new actor created” suffix; when another Raider is actually co-present, a second suffix states that they are present. Those facts are useful in the event record but repetitive in the main diary sentence.

**Measured (source inspection):** the catalog contains exactly **6** Gadgeteer lines, **6** Brawler lines, **7** Hoarder lines, and **12** general lines, with no Cautious-specific voice pool. The Hub pool contains **10** lines. These paths explain the observed pattern: common, otherwise-unmapped event kinds reuse a small generic voice pool; there is no short-term line memory; and area-diary receipts repeat the same negative-effect explanation even when it is already available as structured event detail. The context-specific activity and location text is a useful base, not a reason to rewrite all narration.

## 3. Repetition measure to track

**Proposed primary metric:** per Raider, count the percentage of `Raider:` quote entries whose exact trimmed quote matches any of the immediately previous **20** Raider quotes. Reset at the start of each Raider’s measured run. Report the numerator, denominator, trait, event-kind mix, horizon, seed count, and whether authored fallback or AI output supplied the line.

**Proposed initial target:** no more than **10%** repeated within the previous **20** quotes in each trait’s representative **8-hour** harness run. This is a review target, not a validated fun threshold. Report the previous-**50** repeat share and top-six share as supporting diagnostics; neither should be hidden by averaging traits together. Add a rolling distinct-line count per **100** quote entries so the whole-run unique-count ratio does not obscure local variety.

## 4. Fix options

Every option below is presentation/content-only. None may change simulation state, timing, item ownership, rewards, Signal, Raider decisions, balance, rules versions, or fingerprints.

| Option | Expected effect | Cost | Risk | Gameplay state change? |
|---|---|---|---|---|
| **(a) Event-specific quote pools** for damage, Greed Check, extraction decisions, allocation, and other frequent events | Makes each aside describe the actual moment and prevents unrelated events from sharing one small fallback pool. | Medium: author, review, and maintain per-event pools; add event-to-pool coverage tests. | A line can still imply a fact the event did not commit; rare events may have shallow pools. | **No.** |
| **(b) Deterministic per-Raider repetition governor** with recent-line memory, no reuse within a proposed window, and stable rotation/tie-breaking | Directly lowers exact short-window repeats while keeping replay stable. | Medium: separate presentation metadata and deterministic tests, including pool exhaustion. | A too-small or context-filtered candidate set can exhaust; fallback must not become a context mismatch. | **No.** Store only narration-selection metadata, outside gameplay outcomes. |
| **(c) History-aware lines** such as first visit, repeated bad search, raid veteran, comeback, or streak | Adds continuity and personal recognition from events that actually happened. | Medium-high: define factual counters, retention, privacy, and review rules. | A stale or incorrect memory can narrate an event that never happened. | **No.** Derive from committed facts; do not make a new event or decision. |
| **(d) Template slots** for location, object, robot, weather, and item, with grammar and fact checks | Increases combinations while keeping the joke tied to a known subject. | Medium: author templates, validate slot types and grammar, and test every substitution. | Agreement errors, unsafe/missing values, and fabricated facts from an unchecked slot. | **No.** |
| **(e) Rarity tiers** (`common`, `uncommon`, `rare`) | Keeps the best callbacks special and supports pacing beyond simple rotation. | Low-medium: author tags and deterministic frequency rules. | “Rare” lines can be starved or become confusing if no rarity contract is visible to reviewers. | **No.** |
| **(f) Rate-limit mechanical lines** and move changing numbers to factual suffixes or collapsible details | Improves story readability without losing auditability or committed numbers. | Low-medium: separate narrative presentation from the event details and verify the UI projection. | Hiding a number could obscure a consequence important to the player. Keep authoritative details accessible. | **No.** |
| **(g) Hide repetitive boilerplate** such as “No loot, damage or new actor created” from visible diary text while retaining it in the event record | Removes repeated negative statements from every area-diary line. | Low: presentation-only projection and regression tests. | A reader may mistake a flavor observation for a consequential event if scope is not clear. | **No.** |
| **(h) Hub variety:** larger pool, Hub-specific history, time-of-day context, and other actual residents | Makes Hub stays feel less like a fixed ten-line loop. | Medium-high: author more Hub lines and define which resident facts are visible and current. | Privacy leakage or a line implying an interaction that did not occur. | **No.** |
| **(i) AI-assisted offline authoring** with human review, legal/tone checks, and retained provenance | Scales drafts without placing generation in the gameplay path. | Medium: drafting/model cost plus substantial editorial and QA time. | Generated drafts can drift in tone, copy source phrasing, or assert unsupported facts. | **No.** Drafts are not runtime content until separately approved. |
| **(j) Live AI for last-mile variation only**, with a per-entry budget, strict validation, and immediate authored fallback | Can vary an otherwise-correct event-fit line without delaying the authoritative diary. | High: provider cost, latency/availability monitoring, privacy review, validation, and fallback maintenance. | Unsupported factual claims, private-data exposure, rejected or late output, and cost spikes. | **No.** Output is optional presentation; no model result may affect simulation. |
| **(k) Small trait-voice expansion** for all four existing traits, always subordinate to event fit | Gives Brawler, Cautious, Gadgeteer, and Hoarder distinct but context-fitting reactions. | Medium: voice guide, authored examples, and trait/event review. | Caricature, trait stereotypes, or a joke that overrides what actually happened. | **No.** Traits affect wording only. |

**Recommended order (Proposed):** begin with **(f)** and **(g)** to declutter the current diary; then implement **(a)** and **(b)** as a separately approved slice. Add **(c)** and **(k)** using only committed facts, then **(d)** and **(e)** where tests show a need. Expand Hub-specific content under **(h)**. Use **(i)** to accelerate human-reviewed pool growth. Consider **(j)** last, only after an authored baseline passes the same factual, privacy, latency, repetition, and comedy gates. This order is not implementation authorization.

### Twelve proposed example lines

These are **Proposed** lines only; any placeholder must be populated from the corresponding committed event. If a required field is missing, that variant is ineligible and a source-free alternative must be selected. Each line states what happened before its deadpan joke.

| # | Event family | Proposed line |
|---:|---|---|
| 1 | Damage taken | “I lost {health} health to {source}. I have promoted the incident to a safety briefing.” |
| 2 | Damage taken | “My shield absorbed {damage} damage. It has requested hazard pay.” |
| 3 | Damage taken | “Unmitigated hit: {damage} health lost. I have revised my definition of ‘unmitigated’ to include me.” |
| 4 | Greed Check | “Greed Check: I stayed in the raid. My exit plan remains available as a theory.” |
| 5 | Greed Check | “Greed Check: I chose extraction. The loot can continue its career without me.” |
| 6 | Greed Check | “Greed Check: I stayed for now. Restraint has been placed on hold again.” |
| 7 | Extraction decision | “I chose extraction from {location}. The deadline has finally received a straight answer.” |
| 8 | Extraction decision | “The extraction vehicle arrived. I am not going to start a second discussion with it.” |
| 9 | Extraction decision | “Extraction complete. I am no longer in the field; please file the silence as progress.” |
| 10 | Item allocated | “Item {item} was allocated to me. My backpack has accepted another management responsibility.” |
| 11 | Item allocated | “I received {item} through the allocation. The bag has won another argument.” |
| 12 | Item allocated | “The allocation gave me {item}. I will call this inventory expansion a unanimous committee decision.” |

## 5. Content scale estimate

**Measured cadence:** about **56–57** admissions and **6,854–7,031** Raider quotes per Raider per simulated day in this harness. Across traits, observed mean quote counts per **24** hours were about **1,119–1,152** `PersonalityChanged`, **146–165** `ExtractionDecision`, **335–362** combined `ShieldAbsorbed`, `Resilience`, and `UnmitigatedDamage`, and **273–279** `ItemAllocated`.

The table below linearly scales those measured per-day event counts to other horizons. The **8-hour** and **96-hour** values are **Estimates**, not additional simulation runs. They describe the measured one-Raider cadence only; changes in population, traits, or future content could change the mix.

| Event family needing event-fit treatment | **Estimate: occurrences in 8 simulated hours** | **Measured: occurrences in 24 simulated hours** | **Estimate: occurrences in 96 simulated hours** | **Proposed: authored candidates per family and trait** |
|---|---:|---:|---:|---:|
| Greed / personality delta | **373–384** | **1,119–1,152** | **4,476–4,608** | **21** |
| Damage or shield consequence | **112–121** | **335–362** | **1,340–1,448** | **21** |
| Extraction decision | **49–55** | **146–165** | **584–660** | **21** |
| Item allocation | **91–93** | **273–279** | **1,092–1,116** | **21** |

The **21** candidates per family and trait are a conservative **Estimate** tied to the proposed previous-**20** metric: if one family dominates a whole recent window, **21** distinct, context-valid options let the governor choose one not seen in that window. For these **four** families across the **four** traits, that is **336** authored lines. This is a worst-case no-repeat inventory estimate, not a mathematical requirement that grows with total play duration. The same candidate count supports the **8**, **24**, and **96** simulated-hour horizons because the metric is rolling; only the number of exposures grows with time.

A smaller first content batch—**8** candidates per family and trait (**128** lines total)—is a **Proposed pilot estimate**, not a guarantee of the target under event bursts. Measure pool exhaustion and the repeat metric on the actual ordered event stream before expanding. Keep existing contextual pools such as area diary, travel, and robot lines distinct; their volume is not a reason to move them into a generic trait pool. Treat Hub activity separately: its **Measured (current catalog)** pool has **10** lines, and the harness measured about **3.4–3.7** unique Hub lines per observed stay; expanding it to **20** lines is a **Proposed estimate** for a first Hub authoring batch, not a measured need.

## 6. Authoring and loading workflow

1. **Draft from AFK-original identity.** Prefer names and concepts already in the project’s [PARODY_MAPPING.md](lore/PARODY_MAPPING.md) and lore entities. Use broad extraction-shooter tropes—raid phases, loot greed, extraction panic, traders, gear, and close calls—without copying source-game prose, names, or lore. Write `A.R.C.` with periods when referring to Aggressively Roaming Chassis; never use the raw unpunctuated acronym in player-facing content.
2. **Ground every line.** The author records the event family, required committed fields, allowed slots, trait voice, and any rarity. A line must remain true if every optional field is absent. Do not write a first-time visit, streak, comeback, actor, item, damage, ownership, or outcome unless the committed event/memory projection proves it.
3. **Use AI only as a draft assistant unless live use is separately approved.** Store the drafting model/provider and prompt/version with the review record. The assistant may suggest alternatives, not canon, facts, event IDs, or runtime data. A human rewrites or rejects each draft.
4. **Review against [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md) and [LEGAL_GUARDRAILS.md](lore/content-guidelines/LEGAL_GUARDRAILS.md).** Check that consequence comes first, there is one strong deadpan joke, and the line is readable in the Comms Log. Reject copied source prose, raw source names, unsupported mechanics, and jokes that need source-game familiarity.
5. **Validate and version.** After approval, assign stable line and pool IDs, content version, event/trait/rarity tags, required-slot schema, author/reviewer, and provenance. Run JSON/schema/placeholder checks, exact and normalized duplicate checks, trait/event fixture checks, and a search for the forbidden unpunctuated acronym. Preserve approved-source revision and paths if any existing AFK-original content is imported.
6. **Load only after approval.** The existing content approach is a versioned narration catalog embedded by the server and validated by the content contracts. Any future approved content change should use that path and update its catalog version and tests. This proposal’s example lines are not to be added to that catalog now.

## 7. Relationship to the story and world activity proposals

The [Shared Story System proposal](STORY_SYSTEM_PROPOSAL.md) treats the Shared Moment as the canonical record and Raider memory as a deterministic, versioned projection of committed facts. Diary variety should follow that boundary: a history-aware line may say “first time here” only if the stored event sequence proves it, and “comeback” only if a prior recorded loss and later return support it. The line is a personal reaction to the record, not another event; it cannot change the Shared Moment, announce a new winner, or create a memory fact.

The [World Activity proposal](WORLD_ACTIVITY_PROPOSAL.md) distinguishes actual crossings, shared committed outcomes, and presentation-only ambient events. A crossing or ambient observation can enter a diary pool only after the server commits the event with the actual participants and visibility. Cosmetic ambient activity has no mechanical effects and must not be narrated as loot, damage, or combat. Hub residents may be mentioned only when actually present and authorized for that view. No saved opponent snapshot, imagined meeting, or inferred social relationship should appear as live history.

## 8. Measurement and acceptance plan

**Proposed acceptance sequence:**

- Re-run the read-only harness with the same traits, seeds, duration, and authored-only mode before and after a separately approved content change. Report per-trait metrics; do not pool away a failing trait.
- Require the proposed **10%** ceiling for repeats within the previous **20** quotes in each trait’s representative **8-hour** run. Report previous-**50** repeats, top-six share, unique quote count, Hub lines per stay, and mechanical-format share beside it.
- Read a complete long-run transcript from each trait at **24** simulated hours. A reviewer marks event fit, repeated jokes, factual overreach, readability, tone, and whether the voice is distinguishable without changing event meaning.
- Have a separate comedy/tone review using the project’s voice guide. A lower duplicate rate is not proof that the writing is funny.
- Verify that the exact event facts, ownership, timing, and rules/fingerprint outputs are unchanged by narration selection. Tests should use identical seeds and compare authoritative outcomes, not only the formatted text.
- In a small, consented playtest, ask: “Could you tell what happened?”, “Did the line fit the event?”, “Which exact jokes did you notice repeating?”, “Did the Raider’s voice feel distinct?”, “Did the numbers or boilerplate interrupt the story?”, and “Did any line claim something you did not see happen?”

## 9. Risks

- **Tone drift or in-joke overload:** too many named callbacks can make the diary harder to follow than the current generic lines.
- **Content cost:** a full per-event, per-trait inventory can reach hundreds of reviewed lines before map, robot, and event growth.
- **Contradiction with state:** a line can imply a loot transfer, damage, extraction, actor, or history that the committed record does not support.
- **AI cost and latency:** live generation adds spend, failure modes, and queue pressure; current optional narration has measured deadlines and admission limits, not a guarantee that every diary entry can be generated live.
- **Privacy:** a cloud provider must not receive player identity or unrelated private history. Any later live-AI design needs an explicit minimization and retention review.
- **Hidden repetition:** whole entries can look unique because their factual front half changes while the aside repeats. Measure exact aside strings as well as full entries.
- **Overcorrecting with rotation:** a governor can suppress a line that fits the moment and substitute an inferior one. Candidate eligibility must be based on event truth first, then recency.
- **UI loss of facts:** hiding boilerplate must not hide a real consequence or make a cosmetic event look mechanical. Keep structured facts accessible.

## 10. Decisions for the designer

| Decision | Recommendation | Reason |
|---|---|---|
| Approve a first content-only slice? | **Proposed:** approve measurement and authored event-fit work only; no runtime content until a separate implementation approval. | Separates design acceptance from changes to the current experimental balance checkpoint. |
| Adopt the primary repetition target? | **Proposed:** start with **10%** or fewer repeats within the previous **20** quotes per trait in an **8-hour** harness run. | It is a measurable initial gate, but still needs a human read-through and playtest. |
| How much to write before measuring again? | **Proposed:** draft **8** candidates per family/trait (**128** lines) for a pilot; retain **21** per family/trait (**336** lines) as the conservative no-repeat design-space estimate. | Avoids paying for worst-case volume before observing event-fit and governor behavior. |
| Should mechanical values stay visible? | **Proposed:** preserve every committed number in structured detail or a concise suffix, while hiding repetitive explanatory boilerplate from the main diary sentence. | Improves story readability without weakening auditability. |
| Should all traits get dedicated voices? | **Proposed:** yes, but only after event fit; define Cautious explicitly and keep each trait’s joke subordinate to the event. | Current Cautious fallback is the general pool, while the other traits use small dedicated pools. |
| Should rare lines be used? | **Proposed:** defer rarity tags until the history-free event pools and repetition governor pass tests. | Rare status can obscure whether a line was eligible or simply unavailable. |
| Should AI help author or narrate? | **Proposed:** permit offline AI drafts with human/legal/tone review; defer live AI until authored fallback passes acceptance and privacy/cost/latency decisions are approved. | Drafting is easier to review and carries no live latency or provider data exposure. |
| Should Hub activity mention other residents? | **Proposed:** only when a current, authorized, committed Hub fact supports it; otherwise expand the solo Hub pool first. | Prevents invented interaction and avoids private-state leakage. |

## 11. Out of scope

- Any change to gameplay, simulation state, Raider decisions, scheduling, balance values, rules versions, fingerprints, rewards, ownership, Signal, or extraction behavior.
- Adding or loading runtime lines, new canon, new event types, new maps, robots, items, or weather.
- Implementing Shared Moments, persistent Raider memory, crossings, ambient events, or live-AI narration.
- Reproducing the designer-provided short sample from unavailable source data.
- Treating deterministic harness outcomes or this proposal’s sample text as balance validation, approved canon, or production-population evidence.
