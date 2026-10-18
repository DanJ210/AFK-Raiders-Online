# AFK Raiders Online project instructions

## Read first

- [Product and architecture brief](../docs/MULTIPLAYER_REBUILD_BRIEF.md): confirmed direction, proposed mechanics, and milestone acceptance criteria.
- [Implementation progress](../docs/IMPLEMENTATION_PROGRESS.md): completed work, verification evidence, limitations, and next steps.
- [Game design](../docs/GAME_DESIGN.md): gameplay rules and pacing; consult the relevant sections for gameplay changes and surface conflicts with the active progress checkpoint or brief rather than silently choosing one.
- [AI systems design](../docs/AI_SYSTEMS_DESIGN.md): AI architecture and its own implementation phases/steps. Keep those distinct from G-series milestones and reconcile both against the progress checkpoint before planning AI work.
- [README](../README.md): current behavior, local commands, experimental rules, and content provenance.

Keep these roles distinct. Implemented prototype rules are not validated balance; proposed milestones are not approval to implement every mechanic.

## Content and lore reuse

- Before authoring zones, loot tables, Comms flavor, robots, items, or narrative content, inspect the relevant original AFK Raiders data. The read-only local reference is `C:\Users\DanJ2\source\repos\afk-raiders\src\content`; the online reference is [DanJ210/afk-raiders/src/content](https://github.com/DanJ210/afk-raiders/tree/main/src/content). Inspect only the content relevant to the active slice; do not rebuild it from scratch without checking existing material.
- Read the copied [lore index](../docs/lore/README.md), [legal guardrails](../docs/lore/content-guidelines/LEGAL_GUARDRAILS.md), and [tone guide](../docs/lore/content-guidelines/TONE_VOICE.md), then the relevant mapping/entities/backlog before reusing or extending parody. The local lore folder is the versioned AFK-original canon and writing reference, not proof that its content is implemented.
- Reuse suitable AFK-original names, flavor, entity concepts, and data selectively. Review legal/tone requirements and record the actual source revision and paths for imported runtime content in the README provenance section; a moving `main` link alone is not provenance.
- Adapt legacy data schemas and event conditions to the C# authoritative server. Shared loot is generated once per world/opportunity, and Comms describes actual committed events with owner-private effects. Do not import the legacy client ticker, saves, client-side ownership logic, or balance as approved online rules.
- Legacy lore references to `src/content/*.json`, TypeScript, suffixes, and content validation describe the offline layout. They do not prescribe an online runtime path or authorize dependencies. Keep reusable authored text separate from domain rules using the online project's approved content approach, and validate imported schemas, references, narration truth, and unchanged gameplay.
- Content availability does not authorize enabling every referenced mechanic, changing G-slice scope, or selecting persistence/account infrastructure.

## Product invariants

- This is an autonomous zero-player extraction RPG with social PvPvE, not a manually controlled shooter. Raiders progress and encounters resolve without Handler input.
- Human encounters involve actual Raiders in the same current server-owned raid, never saved opponent snapshots presented as live participants.
- Use one authoritative server timeline and shared world entities. Loot creation, transfer, consumption, and extraction must preserve unique ownership.
- Crates are autonomous event/contest opportunities, not reflex-based claim races. Handler commands, when implemented, remain limited and optional.
- The Raider runs its own economy (confirmed 2026-10-17, Godville-style): it sells, buys, equips, repairs, stashes, and departs on its own through a deterministic, trait-weighted, server-owned utility policy. Weapons come mainly from raid loot and sometimes from seeded, finite Desperanza trader stock. Do not add or extend Handler shop, sell, equip, repair, or loadout-staging controls; the existing Outfitter is transitional and slated for replacement.
- The Handler interacts only through spendable Signal actions. Signals may temporarily nudge scoring weights (greed, spending, caution) but never pick a specific item or bypass validation. An absent Handler loses nothing owed.
- AI models never decide Raider actions, purchases, prices, or outcomes. They may voice committed decisions with authored fallbacks. Any future hybrid that lets a model pick among pre-ranked legal options needs separate approval, must journal the pick for replay, and must fall back to the top deterministic choice.
- Disconnecting a Handler does not withdraw their Raider or erase unfavorable outcomes. Server outages require an explicit recovery policy, not browser-side replay.
- Preserve original parody identity. The sibling `afk-raiders` project is a read-only reference and selective content source, not an implementation dependency. Record source revisions for reused content; do not import its saves, local ticker, or balance by default.

## Engineering boundaries

- Backend: ASP.NET Core (.NET 10), C# domain rules, SignalR delivery. Frontend: Vue 3, TypeScript, Vite. Do not introduce a second client-side gameplay engine.
- Keep scheduling, transport, persistence, and presentation outside domain rules. Inject time and make randomness reproducible; process explicit deadlines in deterministic order.
- Serialize raid mutations through the coordinator. Repeated scheduling must not repeat rewards or settlement; future commands need authorization, validation, and deduplication. Autonomous trades are journaled events: each buy or sale commits exactly once, scarce trader stock is allocated in deterministic order, and coins or items are never duplicated.
- Clients render filtered projections and submit intent only. Countdown animation and polling must never advance simulation or determine ownership.
- Narration follows committed events. Never describe loot transfers, victories, or encounters that did not occur.
- For UI/API failures, trace the action through the Vue component, client request, Vite proxy target, and ASP.NET route/handler; verify which server/version the browser is reaching rather than inferring the cause from the displayed error. Use an isolated verification server when a shared live server is in use; do not restart or mutate it for browser checks.
- For Vue layout changes, verify populated states with representative short and long content, relevant equipment variants, and expanded/collapsed panels at the affected viewport; do not rely only on the default or empty state.
- Keep development identities and in-memory raid endpoints unavailable in Production. Durable accounts and recovery are prerequisites for external release.
- Preserve type safety and existing patterns. Surface errors explicitly; avoid silent fallbacks and unrelated refactors.

## AI narrative enrichment

- Local model narration is optional enrichment, never part of simulation authority or a reason to delay committed gameplay. Preserve authored narration as the immediate fallback and retain factual validation.
- Before changing prompts or acceptance thresholds, inspect the enrichment status and failure reason. Distinguish `NotRequested`, busy/queued, timeout, expired/stale, rejected, and accepted output; do not count authored or `NotRequested` entries as model rejections.
- Benchmark end-to-end latency and acceptance on the available target hardware before setting budgets. Keep backpressure and stale-event guards, report sample size and limitations, and check that authored fallbacks remain varied when enrichment is unavailable.

## Workflow and progress

- Treat the progress ledger's **Agent handoff checkpoint** and stable S-step checklists as the authoritative execution state. Read them at task start; do not infer progress from chat or session-local todos.
- Before code changes, record the active step, bounded scope, prerequisites, unresolved decisions, and next action. Mark implementation **In progress** when it begins, not during proposal-only planning.
- For unresolved gameplay choices, seek confirmation unless the user has authorized autonomous execution. Under that authorization, document assumptions as experimental before treating them as requirements.
- When asked for a copy/paste prompt to start the next stage, verify the current checkpoint and relevant design-document step numbers, then include the complete usable prompt in that response rather than only promising to provide one.
- When a user-raised gameplay idea is outside the active slice, record it in the progress ledger as explicitly deferred/future work with its relevant context; do not silently expand the current scope or let the idea disappear from the handoff.
- Add focused deterministic tests for changed behavior, including absent Handlers, shared consequences, ownership, authorization, timing ties, and retry/repeated-settlement cases where applicable.
- Update the progress ledger after each completed slice: status, implemented scope, verification date and evidence, limitations, and next step. Record pending or failing validation honestly; never mark a milestone complete on code presence alone.
- Before pausing or handing off unfinished work, record changed file links, the exact resume point, completed/pending checks, blockers, and the next action. On verified completion, check the stable step and acceptance items, clear active implementation, and advance the checkpoint to the next proposed step. Step completion must not imply completion of a broader milestone.
- Update the README when current behavior or local commands change. Update the brief when product direction or milestone scope changes. Do not use conversation history as the only handoff.
- Keep secrets, development access tokens, invite codes, and private player data out of documentation and commits.

## Validation commands

From the workspace root on Windows:

```powershell
dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj
Set-Location web
npm run build
```

Use the smallest applicable test selection; check two independent browser sessions for multiplayer changes. Follow the README for local startup. Documentation-only changes need link/content checks, not a gameplay build. Do not add tools or dependencies just to track progress.
