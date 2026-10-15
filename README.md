# AFK Raiders Online

## Product direction

AFK Raiders Online is an autonomous zero-player RPG inspired by extraction shooters, expanding the [original solo concept](https://github.com/DanJ210/afk-raiders) with live social integration.

You are the Handler, not the Raider. The intended game plays itself: Raiders carry out their activities and resolve encounters without requiring the Handler to move, aim, click first, or respond to every event. Social integration adds encounters between real, human-owned Raiders within the same server-owned raid; it does not replace autonomy with mandatory live participation.

Raiders act autonomously; encounters are resolved by server-owned rules rather than Handler click speed. Experimental encounter factors and broader Handler intervention rules are not validated or approved beyond the bounded slice below.

**Confirmed product direction:** Raiders reside in **Desperanza** between raids, autonomously join ongoing shared raids, organically meet other actual Raiders, return home, and repeat without invites or a Handler redeploying them. The Handler monitors comms and may influence selected aspects without mandatory moment-to-moment input.

**Main-game design baseline (clarified 2026-10-12):** The full [original game design](docs/GAME_DESIGN.md) remains the intended game, not just social-encounter inspiration. The Handler sits in Desperanza and observes every Raider gameplay action, encounter, and outcome through a parody Comms Log with diary and active-thread presentation. Loot tables/value, home stash/economy, preparation, survival, progression, and Handler interactions remain design scope. The server rebuild enables that game and real social encounters; it does not import the old client simulation or saves. Legacy "current implementation" descriptions are not claims about this checkout, and exact tuning remains unvalidated.

**Current implementation boundary:** G1-G6 extend the S5 loop as Development-only, in-memory slices for two hard-coded development identities. Its admission cadence, capacity, and home interval are experiment settings for this slice only—not public matchmaking policy or validated balance. There are no accounts, persistent home assets, restart recovery, or larger-population fairness safeguards.

See the [multiplayer rebuild brief](docs/MULTIPLAYER_REBUILD_BRIEF.md) for the product contract, proposed rules, architecture, and milestones.

## Implementation handoff

- [Implementation progress](docs/IMPLEMENTATION_PROGRESS.md): authoritative agent handoff checkpoint, marked step checklists, milestone status, verification evidence, and next actions.
- [Main-game coverage map](docs/IMPLEMENTATION_PROGRESS.md#main-game-design-coverage-map-2026-10-12): implemented/partial/missing original systems, multiplayer adaptations, and proposed G1-G7 delivery beginning with complete Comms and the loot/value/home loop. This is a roadmap, not new runtime behavior.
- [Copilot instructions](.github/copilot-instructions.md): project-wide implementation guardrails and progress-maintenance workflow.

Read these alongside this README before extending the game. The progress ledger distinguishes completed technical work from unimplemented proposals and unvalidated balance.

## Current implementation: Desperanza duty and shared autonomous raids

This is a clean server-owned implementation. It does not load prototype saves or run the prototype's browser simulation. In Development, each of the two development identities can enroll once for autonomous duty. The first enrollment starts a solo raid immediately; another eligible Raider can join the ongoing world on an admission pulse. Each Raider returns to Desperanza independently, and the world remains open while any admitted Raider is still active.

The Handler console shows home/on-duty, cooling-down, paused, and in-raid status; current raid activity, shared comms/receipts, and private health, backpack, and extracted results. Pausing stops future admissions only. Existing journeys, searches, the salvage encounter, robot combat, and extraction continue if the Handler closes the browser or does not respond. There is no invite or explicit deployment step in this S5 flow.

## Run locally

1. Start the server in Development mode:

   ```powershell
   dotnet run --project server\AFKRaiders.Server
   ```

2. In a second terminal, start the browser client:

   ```powershell
   cd web
   npm install
   npm run dev
   ```

3. Open the Vite URL, select a development identity, and enroll it for autonomous duty. To verify co-op, open a second independent browser session, select the other development identity, and enroll it while the world is active; admission occurs on the next pulse if capacity remains.

Each browser stores its access token per development identity so it can reconnect while the in-memory server state remains alive; enrollment retries reuse a request ID. A server restart erases that state and invalidates access. If the page reports an unavailable or outdated server, restart the server from this checkout and reload the browser. The Vite API proxy defaults to `http://127.0.0.1:5029`; set `VITE_API_PROXY_TARGET` in the environment before starting Vite to target a different local server.

Development identities, enrollment, raid creation, and the in-memory raid service are available only in the ASP.NET Core Development environment. Do not expose this server as a production deployment.

## G6: autonomous risk, residence and shared conditions (2026-10-14)

Current rules version: **`desperanza-g6-v1`**. Mechanics run on the server without inference or Handler input.

- One world-scoped zone/condition/danger profile: Light Fog (Low), Apology Weather (Medium), or Overtime Red (High). Greed carried by eligible Raiders affects the next world's condition pool, not individual weather copies. Forecasts precede environmental impacts by 10 seconds; only actual living, nontraveling occupants at impact time are affected.
- Greed grows while staying and searching; mood changes with damage/healing/support. Positive mood trims up to 5 raw damage before shields. Calm lowers greed by 12 and raises mood by 3; Pressure raises greed by 10 and lowers mood by 2. These change extraction willingness, valuable-item appetite and resilience, not just dialogue.
- Natural extraction rolls every 30 seconds, ordinarily after 900/960/1020 seconds on Low/Medium/High. Distress and deadline urgency can bypass the ordinary guard. Raiders can still ignore urgency and fail. The world closes at 1320 seconds; admission stops after 300 seconds. CallExtract interrupts unfinished activities/search without inventing rewards, follows connected paths and finishes existing travel before rerouting.
- **Long stays are retained:** three 60-65-second nonreward activities at each arrival, including Market revisits, with diary beats every 20 seconds. At the exit, an unready Raider stays and writes diary entries rather than magically finding more loot. Actual co-present Raiders may be mentioned; fictional visitors are not generated. Movement still follows the connected seeded itinerary; random wandering/revisits are explicitly deferred, not forgotten.
- Elapsed world exposure and danger increase both environmental risk and the weighted chance of valuable items from still-finite shared opportunities. Catalog prices and item counts do not inflate. Robots vary by danger but remain one shared state per world. Early extraction or knockout can be much shorter than the experimental 15-20-minute ordinary target.
- Authored medical, robot, condition and ambient pools rotate without replacement; per-Handler voice continuity survives returns. Shared asides are identical for actual recipients. Phi now uses compact object cues and an `I consider` prefill to reduce fragment/opening rejections, without weakening factual guards.

**Verification:** 157 server tests, 5 frontend journal/lifecycle tests and production TypeScript/Vite build passed. Frozen seed/policy audits and full accounting are in the [G6 ledger](docs/IMPLEMENTATION_PROGRESS.md#g6-autonomous-riskreward-conditions-and-broader-parody-content). Low starter unattended extracted 100%; Medium improved from 88.33% to 97.08% with the fixed attended policy. **High remains severe experimental tuning:** starter unattended extracted only 1.67%, attended 53.33%; staged High unattended mean net was -13.49. Sparse ordinary High successes do not establish robust pacing. Passing the frozen slice thresholds is not general balance approval.

The final six-output native Phi smoke accepted six distinct sentences in 12.60-23.48 seconds after 32.58 seconds initialization; earlier candidates accepted 0/6 and 1/6. This is a small local sample, not a promised acceptance rate, latency or comedy quality. Authored fallback remains essential. **The human comedy/pacing playtest gate is still open.**

Journal badges show **Authored** or **AI-enhanced**. Expand the badge for exact source/status, an explanation and any diagnostic reason. `NotRequested` means no optional AI request was made, not a rejected output. By default, only activity starts, searches, robot rounds, travel and extraction starts are candidates for live enrichment; frequent residence diaries and other receipts intentionally remain authored. C4 makes this reviewed candidate list explicit in the [narration catalog](server/AFKRaiders.Server/Content/narration-catalog.json); diary/downed/knockout candidates are supported but not enabled. At most one live request per world may be pending. Events arriving while it is busy remain authored with `LiveBudgetBusy`, without later retrying an obsolete event. This intentionally trades enrichment coverage for freshness and bounded CPU demand; it is not counted as successful generation.

New failed live requests distinguish `Rejected:<validation reason>`, `InferenceDeadlineExceeded`, `ModelGateDeadlineExceeded`, `QueueFull`, model/worker errors and stale callbacks. Historical entries cannot acquire reasons that the old server did not retain. The total native deadline remains 25 seconds including model-gate wait; factual guards are unchanged. Model generation can still fail under load or contend with AAR/appraisal requests.

**Narration follow-up verification:** 157/157 server regressions passed (excluding the unchanged, previously verified eight-hour mechanics audit), 7/7 frontend tests and production build passed. Installed-model smoke accepted 6/6 distinct asides in 5.99-9.29s; native cancellation returned an explicit reason in 0.19s. An isolated live-model browser verified actual AI-enhanced/Ready journal delivery and expanded details. These samples do not prove the cause of the historical 41 failures or guarantee future acceptance. Restart your development backend to load the changes; this clears its existing in-memory sessions/results.

Reviewed original content at `f62d088a41da951a1fbe9ed7215d2957a933ce50`: `src/content/zones/zone_conditions.json`, `zones/zones.json`, `robots.json`, `healing_items.json`, `raiding-events/ambient_events.json`, and `robot_encounter_activities.json`. Adapted themes/names under the tone/legal rules; no legacy simulation, saves or balance imported.

Ready Up remains explicitly deferred pending a recurring-admission cost/timing decision. G7 progression, persistence/authentication, public matchmaking and direct PvP/theft remain outside G6.

**Load these changes:** the existing user server was not stopped or restarted. Stop it yourself with Ctrl+C in its terminal, then restart:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:AFK_AI_MODEL_PATH = 'C:\Users\DanJ2\Models\Phi-3.5-mini-onnx'
dotnet run --project server\AFKRaiders.Server
```

Reload and re-enroll after startup; check for `desperanza-g6-v1`. Restart clears in-memory duty, inventory and results. Model initialization may take a minute; authored gameplay/narration does not wait on inference.

### Historical pre-G6 Comms and area residence (2026-10-13)

The following records the earlier repair; the G6 section above supersedes its condition/risk boundary and prompt wording.

- **Experimental normal target: 15–20 minutes**, not validated balance. Every arrival at Market (including the return visit), Salvage Yard, Service Depot, and Extraction Platform includes three server-deadlined nonreward activities after any finite search/combat: surveying approach/exit markings, checking footing/straps/cover, and rummaging empty fittings for route clues. Each takes 60–65 seeded seconds. Travel, finite opportunity cutoffs, robot rounds, and extraction retain their existing timings. Emergency extraction and knockout can finish earlier.
- Activities have real start/completion records and active threads; they do **not** mint loot, reroll opportunities, modify stats, add enemies, or enable G6 conditions. The old fast demonstration pacing is now an explicit **test-only constructor fixture**, never the runtime default.
- Every committed event passes through the immediate canon fallback narrator. Its original receipt is retained in `factsText`; first-person Raider commentary is supplementary. The diary and active-thread logs show the same authoritative activity. Optional generated mood is labeled separately, never allowed to rewrite inventory, rewards, health, encounters, or outcomes.
- Available local ONNX inference enriches selected live Comms once per source event. A shared event receives one shared aside; private events retain their recipient filter. Journals and the browser **upsert** updates by source ID without duplicates. Full snapshots carry updates to existing entries; the paged journal's sequence cursor is for append/history reads.
- Provenance is explicit: `CanonFallback` for immediate narration, `Generated` only for accepted mood; enrichment status is `NotRequested`, `Pending`, `Ready`, `Fallback`, or `Expired`. AAR/appraisal fallback stays `ArchivalLog`. Model failure or queue saturation does not interrupt play.
- Phi uses its instruction chat tokens, a **32 output-token** budget added to the encoded prompt length, one serialized model gate, and a **25-second total inference deadline** including gate wait. Native `terminate_session` cancellation interrupts started inference rather than abandoning a `Task.Run`. The eight-request nonblocking queue declines overflow with explicit fallback; live requests older than 90 seconds or for completed worlds are discarded. All fact-bearing AAR/appraisal text remains deterministic; the model may append only validated subjective mood.
- Local Phi-3.5 CPU prerequisite benchmark supplied on 2026-10-13: initialization **70.04s**, cold short output **13.42s**, warm **8.29s**. The old 1.5-second target was not feasible. The benchmark also produced quote/voice artifacts; the implementation therefore accepts only a strict first-person subjective sentence guard and falls back on all other output. Initialization availability is not proof of useful generation.
- Initial changed-code native smoke (2026-10-13): init 59.19s; Market Surveying ambient took 25.18s and fell back to ArchivalLog (prompt too long, closed ~28-option grammar); 100ms cancellation returned fallback in 0.20s. The current compact Phi prompt uses first-person "I" prefill, a 32 output-token cap, early stop at first sentence end, and an open but strict subjective guard (single first-person sentence, 4-20 words, letters/basic punctuation only, denylist of loot/outcome/combat/actor/third-person words, no unknown proper nouns). Subsequent native Generated success and live delivery are recorded in the progress ledger; authored fallback remains the baseline.
- Verification: **128 server tests passed** (after the ambient fix; previously 110) (including six normal seeded solo/shared duration fixtures, finite ownership/repeated ticks, narration wiring, private/shared callback delivery, stale/invalid callback rejection, bounded cancellation and fallback provenance); browser journal merge test passed; Vue/TypeScript + Vite build passed. New live-server/native cancellation/browser observation remains a separate smoke check, not claimed by deterministic tests.
- Follow-up parody pass: six distinct opening lines per residence activity, four distinct completion lines, and expanded travel/search/item/combat/extraction/disposition pools. Stable mixed-sequence selection avoids the old short-pool modulo cadence. Local SLM prompts receive compact activity-specific, sequence-varied metaphor themes instead of only a generic mood instruction. These are curated C# content pools, matching the existing bounded server content approach; importing the legacy JSON runtime remains outside this repair.
- Live lifecycle repair: intentional end-of-raid, session cleanup, and raid-switch stops release connection ownership before stopping. Stale callbacks cannot change a replacement connection's UI; successful recovery clears only the live-close warning. Genuine unexpected closure still reports the warning and snapshot polling continues.
- Follow-up verification: **133 full-suite server tests**, **5 frontend journal/lifecycle tests**, Vue/TypeScript + Vite build, and editor diagnostics passed. Cancellation tests now synchronize with actual inference start and separately check queued deadline expiry, avoiding the previous assumption that inference starts within 50ms on a shared machine. An isolated, mocked-transport browser check verified Connected live -> At home - polling with zero false close warnings and rendered the zipper joke in the journal. Actual local Phi generated "I'm zipping through the legal landscape, unzipping loopholes with a grin." Other smoke samples timed out or failed the guard; authored parody remains the dependable baseline, not a guarantee of generated comedy.
- Rejection tuning: prompt requests a complete 6–12-word dry first-person complaint, opinions rather than actions/discoveries, grounded in the physical wasteland. The 32-token cap gives completion headroom; it does not guarantee adherence to the requested length. Guard limits remain unchanged. Rejection warnings include the reason, raw character count, and whether the 120-character preview was truncated; unfinished outputs are never completed artificially. Final three-sample native smoke accepted Checking and Surveying (21.36s / 11.06s), while Rummaging safely fell back with `MissingSentenceTerminator` (18.59s). This is smoke evidence, not a measured acceptance-rate guarantee.

Existing running servers are **not** hot-patched by a build. To use these changes, in the terminal owning the old server press **Ctrl+C** yourself (restart clears in-memory duty, credentials and stash), then from this workspace:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:AFK_AI_MODEL_PATH = "C:\Users\DanJ2\Models\Phi-3.5-mini-onnx"
dotnet run --project server\AFKRaiders.Server\AFKRaiders.Server.csproj
```

Allow roughly a minute or more for model initialization, check `/api/dev/status`, reload the browser, and enroll afresh. No existing user process was stopped by this repair.

Lock-safe verification while a server is running:

```powershell
dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj --no-restore -p:OutputPath=bin\Verification\ -p:UseAppHost=false
Set-Location web
node --experimental-strip-types --test tests\journal.test.mjs
npm run build
```

## Historical S5 behavior and experimental rules

This is the networking/journey baseline, not a current G6 mechanics specification. G2-G6 supersede its fixed loot/robot, health, support and extraction descriptions.

- One server clock runs independently of requests and browser connections. A 250ms scheduler processes explicit deadlines and a 30-second autonomous-admission pulse.
- Enrolling for duty is a one-time opt-in, enabled by default for the two development slots. The first eligible Raider starts a solo world immediately. Each world admits at most two actual Raiders. If no other eligible Raider is available, a Raider proceeds alone; no synthetic participant is created.
- Each Raider is eligible to re-enter after a 60-second home interval. Pausing duty affects only future admissions; it does not interrupt an active raid or return an already-admitted Raider early. Closing a browser does not pause duty or withdraw the Raider.
- At each admission pulse, due world deadlines are advanced first, returned Raiders are refreshed, and a world closes only after all admitted Raiders settle; eligible Raiders are then admitted to the ongoing world or a new one. The server uses stable deadline/identity ordering, and repeated scheduling does not settle the same outcome twice.
- The web client now checks the Development API's `desperanza-g6-v1` rules version before enabling enrollment. Enrollment is idempotent for a repeated request ID. Vite may select another local port when its default is occupied; Development CORS is limited to loopback origins.
- The graph connects Entry Tunnel to Market; Market to Salvage Yard, Service Depot, and Extraction Platform; and each branch to Extraction Platform. Each Raider chooses a seed-derived first branch, visits Market -> first branch -> Market -> other branch -> Extraction Platform, and extracts. Edge travel takes 8-12 seed-derived seconds. This is a bounded route policy, not free exploration or adaptive risk-based routing.
- Market and Salvage Yard have one finite opportunity with two unique items each. Service Depot has two more unique items, but the locker is accessible only after defeating its robot; if the robot escapes, both items are lost for that raid. Depot salvage uses new instances of two already-reviewed scrap catalog entries.
- The first arrival starts an opportunity's shared search window: ten seconds for Market/Depot, twelve for the humming crate. Later eligible arrivals through the exact closing instant join without resetting the deadline. Traveling Raiders are not present at their origin for encounter eligibility. Once searching, a participant remains until resolution; no separate pending search continues or awards later.
- One Roomba Prime guards the Service Depot. Its single server-owned 70-HP state becomes active when the first actual Raider arrives. Every five seconds, each actual engaged Raider deals 20 damage in stable development identity order; any surviving robot deals 10 damage to each fighter. Raiders start with 40 health. A maximum of three rounds prevents a full-health Raider from being knocked out by this slice.
- Raiders who arrive while the robot is active join its next due round. Raiders in transit, elsewhere, or already extracting do not participate. Defeating it unlocks the existing finite Depot search for actual Raiders present; no duplicate robot loot is minted. If it survives three rounds, it escapes and the Depot items are forfeited; the Raider continues its route with carried loot. Solo Raiders can still complete the rest of the journey, but cannot defeat the robot alone under these experimental values.
- At each due timestamp, all travel arrivals are admitted first, then a due robot round resolves, then due searches, recovery/extraction, and ready-Raider scheduling. Raiders use stable development identity order and opportunities use stable opportunity ID order within each phase. Browser packet order and scheduler frequency never decide a tie, and robot rounds do not pause unrelated Raider deadlines.
- Arrival after an opportunity resolves yields an explicit exhausted-opportunity event, no item, and immediate continuation. Revisiting Market as transit does not start another search.
- Each Raider receives a seed-derived Cautious or Hoarder disposition, greed (15 or 35), salvage aptitude (4-8), and initiative (0-5). Names/callsigns come from the original parody content. These are demonstration profiles, not a progression or loadout system.
- Only actual eligible co-present humming-crate search participants enter its encounter. A single participant searches alone; no opponent is invented. Two cautious participants cooperate over the crate's two items. Lower backpack value receives the higher-value item, then the other receives the second; equal backpack values use stable development identity order. Market and Depot searches use the same allocation without contests.
- If either Raider is a hoarder, both contest. Score = greed + aptitude * 3 + initiative - backpack value / 5 (integer division). Highest score receives both items; exact score ties use stable development identity order, never request arrival. The receipt exposes the factors and actual allocations to both participants.
- Robot damage persists in each Raider's private health projection. Robot encounter receipts show only actual participants and their damage. Cooperation can defeat the robot sooner and unlock the locker; solo exposure can cost health and let the robot escape. Health is not yet used for incapacitation, healing, or later encounters. Cooperation, ordinary searches, and solo searching take two seconds to pack; contesting takes eight seconds to recover. Each Raider then continues its own remaining route and extracts for ten seconds.
- Loot has unique server-owned instance IDs. Each Raider independently moves items out of its backpack into an in-memory extracted result exactly once. Its Handler sees that result while other Raiders are still active. The raid completes only when all admitted Raiders have extracted; this is not persistent account inventory.
- SignalR sends revision notifications, and clients fetch filtered projections. Three-second snapshot polling covers missed notifications; countdowns are display-only estimates of server time. Own health, backpacks, and extracted loot are private; robot health is shared, while detailed robot receipts are visible only to actual participants.
- Access is stored per identity in browser local storage; enrollment request IDs are session-scoped for retry safety. The server keeps identity, duty, worlds, credentials, and results only in memory. There is no persistence, restart recovery, or offline replay.
- The console shows home/in-raid status, current activity and countdown, comms and filtered shared encounter receipts, and the Handler's private health/loot/result. A shared encounter names only actual participants in the same authoritative world. Shared robot/opportunity state does not expose another Raider's private inventory.
- No direct PvP combat, theft, target selection, pair cooldown, or broader repeat-target fairness system is included. Two development identities and the capacity limit bound this test; they do not validate fairness in a public population. Handler influence is limited to monitoring and pausing future admissions.

The `desperanza-duty-v1` admission settings and the existing `journeys-robot-v1`/`salvage-v1` journey, robot, and salvage rules are experimental slice settings, not validated balance or approval of broader mechanics. No new sibling-repository content was reused for S5.

### S5 verification snapshot (2026-10-12)

- `dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj --no-restore --verbosity minimal`: **30 passed, 0 failed**. An isolated output path was used because the normal output was locked by an existing server process.
- `npm run build` from `web`: Vue/TypeScript checks and the Vite production build passed.
- Two independent browser clients enrolled as the two development identities and showed the same live world and revision, same actual participants, and identical shared Salvage Yard contest and Service Depot robot receipts. The browser projections showed different private inventories consistent with the shared allocation. A separate observed cycle showed Raiders returning home independently and the world closing after the final return.
- The browser check is manual, not a committed end-to-end suite. It did not verify public matching, restart recovery, repeat-pair fairness, or a production identity lifecycle. Server tests cover enrollment retries, admission pulse ordering/repetition, home interval and re-entry, pause/resume, offline progression/settlement, authorization, late actual admission, private results, and unique shared-reward ownership.
- This is a bounded S5 Development slice, not completion of M2, M3, or the social PvP loop. No accounts, persistence, public pool, direct PvP combat, theft, target-selection/fairness controls, or Handler encounter commands are present. Server restart erases the whole slice.

**Next proposed step:** define S6's bounded durable identity, inventory settlement, reconnection, and restart-recovery scope before adding production account or recovery code. The S5 experimental admission rules do not approve broader population, PvP, fairness, or Handler-intervention mechanics.

## Content provenance

The curated names/callsigns, scrap catalog items, values, and flavor lines, and search-opening lines were reviewed from the original repository at commit `f62d088a41da951a1fbe9ed7215d2957a933ce50`; the inspected source files had no working-tree changes:

- `src/content/raider_identity.json`
- `src/content/loot-tables/scrap_components.json` (eight existing scrap items plus C2's `battery_half_full`, selectively adapted into the server catalog)
- `src/content/loot-tables/personal_junk.json` (`single_sock`, selectively adapted into the server catalog)
- `src/content/loot-tables/arc_tech.json` (`arc_data_shard_corrupted`, selectively adapted into the server catalog)
- `src/content/raiding-events/search_activities.json`
- `src/content/robots.json` (Roomba Prime name and one warning-tone flavor line only)
- `src/content/flavor.json` (pre-G6 review of hoarder justification, mood, extraction, and Handler-distance patterns, selectively adapted in `RaiderCommsContent.cs`; no reward-bearing or unverifiable gossip lines imported). New surveying/checking/rummaging commentary follows the copied lore tone, legal guardrails, mapping, and zone themes at the same original revision.
- `src/content/raiding-events/ambient_events.json` (follow-up adaptations of zipper legal standing, clipboard bias, machine performance reviews, timer small talk, optional panic, and dignity paperwork). Search/activity and flavor themes inform the expanded authored pools and short SLM style hints; factual discoveries, named encounters, and reward claims are not imported into nonreward residence beats.

Only selected original content is bundled in the server slice. No prototype saves, engine code, or balance are imported. The robot's health/damage and its use of the existing Depot locker are local experimental rules. New narration follows committed server events.

For the C2 intake, the sibling repository was at the same recorded commit above. The three loot-table files listed for C2 were clean; the only sibling worktree difference was the unrelated untracked `docs/MULTIPLAYER_REBUILD_BRIEF.md`, which was not used or changed.

C3 imports no additional item definitions or flavor. The existing `scrap_metal_twisted` entry now participates in weighted selection; its shared weight of 25 was checked against `src/content/loot-tables/scrap_components.json` at the same revision above, with that source path clean. The other shared weights retain their C2 values. These are experimental online distribution settings, not approval of legacy balance.

C4 relocates existing online authored text without importing new lines. Its source correspondence review used the same original revision above and these clean paths: `src/content/downed_events.json`, `src/content/knocked_out_events.json`, `src/content/raiding-events/robot_encounter_activities.json`, `src/content/raiding-events/ambient_events.json`, `src/content/raiding-events/search_activities.json`, and `src/content/flavor.json`. This records the review revision, not proof that every earlier online adaptation was copied verbatim from that revision. The legacy robot-progress pool is preserved but remains unused by authoritative robot resolution; moving it to JSON does not invent combat events or drops.

### Editing bundled catalogs

- [Loot and opportunities](server/AFKRaiders.Server/Content/loot-catalog.json) use schema version 3 and content version `desperanza-shared-loot-v3`. The catalog retains the ordered S5 fixed fixture; weighted generation uses one shared catalog-level `pool`, not duplicated per-opportunity item lists.
- [Weapons and supplies](server/AFKRaiders.Server/Content/shop-catalog.json) contain the existing ordered Outfitter definitions. The zero-price `tea_kettle` remains standard issue; staging limits and all resolution rules remain C#.
- The loot file's `eligibilityCategories` vocabulary is `Residential`, `Salvage`, `Robot`, and `Technology`. Each opportunity declares `eligibleCategories` and a finite `drawCount`. The shared pool defines each candidate's `catalogId` and `dropWeight` once; both generators filter it by category intersection, retaining shared-pool order. Eligibility, drop weight, item rarity, and value are distinct fields. Supply behavior still uses the separate `Medical` and `ShieldRecharger` categories.
- Experimental category mappings retained from C2: Market (`market-locker`) allows Residential/Salvage; Humming Crate (`humming-crate`) allows Salvage/Technology; the already-existing robot-gated Service Depot locker (`depot-locker`) allows Robot/Technology. Location is placement only, never a loot-selection conditional. The same opportunity configuration can describe a locker, crate or activity without its own item list. An item matching multiple allowed categories remains a single candidate. Robot defeat only makes the existing locker accessible; it does not grant an additional robot drop.
- To add reviewed loot, add its catalog definition with eligibility categories and one shared pool entry. It is then eligible in every matching opportunity without editing those opportunities. C3 deliberately broadens Market/Crate pools to all matching shared entries, including twisted scrap; the depot retains its seven eligible items, though seeded outputs can change with shared ordering. Do not treat the new distributions as balance approval.
- Catalog fields are case-sensitive camelCase and required. Schema 1/2 and per-opportunity `pool` fields are rejected rather than migrated silently. Unknown/duplicate properties or categories, duplicate IDs/shared-pool entries, missing references, empty category-filtered pools, invalid draw counts, and non-positive/overflowing drop weights fail startup with catalog context. Invalid tables do not fall back to fixed loot.
- All catalog files are embedded into the server assembly at build time. Edit source JSON, validate, then rebuild and redeploy. There is no file watching, hot reload, client-supplied content or community-package loader. Published execution does not depend on the working directory or loose JSON files.
- Array order is significant for deterministic loot selection and presentation; do not sort during loading. The C1 pre-expansion fingerprint `52392C0BB009F79A680F03AAC5C9C46BF2387CB4AD98B7F7D9A39EDD3A4540C0` remains recorded as a legacy baseline, not a target for C2/C3's intentionally changed content and outcomes. The exact fixed S5 compatibility fixture remains unchanged.
- C1 itself introduced no additional legacy content. C2's three selected item definitions and their source paths are recorded above; the sibling revision and C2-relevant worktree state are explicit. Older weapon/supply definition comments still do not establish an exact source revision for those imports.

Focused validation from the workspace root:

```powershell
dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj --filter "FullyQualifiedName~CatalogContentTests|FullyQualifiedName~CatalogCompatibilityTests|FullyQualifiedName~LootCatalogTests|FullyQualifiedName~EquipmentAndShopTests"
```

### Editing authored narration and AI cues

[Narration and flavor](server/AFKRaiders.Server/Content/narration-catalog.json) use schema 1 and content version `desperanza-narration-v1`. C4 preserves the existing wording and ordering in 32 pools and 108 templates:

- `pools` contain survival lines, residence tasks, rotating Comms voice lines and short `style.*` object cues for the existing guarded AI prompt. Pool order affects deterministic selection. Keep at least two distinct nonblank, single-line entries per pool; duplicates and control characters are rejected.
- `templates` contain authored narrator messages and simulation presentation, with numbered placeholders such as `{0}`. Keep every declared argument index; do not introduce named tokens, alignment or arbitrary format suffixes. Existing `HH:mm:ss`, `P0` and `F2` formats retain their original argument meaning. The unused `survival.robot-progress` pool alone retains its existing named robot tokens.
- Required IDs and argument counts live in [NarrationContracts.cs](server/AFKRaiders.Server/Content/NarrationContracts.cs). Wording edits need no C# changes; new keys or argument contracts require reviewed call-site changes. Unknown/duplicate IDs or properties, missing keys/arguments, malformed placeholders, unsupported versions and invalid candidate configuration fail startup explicitly.
- `style.*` entries must be short lowercase object phrases (40 characters maximum), not instructions. Shape validation is not semantic approval: review cues for tone and suitability before using them in prompts.
- `enrichmentEventKinds` preserves the five default candidates above. Optional `AreaDiary`, `Downed` and `KnockedOut` may be deliberately enabled after review; an empty array disables live-event candidates, not item appraisal or post-raid debriefs. This does not alter the one-pending-request limit, 90-second freshness, 25-second model budget, privacy checks or fallback handling.
- Mechanics, event dispatch, actor/instance IDs, compact status labels, factual receipt assembly and facts/aside delivery wrappers remain C#. Authored templates describe already-committed events; changing their text must not assert additional rewards, damage, actors or actions. JSON cannot select new gameplay effects.

AI can assist **offline drafting** of these pools/templates, but suggestions must be reviewed against the [tone guide](docs/lore/content-guidelines/TONE_VOICE.md), [legal guardrails](docs/lore/content-guidelines/LEGAL_GUARDRAILS.md), placeholder contracts and committed-event truth before editing the source catalog. No automatic model-to-catalog writing or live generated gameplay is introduced. Runtime AI still supplies only validated optional asides; it never replaces authoritative facts. Existing wording has not been rewritten or declared funnier by C4.

Focused narration validation:

```powershell
dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj --filter "FullyQualifiedName~NarrationContentTests|FullyQualifiedName~NarrationCompatibilityTests|FullyQualifiedName~NarrativeEnrichmentTests|FullyQualifiedName~CommsAndPacingTests"
```

The pre-migration text/simulation fingerprint `20E1569D4BB57C2A4038EC9C38B47074921DA48677763CE5127ED9CDECC5CC69` remains frozen in [NarrationCompatibilityTests.cs](tests/AFKRaiders.Server.Tests/NarrationCompatibilityTests.cs). Future intentional wording changes require an explicitly reviewed compatibility update, not blindly replacing the hash.

See the [implementation progress ledger](docs/IMPLEMENTATION_PROGRESS.md) for C2-C4 status, verification evidence, and limitations. Weights and category mappings are experimental; automated coverage is not human comedy or balance approval.

Run the server tests with `dotnet test AFKRaidersOnline.slnx`.
