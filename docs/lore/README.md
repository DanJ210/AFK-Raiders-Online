# AFK Raiders Lore Wiki

This folder is the versioned source of truth for AFK Raiders parody canon. It is where we translate outside inspiration, extraction-shooter tropes, and player-facing comedy ideas into legally distinct AFK Raiders names, rules, tone, and content hooks.

Do not commit copied source-wiki prose, screenshots, logos, codex text, or exact source names here. Public lore pages can be used as intake, but this repo stores only original AFK Raiders canon.

## Online reuse context

This folder was copied from the legacy offline AFK Raiders project and remains the online project's AFK-original canon and writing reference. Read the [Legal Guardrails](content-guidelines/LEGAL_GUARDRAILS.md), [Tone Voice](content-guidelines/TONE_VOICE.md), and relevant entities/mappings before reusing or adding parody.

Existing game-facing data is available in the read-only sibling `C:\Users\DanJ2\source\repos\afk-raiders\src\content` or [the original repository's content folder](https://github.com/DanJ210/afk-raiders/tree/main/src/content). It includes zones, loot tables, Comms flavor, robots, event templates, and other content that should be inspected before writing replacements. Selectively reuse suitable original content and record its source revision/paths in the online README's provenance section.

The workflow and runtime-path references below describe the legacy layout. This online checkout does not yet have that complete runtime content system. Reusing lore/data does not mean importing the client simulation, approving legacy balance, or implementing every concept. Adapt content to authoritative C# events and unique shared-world loot ownership; the browser only presents authorized projections. Online storage/loading and validation follow the approved implementation slice rather than assuming `src/content/` or TypeScript is the new runtime location.

## Index

- [Parody Mapping](PARODY_MAPPING.md): canonical trope-to-AFK equivalent table.
- [World History](entities/world-history.md): unreliable AFK eras and old-world context hooks.
- [Desperanza](entities/desperanza.md): underground hub infrastructure and social texture.
- [Factions And Brands](entities/factions-and-brands.md): survivor groups and defunct old-world brands.
- [Traders](entities/traders.md): Desperanza vendor archetypes and future shop voices.
- [Robots](entities/robots.md): A.R.C. chassis roster, deadliness tiers, and content hooks.
- [Zones](entities/zones.md): original zone identities, condition hooks, and danger-level tone.
- [Field Phenomena](entities/field-phenomena.md): surface landmarks, oddities, and condition hooks.
- [Skills](entities/skills.md): autonomous Raider and Handler skill identities.
- [Legal Guardrails](content-guidelines/LEGAL_GUARDRAILS.md): required checks before adding content.
- [Tone Voice](content-guidelines/TONE_VOICE.md): deadpan comedy and writing patterns.
- [Backlog](BACKLOG.md): approved AFK-original lore/content ideas not yet converted into runtime JSON.

## Workflow

1. Read source material only as reference intake.
2. Reduce it to a general function, trope, or design need.
3. Create an original AFK Raiders equivalent in [PARODY_MAPPING.md](PARODY_MAPPING.md) or [BACKLOG.md](BACKLOG.md).
4. Add runtime text only after the concept is AFK-original and fits the tone guide.
5. Put game-facing text in `src/content/*.json`; keep engine logic text-free.

## Canon Boundaries

- AFK Raiders is an affectionate parody of extraction-shooter behavior, not a lore-compatible alternate universe for another game.
- A.R.C. means `Aggressively Roaming Chassis` in this project.
- `src/content/` is still the runtime source of comms lines, loot flavor, zones, robots, and event templates.
- This wiki explains why content exists and how to write more without drifting into copied source lore.