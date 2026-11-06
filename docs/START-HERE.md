# START HERE — AFK Raiders Online documentation index

AFK Raiders Online is the online, multiplayer rebuild of an offline zero-player autonomous parody game — a deadpan extraction-shooter parody. The Raider deploys, fights, trades, and extracts autonomously on an authoritative server; the Handler cannot control it directly and can only nudge it with spendable Signal actions.

Reference repo: https://github.com/DanJ210/AFK-Raiders-Online (branch main). Read docs/, docs/balance/ and docs/lore/ as needed.

## Read these first

| File | What it is | Link |
|---|---|---|
| [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md) | The progress ledger and handoff checkpoint; the source of truth for status, decisions, and open questions. It is very large — read the "Agent handoff checkpoint" table at the top first. | [GitHub](https://github.com/DanJ210/AFK-Raiders-Online/blob/main/docs/IMPLEMENTATION_PROGRESS.md) |
| [GAME_DESIGN.md](GAME_DESIGN.md) | The original offline game design document; the main-game design baseline (autonomous loop, Comms Log, loot, stash, survival, Handler interactions). | [GitHub](https://github.com/DanJ210/AFK-Raiders-Online/blob/main/docs/GAME_DESIGN.md) |
| [MULTIPLAYER_REBUILD_BRIEF.md](MULTIPLAYER_REBUILD_BRIEF.md) | The confirmed product direction and milestone brief for the server-authoritative shared-world rebuild. | [GitHub](https://github.com/DanJ210/AFK-Raiders-Online/blob/main/docs/MULTIPLAYER_REBUILD_BRIEF.md) |
| [AI_SYSTEMS_DESIGN.md](AI_SYSTEMS_DESIGN.md) | The AI/ONNX narration-enrichment architecture plan, with the bounded runtime status of local-model narration (AI voices committed events; it never decides outcomes). | [GitHub](https://github.com/DanJ210/AFK-Raiders-Online/blob/main/docs/AI_SYSTEMS_DESIGN.md) |
| [STORY_SYSTEM_PROPOSAL.md](STORY_SYSTEM_PROPOSAL.md) | **Proposal, awaiting approval:** shared moments, Raider memory, story direction, and narration architecture. | This workspace |

### docs/balance/

| File | What it is | Status |
|---|---|---|
| [BAL-0-baseline.md](balance/BAL-0-baseline.md) | Generated baseline measurements under the pre-tuning rules (`desperanza-bal-0-v1`). | Historical |
| [BAL-1-measurements.md](balance/BAL-1-measurements.md) | Generated measurements after BAL-1 "weapons matter" (`desperanza-bal-1-v1`). | Historical |
| [BAL-1S-measurements.md](balance/BAL-1S-measurements.md) | Generated 64-seed/96h measurements after BAL-1S shields and repair (`desperanza-bal-1s-v1`). | Historical |
| [BAL-1S-post-BAL-1-gate.md](balance/BAL-1S-post-BAL-1-gate.md) | Record of the BAL-1S prerequisite gate rerun after BAL-1, with documented departures. | Historical |
| [BAL-2-proposal.md](balance/BAL-2-proposal.md) | Approved BAL-2 robot-tier and defeat-loot proposal; preserved for reasoning and approval history, current values live elsewhere. | Historical |
| [BAL-2-measurements.md](balance/BAL-2-measurements.md) | Matched before/after BAL-2 measurement diagnostics. | Historical |
| [BAL-4-proposal.md](balance/BAL-4-proposal.md) | The BAL-4 tuning proposal and first-arm results; superseded in part by the measured follow-up arms. | Superseded |
| [BAL-4-arms.md](balance/BAL-4-arms.md) | The measured BAL-4 experiment arms 1–3 (one lever per arm), awaiting designer accept/hold decisions. | Current |

### docs/lore/

The lore wiki is the source of truth for parody canon and the legal guardrails.

| File | What it is |
|---|---|
| [README.md](lore/README.md) | Lore wiki index and online reuse context. |
| [PARODY_MAPPING.md](lore/PARODY_MAPPING.md) | Canonical trope-to-AFK-equivalent mapping table. |
| [BACKLOG.md](lore/BACKLOG.md) | AFK-original concepts tracked through lore intake and runtime seeding. |
| [content-guidelines/LEGAL_GUARDRAILS.md](lore/content-guidelines/LEGAL_GUARDRAILS.md) | Legal distinctness rules for all parody content. |
| [content-guidelines/TONE_VOICE.md](lore/content-guidelines/TONE_VOICE.md) | Deadpan tone and voice guide. |
| [entities/](lore/entities/) | desperanza.md, factions-and-brands.md, field-phenomena.md, robots.md, skills.md, traders.md, weapons.md, world-history.md, zones.md — per-topic AFK-original canon. |

## Raw links

Raw URLs are easier for tools and assistants to read than the normal GitHub pages. Pattern: `https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/<path>`

- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/IMPLEMENTATION_PROGRESS.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/GAME_DESIGN.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/MULTIPLAYER_REBUILD_BRIEF.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/AI_SYSTEMS_DESIGN.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-0-baseline.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-1-measurements.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-1S-measurements.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-1S-post-BAL-1-gate.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-2-proposal.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-2-measurements.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-4-proposal.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/balance/BAL-4-arms.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/README.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/PARODY_MAPPING.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/BACKLOG.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/content-guidelines/LEGAL_GUARDRAILS.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/content-guidelines/TONE_VOICE.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/desperanza.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/factions-and-brands.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/field-phenomena.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/robots.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/skills.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/traders.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/weapons.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/world-history.md
- https://raw.githubusercontent.com/DanJ210/AFK-Raiders-Online/main/docs/lore/entities/zones.md

## Ground rules for any assistant

- The ledger's handoff checkpoint is the authority on status; do not trust older chat summaries.
- Check the ledger's "Last updated" date and the latest commit before advising, and say if something might be stale.
- Frozen contracts are listed in the ledger and balance proposals (for example trader-only Temptrest, once-per-raid weapon wear, deterministic ownership, once-only settlement, no Handler economy controls, no PvP/theft, no AI authority over outcomes). Do not suggest changing them without flagging it.
- Balance values are experimental until a human approves them.
- Follow docs/lore/content-guidelines (use "A.R.C." with periods, never raw "ARC"; original AFK names only; deadpan tone). Shield names are placeholders pending approval.
- Do not commit, push or restart user-managed services unless asked.

## Current status

See the Agent handoff checkpoint at the top of [IMPLEMENTATION_PROGRESS.md](IMPLEMENTATION_PROGRESS.md). Status, step progress, and balance numbers are not copied here so this file does not go stale.

## Starter message for a new chat

```text
Reference repo: https://github.com/DanJ210/AFK-Raiders-Online (main). Start with docs/START-HERE.md, then read the handoff checkpoint in docs/IMPLEMENTATION_PROGRESS.md. Use docs/balance/ and docs/lore/ as needed. Tell me if anything looks stale.
```

## Keeping this file current

Update START-HERE.md only when files are added, renamed, or retired in docs/, docs/balance/ or docs/lore/. List the file names here, not their contents.
