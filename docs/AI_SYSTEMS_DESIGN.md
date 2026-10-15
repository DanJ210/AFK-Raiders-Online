# AFK Raiders: AI & ONNX Systems Architecture Plan

Date: 2026-10-12  
Status: Architecture roadmap with bounded runtime status below (2026-10-14)  
Baseline alignment: [GAME_DESIGN.md](GAME_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md), [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md)

---

## Current bounded implementation (G6, 2026-10-14)

The runtime calls `IRaidNarrator.FormatEvent` on every committed event. It retains original receipts separately from immediate first-person canon fallback and queues optional live-event enrichment through the coordinator, outside domain mutation. Source-ID upserts propagate callbacks into private journals, snapshot polling, and active/recent threads. A shared event is queued once and remains visible only to actual participants. G6 implements curated, deterministic shared conditions and danger-specific robots. **Generative mechanical anomalies and learned utility AI remain unimplemented proposals**, not requirements on world startup. Residence/condition diary uses immediate authored pools, not extra inference load; voice rotation persists across admissions.

Normal experimental fixtures target 900–1200 seconds via three nonreward surveying/checking/rummaging activities per arrival. This is authorized-autopilot tuning, not approved balance; no repeated loot or additional enemy is created.

Local Phi-3.5 mini CPU evidence: initialization 70.04s; 31-token short response 13.42s cold / 8.29s warm (48-token output cap). A 1.5s complete-response target is therefore superseded by **25s** for bounded optional mood, including model-gate wait. The eight-request queue never blocks the simulation; queue-full requests explicitly retain fallback, live requests expire at 90s or world completion. One model gate serializes all Comms/AAR/appraisal access. Cancellation registers GenAI's native `terminate_session` option and awaits inference exit before releasing the gate; it does not merely cancel task scheduling.

Phi prompts use `<|system|>`, `<|user|>`, `<|assistant|>` and `<|end|>` tokens, `I consider` prefill, and short physical-object cues. `max_length` is encoded prompt length **plus 32 output tokens**, with early stopping at the first sentence ending. The prompt requests a complete 6-10-word dry opinion joke in a physical, not digital, wasteland; this requested length is guidance, not guaranteed model behavior. Conservative validation retains the single first-person sentence, 4–20-word, 8–140-character, punctuation/character, outcome/actor denylist, and unknown-proper-noun checks. This is not proof that arbitrary subjective prose contains no world claim: model input excludes rewards/health/outcome details, and factual receipts remain separate. AAR/appraisal facts remain deterministic and accepted mood is appended with a Generated label. Fallback AAR results retain `ArchivalLog`, not a false Generated source.

Verification: final G6 regression **157 server tests**, **5 frontend journal/lifecycle tests** and production build passed. Tests exercise the cancellation/serialization execution seam with a running cancellable backend, not installed native weights. Native prompt candidates accepted 0/6, then 1/6, then **6/6 distinct** outputs with the final prefill/cues (12.60-23.48s after 32.58s initialization). This small sample does not prove stable acceptance or funny prose. Browser G6 mechanics delivery is tested separately in template mode; it is not an installed-model two-client soak. Strict guard may reject useful prose; authored fallback remains the entertainment baseline and the human comedy gate remains open.

**Reliability follow-up (2026-10-14):** Live-event admission now permits one pending request per world. Other eligible events explicitly retain authored narration with `NotRequested / LiveBudgetBusy` and are not retried later. The eight-slot queue remains for accepted live requests and terminal debriefs; this is admission control, not more inference concurrency or a longer deadline. Live failure reasons propagate through source-ID journal upserts to expandable Authored/AI-enhanced badges: output rejection, model-gate deadline, native inference deadline, queue unavailable/full, worker/model errors and staleness. Existing historical entries lack this new field; their original reasons cannot be reconstructed from status alone.

The unchanged current prompt accepted 12/12 isolated samples (8 distinct) before admission changes; this does **not** establish why the user's historical 41 fallbacks occurred. Updated result handling accepted 6/6 distinct samples in 5.99-9.29s, init 20.22s, and native cancellation produced explicit `CallerCancelled` fallback in 0.19s. No model/guard/prompt budget was changed. Fixed-prompt repeats remain possible: deterministic decoding with a finite topic set is not limitless generated variety. The admission policy targets burst backlog/expiry, not guaranteed generation quality.

Initial changed-code native smoke (2026-10-13): init 59.19s; Market Surveying ambient took 25.18s and fell back to ArchivalLog (prompt too long, closed ~28-option grammar); 100ms cancellation returned fallback in 0.20s. Subsequent compact-prompt generation and live delivery succeeded, as recorded in the progress ledger. The latest rejection-tuning smoke accepted Checking and Surveying in 21.36s / 11.06s; Rummaging fell back in 18.59s with `MissingSentenceTerminator`. Rejection logs now distinguish validation reasons and explicitly label preview truncation, rather than implying that a 120-character preview is the complete model output. The unchanged guard never patches an unfinished sentence or loosens outcome checks. Fallback remains the entertainment baseline; smoke samples do not establish an acceptance-rate or latency guarantee.

The sections below retain future architectural intent; candidate latency numbers and generative world/fact examples are not current runtime guarantees.

---

## 1. Executive Summary & Design Philosophy

AFK Raiders Online is an autonomous extraction comedy RPG where the player acts as a remote **Handler** in Desperanza monitoring an autonomous **Raider** via the **Comms Log**. 

The emergence of efficient Small Language Models (SLMs) and ONNX Runtime provides an opportunity to deliver emergent, non-repetitive comedy, deadpan radio dialogue, unique shared zone anomalies, and post-raid After-Action Reports (AARs) without relying on expensive, high-latency cloud APIs.

### Core Architectural Invariants

1. **Mechanics First, Generative Narrative Second ("AI Skinning"):**
   - The server domain simulation (`RaidSimulation.cs`) remains 100% deterministic, authoritative, and testable.
   - Core gameplay numbers (damage, health deltas, item drop tiers, timestamps, participant status) are strictly decided by the C# engine using seeded rules.
   - AI models never decide whether a Raider lives, dies, or successfully loots. They interpret, narrate, and contextualize committed mechanical facts into parody dialogue conforming to [TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md).

2. **Strict Non-Blocking Simulation Ticks:**
   - The authoritative simulation tick loop (`RaidClock.cs`) must run in sub-millisecond cycles.
   - **Never execute AI inference synchronously inside `AdvanceTicks()`**.
   - Generative inference runs asynchronously in background worker services (`System.Threading.Channels` or `IHostedService`) and streams or broadcasts narrative updates via SignalR (`RaidHub.cs`).

3. **Deterministic Shared-World Consensus:**
   - When multiple Raiders experience a shared event (e.g. co-present salvage contest, shared A.R.C. robot encounter), all Handlers must see consistent facts.
   - A shared event narrative is generated once on the server and distributed to all present Handlers. Individual raiders only receive private generative flavor for private inventory or personal status changes.

4. **Offline & Zero-Cost Infrastructure (Local ONNX):**
   - Inference uses quantized on-device / in-process models via `Microsoft.ML.OnnxRuntimeGenAI`.
   - No external API keys (OpenAI, Anthropic, etc.) are mandatory for core gameplay; the server remains self-contained.
   - A deterministic, template-based narration engine acts as the primary fallback if an ONNX model is uninitialized, disabled, or fails validation.

---

## 2. Technical Stack & Model Evaluation

| Component | Technology | Rationale |
|---|---|---|
| **Runtime Engine** | `Microsoft.ML.OnnxRuntimeGenAI` (or `DirectML` on Windows Server) | Native C# bindings for high-throughput, low-latency token generation on CPU/GPU. |
| **Primary SLM Candidate** | **SmolLM2-360M-Instruct** (or 135M) (INT4/ONNX) | ~250MB–400MB memory footprint. Sub-50ms CPU generation for short 1-2 sentence radio diary lines. Extremely fast. |
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
- **Execution:** Runs synchronously in sub-millisecond time inside `RaidSimulation.cs` during decision ticks (e.g. weighting autonomous branch selection or extraction appetite based on Raider traits).

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
- **Zero Simulation Stall:** Benchmark tests ensure server tick rate remains steady at 250ms/tick under peak AI generation load.
- **Graceful Degradation:** Current bounded optional mood uses a 25s deadline and native session cancellation; template facts remain in place on timeout, rejected quality, missing model, stale callback, or queue saturation. More expressive fact-bearing generation remains proposed.
- **Test Suite Determinism:** All `dotnet test` runs execute against the template provider by default, guaranteeing deterministic and instant test verification.
