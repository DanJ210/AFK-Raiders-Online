# AFK Raiders: AI & ONNX Systems Architecture Plan

Date: 2026-10-12  
Status: Architecture roadmap with bounded runtime status below (2026-10-14)  
Baseline alignment: [GAME_DESIGN.md](GAME_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md), [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md)

---

## Current bounded implementation (G6, 2026-10-14)

**Current implementation:** The runtime calls `IRaidNarrator.FormatEvent` on every committed event. It retains original receipts separately from immediate first-person canon fallback and queues optional local ONNX live-event enrichment through the coordinator, outside domain mutation. Source-ID upserts propagate callbacks into private journals, snapshot polling, and active/recent threads. A shared event is queued once and remains visible only to actual participants. The current simulation and G6's curated shared conditions and danger-specific robots are deterministic. **Generative mechanical anomalies and learned utility AI remain unimplemented proposals**, not requirements on world startup. Residence/condition diary uses immediate authored pools, not extra inference load; voice rotation persists across admissions. This describes current behavior, not a restriction on future approved AI decision-making or hosting choices.

Normal experimental fixtures target 900–1200 seconds via three nonreward surveying/checking/rummaging activities per arrival. This is authorized-autopilot tuning, not approved balance; no repeated loot or additional enemy is created.

Local Phi-3.5 mini CPU evidence: initialization 70.04s; 31-token short response 13.42s cold / 8.29s warm (48-token output cap). An earlier 1.5s complete-response target was never benchmarked and is superseded by **25s** for bounded optional mood, including model-gate wait. The eight-request queue never blocks the simulation; queue-full requests explicitly retain fallback, live requests expire at 90s or world completion. One model gate serializes all Comms/AAR/appraisal access. Cancellation registers GenAI's native `terminate_session` option and awaits inference exit before releasing the gate; it does not merely cancel task scheduling.

Phi prompts use `<|system|>`, `<|user|>`, `<|assistant|>` and `<|end|>` tokens, `I consider` prefill, and physical-object cues. As of COMMS-4 (2026-10-16), `max_length` is encoded prompt length **plus 96 output tokens**, with early stopping at the first sentence ending and the unchanged 25-second total deadline. COMMS-5 (2026-10-19) doubles only the character cap: both prompt builders request a complete dry opinion joke of at most **400 characters**. Validation accepts 8-400 characters and at least four words, with no separate word-count maximum; it retains the single first-person sentence, punctuation/character, outcome/actor denylist, and unknown-proper-noun checks. Logging keeps a 400-character preview and reports raw length plus truncation explicitly. Preview truncation is separate from acceptance and never rewrites text. The 96-token completion budget remains unchanged; this character-cap change is not native latency or acceptance-rate evidence. This is not proof that arbitrary subjective prose contains no world claim: model input excludes rewards/health/outcome details, and factual receipts remain separate. AAR/appraisal facts remain deterministic and accepted mood is appended with a Generated label. Fallback AAR results retain `ArchivalLog`, not a false Generated source.

The actor/outcome vocabulary is code-owned: `DeniedWords`, `AllowedTermsFor`, and `ValidateAmbientCore` in [OnnxGenAiNarrativeGenerator.cs](../server/AFKRaiders.Server/AI/OnnxGenAiNarrativeGenerator.cs). It is not an authored content-pool setting. Failure reason `ForbiddenOutcomeOrActorWord:<token>` identifies the first blocked word. Grounded proper nouns and robot-context exemptions remain narrowly scoped; arbitrary actors/outcomes are not enabled by the larger length budget. Conservative lexical false positives remain a known limitation. The higher completion ceiling may spend more of the same deadline; it does not prove native latency or rejection-rate improvement.

**COMMS-6 grounded possessives (2026-10-22):** Generation and delivery validate ASCII `'s` possessives against their base terms, without rewriting the accepted sentence. `Tunnel's` therefore matches a context-approved `Tunnel`; unknown names still fail, and possessive actor/outcome words use the same denials and context-scoped exemptions as their base terms. The user's exact 226-character example passes validation and journal/thread delivery. Focused deterministic narration tests passed 114/114; no native latency/acceptance sampling or live backend restart. Prompt, 400-character/96-token limits, deadline, admission and gameplay remain unchanged.

Verification: final G6 regression **157 server tests**, **5 frontend journal/lifecycle tests** and production build passed. Tests exercise the cancellation/serialization execution seam with a running cancellable backend, not installed native weights. Native prompt candidates accepted 0/6, then 1/6, then **6/6 distinct** outputs with the final prefill/cues (12.60-23.48s after 32.58s initialization). This small sample does not prove stable acceptance or funny prose. Browser G6 mechanics delivery is tested separately in template mode; it is not an installed-model two-client soak. Strict guard may reject useful prose; authored fallback remains the entertainment baseline and the human comedy gate remains open.

**Reliability follow-up (2026-10-14):** Live-event admission now permits one pending request per world. Other eligible events explicitly retain authored narration with `NotRequested / LiveBudgetBusy` and are not retried later. The eight-slot queue remains for accepted live requests and terminal debriefs; this is admission control, not more inference concurrency or a longer deadline. Live failure reasons propagate through source-ID journal upserts to expandable Authored/AI-enhanced badges: output rejection, model-gate deadline, native inference deadline, queue unavailable/full, worker/model errors and staleness. Existing historical entries lack this new field; their original reasons cannot be reconstructed from status alone.

**COMMS-3 structured prompts (2026-10-15):** Live asides now receive an `AmbientCommsContext` (location, committed activity, event kind, condition, danger, robot name, anonymous nearby-Raider count) instead of a single style hint. The context maps to one of nine categories (AreaActivity, Search, Travel, Robot, SharedRobot, Exit, Diary, Floor, General), each with its own situation phrase and joke subject; locations add a short lore cue. The prompt contains only committed facts. The validator uses context-derived allowed proper nouns: the robot name and robot vocabulary are allowed only for robot categories, and the system prompt forbids naming winners or injuries. The same terms are used to revalidate output on delivery. Scheduling, deadlines, the 32-token cap and the one-request-per-world admission are unchanged. Deferred: Desperanza resting and Hub-crowd prompts, which need an AI-eligible Hub event cadence and a privacy decision about crowd counts.

**Current implementation—offline diagnosis follow-up (observed 2026-10-15):** The observed running Development backend reported `OnnxGenAiNarrator / available: false / Offline (Template Fallback)`. That explains eligible authored-only entries without establishing whether its launch omitted the path or model initialization failed. Missing/invalid model directories now produce explicit startup warnings. Blank `AI:ModelPath` falls through to `AFK_AI_MODEL_PATH`; no hard-coded model path or automatic download is introduced. Eligible events observed while ONNX is unavailable retain `NotRequested / ModelUnavailable`, propagated to explanatory journal details. Repeated reads do not repeatedly increment the presentation revision. An isolated launch with the existing model and blank configuration verified available status and accepted live AI-enhanced browser delivery. Guards, candidate catalog, deadlines and single-pending admission remain unchanged.

The unchanged current prompt accepted 12/12 isolated samples (8 distinct) before admission changes; this does **not** establish why the user's historical 41 fallbacks occurred. Updated result handling accepted 6/6 distinct samples in 5.99-9.29s, init 20.22s, and native cancellation produced explicit `CallerCancelled` fallback in 0.19s. No model/guard/prompt budget was changed. Fixed-prompt repeats remain possible: deterministic decoding with a finite topic set is not limitless generated variety. The admission policy targets burst backlog/expiry, not guaranteed generation quality.

Initial changed-code native smoke (2026-10-13): init 59.19s; Market Surveying ambient took 25.18s and fell back to ArchivalLog (prompt too long, closed ~28-option grammar); 100ms cancellation returned fallback in 0.20s. Subsequent compact-prompt generation and live delivery succeeded, as recorded in the progress ledger. The latest rejection-tuning smoke accepted Checking and Surveying in 21.36s / 11.06s; Rummaging fell back in 18.59s with `MissingSentenceTerminator`. Rejection logs now distinguish validation reasons and explicitly label preview truncation, rather than implying that a 120-character preview is the complete model output. The unchanged guard never patches an unfinished sentence or loosens outcome checks. Fallback remains the entertainment baseline; smoke samples do not establish an acceptance-rate or latency guarantee.

The sections below retain future architectural intent; unmeasured candidate latency claims and generative world/fact examples are not current runtime guarantees.

---

## 1. Executive Summary & Design Philosophy

AFK Raiders Online is an autonomous extraction comedy RPG where the player acts as a remote **Handler** in Desperanza monitoring an autonomous **Raider** via the **Comms Log**. 

Local ONNX Runtime provides one option for emergent, non-repetitive comedy, deadpan radio dialogue, unique shared zone anomalies, and post-raid After-Action Reports (AARs). The current bounded implementation uses local inference for optional narration. Cloud-hosted models and paid APIs are also permitted by the designer decision recorded in the [implementation ledger](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint); model, provider, and role choices remain future design work.

### Core Architectural Invariants

1. **AI authority follows approved design:**
   - The designer decision of 2026-11-06 removed the permanent rule that AI cannot decide outcomes. Future approved designs may let AI participate in decisions, direct systems, and act as agents, including AI-controlled Handlers or Raiders.
   - This permission is not implementation approval. The current simulation is deterministic, authoritative, and testable; its seeded C# rules describe current behavior, not a permanent ban on AI decision-making.
   - **Current implementation (ECON-1/2):** Raider sell, buy, equip, repair, and departure decisions use a deterministic, trait-weighted C# utility policy. This is current behavior, not a frozen requirement for future decision systems.
   - AI-generated output must remain consistent with the facts and state committed by the authoritative server. Narration currently follows [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md).

2. **Strict Non-Blocking Simulation Ticks:**
   - The authoritative simulation tick loop (`RaidClock.cs`) must not wait on inference.
   - **Never execute AI inference synchronously inside `AdvanceTicks()`**.
   - Generative inference runs asynchronously in background worker services (`System.Threading.Channels` or `IHostedService`) and streams or broadcasts narrative updates via SignalR (`RaidHub.cs`).

3. **Deterministic Shared-World Consensus:**
   - When multiple Raiders experience a shared event (e.g. co-present salvage contest, shared A.R.C. robot encounter), all Handlers must see consistent facts.
   - A shared event narrative is generated once on the server and distributed to all present Handlers. Individual raiders only receive private generative flavor for private inventory or personal status changes.

4. **Model and hosting choices are open:**
   - The designer decision of 2026-11-06 removed the offline-only, zero-cost, and no-API-key constraints. Cloud-hosted models and paid APIs are permitted options; see the [implementation ledger](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint).
   - **Current implementation:** Local inference uses `Microsoft.ML.OnnxRuntimeGenAI`; authored template narration is available immediately and remains the fallback when the local model is uninitialized, disabled, or fails validation. Neither local hosting nor zero-cost operation is a permanent requirement.

## Consequences to design for (not blockers)

- Tests, replay, and the balance harness currently rely on deterministic seeds; an AI decider would need a recorded or stubbed mode so measurements stay repeatable.
- Once-only settlement and idempotency require AI decisions to be recorded before they take effect.
- Cloud models add cost, latency variance, an API key/secret to manage, availability risk, and privacy considerations for any player data sent to a provider.
- AI-controlled participants should be clearly labelled. The existing rule against presenting saved opponents as live participants still applies.
- Balance measurements should keep AI-driven participants out of matched cohorts unless they are deliberately being measured.

## Pacing options (proposal, not approved)

No values or rules change here. Keep narrative presentation cadence separate from simulation timing:

### (a) Narrative-only cadence

These can be slowed without changing mechanical outcomes:

- **Beat cooldowns:** A presentation-only cooldown could be added around optional authored/AI beats. There is no dedicated cooldown today; committed beats originate in [RaidSimulation.cs](../server/AFKRaiders.Server/Domain/RaidSimulation.cs) and optional live narration is queued in [RaidCoordinator.cs](../server/AFKRaiders.Server/Domain/RaidCoordinator.cs) (`QueueLiveNarration`).
- **Hub downtime:** Flavor-only Hub activity uses `HubActivityInterval` in [RaidCoordinator.cs](../server/AFKRaiders.Server/Domain/RaidCoordinator.cs); `RecordHubActivities` records no gameplay effects. This does not include `HomeInterval`, which schedules autonomous Raider preparation and admission.
- **Delayed reactions:** Optional enrichment may arrive later through `QueueLiveNarration` and [OnnxGenAiNarrativeGenerator.cs](../server/AFKRaiders.Server/AI/OnnxGenAiNarrativeGenerator.cs). Delay only the narrative; retain immediate authored fallback and do not defer committed events.
- **Current AI bounds:** Live narration expires after 90 seconds in `QueueLiveNarration`; the 25-second model deadline is `InferenceBudget` in [OnnxGenAiNarrativeGenerator.cs](../server/AFKRaiders.Server/AI/OnnxGenAiNarrativeGenerator.cs). These govern optional flavor, not simulation outcomes.
- **Admission bound:** [RaidSimulation.cs](../server/AFKRaiders.Server/Domain/RaidSimulation.cs) (`TryQueueNarration`) allows only one pending live request per world. This is a backpressure invariant, not a timer; this proposal does not relax it.

### (b) Mechanical timings and BAL comparison windows

Do not change these as pacing tweaks. Any mechanical-timing change requires a new BAL-0 baseline; changing measurement windows also requires a new comparison baseline.

- **5-second robot rounds:** `RobotRoundInterval` in [RaidSimulation.cs](../server/AFKRaiders.Server/Domain/RaidSimulation.cs); danger profiles are in [RaidConditions.cs](../server/AFKRaiders.Server/Domain/RaidConditions.cs).
- **30-second admission pulse:** `AdmissionPulse` in [RaidCoordinator.cs](../server/AFKRaiders.Server/Domain/RaidCoordinator.cs).
- **1,320-second world expiry:** `RaidConditions.DeadlineSeconds` in [RaidConditions.cs](../server/AFKRaiders.Server/Domain/RaidConditions.cs).
- **900–1,200-second raid target:** experimental fixture bounds in [CommsAndPacingTests.cs](../tests/AFKRaiders.Server.Tests/CommsAndPacingTests.cs) and [G6RiskRewardTests.cs](../tests/AFKRaiders.Server.Tests/G6RiskRewardTests.cs), not approved balance.
- **8h/24h/48h/96h measurement windows:** `SettlementWindows` in [BalanceMeasurementTests.cs](../tests/AFKRaiders.Server.Tests/BalanceMeasurementTests.cs); changing them changes the comparison frame.
- **8h High age block and 24h readiness gate:** `BalanceExperimentRules.BlocksHigh` in [BalanceExperimentRules.cs](../server/AFKRaiders.Server/Domain/BalanceExperimentRules.cs), applied by `RaidCoordinator.IsBlockedFromHigh` in [RaidCoordinator.cs](../server/AFKRaiders.Server/Domain/RaidCoordinator.cs).

---

## 2. Technical Stack & Model Evaluation

| Component | Technology | Rationale |
|---|---|---|
| **Runtime Engine** | `Microsoft.ML.OnnxRuntimeGenAI` (or `DirectML` on Windows Server) | Native C# bindings for local token generation on CPU/GPU. |
| **Primary SLM Candidate** | **SmolLM2-360M-Instruct** (or 135M) (INT4/ONNX) | ~250MB–400MB memory footprint. Unmeasured. Earlier drafts claimed sub-50ms; this was never benchmarked. The only measured model is Phi-3.5 mini on CPU: 8-13s per short reply, 20-70s initialization (see the measured evidence above). Treat any smaller model as untested until benchmarked on the target hardware. |
| **Complex Generator Candidate** | **Qwen2.5-0.5B-Instruct** or **Phi-3.5-mini-Instruct** (INT4) | Capable of emitting reliable structured JSON for shared zone conditions, anomalies, and multi-bullet mission debriefs. |
| **Inter-Process Decoupling** | `System.Threading.Channels.Channel<NarrativeRequest>` | Producers (simulation events) enqueue requests without waiting; consumers (ONNX worker) pop, generate, and dispatch via SignalR. |

---

## 3. Four Core AI Gameplay Pipelines

### Pipeline A: Context-Aware Parody Comms (Event Skinning)
- **Trigger:** An authoritative simulation event occurs (e.g., `SalvageContestResolved`, `RobotCombatRound`, `RaiderDowned`).
- **Input Context:**
  ```json
  {
    "raiderName": "Unit-77",
    "event": "SalvageContestWon",
    "location": "Service Depot",
    "rival": "ScrapCollector",
    "itemWon": "Tactical Coffee Maker",
    "greedLevel": 45,
    "toneConstraint": "Deadpan tactical rationalization"
  }
  ```
- **Generated Comms Entry:**
  > *"Discovered rival operative ScrapCollector reaching for Locker A. Initiated aggressive negotiation protocol. Recovered tactical 12-cup brewing unit (+1 Morale). Greed +5."*

### Pipeline B: Shared Zone Conditions & Anomalies (Raid Initialization)
- **Trigger:** A new raid instance is created by `RaidCoordinator.cs`.
- **Frequency:** Once per 15–20 minute raid cycle.
- **Workflow:**
  1. The server draws base mechanical parameters: `BaseDanger: Medium`, `Zone: Subterranean Mall`, `WeatherModifier: Toxic`.
  2. The ONNX SLM expands these into an in-world parody condition description and ambient hazard name.
  3. The resulting `ZoneCondition` is saved to the raid instance so all joining Raiders experience identical environmental lore.

### Pipeline C: Post-Raid After-Action Report (AAR Debriefing)
- **Trigger:** A Raider completes extraction or settles terminal KNOCKED_OUT state in Desperanza.
- **Context:** Chronological highlights from the Raider’s journal (encounters, damage taken, top item secured, final greed).
- **Output:** A concise, 3-bullet deadpan debrief presented to the Handler in the Desperanza Hub.

### Pipeline D: Utility AI for Raider Personality (Non-LLM / Classical ONNX)
- **Concept:** A small multi-layer perceptron (MLP) trained via behavioral cloning or reinforcement learning and exported to an ONNX classification model (< 100 KB).
- **Execution:** Proposed to run synchronously inside `RaidSimulation.cs` during decision ticks (e.g. weighting autonomous branch selection or extraction appetite based on Raider traits); latency is unmeasured.
- **Historical economy note (2026-10-17; AI restriction superseded 2026-11-06):** The autonomous economy started with hand-authored deterministic utility scoring. The former requirement that any learned model remain deterministic for identical inputs is no longer a project rule; any future AI decision system remains subject to an approved design and authoritative validation/recording.

---

## 4. Architectural Interfaces & Design Contracts

To prevent tight coupling and keep test suites fast, the engine accesses all narration and generative features through clean interfaces:

```csharp
// Core domain contract for narration
public interface IRaidNarrator
{
    // Synchronous immediate fallback (template/catalog based)
    string FormatEvent(RaidEvent evt, RaiderProjection raider);

    // Asynchronous queue for AI-enriched generation
    ValueTask EnqueueEnrichmentAsync(NarrativeEnrichmentRequest request, CancellationToken ct = default);
}

public sealed record NarrativeEnrichmentRequest(
    Guid RaidId,
    string RaiderId,
    long EventSequence,
    string EventType,
    object EventPayload,
    Action<string> OnGenerated
);
```

### Fallback Guarantee
If the AI pipeline is disabled (e.g., during fast unit tests, low-memory environments, or model load failures), `IRaidNarrator` seamlessly uses the deterministic template engine. Automated tests never fail due to model weights or inference timeouts.

---

## 5. Phased Integration with the G-Series Roadmap

Rather than building an isolated AI engine before core gameplay mechanics exist, the AI implementation should be staggered to build on top of verified gameplay milestones:

```
[G1: Comms & Journal] ──► Establish IRaidNarrator, typed event schemas, Comms Log UI
        │
        ▼
[Phase 1: Async AI Spine] ──► Background worker, ONNX runtime spike, Post-Raid AAR
        │
        ▼
[G2: Catalog & Drops] ────► Structured item lore & dynamic parody item names
        │
        ▼
[G5: Survival & Meds] ────► High-stakes combat banter & terminal debriefings
        │
        ▼
[G6: Zone Conditions] ────► Curated shared conditions & danger-specific robots
                           Generative anomalies remain proposed
```

### Detailed Sequence:

1. **Step 1 (Part of G1): Narrator Contract & Template Baseline**
   - Define typed event models and the `IRaidNarrator` interface.
   - Implement the deterministic, template-based parody dictionary using approved content from `docs/lore/` and [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md).
   - Wire the Comms Log in `App.vue` to display diary and active threads.

2. **Step 2 (Immediate Post-G1 Spike): Async ONNX Worker Foundation**
   - Add optional `AFKRaiders.Server.AI` project or internal service referencing `Microsoft.ML.OnnxRuntimeGenAI`.
   - Implement `Channel<NarrativeEnrichmentRequest>` background worker.
   - Implement Post-Raid AAR debriefs (executed once at extraction/return, zero tick impact).

3. **AI-3 (Implemented with G4): Item Appraisal & Object Overvaluation**
   - Server catalog/classification facts plus optional validated subjective item mood; not generated mechanical items.

4. **AI-4 (Implemented with G5): Combat Banter & Terminal Debriefings**
   - Authored combat/support/medical receipts and factual terminal AAR with optional mood.

5. **G6 condition/robot content (implemented); generative expansion (proposed)**
   - Curated world-scoped condition profiles and original robot dialogue, never startup inference.
   - Generative anomalies/weather and learned utility decision-making remain future proposals.

The older draft labeled G6 conditions "Step 4", conflicting with the progress ledger's G5/AI-4 combat/debrief milestone. Feature evidence above is authoritative: the label did not imply generated mechanics were implemented or approved.

---

## 6. Verification & Quality Acceptance

- **Safety & Tone Guardrails:** Generated strings must pass lightweight regex filters ensuring compliance with [LEGAL_GUARDRAILS.md](lore/content-guidelines/LEGAL_GUARDRAILS.md) and [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md).
- **Zero Simulation Stall:** Acceptance target is a steady 250ms tick cadence under peak AI generation load; the available deterministic tests do not establish native-model tick performance.
- **Graceful Degradation:** Current bounded optional mood uses a 25s deadline and native session cancellation; template facts remain in place on timeout, rejected quality, missing model, stale callback, or queue saturation. More expressive fact-bearing generation remains proposed.
- **Test Suite Determinism:** All `dotnet test` runs execute against the template provider by default, guaranteeing deterministic and instant test verification.
