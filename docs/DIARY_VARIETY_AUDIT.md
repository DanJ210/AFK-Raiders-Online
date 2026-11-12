# DIARY VARIETY slice 1 audit

## Scope and method (2026-11-12)

Parts A/B only. Part C requires designer review and has **not** been drafted or loaded.
Decision authority is recorded in the [handoff](IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint).
Baseline source: `e1dfa85`; narration `desperanza-narration-v6`, gameplay
`desperanza-bal-4-arm3-v1`. The [read-only utility](../tools/DiaryVarietyReadOnlyAnalysis/Program.cs)
runs one autonomous Raider per cohort, traits separately, seeds 101/202/303,
5-second injected-clock increments, at 8h and 24h. No commands, crossings,
live-model generation, persistence or running-backend access.

**Measured:** a quote is exact trimmed text after `Raider: `. Previous 20/50
means previous quotes across **all** kinds/pools for that Raider, not a filtered
subsequence. History resets per seed, not per raid; initial quotes remain in the
denominator. Group counts sum independent cohorts; the 8h snapshots are prefixes
of the 24h runs, not independent evidence. Pool share uses all quote occurrences;
repeat share uses that row's quote count. Top-ten frequencies aggregate the three
seeds for the trait/horizon/pool. Hub stays end at admission; the final observed
stay is marked partial. Stays with no activity are not counted. Hub activity is
not a `Raider:` quote and is reported separately.

**Source-audited:** the utility is not referenced by the main solution or default
server test project. `**/bin/` and `**/obj/` are ignored; `git ls-files '*bin*'
'*obj*'` returned no files. Release validation avoids the user's locked Debug
backend executable; no backend restart. No tool/dependency was added to the
default test run.

## Cautious: why the apparent advantage?

**Measured, 24h:** fallback concentration is nearly identical: Cautious
8,197/20,561 (39.87%), Hoarder 8,232/20,688 (39.79%), Brawler
8,248/20,634 (39.97%), Gadgeteer 8,465/21,094 (40.13%).
Within those fallback occurrences, previous-20 repetition is respectively
1,112/8,197 (13.57%), 5,435/8,232 (66.02%), 6,294/8,248 (76.31%),
and 6,538/8,465 (77.24%). Cautious's overall 1,120/20,561 (5.45%)
is **not** a 5.4% general-pool repeat rate.

**Measured:** Cautious's 1,112 fallback repeats plus eight area-diary repeats
account for all 1,120 previous-20 repeats. `PersonalityChanged` supplies
3,357 fallback quotes, but only 335 previous-20 repeats for Cautious.
Other examples: Arrived 75/781, PackingCompleted 116/413, ShieldAbsorbed
79/579. At previous 50, 8,161/8,197 (99.56%) of the general pool repeats:
the advantage disappears (overall 40.11%).

**Source-audited / inferred explanation:** existing rotation is per logical
family, with most fallthrough kinds sharing `General`, plus a last-line guard.
It persists across admissions but only prevents immediate repeats. Twelve general
lines put a full rotation farther apart in the mixed event stream than six/seven
trait lines. Around 40% fallback traffic implies roughly 30 quotes per twelve-line
rotation versus 15/17.5 for six/seven lines. This spacing is an explanation, not
an independently randomized causal experiment. The proposal's claim of no
short-term memory was incomplete: there was an immediate-repeat guard, not a
previous-20 governor. The general pool is not a dedicated Cautious voice.

## Event coverage and Part C sizing

The runtime emits the literal kinds listed below plus dynamic `RobotDefeated`
and `RobotEscaped`. AreaDiary routes Surveying/Checking/other to diary pools;
area start/completion route Surveying/Checking/Rummaging to their corresponding
six-/four-line pools. Unknown activities fall through except AreaDiary.
The unused compatibility aliases (`SearchStarted`, `ItemAwarded`, `RobotJoined`,
`ExtractionCompleted`, etc.) are not newly emitted event types.

In the baseline, all other kinds outside Travel, Search, ItemAllocated,
ConsumableUsed, WorldCondition, the three fighting kinds and ExtractionStarted/
Extracted fall through: Hoarder -> `voice.hoarder` (7), Brawler ->
`voice.brawler` (6), Gadgeteer -> `voice.gadgeteer` (6), Cautious/shared ->
`voice.general` (12). Zero observations mean **unmeasured**, not unreachable:
solo cohorts do not exercise actual crossings, joins, support or pocket salvage.
The generated coverage table gives each kind's per-trait count and share.

**Proposed pilot, not loaded:** eight candidates per family per trait, beginning
with personality change, shield absorption, unmitigated damage, resilience,
extraction decision (stay/leave eligibility), search resolution, arrival/admission,
packing, condition warning/impact, robot aftermath, and recovery/KO/pocket outcomes.
Eight is a drafting size, not proof of either rolling target. The lowest-volume
families may be grouped only with explicitly compatible committed fields; retain
distinct eligibility for damage mitigated versus damage taken and stay versus exit.
Hub activity needs a separate expansion; references to other residents remain
excluded. Review the family scope after Part B before producing the pilot.

## Part B implementation and limitations

Implemented presentation/selection only; Part C is **awaiting designer review**.
No new authored lines, templates, gameplay events, slots or catalog pools were loaded.

### Conservative event-fit reuse

These are code-owned **selection route keys**, not new JSON pools. Indices are
zero-based in the existing approved source pool. They are eligibility subsets,
not trait overrides; fallback kinds without genuinely fitting lines remain unchanged.

| Kind | Event-fit key | Approved source / indices | Eligible size |
|---|---|---|---:|
| Arrived | voice.arrival | voice.travel: 6, 7, 10, 12 (maps/arrival, no continued-travel claim) | 4 |
| Departure, DutyAdmission | voice.admission | voice.travel: 6, 7, 8, 9, 10, 11, 12 (pod/tube/departure context) | 7 |
| PackingStarted, PackingCompleted | voice.packing | voice.allocation: 0-5 (already allocated cargo, not a new award or sale) | 6 |
| ExtractionDecision | voice.exit-decision | voice.exit: 2, 4, 9 (timer/panic/plan, true for stay or leave) | 3 |
| SharedExtractionCalled, ExtractionVehicleArrived, ExtractionJoined | voice.shared-exit | voice.exit: 2, 3, 4, 7, 8, 9, 12 (no individual boarding/outcome claim) | 7 |
| RobotDefeated, RobotEscaped | voice.robot-aftermath | voice.robot: 3, 5, 10 (aftermath, not continued fighting) | 3 |

**Still on their original trait/general pool for Part C:** ActivityInterrupted,
ConditionImpact, CrossedPaths, DeadlineExpired, Deployment, Downed, EncounterResolved,
HandlerAction, HazardWarning, HiddenPocketEmpty, HiddenPocketSalvaged, KnockedOut,
OpportunityExhausted, PersonalityChanged, RecoveryStarted, RecoveryCompleted,
Resilience, RobotDefeatCargoResolved, SearchCompleted, ShieldAbsorbed,
UnmitigatedDamage. No existing approved family-specific lines safely cover these
consequences as a pool. Zero-observation kinds remain explicitly unmeasured.
The Cautious general pool and existing generic voice limitations remain until
human-reviewed dedicated lines are approved; the [voice guide](lore/content-guidelines/TONE_VOICE.md)
defines all four pilot voices without loading them.

### Governor

[NarrationSelectionMemory](../server/AFKRaiders.Server/Content/NarrationSelectionMemory.cs)
stores line last-use positions and a quote ordinal only. It is separate from
health, greed, inventory, decisions and scheduling, carried through the existing
voice checkpoint. Eligibility is resolved before recency. Prefer an unseen line;
otherwise choose the least recently used eligible line. Ties rotate from the
recipients' maximum **quote ordinal**, never the global event sequence or gameplay
RNG. For shared entries, minimize the most-recent use among actual recipients,
comparing relative ages rather than incomparable absolute counters. All recipients
record the same chosen line. Hub activity uses the same memory/algorithm, across
stays and raids; its event schedule, journal IDs and cadence are unchanged.
The metadata is bounded by finite authored line identities, not an ever-growing
event archive. Replay consumes each committed event once in order; transport
polling/upserts never reselect lines.

**Surprise and correction:** an initial global-sequence tie-breaker changed
quotes under reversed admission order (eight existing regression failures).
Switching to narration quote ordinals fixed that without relaxing those tests.
The final matched outcome comparison includes thread facts after removing only
the appended Raider aside. This prevents narration-bearing thread strings and
random starter IDs from being mistaken for gameplay changes.

### Client

[DiaryEntryText](../web/src/components/DiaryEntryText.vue) is wired into the
continuous journal. Personality deltas, extraction odds/greed, travel ETA and
activity-duration values move behind native keyboard-accessible **Event details**.
The full original record remains intact there; direct damage/shield/combat numbers
remain in their main sentences. AreaDiary hides only the exact recurring
no-effect/co-presence suffixes in the main sentence. Other kinds and unrecognized
formats remain unchanged. Optional generated-aside badges/rendering and active
thread factual progression are unchanged; no live-AI functionality was added.

### Compatibility, targets and review gate

Narration content version **v6 -> v7** marks changed routing/selection semantics
only. The JSON diff is that single version field; pool lines, templates,
enrichment eligibility and schema are unchanged. Narration compatibility hashes
change because the compatibility fixture includes authored selection and formatted
projections, not because of different outcomes:

| Arm | Previous narration compatibility | New narration compatibility |
|---|---|---|
| Arm 3 | `4C3D0A00E28B25D579E8AE72D3EDC2422532088463BC3A87DB1BA3BF386FE4AC` | `5B1952FBA537B825876C72B31472E6BEC949287B171723F0D3114FAB44E523A4` |
| Arm 4A | `6BCECA6CE60AC87DAE8EB5D53FDD3635C1104E0A27F8615AFBEE78CC53F70C87` | `7DF539331C26D1DBD89BCB3C499AD7BFC4DB8C0C322641BDE842EAA6F24880CC` |
| Arm 4B | `6285BA484563A01061EFC86D38060166A8AADF646C991D57837858AC8C94F5F5` | `5CC0D89687CD23C9868E213D1E5A84FE680BC31D648CE7C19E18CFE35CE756B9` |
| Arm 5 | `A26935F4A799EA192DC3999489C0A6161CF1D1011A1BEF564FDB160D0C8D6744` | `30D651C3F659502A7FBD23177EB75CFA3ED8E761BD934E246E7F7AC0E234B1CE` |

Gameplay rules versions and existing catalog/gameplay compatibility inputs/hashes
are unchanged. All 24 paired trait/seed/horizon outcome hashes match untouched
`e1dfa85`; twelve 8h hashes are frozen in focused regression tests.

**Measured 8h verdict:** Cautious meets both experimental targets in all three
seeds. Hoarder meets previous 20, narrowly in seed 202 (211/2,111 = 9.9953%,
not above 10% despite rounding to 10.00%), but misses previous 50. Brawler and
Gadgeteer miss both. The slice does **not** meet the targets for all traits.
Remaining generic traffic falls from about 40% to about 27%; the six-/seven-line
fallback pools still dominate repeated quotes. A three-line exit-decision subset
introduces local exit-pool repetition (shown below); event fit must not be sacrificed
to suppress that measurement. Pool growth/trait voices need human review.

Hub stays already had no within-stay repeated activity text in these cohorts.
After the governor, the ten-line pool's unique-per-stay results are unchanged:
Cautious 583 entries / 172 observed stays (mean 3.390 unique),
Hoarder 603/169 (3.568), Brawler 636/171 (3.719), Gadgeteer 562/167 (3.365).
This is not evidence that a longer/live Hub stay cannot repeat.

**Review decisions:** accept or edit the decluttered presentation and conservative
reuse; select/prioritize the proposed pilot families; review the dedicated Cautious
definition; then authorize Part C's eight **draft-only** lines per chosen family
per trait. No pilot document has been created, no drafts loaded, and no automatic
continuation past this gate is authorized.

### Verification

- Focused narration/replay/outcome/admission-order tests: **83/83 passed**.
- Web tests: **28/28 passed**; Vue/TypeScript production build passed.
- Populated component at 360/540/900/1280px, collapsed and expanded: eight layout
  checks passed with no horizontal overflow; keyboard expansion and retained
  numbers/co-presence record verified. Included short/long content and shield receipts.
  This was a temporary frontend-only fixture, not a restarted backend or a live
  two-client gameplay soak; it was removed and its Vite process stopped.
- Full server suite: **535/535 passed** in Release with `DOTNET_PROCESSOR_COUNT=2`;
  the known timing-sensitive cancellation test did not fail (no rerun needed).
  Final path/diff/raw-acronym checks are recorded in the ledger.

### Reproduction

From the workspace root, use Release output if a running Debug backend holds its
executable. The tool is opt-in and emits JSON/report text to stdout:

```powershell
dotnet run --configuration Release --project tools\DiaryVarietyReadOnlyAnalysis\DiaryVarietyReadOnlyAnalysis.csproj > diary-after.json
dotnet run --no-build --configuration Release --project tools\DiaryVarietyReadOnlyAnalysis\DiaryVarietyReadOnlyAnalysis.csproj -- --compare diary-before.json diary-after.json
```

The baseline used an isolated archive of `e1dfa85` with only the expanded analysis
utility copied in. `--compare` verifies matched outcome hashes before printing
the Markdown tables. `--diagnostic` limits output to Cautious/101/8h and includes
the presentation-free projection for precise outcome diagnostics. No game save
is written or consumed. Generated tables below preserve the before/after evidence.

## Generated measurements

All counts below are **Measured**. Zero repeats retain their full denominators. Source pool IDs identify the existing catalog text, even when an event-fit route uses a subset.

### Trait totals

| Trait | Hours | Quotes (before = after) | Before R20 | After R20 | Before R50 | After R50 |
|---|---:|---:|---:|---:|---:|---:|
| Cautious | 8 | 6619 | 356/6619 (5.38%) | 71/6619 (1.07%) | 2617/6619 (39.54%) | 1113/6619 (16.82%) |
| Cautious | 24 | 20561 | 1120/20561 (5.45%) | 242/20561 (1.18%) | 8247/20561 (40.11%) | 3714/20561 (18.06%) |
| Hoarder | 8 | 6563 | 1703/6563 (25.95%) | 618/6563 (9.42%) | 2637/6563 (40.18%) | 1794/6563 (27.34%) |
| Hoarder | 24 | 20688 | 5443/20688 (26.31%) | 2038/20688 (9.85%) | 8307/20688 (40.15%) | 5741/20688 (27.75%) |
| Brawler | 8 | 6673 | 2039/6673 (30.56%) | 826/6673 (12.38%) | 2704/6673 (40.52%) | 1859/6673 (27.86%) |
| Brawler | 24 | 20634 | 6302/20634 (30.54%) | 2553/20634 (12.37%) | 8337/20634 (40.40%) | 5763/20634 (27.93%) |
| Gadgeteer | 8 | 6753 | 1977/6753 (29.28%) | 747/6753 (11.06%) | 2679/6753 (39.67%) | 1829/6753 (27.08%) |
| Gadgeteer | 24 | 21094 | 6553/21094 (31.07%) | 2675/21094 (12.68%) | 8552/21094 (40.54%) | 5924/21094 (28.08%) |

### Individual seeds and unchanged outcome fingerprints

Fingerprints retain committed factual receipts, all event timestamps/fields, thread factual logs, KO/loot/coins/shields/stash/stats/stock/Signal and projected timing. Only narration fields and appended Raider quotes are excluded. Random GUID-derived starter-kettle IDs are bijectively canonicalized in encounter order; identity references and ownership relationships remain. AAR/appraisal and highlights are presentation, not outcome evidence.

| Trait | Seed | Hours | Quotes | Before R20 / R50 | After R20 / R50 | Matched SHA-256 |
|---|---:|---:|---:|---|---|---|
| Cautious | 101 | 8 | 2243 | 97/2243 (4.32%) / 870/2243 (38.79%) | 15/2243 (0.67%) / 345/2243 (15.38%) | `779BF840A4B9EEDE74C4CBA4B8DEA217AF2A83EE5FB37124CDCAD4732D801744` |
| Cautious | 101 | 24 | 6925 | 286/6925 (4.13%) / 2739/6925 (39.55%) | 70/6925 (1.01%) / 1206/6925 (17.42%) | `BA5FAF83F13C11B1A34D7CFAD43F72374D5D8E481713DE3B4EE918B21CC1BB5B` |
| Cautious | 202 | 8 | 2110 | 144/2110 (6.82%) / 856/2110 (40.57%) | 22/2110 (1.04%) / 393/2110 (18.63%) | `1B5DD5DEC6ABBFC966A903D4D752E3DDFA3A8536E92231570610E44F191C7728` |
| Cautious | 202 | 24 | 6725 | 458/6725 (6.81%) / 2752/6725 (40.92%) | 84/6725 (1.25%) / 1326/6725 (19.72%) | `E37CA5ED3ED1EFCCFC78AC0A1372BB409C7E20867AE2C01AD5A46E7C26E5A012` |
| Cautious | 303 | 8 | 2266 | 115/2266 (5.08%) / 891/2266 (39.32%) | 34/2266 (1.50%) / 375/2266 (16.55%) | `3E35BD2469BA194C34889E457BB7F6E47B48EB37ABF83B489A96D466B51A486B` |
| Cautious | 303 | 24 | 6911 | 376/6911 (5.44%) / 2756/6911 (39.88%) | 88/6911 (1.27%) / 1182/6911 (17.10%) | `09633678BB668037DDF85175273769329320A39E5B742B6F608A9A696856A0C0` |
| Hoarder | 101 | 8 | 2261 | 556/2261 (24.59%) / 891/2261 (39.41%) | 198/2261 (8.76%) / 599/2261 (26.49%) | `897B6CE33C6C4245E06944A1DDF112BC0234F2F6A71DB161966B2D4CC8229AE5` |
| Hoarder | 101 | 24 | 7012 | 1918/7012 (27.35%) / 2831/7012 (40.37%) | 740/7012 (10.55%) / 1969/7012 (28.08%) | `238329E8A8923B4681EC1070B923D80C21E4E00C2382DB7DB4AB44B74469FA7D` |
| Hoarder | 202 | 8 | 2111 | 573/2111 (27.14%) / 858/2111 (40.64%) | 211/2111 (10.00%) / 586/2111 (27.76%) | `8DA9D514FE47482B3A6EB24169964702B089EDDF7589F78CD9E3588EAD5EFA8B` |
| Hoarder | 202 | 24 | 6820 | 1827/6820 (26.79%) / 2763/6820 (40.51%) | 706/6820 (10.35%) / 1917/6820 (28.11%) | `76A3116067CB32390031BA138D31A5A7CB62170611E33E86EC046F4E473A3BE4` |
| Hoarder | 303 | 8 | 2191 | 574/2191 (26.20%) / 888/2191 (40.53%) | 209/2191 (9.54%) / 609/2191 (27.80%) | `ACDF75C2BC66803C914A76D8D703C2BB3B1A8987787792F9B0D7D13569317958` |
| Hoarder | 303 | 24 | 6856 | 1698/6856 (24.77%) / 2713/6856 (39.57%) | 592/6856 (8.63%) / 1855/6856 (27.06%) | `3C342E3D740A379DA7F429B1428AC2FFF169AC94F995031EFEA71CA95948C273` |
| Brawler | 101 | 8 | 2244 | 667/2244 (29.72%) / 895/2244 (39.88%) | 249/2244 (11.10%) / 606/2244 (27.01%) | `C6B745A63428214B3B46494F3A0B9403BE405AB0A58E6AA62DCF375F799CD1F0` |
| Brawler | 101 | 24 | 6991 | 2184/6991 (31.24%) / 2835/6991 (40.55%) | 900/6991 (12.87%) / 1966/6991 (28.12%) | `BFFB36F4593DF6A7CD1150136DC5D77025FB221C4B131F93A1DAF77146B55379` |
| Brawler | 202 | 8 | 2255 | 733/2255 (32.51%) / 936/2255 (41.51%) | 313/2255 (13.88%) / 653/2255 (28.96%) | `52266B94E71CC5BF58857BD9D2E430F344EBFE4BB215EF8F2B246BF9E379ABC0` |
| Brawler | 202 | 24 | 6869 | 2142/6869 (31.18%) / 2807/6869 (40.86%) | 896/6869 (13.04%) / 1960/6869 (28.53%) | `0B47ECA3A0E3C721A826B8BEAE09C54B89E64890F196858657BAB1FA7745D243` |
| Brawler | 303 | 8 | 2174 | 639/2174 (29.39%) / 873/2174 (40.16%) | 264/2174 (12.14%) / 600/2174 (27.60%) | `030DAC439CE90DFB2566048DA5D757FD0BBAA691C2E3EB01202D22FD6C6F1560` |
| Brawler | 303 | 24 | 6774 | 1976/6774 (29.17%) / 2695/6774 (39.78%) | 757/6774 (11.18%) / 1837/6774 (27.12%) | `0D0F817707DA5F9C17AB9F6662A2800376AEBBFB0E7222D24697432868B378A6` |
| Gadgeteer | 101 | 8 | 2275 | 654/2275 (28.75%) / 892/2275 (39.21%) | 240/2275 (10.55%) / 601/2275 (26.42%) | `0EA6F95A27311D82EF151346152A3E8EAA0461D3403D44BA78FE9F448AAD17E9` |
| Gadgeteer | 101 | 24 | 7068 | 2226/7068 (31.49%) / 2878/7068 (40.72%) | 926/7068 (13.10%) / 1998/7068 (28.27%) | `A000089AF15BEDFF5107BF9320B751D3CF60D5673BB20C2E83E491B5FB8DBC1C` |
| Gadgeteer | 202 | 8 | 2198 | 641/2198 (29.16%) / 874/2198 (39.76%) | 246/2198 (11.19%) / 603/2198 (27.43%) | `A32B6E1B4C1AFDDD0EED03780217F4257A45F17335E636187BFAC9E12974529C` |
| Gadgeteer | 202 | 24 | 7076 | 2237/7076 (31.61%) / 2898/7076 (40.96%) | 962/7076 (13.60%) / 2025/7076 (28.62%) | `36CA372A486CEFF08C02984F061E2BDE80516E74D8185D925418B6AB651B69C0` |
| Gadgeteer | 303 | 8 | 2280 | 682/2280 (29.91%) / 913/2280 (40.04%) | 261/2280 (11.45%) / 625/2280 (27.41%) | `A0882EB97A080B2D7FC30F4006F9720E0C01CAA6CF6D1FBA5C354655E5865BFB` |
| Gadgeteer | 303 | 24 | 6950 | 2090/6950 (30.07%) / 2776/6950 (39.94%) | 787/6950 (11.32%) / 1901/6950 (27.35%) | `DE41233082CB79D831B8346B0B2DC40902FDFF3050E2D639FE8BEAB6DA6FE898` |

### Baseline event routing, pool size and fallthrough share (24h)

F denotes trait/general fallthrough; E denotes event/activity fit. Unobserved kinds retain the source-audited fallback (7/6/6/12 by trait); no measured share is inferred.

| Kind | Trait | Source pool (size) | Route | Quotes / all quotes (share) |
|---|---|---|---|---|
| ActivityInterrupted | Cautious | voice.general (12) | F | 155/20561 (0.75%) |
| ActivityInterrupted | Hoarder | voice.hoarder (7) | F | 153/20688 (0.74%) |
| ActivityInterrupted | Brawler | voice.brawler (6) | F | 155/20634 (0.75%) |
| ActivityInterrupted | Gadgeteer | voice.gadgeteer (6) | F | 156/21094 (0.74%) |
| AreaActivityCompleted | Cautious | voice.complete-surveying (4) | E | 645/20561 (3.14%) |
| AreaActivityCompleted | Cautious | voice.complete-checking (4) | E | 599/20561 (2.91%) |
| AreaActivityCompleted | Cautious | voice.complete-rummaging (4) | E | 566/20561 (2.75%) |
| AreaActivityCompleted | Hoarder | voice.complete-surveying (4) | E | 662/20688 (3.20%) |
| AreaActivityCompleted | Hoarder | voice.complete-checking (4) | E | 609/20688 (2.94%) |
| AreaActivityCompleted | Hoarder | voice.complete-rummaging (4) | E | 569/20688 (2.75%) |
| AreaActivityCompleted | Brawler | voice.complete-surveying (4) | E | 645/20634 (3.13%) |
| AreaActivityCompleted | Brawler | voice.complete-checking (4) | E | 605/20634 (2.93%) |
| AreaActivityCompleted | Brawler | voice.complete-rummaging (4) | E | 577/20634 (2.80%) |
| AreaActivityCompleted | Gadgeteer | voice.complete-surveying (4) | E | 668/21094 (3.17%) |
| AreaActivityCompleted | Gadgeteer | voice.complete-checking (4) | E | 615/21094 (2.92%) |
| AreaActivityCompleted | Gadgeteer | voice.complete-rummaging (4) | E | 585/21094 (2.77%) |
| AreaActivityStarted | Cautious | voice.start-surveying (6) | E | 714/20561 (3.47%) |
| AreaActivityStarted | Cautious | voice.start-checking (6) | E | 645/20561 (3.14%) |
| AreaActivityStarted | Cautious | voice.start-rummaging (6) | E | 598/20561 (2.91%) |
| AreaActivityStarted | Hoarder | voice.start-surveying (6) | E | 718/20688 (3.47%) |
| AreaActivityStarted | Hoarder | voice.start-checking (6) | E | 660/20688 (3.19%) |
| AreaActivityStarted | Hoarder | voice.start-rummaging (6) | E | 609/20688 (2.94%) |
| AreaActivityStarted | Brawler | voice.start-surveying (6) | E | 718/20634 (3.48%) |
| AreaActivityStarted | Brawler | voice.start-checking (6) | E | 644/20634 (3.12%) |
| AreaActivityStarted | Brawler | voice.start-rummaging (6) | E | 603/20634 (2.92%) |
| AreaActivityStarted | Gadgeteer | voice.start-surveying (6) | E | 718/21094 (3.40%) |
| AreaActivityStarted | Gadgeteer | voice.start-checking (6) | E | 668/21094 (3.17%) |
| AreaActivityStarted | Gadgeteer | voice.start-rummaging (6) | E | 613/21094 (2.91%) |
| AreaDiary | Cautious | voice.diary-surveying (8) | E | 1881/20561 (9.15%) |
| AreaDiary | Cautious | voice.diary-checking (8) | E | 1741/20561 (8.47%) |
| AreaDiary | Cautious | voice.diary-general (17) | E | 1652/20561 (8.03%) |
| AreaDiary | Hoarder | voice.diary-surveying (8) | E | 1924/20688 (9.30%) |
| AreaDiary | Hoarder | voice.diary-checking (8) | E | 1760/20688 (8.51%) |
| AreaDiary | Hoarder | voice.diary-general (17) | E | 1669/20688 (8.07%) |
| AreaDiary | Brawler | voice.diary-surveying (8) | E | 1890/20634 (9.16%) |
| AreaDiary | Brawler | voice.diary-checking (8) | E | 1739/20634 (8.43%) |
| AreaDiary | Brawler | voice.diary-general (17) | E | 1685/20634 (8.17%) |
| AreaDiary | Gadgeteer | voice.diary-surveying (8) | E | 1930/21094 (9.15%) |
| AreaDiary | Gadgeteer | voice.diary-checking (8) | E | 1779/21094 (8.43%) |
| AreaDiary | Gadgeteer | voice.diary-general (17) | E | 1707/21094 (8.09%) |
| Arrived | Cautious | voice.general (12) | F | 781/20561 (3.80%) |
| Arrived | Hoarder | voice.hoarder (7) | F | 767/20688 (3.71%) |
| Arrived | Brawler | voice.brawler (6) | F | 774/20634 (3.75%) |
| Arrived | Gadgeteer | voice.gadgeteer (6) | F | 778/21094 (3.69%) |
| ConditionImpact | Cautious | voice.general (12) | F | 259/20561 (1.26%) |
| ConditionImpact | Hoarder | voice.hoarder (7) | F | 258/20688 (1.25%) |
| ConditionImpact | Brawler | voice.brawler (6) | F | 266/20634 (1.29%) |
| ConditionImpact | Gadgeteer | voice.gadgeteer (6) | F | 287/21094 (1.36%) |
| ConsumableUsed | Cautious | voice.consumable (8) | E | 269/20561 (1.31%) |
| ConsumableUsed | Hoarder | voice.consumable (8) | E | 274/20688 (1.32%) |
| ConsumableUsed | Brawler | voice.consumable (8) | E | 270/20634 (1.31%) |
| ConsumableUsed | Gadgeteer | voice.consumable (8) | E | 315/21094 (1.49%) |
| CrossedPaths | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| DeadlineExpired | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| Departure | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| Deployment | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| Downed | Cautious | voice.general (12) | F | 20/20561 (0.10%) |
| Downed | Hoarder | voice.hoarder (7) | F | 20/20688 (0.10%) |
| Downed | Brawler | voice.brawler (6) | F | 26/20634 (0.13%) |
| Downed | Gadgeteer | voice.gadgeteer (6) | F | 13/21094 (0.06%) |
| DutyAdmission | Cautious | voice.general (12) | F | 171/20561 (0.83%) |
| DutyAdmission | Hoarder | voice.hoarder (7) | F | 169/20688 (0.82%) |
| DutyAdmission | Brawler | voice.brawler (6) | F | 170/20634 (0.82%) |
| DutyAdmission | Gadgeteer | voice.gadgeteer (6) | F | 167/21094 (0.79%) |
| EncounterResolved | Cautious | voice.general (12) | F | 142/20561 (0.69%) |
| EncounterResolved | Hoarder | voice.hoarder (7) | F | 138/20688 (0.67%) |
| EncounterResolved | Brawler | voice.brawler (6) | F | 143/20634 (0.69%) |
| EncounterResolved | Gadgeteer | voice.gadgeteer (6) | F | 144/21094 (0.68%) |
| Extracted | Cautious | voice.exit (13) | E | 149/20561 (0.72%) |
| Extracted | Hoarder | voice.exit (13) | E | 147/20688 (0.71%) |
| Extracted | Brawler | voice.exit (13) | E | 142/20634 (0.69%) |
| Extracted | Gadgeteer | voice.exit (13) | E | 153/21094 (0.73%) |
| ExtractionDecision | Cautious | voice.general (12) | F | 456/20561 (2.22%) |
| ExtractionDecision | Hoarder | voice.hoarder (7) | F | 472/20688 (2.28%) |
| ExtractionDecision | Brawler | voice.brawler (6) | F | 437/20634 (2.12%) |
| ExtractionDecision | Gadgeteer | voice.gadgeteer (6) | F | 494/21094 (2.34%) |
| ExtractionJoined | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| ExtractionStarted | Cautious | voice.exit (13) | E | 157/20561 (0.76%) |
| ExtractionStarted | Hoarder | voice.exit (13) | E | 154/20688 (0.74%) |
| ExtractionStarted | Brawler | voice.exit (13) | E | 155/20634 (0.75%) |
| ExtractionStarted | Gadgeteer | voice.exit (13) | E | 157/21094 (0.74%) |
| ExtractionVehicleArrived | Cautious | voice.general (12) | F | 149/20561 (0.72%) |
| ExtractionVehicleArrived | Hoarder | voice.hoarder (7) | F | 148/20688 (0.72%) |
| ExtractionVehicleArrived | Brawler | voice.brawler (6) | F | 153/20634 (0.74%) |
| ExtractionVehicleArrived | Gadgeteer | voice.gadgeteer (6) | F | 154/21094 (0.73%) |
| HandlerAction | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| HazardWarning | Cautious | voice.general (12) | F | 248/20561 (1.21%) |
| HazardWarning | Hoarder | voice.hoarder (7) | F | 237/20688 (1.15%) |
| HazardWarning | Brawler | voice.brawler (6) | F | 255/20634 (1.24%) |
| HazardWarning | Gadgeteer | voice.gadgeteer (6) | F | 280/21094 (1.33%) |
| HiddenPocketEmpty | Cautious | voice.general (12) | F | 20/20561 (0.10%) |
| HiddenPocketEmpty | Hoarder | voice.hoarder (7) | F | 20/20688 (0.10%) |
| HiddenPocketEmpty | Brawler | voice.brawler (6) | F | 26/20634 (0.13%) |
| HiddenPocketEmpty | Gadgeteer | voice.gadgeteer (6) | F | 13/21094 (0.06%) |
| HiddenPocketSalvaged | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| ItemAllocated | Cautious | voice.allocation (14) | E | 826/20561 (4.02%) |
| ItemAllocated | Hoarder | voice.allocation (14) | E | 818/20688 (3.95%) |
| ItemAllocated | Brawler | voice.allocation (14) | E | 824/20634 (3.99%) |
| ItemAllocated | Gadgeteer | voice.allocation (14) | E | 836/21094 (3.96%) |
| KnockedOut | Cautious | voice.general (12) | F | 20/20561 (0.10%) |
| KnockedOut | Hoarder | voice.hoarder (7) | F | 20/20688 (0.10%) |
| KnockedOut | Brawler | voice.brawler (6) | F | 26/20634 (0.13%) |
| KnockedOut | Gadgeteer | voice.gadgeteer (6) | F | 13/21094 (0.06%) |
| OpportunityExhausted | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| PackingCompleted | Cautious | voice.general (12) | F | 413/20561 (2.01%) |
| PackingCompleted | Hoarder | voice.hoarder (7) | F | 409/20688 (1.98%) |
| PackingCompleted | Brawler | voice.brawler (6) | F | 412/20634 (2.00%) |
| PackingCompleted | Gadgeteer | voice.gadgeteer (6) | F | 418/21094 (1.98%) |
| PackingStarted | Cautious | voice.general (12) | F | 413/20561 (2.01%) |
| PackingStarted | Hoarder | voice.hoarder (7) | F | 409/20688 (1.98%) |
| PackingStarted | Brawler | voice.brawler (6) | F | 412/20634 (2.00%) |
| PackingStarted | Gadgeteer | voice.gadgeteer (6) | F | 418/21094 (1.98%) |
| PersonalityChanged | Cautious | voice.general (12) | F | 3357/20561 (16.33%) |
| PersonalityChanged | Hoarder | voice.hoarder (7) | F | 3394/20688 (16.41%) |
| PersonalityChanged | Brawler | voice.brawler (6) | F | 3384/20634 (16.40%) |
| PersonalityChanged | Gadgeteer | voice.gadgeteer (6) | F | 3455/21094 (16.38%) |
| RecoveryCompleted | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| RecoveryStarted | All | trait/general (7/6/6/12) | F | 0 observed; unmeasured |
| Resilience | Cautious | voice.general (12) | F | 411/20561 (2.00%) |
| Resilience | Hoarder | voice.hoarder (7) | F | 452/20688 (2.18%) |
| Resilience | Brawler | voice.brawler (6) | F | 443/20634 (2.15%) |
| Resilience | Gadgeteer | voice.gadgeteer (6) | F | 487/21094 (2.31%) |
| RobotCombatStarted | Cautious | voice.robot (11) | E | 161/20561 (0.78%) |
| RobotCombatStarted | Hoarder | voice.robot (11) | E | 157/20688 (0.76%) |
| RobotCombatStarted | Brawler | voice.robot (11) | E | 155/20634 (0.75%) |
| RobotCombatStarted | Gadgeteer | voice.robot (11) | E | 157/21094 (0.74%) |
| RobotDefeatCargoResolved | Cautious | voice.general (12) | F | 100/20561 (0.49%) |
| RobotDefeatCargoResolved | Hoarder | voice.hoarder (7) | F | 102/20688 (0.49%) |
| RobotDefeatCargoResolved | Brawler | voice.brawler (6) | F | 99/20634 (0.48%) |
| RobotDefeatCargoResolved | Gadgeteer | voice.gadgeteer (6) | F | 107/21094 (0.51%) |
| RobotDefeated | Cautious | voice.general (12) | F | 100/20561 (0.49%) |
| RobotDefeated | Hoarder | voice.hoarder (7) | F | 102/20688 (0.49%) |
| RobotDefeated | Brawler | voice.brawler (6) | F | 99/20634 (0.48%) |
| RobotDefeated | Gadgeteer | voice.gadgeteer (6) | F | 107/21094 (0.51%) |
| RobotEscaped | Cautious | voice.general (12) | F | 61/20561 (0.30%) |
| RobotEscaped | Hoarder | voice.hoarder (7) | F | 55/20688 (0.27%) |
| RobotEscaped | Brawler | voice.brawler (6) | F | 56/20634 (0.27%) |
| RobotEscaped | Gadgeteer | voice.gadgeteer (6) | F | 50/21094 (0.24%) |
| RobotParticipantJoined | Cautious | voice.robot (11) | E | 161/20561 (0.78%) |
| RobotParticipantJoined | Hoarder | voice.robot (11) | E | 157/20688 (0.76%) |
| RobotParticipantJoined | Brawler | voice.robot (11) | E | 155/20634 (0.75%) |
| RobotParticipantJoined | Gadgeteer | voice.robot (11) | E | 157/21094 (0.74%) |
| RobotRound | Cautious | voice.robot (11) | E | 334/20561 (1.62%) |
| RobotRound | Hoarder | voice.robot (11) | E | 326/20688 (1.58%) |
| RobotRound | Brawler | voice.robot (11) | E | 321/20634 (1.56%) |
| RobotRound | Gadgeteer | voice.robot (11) | E | 313/21094 (1.48%) |
| Search | Cautious | voice.search (12) | E | 313/20561 (1.52%) |
| Search | Hoarder | voice.search (12) | E | 307/20688 (1.48%) |
| Search | Brawler | voice.search (12) | E | 314/20634 (1.52%) |
| Search | Gadgeteer | voice.search (12) | E | 313/21094 (1.48%) |
| SearchCompleted | Cautious | voice.general (12) | F | 171/20561 (0.83%) |
| SearchCompleted | Hoarder | voice.hoarder (7) | F | 169/20688 (0.82%) |
| SearchCompleted | Brawler | voice.brawler (6) | F | 170/20634 (0.82%) |
| SearchCompleted | Gadgeteer | voice.gadgeteer (6) | F | 167/21094 (0.79%) |
| SharedExtractionCalled | Cautious | voice.general (12) | F | 157/20561 (0.76%) |
| SharedExtractionCalled | Hoarder | voice.hoarder (7) | F | 154/20688 (0.74%) |
| SharedExtractionCalled | Brawler | voice.brawler (6) | F | 155/20634 (0.75%) |
| SharedExtractionCalled | Gadgeteer | voice.gadgeteer (6) | F | 157/21094 (0.74%) |
| ShieldAbsorbed | Cautious | voice.general (12) | F | 579/20561 (2.82%) |
| ShieldAbsorbed | Hoarder | voice.hoarder (7) | F | 564/20688 (2.73%) |
| ShieldAbsorbed | Brawler | voice.brawler (6) | F | 549/20634 (2.66%) |
| ShieldAbsorbed | Gadgeteer | voice.gadgeteer (6) | F | 580/21094 (2.75%) |
| Travel | Cautious | voice.travel (13) | E | 782/20561 (3.80%) |
| Travel | Hoarder | voice.travel (13) | E | 767/20688 (3.71%) |
| Travel | Brawler | voice.travel (13) | E | 774/20634 (3.75%) |
| Travel | Gadgeteer | voice.travel (13) | E | 778/21094 (3.69%) |
| UnmitigatedDamage | Cautious | voice.general (12) | F | 14/20561 (0.07%) |
| UnmitigatedDamage | Hoarder | voice.hoarder (7) | F | 20/20688 (0.10%) |
| UnmitigatedDamage | Brawler | voice.brawler (6) | F | 38/20634 (0.18%) |
| UnmitigatedDamage | Gadgeteer | voice.gadgeteer (6) | F | 20/21094 (0.09%) |
| WorldCondition | Cautious | voice.condition (6) | E | 171/20561 (0.83%) |
| WorldCondition | Hoarder | voice.condition (6) | E | 169/20688 (0.82%) |
| WorldCondition | Brawler | voice.condition (6) | E | 170/20634 (0.82%) |
| WorldCondition | Gadgeteer | voice.condition (6) | E | 167/21094 (0.79%) |

### Before/after by event kind

| Trait | Hours | ID | Before count/share | After count/share | Before R20 / R50 | After R20 / R50 |
|---|---:|---|---|---|---|---|
| Cautious | 8 | AreaDiary | 1709/6619 (25.82%) | 1709/6619 (25.82%) | 2/1709 (0.12%) / 37/1709 (2.17%) | 0/1709 (0.00%) / 30/1709 (1.76%) |
| Cautious | 8 | PersonalityChanged | 1077/6619 (16.27%) | 1077/6619 (16.27%) | 102/1077 (9.47%) / 1066/1077 (98.98%) | 12/1077 (1.11%) / 613/1077 (56.92%) |
| Cautious | 8 | AreaActivityStarted | 635/6619 (9.59%) | 635/6619 (9.59%) | 0/635 (0.00%) / 0/635 (0.00%) | 0/635 (0.00%) / 0/635 (0.00%) |
| Cautious | 8 | AreaActivityCompleted | 584/6619 (8.82%) | 584/6619 (8.82%) | 0/584 (0.00%) / 0/584 (0.00%) | 0/584 (0.00%) / 0/584 (0.00%) |
| Cautious | 8 | ItemAllocated | 270/6619 (4.08%) | 270/6619 (4.08%) | 0/270 (0.00%) / 0/270 (0.00%) | 0/270 (0.00%) / 0/270 (0.00%) |
| Cautious | 8 | Travel | 259/6619 (3.91%) | 259/6619 (3.91%) | 0/259 (0.00%) / 0/259 (0.00%) | 0/259 (0.00%) / 0/259 (0.00%) |
| Cautious | 8 | Arrived | 259/6619 (3.91%) | 259/6619 (3.91%) | 24/259 (9.27%) / 255/259 (98.46%) | 0/259 (0.00%) / 1/259 (0.39%) |
| Cautious | 8 | ShieldAbsorbed | 186/6619 (2.81%) | 186/6619 (2.81%) | 21/186 (11.29%) / 184/186 (98.92%) | 6/186 (3.23%) / 136/186 (73.12%) |
| Cautious | 8 | ExtractionDecision | 152/6619 (2.30%) | 152/6619 (2.30%) | 11/152 (7.24%) / 152/152 (100.00%) | 41/152 (26.97%) / 42/152 (27.63%) |
| Cautious | 8 | PackingStarted | 135/6619 (2.04%) | 135/6619 (2.04%) | 21/135 (15.56%) / 132/135 (97.78%) | 0/135 (0.00%) / 0/135 (0.00%) |
| Cautious | 8 | PackingCompleted | 135/6619 (2.04%) | 135/6619 (2.04%) | 38/135 (28.15%) / 132/135 (97.78%) | 0/135 (0.00%) / 0/135 (0.00%) |
| Cautious | 8 | RobotRound | 113/6619 (1.71%) | 113/6619 (1.71%) | 0/113 (0.00%) / 0/113 (0.00%) | 0/113 (0.00%) / 0/113 (0.00%) |
| Cautious | 8 | Search | 105/6619 (1.59%) | 105/6619 (1.59%) | 0/105 (0.00%) / 0/105 (0.00%) | 0/105 (0.00%) / 0/105 (0.00%) |
| Cautious | 8 | Resilience | 85/6619 (1.28%) | 85/6619 (1.28%) | 9/85 (10.59%) / 82/85 (96.47%) | 1/85 (1.18%) / 51/85 (60.00%) |
| Cautious | 8 | ConditionImpact | 80/6619 (1.21%) | 80/6619 (1.21%) | 10/80 (12.50%) / 78/80 (97.50%) | 1/80 (1.25%) / 48/80 (60.00%) |
| Cautious | 8 | HazardWarning | 76/6619 (1.15%) | 76/6619 (1.15%) | 5/76 (6.58%) / 74/76 (97.37%) | 0/76 (0.00%) / 45/76 (59.21%) |
| Cautious | 8 | ConsumableUsed | 62/6619 (0.94%) | 62/6619 (0.94%) | 0/62 (0.00%) / 0/62 (0.00%) | 0/62 (0.00%) / 0/62 (0.00%) |
| Cautious | 8 | DutyAdmission | 59/6619 (0.89%) | 59/6619 (0.89%) | 21/59 (35.59%) / 56/59 (94.92%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Cautious | 8 | WorldCondition | 59/6619 (0.89%) | 59/6619 (0.89%) | 0/59 (0.00%) / 0/59 (0.00%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Cautious | 8 | SearchCompleted | 59/6619 (0.89%) | 59/6619 (0.89%) | 31/59 (52.54%) / 56/59 (94.92%) | 1/59 (1.69%) / 38/59 (64.41%) |
| Cautious | 8 | RobotCombatStarted | 53/6619 (0.80%) | 53/6619 (0.80%) | 0/53 (0.00%) / 0/53 (0.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Cautious | 8 | RobotParticipantJoined | 53/6619 (0.80%) | 53/6619 (0.80%) | 0/53 (0.00%) / 0/53 (0.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Cautious | 8 | ExtractionStarted | 53/6619 (0.80%) | 53/6619 (0.80%) | 0/53 (0.00%) / 0/53 (0.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Cautious | 8 | SharedExtractionCalled | 53/6619 (0.80%) | 53/6619 (0.80%) | 10/53 (18.87%) / 53/53 (100.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Cautious | 8 | ActivityInterrupted | 52/6619 (0.79%) | 52/6619 (0.79%) | 7/52 (13.46%) / 52/52 (100.00%) | 1/52 (1.92%) / 30/52 (57.69%) |
| Cautious | 8 | ExtractionVehicleArrived | 48/6619 (0.73%) | 48/6619 (0.73%) | 12/48 (25.00%) / 48/48 (100.00%) | 0/48 (0.00%) / 0/48 (0.00%) |
| Cautious | 8 | Extracted | 48/6619 (0.73%) | 48/6619 (0.73%) | 0/48 (0.00%) / 0/48 (0.00%) | 0/48 (0.00%) / 0/48 (0.00%) |
| Cautious | 8 | EncounterResolved | 46/6619 (0.69%) | 46/6619 (0.69%) | 0/46 (0.00%) / 46/46 (100.00%) | 0/46 (0.00%) / 29/46 (63.04%) |
| Cautious | 8 | RobotDefeated | 30/6619 (0.45%) | 30/6619 (0.45%) | 0/30 (0.00%) / 30/30 (100.00%) | 0/30 (0.00%) / 0/30 (0.00%) |
| Cautious | 8 | RobotDefeatCargoResolved | 30/6619 (0.45%) | 30/6619 (0.45%) | 2/30 (6.67%) / 30/30 (100.00%) | 0/30 (0.00%) / 19/30 (63.33%) |
| Cautious | 8 | RobotEscaped | 23/6619 (0.35%) | 23/6619 (0.35%) | 4/23 (17.39%) / 23/23 (100.00%) | 0/23 (0.00%) / 0/23 (0.00%) |
| Cautious | 8 | Downed | 8/6619 (0.12%) | 8/6619 (0.12%) | 7/8 (87.50%) / 8/8 (100.00%) | 2/8 (25.00%) / 8/8 (100.00%) |
| Cautious | 8 | KnockedOut | 8/6619 (0.12%) | 8/6619 (0.12%) | 8/8 (100.00%) / 8/8 (100.00%) | 2/8 (25.00%) / 8/8 (100.00%) |
| Cautious | 8 | HiddenPocketEmpty | 8/6619 (0.12%) | 8/6619 (0.12%) | 8/8 (100.00%) / 8/8 (100.00%) | 4/8 (50.00%) / 8/8 (100.00%) |
| Cautious | 8 | UnmitigatedDamage | 7/6619 (0.11%) | 7/6619 (0.11%) | 3/7 (42.86%) / 7/7 (100.00%) | 0/7 (0.00%) / 7/7 (100.00%) |
| Cautious | 24 | AreaDiary | 5274/20561 (25.65%) | 5274/20561 (25.65%) | 8/5274 (0.15%) / 86/5274 (1.63%) | 0/5274 (0.00%) / 71/5274 (1.35%) |
| Cautious | 24 | PersonalityChanged | 3357/20561 (16.33%) | 3357/20561 (16.33%) | 335/3357 (9.98%) / 3346/3357 (99.67%) | 53/3357 (1.58%) / 2023/3357 (60.26%) |
| Cautious | 24 | AreaActivityStarted | 1957/20561 (9.52%) | 1957/20561 (9.52%) | 0/1957 (0.00%) / 0/1957 (0.00%) | 0/1957 (0.00%) / 0/1957 (0.00%) |
| Cautious | 24 | AreaActivityCompleted | 1810/20561 (8.80%) | 1810/20561 (8.80%) | 0/1810 (0.00%) / 0/1810 (0.00%) | 0/1810 (0.00%) / 0/1810 (0.00%) |
| Cautious | 24 | ItemAllocated | 826/20561 (4.02%) | 826/20561 (4.02%) | 0/826 (0.00%) / 0/826 (0.00%) | 0/826 (0.00%) / 0/826 (0.00%) |
| Cautious | 24 | Travel | 782/20561 (3.80%) | 782/20561 (3.80%) | 0/782 (0.00%) / 0/782 (0.00%) | 0/782 (0.00%) / 0/782 (0.00%) |
| Cautious | 24 | Arrived | 781/20561 (3.80%) | 781/20561 (3.80%) | 75/781 (9.60%) / 777/781 (99.49%) | 0/781 (0.00%) / 1/781 (0.13%) |
| Cautious | 24 | ShieldAbsorbed | 579/20561 (2.82%) | 579/20561 (2.82%) | 79/579 (13.64%) / 577/579 (99.65%) | 26/579 (4.49%) / 468/579 (80.83%) |
| Cautious | 24 | ExtractionDecision | 456/20561 (2.22%) | 456/20561 (2.22%) | 40/456 (8.77%) / 456/456 (100.00%) | 124/456 (27.19%) / 126/456 (27.63%) |
| Cautious | 24 | PackingStarted | 413/20561 (2.01%) | 413/20561 (2.01%) | 61/413 (14.77%) / 410/413 (99.27%) | 0/413 (0.00%) / 0/413 (0.00%) |
| Cautious | 24 | PackingCompleted | 413/20561 (2.01%) | 413/20561 (2.01%) | 116/413 (28.09%) / 410/413 (99.27%) | 0/413 (0.00%) / 0/413 (0.00%) |
| Cautious | 24 | Resilience | 411/20561 (2.00%) | 411/20561 (2.00%) | 35/411 (8.52%) / 408/411 (99.27%) | 6/411 (1.46%) / 281/411 (68.37%) |
| Cautious | 24 | RobotRound | 334/20561 (1.62%) | 334/20561 (1.62%) | 0/334 (0.00%) / 0/334 (0.00%) | 0/334 (0.00%) / 0/334 (0.00%) |
| Cautious | 24 | Search | 313/20561 (1.52%) | 313/20561 (1.52%) | 0/313 (0.00%) / 0/313 (0.00%) | 0/313 (0.00%) / 0/313 (0.00%) |
| Cautious | 24 | ConsumableUsed | 269/20561 (1.31%) | 269/20561 (1.31%) | 0/269 (0.00%) / 0/269 (0.00%) | 0/269 (0.00%) / 0/269 (0.00%) |
| Cautious | 24 | ConditionImpact | 259/20561 (1.26%) | 259/20561 (1.26%) | 31/259 (11.97%) / 257/259 (99.23%) | 3/259 (1.16%) / 170/259 (65.64%) |
| Cautious | 24 | HazardWarning | 248/20561 (1.21%) | 248/20561 (1.21%) | 20/248 (8.06%) / 246/248 (99.19%) | 1/248 (0.40%) / 163/248 (65.73%) |
| Cautious | 24 | DutyAdmission | 171/20561 (0.83%) | 171/20561 (0.83%) | 65/171 (38.01%) / 168/171 (98.25%) | 0/171 (0.00%) / 0/171 (0.00%) |
| Cautious | 24 | WorldCondition | 171/20561 (0.83%) | 171/20561 (0.83%) | 0/171 (0.00%) / 0/171 (0.00%) | 0/171 (0.00%) / 0/171 (0.00%) |
| Cautious | 24 | SearchCompleted | 171/20561 (0.83%) | 171/20561 (0.83%) | 83/171 (48.54%) / 168/171 (98.25%) | 2/171 (1.17%) / 106/171 (61.99%) |
| Cautious | 24 | RobotCombatStarted | 161/20561 (0.78%) | 161/20561 (0.78%) | 0/161 (0.00%) / 0/161 (0.00%) | 0/161 (0.00%) / 0/161 (0.00%) |
| Cautious | 24 | RobotParticipantJoined | 161/20561 (0.78%) | 161/20561 (0.78%) | 0/161 (0.00%) / 0/161 (0.00%) | 0/161 (0.00%) / 0/161 (0.00%) |
| Cautious | 24 | ExtractionStarted | 157/20561 (0.76%) | 157/20561 (0.76%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Cautious | 24 | SharedExtractionCalled | 157/20561 (0.76%) | 157/20561 (0.76%) | 24/157 (15.29%) / 157/157 (100.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Cautious | 24 | ActivityInterrupted | 155/20561 (0.75%) | 155/20561 (0.75%) | 23/155 (14.84%) / 155/155 (100.00%) | 1/155 (0.65%) / 90/155 (58.06%) |
| Cautious | 24 | ExtractionVehicleArrived | 149/20561 (0.72%) | 149/20561 (0.72%) | 31/149 (20.81%) / 149/149 (100.00%) | 0/149 (0.00%) / 0/149 (0.00%) |
| Cautious | 24 | Extracted | 149/20561 (0.72%) | 149/20561 (0.72%) | 0/149 (0.00%) / 0/149 (0.00%) | 0/149 (0.00%) / 0/149 (0.00%) |
| Cautious | 24 | EncounterResolved | 142/20561 (0.69%) | 142/20561 (0.69%) | 0/142 (0.00%) / 142/142 (100.00%) | 0/142 (0.00%) / 80/142 (56.34%) |
| Cautious | 24 | RobotDefeated | 100/20561 (0.49%) | 100/20561 (0.49%) | 2/100 (2.00%) / 100/100 (100.00%) | 0/100 (0.00%) / 0/100 (0.00%) |
| Cautious | 24 | RobotDefeatCargoResolved | 100/20561 (0.49%) | 100/20561 (0.49%) | 5/100 (5.00%) / 100/100 (100.00%) | 0/100 (0.00%) / 61/100 (61.00%) |
| Cautious | 24 | RobotEscaped | 61/20561 (0.30%) | 61/20561 (0.30%) | 24/61 (39.34%) / 61/61 (100.00%) | 0/61 (0.00%) / 0/61 (0.00%) |
| Cautious | 24 | Downed | 20/20561 (0.10%) | 20/20561 (0.10%) | 18/20 (90.00%) / 20/20 (100.00%) | 7/20 (35.00%) / 20/20 (100.00%) |
| Cautious | 24 | KnockedOut | 20/20561 (0.10%) | 20/20561 (0.10%) | 19/20 (95.00%) / 20/20 (100.00%) | 8/20 (40.00%) / 20/20 (100.00%) |
| Cautious | 24 | HiddenPocketEmpty | 20/20561 (0.10%) | 20/20561 (0.10%) | 19/20 (95.00%) / 20/20 (100.00%) | 11/20 (55.00%) / 20/20 (100.00%) |
| Cautious | 24 | UnmitigatedDamage | 14/20561 (0.07%) | 14/20561 (0.07%) | 7/14 (50.00%) / 14/14 (100.00%) | 0/14 (0.00%) / 14/14 (100.00%) |
| Hoarder | 8 | AreaDiary | 1697/6563 (25.86%) | 1697/6563 (25.86%) | 5/1697 (0.29%) / 41/1697 (2.42%) | 0/1697 (0.00%) / 29/1697 (1.71%) |
| Hoarder | 8 | PersonalityChanged | 1066/6563 (16.24%) | 1066/6563 (16.24%) | 666/1066 (62.48%) / 1060/1066 (99.44%) | 281/1066 (26.36%) / 1055/1066 (98.97%) |
| Hoarder | 8 | AreaActivityStarted | 627/6563 (9.55%) | 627/6563 (9.55%) | 0/627 (0.00%) / 0/627 (0.00%) | 0/627 (0.00%) / 0/627 (0.00%) |
| Hoarder | 8 | AreaActivityCompleted | 578/6563 (8.81%) | 578/6563 (8.81%) | 0/578 (0.00%) / 0/578 (0.00%) | 0/578 (0.00%) / 0/578 (0.00%) |
| Hoarder | 8 | ItemAllocated | 264/6563 (4.02%) | 264/6563 (4.02%) | 0/264 (0.00%) / 0/264 (0.00%) | 0/264 (0.00%) / 0/264 (0.00%) |
| Hoarder | 8 | Travel | 250/6563 (3.81%) | 250/6563 (3.81%) | 0/250 (0.00%) / 0/250 (0.00%) | 0/250 (0.00%) / 0/250 (0.00%) |
| Hoarder | 8 | Arrived | 250/6563 (3.81%) | 250/6563 (3.81%) | 100/250 (40.00%) / 247/250 (98.80%) | 0/250 (0.00%) / 0/250 (0.00%) |
| Hoarder | 8 | ShieldAbsorbed | 181/6563 (2.76%) | 181/6563 (2.76%) | 128/181 (70.72%) / 180/181 (99.45%) | 88/181 (48.62%) / 180/181 (99.45%) |
| Hoarder | 8 | ExtractionDecision | 164/6563 (2.50%) | 164/6563 (2.50%) | 99/164 (60.37%) / 164/164 (100.00%) | 54/164 (32.93%) / 56/164 (34.15%) |
| Hoarder | 8 | PackingStarted | 132/6563 (2.01%) | 132/6563 (2.01%) | 91/132 (68.94%) / 130/132 (98.48%) | 0/132 (0.00%) / 0/132 (0.00%) |
| Hoarder | 8 | PackingCompleted | 132/6563 (2.01%) | 132/6563 (2.01%) | 95/132 (71.97%) / 130/132 (98.48%) | 0/132 (0.00%) / 0/132 (0.00%) |
| Hoarder | 8 | Resilience | 111/6563 (1.69%) | 111/6563 (1.69%) | 61/111 (54.95%) / 110/111 (99.10%) | 40/111 (36.04%) / 108/111 (97.30%) |
| Hoarder | 8 | RobotRound | 109/6563 (1.66%) | 109/6563 (1.66%) | 0/109 (0.00%) / 0/109 (0.00%) | 0/109 (0.00%) / 0/109 (0.00%) |
| Hoarder | 8 | Search | 101/6563 (1.54%) | 101/6563 (1.54%) | 0/101 (0.00%) / 0/101 (0.00%) | 0/101 (0.00%) / 0/101 (0.00%) |
| Hoarder | 8 | ConditionImpact | 82/6563 (1.25%) | 82/6563 (1.25%) | 60/82 (73.17%) / 81/82 (98.78%) | 32/82 (39.02%) / 80/82 (97.56%) |
| Hoarder | 8 | HazardWarning | 74/6563 (1.13%) | 74/6563 (1.13%) | 49/74 (66.22%) / 74/74 (100.00%) | 29/74 (39.19%) / 73/74 (98.65%) |
| Hoarder | 8 | ConsumableUsed | 60/6563 (0.91%) | 60/6563 (0.91%) | 0/60 (0.00%) / 0/60 (0.00%) | 0/60 (0.00%) / 0/60 (0.00%) |
| Hoarder | 8 | DutyAdmission | 58/6563 (0.88%) | 58/6563 (0.88%) | 55/58 (94.83%) / 55/58 (94.83%) | 0/58 (0.00%) / 0/58 (0.00%) |
| Hoarder | 8 | WorldCondition | 58/6563 (0.88%) | 58/6563 (0.88%) | 0/58 (0.00%) / 0/58 (0.00%) | 0/58 (0.00%) / 0/58 (0.00%) |
| Hoarder | 8 | SearchCompleted | 58/6563 (0.88%) | 58/6563 (0.88%) | 56/58 (96.55%) / 56/58 (96.55%) | 26/58 (44.83%) / 55/58 (94.83%) |
| Hoarder | 8 | RobotCombatStarted | 51/6563 (0.78%) | 51/6563 (0.78%) | 0/51 (0.00%) / 0/51 (0.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Hoarder | 8 | RobotParticipantJoined | 51/6563 (0.78%) | 51/6563 (0.78%) | 0/51 (0.00%) / 0/51 (0.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Hoarder | 8 | ExtractionStarted | 51/6563 (0.78%) | 51/6563 (0.78%) | 0/51 (0.00%) / 0/51 (0.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Hoarder | 8 | SharedExtractionCalled | 51/6563 (0.78%) | 51/6563 (0.78%) | 44/51 (86.27%) / 51/51 (100.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Hoarder | 8 | ActivityInterrupted | 50/6563 (0.76%) | 50/6563 (0.76%) | 40/50 (80.00%) / 50/50 (100.00%) | 19/50 (38.00%) / 50/50 (100.00%) |
| Hoarder | 8 | ExtractionVehicleArrived | 49/6563 (0.75%) | 49/6563 (0.75%) | 48/49 (97.96%) / 49/49 (100.00%) | 0/49 (0.00%) / 0/49 (0.00%) |
| Hoarder | 8 | Extracted | 49/6563 (0.75%) | 49/6563 (0.75%) | 0/49 (0.00%) / 0/49 (0.00%) | 0/49 (0.00%) / 0/49 (0.00%) |
| Hoarder | 8 | EncounterResolved | 43/6563 (0.66%) | 43/6563 (0.66%) | 5/43 (11.63%) / 43/43 (100.00%) | 4/43 (9.30%) / 43/43 (100.00%) |
| Hoarder | 8 | RobotDefeated | 31/6563 (0.47%) | 31/6563 (0.47%) | 19/31 (61.29%) / 31/31 (100.00%) | 0/31 (0.00%) / 0/31 (0.00%) |
| Hoarder | 8 | RobotDefeatCargoResolved | 31/6563 (0.47%) | 31/6563 (0.47%) | 30/31 (96.77%) / 31/31 (100.00%) | 13/31 (41.94%) / 31/31 (100.00%) |
| Hoarder | 8 | RobotEscaped | 20/6563 (0.30%) | 20/6563 (0.30%) | 20/20 (100.00%) / 20/20 (100.00%) | 0/20 (0.00%) / 0/20 (0.00%) |
| Hoarder | 8 | UnmitigatedDamage | 10/6563 (0.15%) | 10/6563 (0.15%) | 8/10 (80.00%) / 10/10 (100.00%) | 8/10 (80.00%) / 10/10 (100.00%) |
| Hoarder | 8 | Downed | 8/6563 (0.12%) | 8/6563 (0.12%) | 8/8 (100.00%) / 8/8 (100.00%) | 8/8 (100.00%) / 8/8 (100.00%) |
| Hoarder | 8 | KnockedOut | 8/6563 (0.12%) | 8/6563 (0.12%) | 8/8 (100.00%) / 8/8 (100.00%) | 8/8 (100.00%) / 8/8 (100.00%) |
| Hoarder | 8 | HiddenPocketEmpty | 8/6563 (0.12%) | 8/6563 (0.12%) | 8/8 (100.00%) / 8/8 (100.00%) | 8/8 (100.00%) / 8/8 (100.00%) |
| Hoarder | 24 | AreaDiary | 5353/20688 (25.87%) | 5353/20688 (25.87%) | 8/5353 (0.15%) / 96/5353 (1.79%) | 0/5353 (0.00%) / 77/5353 (1.44%) |
| Hoarder | 24 | PersonalityChanged | 3394/20688 (16.41%) | 3394/20688 (16.41%) | 2198/3394 (64.76%) / 3388/3394 (99.82%) | 1035/3394 (30.49%) / 3383/3394 (99.68%) |
| Hoarder | 24 | AreaActivityStarted | 1987/20688 (9.60%) | 1987/20688 (9.60%) | 0/1987 (0.00%) / 0/1987 (0.00%) | 0/1987 (0.00%) / 0/1987 (0.00%) |
| Hoarder | 24 | AreaActivityCompleted | 1840/20688 (8.89%) | 1840/20688 (8.89%) | 0/1840 (0.00%) / 0/1840 (0.00%) | 0/1840 (0.00%) / 0/1840 (0.00%) |
| Hoarder | 24 | ItemAllocated | 818/20688 (3.95%) | 818/20688 (3.95%) | 0/818 (0.00%) / 0/818 (0.00%) | 0/818 (0.00%) / 0/818 (0.00%) |
| Hoarder | 24 | Travel | 767/20688 (3.71%) | 767/20688 (3.71%) | 0/767 (0.00%) / 0/767 (0.00%) | 0/767 (0.00%) / 0/767 (0.00%) |
| Hoarder | 24 | Arrived | 767/20688 (3.71%) | 767/20688 (3.71%) | 306/767 (39.90%) / 764/767 (99.61%) | 0/767 (0.00%) / 0/767 (0.00%) |
| Hoarder | 24 | ShieldAbsorbed | 564/20688 (2.73%) | 564/20688 (2.73%) | 411/564 (72.87%) / 563/564 (99.82%) | 293/564 (51.95%) / 563/564 (99.82%) |
| Hoarder | 24 | ExtractionDecision | 472/20688 (2.28%) | 472/20688 (2.28%) | 294/472 (62.29%) / 472/472 (100.00%) | 136/472 (28.81%) / 138/472 (29.24%) |
| Hoarder | 24 | Resilience | 452/20688 (2.18%) | 452/20688 (2.18%) | 255/452 (56.42%) / 451/452 (99.78%) | 156/452 (34.51%) / 449/452 (99.34%) |
| Hoarder | 24 | PackingStarted | 409/20688 (1.98%) | 409/20688 (1.98%) | 289/409 (70.66%) / 407/409 (99.51%) | 0/409 (0.00%) / 0/409 (0.00%) |
| Hoarder | 24 | PackingCompleted | 409/20688 (1.98%) | 409/20688 (1.98%) | 310/409 (75.79%) / 407/409 (99.51%) | 0/409 (0.00%) / 0/409 (0.00%) |
| Hoarder | 24 | RobotRound | 326/20688 (1.58%) | 326/20688 (1.58%) | 0/326 (0.00%) / 0/326 (0.00%) | 0/326 (0.00%) / 0/326 (0.00%) |
| Hoarder | 24 | Search | 307/20688 (1.48%) | 307/20688 (1.48%) | 0/307 (0.00%) / 0/307 (0.00%) | 0/307 (0.00%) / 0/307 (0.00%) |
| Hoarder | 24 | ConsumableUsed | 274/20688 (1.32%) | 274/20688 (1.32%) | 0/274 (0.00%) / 0/274 (0.00%) | 0/274 (0.00%) / 0/274 (0.00%) |
| Hoarder | 24 | ConditionImpact | 258/20688 (1.25%) | 258/20688 (1.25%) | 189/258 (73.26%) / 257/258 (99.61%) | 78/258 (30.23%) / 256/258 (99.22%) |
| Hoarder | 24 | HazardWarning | 237/20688 (1.15%) | 237/20688 (1.15%) | 167/237 (70.46%) / 237/237 (100.00%) | 79/237 (33.33%) / 236/237 (99.58%) |
| Hoarder | 24 | DutyAdmission | 169/20688 (0.82%) | 169/20688 (0.82%) | 166/169 (98.22%) / 166/169 (98.22%) | 0/169 (0.00%) / 0/169 (0.00%) |
| Hoarder | 24 | WorldCondition | 169/20688 (0.82%) | 169/20688 (0.82%) | 0/169 (0.00%) / 0/169 (0.00%) | 0/169 (0.00%) / 0/169 (0.00%) |
| Hoarder | 24 | SearchCompleted | 169/20688 (0.82%) | 169/20688 (0.82%) | 167/169 (98.82%) / 167/169 (98.82%) | 69/169 (40.83%) / 166/169 (98.22%) |
| Hoarder | 24 | RobotCombatStarted | 157/20688 (0.76%) | 157/20688 (0.76%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Hoarder | 24 | RobotParticipantJoined | 157/20688 (0.76%) | 157/20688 (0.76%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Hoarder | 24 | ExtractionStarted | 154/20688 (0.74%) | 154/20688 (0.74%) | 0/154 (0.00%) / 0/154 (0.00%) | 0/154 (0.00%) / 0/154 (0.00%) |
| Hoarder | 24 | SharedExtractionCalled | 154/20688 (0.74%) | 154/20688 (0.74%) | 128/154 (83.12%) / 154/154 (100.00%) | 0/154 (0.00%) / 0/154 (0.00%) |
| Hoarder | 24 | ActivityInterrupted | 153/20688 (0.74%) | 153/20688 (0.74%) | 112/153 (73.20%) / 153/153 (100.00%) | 57/153 (37.25%) / 153/153 (100.00%) |
| Hoarder | 24 | ExtractionVehicleArrived | 148/20688 (0.72%) | 148/20688 (0.72%) | 147/148 (99.32%) / 148/148 (100.00%) | 0/148 (0.00%) / 0/148 (0.00%) |
| Hoarder | 24 | Extracted | 147/20688 (0.71%) | 147/20688 (0.71%) | 0/147 (0.00%) / 0/147 (0.00%) | 0/147 (0.00%) / 0/147 (0.00%) |
| Hoarder | 24 | EncounterResolved | 138/20688 (0.67%) | 138/20688 (0.67%) | 23/138 (16.67%) / 138/138 (100.00%) | 21/138 (15.22%) / 138/138 (100.00%) |
| Hoarder | 24 | RobotDefeated | 102/20688 (0.49%) | 102/20688 (0.49%) | 50/102 (49.02%) / 102/102 (100.00%) | 0/102 (0.00%) / 0/102 (0.00%) |
| Hoarder | 24 | RobotDefeatCargoResolved | 102/20688 (0.49%) | 102/20688 (0.49%) | 90/102 (88.24%) / 102/102 (100.00%) | 38/102 (37.25%) / 102/102 (100.00%) |
| Hoarder | 24 | RobotEscaped | 55/20688 (0.27%) | 55/20688 (0.27%) | 55/55 (100.00%) / 55/55 (100.00%) | 0/55 (0.00%) / 0/55 (0.00%) |
| Hoarder | 24 | UnmitigatedDamage | 20/20688 (0.10%) | 20/20688 (0.10%) | 18/20 (90.00%) / 20/20 (100.00%) | 16/20 (80.00%) / 20/20 (100.00%) |
| Hoarder | 24 | Downed | 20/20688 (0.10%) | 20/20688 (0.10%) | 20/20 (100.00%) / 20/20 (100.00%) | 20/20 (100.00%) / 20/20 (100.00%) |
| Hoarder | 24 | KnockedOut | 20/20688 (0.10%) | 20/20688 (0.10%) | 20/20 (100.00%) / 20/20 (100.00%) | 20/20 (100.00%) / 20/20 (100.00%) |
| Hoarder | 24 | HiddenPocketEmpty | 20/20688 (0.10%) | 20/20688 (0.10%) | 20/20 (100.00%) / 20/20 (100.00%) | 20/20 (100.00%) / 20/20 (100.00%) |
| Brawler | 8 | AreaDiary | 1701/6673 (25.49%) | 1701/6673 (25.49%) | 5/1701 (0.29%) / 41/1701 (2.41%) | 0/1701 (0.00%) / 29/1701 (1.70%) |
| Brawler | 8 | PersonalityChanged | 1093/6673 (16.38%) | 1093/6673 (16.38%) | 801/1093 (73.28%) / 1088/1093 (99.54%) | 414/1093 (37.88%) / 1083/1093 (99.09%) |
| Brawler | 8 | AreaActivityStarted | 631/6673 (9.46%) | 631/6673 (9.46%) | 0/631 (0.00%) / 0/631 (0.00%) | 0/631 (0.00%) / 0/631 (0.00%) |
| Brawler | 8 | AreaActivityCompleted | 586/6673 (8.78%) | 586/6673 (8.78%) | 0/586 (0.00%) / 0/586 (0.00%) | 0/586 (0.00%) / 0/586 (0.00%) |
| Brawler | 8 | ItemAllocated | 260/6673 (3.90%) | 260/6673 (3.90%) | 0/260 (0.00%) / 0/260 (0.00%) | 0/260 (0.00%) / 0/260 (0.00%) |
| Brawler | 8 | Travel | 255/6673 (3.82%) | 255/6673 (3.82%) | 0/255 (0.00%) / 0/255 (0.00%) | 0/255 (0.00%) / 0/255 (0.00%) |
| Brawler | 8 | Arrived | 255/6673 (3.82%) | 255/6673 (3.82%) | 112/255 (43.92%) / 252/255 (98.82%) | 0/255 (0.00%) / 1/255 (0.39%) |
| Brawler | 8 | ShieldAbsorbed | 183/6673 (2.74%) | 183/6673 (2.74%) | 166/183 (90.71%) / 182/183 (99.45%) | 129/183 (70.49%) / 182/183 (99.45%) |
| Brawler | 8 | ExtractionDecision | 164/6673 (2.46%) | 164/6673 (2.46%) | 116/164 (70.73%) / 164/164 (100.00%) | 55/164 (33.54%) / 58/164 (35.37%) |
| Brawler | 8 | Resilience | 133/6673 (1.99%) | 133/6673 (1.99%) | 100/133 (75.19%) / 132/133 (99.25%) | 58/133 (43.61%) / 131/133 (98.50%) |
| Brawler | 8 | PackingStarted | 130/6673 (1.95%) | 130/6673 (1.95%) | 95/130 (73.08%) / 128/130 (98.46%) | 0/130 (0.00%) / 0/130 (0.00%) |
| Brawler | 8 | PackingCompleted | 130/6673 (1.95%) | 130/6673 (1.95%) | 130/130 (100.00%) / 130/130 (100.00%) | 0/130 (0.00%) / 0/130 (0.00%) |
| Brawler | 8 | RobotRound | 117/6673 (1.75%) | 117/6673 (1.75%) | 0/117 (0.00%) / 0/117 (0.00%) | 0/117 (0.00%) / 0/117 (0.00%) |
| Brawler | 8 | Search | 104/6673 (1.56%) | 104/6673 (1.56%) | 0/104 (0.00%) / 0/104 (0.00%) | 0/104 (0.00%) / 0/104 (0.00%) |
| Brawler | 8 | ConditionImpact | 80/6673 (1.20%) | 80/6673 (1.20%) | 63/80 (78.75%) / 79/80 (98.75%) | 30/80 (37.50%) / 78/80 (97.50%) |
| Brawler | 8 | HazardWarning | 77/6673 (1.15%) | 77/6673 (1.15%) | 65/77 (84.42%) / 77/77 (100.00%) | 30/77 (38.96%) / 76/77 (98.70%) |
| Brawler | 8 | ConsumableUsed | 77/6673 (1.15%) | 77/6673 (1.15%) | 0/77 (0.00%) / 0/77 (0.00%) | 0/77 (0.00%) / 0/77 (0.00%) |
| Brawler | 8 | DutyAdmission | 59/6673 (0.88%) | 59/6673 (0.88%) | 56/59 (94.92%) / 56/59 (94.92%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Brawler | 8 | WorldCondition | 59/6673 (0.88%) | 59/6673 (0.88%) | 0/59 (0.00%) / 0/59 (0.00%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Brawler | 8 | SearchCompleted | 59/6673 (0.88%) | 59/6673 (0.88%) | 57/59 (96.61%) / 57/59 (96.61%) | 26/59 (44.07%) / 57/59 (96.61%) |
| Brawler | 8 | ActivityInterrupted | 52/6673 (0.78%) | 52/6673 (0.78%) | 49/52 (94.23%) / 52/52 (100.00%) | 24/52 (46.15%) / 52/52 (100.00%) |
| Brawler | 8 | ExtractionStarted | 52/6673 (0.78%) | 52/6673 (0.78%) | 0/52 (0.00%) / 0/52 (0.00%) | 0/52 (0.00%) / 0/52 (0.00%) |
| Brawler | 8 | SharedExtractionCalled | 52/6673 (0.78%) | 52/6673 (0.78%) | 52/52 (100.00%) / 52/52 (100.00%) | 0/52 (0.00%) / 0/52 (0.00%) |
| Brawler | 8 | RobotCombatStarted | 51/6673 (0.76%) | 51/6673 (0.76%) | 0/51 (0.00%) / 0/51 (0.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Brawler | 8 | RobotParticipantJoined | 51/6673 (0.76%) | 51/6673 (0.76%) | 0/51 (0.00%) / 0/51 (0.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Brawler | 8 | ExtractionVehicleArrived | 51/6673 (0.76%) | 51/6673 (0.76%) | 51/51 (100.00%) / 51/51 (100.00%) | 0/51 (0.00%) / 0/51 (0.00%) |
| Brawler | 8 | Extracted | 48/6673 (0.72%) | 48/6673 (0.72%) | 0/48 (0.00%) / 0/48 (0.00%) | 0/48 (0.00%) / 0/48 (0.00%) |
| Brawler | 8 | EncounterResolved | 45/6673 (0.67%) | 45/6673 (0.67%) | 7/45 (15.56%) / 45/45 (100.00%) | 6/45 (13.33%) / 45/45 (100.00%) |
| Brawler | 8 | RobotDefeated | 26/6673 (0.39%) | 26/6673 (0.39%) | 24/26 (92.31%) / 26/26 (100.00%) | 0/26 (0.00%) / 0/26 (0.00%) |
| Brawler | 8 | RobotDefeatCargoResolved | 26/6673 (0.39%) | 26/6673 (0.39%) | 26/26 (100.00%) / 26/26 (100.00%) | 16/26 (61.54%) / 26/26 (100.00%) |
| Brawler | 8 | RobotEscaped | 25/6673 (0.37%) | 25/6673 (0.37%) | 25/25 (100.00%) / 25/25 (100.00%) | 0/25 (0.00%) / 0/25 (0.00%) |
| Brawler | 8 | UnmitigatedDamage | 14/6673 (0.21%) | 14/6673 (0.21%) | 12/14 (85.71%) / 14/14 (100.00%) | 11/14 (78.57%) / 14/14 (100.00%) |
| Brawler | 8 | Downed | 9/6673 (0.13%) | 9/6673 (0.13%) | 9/9 (100.00%) / 9/9 (100.00%) | 9/9 (100.00%) / 9/9 (100.00%) |
| Brawler | 8 | KnockedOut | 9/6673 (0.13%) | 9/6673 (0.13%) | 9/9 (100.00%) / 9/9 (100.00%) | 9/9 (100.00%) / 9/9 (100.00%) |
| Brawler | 8 | HiddenPocketEmpty | 9/6673 (0.13%) | 9/6673 (0.13%) | 9/9 (100.00%) / 9/9 (100.00%) | 9/9 (100.00%) / 9/9 (100.00%) |
| Brawler | 24 | AreaDiary | 5314/20634 (25.75%) | 5314/20634 (25.75%) | 8/5314 (0.15%) / 107/5314 (2.01%) | 0/5314 (0.00%) / 78/5314 (1.47%) |
| Brawler | 24 | PersonalityChanged | 3384/20634 (16.40%) | 3384/20634 (16.40%) | 2526/3384 (74.65%) / 3379/3384 (99.85%) | 1330/3384 (39.30%) / 3374/3384 (99.70%) |
| Brawler | 24 | AreaActivityStarted | 1965/20634 (9.52%) | 1965/20634 (9.52%) | 0/1965 (0.00%) / 0/1965 (0.00%) | 0/1965 (0.00%) / 0/1965 (0.00%) |
| Brawler | 24 | AreaActivityCompleted | 1827/20634 (8.85%) | 1827/20634 (8.85%) | 0/1827 (0.00%) / 0/1827 (0.00%) | 0/1827 (0.00%) / 0/1827 (0.00%) |
| Brawler | 24 | ItemAllocated | 824/20634 (3.99%) | 824/20634 (3.99%) | 0/824 (0.00%) / 0/824 (0.00%) | 0/824 (0.00%) / 0/824 (0.00%) |
| Brawler | 24 | Travel | 774/20634 (3.75%) | 774/20634 (3.75%) | 0/774 (0.00%) / 0/774 (0.00%) | 0/774 (0.00%) / 0/774 (0.00%) |
| Brawler | 24 | Arrived | 774/20634 (3.75%) | 774/20634 (3.75%) | 336/774 (43.41%) / 771/774 (99.61%) | 0/774 (0.00%) / 1/774 (0.13%) |
| Brawler | 24 | ShieldAbsorbed | 549/20634 (2.66%) | 549/20634 (2.66%) | 494/549 (89.98%) / 548/549 (99.82%) | 389/549 (70.86%) / 548/549 (99.82%) |
| Brawler | 24 | Resilience | 443/20634 (2.15%) | 443/20634 (2.15%) | 338/443 (76.30%) / 442/443 (99.77%) | 194/443 (43.79%) / 441/443 (99.55%) |
| Brawler | 24 | ExtractionDecision | 437/20634 (2.12%) | 437/20634 (2.12%) | 292/437 (66.82%) / 437/437 (100.00%) | 118/437 (27.00%) / 122/437 (27.92%) |
| Brawler | 24 | PackingStarted | 412/20634 (2.00%) | 412/20634 (2.00%) | 322/412 (78.16%) / 410/412 (99.51%) | 0/412 (0.00%) / 0/412 (0.00%) |
| Brawler | 24 | PackingCompleted | 412/20634 (2.00%) | 412/20634 (2.00%) | 412/412 (100.00%) / 412/412 (100.00%) | 0/412 (0.00%) / 0/412 (0.00%) |
| Brawler | 24 | RobotRound | 321/20634 (1.56%) | 321/20634 (1.56%) | 0/321 (0.00%) / 0/321 (0.00%) | 0/321 (0.00%) / 0/321 (0.00%) |
| Brawler | 24 | Search | 314/20634 (1.52%) | 314/20634 (1.52%) | 0/314 (0.00%) / 0/314 (0.00%) | 0/314 (0.00%) / 0/314 (0.00%) |
| Brawler | 24 | ConsumableUsed | 270/20634 (1.31%) | 270/20634 (1.31%) | 0/270 (0.00%) / 0/270 (0.00%) | 0/270 (0.00%) / 0/270 (0.00%) |
| Brawler | 24 | ConditionImpact | 266/20634 (1.29%) | 266/20634 (1.29%) | 201/266 (75.56%) / 265/266 (99.62%) | 98/266 (36.84%) / 264/266 (99.25%) |
| Brawler | 24 | HazardWarning | 255/20634 (1.24%) | 255/20634 (1.24%) | 199/255 (78.04%) / 255/255 (100.00%) | 93/255 (36.47%) / 254/255 (99.61%) |
| Brawler | 24 | DutyAdmission | 170/20634 (0.82%) | 170/20634 (0.82%) | 167/170 (98.24%) / 167/170 (98.24%) | 0/170 (0.00%) / 0/170 (0.00%) |
| Brawler | 24 | WorldCondition | 170/20634 (0.82%) | 170/20634 (0.82%) | 0/170 (0.00%) / 0/170 (0.00%) | 0/170 (0.00%) / 0/170 (0.00%) |
| Brawler | 24 | SearchCompleted | 170/20634 (0.82%) | 170/20634 (0.82%) | 168/170 (98.82%) / 168/170 (98.82%) | 80/170 (47.06%) / 168/170 (98.82%) |
| Brawler | 24 | RobotCombatStarted | 155/20634 (0.75%) | 155/20634 (0.75%) | 0/155 (0.00%) / 0/155 (0.00%) | 0/155 (0.00%) / 0/155 (0.00%) |
| Brawler | 24 | RobotParticipantJoined | 155/20634 (0.75%) | 155/20634 (0.75%) | 0/155 (0.00%) / 0/155 (0.00%) | 0/155 (0.00%) / 0/155 (0.00%) |
| Brawler | 24 | ActivityInterrupted | 155/20634 (0.75%) | 155/20634 (0.75%) | 143/155 (92.26%) / 155/155 (100.00%) | 67/155 (43.23%) / 155/155 (100.00%) |
| Brawler | 24 | ExtractionStarted | 155/20634 (0.75%) | 155/20634 (0.75%) | 0/155 (0.00%) / 0/155 (0.00%) | 0/155 (0.00%) / 0/155 (0.00%) |
| Brawler | 24 | SharedExtractionCalled | 155/20634 (0.75%) | 155/20634 (0.75%) | 155/155 (100.00%) / 155/155 (100.00%) | 0/155 (0.00%) / 0/155 (0.00%) |
| Brawler | 24 | ExtractionVehicleArrived | 153/20634 (0.74%) | 153/20634 (0.74%) | 153/153 (100.00%) / 153/153 (100.00%) | 0/153 (0.00%) / 0/153 (0.00%) |
| Brawler | 24 | EncounterResolved | 143/20634 (0.69%) | 143/20634 (0.69%) | 35/143 (24.48%) / 143/143 (100.00%) | 30/143 (20.98%) / 143/143 (100.00%) |
| Brawler | 24 | Extracted | 142/20634 (0.69%) | 142/20634 (0.69%) | 0/142 (0.00%) / 0/142 (0.00%) | 0/142 (0.00%) / 0/142 (0.00%) |
| Brawler | 24 | RobotDefeated | 99/20634 (0.48%) | 99/20634 (0.48%) | 87/99 (87.88%) / 99/99 (100.00%) | 0/99 (0.00%) / 0/99 (0.00%) |
| Brawler | 24 | RobotDefeatCargoResolved | 99/20634 (0.48%) | 99/20634 (0.48%) | 99/99 (100.00%) / 99/99 (100.00%) | 47/99 (47.47%) / 99/99 (100.00%) |
| Brawler | 24 | RobotEscaped | 56/20634 (0.27%) | 56/20634 (0.27%) | 56/56 (100.00%) / 56/56 (100.00%) | 0/56 (0.00%) / 0/56 (0.00%) |
| Brawler | 24 | UnmitigatedDamage | 38/20634 (0.18%) | 38/20634 (0.18%) | 33/38 (86.84%) / 38/38 (100.00%) | 29/38 (76.32%) / 38/38 (100.00%) |
| Brawler | 24 | Downed | 26/20634 (0.13%) | 26/20634 (0.13%) | 26/26 (100.00%) / 26/26 (100.00%) | 26/26 (100.00%) / 26/26 (100.00%) |
| Brawler | 24 | KnockedOut | 26/20634 (0.13%) | 26/20634 (0.13%) | 26/26 (100.00%) / 26/26 (100.00%) | 26/26 (100.00%) / 26/26 (100.00%) |
| Brawler | 24 | HiddenPocketEmpty | 26/20634 (0.13%) | 26/20634 (0.13%) | 26/26 (100.00%) / 26/26 (100.00%) | 26/26 (100.00%) / 26/26 (100.00%) |
| Gadgeteer | 8 | AreaDiary | 1757/6753 (26.02%) | 1757/6753 (26.02%) | 5/1757 (0.28%) / 41/1757 (2.33%) | 0/1757 (0.00%) / 28/1757 (1.59%) |
| Gadgeteer | 8 | PersonalityChanged | 1105/6753 (16.36%) | 1105/6753 (16.36%) | 796/1105 (72.04%) / 1100/1105 (99.55%) | 388/1105 (35.11%) / 1095/1105 (99.10%) |
| Gadgeteer | 8 | AreaActivityStarted | 652/6753 (9.65%) | 652/6753 (9.65%) | 0/652 (0.00%) / 0/652 (0.00%) | 0/652 (0.00%) / 0/652 (0.00%) |
| Gadgeteer | 8 | AreaActivityCompleted | 608/6753 (9.00%) | 608/6753 (9.00%) | 0/608 (0.00%) / 0/608 (0.00%) | 0/608 (0.00%) / 0/608 (0.00%) |
| Gadgeteer | 8 | ItemAllocated | 260/6753 (3.85%) | 260/6753 (3.85%) | 0/260 (0.00%) / 0/260 (0.00%) | 0/260 (0.00%) / 0/260 (0.00%) |
| Gadgeteer | 8 | Travel | 256/6753 (3.79%) | 256/6753 (3.79%) | 0/256 (0.00%) / 0/256 (0.00%) | 0/256 (0.00%) / 0/256 (0.00%) |
| Gadgeteer | 8 | Arrived | 256/6753 (3.79%) | 256/6753 (3.79%) | 106/256 (41.41%) / 253/256 (98.83%) | 0/256 (0.00%) / 1/256 (0.39%) |
| Gadgeteer | 8 | ShieldAbsorbed | 180/6753 (2.67%) | 180/6753 (2.67%) | 160/180 (88.89%) / 179/180 (99.44%) | 113/180 (62.78%) / 179/180 (99.44%) |
| Gadgeteer | 8 | ExtractionDecision | 168/6753 (2.49%) | 168/6753 (2.49%) | 120/168 (71.43%) / 168/168 (100.00%) | 54/168 (32.14%) / 54/168 (32.14%) |
| Gadgeteer | 8 | PackingStarted | 130/6753 (1.93%) | 130/6753 (1.93%) | 97/130 (74.62%) / 128/130 (98.46%) | 0/130 (0.00%) / 0/130 (0.00%) |
| Gadgeteer | 8 | PackingCompleted | 130/6753 (1.93%) | 130/6753 (1.93%) | 130/130 (100.00%) / 130/130 (100.00%) | 0/130 (0.00%) / 0/130 (0.00%) |
| Gadgeteer | 8 | Resilience | 126/6753 (1.87%) | 126/6753 (1.87%) | 87/126 (69.05%) / 125/126 (99.21%) | 53/126 (42.06%) / 124/126 (98.41%) |
| Gadgeteer | 8 | RobotRound | 121/6753 (1.79%) | 121/6753 (1.79%) | 0/121 (0.00%) / 0/121 (0.00%) | 0/121 (0.00%) / 0/121 (0.00%) |
| Gadgeteer | 8 | Search | 103/6753 (1.53%) | 103/6753 (1.53%) | 0/103 (0.00%) / 0/103 (0.00%) | 0/103 (0.00%) / 0/103 (0.00%) |
| Gadgeteer | 8 | ConsumableUsed | 79/6753 (1.17%) | 79/6753 (1.17%) | 0/79 (0.00%) / 0/79 (0.00%) | 0/79 (0.00%) / 0/79 (0.00%) |
| Gadgeteer | 8 | ConditionImpact | 71/6753 (1.05%) | 71/6753 (1.05%) | 51/71 (71.83%) / 70/71 (98.59%) | 23/71 (32.39%) / 69/71 (97.18%) |
| Gadgeteer | 8 | HazardWarning | 70/6753 (1.04%) | 70/6753 (1.04%) | 55/70 (78.57%) / 70/70 (100.00%) | 24/70 (34.29%) / 69/70 (98.57%) |
| Gadgeteer | 8 | DutyAdmission | 56/6753 (0.83%) | 56/6753 (0.83%) | 53/56 (94.64%) / 53/56 (94.64%) | 0/56 (0.00%) / 0/56 (0.00%) |
| Gadgeteer | 8 | WorldCondition | 56/6753 (0.83%) | 56/6753 (0.83%) | 0/56 (0.00%) / 0/56 (0.00%) | 0/56 (0.00%) / 0/56 (0.00%) |
| Gadgeteer | 8 | SearchCompleted | 56/6753 (0.83%) | 56/6753 (0.83%) | 54/56 (96.43%) / 54/56 (96.43%) | 22/56 (39.29%) / 54/56 (96.43%) |
| Gadgeteer | 8 | RobotCombatStarted | 53/6753 (0.78%) | 53/6753 (0.78%) | 0/53 (0.00%) / 0/53 (0.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Gadgeteer | 8 | RobotParticipantJoined | 53/6753 (0.78%) | 53/6753 (0.78%) | 0/53 (0.00%) / 0/53 (0.00%) | 0/53 (0.00%) / 0/53 (0.00%) |
| Gadgeteer | 8 | ExtractionStarted | 50/6753 (0.74%) | 50/6753 (0.74%) | 0/50 (0.00%) / 0/50 (0.00%) | 0/50 (0.00%) / 0/50 (0.00%) |
| Gadgeteer | 8 | SharedExtractionCalled | 50/6753 (0.74%) | 50/6753 (0.74%) | 49/50 (98.00%) / 50/50 (100.00%) | 0/50 (0.00%) / 0/50 (0.00%) |
| Gadgeteer | 8 | ActivityInterrupted | 49/6753 (0.73%) | 49/6753 (0.73%) | 46/49 (93.88%) / 49/49 (100.00%) | 15/49 (30.61%) / 49/49 (100.00%) |
| Gadgeteer | 8 | ExtractionVehicleArrived | 49/6753 (0.73%) | 49/6753 (0.73%) | 49/49 (100.00%) / 49/49 (100.00%) | 0/49 (0.00%) / 0/49 (0.00%) |
| Gadgeteer | 8 | Extracted | 49/6753 (0.73%) | 49/6753 (0.73%) | 0/49 (0.00%) / 0/49 (0.00%) | 0/49 (0.00%) / 0/49 (0.00%) |
| Gadgeteer | 8 | EncounterResolved | 47/6753 (0.70%) | 47/6753 (0.70%) | 10/47 (21.28%) / 47/47 (100.00%) | 9/47 (19.15%) / 47/47 (100.00%) |
| Gadgeteer | 8 | RobotDefeated | 27/6753 (0.40%) | 27/6753 (0.40%) | 25/27 (92.59%) / 27/27 (100.00%) | 0/27 (0.00%) / 0/27 (0.00%) |
| Gadgeteer | 8 | RobotDefeatCargoResolved | 27/6753 (0.40%) | 27/6753 (0.40%) | 27/27 (100.00%) / 27/27 (100.00%) | 16/27 (59.26%) / 27/27 (100.00%) |
| Gadgeteer | 8 | RobotEscaped | 26/6753 (0.39%) | 26/6753 (0.39%) | 26/26 (100.00%) / 26/26 (100.00%) | 0/26 (0.00%) / 0/26 (0.00%) |
| Gadgeteer | 8 | UnmitigatedDamage | 12/6753 (0.18%) | 12/6753 (0.18%) | 10/12 (83.33%) / 12/12 (100.00%) | 9/12 (75.00%) / 12/12 (100.00%) |
| Gadgeteer | 8 | Downed | 7/6753 (0.10%) | 7/6753 (0.10%) | 7/7 (100.00%) / 7/7 (100.00%) | 7/7 (100.00%) / 7/7 (100.00%) |
| Gadgeteer | 8 | KnockedOut | 7/6753 (0.10%) | 7/6753 (0.10%) | 7/7 (100.00%) / 7/7 (100.00%) | 7/7 (100.00%) / 7/7 (100.00%) |
| Gadgeteer | 8 | HiddenPocketEmpty | 7/6753 (0.10%) | 7/6753 (0.10%) | 7/7 (100.00%) / 7/7 (100.00%) | 7/7 (100.00%) / 7/7 (100.00%) |
| Gadgeteer | 24 | AreaDiary | 5416/21094 (25.68%) | 5416/21094 (25.68%) | 15/5416 (0.28%) / 105/5416 (1.94%) | 0/5416 (0.00%) / 67/5416 (1.24%) |
| Gadgeteer | 24 | PersonalityChanged | 3455/21094 (16.38%) | 3455/21094 (16.38%) | 2610/3455 (75.54%) / 3450/3455 (99.86%) | 1402/3455 (40.58%) / 3445/3455 (99.71%) |
| Gadgeteer | 24 | AreaActivityStarted | 1999/21094 (9.48%) | 1999/21094 (9.48%) | 0/1999 (0.00%) / 0/1999 (0.00%) | 0/1999 (0.00%) / 0/1999 (0.00%) |
| Gadgeteer | 24 | AreaActivityCompleted | 1868/21094 (8.86%) | 1868/21094 (8.86%) | 0/1868 (0.00%) / 0/1868 (0.00%) | 0/1868 (0.00%) / 0/1868 (0.00%) |
| Gadgeteer | 24 | ItemAllocated | 836/21094 (3.96%) | 836/21094 (3.96%) | 0/836 (0.00%) / 0/836 (0.00%) | 0/836 (0.00%) / 0/836 (0.00%) |
| Gadgeteer | 24 | Travel | 778/21094 (3.69%) | 778/21094 (3.69%) | 0/778 (0.00%) / 0/778 (0.00%) | 0/778 (0.00%) / 0/778 (0.00%) |
| Gadgeteer | 24 | Arrived | 778/21094 (3.69%) | 778/21094 (3.69%) | 355/778 (45.63%) / 775/778 (99.61%) | 0/778 (0.00%) / 1/778 (0.13%) |
| Gadgeteer | 24 | ShieldAbsorbed | 580/21094 (2.75%) | 580/21094 (2.75%) | 532/580 (91.72%) / 579/580 (99.83%) | 421/580 (72.59%) / 579/580 (99.83%) |
| Gadgeteer | 24 | ExtractionDecision | 494/21094 (2.34%) | 494/21094 (2.34%) | 360/494 (72.87%) / 494/494 (100.00%) | 151/494 (30.57%) / 152/494 (30.77%) |
| Gadgeteer | 24 | Resilience | 487/21094 (2.31%) | 487/21094 (2.31%) | 372/487 (76.39%) / 486/487 (99.79%) | 225/487 (46.20%) / 485/487 (99.59%) |
| Gadgeteer | 24 | PackingStarted | 418/21094 (1.98%) | 418/21094 (1.98%) | 329/418 (78.71%) / 416/418 (99.52%) | 0/418 (0.00%) / 0/418 (0.00%) |
| Gadgeteer | 24 | PackingCompleted | 418/21094 (1.98%) | 418/21094 (1.98%) | 418/418 (100.00%) / 418/418 (100.00%) | 0/418 (0.00%) / 0/418 (0.00%) |
| Gadgeteer | 24 | ConsumableUsed | 315/21094 (1.49%) | 315/21094 (1.49%) | 0/315 (0.00%) / 0/315 (0.00%) | 0/315 (0.00%) / 0/315 (0.00%) |
| Gadgeteer | 24 | Search | 313/21094 (1.48%) | 313/21094 (1.48%) | 0/313 (0.00%) / 0/313 (0.00%) | 0/313 (0.00%) / 0/313 (0.00%) |
| Gadgeteer | 24 | RobotRound | 313/21094 (1.48%) | 313/21094 (1.48%) | 0/313 (0.00%) / 0/313 (0.00%) | 0/313 (0.00%) / 0/313 (0.00%) |
| Gadgeteer | 24 | ConditionImpact | 287/21094 (1.36%) | 287/21094 (1.36%) | 215/287 (74.91%) / 286/287 (99.65%) | 98/287 (34.15%) / 285/287 (99.30%) |
| Gadgeteer | 24 | HazardWarning | 280/21094 (1.33%) | 280/21094 (1.33%) | 214/280 (76.43%) / 280/280 (100.00%) | 98/280 (35.00%) / 279/280 (99.64%) |
| Gadgeteer | 24 | DutyAdmission | 167/21094 (0.79%) | 167/21094 (0.79%) | 164/167 (98.20%) / 164/167 (98.20%) | 0/167 (0.00%) / 0/167 (0.00%) |
| Gadgeteer | 24 | WorldCondition | 167/21094 (0.79%) | 167/21094 (0.79%) | 0/167 (0.00%) / 0/167 (0.00%) | 0/167 (0.00%) / 0/167 (0.00%) |
| Gadgeteer | 24 | SearchCompleted | 167/21094 (0.79%) | 167/21094 (0.79%) | 165/167 (98.80%) / 165/167 (98.80%) | 72/167 (43.11%) / 165/167 (98.80%) |
| Gadgeteer | 24 | RobotCombatStarted | 157/21094 (0.74%) | 157/21094 (0.74%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Gadgeteer | 24 | RobotParticipantJoined | 157/21094 (0.74%) | 157/21094 (0.74%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Gadgeteer | 24 | ExtractionStarted | 157/21094 (0.74%) | 157/21094 (0.74%) | 0/157 (0.00%) / 0/157 (0.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Gadgeteer | 24 | SharedExtractionCalled | 157/21094 (0.74%) | 157/21094 (0.74%) | 156/157 (99.36%) / 157/157 (100.00%) | 0/157 (0.00%) / 0/157 (0.00%) |
| Gadgeteer | 24 | ActivityInterrupted | 156/21094 (0.74%) | 156/21094 (0.74%) | 147/156 (94.23%) / 156/156 (100.00%) | 64/156 (41.03%) / 156/156 (100.00%) |
| Gadgeteer | 24 | ExtractionVehicleArrived | 154/21094 (0.73%) | 154/21094 (0.73%) | 154/154 (100.00%) / 154/154 (100.00%) | 0/154 (0.00%) / 0/154 (0.00%) |
| Gadgeteer | 24 | Extracted | 153/21094 (0.73%) | 153/21094 (0.73%) | 0/153 (0.00%) / 0/153 (0.00%) | 0/153 (0.00%) / 0/153 (0.00%) |
| Gadgeteer | 24 | EncounterResolved | 144/21094 (0.68%) | 144/21094 (0.68%) | 38/144 (26.39%) / 144/144 (100.00%) | 31/144 (21.53%) / 144/144 (100.00%) |
| Gadgeteer | 24 | RobotDefeated | 107/21094 (0.51%) | 107/21094 (0.51%) | 99/107 (92.52%) / 107/107 (100.00%) | 0/107 (0.00%) / 0/107 (0.00%) |
| Gadgeteer | 24 | RobotDefeatCargoResolved | 107/21094 (0.51%) | 107/21094 (0.51%) | 103/107 (96.26%) / 107/107 (100.00%) | 57/107 (53.27%) / 107/107 (100.00%) |
| Gadgeteer | 24 | RobotEscaped | 50/21094 (0.24%) | 50/21094 (0.24%) | 50/50 (100.00%) / 50/50 (100.00%) | 0/50 (0.00%) / 0/50 (0.00%) |
| Gadgeteer | 24 | UnmitigatedDamage | 20/21094 (0.09%) | 20/21094 (0.09%) | 18/20 (90.00%) / 20/20 (100.00%) | 17/20 (85.00%) / 20/20 (100.00%) |
| Gadgeteer | 24 | Downed | 13/21094 (0.06%) | 13/21094 (0.06%) | 13/13 (100.00%) / 13/13 (100.00%) | 13/13 (100.00%) / 13/13 (100.00%) |
| Gadgeteer | 24 | KnockedOut | 13/21094 (0.06%) | 13/21094 (0.06%) | 13/13 (100.00%) / 13/13 (100.00%) | 13/13 (100.00%) / 13/13 (100.00%) |
| Gadgeteer | 24 | HiddenPocketEmpty | 13/21094 (0.06%) | 13/21094 (0.06%) | 13/13 (100.00%) / 13/13 (100.00%) | 13/13 (100.00%) / 13/13 (100.00%) |

### Before/after by source pool

| Trait | Hours | ID | Before count/share | After count/share | Before R20 / R50 | After R20 / R50 |
|---|---:|---|---|---|---|---|
| Cautious | 8 | voice.general | 2616/6619 (39.52%) | 1722/6619 (26.02%) | 354/2616 (13.53%) / 2580/2616 (98.62%) | 30/1722 (1.74%) / 1040/1722 (60.39%) |
| Cautious | 8 | voice.diary-surveying | 618/6619 (9.34%) | 618/6619 (9.34%) | 0/618 (0.00%) / 17/618 (2.75%) | 0/618 (0.00%) / 17/618 (2.75%) |
| Cautious | 8 | voice.diary-checking | 567/6619 (8.57%) | 567/6619 (8.57%) | 0/567 (0.00%) / 13/567 (2.29%) | 0/567 (0.00%) / 13/567 (2.29%) |
| Cautious | 8 | voice.diary-general | 524/6619 (7.92%) | 524/6619 (7.92%) | 2/524 (0.38%) / 7/524 (1.34%) | 0/524 (0.00%) / 0/524 (0.00%) |
| Cautious | 8 | voice.allocation | 270/6619 (4.08%) | 540/6619 (8.16%) | 0/270 (0.00%) / 0/270 (0.00%) | 0/540 (0.00%) / 0/540 (0.00%) |
| Cautious | 8 | voice.travel | 259/6619 (3.91%) | 577/6619 (8.72%) | 0/259 (0.00%) / 0/259 (0.00%) | 0/577 (0.00%) / 1/577 (0.17%) |
| Cautious | 8 | voice.start-surveying | 233/6619 (3.52%) | 233/6619 (3.52%) | 0/233 (0.00%) / 0/233 (0.00%) | 0/233 (0.00%) / 0/233 (0.00%) |
| Cautious | 8 | voice.robot | 219/6619 (3.31%) | 272/6619 (4.11%) | 0/219 (0.00%) / 0/219 (0.00%) | 0/272 (0.00%) / 0/272 (0.00%) |
| Cautious | 8 | voice.complete-surveying | 209/6619 (3.16%) | 209/6619 (3.16%) | 0/209 (0.00%) / 0/209 (0.00%) | 0/209 (0.00%) / 0/209 (0.00%) |
| Cautious | 8 | voice.start-checking | 209/6619 (3.16%) | 209/6619 (3.16%) | 0/209 (0.00%) / 0/209 (0.00%) | 0/209 (0.00%) / 0/209 (0.00%) |
| Cautious | 8 | voice.complete-checking | 194/6619 (2.93%) | 194/6619 (2.93%) | 0/194 (0.00%) / 0/194 (0.00%) | 0/194 (0.00%) / 0/194 (0.00%) |
| Cautious | 8 | voice.start-rummaging | 193/6619 (2.92%) | 193/6619 (2.92%) | 0/193 (0.00%) / 0/193 (0.00%) | 0/193 (0.00%) / 0/193 (0.00%) |
| Cautious | 8 | voice.complete-rummaging | 181/6619 (2.73%) | 181/6619 (2.73%) | 0/181 (0.00%) / 0/181 (0.00%) | 0/181 (0.00%) / 0/181 (0.00%) |
| Cautious | 8 | voice.search | 105/6619 (1.59%) | 105/6619 (1.59%) | 0/105 (0.00%) / 0/105 (0.00%) | 0/105 (0.00%) / 0/105 (0.00%) |
| Cautious | 8 | voice.exit | 101/6619 (1.53%) | 354/6619 (5.35%) | 0/101 (0.00%) / 0/101 (0.00%) | 41/354 (11.58%) / 42/354 (11.86%) |
| Cautious | 8 | voice.consumable | 62/6619 (0.94%) | 62/6619 (0.94%) | 0/62 (0.00%) / 0/62 (0.00%) | 0/62 (0.00%) / 0/62 (0.00%) |
| Cautious | 8 | voice.condition | 59/6619 (0.89%) | 59/6619 (0.89%) | 0/59 (0.00%) / 0/59 (0.00%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Cautious | 24 | voice.general | 8197/20561 (39.87%) | 5496/20561 (26.73%) | 1112/8197 (13.57%) / 8161/8197 (99.56%) | 118/5496 (2.15%) / 3516/5496 (63.97%) |
| Cautious | 24 | voice.diary-surveying | 1881/20561 (9.15%) | 1881/20561 (9.15%) | 0/1881 (0.00%) / 38/1881 (2.02%) | 0/1881 (0.00%) / 38/1881 (2.02%) |
| Cautious | 24 | voice.diary-checking | 1741/20561 (8.47%) | 1741/20561 (8.47%) | 0/1741 (0.00%) / 33/1741 (1.90%) | 0/1741 (0.00%) / 33/1741 (1.90%) |
| Cautious | 24 | voice.diary-general | 1652/20561 (8.03%) | 1652/20561 (8.03%) | 8/1652 (0.48%) / 15/1652 (0.91%) | 0/1652 (0.00%) / 0/1652 (0.00%) |
| Cautious | 24 | voice.allocation | 826/20561 (4.02%) | 1652/20561 (8.03%) | 0/826 (0.00%) / 0/826 (0.00%) | 0/1652 (0.00%) / 0/1652 (0.00%) |
| Cautious | 24 | voice.travel | 782/20561 (3.80%) | 1734/20561 (8.43%) | 0/782 (0.00%) / 0/782 (0.00%) | 0/1734 (0.00%) / 1/1734 (0.06%) |
| Cautious | 24 | voice.start-surveying | 714/20561 (3.47%) | 714/20561 (3.47%) | 0/714 (0.00%) / 0/714 (0.00%) | 0/714 (0.00%) / 0/714 (0.00%) |
| Cautious | 24 | voice.robot | 656/20561 (3.19%) | 817/20561 (3.97%) | 0/656 (0.00%) / 0/656 (0.00%) | 0/817 (0.00%) / 0/817 (0.00%) |
| Cautious | 24 | voice.complete-surveying | 645/20561 (3.14%) | 645/20561 (3.14%) | 0/645 (0.00%) / 0/645 (0.00%) | 0/645 (0.00%) / 0/645 (0.00%) |
| Cautious | 24 | voice.start-checking | 645/20561 (3.14%) | 645/20561 (3.14%) | 0/645 (0.00%) / 0/645 (0.00%) | 0/645 (0.00%) / 0/645 (0.00%) |
| Cautious | 24 | voice.complete-checking | 599/20561 (2.91%) | 599/20561 (2.91%) | 0/599 (0.00%) / 0/599 (0.00%) | 0/599 (0.00%) / 0/599 (0.00%) |
| Cautious | 24 | voice.start-rummaging | 598/20561 (2.91%) | 598/20561 (2.91%) | 0/598 (0.00%) / 0/598 (0.00%) | 0/598 (0.00%) / 0/598 (0.00%) |
| Cautious | 24 | voice.complete-rummaging | 566/20561 (2.75%) | 566/20561 (2.75%) | 0/566 (0.00%) / 0/566 (0.00%) | 0/566 (0.00%) / 0/566 (0.00%) |
| Cautious | 24 | voice.search | 313/20561 (1.52%) | 313/20561 (1.52%) | 0/313 (0.00%) / 0/313 (0.00%) | 0/313 (0.00%) / 0/313 (0.00%) |
| Cautious | 24 | voice.exit | 306/20561 (1.49%) | 1068/20561 (5.19%) | 0/306 (0.00%) / 0/306 (0.00%) | 124/1068 (11.61%) / 126/1068 (11.80%) |
| Cautious | 24 | voice.consumable | 269/20561 (1.31%) | 269/20561 (1.31%) | 0/269 (0.00%) / 0/269 (0.00%) | 0/269 (0.00%) / 0/269 (0.00%) |
| Cautious | 24 | voice.condition | 171/20561 (0.83%) | 171/20561 (0.83%) | 0/171 (0.00%) / 0/171 (0.00%) | 0/171 (0.00%) / 0/171 (0.00%) |
| Hoarder | 8 | voice.hoarder | 2617/6563 (39.88%) | 1730/6563 (26.36%) | 1698/2617 (64.88%) / 2596/2617 (99.20%) | 564/1730 (32.60%) / 1709/1730 (98.79%) |
| Hoarder | 8 | voice.diary-surveying | 607/6563 (9.25%) | 607/6563 (9.25%) | 0/607 (0.00%) / 17/607 (2.80%) | 0/607 (0.00%) / 17/607 (2.80%) |
| Hoarder | 8 | voice.diary-checking | 563/6563 (8.58%) | 563/6563 (8.58%) | 0/563 (0.00%) / 12/563 (2.13%) | 0/563 (0.00%) / 12/563 (2.13%) |
| Hoarder | 8 | voice.diary-general | 527/6563 (8.03%) | 527/6563 (8.03%) | 5/527 (0.95%) / 12/527 (2.28%) | 0/527 (0.00%) / 0/527 (0.00%) |
| Hoarder | 8 | voice.allocation | 264/6563 (4.02%) | 528/6563 (8.05%) | 0/264 (0.00%) / 0/264 (0.00%) | 0/528 (0.00%) / 0/528 (0.00%) |
| Hoarder | 8 | voice.travel | 250/6563 (3.81%) | 558/6563 (8.50%) | 0/250 (0.00%) / 0/250 (0.00%) | 0/558 (0.00%) / 0/558 (0.00%) |
| Hoarder | 8 | voice.start-surveying | 226/6563 (3.44%) | 226/6563 (3.44%) | 0/226 (0.00%) / 0/226 (0.00%) | 0/226 (0.00%) / 0/226 (0.00%) |
| Hoarder | 8 | voice.robot | 211/6563 (3.21%) | 262/6563 (3.99%) | 0/211 (0.00%) / 0/211 (0.00%) | 0/262 (0.00%) / 0/262 (0.00%) |
| Hoarder | 8 | voice.complete-surveying | 208/6563 (3.17%) | 208/6563 (3.17%) | 0/208 (0.00%) / 0/208 (0.00%) | 0/208 (0.00%) / 0/208 (0.00%) |
| Hoarder | 8 | voice.start-checking | 208/6563 (3.17%) | 208/6563 (3.17%) | 0/208 (0.00%) / 0/208 (0.00%) | 0/208 (0.00%) / 0/208 (0.00%) |
| Hoarder | 8 | voice.complete-checking | 193/6563 (2.94%) | 193/6563 (2.94%) | 0/193 (0.00%) / 0/193 (0.00%) | 0/193 (0.00%) / 0/193 (0.00%) |
| Hoarder | 8 | voice.start-rummaging | 193/6563 (2.94%) | 193/6563 (2.94%) | 0/193 (0.00%) / 0/193 (0.00%) | 0/193 (0.00%) / 0/193 (0.00%) |
| Hoarder | 8 | voice.complete-rummaging | 177/6563 (2.70%) | 177/6563 (2.70%) | 0/177 (0.00%) / 0/177 (0.00%) | 0/177 (0.00%) / 0/177 (0.00%) |
| Hoarder | 8 | voice.search | 101/6563 (1.54%) | 101/6563 (1.54%) | 0/101 (0.00%) / 0/101 (0.00%) | 0/101 (0.00%) / 0/101 (0.00%) |
| Hoarder | 8 | voice.exit | 100/6563 (1.52%) | 364/6563 (5.55%) | 0/100 (0.00%) / 0/100 (0.00%) | 54/364 (14.84%) / 56/364 (15.38%) |
| Hoarder | 8 | voice.consumable | 60/6563 (0.91%) | 60/6563 (0.91%) | 0/60 (0.00%) / 0/60 (0.00%) | 0/60 (0.00%) / 0/60 (0.00%) |
| Hoarder | 8 | voice.condition | 58/6563 (0.88%) | 58/6563 (0.88%) | 0/58 (0.00%) / 0/58 (0.00%) | 0/58 (0.00%) / 0/58 (0.00%) |
| Hoarder | 24 | voice.hoarder | 8232/20688 (39.79%) | 5547/20688 (26.81%) | 5435/8232 (66.02%) / 8211/8232 (99.74%) | 1902/5547 (34.29%) / 5526/5547 (99.62%) |
| Hoarder | 24 | voice.diary-surveying | 1924/20688 (9.30%) | 1924/20688 (9.30%) | 0/1924 (0.00%) / 47/1924 (2.44%) | 0/1924 (0.00%) / 47/1924 (2.44%) |
| Hoarder | 24 | voice.diary-checking | 1760/20688 (8.51%) | 1760/20688 (8.51%) | 0/1760 (0.00%) / 30/1760 (1.70%) | 0/1760 (0.00%) / 30/1760 (1.70%) |
| Hoarder | 24 | voice.diary-general | 1669/20688 (8.07%) | 1669/20688 (8.07%) | 8/1669 (0.48%) / 19/1669 (1.14%) | 0/1669 (0.00%) / 0/1669 (0.00%) |
| Hoarder | 24 | voice.allocation | 818/20688 (3.95%) | 1636/20688 (7.91%) | 0/818 (0.00%) / 0/818 (0.00%) | 0/1636 (0.00%) / 0/1636 (0.00%) |
| Hoarder | 24 | voice.travel | 767/20688 (3.71%) | 1703/20688 (8.23%) | 0/767 (0.00%) / 0/767 (0.00%) | 0/1703 (0.00%) / 0/1703 (0.00%) |
| Hoarder | 24 | voice.start-surveying | 718/20688 (3.47%) | 718/20688 (3.47%) | 0/718 (0.00%) / 0/718 (0.00%) | 0/718 (0.00%) / 0/718 (0.00%) |
| Hoarder | 24 | voice.complete-surveying | 662/20688 (3.20%) | 662/20688 (3.20%) | 0/662 (0.00%) / 0/662 (0.00%) | 0/662 (0.00%) / 0/662 (0.00%) |
| Hoarder | 24 | voice.start-checking | 660/20688 (3.19%) | 660/20688 (3.19%) | 0/660 (0.00%) / 0/660 (0.00%) | 0/660 (0.00%) / 0/660 (0.00%) |
| Hoarder | 24 | voice.robot | 640/20688 (3.09%) | 797/20688 (3.85%) | 0/640 (0.00%) / 0/640 (0.00%) | 0/797 (0.00%) / 0/797 (0.00%) |
| Hoarder | 24 | voice.complete-checking | 609/20688 (2.94%) | 609/20688 (2.94%) | 0/609 (0.00%) / 0/609 (0.00%) | 0/609 (0.00%) / 0/609 (0.00%) |
| Hoarder | 24 | voice.start-rummaging | 609/20688 (2.94%) | 609/20688 (2.94%) | 0/609 (0.00%) / 0/609 (0.00%) | 0/609 (0.00%) / 0/609 (0.00%) |
| Hoarder | 24 | voice.complete-rummaging | 569/20688 (2.75%) | 569/20688 (2.75%) | 0/569 (0.00%) / 0/569 (0.00%) | 0/569 (0.00%) / 0/569 (0.00%) |
| Hoarder | 24 | voice.search | 307/20688 (1.48%) | 307/20688 (1.48%) | 0/307 (0.00%) / 0/307 (0.00%) | 0/307 (0.00%) / 0/307 (0.00%) |
| Hoarder | 24 | voice.exit | 301/20688 (1.45%) | 1075/20688 (5.20%) | 0/301 (0.00%) / 0/301 (0.00%) | 136/1075 (12.65%) / 138/1075 (12.84%) |
| Hoarder | 24 | voice.consumable | 274/20688 (1.32%) | 274/20688 (1.32%) | 0/274 (0.00%) / 0/274 (0.00%) | 0/274 (0.00%) / 0/274 (0.00%) |
| Hoarder | 24 | voice.condition | 169/20688 (0.82%) | 169/20688 (0.82%) | 0/169 (0.00%) / 0/169 (0.00%) | 0/169 (0.00%) / 0/169 (0.00%) |
| Brawler | 8 | voice.brawler | 2681/6673 (40.18%) | 1789/6673 (26.81%) | 2034/2681 (75.87%) / 2663/2681 (99.33%) | 771/1789 (43.10%) / 1771/1789 (98.99%) |
| Brawler | 8 | voice.diary-surveying | 605/6673 (9.07%) | 605/6673 (9.07%) | 0/605 (0.00%) / 16/605 (2.64%) | 0/605 (0.00%) / 16/605 (2.64%) |
| Brawler | 8 | voice.diary-checking | 564/6673 (8.45%) | 564/6673 (8.45%) | 0/564 (0.00%) / 13/564 (2.30%) | 0/564 (0.00%) / 13/564 (2.30%) |
| Brawler | 8 | voice.diary-general | 532/6673 (7.97%) | 532/6673 (7.97%) | 5/532 (0.94%) / 12/532 (2.26%) | 0/532 (0.00%) / 0/532 (0.00%) |
| Brawler | 8 | voice.allocation | 260/6673 (3.90%) | 520/6673 (7.79%) | 0/260 (0.00%) / 0/260 (0.00%) | 0/520 (0.00%) / 0/520 (0.00%) |
| Brawler | 8 | voice.travel | 255/6673 (3.82%) | 569/6673 (8.53%) | 0/255 (0.00%) / 0/255 (0.00%) | 0/569 (0.00%) / 1/569 (0.18%) |
| Brawler | 8 | voice.start-surveying | 230/6673 (3.45%) | 230/6673 (3.45%) | 0/230 (0.00%) / 0/230 (0.00%) | 0/230 (0.00%) / 0/230 (0.00%) |
| Brawler | 8 | voice.robot | 219/6673 (3.28%) | 270/6673 (4.05%) | 0/219 (0.00%) / 0/219 (0.00%) | 0/270 (0.00%) / 0/270 (0.00%) |
| Brawler | 8 | voice.complete-surveying | 207/6673 (3.10%) | 207/6673 (3.10%) | 0/207 (0.00%) / 0/207 (0.00%) | 0/207 (0.00%) / 0/207 (0.00%) |
| Brawler | 8 | voice.start-checking | 207/6673 (3.10%) | 207/6673 (3.10%) | 0/207 (0.00%) / 0/207 (0.00%) | 0/207 (0.00%) / 0/207 (0.00%) |
| Brawler | 8 | voice.complete-checking | 195/6673 (2.92%) | 195/6673 (2.92%) | 0/195 (0.00%) / 0/195 (0.00%) | 0/195 (0.00%) / 0/195 (0.00%) |
| Brawler | 8 | voice.start-rummaging | 194/6673 (2.91%) | 194/6673 (2.91%) | 0/194 (0.00%) / 0/194 (0.00%) | 0/194 (0.00%) / 0/194 (0.00%) |
| Brawler | 8 | voice.complete-rummaging | 184/6673 (2.76%) | 184/6673 (2.76%) | 0/184 (0.00%) / 0/184 (0.00%) | 0/184 (0.00%) / 0/184 (0.00%) |
| Brawler | 8 | voice.search | 104/6673 (1.56%) | 104/6673 (1.56%) | 0/104 (0.00%) / 0/104 (0.00%) | 0/104 (0.00%) / 0/104 (0.00%) |
| Brawler | 8 | voice.exit | 100/6673 (1.50%) | 367/6673 (5.50%) | 0/100 (0.00%) / 0/100 (0.00%) | 55/367 (14.99%) / 58/367 (15.80%) |
| Brawler | 8 | voice.consumable | 77/6673 (1.15%) | 77/6673 (1.15%) | 0/77 (0.00%) / 0/77 (0.00%) | 0/77 (0.00%) / 0/77 (0.00%) |
| Brawler | 8 | voice.condition | 59/6673 (0.88%) | 59/6673 (0.88%) | 0/59 (0.00%) / 0/59 (0.00%) | 0/59 (0.00%) / 0/59 (0.00%) |
| Brawler | 24 | voice.brawler | 8248/20634 (39.97%) | 5580/20634 (27.04%) | 6294/8248 (76.31%) / 8230/8248 (99.78%) | 2435/5580 (43.64%) / 5562/5580 (99.68%) |
| Brawler | 24 | voice.diary-surveying | 1890/20634 (9.16%) | 1890/20634 (9.16%) | 0/1890 (0.00%) / 44/1890 (2.33%) | 0/1890 (0.00%) / 44/1890 (2.33%) |
| Brawler | 24 | voice.diary-checking | 1739/20634 (8.43%) | 1739/20634 (8.43%) | 0/1739 (0.00%) / 34/1739 (1.96%) | 0/1739 (0.00%) / 34/1739 (1.96%) |
| Brawler | 24 | voice.diary-general | 1685/20634 (8.17%) | 1685/20634 (8.17%) | 8/1685 (0.47%) / 29/1685 (1.72%) | 0/1685 (0.00%) / 0/1685 (0.00%) |
| Brawler | 24 | voice.allocation | 824/20634 (3.99%) | 1648/20634 (7.99%) | 0/824 (0.00%) / 0/824 (0.00%) | 0/1648 (0.00%) / 0/1648 (0.00%) |
| Brawler | 24 | voice.travel | 774/20634 (3.75%) | 1718/20634 (8.33%) | 0/774 (0.00%) / 0/774 (0.00%) | 0/1718 (0.00%) / 1/1718 (0.06%) |
| Brawler | 24 | voice.start-surveying | 718/20634 (3.48%) | 718/20634 (3.48%) | 0/718 (0.00%) / 0/718 (0.00%) | 0/718 (0.00%) / 0/718 (0.00%) |
| Brawler | 24 | voice.complete-surveying | 645/20634 (3.13%) | 645/20634 (3.13%) | 0/645 (0.00%) / 0/645 (0.00%) | 0/645 (0.00%) / 0/645 (0.00%) |
| Brawler | 24 | voice.start-checking | 644/20634 (3.12%) | 644/20634 (3.12%) | 0/644 (0.00%) / 0/644 (0.00%) | 0/644 (0.00%) / 0/644 (0.00%) |
| Brawler | 24 | voice.robot | 631/20634 (3.06%) | 786/20634 (3.81%) | 0/631 (0.00%) / 0/631 (0.00%) | 0/786 (0.00%) / 0/786 (0.00%) |
| Brawler | 24 | voice.complete-checking | 605/20634 (2.93%) | 605/20634 (2.93%) | 0/605 (0.00%) / 0/605 (0.00%) | 0/605 (0.00%) / 0/605 (0.00%) |
| Brawler | 24 | voice.start-rummaging | 603/20634 (2.92%) | 603/20634 (2.92%) | 0/603 (0.00%) / 0/603 (0.00%) | 0/603 (0.00%) / 0/603 (0.00%) |
| Brawler | 24 | voice.complete-rummaging | 577/20634 (2.80%) | 577/20634 (2.80%) | 0/577 (0.00%) / 0/577 (0.00%) | 0/577 (0.00%) / 0/577 (0.00%) |
| Brawler | 24 | voice.search | 314/20634 (1.52%) | 314/20634 (1.52%) | 0/314 (0.00%) / 0/314 (0.00%) | 0/314 (0.00%) / 0/314 (0.00%) |
| Brawler | 24 | voice.exit | 297/20634 (1.44%) | 1042/20634 (5.05%) | 0/297 (0.00%) / 0/297 (0.00%) | 118/1042 (11.32%) / 122/1042 (11.71%) |
| Brawler | 24 | voice.consumable | 270/20634 (1.31%) | 270/20634 (1.31%) | 0/270 (0.00%) / 0/270 (0.00%) | 0/270 (0.00%) / 0/270 (0.00%) |
| Brawler | 24 | voice.condition | 170/20634 (0.82%) | 170/20634 (0.82%) | 0/170 (0.00%) / 0/170 (0.00%) | 0/170 (0.00%) / 0/170 (0.00%) |
| Gadgeteer | 8 | voice.gadgeteer | 2656/6753 (39.33%) | 1764/6753 (26.12%) | 1972/2656 (74.25%) / 2638/2656 (99.32%) | 693/1764 (39.29%) / 1746/1764 (98.98%) |
| Gadgeteer | 8 | voice.diary-surveying | 625/6753 (9.26%) | 625/6753 (9.26%) | 0/625 (0.00%) / 18/625 (2.88%) | 0/625 (0.00%) / 18/625 (2.88%) |
| Gadgeteer | 8 | voice.diary-checking | 584/6753 (8.65%) | 584/6753 (8.65%) | 0/584 (0.00%) / 10/584 (1.71%) | 0/584 (0.00%) / 10/584 (1.71%) |
| Gadgeteer | 8 | voice.diary-general | 548/6753 (8.11%) | 548/6753 (8.11%) | 5/548 (0.91%) / 13/548 (2.37%) | 0/548 (0.00%) / 0/548 (0.00%) |
| Gadgeteer | 8 | voice.allocation | 260/6753 (3.85%) | 520/6753 (7.70%) | 0/260 (0.00%) / 0/260 (0.00%) | 0/520 (0.00%) / 0/520 (0.00%) |
| Gadgeteer | 8 | voice.travel | 256/6753 (3.79%) | 568/6753 (8.41%) | 0/256 (0.00%) / 0/256 (0.00%) | 0/568 (0.00%) / 1/568 (0.18%) |
| Gadgeteer | 8 | voice.start-surveying | 235/6753 (3.48%) | 235/6753 (3.48%) | 0/235 (0.00%) / 0/235 (0.00%) | 0/235 (0.00%) / 0/235 (0.00%) |
| Gadgeteer | 8 | voice.robot | 227/6753 (3.36%) | 280/6753 (4.15%) | 0/227 (0.00%) / 0/227 (0.00%) | 0/280 (0.00%) / 0/280 (0.00%) |
| Gadgeteer | 8 | voice.complete-surveying | 215/6753 (3.18%) | 215/6753 (3.18%) | 0/215 (0.00%) / 0/215 (0.00%) | 0/215 (0.00%) / 0/215 (0.00%) |
| Gadgeteer | 8 | voice.start-checking | 215/6753 (3.18%) | 215/6753 (3.18%) | 0/215 (0.00%) / 0/215 (0.00%) | 0/215 (0.00%) / 0/215 (0.00%) |
| Gadgeteer | 8 | voice.complete-checking | 203/6753 (3.01%) | 203/6753 (3.01%) | 0/203 (0.00%) / 0/203 (0.00%) | 0/203 (0.00%) / 0/203 (0.00%) |
| Gadgeteer | 8 | voice.start-rummaging | 202/6753 (2.99%) | 202/6753 (2.99%) | 0/202 (0.00%) / 0/202 (0.00%) | 0/202 (0.00%) / 0/202 (0.00%) |
| Gadgeteer | 8 | voice.complete-rummaging | 190/6753 (2.81%) | 190/6753 (2.81%) | 0/190 (0.00%) / 0/190 (0.00%) | 0/190 (0.00%) / 0/190 (0.00%) |
| Gadgeteer | 8 | voice.search | 103/6753 (1.53%) | 103/6753 (1.53%) | 0/103 (0.00%) / 0/103 (0.00%) | 0/103 (0.00%) / 0/103 (0.00%) |
| Gadgeteer | 8 | voice.exit | 99/6753 (1.47%) | 366/6753 (5.42%) | 0/99 (0.00%) / 0/99 (0.00%) | 54/366 (14.75%) / 54/366 (14.75%) |
| Gadgeteer | 8 | voice.consumable | 79/6753 (1.17%) | 79/6753 (1.17%) | 0/79 (0.00%) / 0/79 (0.00%) | 0/79 (0.00%) / 0/79 (0.00%) |
| Gadgeteer | 8 | voice.condition | 56/6753 (0.83%) | 56/6753 (0.83%) | 0/56 (0.00%) / 0/56 (0.00%) | 0/56 (0.00%) / 0/56 (0.00%) |
| Gadgeteer | 24 | voice.gadgeteer | 8465/21094 (40.13%) | 5722/21094 (27.13%) | 6538/8465 (77.24%) / 8447/8465 (99.79%) | 2524/5722 (44.11%) / 5704/5722 (99.69%) |
| Gadgeteer | 24 | voice.diary-surveying | 1930/21094 (9.15%) | 1930/21094 (9.15%) | 0/1930 (0.00%) / 45/1930 (2.33%) | 0/1930 (0.00%) / 45/1930 (2.33%) |
| Gadgeteer | 24 | voice.diary-checking | 1779/21094 (8.43%) | 1779/21094 (8.43%) | 0/1779 (0.00%) / 22/1779 (1.24%) | 0/1779 (0.00%) / 22/1779 (1.24%) |
| Gadgeteer | 24 | voice.diary-general | 1707/21094 (8.09%) | 1707/21094 (8.09%) | 15/1707 (0.88%) / 38/1707 (2.23%) | 0/1707 (0.00%) / 0/1707 (0.00%) |
| Gadgeteer | 24 | voice.allocation | 836/21094 (3.96%) | 1672/21094 (7.93%) | 0/836 (0.00%) / 0/836 (0.00%) | 0/1672 (0.00%) / 0/1672 (0.00%) |
| Gadgeteer | 24 | voice.travel | 778/21094 (3.69%) | 1723/21094 (8.17%) | 0/778 (0.00%) / 0/778 (0.00%) | 0/1723 (0.00%) / 1/1723 (0.06%) |
| Gadgeteer | 24 | voice.start-surveying | 718/21094 (3.40%) | 718/21094 (3.40%) | 0/718 (0.00%) / 0/718 (0.00%) | 0/718 (0.00%) / 0/718 (0.00%) |
| Gadgeteer | 24 | voice.complete-surveying | 668/21094 (3.17%) | 668/21094 (3.17%) | 0/668 (0.00%) / 0/668 (0.00%) | 0/668 (0.00%) / 0/668 (0.00%) |
| Gadgeteer | 24 | voice.start-checking | 668/21094 (3.17%) | 668/21094 (3.17%) | 0/668 (0.00%) / 0/668 (0.00%) | 0/668 (0.00%) / 0/668 (0.00%) |
| Gadgeteer | 24 | voice.robot | 627/21094 (2.97%) | 784/21094 (3.72%) | 0/627 (0.00%) / 0/627 (0.00%) | 0/784 (0.00%) / 0/784 (0.00%) |
| Gadgeteer | 24 | voice.complete-checking | 615/21094 (2.92%) | 615/21094 (2.92%) | 0/615 (0.00%) / 0/615 (0.00%) | 0/615 (0.00%) / 0/615 (0.00%) |
| Gadgeteer | 24 | voice.start-rummaging | 613/21094 (2.91%) | 613/21094 (2.91%) | 0/613 (0.00%) / 0/613 (0.00%) | 0/613 (0.00%) / 0/613 (0.00%) |
| Gadgeteer | 24 | voice.complete-rummaging | 585/21094 (2.77%) | 585/21094 (2.77%) | 0/585 (0.00%) / 0/585 (0.00%) | 0/585 (0.00%) / 0/585 (0.00%) |
| Gadgeteer | 24 | voice.consumable | 315/21094 (1.49%) | 315/21094 (1.49%) | 0/315 (0.00%) / 0/315 (0.00%) | 0/315 (0.00%) / 0/315 (0.00%) |
| Gadgeteer | 24 | voice.search | 313/21094 (1.48%) | 313/21094 (1.48%) | 0/313 (0.00%) / 0/313 (0.00%) | 0/313 (0.00%) / 0/313 (0.00%) |
| Gadgeteer | 24 | voice.exit | 310/21094 (1.47%) | 1115/21094 (5.29%) | 0/310 (0.00%) / 0/310 (0.00%) | 151/1115 (13.54%) / 152/1115 (13.63%) |
| Gadgeteer | 24 | voice.condition | 167/21094 (0.79%) | 167/21094 (0.79%) | 0/167 (0.00%) / 0/167 (0.00%) | 0/167 (0.00%) / 0/167 (0.00%) |

### Top ten exact quotes per pool (before and after)

The union of each stage's top ten is shown; '-' means outside that stage's top ten, not zero occurrences.

<details><summary>Cautious / 8h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.general | I would like my paperwork waterproof. | 219 | 143 |
| voice.general | I'm filing that under cautiously optimistic. | 219 | 144 |
| voice.general | I'm treating this as routine. Routine has declined to comment. | 219 | 143 |
| voice.general | Desperanza defines a shortcut as two stamps and a witness. | 218 | 144 |
| voice.general | Home is where the paperwork recognizes your handwriting. | 218 | 144 |
| voice.general | I consider my confidence provisional, like the flooring. | 218 | 144 |
| voice.general | I'm maintaining a professional distance from my own judgment. | 218 | 144 |
| voice.general | I'm submitting my concerns in the form of continued movement. | 218 | 144 |
| voice.general | The Before-ish sounds like a filing category with excellent publicity. | 218 | - |
| voice.general | All roads lead to a Tube Routes map. The routes remain a separate matter. | 217 | 143 |
| voice.general | I maintain a respectful distance from the hydroponic committee. | - | 143 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 79 | 77 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 78 | 77 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 78 | 77 |
| voice.diary-surveying | My sense of direction is on probation. | 78 | 77 |
| voice.diary-surveying | I distrust confidence sold without a map. | 77 | 76 |
| voice.diary-surveying | I consider signage a complicated relationship. | 76 | 77 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 76 | 78 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 76 | 79 |
| voice.diary-checking | I consider stitching more reliable than promises. | 72 | 70 |
| voice.diary-checking | My socks deserve a grievance procedure. | 72 | 70 |
| voice.diary-checking | My zipper has retained opposing counsel. | 72 | 70 |
| voice.diary-checking | I distrust buckles with management ambitions. | 71 | 71 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 71 | 71 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 71 | 72 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 69 | 71 |
| voice.diary-checking | My confidence is held together by adhesive. | 69 | 72 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 33 | - |
| voice.diary-general | I resent the retail markup on disappointment. | 33 | - |
| voice.diary-general | My curiosity has terrible credit. | 33 | - |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 32 | 31 |
| voice.diary-general | I prefer directions without a subscription. | 32 | 32 |
| voice.diary-general | My ambition exceeds the shelving specification. | 32 | 32 |
| voice.diary-general | My priorities need a returns policy. | 32 | 31 |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | 30 | - |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 30 | 31 |
| voice.diary-general | My patience qualifies as antique stock. | 30 | 31 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 32 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | - | 32 |
| voice.diary-general | I trust the hub's directions as far as I can fold them. | - | 31 |
| voice.diary-general | The Great Downstairs sounds less like an era and more like a basement committee. | - | 31 |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 20 | - |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | 20 | - |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 20 | 46 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 20 | 46 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 20 | 46 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 20 | 46 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 20 | 46 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 20 | 46 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 20 | - |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 20 | 34 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | - | 34 |
| voice.allocation | A coupon is a promise with arithmetic attached. | - | 33 |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | - | 33 |
| voice.travel | I consider walking away a premium survival feature. | 21 | - |
| voice.travel | I'm budgeting my optimism for the return journey. | 21 | - |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 21 | 34 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 21 | 34 |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 21 | 34 |
| voice.travel | I'm treating every step as a small investment in dry socks. | 21 | - |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 20 | 66 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 20 | 65 |
| voice.travel | Every pod has one seat and several opinions about the route. | 19 | 37 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | 19 | 65 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 67 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | - | 39 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 37 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 41 | 39 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 39 | 39 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 39 | 38 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 38 | 38 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 38 | 40 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 38 | 39 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 22 | 24 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 21 | 27 |
| voice.robot | I would like to speak to the machine's warranty department. | 21 | 24 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 21 | 23 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 20 | - |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 19 | 23 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 19 | 25 |
| voice.robot | I'm reserving my dignity for the performance review. | 19 | 29 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 19 | 24 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 19 | 24 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 26 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 54 | 52 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 53 | 52 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 51 | 52 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 51 | 53 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 37 | 35 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 36 | 34 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 34 | 35 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 34 | 36 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 34 | 34 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 34 | 35 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 49 | 49 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 49 | 49 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 49 | 48 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 47 | 48 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 34 | 32 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 32 | 32 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 32 | 34 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 32 | 32 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 32 | 31 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 31 | 32 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 46 | 45 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 46 | 45 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 45 | 45 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 44 | 46 |
| voice.search | I consider this locker a patience seminar with hinges. | 9 | 9 |
| voice.search | I have a system. The system involves suspicious containers. | 9 | - |
| voice.search | I search this locker with the confidence of an expired warranty. | 9 | 9 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 9 | 9 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 9 | 9 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 9 | 9 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 9 | - |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 9 | 9 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 9 | 9 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | 8 | 8 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | - | 9 |
| voice.search | Every drawer is a tiny referendum on my priorities. | - | 9 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 9 | 16 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 9 | - |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 8 | 24 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 8 | 24 |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 8 | 53 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 8 | 54 |
| voice.exit | I'm rating this exit strategy above the alternative. | 8 | 16 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 8 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | 8 | 25 |
| voice.exit | I will board when ready. My dignity will require separate transport. | 7 | 16 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | - | 53 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 25 |
| voice.consumable | I consider adhesive a bedside manner. | 9 | 8 |
| voice.consumable | My treatment plan needs a laminated apology. | 9 | 7 |
| voice.consumable | I prefer medical tape without motivational quotes. | 8 | 7 |
| voice.consumable | My confidence comes in questionable packaging. | 8 | 8 |
| voice.consumable | I consider premium panic a billing category. | 7 | 8 |
| voice.consumable | My dignity is not covered by the warranty. | 7 | 8 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 7 | 9 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 7 | 7 |
| voice.condition | I consider this forecast a workplace grievance. | 12 | 10 |
| voice.condition | I prefer weather without administrative ambition. | 10 | 9 |
| voice.condition | My socks oppose the atmospheric agenda. | 10 | 10 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 9 | 9 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 9 | 12 |
| voice.condition | My optimism needs a waterproof case. | 9 | 9 |

</details>

<details><summary>Cautious / 24h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.general | Desperanza defines a shortcut as two stamps and a witness. | 684 | 458 |
| voice.general | Home is where the paperwork recognizes your handwriting. | 684 | 458 |
| voice.general | I consider my confidence provisional, like the flooring. | 684 | 460 |
| voice.general | I would like my paperwork waterproof. | 684 | 457 |
| voice.general | I'm filing that under cautiously optimistic. | 684 | - |
| voice.general | I'm maintaining a professional distance from my own judgment. | 684 | 459 |
| voice.general | I'm submitting my concerns in the form of continued movement. | 684 | 460 |
| voice.general | I'm treating this as routine. Routine has declined to comment. | 684 | 458 |
| voice.general | The Before-ish sounds like a filing category with excellent publicity. | 682 | - |
| voice.general | All roads lead to a Tube Routes map. The routes remain a separate matter. | 681 | 457 |
| voice.general | The Pawn Desk can appraise anything except my confidence. | - | 458 |
| voice.general | I maintain a respectful distance from the hydroponic committee. | - | 457 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 236 | 235 |
| voice.diary-surveying | I consider signage a complicated relationship. | 235 | 235 |
| voice.diary-surveying | I distrust confidence sold without a map. | 235 | 235 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 235 | 235 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 235 | 235 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 235 | 236 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 235 | 235 |
| voice.diary-surveying | My sense of direction is on probation. | 235 | 235 |
| voice.diary-checking | My zipper has retained opposing counsel. | 219 | 217 |
| voice.diary-checking | I consider stitching more reliable than promises. | 218 | 217 |
| voice.diary-checking | I distrust buckles with management ambitions. | 218 | 218 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 218 | 217 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 218 | 219 |
| voice.diary-checking | My socks deserve a grievance procedure. | 218 | 217 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 216 | 218 |
| voice.diary-checking | My confidence is held together by adhesive. | 216 | 218 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 100 | 98 |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 99 | 97 |
| voice.diary-general | I prefer directions without a subscription. | 99 | 97 |
| voice.diary-general | I resent the retail markup on disappointment. | 99 | 97 |
| voice.diary-general | My ambition exceeds the shelving specification. | 99 | 98 |
| voice.diary-general | My curiosity has terrible credit. | 99 | 97 |
| voice.diary-general | My priorities need a returns policy. | 99 | - |
| voice.diary-general | My patience qualifies as antique stock. | 98 | - |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 97 | 97 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | 97 | - |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 98 |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | - | 97 |
| voice.diary-general | I trust the hub's directions as far as I can fold them. | - | 97 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 60 | 139 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 60 | 139 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 60 | 138 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 60 | 138 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 60 | 139 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 60 | 139 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 60 | - |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 60 | - |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | 58 | - |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 58 | 103 |
| voice.allocation | A coupon is a promise with arithmetic attached. | - | 103 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | - | 103 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | - | 103 |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 62 | 103 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 62 | 103 |
| voice.travel | Every pod has one seat and several opinions about the route. | 60 | 112 |
| voice.travel | I consider walking away a premium survival feature. | 60 | - |
| voice.travel | I trust the tube map exactly as far as I can fold it. | 60 | 196 |
| voice.travel | I'm budgeting my optimism for the return journey. | 60 | - |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 60 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 60 | 103 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | 60 | 113 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 60 | 197 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 197 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | - | 195 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 110 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 121 | 119 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 119 | 119 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 119 | 119 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 119 | 118 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 118 | 121 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 118 | 118 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 61 | 71 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 60 | 80 |
| voice.robot | I would like to speak to the machine's warranty department. | 60 | 72 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 60 | 71 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 60 | - |
| voice.robot | I'm reserving my dignity for the performance review. | 60 | 83 |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 59 | 71 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 59 | 72 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 59 | 72 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 59 | 72 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 82 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 162 | 161 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 162 | 161 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 161 | 162 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 160 | 161 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 109 | 107 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 108 | 107 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 107 | 107 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 107 | 109 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 107 | 107 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 107 | 108 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 152 | 149 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 149 | 150 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 149 | 151 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 149 | 149 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 101 | 100 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 100 | 101 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 100 | 99 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 99 | 99 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 99 | 99 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 99 | 100 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 143 | 141 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 141 | 142 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 141 | 142 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 141 | 141 |
| voice.search | I consider this locker a patience seminar with hinges. | 27 | 26 |
| voice.search | I have a system. The system involves suspicious containers. | 27 | 26 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 27 | 26 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 27 | 27 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 27 | 27 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 27 | 25 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 27 | - |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 26 | - |
| voice.search | Every drawer is a tiny referendum on my priorities. | 25 | 27 |
| voice.search | I search this locker with the confidence of an expired warranty. | 25 | 26 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | - | 27 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | - | 26 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 25 | 51 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 24 | 73 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 24 | 50 |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 24 | 157 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 24 | 157 |
| voice.exit | I'm rating this exit strategy above the alternative. | 24 | - |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 23 | 74 |
| voice.exit | I will board when ready. My dignity will require separate transport. | 23 | 50 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 23 | 158 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 23 | - |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 74 |
| voice.exit | The platform is a meeting place for me and a deadline. | - | 74 |
| voice.consumable | I consider adhesive a bedside manner. | 35 | 34 |
| voice.consumable | I consider premium panic a billing category. | 34 | 33 |
| voice.consumable | I prefer medical tape without motivational quotes. | 34 | 34 |
| voice.consumable | My confidence comes in questionable packaging. | 34 | 33 |
| voice.consumable | My treatment plan needs a laminated apology. | 34 | 33 |
| voice.consumable | My dignity is not covered by the warranty. | 33 | 33 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 33 | 35 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 32 | 34 |
| voice.condition | I consider this forecast a workplace grievance. | 30 | 30 |
| voice.condition | My socks oppose the atmospheric agenda. | 30 | 28 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 28 | 27 |
| voice.condition | I prefer weather without administrative ambition. | 28 | 28 |
| voice.condition | My optimism needs a waterproof case. | 28 | 28 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 27 | 30 |

</details>

<details><summary>Hoarder / 8h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.hoarder | I can justify this in writing. My handwriting is classified. | 375 | 248 |
| voice.hoarder | I have a system. Please don't audit it. | 375 | 247 |
| voice.hoarder | I consider restraint an optional accessory. | 374 | 248 |
| voice.hoarder | I prefer to call it inventory ambition. | 374 | 247 |
| voice.hoarder | I'm emotionally invested in the phrase 'could be useful'. | 374 | 247 |
| voice.hoarder | I'm budgeting for regret. It's a renewable resource. | 373 | 247 |
| voice.hoarder | I consider hoarding a form of long-term strategic planning. | 372 | 246 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 78 | 76 |
| voice.diary-surveying | I distrust confidence sold without a map. | 76 | 75 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 76 | 76 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 76 | 76 |
| voice.diary-surveying | My sense of direction is on probation. | 76 | 77 |
| voice.diary-surveying | I consider signage a complicated relationship. | 75 | 75 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 75 | 75 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 75 | 77 |
| voice.diary-checking | I consider stitching more reliable than promises. | 71 | 70 |
| voice.diary-checking | My socks deserve a grievance procedure. | 71 | 71 |
| voice.diary-checking | My zipper has retained opposing counsel. | 71 | 70 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 70 | 70 |
| voice.diary-checking | I distrust buckles with management ambitions. | 70 | 71 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 70 | 71 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 70 | 70 |
| voice.diary-checking | My confidence is held together by adhesive. | 70 | 70 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 33 | 31 |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 33 | - |
| voice.diary-general | I prefer directions without a subscription. | 33 | 32 |
| voice.diary-general | I resent the retail markup on disappointment. | 33 | 31 |
| voice.diary-general | My ambition exceeds the shelving specification. | 33 | 32 |
| voice.diary-general | My curiosity has terrible credit. | 33 | 31 |
| voice.diary-general | My priorities need a returns policy. | 33 | - |
| voice.diary-general | My patience qualifies as antique stock. | 31 | 31 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | 31 | - |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | 30 | 31 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 31 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | - | 31 |
| voice.diary-general | I trust the hub's directions as far as I can fold them. | - | 31 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 20 | 45 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 20 | 45 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 20 | 45 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 20 | 45 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 19 | 45 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 19 | 45 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 19 | 33 |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 19 | 33 |
| voice.allocation | A coupon is a promise with arithmetic attached. | 18 | - |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | 18 | - |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | - | 33 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | - | 33 |
| voice.travel | I consider walking away a premium survival feature. | 21 | 33 |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 21 | 33 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 21 | - |
| voice.travel | Every pod has one seat and several opinions about the route. | 19 | 36 |
| voice.travel | I'm budgeting my optimism for the return journey. | 19 | - |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 19 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 19 | 33 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | 19 | 38 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 19 | 64 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 19 | 62 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 65 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | - | 62 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 36 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 39 | 36 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 39 | 38 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 38 | 37 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 37 | 37 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 37 | 39 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 36 | 39 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 21 | 22 |
| voice.robot | I would like to speak to the machine's warranty department. | 20 | 24 |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 19 | 23 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 19 | 25 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 19 | 24 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 19 | 24 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 19 | - |
| voice.robot | I'm reserving my dignity for the performance review. | 19 | 27 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 19 | 24 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 19 | 23 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 24 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 54 | 52 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 52 | 51 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 51 | 52 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 51 | 53 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 36 | 35 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 35 | 34 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 35 | 34 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 35 | 36 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 34 | 35 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 33 | 34 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 49 | 48 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 48 | 49 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 48 | 48 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 48 | 48 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 34 | 31 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 33 | 31 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 32 | 32 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 32 | 34 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 31 | 32 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 31 | 33 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 45 | 44 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 45 | 44 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 45 | 44 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 42 | 45 |
| voice.search | I consider this locker a patience seminar with hinges. | 9 | 9 |
| voice.search | I have a system. The system involves suspicious containers. | 9 | 9 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 9 | 8 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 9 | 9 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 9 | 8 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 9 | 9 |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 9 | 9 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 9 | 9 |
| voice.search | Every drawer is a tiny referendum on my priorities. | 8 | - |
| voice.search | I search this locker with the confidence of an expired warranty. | 8 | - |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | - | 9 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | - | 8 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 9 | 24 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 9 | 16 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 9 | 57 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 9 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 8 | 56 |
| voice.exit | I'm rating this exit strategy above the alternative. | 8 | 17 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 8 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | 8 | 26 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 7 | 24 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 7 | 56 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 25 |
| voice.exit | I will board when ready. My dignity will require separate transport. | - | 16 |
| voice.consumable | I consider adhesive a bedside manner. | 8 | 8 |
| voice.consumable | I prefer medical tape without motivational quotes. | 8 | 7 |
| voice.consumable | My confidence comes in questionable packaging. | 8 | 7 |
| voice.consumable | My treatment plan needs a laminated apology. | 8 | 8 |
| voice.consumable | I consider premium panic a billing category. | 7 | 8 |
| voice.consumable | My dignity is not covered by the warranty. | 7 | 8 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 7 | 7 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 7 | 7 |
| voice.condition | I consider this forecast a workplace grievance. | 12 | 9 |
| voice.condition | My socks oppose the atmospheric agenda. | 10 | 9 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 9 | 9 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 9 | 12 |
| voice.condition | I prefer weather without administrative ambition. | 9 | 9 |
| voice.condition | My optimism needs a waterproof case. | 9 | 10 |

</details>

<details><summary>Hoarder / 24h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.hoarder | I can justify this in writing. My handwriting is classified. | 1177 | 794 |
| voice.hoarder | I consider restraint an optional accessory. | 1177 | 794 |
| voice.hoarder | I have a system. Please don't audit it. | 1177 | 791 |
| voice.hoarder | I prefer to call it inventory ambition. | 1176 | 792 |
| voice.hoarder | I'm emotionally invested in the phrase 'could be useful'. | 1176 | 793 |
| voice.hoarder | I'm budgeting for regret. It's a renewable resource. | 1175 | 792 |
| voice.hoarder | I consider hoarding a form of long-term strategic planning. | 1174 | 791 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 242 | 241 |
| voice.diary-surveying | I distrust confidence sold without a map. | 241 | 239 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 241 | 241 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 241 | 240 |
| voice.diary-surveying | My sense of direction is on probation. | 241 | 241 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 240 | 241 |
| voice.diary-surveying | I consider signage a complicated relationship. | 239 | 239 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 239 | 242 |
| voice.diary-checking | I consider stitching more reliable than promises. | 221 | 219 |
| voice.diary-checking | I distrust buckles with management ambitions. | 221 | 220 |
| voice.diary-checking | My socks deserve a grievance procedure. | 221 | 220 |
| voice.diary-checking | My zipper has retained opposing counsel. | 221 | 219 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 219 | 220 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 219 | 221 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 219 | 220 |
| voice.diary-checking | My confidence is held together by adhesive. | 219 | 221 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 102 | 98 |
| voice.diary-general | I resent the retail markup on disappointment. | 102 | 99 |
| voice.diary-general | My curiosity has terrible credit. | 102 | - |
| voice.diary-general | My priorities need a returns policy. | 99 | 99 |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 98 | 98 |
| voice.diary-general | I prefer directions without a subscription. | 98 | 99 |
| voice.diary-general | My ambition exceeds the shelving specification. | 98 | 99 |
| voice.diary-general | My patience qualifies as antique stock. | 98 | - |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | 97 | 98 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 97 | 98 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 98 |
| voice.diary-general | I trust the hub's directions as far as I can fold them. | - | 98 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 59 | 137 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 59 | 137 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 59 | 138 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 59 | 138 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 59 | 137 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 59 | 137 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 59 | 102 |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 59 | - |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | 58 | - |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 58 | 102 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | - | 102 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | - | 102 |
| voice.travel | I consider walking away a premium survival feature. | 60 | 102 |
| voice.travel | I'm budgeting my optimism for the return journey. | 60 | - |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 60 | 101 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 60 | - |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 60 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 60 | 101 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 59 | 193 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 59 | 192 |
| voice.travel | Every pod has one seat and several opinions about the route. | 58 | 110 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | 58 | 192 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 193 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | - | 110 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 109 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 121 | 118 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 121 | 119 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 120 | 119 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 119 | 120 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 119 | 121 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 118 | 121 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 167 | 165 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 165 | 165 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 165 | 166 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 165 | 166 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 112 | 109 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 112 | 109 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 109 | 110 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 109 | 111 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 109 | 111 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 109 | 110 |
| voice.robot | I would like to speak to the machine's warranty department. | 60 | 71 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 60 | 70 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 59 | 70 |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 58 | 70 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 58 | 79 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 58 | - |
| voice.robot | I'm reserving my dignity for the performance review. | 58 | 80 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 58 | 70 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 57 | 70 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 57 | 70 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 78 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 154 | 152 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 153 | 153 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 151 | 153 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 151 | 151 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 103 | 101 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 102 | 101 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 101 | 101 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 101 | 103 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 101 | 101 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 101 | 102 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 143 | 143 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 143 | 142 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 142 | 142 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 141 | 142 |
| voice.search | I have a system. The system involves suspicious containers. | 27 | 26 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 27 | 27 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 27 | 26 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 27 | - |
| voice.search | I consider this locker a patience seminar with hinges. | 26 | 25 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 26 | 25 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 26 | - |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 25 | 26 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | 24 | 25 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | 24 | 27 |
| voice.search | Every drawer is a tiny referendum on my priorities. | - | 25 |
| voice.search | I search this locker with the confidence of an expired warranty. | - | 25 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 24 | 73 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 24 | - |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 24 | 162 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 24 | 50 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 23 | 72 |
| voice.exit | I will board when ready. My dignity will require separate transport. | 23 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 23 | 161 |
| voice.exit | I'm rating this exit strategy above the alternative. | 23 | 50 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 23 | 163 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 23 | - |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 74 |
| voice.exit | The platform is a meeting place for me and a deadline. | - | 73 |
| voice.exit | The Hatch Authority calls it a departure. My knees call it a negotiation. | - | 50 |
| voice.consumable | I consider adhesive a bedside manner. | 35 | 34 |
| voice.consumable | I consider premium panic a billing category. | 35 | 34 |
| voice.consumable | I prefer medical tape without motivational quotes. | 35 | 35 |
| voice.consumable | My confidence comes in questionable packaging. | 35 | 35 |
| voice.consumable | My treatment plan needs a laminated apology. | 35 | 34 |
| voice.consumable | My dignity is not covered by the warranty. | 34 | 34 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 33 | 34 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 32 | 34 |
| voice.condition | I consider this forecast a workplace grievance. | 30 | 29 |
| voice.condition | My socks oppose the atmospheric agenda. | 30 | 27 |
| voice.condition | I prefer weather without administrative ambition. | 28 | 27 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 27 | 28 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 27 | 30 |
| voice.condition | My optimism needs a waterproof case. | 27 | 28 |

</details>

<details><summary>Brawler / 8h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.brawler | I have elected to make this problem regret its posture. | 448 | 298 |
| voice.brawler | I am applying confidence at a range best measured in apologies. | 447 | 296 |
| voice.brawler | My tactical reserve consists of forward momentum and one bandage. | 447 | 299 |
| voice.brawler | The plan is direct. The paperwork can catch up afterward. | 447 | 298 |
| voice.brawler | I am keeping my shield charged and my argument louder. | 446 | 299 |
| voice.brawler | I heard a problem. It appears to have volunteered for a meeting. | 446 | 299 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 77 | 75 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 77 | 76 |
| voice.diary-surveying | I distrust confidence sold without a map. | 76 | 75 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 76 | 75 |
| voice.diary-surveying | My sense of direction is on probation. | 76 | 76 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 75 | 76 |
| voice.diary-surveying | I consider signage a complicated relationship. | 74 | 75 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 74 | 77 |
| voice.diary-checking | I consider stitching more reliable than promises. | 72 | 70 |
| voice.diary-checking | My zipper has retained opposing counsel. | 72 | 70 |
| voice.diary-checking | I distrust buckles with management ambitions. | 71 | 71 |
| voice.diary-checking | My socks deserve a grievance procedure. | 71 | 71 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 70 | 71 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 70 | 70 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 69 | 70 |
| voice.diary-checking | My confidence is held together by adhesive. | 69 | 71 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 36 | 32 |
| voice.diary-general | I resent the retail markup on disappointment. | 33 | - |
| voice.diary-general | My curiosity has terrible credit. | 33 | - |
| voice.diary-general | My priorities need a returns policy. | 33 | - |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 32 | 32 |
| voice.diary-general | I prefer directions without a subscription. | 32 | 32 |
| voice.diary-general | My ambition exceeds the shelving specification. | 32 | 32 |
| voice.diary-general | My patience qualifies as antique stock. | 31 | 32 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | 30 | 32 |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | 30 | 32 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | - | 32 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | - | 32 |
| voice.diary-general | The Great Downstairs sounds less like an era and more like a basement committee. | - | 32 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 20 | 44 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 20 | 45 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 20 | 44 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 20 | 44 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 20 | 44 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 20 | 45 |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 18 | 32 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | 18 | - |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 18 | - |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 18 | - |
| voice.allocation | A coupon is a promise with arithmetic attached. | - | 32 |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | - | 32 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | - | 32 |
| voice.travel | I consider walking away a premium survival feature. | 21 | 33 |
| voice.travel | I'm budgeting my optimism for the return journey. | 21 | 33 |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 21 | 34 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 21 | - |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 20 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 20 | - |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 20 | 63 |
| voice.travel | Every pod has one seat and several opinions about the route. | 19 | 37 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | 19 | 38 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 19 | 65 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 66 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | - | 65 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 37 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 40 | 37 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 39 | 38 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 39 | 38 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 38 | 39 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 37 | 39 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 37 | 39 |
| voice.robot | I would like to speak to the machine's warranty department. | 21 | 24 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 21 | 23 |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 20 | 23 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 20 | 26 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 20 | 24 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 20 | 23 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 20 | - |
| voice.robot | I'm reserving my dignity for the performance review. | 20 | 28 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 20 | 25 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 19 | 24 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 27 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 53 | 52 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 52 | 51 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 52 | 52 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 50 | 52 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 36 | 34 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 35 | 34 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 35 | 33 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 34 | 35 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 34 | 35 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 33 | 36 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 50 | 49 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 49 | 49 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 48 | 48 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 48 | 49 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 34 | 32 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 33 | 32 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 32 | 32 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 32 | 33 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 32 | 32 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 31 | 33 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 47 | 46 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 46 | 46 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 46 | 46 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 45 | 46 |
| voice.search | I consider this locker a patience seminar with hinges. | 9 | 9 |
| voice.search | I have a system. The system involves suspicious containers. | 9 | - |
| voice.search | I search this locker with the confidence of an expired warranty. | 9 | 9 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 9 | 9 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 9 | 9 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 9 | 9 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 9 | - |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 9 | 9 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 9 | 9 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | 8 | 9 |
| voice.search | Every drawer is a tiny referendum on my priorities. | - | 9 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | - | 8 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 9 | 25 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 9 | 16 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 9 | 57 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 9 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 8 | 57 |
| voice.exit | I'm rating this exit strategy above the alternative. | 8 | 17 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 8 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | 8 | 26 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 7 | 24 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 7 | 57 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 25 |
| voice.exit | I will board when ready. My dignity will require separate transport. | - | 16 |
| voice.consumable | I consider adhesive a bedside manner. | 11 | 9 |
| voice.consumable | I prefer medical tape without motivational quotes. | 11 | 9 |
| voice.consumable | My treatment plan needs a laminated apology. | 11 | 10 |
| voice.consumable | I consider premium panic a billing category. | 9 | 10 |
| voice.consumable | My confidence comes in questionable packaging. | 9 | 9 |
| voice.consumable | My dignity is not covered by the warranty. | 9 | 10 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 9 | 10 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 8 | 10 |
| voice.condition | I consider this forecast a workplace grievance. | 12 | 11 |
| voice.condition | My socks oppose the atmospheric agenda. | 11 | 9 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 9 | 9 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 9 | 12 |
| voice.condition | I prefer weather without administrative ambition. | 9 | 9 |
| voice.condition | My optimism needs a waterproof case. | 9 | 9 |

</details>

<details><summary>Brawler / 24h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.brawler | I have elected to make this problem regret its posture. | 1376 | 930 |
| voice.brawler | I am applying confidence at a range best measured in apologies. | 1375 | 930 |
| voice.brawler | The plan is direct. The paperwork can catch up afterward. | 1375 | 929 |
| voice.brawler | I am keeping my shield charged and my argument louder. | 1374 | 930 |
| voice.brawler | I heard a problem. It appears to have volunteered for a meeting. | 1374 | 930 |
| voice.brawler | My tactical reserve consists of forward momentum and one bandage. | 1374 | 931 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 238 | 237 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 238 | 236 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 237 | 236 |
| voice.diary-surveying | I distrust confidence sold without a map. | 236 | 235 |
| voice.diary-surveying | My sense of direction is on probation. | 236 | 237 |
| voice.diary-surveying | I consider signage a complicated relationship. | 235 | 235 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 235 | 237 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 235 | 237 |
| voice.diary-checking | I consider stitching more reliable than promises. | 219 | 216 |
| voice.diary-checking | My socks deserve a grievance procedure. | 219 | 218 |
| voice.diary-checking | My zipper has retained opposing counsel. | 219 | 216 |
| voice.diary-checking | I distrust buckles with management ambitions. | 218 | 218 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 216 | 217 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 216 | 219 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 216 | 217 |
| voice.diary-checking | My confidence is held together by adhesive. | 216 | 218 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 102 | 99 |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 101 | 99 |
| voice.diary-general | I prefer directions without a subscription. | 101 | 100 |
| voice.diary-general | I resent the retail markup on disappointment. | 101 | 99 |
| voice.diary-general | My ambition exceeds the shelving specification. | 101 | 100 |
| voice.diary-general | My curiosity has terrible credit. | 101 | - |
| voice.diary-general | My priorities need a returns policy. | 101 | - |
| voice.diary-general | My patience qualifies as antique stock. | 100 | 99 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 99 | 99 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | 99 | - |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 100 |
| voice.diary-general | I trust the hub's directions as far as I can fold them. | - | 100 |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | - | 99 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 60 | 138 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 60 | 139 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 60 | 139 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 60 | 138 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 59 | 138 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 59 | 138 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 59 | - |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 59 | 103 |
| voice.allocation | A coupon is a promise with arithmetic attached. | 58 | - |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | 58 | 102 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | - | 104 |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | - | 103 |
| voice.travel | Every pod has one seat and several opinions about the route. | 60 | 109 |
| voice.travel | I consider walking away a premium survival feature. | 60 | - |
| voice.travel | I'm budgeting my optimism for the return journey. | 60 | - |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 60 | 103 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 60 | 102 |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 60 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 60 | 103 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 60 | 195 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 60 | 193 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | 59 | 194 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 196 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | - | 111 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 109 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 121 | 119 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 121 | 119 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 119 | 119 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 119 | 120 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 119 | 120 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 119 | 121 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 162 | 161 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 162 | 162 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 161 | 162 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 160 | 160 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 108 | 107 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 108 | 108 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 108 | 106 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 108 | 108 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 106 | 108 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 106 | 107 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 58 | 77 |
| voice.robot | I would like to speak to the machine's warranty department. | 58 | 69 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 58 | 69 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 58 | 69 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 58 | - |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 57 | 69 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 57 | 69 |
| voice.robot | I'm reserving my dignity for the performance review. | 57 | 80 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 57 | 70 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 57 | 69 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 77 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 152 | 152 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 152 | 152 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 151 | 150 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 150 | 151 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 102 | 100 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 102 | 100 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 101 | 101 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 100 | 100 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 99 | 101 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 99 | 101 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 145 | 145 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 144 | 144 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 144 | 144 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 144 | 144 |
| voice.search | I consider this locker a patience seminar with hinges. | 27 | 26 |
| voice.search | I have a system. The system involves suspicious containers. | 27 | - |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 27 | 27 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 27 | 27 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 27 | 26 |
| voice.search | Every drawer is a tiny referendum on my priorities. | 26 | 26 |
| voice.search | I search this locker with the confidence of an expired warranty. | 26 | 26 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 26 | 27 |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 26 | 27 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 26 | 26 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | - | 26 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 25 | 50 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 24 | 74 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 24 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 24 | 150 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 24 | 150 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 22 | 74 |
| voice.exit | I will board when ready. My dignity will require separate transport. | 22 | 49 |
| voice.exit | I'm rating this exit strategy above the alternative. | 22 | 49 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 22 | 152 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 22 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | - | 75 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 74 |
| voice.consumable | I consider adhesive a bedside manner. | 35 | 34 |
| voice.consumable | I consider premium panic a billing category. | 34 | 33 |
| voice.consumable | I prefer medical tape without motivational quotes. | 34 | 33 |
| voice.consumable | My confidence comes in questionable packaging. | 34 | 34 |
| voice.consumable | My treatment plan needs a laminated apology. | 34 | 34 |
| voice.consumable | My dignity is not covered by the warranty. | 33 | 34 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 33 | 34 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 33 | 34 |
| voice.condition | I consider this forecast a workplace grievance. | 30 | 30 |
| voice.condition | My socks oppose the atmospheric agenda. | 30 | 27 |
| voice.condition | I prefer weather without administrative ambition. | 29 | 28 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 27 | 27 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 27 | 30 |
| voice.condition | My optimism needs a waterproof case. | 27 | 28 |

</details>

<details><summary>Gadgeteer / 8h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.gadgeteer | I have a tool for this. The tool is currently evaluating me. | 443 | 293 |
| voice.gadgeteer | I packed the adapter. Now I need to identify what it adapts. | 443 | 293 |
| voice.gadgeteer | The equipment is ready. Its instructions remain theoretical. | 443 | 295 |
| voice.gadgeteer | This battery has been assigned three jobs and a supervisory title. | 443 | 293 |
| voice.gadgeteer | Every spare component is essential once the correct component is known. | 442 | 295 |
| voice.gadgeteer | I am optimizing the plan around the device I brought by mistake. | 442 | 295 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 80 | 79 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 80 | 79 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 79 | 77 |
| voice.diary-surveying | My sense of direction is on probation. | 78 | 78 |
| voice.diary-surveying | I consider signage a complicated relationship. | 77 | 77 |
| voice.diary-surveying | I distrust confidence sold without a map. | 77 | 77 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 77 | 79 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 77 | 79 |
| voice.diary-checking | I consider stitching more reliable than promises. | 74 | 72 |
| voice.diary-checking | I distrust buckles with management ambitions. | 74 | 73 |
| voice.diary-checking | My socks deserve a grievance procedure. | 74 | 74 |
| voice.diary-checking | My zipper has retained opposing counsel. | 74 | 72 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 73 | 72 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 72 | 74 |
| voice.diary-checking | My confidence is held together by adhesive. | 72 | 74 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 71 | 73 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 36 | 32 |
| voice.diary-general | My curiosity has terrible credit. | 36 | - |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 35 | 33 |
| voice.diary-general | I resent the retail markup on disappointment. | 35 | 32 |
| voice.diary-general | My priorities need a returns policy. | 35 | - |
| voice.diary-general | My ambition exceeds the shelving specification. | 34 | 34 |
| voice.diary-general | Desperanza calls this a home interval. My backpack calls it unpaid leave. | 31 | 32 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 31 | 32 |
| voice.diary-general | I prefer directions without a subscription. | 31 | 34 |
| voice.diary-general | My patience qualifies as antique stock. | 31 | 33 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | - | 33 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 32 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 20 | 44 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 20 | 45 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 20 | 45 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 20 | 44 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 19 | 44 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | 19 | 44 |
| voice.allocation | The Pawn Desk says every object has a future. Mine appears to involve paperwork. | 19 | - |
| voice.allocation | The Pawn Desk values this highly, which is how I know the price is emotional. | 19 | - |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 18 | 32 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | 18 | 33 |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | - | 32 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | - | 32 |
| voice.travel | I consider walking away a premium survival feature. | 21 | 34 |
| voice.travel | I'm budgeting my optimism for the return journey. | 21 | - |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 21 | 34 |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 21 | - |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 21 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 20 | 34 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 20 | 65 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | 20 | 65 |
| voice.travel | Every pod has one seat and several opinions about the route. | 19 | 36 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | 18 | 66 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | - | 64 |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | - | 37 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 35 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 41 | 38 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 40 | 39 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 40 | 38 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 38 | 40 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 38 | 40 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 38 | 40 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 22 | 28 |
| voice.robot | I would like to speak to the machine's warranty department. | 22 | 24 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 22 | 24 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 22 | 24 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 21 | - |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 20 | 25 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 20 | 25 |
| voice.robot | I'm reserving my dignity for the performance review. | 20 | 29 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 20 | 25 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 19 | 25 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 28 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 55 | 54 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 55 | 54 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 53 | 55 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 52 | 52 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 37 | 36 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 37 | 36 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 36 | 35 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 36 | 36 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 35 | 36 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 34 | 36 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 52 | 51 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 51 | 51 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 51 | 51 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 49 | 50 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 35 | 33 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 34 | 33 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 34 | 35 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 34 | 34 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 33 | 34 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 32 | 33 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 48 | 48 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 48 | 48 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 47 | 47 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 47 | 47 |
| voice.search | I consider this locker a patience seminar with hinges. | 9 | 9 |
| voice.search | I have a system. The system involves suspicious containers. | 9 | - |
| voice.search | I search this locker with the confidence of an expired warranty. | 9 | 9 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 9 | 9 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 9 | 9 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 9 | 9 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 9 | 8 |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 9 | - |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 9 | 9 |
| voice.search | Every drawer is a tiny referendum on my priorities. | 8 | 9 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | - | 9 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | - | 8 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 9 | 24 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 9 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 9 | 57 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 9 | 58 |
| voice.exit | I'm rating this exit strategy above the alternative. | 9 | 16 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 9 | 17 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 7 | 24 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 7 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | 7 | 25 |
| voice.exit | I will board when ready. My dignity will require separate transport. | 6 | 16 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | - | 59 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 24 |
| voice.consumable | I consider adhesive a bedside manner. | 11 | 9 |
| voice.consumable | My treatment plan needs a laminated apology. | 11 | 10 |
| voice.consumable | I consider premium panic a billing category. | 10 | 10 |
| voice.consumable | I prefer medical tape without motivational quotes. | 10 | 10 |
| voice.consumable | My confidence comes in questionable packaging. | 10 | 10 |
| voice.consumable | My dignity is not covered by the warranty. | 10 | 10 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 9 | 9 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 8 | 11 |
| voice.condition | I consider this forecast a workplace grievance. | 11 | 9 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 9 | 9 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 9 | 11 |
| voice.condition | I prefer weather without administrative ambition. | 9 | 9 |
| voice.condition | My optimism needs a waterproof case. | 9 | 9 |
| voice.condition | My socks oppose the atmospheric agenda. | 9 | 9 |

</details>

<details><summary>Gadgeteer / 24h</summary>

| Pool | Exact quote | Before count | After count |
|---|---|---:|---:|
| voice.gadgeteer | I packed the adapter. Now I need to identify what it adapts. | 1412 | 953 |
| voice.gadgeteer | This battery has been assigned three jobs and a supervisory title. | 1412 | 953 |
| voice.gadgeteer | I have a tool for this. The tool is currently evaluating me. | 1411 | 953 |
| voice.gadgeteer | Every spare component is essential once the correct component is known. | 1410 | 954 |
| voice.gadgeteer | I am optimizing the plan around the device I brought by mistake. | 1410 | 954 |
| voice.gadgeteer | The equipment is ready. Its instructions remain theoretical. | 1410 | 955 |
| voice.diary-surveying | I consider north a suggestion with excellent branding. | 242 | 241 |
| voice.diary-surveying | I distrust confidence sold without a map. | 242 | 241 |
| voice.diary-surveying | I prefer exits with fewer philosophical requirements. | 242 | 241 |
| voice.diary-surveying | My route notes need an optimism disclaimer. | 242 | 241 |
| voice.diary-surveying | My sense of direction is on probation. | 242 | 241 |
| voice.diary-surveying | I consider signage a complicated relationship. | 240 | 241 |
| voice.diary-surveying | My clipboard deserves hazard pay. | 240 | 242 |
| voice.diary-surveying | My reservations have acquired their own appendix. | 240 | 242 |
| voice.diary-checking | My zipper has retained opposing counsel. | 224 | 222 |
| voice.diary-checking | I consider stitching more reliable than promises. | 223 | 221 |
| voice.diary-checking | My socks deserve a grievance procedure. | 223 | 223 |
| voice.diary-checking | I distrust buckles with management ambitions. | 222 | 222 |
| voice.diary-checking | I prefer warranties without emotional exclusions. | 222 | 223 |
| voice.diary-checking | My baggage relationship needs firmer boundaries. | 222 | 223 |
| voice.diary-checking | My confidence is held together by adhesive. | 222 | 223 |
| voice.diary-checking | I consider preventive embarrassment a sound investment. | 221 | 222 |
| voice.diary-general | I consider empty shelves an aggressive sales strategy. | 103 | 101 |
| voice.diary-general | I consider nostalgia a poorly labeled aisle. | 102 | 101 |
| voice.diary-general | I resent the retail markup on disappointment. | 102 | 101 |
| voice.diary-general | My curiosity has terrible credit. | 102 | - |
| voice.diary-general | My priorities need a returns policy. | 102 | 101 |
| voice.diary-general | I consider lettuce custody a matter for Hydroponic Arguments. | 101 | 101 |
| voice.diary-general | I prefer directions without a subscription. | 101 | 101 |
| voice.diary-general | My ambition exceeds the shelving specification. | 101 | 101 |
| voice.diary-general | My patience qualifies as antique stock. | 101 | 101 |
| voice.diary-general | The Before-ish survives best on labels. The labels have very optimistic historians. | 101 | 101 |
| voice.diary-general | Civilization is one working stamp away from requesting another stamp. | - | 101 |
| voice.allocation | I call it strategic infrastructure. Don't ask. | 62 | 140 |
| voice.allocation | I'm putting this on the manifest, not on a pedestal. Yet. | 62 | 140 |
| voice.allocation | I consider sentimental value a perfectly legitimate accounting category. | 61 | 140 |
| voice.allocation | I'm keeping my justification brief: could be useful. | 61 | 140 |
| voice.allocation | A coupon is a promise with arithmetic attached. | 59 | - |
| voice.allocation | An OopsAll Logistics label proves intent, not delivery. | 59 | 104 |
| voice.allocation | Bravado Battery promises courage; the cell requests a quieter audience. | 59 | 104 |
| voice.allocation | ComfortCo guaranteed comfort until further notice. The notice is missing. | 59 | 104 |
| voice.allocation | Coupon Barons calculate discounts in a currency called almost enough. | 59 | 104 |
| voice.allocation | I can defend this decision in writing. Please don't request the writing. | 59 | 141 |
| voice.allocation | I'm treating this as the foundation of a very specific future strategy. | - | 141 |
| voice.travel | I'm calling this cardio. Desperanza can dispute the invoice. | 61 | 103 |
| voice.travel | Every pod has one seat and several opinions about the route. | 60 | 109 |
| voice.travel | I consider walking away a premium survival feature. | 60 | 102 |
| voice.travel | I trust the tube map exactly as far as I can fold it. | 60 | 195 |
| voice.travel | I'm budgeting my optimism for the return journey. | 60 | - |
| voice.travel | I'm following the route. My instincts are strictly advisory. | 60 | 103 |
| voice.travel | I'm traveling light, if emotional baggage doesn't count. | 60 | - |
| voice.travel | I'm treating every step as a small investment in dry socks. | 60 | - |
| voice.travel | The Hatch Authority handles departure forms. Gravity handles the rest. | 60 | 110 |
| voice.travel | The tube map has several arrows and no interest in follow-up questions. | 60 | 196 |
| voice.travel | Arrival is the part the schedule mentions in smaller print. | - | 196 |
| voice.travel | Tube Routes taught me that direct is an aspiration, not a measurement. | - | 195 |
| voice.travel | The surface is a short trip if measured in paperwork. | - | 109 |
| voice.start-surveying | I'm surveying the exits. Optimism is not a route marker. | 121 | 118 |
| voice.start-surveying | I consider this floor plan a strongly worded suggestion. | 120 | 120 |
| voice.start-surveying | I'm evaluating this route as an exit strategy, not a lifestyle. | 120 | 119 |
| voice.start-surveying | I'm reading the signage. My confidence has filed an appeal. | 120 | 120 |
| voice.start-surveying | I'm checking the sightlines. My dignity prefers an unobstructed escape. | 119 | 121 |
| voice.start-surveying | I'm taking field notes. The margins are mostly reservations. | 118 | 120 |
| voice.complete-surveying | I consider this survey adequate. Adequate is a generous department. | 168 | 166 |
| voice.complete-surveying | I'm filing the route notes under optimism with supporting evidence. | 168 | 168 |
| voice.complete-surveying | I'm promoting my reservations to a formal travel advisory. | 166 | 166 |
| voice.complete-surveying | I'm ready to stop reading the signs and start disputing the implications. | 166 | 168 |
| voice.start-checking | I'm checking my straps. I prefer my baggage attached. | 112 | 111 |
| voice.start-checking | I'm checking the footing. My socks deserve legal representation. | 112 | 111 |
| voice.start-checking | I'm arguing with my backpack zipper like it has legal standing. | 111 | 111 |
| voice.start-checking | I'm inspecting the buckles. Trust is good; industrial stitching is better. | 111 | 112 |
| voice.start-checking | I'm reviewing my equipment's commitment to remaining equipment. | 111 | 112 |
| voice.start-checking | I'm treating this inspection as preventive embarrassment. | 111 | 111 |
| voice.robot | I consider confidence a defense strategy with disappointing peer review. | 58 | 78 |
| voice.robot | I would like to speak to the machine's warranty department. | 58 | 69 |
| voice.robot | I'm entering strategic denial mode. It's a recognized posture. | 58 | 67 |
| voice.robot | I'm not arguing with a chassis. I'm lodging a very loud complaint. | 58 | 68 |
| voice.robot | I'm requesting that this be classified as a workplace disagreement. | 58 | - |
| voice.robot | I'm reserving my dignity for the performance review. | 57 | 81 |
| voice.robot | A.R.C. means Aggressively Roaming Chassis. I am hoping the aggression has office hours. | 56 | 68 |
| voice.robot | I would appeal to the chassis. It appears to be the entire department. | 56 | 70 |
| voice.robot | That A.R.C. chassis has the confidence of a machine with no supervisor. | 56 | 70 |
| voice.robot | That warning tone sounds like a disciplinary meeting with wheels. | 56 | 68 |
| voice.robot | We both have a maintenance schedule. Only one of us brought tools. | - | 78 |
| voice.complete-checking | I'm concluding the inspection before the zipper requests counsel. | 156 | 153 |
| voice.complete-checking | I consider my baggage relationship functional, pending appeal. | 153 | 154 |
| voice.complete-checking | I'm ready to proceed with a reasonable amount of unreasonable confidence. | 153 | 154 |
| voice.complete-checking | I'm signing off on the straps. My signature carries no warranty. | 153 | 154 |
| voice.start-rummaging | I'm rummaging for route clues, not another payday. Could be useful. | 104 | 102 |
| voice.start-rummaging | I'm approaching this empty shelf with professionally unreasonable expectations. | 103 | 103 |
| voice.start-rummaging | I'm inspecting empty fittings. Even my greed needs directions. | 103 | 103 |
| voice.start-rummaging | I consider a suspiciously philosophical label useful route intelligence. | 101 | 101 |
| voice.start-rummaging | I'm calling this reconnaissance so my priorities look respectable. | 101 | 102 |
| voice.start-rummaging | I'm conducting a clipboard-free study of salvageable regret. | 101 | 102 |
| voice.complete-rummaging | I'm calling this a thorough review of empty-space management. | 148 | 146 |
| voice.complete-rummaging | I consider my curiosity satisfied. My greed has submitted a dissent. | 146 | 147 |
| voice.complete-rummaging | I'm leaving the empty fittings off the souvenir manifest. | 146 | 145 |
| voice.complete-rummaging | I'm filing this under useful directions, questionable priorities. | 145 | 147 |
| voice.consumable | I consider adhesive a bedside manner. | 41 | 39 |
| voice.consumable | My treatment plan needs a laminated apology. | 41 | 39 |
| voice.consumable | I consider premium panic a billing category. | 39 | 40 |
| voice.consumable | I prefer medical tape without motivational quotes. | 39 | 39 |
| voice.consumable | My confidence comes in questionable packaging. | 39 | 39 |
| voice.consumable | My dignity is not covered by the warranty. | 39 | 40 |
| voice.consumable | Nearly Fresh calls this a snack. The wrapper declined to provide a date. | 39 | 38 |
| voice.consumable | The Water Ration Desk says hydration improves paperwork posture. | 38 | 41 |
| voice.search | I consider this locker a patience seminar with hinges. | 27 | 27 |
| voice.search | I have a system. The system involves suspicious containers. | 27 | 25 |
| voice.search | I'm conducting research. The clipboard would only introduce bias. | 27 | 27 |
| voice.search | I'm listening for the hum. I prefer my bad decisions with a soundtrack. | 27 | 27 |
| voice.search | I'm offering the zipper a brief prayer and no bargaining rights. | 27 | 27 |
| voice.search | I'm taking this search seriously. My dignity can wait outside. | 27 | 26 |
| voice.search | Nearly Fresh labels snacks by legal weather, not calendar. | 27 | 25 |
| voice.search | OopsAll Logistics defines delivered as the point at which nobody can find the package. | 27 | - |
| voice.search | I search this locker with the confidence of an expired warranty. | 25 | 27 |
| voice.search | A suspicious label is still more reliable than my shopping instinct. | 24 | - |
| voice.search | Every drawer is a tiny referendum on my priorities. | - | 27 |
| voice.search | Clipboard Researchers call it a sample. I call it a container with boundaries. | - | 26 |
| voice.exit | I'm ready for Desperanza. My dignity can take the next shuttle. | 25 | 52 |
| voice.exit | Departure windows are punctual in the way paperwork is punctual. | 24 | 75 |
| voice.exit | I consider boarding priority worth precisely none of my remaining dignity. | 24 | 74 |
| voice.exit | I consider leaving a tactical investment in future rummaging. | 24 | - |
| voice.exit | I'm keeping my panic optional and my paperwork accessible. | 24 | 169 |
| voice.exit | I'm making small talk with the timer. Strictly professional small talk. | 24 | 170 |
| voice.exit | I'm rating this exit strategy above the alternative. | 24 | 51 |
| voice.exit | My extraction plan was written on a receipt. The return policy is missing. | 24 | 170 |
| voice.exit | The Exit Strategy Booth charges extra for maps that acknowledge distance. | 24 | - |
| voice.exit | The platform is a meeting place for me and a deadline. | 24 | 76 |
| voice.exit | The Exit Strategy Booth uses nearby as a unit of distance. | - | 75 |
| voice.exit | I will board when ready. My dignity will require separate transport. | - | 52 |
| voice.condition | I consider this forecast a workplace grievance. | 30 | 29 |
| voice.condition | My socks oppose the atmospheric agenda. | 29 | 27 |
| voice.condition | Apology Weather sounds less personal when filed in triplicate. | 27 | 27 |
| voice.condition | I consider a forecast with no forms a suspiciously generous forecast. | 27 | 30 |
| voice.condition | I prefer weather without administrative ambition. | 27 | 27 |
| voice.condition | My optimism needs a waterproof case. | 27 | 27 |

</details>

### Hub entries beside unique lines per stay

Each cell is entries / unique exact lines; final observed stays may be partial. No resident reference or cadence change. Same ten-line pool; LRU changes order, not its size.

| Trait | Hours | Seed | Stay | Partial | Before entries / unique | After entries / unique |
|---|---:|---:|---:|---|---|---|
| Cautious | 8 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 8 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 2 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 101 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 101 | 4 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Cautious | 8 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 101 | 8 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 9 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 10 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 101 | 11 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 101 | 13 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 14 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 17 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 24 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 2 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 4 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 8 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 9 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 10 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 11 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 13 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 14 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 17 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 19 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 20 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 21 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 22 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 23 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 24 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 25 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 101 | 26 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 27 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 28 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 29 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 30 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 31 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 32 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 33 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 34 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 101 | 35 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 36 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 37 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 38 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 39 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 40 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 41 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 42 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 43 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 44 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 45 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 46 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 101 | 47 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 48 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 49 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 50 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 101 | 51 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 52 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 53 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 101 | 54 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 101 | 55 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 8 | 202 | 1 | False | 5 / 5 | 5 / 5 |
| Cautious | 8 | 202 | 2 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 4 | False | 6 / 6 | 6 / 6 |
| Cautious | 8 | 202 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 202 | 9 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 10 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 202 | 11 | False | 6 / 6 | 6 / 6 |
| Cautious | 8 | 202 | 12 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 14 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 202 | 17 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 202 | 18 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 202 | 19 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 202 | 20 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 24 | 202 | 1 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 202 | 2 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 4 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 202 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 9 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 10 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 11 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 202 | 12 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 14 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 17 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 18 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 19 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 20 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 21 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 22 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 23 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 24 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 25 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 26 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 27 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 28 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 29 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 30 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 31 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 202 | 32 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 33 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 34 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 35 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 36 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 37 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 38 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 39 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 40 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 41 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 42 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 43 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 44 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 45 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 202 | 46 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 202 | 47 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 48 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 202 | 49 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 50 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 51 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 52 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 53 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 54 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 55 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 56 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 202 | 57 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 202 | 58 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 202 | 59 | True | 1 / 1 | 1 / 1 |
| Cautious | 8 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 8 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Cautious | 8 | 303 | 2 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 6 | False | 5 / 5 | 5 / 5 |
| Cautious | 8 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 8 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 303 | 12 | False | 2 / 2 | 2 / 2 |
| Cautious | 8 | 303 | 13 | False | 6 / 6 | 6 / 6 |
| Cautious | 8 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Cautious | 8 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Cautious | 8 | 303 | 18 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Cautious | 24 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 2 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 3 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 6 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 8 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 12 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 13 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 18 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 19 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 20 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 21 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 22 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 23 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 24 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 25 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 26 | False | 6 / 6 | 6 / 6 |
| Cautious | 24 | 303 | 27 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 28 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 29 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 30 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 31 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 32 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 33 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 34 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 35 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 36 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 37 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 38 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 39 | False | 5 / 5 | 5 / 5 |
| Cautious | 24 | 303 | 40 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 41 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 42 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 43 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 44 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 45 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 46 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 47 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 48 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 49 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 50 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 51 | False | 2 / 2 | 2 / 2 |
| Cautious | 24 | 303 | 52 | False | 3 / 3 | 3 / 3 |
| Cautious | 24 | 303 | 53 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 54 | False | 4 / 4 | 4 / 4 |
| Cautious | 24 | 303 | 55 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 8 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 2 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 4 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 6 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 8 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 9 | False | 2 / 2 | 2 / 2 |
| Hoarder | 8 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 11 | False | 6 / 6 | 6 / 6 |
| Hoarder | 8 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 13 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 101 | 14 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 16 | False | 5 / 5 | 5 / 5 |
| Hoarder | 8 | 101 | 17 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 24 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 2 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 4 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 6 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 8 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 9 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 11 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 13 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 14 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 16 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 101 | 17 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 19 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 20 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 21 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 22 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 23 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 24 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 25 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 26 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 27 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 28 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 29 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 30 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 31 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 32 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 33 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 34 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 101 | 35 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 101 | 36 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 37 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 38 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 39 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 40 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 101 | 41 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 42 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 43 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 44 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 45 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 46 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 47 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 48 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 49 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 50 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 101 | 51 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 52 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 101 | 53 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 101 | 54 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 101 | 55 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 8 | 202 | 1 | False | 7 / 7 | 7 / 7 |
| Hoarder | 8 | 202 | 2 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 202 | 4 | False | 6 / 6 | 6 / 6 |
| Hoarder | 8 | 202 | 5 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 6 | False | 5 / 5 | 5 / 5 |
| Hoarder | 8 | 202 | 7 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 9 | False | 2 / 2 | 2 / 2 |
| Hoarder | 8 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 202 | 11 | False | 6 / 6 | 6 / 6 |
| Hoarder | 8 | 202 | 12 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Hoarder | 8 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 17 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 18 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 202 | 19 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 24 | 202 | 1 | False | 7 / 7 | 7 / 7 |
| Hoarder | 24 | 202 | 2 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 4 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 202 | 5 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 6 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 202 | 7 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 9 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 11 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 202 | 12 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 17 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 18 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 19 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 202 | 20 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 21 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 22 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 23 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 24 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 25 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 26 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 27 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 28 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 29 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 30 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 31 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 32 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 33 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 34 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 35 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 36 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 37 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 38 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 39 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 40 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 41 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 42 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 43 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 44 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 45 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 46 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 47 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 48 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 202 | 49 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 50 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 202 | 51 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 52 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 202 | 53 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 202 | 54 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 202 | 55 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 202 | 56 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 8 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Hoarder | 8 | 303 | 2 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 303 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 6 | False | 5 / 5 | 5 / 5 |
| Hoarder | 8 | 303 | 7 | False | 2 / 2 | 2 / 2 |
| Hoarder | 8 | 303 | 8 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 303 | 12 | False | 6 / 6 | 6 / 6 |
| Hoarder | 8 | 303 | 13 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Hoarder | 8 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Hoarder | 8 | 303 | 18 | False | 6 / 6 | 6 / 6 |
| Hoarder | 8 | 303 | 19 | True | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Hoarder | 24 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 2 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 3 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 6 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 7 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 8 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 12 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 303 | 13 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 18 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 303 | 19 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 303 | 20 | False | 6 / 6 | 6 / 6 |
| Hoarder | 24 | 303 | 21 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 22 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 23 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 24 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 25 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 26 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 27 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 28 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 29 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 30 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 31 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 32 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 33 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 34 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 35 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 36 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 37 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 38 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 39 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 40 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 41 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 42 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 43 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 44 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 45 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 46 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 47 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 48 | False | 5 / 5 | 5 / 5 |
| Hoarder | 24 | 303 | 49 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 50 | False | 3 / 3 | 3 / 3 |
| Hoarder | 24 | 303 | 51 | False | 4 / 4 | 4 / 4 |
| Hoarder | 24 | 303 | 52 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 53 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 54 | False | 2 / 2 | 2 / 2 |
| Hoarder | 24 | 303 | 55 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 8 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 2 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 3 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 4 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 8 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 9 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 13 | False | 5 / 5 | 5 / 5 |
| Brawler | 8 | 101 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 8 | 101 | 15 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 101 | 17 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 24 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 2 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 3 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 4 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 101 | 7 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 8 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 9 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 13 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 101 | 15 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 17 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 19 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 20 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 21 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 22 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 23 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 24 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 25 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 26 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 27 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 28 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 29 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 30 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 31 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 32 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 33 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 34 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 35 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 36 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 37 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 38 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 39 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 40 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 101 | 41 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 42 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 43 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 44 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 45 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 46 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 47 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 48 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 49 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 50 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 101 | 51 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 52 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 101 | 53 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 101 | 54 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 101 | 55 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 8 | 202 | 1 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 2 | False | 5 / 5 | 5 / 5 |
| Brawler | 8 | 202 | 3 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 4 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 5 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Brawler | 8 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 9 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 12 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 8 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 202 | 17 | False | 2 / 2 | 2 / 2 |
| Brawler | 8 | 202 | 18 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 202 | 19 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 24 | 202 | 1 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 2 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 202 | 3 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 4 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 5 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 9 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 12 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 16 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 17 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 18 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 19 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 20 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 21 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 22 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 23 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 24 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 25 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 26 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 27 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 28 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 29 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 30 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 202 | 31 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 202 | 32 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 33 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 34 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 202 | 35 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 36 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 37 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 202 | 38 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 39 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 40 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 41 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 42 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 43 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 44 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 45 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 46 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 202 | 47 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 48 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 202 | 49 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 50 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 51 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 52 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 53 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 202 | 54 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 202 | 55 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 202 | 56 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 202 | 57 | True | 4 / 4 | 4 / 4 |
| Brawler | 8 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 8 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Brawler | 8 | 303 | 2 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 3 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 4 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 6 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 8 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 303 | 12 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 303 | 13 | False | 5 / 5 | 5 / 5 |
| Brawler | 8 | 303 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 8 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Brawler | 8 | 303 | 16 | False | 4 / 4 | 4 / 4 |
| Brawler | 8 | 303 | 17 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 303 | 18 | False | 6 / 6 | 6 / 6 |
| Brawler | 8 | 303 | 19 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Brawler | 24 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 2 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 3 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 4 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 5 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 6 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 8 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 10 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 11 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 12 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 13 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 14 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 15 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 16 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 17 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 18 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 19 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 20 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 21 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 22 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 23 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 24 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 25 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 26 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 27 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 28 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 29 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 30 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 31 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 32 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 33 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 34 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 35 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 36 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 37 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 38 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 39 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 40 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 41 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 42 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 43 | False | 4 / 4 | 4 / 4 |
| Brawler | 24 | 303 | 44 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 45 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 46 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 47 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 48 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 49 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 50 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 51 | False | 2 / 2 | 2 / 2 |
| Brawler | 24 | 303 | 52 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 53 | False | 3 / 3 | 3 / 3 |
| Brawler | 24 | 303 | 54 | False | 6 / 6 | 6 / 6 |
| Brawler | 24 | 303 | 55 | False | 5 / 5 | 5 / 5 |
| Brawler | 24 | 303 | 56 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 8 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 2 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 101 | 3 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 101 | 4 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Gadgeteer | 8 | 101 | 7 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 8 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 101 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 11 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 101 | 13 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 101 | 14 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 101 | 17 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 24 | 101 | 1 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 2 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 3 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 4 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 5 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 6 | False | 6 / 6 | 6 / 6 |
| Gadgeteer | 24 | 101 | 7 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 8 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 10 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 11 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 12 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 13 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 14 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 15 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 17 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 18 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 19 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 20 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 21 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 22 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 23 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 24 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 101 | 25 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 26 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 101 | 27 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 28 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 29 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 30 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 31 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 32 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 33 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 34 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 35 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 36 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 37 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 38 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 39 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 40 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 41 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 42 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 43 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 44 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 45 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 46 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 101 | 47 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 48 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 49 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 50 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 51 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 52 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 101 | 53 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 101 | 54 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 8 | 202 | 1 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 202 | 2 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 8 | 202 | 3 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 202 | 4 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 202 | 5 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 202 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 11 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 8 | 202 | 12 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 202 | 17 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 8 | 202 | 18 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 202 | 19 | True | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 24 | 202 | 1 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 2 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 202 | 3 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 4 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 5 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 6 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 7 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 8 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 10 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 11 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 202 | 12 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 13 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 14 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 15 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 17 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 202 | 18 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 19 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 20 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 21 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 22 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 23 | False | 6 / 6 | 6 / 6 |
| Gadgeteer | 24 | 202 | 24 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 25 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 26 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 27 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 28 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 29 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 30 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 31 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 32 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 33 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 34 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 35 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 36 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 37 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 38 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 39 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 40 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 202 | 41 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 42 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 43 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 44 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 45 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 46 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 47 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 48 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 49 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 50 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 51 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 52 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 202 | 53 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 202 | 54 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 202 | 55 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 8 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 8 | 303 | 2 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 303 | 3 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 5 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 6 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 8 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 8 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 10 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 11 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 12 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 13 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 15 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 8 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 8 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 8 | 303 | 18 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 0 | False | 10 / 10 | 10 / 10 |
| Gadgeteer | 24 | 303 | 1 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 2 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 303 | 3 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 303 | 4 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 5 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 6 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 7 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 8 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 303 | 9 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 10 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 11 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 12 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 13 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 14 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 15 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 16 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 17 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 18 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 19 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 20 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 21 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 22 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 23 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 24 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 25 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 26 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 27 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 28 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 303 | 29 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 30 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 31 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 32 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 33 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 34 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 35 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 36 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 37 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 38 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 39 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 40 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 41 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 42 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 43 | False | 2 / 2 | 2 / 2 |
| Gadgeteer | 24 | 303 | 44 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 45 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 46 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 47 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 48 | False | 4 / 4 | 4 / 4 |
| Gadgeteer | 24 | 303 | 49 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 50 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 51 | False | 5 / 5 | 5 / 5 |
| Gadgeteer | 24 | 303 | 52 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 53 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 54 | False | 3 / 3 | 3 / 3 |
| Gadgeteer | 24 | 303 | 55 | False | 3 / 3 | 3 / 3 |
