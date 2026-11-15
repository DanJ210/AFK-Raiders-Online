# DIARY VARIETY: candidate asides for human review

**C3 revised draft: 624 candidates in 25 subfamilies; human review pending.**
No candidate is approved, catalogued, or loaded.

All 592 C2 IDs are retained, including seven unchanged keep lines; 32
ActivityInterrupted candidates are added. All 64 PersonalityChanged lines
were rewritten. The [C3 report](#automated-review-checks) uses the same strict
grounding screen on the unchanged C2 baseline and this draft. Old C2 grounding
annotations are not strict evidence. No line is approved or ready for loading.

Wording is original, informed by AFK-original canon, not copied source prose.
The read-only original `afk-raiders` revision inspected was
`f62d088a41da951a1fbe9ed7215d2957a933ce50`: `src/content/robots.json`,
`src/content/zones/zones.json`, `src/content/zones/zone_conditions.json`, and
`src/content/healing_items.json`; the earlier intake also covered
`src/content/trait_events.json`, `src/content/loot-tables/personal_junk.json`,
and `src/content/weapons.json`. This document imports nothing into runtime.

## Presentation and eligibility

[journal.ts](../web/src/journal.ts) `diaryPresentation` and
[DiaryEntryText.vue](../web/src/components/DiaryEntryText.vue) render the
committed facts before `\nRaider: `. PersonalityChanged presents its main reason;
numbers and other details remain available. ExtractionDecision presents its reason
and heading/staying decision; other pilot facts remain unchanged. Asides add
voice, not a second summary or a substitute receipt.
**Yes, the factual consequence stays visible.** Personality numeric deltas and
exit odds/Greed move to expandable **Event details**, not into the aside; the full
original receipt remains accessible. The existing
[presentation tests](../web/tests/journal.test.mjs) verified these distinctions
and the native expandable record: four selected tests passed on 2026-11-13.
This was source/test verification, not a new browser session or runtime load.

All rows have rarity **common**. Required event facts and slots must already be
committed and visible to the recipient. Missing slots make a row ineligible:
select another or retain existing narration, never fabricate a value. In
particular `{location}` is the committed location, not a newly invented setting;
`{condition}` is the committed condition; `{source}` and `{robot}` must occur
in the exact original committed text/thread, never an opponent inferred later.
`{item}` requires an exact item **and recipient** in committed shared awards
for SI: SearchCompleted and EncounterResolved do not directly supply ItemName.
HS instead uses its directly committed preserved ItemName/instance receipt.
Shared awards are not automatically the speaker's. `{weapon}` is not a generally
committed pilot field and is not authorized by these rules.

Shapes: **J** places the joke before a contextual noun slot near the end; **M**
integrates the slot into a grammatical clause; **O** uses no slot and makes an
event-specific joke. None repeats the fact receipt. Each trait/subfamily needs
all three shapes and at least four lines. C3 concrete evidence is an actual
physical object, including an explicit hypothetical comparison used in the
line; an exact canon name gated by a committed fact; or an actually used
committed non-location slot. Location alone never counts. Neither an ungrounded
named entity nor a direction word by itself counts. Decisions and reviewer
notes remain blank for human review.

The six actual modes are Gear obsession, Overconfidence, False optimism, Genre
tension, Loot greed, and Bureaucracy. Cautious practical survival understatement
must use the mode actually expressed, not a seventh “Risk-awareness” mode.
Targets for every trait/subfamily: at least 60% concrete-noun lines, no more
than `floor(n/8)` False optimism lines, and at most three lines using any
content stem, including the combined change-synonym stem. Across each trait, My and I openers are each
at most 12%, each actual mode is 6%-22%, and office/record content is at most 12%.
The earlier C2 ordinary-stem targets (two/set, eight/trait) and per-set office
target are retained as separately reported **unresolved drafting screens**,
not silently described as passes. The C3 mode requirement needs procedural
jokes; one office line in an eight-line set is already 12.5%, including the
unchanged keep SH-B-03. This arithmetic conflict needs designer resolution
before loading; no exception changes that line or its honest office flag.
For the stem count only, exclude committed replacement slots; count all other
content words, including canon anchors and gag words.
Per set and across the draft, at most 15% of lines may contain the whole words `next`,
`perhaps`, `may`, `could`, `still`, or `remain`, including their inflections.
Trait office content is at most 12% both by keyword and manual metaphor review,
independent of Mode. The [check report](#automated-review-checks) lists the
conservative union, including technical record/drawing metaphors even when their
actual joke mode is Gear obsession. These are drafting targets, not a claim
that the draft has passed human approval.

Names and tone were checked against the [lore index](lore/README.md),
[mapping](lore/PARODY_MAPPING.md), [robots](lore/entities/robots.md),
[zones](lore/entities/zones.md), [field phenomena](lore/entities/field-phenomena.md),
[tone](lore/content-guidelines/TONE_VOICE.md), and
[legal guardrails](lore/content-guidelines/LEGAL_GUARDRAILS.md). Entry Tunnel,
Market, Salvage Yard, Service Depot, and Extraction Platform are current online
runtime location anchors, not assertions that they occur in the lore map. Named
lore locations, robots and phenomena are not interchangeable with committed
runtime facts. Only the dotted spelling A.R.C. is permitted. No resident
references; a literal Tea Kettle analogy would not establish ownership.
Megaspoon Works appears in [factions and brands](lore/entities/factions-and-brands.md)
but has no row in the [mapping](lore/PARODY_MAPPING.md); this is a mapping gap,
not authority to add the brand to an event. Named variants require exact
committed names and recipient visibility, and availability in the online
runtime is not established by legacy canon.

**KEEP, wording and mode:** SH-B-03, SH-B-06, SH-C-06, RS-C-03, RS-C-06,
EL-G-03, EL-B-03. This is separate from the human Decision and Reviewer note;
it does not approve any line for selection or loading.

### Committed lore gates and mapping flags

| Candidates | Exact committed gate | Lore use / mapping status |
|---|---|---|
| HW-B-02, HW-H-02 | ConditionName = Overtime Red in this warning's receipt | Closing-time atmosphere, not an invented closing event; [field phenomena](lore/entities/field-phenomena.md), existing mapping row. |
| HW-C-02, HW-G-02 | ConditionName = Apology Weather in this warning's receipt | Apology versus an umbrella or hypothetical wet board; same existing mapping. |
| RD-B-02, RD-H-02 | RobotName = Roomba Prime in this defeat's receipt | Aggressive housekeeping becomes a silent performance/sculpture; [robots](lore/entities/robots.md), existing mapping. |
| RD-C-02 | RobotName = Anxietick in this defeat's receipt | Nervous-machine identity; existing small-machine mapping. |
| RD-G-02 | RobotName = Passive-Aggressor Drone in this defeat's receipt | Disapproval as an imaginary cabinet compartment; existing drone mapping. |
| HS-B-03, HS-C-03, HS-G-03, HS-H-03 | Preserved ItemName = Single Sock / Single Sock / Left Boot / Vintage Comic Book respectively | Exact existing items, not renamed drops or new branded items. |

No candidate needs a new PARODY_MAPPING row. Megaspoon Works would need one
before a future recurring named variant; it is **not used in any candidate**.
Bravado Battery, ComfortCo, Nearly Fresh, OopsAll Logistics, HushTone and Exit
Strategy Unlimited have existing mapping coverage but no matching committed
brand/item fact in these pilot kinds has been established. They are deliberately
not name-dropped. A lore zone also cannot replace a committed node such as
Market; no invented landmark is placed there. Missing exact gates exclude the
named variants, leaving generic rows or the existing narration available.
These are author-checked lore constraints, not implemented eligibility filters.

### REVIEW THESE FIRST — first proposed load batch (152 IDs)

Human review order: proven keep jokes, the repeat-heavy interruption family,
positive shield absorption, resilience, extraction heading, committed finds,
then the strongest generic personality comparisons. This is the first
**proposed loadable batch after approval**, not a ready-to-load catalog.
Every line remains draft-only. Loading requires separate authorization,
selection/slot implementation, eligibility tests, and resolution of the
legacy screens below. No extractor or pool is installed by this task.
Direction-specific PC lines stay in backlog; they require additional exact
receipt parsing and sign gates. Activity-slot candidates are included because
the event already supplies Activity, but its substitution is also unimplemented.
The other 472 IDs are backlog. Review these first, in this order:

1. SH-B-03, SH-B-06, SH-C-06, RS-C-03, RS-C-06, EL-G-03, EL-B-03.
2. AI-B-01, AI-B-02, AI-B-03, AI-B-04, AI-B-05, AI-B-06, AI-B-07, AI-B-08;
   AI-C-01, AI-C-02, AI-C-03, AI-C-04, AI-C-05, AI-C-06, AI-C-07, AI-C-08;
   AI-G-01, AI-G-02, AI-G-03, AI-G-04, AI-G-05, AI-G-06, AI-G-07, AI-G-08;
   AI-H-01, AI-H-02, AI-H-03, AI-H-04, AI-H-05, AI-H-06, AI-H-07, AI-H-08.
3. SH-B-01, SH-B-02, SH-B-04, SH-B-05, SH-B-07, SH-B-08;
   SH-C-01, SH-C-02, SH-C-03, SH-C-04, SH-C-05, SH-C-07, SH-C-08;
   SH-G-01, SH-G-02, SH-G-03, SH-G-04, SH-G-05, SH-G-06, SH-G-07, SH-G-08;
   SH-H-01, SH-H-02, SH-H-03, SH-H-04, SH-H-05, SH-H-06, SH-H-07, SH-H-08.
4. RS-B-01, RS-B-02, RS-B-03, RS-B-04, RS-B-05, RS-B-06, RS-B-07, RS-B-08;
   RS-C-01, RS-C-02, RS-C-04, RS-C-05, RS-C-07, RS-C-08;
   RS-G-01, RS-G-02, RS-G-03, RS-G-04, RS-G-05, RS-G-06, RS-G-07, RS-G-08;
   RS-H-01, RS-H-02, RS-H-03, RS-H-04, RS-H-05, RS-H-06, RS-H-07, RS-H-08.
5. EL-B-01, EL-B-02, EL-B-04, EL-B-05, EL-B-06, EL-B-07, EL-B-08;
   EL-C-01, EL-C-02, EL-C-03, EL-C-04, EL-C-05, EL-C-06, EL-C-07, EL-C-08;
   EL-G-01, EL-G-02, EL-G-04, EL-G-05, EL-G-06, EL-G-07, EL-G-08;
   EL-H-01, EL-H-02, EL-H-03, EL-H-04, EL-H-05, EL-H-06, EL-H-07, EL-H-08.
6. SI-B-01, SI-B-04, SI-C-01, SI-C-04, SI-G-01, SI-G-04, SI-H-01, SI-H-04.
7. PC-B-03, PC-B-06, PC-B-09, PC-B-12;
   PC-C-03, PC-C-06, PC-C-09, PC-C-12;
   PC-G-03, PC-G-06, PC-G-09, PC-G-12;
   PC-H-03, PC-H-06, PC-H-09, PC-H-12.

## Sizing and rotation hypotheses

Counts are per trait over 24 hours in the [Part A/B audit](DIARY_VARIETY_AUDIT.md),
not independent balance or repeat guarantees. Paired starts/completions are
separate streams. Split shares are unknown where only a combined count exists.
Sizing uses a drafting convention, not an audit classification: the dominant PC
stream gets 16; observed streams around 1% or above get eight; lower-share streams
get four. The shared search upper bound supports eight recovered-item lines;
empty/unmeasured streams provisionally get four. Both extraction outcomes get
eight conservatively. Unknown splits are not measured rarity.

| Code | Subfamily | Observed per-trait count / share | Current per trait | Expected spacing / repeat risk |
|---|---|---|---:|---|
| PC | PersonalityChanged | 3357–3455 / 16.33–16.41% | 16 | ≈98; clustering or eligibility can bring repeats inside 50 |
| SH | ShieldAbsorbed, positive charge loss only | all SH 549–580 / 2.66–2.82%; eligible subset unknown | 8 | ≥284 under all-SH bound; subset and bursts unknown |
| UD | UnmitigatedDamage | 14–38 / 0.07–0.18% | 4 | ≈2222–5714; bursts remain possible |
| RS | Resilience | 411–487 / 2.00–2.31% | 8 | ≈346–400; source eligibility can shrink rotation |
| EL | ExtractionDecision heading | combined 437–494 / 2.12–2.34%; split unknown | 8 | ≥342 under combined bound; split and clustering unknown |
| ES | ExtractionDecision staying | combined 437–494 / 2.12–2.34%; split unknown | 8 | ≥342 under combined bound; split and clustering unknown |
| SI | Search/encounter item recovered | SearchCompleted 167–171 / 0.79–0.83%; EncounterResolved 138–144 / 0.67–0.69%; split unknown | 8 | ≥526 using combined upper bounds; item eligibility shrinks rotation |
| SN | Search/encounter nothing recovered | same combined bounds as SI; split unknown | 4 | ≥263 using combined upper bounds; split unknown |
| SX | OpportunityExhausted | 0; unmeasured | 4 | unknown; clustering possible |
| AR | Arrived | 767–781 / 3.69–3.80% | 8 | ≈211–217; bursts remain possible |
| AI | ActivityInterrupted | 153–156 / 0.74–0.75% | 8 | ≈1051–1072 under an even stream; eligibility and clustering can shorten this |
| AD | DutyAdmission / Deployment / Departure | admission 167–171 / 0.79–0.83%; deployment and departure unmeasured | 4 | unknown combined share; admission alone ≈482–506 |
| PS | PackingStarted | 409–418 / 1.98–2.01% | 8 | ≈398–404; clustering possible |
| PF | PackingCompleted | 409–418 / 1.98–2.01% | 8 | ≈398–404; clustering possible |
| HW | HazardWarning | 237–280 / 1.15–1.33% | 8 | ≈602–696; clustering possible |
| HI | ConditionImpact | 258–287 / 1.25–1.36% | 8 | ≈588–640; clustering possible |
| RD | RobotDefeated | 99–107 / 0.48–0.51% | 4 | ≈784–833; robot eligibility can shrink rotation |
| RX | RobotEscaped | 50–61 / 0.24–0.30% | 4 | ≈1333–1667; bursts remain possible |
| RC | RobotDefeatCargoResolved | 99–107 / 0.48–0.51% | 4 | ≈784–833; both outcomes share this set |
| RV | RecoveryStarted / RecoveryCompleted | 0; unmeasured | 4 | unknown; both phases share this set |
| DN | Downed | 13–26 / 0.06–0.13% | 4 | ≈3077–6667; bursts remain possible |
| KO | KnockedOut | 13–26 / 0.06–0.13% | 4 | ≈3077–6667; bursts remain possible |
| DL | DeadlineExpired | 0; unmeasured | 4 | unknown; clustering possible |
| HS | HiddenPocketSalvaged | 0; unmeasured | 4 | unknown; clustering possible |
| HE | HiddenPocketEmpty | 13–26 / 0.06–0.13% | 4 | ≈3077–6667; bursts remain possible |

The search pools conservatively give recovered events eight and empty events
four without assuming a measured split. The extraction pools each use the
combined upper bound, not an invented split. Zero means unmeasured, not rare or
unreachable. SH excludes zero-charge-loss events; the audit does not measure that
eligibility split, and those events retain existing narration. Under an even
mixed stream, PC's `16 / 0.164` gives roughly 98
quotes per rotation, beyond 50. Spacing estimates assume even traffic and fully
eligible pools; clustering and reduced eligibility can shorten them considerably. Lower-share sets usually have more spacing, but clusters,
shared eligibility, and correlated events can defeat that intuition. These are
drafting hypotheses, not observed post-change repeat rates.

## Not covered by this draft's target

The audit reports AreaDiary at 5274–5416 per trait, 25.65–25.87%; its
surveying/checking/general source pools contain 8/8/17 lines, with previous-50
repeats after Part B of 1.24–1.47% (previous-20: zero). Activity starts are 1957–1999,
9.48–9.60%, with six-line pools; completions are 1810–1868, 8.80–8.89%,
with four-line pools. In the 24h audit both had zero previous-20 and previous-50
repeats. Travel is 767–782, 3.69–3.80%, pool 13; ItemAllocated is 818–836,
3.95–4.02%, pool 14; RobotRound is 313–334, 1.48–1.62%, pool 11.
Those are separate existing pools, not expanded here. Shared extraction uses a
seven-line event-fit subset of the existing exit pool; ActivityInterrupted
currently remains on trait/general fallback at runtime, although the AI draft
below now proposes dedicated lines. Travel, ItemAllocated,
RobotRound and SharedExtractionCalled had zero previous-20/50 repeats in the
24h audit. ActivityInterrupted is a disproportionate remaining repeat driver:
in Cautious/Hoarder/Brawler/Gadgeteer order, previous-20 repeats were
1/155, 57/153, 67/155, 64/156; previous-50 were 90/155, 153/153, 155/155,
156/156 (58.06% for Cautious, 100% for the other three). These area/activity
pools and other fallthroughs are not covered by this pilot. No new gameplay audit or
post-draft repeat measurement is implied.

## AI — ActivityInterrupted

**Required:** the recipient's committed `ActivityInterrupted`, including the
exact `activity` and location. `{activity}` denotes only that Raider's
interrupted, unfinished activity. It does not establish an award, completion,
another participant's interruption, or a change to any shared deadline. The
event is emitted by `InterruptForExit` after an extraction decision; Traveling
returns without emitting this event. An actually used `{activity}` qualifies
as a committed non-location slot, not as a physical object.
The props in these lines are hypothetical comparisons, not possessions or acts.
The activity slot extractor and this selection pool are not implemented.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-B-01 | Overconfidence | J | `{activity}` | "A bell couldn't boss me around better than the interruption of {activity}." | bell | no | AI: comparison only; activity interrupted, not finished. |  |  |
| AI-B-02 | Genre tension | M | `{activity}` | "That interruption of {activity} deserves a drumroll with the drum missing." | drum | no | AI: hypothetical drum; no completion. |  |  |
| AI-B-03 | Gear obsession | O | - | "Even a helmet has to make room for an abrupt change of plan." | helmet | no | AI: comparison; no equipment claimed. |  |  |
| AI-B-04 | Loot greed | J | `{activity}` | "I'd trade a trophy for a less abrupt ending to {activity}." | trophy | no | AI: hypothetical trade; no loot or completion. |  |  |
| AI-B-05 | Genre tension | M | `{activity}` | "Put {activity} beside an unfinished fireworks display in the dramatic endings collection." | fireworks | no | AI: figurative comparison; no explosion. |  |  |
| AI-B-06 | Overconfidence | J | `{activity}` | "A boxing glove would make a gentler point than interrupting {activity}." | boxing glove | no | AI: comparison only; no fight. |  |  |
| AI-B-07 | Gear obsession | M | `{activity}` | "Stopping {activity} feels like putting a lid on a trumpet." | lid, trumpet | no | AI: comparison, no equipment or result. |  |  |
| AI-B-08 | False optimism | O | - | "Here's hoping future interruptions come with a champion's belt." | belt | no | AI: wish only, no later outcome or award. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-C-01 | Genre tension | J | `{activity}` | "A handrail sounds reassuring beside the interruption of {activity}." | handrail | no | AI: hypothetical handrail, no completion. |  |  |
| AI-C-02 | Gear obsession | M | `{activity}` | "Stopping {activity} calls to mind a knot left untied." | knot | no | AI: comparison, no actual knot. |  |  |
| AI-C-03 | Overconfidence | O | - | "A compass wouldn't make an interrupted task feel tidier." | compass | no | AI: hypothetical compass, no route guarantee. |  |  |
| AI-C-04 | Loot greed | J | `{activity}` | "I'd rather lose a hypothetical button than forget why I stopped {activity}." | button | no | AI: no item loss; interrupted activity only. |  |  |
| AI-C-05 | Genre tension | M | `{activity}` | "The pause in {activity} has the charm of a chair missing a leg." | chair | no | AI: comparison, no actual chair or finish. |  |  |
| AI-C-06 | Gear obsession | J | `{activity}` | "Even a sturdy buckle can't fasten an ending onto {activity}." | buckle | no | AI: hypothetical buckle, no completion. |  |  |
| AI-C-07 | Overconfidence | M | `{activity}` | "I'll leave {activity} unfinished without awarding it a paper crown." | crown | no | AI: figurative award only; no activity result. |  |  |
| AI-C-08 | False optimism | O | - | "Here's hoping future interruptions land on a cushion." | cushion | no | AI: wish only, no later outcome. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-G-01 | Gear obsession | J | `{activity}` | "A loose spring makes a reasonable mascot for interrupted {activity}." | spring | no | AI: hypothetical spring, no item or completion. |  |  |
| AI-G-02 | Genre tension | M | `{activity}` | "The interruption of {activity} feels like a radio cut off mid-song." | radio | no | AI: comparison, no transmission. |  |  |
| AI-G-03 | Overconfidence | O | - | "I can object to that timing without taking apart a clock." | clock | no | AI: hypothetical clock, no timer altered. |  |  |
| AI-G-04 | Loot greed | J | `{activity}` | "I'd swap a hypothetical washer for another moment of {activity}." | washer | no | AI: no transaction, added time or completion. |  |  |
| AI-G-05 | Gear obsession | M | `{activity}` | "Stopping {activity} resembles pulling a plug before the kettle sings." | plug, kettle | no | AI: hypothetical apparatus, no physical action. |  |  |
| AI-G-06 | Genre tension | J | `{activity}` | "A film reel would be equally annoyed by the interruption of {activity}." | film reel | no | AI: comparison, no projection. |  |  |
| AI-G-07 | Overconfidence | M | `{activity}` | "Interrupting {activity} beats a screwdriver at finding awkward angles." | screwdriver | no | AI: comparison, no tool used. |  |  |
| AI-G-08 | False optimism | O | - | "The optimistic version has a pause button that waits its turn." | button | no | AI: hypothetical button, no timing change. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-H-01 | Bureaucracy | J | `{activity}` | "An interruption form should have a velvet seal for {activity}." | form, seal | yes | AI: hypothetical paperwork; no award or finish. |  |  |
| AI-H-02 | Genre tension | M | `{activity}` | "The interruption of {activity} has the elegance of a dropped teacup." | teacup | no | AI: comparison, no broken item. |  |  |
| AI-H-03 | Overconfidence | O | - | "Being cut off can't outbid my talent for being offended." | - | no | AI: figurative bid; no transaction or completion. |  |  |
| AI-H-04 | Overconfidence | J | `{activity}` | "A jeweled crown would improve the dignity of leaving {activity} unfinished." | crown | no | AI: hypothetical crown; no completion or item. |  |  |
| AI-H-05 | Overconfidence | M | `{activity}` | "Even {activity} can't compete with the size of my offended sigh." | `{activity}` | no | AI: opinion only; no extra time or completion. |  |  |
| AI-H-06 | Genre tension | J | `{activity}` | "A curtain falling halfway down would flatter the interruption of {activity}." | curtain | no | AI: comparison only, no stage. |  |  |
| AI-H-07 | Gear obsession | M | `{activity}` | "Stopping {activity} feels like a zipper refusing to close on a thought." | zipper | no | AI: comparison, no equipment state. |  |  |
| AI-H-08 | False optimism | O | - | "Here's hoping unfinished business comes wrapped in a satin ribbon." | ribbon | no | AI: wish only, no acquisition or later outcome. |  |  |

## PC — PersonalityChanged

**Required:** kind, committed reason and location. The reason is already
presented, and the original receipt retains the actual old and new Greed and
Mood values after clamping. There are no structured old/new fields. A future
extractor could derive `{greedDirection}` or `{moodDirection}` from the exact
formatted receipt only when that pair is unequal; never infer either direction
from the reason, current state, or an attempted pre-clamp change. Such
extraction is unimplemented; direction-dependent rows are ineligible until
implemented and validated. No numbers belong in the aside. Use `{location}`
only as committed context, never as a strict concrete anchor.
Every direction row additionally requires the **specific sign** stated in its
fact-check cell: up-only or down-only. Substituting the opposite sign would
invalidate the joke. Unchanged dimensions provide no direction slot. The
generic comparison rows remain eligible for either actual change.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| PC-B-01 | Overconfidence | J | `{greedDirection}` | "A trophy cabinet would surrender before this appetite; greed went {greedDirection}." | trophy cabinet | no | PC: Greed up only, exact clamped receipt; hypothetical cabinet. |  |  |
| PC-B-02 | Genre tension | M | `{moodDirection}` | "Mood is {moodDirection}; cue the trumpet with a sock stuffed in it." | trumpet, sock | no | PC: Mood down only, exact clamped receipt; hypothetical muted fanfare. |  |  |
| PC-B-03 | Gear obsession | O | - | "This temper fits like a helmet two sizes too proud." | helmet | no | PC: comparison, no equipment claimed. |  |  |
| PC-B-04 | Overconfidence | J | `{location}` | "A boxing glove has nothing on this expression at {location}." | boxing glove | no | PC: comparison, no fight. |  |  |
| PC-B-05 | Genre tension | M | `{location}` | "At {location}, my thoughts have the subtlety of a gong." | gong | no | PC: comparison, no sound or actor. |  |  |
| PC-B-06 | Loot greed | O | - | "Even an imaginary medal would struggle to match this appetite." | medal | no | PC: opinion, no acquisition or greed direction. |  |  |
| PC-B-07 | Overconfidence | J | `{greedDirection}` | "A crown can wait. Restraint finally met a worthy opponent; greed went {greedDirection}." | crown | no | PC: Greed down only, exact clamped receipt; figurative opponent, no fight. |  |  |
| PC-B-08 | Genre tension | M | `{location}` | "At {location}, a curtain would struggle to contain this attitude." | curtain | no | PC: hypothetical curtain, no performance. |  |  |
| PC-B-09 | Gear obsession | O | - | "This resolve has the aerodynamics of a thrown brick." | brick | no | PC: comparison, no thrown object claimed. |  |  |
| PC-B-10 | Overconfidence | J | `{moodDirection}` | "A megaphone would beg me to contain this good cheer; mood went {moodDirection}." | megaphone | no | PC: Mood up only, exact clamped receipt; hypothetical device. |  |  |
| PC-B-11 | Genre tension | M | `{location}` | "At {location}, this expression could frighten a painted shield." | shield | no | PC: hypothetical painted shield, no equipped shield. |  |  |
| PC-B-12 | False optimism | O | - | "With luck this mood comes with a championship belt." | belt | no | PC: wish only, no actual prize. |  |  |
| PC-B-13 | Loot greed | J | `{location}` | "A golden chalice would envy this much enthusiasm at {location}." | chalice | no | PC: hypothetical object, no acquisition. |  |  |
| PC-B-14 | Gear obsession | M | `{location}` | "At {location}, my nerve resembles an overinflated punching bag." | punching bag | no | PC: comparison, no fight. |  |  |
| PC-B-15 | Genre tension | O | - | "A cheap strap has a calmer disposition than this." | strap | no | PC: comparison, no equipment state. |  |  |
| PC-B-16 | Bureaucracy | J | `{location}` | "A medal for dramatic paperwork would suit my expression at {location}." | medal | yes | PC: hypothetical medal, no official award. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-C-01 | Genre tension | J | `{moodDirection}` | "A handrail sounds better than an ovation just now; mood went {moodDirection}." | handrail | no | PC: Mood down only, exact clamped receipt; hypothetical support, no equipment. |  |  |
| PC-C-02 | Gear obsession | M | `{location}` | "At {location}, a buckle seems more persuasive than a flourish." | buckle | no | PC: comparison, no carried gear. |  |  |
| PC-C-03 | Genre tension | O | - | "An ovation is a poor substitute for a railing." | railing | no | PC: comparison, no railing asserted. |  |  |
| PC-C-04 | Overconfidence | J | `{greedDirection}` | "A whistle isn't needed. Even my appetite listens to restraint; greed went {greedDirection}." | whistle | no | PC: Greed down only, exact clamped receipt; no sound or new decision. |  |  |
| PC-C-05 | Genre tension | M | `{location}` | "At {location}, this composure resembles a carefully stacked teacup." | teacup | no | PC: comparison, no object claimed. |  |  |
| PC-C-06 | Loot greed | O | - | "A chipped cup has more appeal than a heroic flourish." | cup | no | PC: hypothetical comparison, no acquisition or delta direction. |  |  |
| PC-C-07 | Overconfidence | J | `{location}` | "A parachute sounds more reassuring than bravado at {location}." | parachute | no | PC: comparison, no travel or possession. |  |  |
| PC-C-08 | Genre tension | M | `{location}` | "At {location}, a metronome would find my nerves rather slow." | metronome | no | PC: hypothetical instrument, no measurement. |  |  |
| PC-C-09 | Loot greed | O | - | "A seesaw seems appropriate for loot and exits." | seesaw | no | PC: comparison, no item or decision. |  |  |
| PC-C-10 | Gear obsession | J | `{moodDirection}` | "A compass with a cheerful needle is preferable to fireworks; mood went {moodDirection}." | compass | no | PC: Mood up only, exact clamped receipt; hypothetical instrument. |  |  |
| PC-C-11 | Genre tension | M | `{location}` | "At {location}, quiet shoes sound preferable to a trumpet." | shoes, trumpet | no | PC: comparison, no equipment or sound. |  |  |
| PC-C-12 | False optimism | O | - | "Here's hoping this mood is a handrail rather than a banana peel." | handrail | no | PC: hope and comparison, no future result. |  |  |
| PC-C-13 | Overconfidence | J | `{location}` | "No brass band is required for this disposition at {location}." | brass band | no | PC: hypothetical instruments, no movement. |  |  |
| PC-C-14 | Gear obsession | M | `{location}` | "At {location}, my caution would choose a lantern over fireworks." | lantern, fireworks | no | PC: hypothetical comparison, no threat inferred. |  |  |
| PC-C-15 | Loot greed | O | - | "Give me a hypothetical first-aid tin over a priceless fanfare." | first-aid tin | no | PC: comparison, no item or injury. |  |  |
| PC-C-16 | Bureaucracy | J | `{location}` | "A caution label would suit this expression at {location}." | label | yes | PC: hypothetical label, no real warning. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-G-01 | Gear obsession | J | `{greedDirection}` | "An adjustable wrench ought to widen with the parts appetite; greed went {greedDirection}." | wrench | no | PC: Greed up only, exact clamped receipt; hypothetical tool, no repair. |  |  |
| PC-G-02 | Genre tension | M | `{location}` | "At {location}, my thoughts resemble mismatched cogs in a toy clock." | cogs, clock | no | PC: comparison, no mechanism claimed. |  |  |
| PC-G-03 | Gear obsession | O | - | "These instincts have all the elegance of a bent hinge." | hinge | no | PC: comparison, no damaged gear. |  |  |
| PC-G-04 | Overconfidence | J | `{moodDirection}` | "A gauge couldn't measure this much smug satisfaction; mood is {moodDirection}." | gauge | no | PC: Mood up only, exact clamped receipt; no actual measurement. |  |  |
| PC-G-05 | Genre tension | M | `{location}` | "At {location}, my curiosity resembles a kettle with theatrical ambitions." | kettle | no | PC: comparison, no equipment or behavior. |  |  |
| PC-G-06 | Loot greed | O | - | "A spare cog would make excellent emotional support, in theory." | cog | no | PC: hypothetical object, no acquisition or greed direction. |  |  |
| PC-G-07 | Gear obsession | J | `{location}` | "A crooked rivet would feel understood by my reasoning at {location}." | rivet | no | PC: comparison, no repair or tool use. |  |  |
| PC-G-08 | Bureaucracy | M | `{location}` | "At {location}, a warning label would look underdressed beside my curiosity." | label | yes | PC: hypothetical label, no real warning. |  |  |
| PC-G-09 | Overconfidence | O | - | "No multimeter is needed to admire this much enthusiasm." | multimeter | no | PC: hypothetical tool, no mood-only inference. |  |  |
| PC-G-10 | Loot greed | J | `{greedDirection}` | "A jeweled bolt sounds almost excessive. Almost; greed went {greedDirection}." | bolt | no | PC: Greed down only, exact clamped receipt; no acquisition or value. |  |  |
| PC-G-11 | Genre tension | M | `{location}` | "At {location}, my imagination resembles a telescope pointed at its own lens." | telescope | no | PC: comparison, no observation. |  |  |
| PC-G-12 | False optimism | O | - | "With luck this outlook runs longer than a bargain battery." | battery | no | PC: hope and comparison, no future outcome. |  |  |
| PC-G-13 | Gear obsession | J | `{location}` | "A loose screw would call this judgment artistic at {location}." | screw | no | PC: comparison, no equipment state. |  |  |
| PC-G-14 | Overconfidence | M | `{location}` | "At {location}, a flywheel would envy my theoretical composure." | flywheel | no | PC: hypothetical machine, no measured effect. |  |  |
| PC-G-15 | Genre tension | O | - | "This intuition wobbles like an unbalanced gyroscope." | gyroscope | no | PC: comparison, no machine asserted. |  |  |
| PC-G-16 | Loot greed | J | `{moodDirection}` | "A gold-plated hammer sounds like consolation rather than excess; mood went {moodDirection}." | hammer | no | PC: Mood down only, exact clamped receipt; hypothetical object, no award. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-H-01 | Loot greed | J | `{greedDirection}` | "A velvet pedestal for every bottle cap sounds insufficient; greed went {greedDirection}." | pedestal, bottle cap | no | PC: Greed up only, exact clamped receipt; hypothetical objects, no find. |  |  |
| PC-H-02 | Genre tension | M | `{location}` | "At {location}, my thoughts resemble antiques wearing tiny crowns." | crowns | no | PC: comparison, no possession or appraisal. |  |  |
| PC-H-03 | Loot greed | O | - | "A bent spoon sounds more persuasive than restraint." | spoon | no | PC: hypothetical object, no acquisition. |  |  |
| PC-H-04 | Overconfidence | J | `{moodDirection}` | "A diamond would envy the sparkle in this enthusiasm; mood is {moodDirection}." | diamond | no | PC: Mood up only, exact clamped receipt; hypothetical gemstone. |  |  |
| PC-H-05 | Gear obsession | M | `{location}` | "At {location}, a pouch seems like a sensible answer to everything." | pouch | no | PC: hypothetical pouch, no inventory claim. |  |  |
| PC-H-06 | Loot greed | O | - | "A bottle cap deserves more respect than a reasonable limit." | bottle cap | no | PC: opinion, no award or greed direction. |  |  |
| PC-H-07 | Genre tension | J | `{location}` | "An empty display case would admire this level of longing at {location}." | display case | no | PC: hypothetical case, no valuation. |  |  |
| PC-H-08 | Bureaucracy | M | `{location}` | "At {location}, a price tag would struggle to keep up with this enthusiasm." | price tag | yes | PC: hypothetical tag, no actual appraisal. |  |  |
| PC-H-09 | Gear obsession | O | - | "A backpack with a secret compartment sounds like emotional support." | backpack | no | PC: hypothetical comparison, no capacity claim. |  |  |
| PC-H-10 | Overconfidence | J | `{location}` | "A golden goblet has less confidence than this opinion at {location}." | goblet | no | PC: comparison, no loot produced. |  |  |
| PC-H-11 | Loot greed | M | `{location}` | "At {location}, even a hypothetical washer has sentimental appeal." | washer | no | PC: subjective comparison, no item or real value. |  |  |
| PC-H-12 | False optimism | O | - | "With luck this appetite belongs in a velvet jewelry box." | box | no | PC: hope and comparison, no future result. |  |  |
| PC-H-13 | Gear obsession | J | `{greedDirection}` | "A zipper needn't brace for quite so much ambition; greed went {greedDirection}." | zipper | no | PC: Greed down only, exact clamped receipt; no backpack contents or capacity. |  |  |
| PC-H-14 | Genre tension | M | `{location}` | "At {location}, my enthusiasm looks like a chandelier dressed for a coronation." | chandelier | no | PC: comparison, no actual object. |  |  |
| PC-H-15 | Gear obsession | O | - | "A spare key would be worth more than a sensible plan." | key | no | PC: hypothetical comparison, no healing or item. |  |  |
| PC-H-16 | Loot greed | J | `{moodDirection}` | "A jeweled box would make a fine home for this disappointment; mood went {moodDirection}." | box | no | PC: Mood down only, exact clamped receipt; hypothetical consolation, no item. |  |  |

## SH — ShieldAbsorbed

**Required:** committed kind, location, and strictly positive charge lost in the
original receipt. A zero charge loss is possible, including after resilience;
these candidates are ineligible then and existing narration remains. Charge
loss does not prove a positive HP reduction. Do not infer the attacker,
remaining charge, total protection, or HP damage. `{location}` is context; no
source is supplied by this event.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-B-01 | Gear obsession | J | `{location}` | "That barrier spends its energy louder than I argue at {location}." | `{location}` | no | SH: no source or remaining charge. |  |  |
| SH-B-02 | False optimism | M | `{location}` | "At {location}, here's hoping that shield made its last dramatic entrance." | shield | no | SH: hope only; no charge balance or future guarantee. |  |  |
| SH-B-03 | Genre tension | O | - | "My shield paid the cover charge for that entrance." | shield | yes | SH: figurative payment; no remaining balance asserted. |  |  |
| SH-B-04 | Gear obsession | J | `{location}` | "That defensive expense met a louder opinion at {location}." | `{location}` | no | SH: no shield balance. |  |  |
| SH-B-05 | Overconfidence | M | `{location}` | "At {location}, my barrier insists on arguing first." | `{location}` | no | SH: figurative shield, no source asserted. |  |  |
| SH-B-06 | Genre tension | O | - | "That shield charge went out swinging." | shield | no | SH: absorption only, no additional hit. |  |  |
| SH-B-07 | Loot greed | J | `{location}` | "That spent energy was worth more than a jeweled trophy at {location}." | trophy | no | SH: opinion, no additional equipment or value claimed. |  |  |
| SH-B-08 | Bureaucracy | M | `{location}` | "At {location}, the barrier would like a receipt for its energy bill." | barrier, receipt | yes | SH: figurative receipt, no balance or complete prevention. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-C-01 | Gear obsession | J | `{location}` | "A barrier is useful, not a promise, at {location}." | `{location}` | no | SH: no balance or immunity claim. |  |  |
| SH-C-02 | False optimism | M | `{location}` | "At {location}, here's hoping the shield has supplied enough excitement." | shield | no | SH: hope only; no source or future outcome. |  |  |
| SH-C-03 | Gear obsession | O | - | "A shield spending charge is not an invitation to get careless." | shield | no | SH: no remaining charge inferred. |  |  |
| SH-C-04 | Overconfidence | J | `{location}` | "That barrier expense deserves a nod rather than a parade at {location}." | barrier | no | SH: no numeric margin asserted. |  |  |
| SH-C-05 | Bureaucracy | M | `{location}` | "At {location}, protection deserves a receipt for its trouble." | receipt | yes | SH: imagined receipt, no origin or damage outcome. |  |  |
| SH-C-06 | Gear obsession | O | - | "Even a good shield is an umbrella, not a roof." | shield | no | SH: comparison, no immunity claimed. |  |  |
| SH-C-07 | Loot greed | J | `{location}` | "No trinket is worth forgetting that protection at {location}." | `{location}` | no | SH: no acquired item or decision. |  |  |
| SH-C-08 | Genre tension | M | `{location}` | "The charge spent at {location} bought a moment of caution." | `{location}` | no | SH: figurative purchase, no future event. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-G-01 | Gear obsession | J | `{location}` | "That barrier spent energy with impeccable timing at {location}." | `{location}` | no | SH: no mechanism or balance inferred. |  |  |
| SH-G-02 | Gear obsession | M | `{location}` | "At {location}, the defensive discharge contributed to my field test." | `{location}` | no | SH: metaphorical experiment, no source. |  |  |
| SH-G-03 | Gear obsession | O | - | "A shield converts charge into a very persuasive argument." | shield | no | SH: no charge remaining or HP result. |  |  |
| SH-G-04 | Bureaucracy | J | `{location}` | "The barrier deserves an itemized receipt for that expenditure at {location}." | barrier, receipt | yes | SH: hypothetical receipt, no remaining charge. |  |  |
| SH-G-05 | False optimism | M | `{location}` | "At {location}, here's hoping that barrier expenditure was the final surprise." | barrier | no | SH: hope only; no remaining charge or outcome. |  |  |
| SH-G-06 | Genre tension | O | - | "A shield spends charge with all the subtlety of a brass band." | shield | no | SH: hypothetical band, no added hit or outcome. |  |  |
| SH-G-07 | Loot greed | J | `{location}` | "I'd frame that energy cost as a fine specimen at {location}." | `{location}` | no | SH: no remaining charge or duration inferred. |  |  |
| SH-G-08 | Genre tension | M | `{location}` | "At {location}, the shield spent power with the timing of a stage curtain." | shield, curtain | no | SH: comparison, no balance claim. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-H-01 | False optimism | J | `{location}` | "With luck that shield won't need to spend another spark at {location}." | shield | no | SH: wish only, no remaining charge or guarantee. |  |  |
| SH-H-02 | Bureaucracy | M | `{location}` | "At {location}, the shield's expense deserves a stamped receipt." | shield, receipt | yes | SH: hypothetical receipt, no remaining charge. |  |  |
| SH-H-03 | Loot greed | O | - | "Spent charge ought to come with a velvet display case." | - | no | SH: figurative case, no charge remaining. |  |  |
| SH-H-04 | Overconfidence | J | `{location}` | "I'd price that energy cost above gold at {location}." | `{location}` | no | SH: opinion, not an assessment of charge. |  |  |
| SH-H-05 | Gear obsession | M | `{location}` | "The defensive expense at {location} deserves a place among my treasures." | `{location}` | no | SH: figurative collection, no transfer. |  |  |
| SH-H-06 | Loot greed | O | - | "Watching charge leave a shield feels like auctioning heirlooms." | shield | yes | SH: comparison, no actual sale. |  |  |
| SH-H-07 | Loot greed | J | `{location}` | "That protective expenditure was worth a crown at {location}." | `{location}` | no | SH: hypothetical valuation, no purchase. |  |  |
| SH-H-08 | Genre tension | M | `{location}` | "At {location}, my barrier offered a generous donation to my peace of mind." | `{location}` | yes | SH: figurative donation, no guaranteed survival. |  |  |

## UD — UnmitigatedDamage

**Required:** kind and committed source for J/M candidates. `{source}` must
match the original committed text/thread exactly. Resilience can reduce raw
damage to zero: no line assumes an HP wound or a later outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-B-01 | Overconfidence | J | `{source}` | "Skipping my shield is a bold introduction from {source}." | `{source}` | no | UD: exact source, no HP wound assumed. |  |  |
| UD-B-02 | Genre tension | M | `{source}` | "That delivery from {source} bypassed the polite layer." | `{source}` | no | UD: shield path bypassed, no wound or actor inferred. |  |  |
| UD-B-03 | Genre tension | O | - | "The shield was apparently not invited to this particular argument." | shield | no | UD: unshielded path only, no HP delta asserted. |  |  |
| UD-B-04 | Bureaucracy | J | `{source}` | "The shield-bypass complaint form names its suspect: {source}." | `{source}`, form | yes | UD: figurative complaint, no injury inferred. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-C-01 | Bureaucracy | J | `{source}` | "The shield-bypass form has an unwelcome entry: {source}." | `{source}`, form | yes | UD: hypothetical form, no HP loss assumed. |  |  |
| UD-C-02 | Bureaucracy | M | `{source}` | "The shield's absence from the equation involving {source} is noted." | `{source}` | yes | UD: record metaphor, no remaining charge or injury claimed. |  |  |
| UD-C-03 | Genre tension | O | - | "Unshielded damage is an inconvenient way to test caution." | - | no | UD: raw damage path only; no HP amount. |  |  |
| UD-C-04 | Overconfidence | J | `{source}` | "I won't mistake an uncovered hit for a warning from {source}." | `{source}` | no | UD: source exact; no wound or forecast. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-G-01 | Bureaucracy | J | `{source}` | "The bypass form has an unwelcome entry: {source}." | `{source}`, form | yes | UD: hypothetical form; no HP loss or charge level. |  |  |
| UD-G-02 | Genre tension | M | `{source}` | "Even {source} seems unimpressed by an absent umbrella." | `{source}`, umbrella | no | UD: shield bypass comparison; no HP loss assumed. |  |  |
| UD-G-03 | Loot greed | O | - | "A hypothetical copper wire would be more welcome than unshielded damage." | wire | no | UD: hypothetical object, no acquisition or HP claim. |  |  |
| UD-G-04 | Overconfidence | J | `{source}` | "A bypass deserves a glare, especially from {source}." | `{source}` | no | UD: no actual test or HP loss. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| UD-H-01 | Bureaucracy | J | `{source}` | "The complaint form should charge admission to {source}." | `{source}`, form | yes | UD: hypothetical form, no transaction or wound. |  |  |
| UD-H-02 | Genre tension | M | `{source}` | "Even {source} shouldn't get free passage past a shield." | `{source}`, shield | no | UD: unshielded path only; no HP outcome. |  |  |
| UD-H-03 | Gear obsession | O | - | "An unshielded hit makes every scrap of padding look priceless." | - | no | UD: no padding possessed or injury assumed. |  |  |
| UD-H-04 | Overconfidence | J | `{source}` | "A diamond would be a more welcome visitor than {source}." | diamond, `{source}` | no | UD: hypothetical diamond; no outcome inferred. |  |  |

## RS — Resilience

**Required:** kind and committed source for J/M candidates. `{source}` must
match the original committed text/thread exactly. The reduction does not
guarantee a wound, healing, or survival.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| RS-B-01 | Loot greed | J | `{source}` | "A trophy for stubbornness would suit the reduced sting of {source}." | trophy, `{source}` | no | RS: hypothetical trophy, no HP result. |  |  |
| RS-B-02 | Bureaucracy | M | `{source}` | "For {source}, my grit filed a request for less damage." | `{source}`, request | yes | RS: figurative request; mitigation only. |  |  |
| RS-B-03 | Gear obsession | O | - | "Grit is a dented helmet for damage, not an off switch." | helmet, switch | no | RS: comparison; reduction, not complete prevention. |  |  |
| RS-B-04 | Overconfidence | J | `{source}` | "I refuse to give the full satisfaction to {source}." | `{source}` | no | RS: exact source; no opponent inferred. |  |  |
| RS-B-05 | Genre tension | M | `{source}` | "The worst of {source} got an argument from my resolve." | `{source}` | no | RS: mitigation only; no zero-damage claim. |  |  |
| RS-B-06 | Loot greed | O | - | "My backbone just negotiated a discount on the damage." | - | yes | RS: figurative discount, no health or trade. |  |  |
| RS-B-07 | False optimism | J | `{source}` | "With luck my stubborn streak has made a lasting impression on {source}." | `{source}` | no | RS: hope only; no full protection or future result. |  |  |
| RS-B-08 | Gear obsession | M | `{source}` | "Against {source}, resolve works like a cushion, not a shield." | `{source}`, cushion | no | RS: comparison, no shield or later outcome. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-C-01 | Loot greed | J | `{source}` | "The damage spared is worth more than a marble from {source}." | `{source}`, marble | no | RS: hypothetical marble, no healing or zero damage. |  |  |
| RS-C-02 | Gear obsession | M | `{source}` | "My resolve gave {source} a little less leverage." | `{source}` | no | RS: reduction only, no gear claimed. |  |  |
| RS-C-03 | Genre tension | O | - | "Reducing a hit is not the same as declining it." | - | no | RS: no health amount or outcome. |  |  |
| RS-C-04 | Overconfidence | J | `{source}` | "I appreciate a smaller contribution from {source}." | `{source}` | no | RS: no numeric margin or survival claim. |  |  |
| RS-C-05 | Gear obsession | M | `{source}` | "My shock absorber of a temperament took the edge off {source} today." | `{source}` | no | RS: metaphorical absorber, no equipment. |  |  |
| RS-C-06 | Genre tension | O | - | "Resilience is a cushion, not an exemption from consequences." | - | no | RS: no zero-damage or wound guarantee. |  |  |
| RS-C-07 | Overconfidence | J | `{source}` | "I'll accept the reduced version of {source}." | `{source}` | no | RS: no amount or consequence inferred. |  |  |
| RS-C-08 | Genre tension | M | `{source}` | "My composure gave {source} a less generous reception." | `{source}` | no | RS: no recovery or injury asserted. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| RS-G-01 | Gear obsession | J | `{source}` | "My built-in damping took the edge off {source}." | `{source}` | no | RS: figurative damping, no hardware or healing. |  |  |
| RS-G-02 | Gear obsession | M | `{source}` | "My resolve buffered {source} without promising a perfect seal." | `{source}` | no | RS: reduction, no HP guarantee. |  |  |
| RS-G-03 | Gear obsession | O | - | "Resilience is a shock absorber for damage, not an off switch." | - | no | RS: no measured value or full prevention. |  |  |
| RS-G-04 | Bureaucracy | J | `{source}` | "The damage report earns a correction for reduced input from {source}." | `{source}`, report | yes | RS: hypothetical report; reduction only. |  |  |
| RS-G-05 | Genre tension | M | `{source}` | "The drama of {source} got a softer landing than advertised." | `{source}` | no | RS: reduction only, no healing or zero damage. |  |  |
| RS-G-06 | Gear obsession | O | - | "The damage curve just met a stubborn bend." | - | no | RS: reduction only; no measurement. |  |  |
| RS-G-07 | Gear obsession | J | `{source}` | "My improvised resilience adds a little damping to {source}." | `{source}` | no | RS: metaphor, no item held. |  |  |
| RS-G-08 | Overconfidence | M | `{source}` | "Against {source}, my tolerance for impact has engineering ambitions." | `{source}` | no | RS: reduction only, no outcome. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-H-01 | Loot greed | J | `{source}` | "That saved sliver of damage deserves a velvet box: {source}." | `{source}` | no | RS: reduction only; no purchase or item. |  |  |
| RS-H-02 | Genre tension | M | `{source}` | "My resilience made {source} less costly than advertised." | `{source}` | no | RS: figurative cost; no acquired item. |  |  |
| RS-H-03 | Gear obsession | O | - | "Mitigation is worth more than a suitcase full of compliments." | suitcase | no | RS: hypothetical comparison, no physical case or healing. |  |  |
| RS-H-04 | False optimism | J | `{source}` | "With luck grit will spare me even more damage from {source}." | `{source}` | no | RS: wish only, no full prevention guaranteed. |  |  |
| RS-H-05 | Overconfidence | M | `{source}` | "My toughness charges {source} an outrageous entrance fee." | `{source}` | yes | RS: figurative fee, no transaction. |  |  |
| RS-H-06 | Overconfidence | O | - | "Even damage has to settle for a smaller portion of my attention." | - | no | RS: mitigation only, no health guarantee. |  |  |
| RS-H-07 | Genre tension | J | `{source}` | "I prize a smaller share of the unpleasantness from {source}." | `{source}` | no | RS: no health assertion. |  |  |
| RS-H-08 | Gear obsession | M | `{source}` | "My resolve wraps {source} in bargain padding before it lands." | `{source}` | no | RS: figurative mitigation, no equipment. |  |  |

## EL — ExtractionDecision, heading

**Required:** kind, committed heading decision, reason and location. The reason
and direction are already presented; neither arrival, boarding nor extraction
success follows from the decision.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| EL-B-01 | False optimism | J | `{location}` | "With luck the exit deserves a champion's belt after {location}." | belt | no | EL: wish only, no completed extraction. |  |  |
| EL-B-02 | Loot greed | M | `{location}` | "At {location}, even a hypothetical trophy would need a way home." | trophy | no | EL: imagined object, no cargo or arrival. |  |  |
| EL-B-03 | Gear obsession | O | - | "My boots know a dramatic direction when they hear one." | - | no | EL: figurative boots, no arrival. |  |  |
| EL-B-04 | Gear obsession | J | `{location}` | "A hypothetical pair of boots would march very proudly from {location}." | boots | no | EL: imagined boots, no completed journey. |  |  |
| EL-B-05 | Genre tension | M | `{location}` | "The farewell to {location} deserves an imaginary drum." | drum | no | EL: no actual music or physical departure. |  |  |
| EL-B-06 | Loot greed | O | - | "Even a trophy needs a route home." | - | no | EL: comparison, no cargo claim. |  |  |
| EL-B-07 | Bureaucracy | J | `{location}` | "The exit application has a very aggressive signature at {location}." | signature | yes | EL: figurative paperwork, no arrival or fight. |  |  |
| EL-B-08 | Genre tension | M | `{location}` | "The curtain over {location} can wait for my bow." | `{location}` | no | EL: figurative curtain; no extraction success. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-C-01 | False optimism | J | `{location}` | "Here's hoping the way home needs less than an emergency compass from {location}." | compass | no | EL: wish only, no successful exit. |  |  |
| EL-C-02 | Gear obsession | M | `{location}` | "At {location}, my internal compass points toward discretion." | `{location}` | no | EL: figurative compass, no navigation result. |  |  |
| EL-C-03 | Genre tension | O | - | "Leaving is the part of the plan that requires actual feet." | - | no | EL: no arrival or movement completed. |  |  |
| EL-C-04 | Loot greed | J | `{location}` | "An imaginary marble isn't worth forgetting the exit from {location}." | marble | no | EL: no acquisition or completed departure. |  |  |
| EL-C-05 | Overconfidence | M | `{location}` | "Leaving {location} sounds less elaborate than a parade float." | float | no | EL: hypothetical float, no arrival. |  |  |
| EL-C-06 | Gear obsession | O | - | "A map is most persuasive when it points toward home." | - | no | EL: comparison, no route safety implied. |  |  |
| EL-C-07 | Loot greed | J | `{location}` | "My appetite will have to argue with the exit from {location}." | `{location}` | no | EL: no item or abandoned loot. |  |  |
| EL-C-08 | Loot greed | M | `{location}` | "From {location}, a bottle cap sounds less persuasive than the exit." | bottle cap | no | EL: hypothetical cap; heading only. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-G-01 | Genre tension | J | `{location}` | "An imaginary curtain would make a dignified exit cue at {location}." | curtain | no | EL: hypothetical curtain, no arrival. |  |  |
| EL-G-02 | False optimism | M | `{location}` | "At {location}, with luck the route needs less engineering than a clock." | clock | no | EL: wish only; no route guarantee or possession. |  |  |
| EL-G-03 | Gear obsession | O | - | "Even a prototype needs an off switch." | - | no | EL: analogy, no equipment or shutdown. |  |  |
| EL-G-04 | Overconfidence | J | `{location}` | "I've solved the direction problem, starting with {location}." | `{location}` | no | EL: direction decision, not successful travel. |  |  |
| EL-G-05 | Overconfidence | M | `{location}` | "Leaving {location} sounds less complicated than a clockwork goose." | - | no | EL: comparison, no safe route asserted. |  |  |
| EL-G-06 | Gear obsession | O | - | "A well-designed lever also knows when to release." | - | no | EL: analogy, no action. |  |  |
| EL-G-07 | Loot greed | J | `{location}` | "An imagined cog would complain about leaving {location}." | cog | no | EL: no loot left behind. |  |  |
| EL-G-08 | Overconfidence | M | `{location}` | "From {location}, I can at least calculate which way is out." | `{location}` | no | EL: heading decision only, no completed journey. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-H-01 | False optimism | J | `{location}` | "Here's hoping home has room for a jeweled box after {location}." | box | no | EL: wish only; no item claimed or arrival. |  |  |
| EL-H-02 | Gear obsession | M | `{location}` | "At {location}, a hypothetical pouch would object to departure." | pouch | no | EL: no item or abandonment asserted. |  |  |
| EL-H-03 | Gear obsession | O | - | "My pockets object to the very idea of departure." | - | no | EL: figurative, no inventory claim. |  |  |
| EL-H-04 | Overconfidence | J | `{location}` | "A golden throne would envy my dignified exit plan from {location}." | throne | no | EL: hypothetical throne, no departure or prize. |  |  |
| EL-H-05 | Genre tension | M | `{location}` | "Departing {location} feels like leaving a shop before the curtain rises." | curtain | no | EL: comparison, no shop, discovery or arrival. |  |  |
| EL-H-06 | Overconfidence | O | - | "A jeweled compass would have no trouble admitting it's time to leave." | compass | no | EL: hypothetical compass; heading only. |  |  |
| EL-H-07 | Loot greed | J | `{location}` | "My appetite for keepsakes disagrees politely with leaving {location}." | `{location}` | no | EL: no keepsake or successful departure. |  |  |
| EL-H-08 | Gear obsession | M | `{location}` | "At {location}, my hoarding instinct protests the exit arrow." | `{location}` | no | EL: metaphor; no actual contents. |  |  |

## ES — ExtractionDecision, staying

**Required:** kind, committed staying decision, reason and location. Staying
means now, not a permanent refusal to extract or a promise of future loot.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| ES-B-01 | Loot greed | J | `{location}` | "A gold medal would approve this reluctance to leave {location}." | medal | no | ES: hypothetical medal, staying now only; no award or later result. |  |  |
| ES-B-02 | Genre tension | M | `{location}` | "At {location}, the curtain can wait for my better angle." | `{location}` | no | ES: figurative curtain, no threat. |  |  |
| ES-B-03 | Overconfidence | O | - | "An early bow would spoil my dramatic posture." | - | no | ES: no departure or outcome claimed. |  |  |
| ES-B-04 | Gear obsession | J | `{location}` | "A stubborn helmet would vote to stay at {location}." | helmet | no | ES: hypothetical helmet, no future event. |  |  |
| ES-B-05 | False optimism | M | `{location}` | "At {location}, with luck staying earns a trophy-worthy story." | trophy | no | ES: hope only, no trophy or outcome. |  |  |
| ES-B-06 | Loot greed | O | - | "The prospect of loot can make an exit feel premature." | - | no | ES: no acquisition or future discovery. |  |  |
| ES-B-07 | Bureaucracy | J | `{location}` | "The official stamp on my stay reads NOT YET at {location}." | stamp | yes | ES: figurative stamp, no future decision. |  |  |
| ES-B-08 | Genre tension | M | `{location}` | "The scenery at {location} hasn't heard my closing argument." | `{location}` | no | ES: figurative, no actual argument. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-C-01 | False optimism | J | `{location}` | "Here's hoping patience is sturdier than a folding chair at {location}." | chair | no | ES: wish and comparison, staying now. |  |  |
| ES-C-02 | Gear obsession | M | `{location}` | "At {location}, my internal compass isn't a stopwatch." | `{location}` | no | ES: figurative instruments, no deadline invented. |  |  |
| ES-C-03 | Overconfidence | O | - | "I can wait without confusing patience for armor." | - | no | ES: comparison, no equipment asserted. |  |  |
| ES-C-04 | Genre tension | J | `{location}` | "A quiet bench sounds more dignified than this pause at {location}." | bench | no | ES: hypothetical bench, no healing or permanent stay. |  |  |
| ES-C-05 | Gear obsession | M | `{location}` | "My map of {location} has room for a pause." | `{location}` | no | ES: figurative map, no route or discovery. |  |  |
| ES-C-06 | Loot greed | O | - | "A trinket is not a reason to forget the way out." | - | no | ES: comparison, no acquisition. |  |  |
| ES-C-07 | Overconfidence | J | `{location}` | "A stopwatch would be less patient about remaining at {location}." | stopwatch | no | ES: hypothetical stopwatch, no future decision. |  |  |
| ES-C-08 | Bureaucracy | M | `{location}` | "At {location}, the stay gets a provisional stamp, not a lifetime permit." | stamp, permit | yes | ES: figurative paperwork; no permanent stay. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-G-01 | Genre tension | J | `{location}` | "A curtain would get impatient waiting for an exit at {location}." | curtain | no | ES: hypothetical curtain, no actual deadline change. |  |  |
| ES-G-02 | False optimism | M | `{location}` | "At {location}, with luck this pause is as useful as a spare fuse." | fuse | no | ES: hope only; no possession or future outcome. |  |  |
| ES-G-03 | Genre tension | O | - | "The imaginary curtain can wait; this act isn't over yet." | curtain | no | ES: staying now, no further result guaranteed. |  |  |
| ES-G-04 | Bureaucracy | J | `{location}` | "A pause permit would need a very small stamp at {location}." | permit, stamp | yes | ES: hypothetical permit, no duration promise. |  |  |
| ES-G-05 | Overconfidence | M | `{location}` | "At {location}, staying sounds more elegant than a clockwork goose." | - | no | ES: comparison, no actual device or new event. |  |  |
| ES-G-06 | Gear obsession | O | - | "Even an exit algorithm needs a pause button." | - | no | ES: metaphor, no mechanism. |  |  |
| ES-G-07 | Loot greed | J | `{location}` | "The possibility of spare components argues for {location}." | `{location}` | no | ES: no component or award. |  |  |
| ES-G-08 | Overconfidence | M | `{location}` | "At {location}, my theory reserves the right to reconsider." | `{location}` | no | ES: no later decision guaranteed. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-H-01 | False optimism | J | `{location}` | "With luck this pause will be as rewarding as a marble at {location}." | marble | no | ES: hope only, no item or later discovery. |  |  |
| ES-H-02 | Genre tension | M | `{location}` | "At {location}, my appetite has reserved a shelf for prospects." | `{location}` | no | ES: metaphor, no inventory. |  |  |
| ES-H-03 | Loot greed | O | - | "The possibility of a bottle cap is a powerful argument." | - | no | ES: no object or award claimed. |  |  |
| ES-H-04 | Gear obsession | J | `{location}` | "A hypothetical zipper would applaud this pause at {location}." | zipper | no | ES: no promised duration or packing action. |  |  |
| ES-H-05 | Overconfidence | M | `{location}` | "At {location}, even a treasure chest would admire this patience." | chest | no | ES: hypothetical chest, no item or permanent decision. |  |  |
| ES-H-06 | Bureaucracy | O | - | "The staying permit has a space for further reconsideration." | permit | yes | ES: imagined permit, no permanent stay. |  |  |
| ES-H-07 | Genre tension | J | `{location}` | "My acquisitive urge has unfinished business with {location}." | `{location}` | no | ES: no actual encounter or ownership. |  |  |
| ES-H-08 | Gear obsession | M | `{location}` | "At {location}, my storage instincts oppose the exit." | `{location}` | no | ES: no actual contents. |  |  |

## SI — Search or encounter, recovered

**Required:** SearchCompleted or EncounterResolved, location, and committed
shared awards demonstrating at least one recipient received an item. `{item}`
is eligible only when the exact item and recipient appear in that committed
awards text; never assume the recipient is the speaker. Every J/M candidate
requires that exact `{item}`; O candidates use no item slot and assert no owner.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SI-B-01 | Loot greed | J | `{item}` | "That object has the nerve to demand a drumroll: {item}." | `{item}` | no | SI: exact awarded item and recipient; no ownership asserted. |  |  |
| SI-B-02 | Bureaucracy | M | `{item}` | "I nominate {item} for the committee's loudest-object award." | `{item}` | yes | SI: imagined nomination; no recipient or real award. |  |  |
| SI-B-03 | Genre tension | O | - | "A recovered prize gets an entrance worthy of a champion." | - | no | SI: award exists; no owner or value inferred. |  |  |
| SI-B-04 | Genre tension | J | `{item}` | "Give the victory fanfare to the object itself: {item}." | `{item}` | no | SI: exact award; figurative fanfare, no fight asserted. |  |  |
| SI-B-05 | Loot greed | M | `{item}` | "I respect {item} for arriving with trophy-level ambition." | `{item}` | no | SI: exact award; opinion, not ownership. |  |  |
| SI-B-06 | Overconfidence | O | - | "Even the loot gets to look tough after a find." | - | no | SI: awarded object exists; no combat claimed. |  |  |
| SI-B-07 | Gear obsession | J | `{item}` | "A display stand would be proud of {item}." | `{item}` | no | SI: exact award; no item claimed by speaker. |  |  |
| SI-B-08 | Loot greed | M | `{item}` | "I'd nominate {item} for the title of loudest treasure." | `{item}` | no | SI: exact award; figurative title, no transfer. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SI-C-01 | Loot greed | J | `{item}` | "A find worth a quiet nod: {item}." | `{item}` | no | SI: exact award; no recipient inferred. |  |  |
| SI-C-02 | False optimism | M | `{item}` | "With luck {item} brings less trouble than a parade." | `{item}` | no | SI: hope only; no personal ownership or promised outcome. |  |  |
| SI-C-03 | Loot greed | O | - | "Even a recovered treasure is no substitute for an exit." | treasure | no | SI: award exists; no owner or successful extraction. |  |  |
| SI-C-04 | Overconfidence | J | `{item}` | "A measured cheer seems appropriate for {item}." | `{item}` | no | SI: exact award; no extraction result. |  |  |
| SI-C-05 | Loot greed | M | `{item}` | "I'll give {item} a nod, not a parade." | `{item}` | no | SI: exact award; no further find. |  |  |
| SI-C-06 | Gear obsession | O | - | "An awarded find doesn't need a victory dance." | - | no | SI: no travel or item usefulness guaranteed. |  |  |
| SI-C-07 | Loot greed | J | `{item}` | "A modest display case would do justice to {item}." | `{item}`, case | no | SI: exact award; no recipient inferred. |  |  |
| SI-C-08 | Overconfidence | M | `{item}` | "My enthusiasm for {item} has learned to use its indoor voice." | `{item}` | no | SI: exact award; no building inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-G-01 | Loot greed | J | `{item}` | "A velvet pedestal would flatter the edges of {item}." | `{item}`, pedestal | no | SI: exact award; no disassembly or ownership. |  |  |
| SI-G-02 | Bureaucracy | M | `{item}` | "The committee would need three forms to classify {item} as interesting." | `{item}`, forms | yes | SI: imagined committee; no ownership or actual paperwork. |  |  |
| SI-G-03 | Genre tension | O | - | "A recovered object is engineering's favorite cliffhanger." | - | no | SI: award exists; no further experiment. |  |  |
| SI-G-04 | Overconfidence | J | `{item}` | "I already have three incompatible theories about {item}." | `{item}` | no | SI: exact award; theories only. |  |  |
| SI-G-05 | False optimism | M | `{item}` | "With luck {item} has a fascinating secret, even if it's only the shape." | `{item}` | no | SI: hopeful opinion, no additional discovery. |  |  |
| SI-G-06 | Loot greed | O | - | "Every recovered object deserves a technical biography." | - | no | SI: award exists; no recipient or valuation. |  |  |
| SI-G-07 | Genre tension | J | `{item}` | "A curtain call would be excessive, but tempting, for {item}." | `{item}`, curtain | no | SI: exact award; no use or transfer. |  |  |
| SI-G-08 | Loot greed | M | `{item}` | "Even {item} deserves a velvet-lined parts cabinet." | `{item}`, cabinet | no | SI: imagined display; no ownership or transfer. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-H-01 | Loot greed | J | `{item}` | "A velvet pedestal is almost good enough for {item}." | `{item}` | no | SI: exact award; no ownership or pedestal. |  |  |
| SI-H-02 | False optimism | M | `{item}` | "With luck {item} will prove as grand as it looks." | `{item}` | no | SI: wish only; no ownership or future discovery. |  |  |
| SI-H-03 | Loot greed | O | - | "A recovered object is my favorite argument against moderation." | - | no | SI: no item identity or owner inferred. |  |  |
| SI-H-04 | Gear obsession | J | `{item}` | "The finest display case would be improved by {item}." | `{item}` | no | SI: exact award; no item moved or owned. |  |  |
| SI-H-05 | Overconfidence | M | `{item}` | "I can make {item} sound like a national treasure." | `{item}` | no | SI: exact award; no actual appraisal. |  |  |
| SI-H-06 | Loot greed | O | - | "A find like that makes restraint look terribly underdressed." | - | no | SI: no transfer or recipient inferred. |  |  |
| SI-H-07 | Genre tension | J | `{item}` | "A choir of angels ought to announce {item}." | `{item}` | no | SI: exact award; figurative choir. |  |  |
| SI-H-08 | Overconfidence | M | `{item}` | "Even {item} cannot compete with my talent for admiration." | `{item}` | no | SI: exact award; opinion, no ownership. |  |  |

## SN — Search or encounter, nothing recovered

**Required:** SearchCompleted or EncounterResolved, location, and committed
evidence that no item was recovered. Do not invent a hidden find, recipient,
subsequent search, or item loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-B-01 | Overconfidence | J | `{location}` | "Even a trophy made of air would look intimidated at {location}." | trophy | no | SN: hypothetical trophy; no find or fight. |  |  |
| SN-B-02 | Genre tension | M | `{location}` | "At {location}, an empty search has the nerve to look proud." | `{location}` | no | SN: no equipment or find. |  |  |
| SN-B-03 | Loot greed | O | - | "An imaginary trophy shelf would protest that fruitless search." | shelf | no | SN: hypothetical shelf, no award. |  |  |
| SN-B-04 | Bureaucracy | J | `{location}` | "A form should let me dispute that empty result at {location}." | form | yes | SN: hypothetical complaint, no recipient or later find. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-C-01 | Bureaucracy | J | `{location}` | "The empty-search form needs only one word at {location}: nothing." | form | yes | SN: hypothetical form, no item or subsequent action. |  |  |
| SN-C-02 | Gear obsession | M | `{location}` | "The search at {location} offers no reason to rearrange a pouch." | `{location}` | no | SN: no pouch or inventory claim. |  |  |
| SN-C-03 | Loot greed | O | - | "Not even a bottle cap. My imaginary display shelf has standards." | bottle cap, shelf | no | SN: no recovered item; hypothetical shelf, no inventory claim. |  |  |
| SN-C-04 | Overconfidence | J | `{location}` | "A dry biscuit would have improved that empty search at {location}." | biscuit | no | SN: hypothetical biscuit, no survival guarantee. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-G-01 | Genre tension | J | `{location}` | "A curtain would be a grand setting for this empty result at {location}." | curtain | no | SN: hypothetical curtain, no hidden item. |  |  |
| SN-G-02 | Gear obsession | M | `{location}` | "At {location}, the parts tray of my ambition comes up empty." | `{location}` | no | SN: figurative tray, no inventory assertion. |  |  |
| SN-G-03 | Bureaucracy | O | - | "The empty-search report would contain a very lonely zero." | report | yes | SN: imagined report, no discovery. |  |  |
| SN-G-04 | Loot greed | J | `{location}` | "A spare cog would have made this result prettier at {location}." | cog | no | SN: hypothetical cog, no award. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-H-01 | Overconfidence | J | `{location}` | "An empty search can't outlast my indignation at {location}." | - | no | SN: attitude only, no future search or item. |  |  |
| SN-H-02 | Genre tension | M | `{location}` | "At {location}, the absence deserves a very small velvet cushion." | `{location}` | no | SN: comparison, no acquisition. |  |  |
| SN-H-03 | Gear obsession | O | - | "An empty search gives a pouch nothing to celebrate." | pouch | no | SN: hypothetical pouch, no inventory claim. |  |  |
| SN-H-04 | Loot greed | J | `{location}` | "Even a bent bottle cap would beat nothing at {location}." | bottle cap | no | SN: hypothetical object, no recovered item. |  |  |

## SX — OpportunityExhausted

**Required:** kind and location. The opportunity is exhausted; no finding,
award, owner, or subsequent opportunity follows from this event.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-B-01 | Overconfidence | J | `{location}` | "A spent opportunity is a punching bag that won't swing back at {location}." | punching bag | no | SX: comparison only, no discovery or fight. |  |  |
| SX-B-02 | Bureaucracy | M | `{location}` | "At {location}, that spent opportunity gets a very angry stamp." | stamp | yes | SX: hypothetical stamp; no further result. |  |  |
| SX-B-03 | Loot greed | O | - | "An imaginary trophy shelf objects to that opportunity running out." | shelf | no | SX: comparison, no award. |  |  |
| SX-B-04 | Overconfidence | J | `{location}` | "I refuse to be impressed by a dead end at {location}." | `{location}` | no | SX: metaphor, no blocked route. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-C-01 | Loot greed | J | `{location}` | "Not even an imaginary coin can extend that spent chance at {location}." | coin | no | SX: hypothetical coin, no item outcome asserted. |  |  |
| SX-C-02 | Gear obsession | M | `{location}` | "At {location}, even a compass can't extend that opportunity." | `{location}` | no | SX: comparison, no route. |  |  |
| SX-C-03 | Genre tension | O | - | "An exhausted chance is a finished page, not another chapter." | - | yes | SX: literary comparison, conservatively office-keyword flagged. |  |  |
| SX-C-04 | Loot greed | J | `{location}` | "No appetite can extend that spent opportunity at {location}." | `{location}` | no | SX: no item or future search. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-G-01 | Bureaucracy | J | `{location}` | "That spent opportunity deserves a CLOSED stamp at {location}." | stamp | yes | SX: hypothetical stamp; no discovery. |  |  |
| SX-G-02 | Gear obsession | M | `{location}` | "At {location}, this experiment has run out of buttons." | `{location}` | no | SX: metaphor, no actual device or experiment. |  |  |
| SX-G-03 | Overconfidence | O | - | "Even an elegant equation has a last term." | - | no | SX: no new event or object. |  |  |
| SX-G-04 | Loot greed | J | `{location}` | "No amount of wishful solder extends that chance at {location}." | `{location}` | no | SX: no item created or recovered. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-H-01 | Loot greed | J | `{location}` | "The word exhausted has an ugly sound at {location}." | `{location}` | no | SX: no actual sound or item. |  |  |
| SX-H-02 | Genre tension | M | `{location}` | "At {location}, my hopes have met a very firm lid." | `{location}` | no | SX: figurative lid, no container asserted. |  |  |
| SX-H-03 | Gear obsession | O | - | "No display case can preserve an exhausted possibility." | - | no | SX: comparison, no possession. |  |  |
| SX-H-04 | Overconfidence | J | `{location}` | "A jeweled lid would not improve that ending at {location}." | lid | no | SX: hypothetical lid, no further opportunity. |  |  |

## AR — Arrived

**Required:** Arrived and committed location. Arrival is not evidence of
subsequent travel, a search, a find, or an encounter.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-B-01 | False optimism | J | `{location}` | "With luck this entrance deserves a cape and fanfare at {location}." | cape | no | AR: hope only, no encounter or costume. |  |  |
| AR-B-02 | Loot greed | M | `{location}` | "At {location}, a trophy would approve of this entrance." | trophy | no | AR: hypothetical trophy, no award. |  |  |
| AR-B-03 | Gear obsession | O | - | "My boots consider every arrival a personal triumph." | - | no | AR: no new gear or ground condition. |  |  |
| AR-B-04 | Gear obsession | J | `{location}` | "A boxing glove would make a quieter introduction to {location}." | boxing glove | no | AR: comparison, no opponent. |  |  |
| AR-B-05 | Genre tension | M | `{location}` | "At {location}, an imaginary drum deserves a solo before my first word." | drum | no | AR: no other actor or actual music. |  |  |
| AR-B-06 | Loot greed | O | - | "Arrival is a fine opening act for my appetite." | - | no | AR: no item or award. |  |  |
| AR-B-07 | Bureaucracy | J | `{location}` | "My entrance deserves a permit stamped MENACING at {location}." | permit | yes | AR: hypothetical permit, no threat or action inferred. |  |  |
| AR-B-08 | Genre tension | M | `{location}` | "At {location}, my posture insists on top billing." | `{location}` | yes | AR: figurative billing, no performance. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-C-01 | False optimism | J | `{location}` | "Here's hoping this visit feels as reassuring as a handrail at {location}." | handrail | no | AR: hope only, no claim of actual handrail. |  |  |
| AR-C-02 | Loot greed | M | `{location}` | "At {location}, a hypothetical coin can wait until I've got my bearings." | coin | no | AR: no possession, discovery, or further movement. |  |  |
| AR-C-03 | Overconfidence | O | - | "Arrival needn't be louder than a dropped spoon." | spoon | no | AR: comparison, no dropped object or further movement. |  |  |
| AR-C-04 | Loot greed | J | `{location}` | "Even an imaginary bottle cap can wait after arrival at {location}." | bottle cap | no | AR: no acquisition or continued travel. |  |  |
| AR-C-05 | Gear obsession | M | `{location}` | "My internal compass gives {location} a respectful glance." | `{location}` | no | AR: figurative instrument, no route. |  |  |
| AR-C-06 | Loot greed | O | - | "First the bearings. Then the appetite gets a word." | - | no | AR: arrival reaction only, no subsequent action, item or discovery. |  |  |
| AR-C-07 | Overconfidence | J | `{location}` | "A hypothetical parade float would make a fussier entrance at {location}." | float | no | AR: no continued movement or search. |  |  |
| AR-C-08 | Bureaucracy | M | `{location}` | "At {location}, my arrival gets a discreet stamp rather than a parade." | stamp | yes | AR: hypothetical stamp; no other actor or event. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-G-01 | Genre tension | J | `{location}` | "A hypothetical fanfare would introduce my arrival at {location}." | fanfare | no | AR: no audience or actual music. |  |  |
| AR-G-02 | Gear obsession | M | `{location}` | "At {location}, my curiosity unfolds its measuring tape." | `{location}` | no | AR: figurative tool, no inspection. |  |  |
| AR-G-03 | Overconfidence | O | - | "Arrival calibrates my enthusiasm without a screwdriver." | - | no | AR: metaphor, no tool or adjustment. |  |  |
| AR-G-04 | False optimism | J | `{location}` | "With luck this visit runs smoother than a well-oiled hinge at {location}." | hinge | no | AR: hope only, no survey or possession. |  |  |
| AR-G-05 | Bureaucracy | M | `{location}` | "At {location}, my imaginary arrival form requests a precise flourish." | form | yes | AR: hypothetical form; no actual coordinates or search. |  |  |
| AR-G-06 | Loot greed | O | - | "A hypothetical spare cog would make this entrance even nicer." | cog | no | AR: hypothetical object, no acquisition. |  |  |
| AR-G-07 | Loot greed | J | `{location}` | "A souvenir bolt would make a grand welcome to {location}." | bolt | no | AR: hypothetical bolt, no acquisition. |  |  |
| AR-G-08 | Overconfidence | M | `{location}` | "At {location}, I am ready to overthink the geometry." | `{location}` | no | AR: attitude only, no search. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-H-01 | False optimism | J | `{location}` | "Here's hoping this visit sparkles like a jewel at {location}." | jewel | no | AR: wish only, no purchase or item. |  |  |
| AR-H-02 | Genre tension | M | `{location}` | "At {location}, anticipation puts on a velvet cape." | `{location}` | no | AR: figurative cape, no equipment. |  |  |
| AR-H-03 | Loot greed | O | - | "Arriving gives my souvenir instincts a lavish reception." | - | no | AR: no item or acquisition. |  |  |
| AR-H-04 | Gear obsession | J | `{location}` | "An imaginary pouch would applaud the arrival at {location}." | pouch | no | AR: no luggage or contents asserted. |  |  |
| AR-H-05 | Overconfidence | M | `{location}` | "At {location}, a hypothetical crown would look less pleased than I do." | crown | no | AR: comparison, no discovery. |  |  |
| AR-H-06 | Loot greed | O | - | "A fresh arrival makes my hunger unwrap the prospects." | - | no | AR: no item or award. |  |  |
| AR-H-07 | Genre tension | J | `{location}` | "A velvet curtain would suit this entrance at {location}." | curtain | no | AR: hypothetical curtain, no item or equipment. |  |  |
| AR-H-08 | Bureaucracy | M | `{location}` | "At {location}, my imaginary arrival form requests a gold seal." | form, seal | yes | AR: hypothetical paperwork, no ownership or search. |  |  |

## AD — DutyAdmission, Departure or Deployment

**Required:** one of the three exact kinds and its committed location. These
lines fit each kind: no line promises boarding, an arrived Raider, or extraction.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AD-B-01 | Overconfidence | J | `{location}` | "A welcome mat would flinch at this raid-opening glare around {location}." | mat | no | AD: hypothetical mat; no travel completion or opponent. |  |  |
| AD-B-02 | Gear obsession | M | `{location}` | "At {location}, a helmet would need room for my entrance." | helmet | no | AD: hypothetical helmet; either start event. |  |  |
| AD-B-03 | Gear obsession | O | - | "For a raid opening this important, a helmet ought to have a spotlight bracket." | helmet | no | AD: hypothetical equipment design, no ownership or transport claim. |  |  |
| AD-B-04 | Overconfidence | J | `{location}` | "I'll bring the raid-opening attitude, starting around {location}." | `{location}` | no | AD: start of raid, no arrival elsewhere or encounter. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-C-01 | Bureaucracy | J | `{location}` | "A quiet start deserves a very small approval stamp at {location}." | stamp | yes | AD: hypothetical stamp; no later outcome. |  |  |
| AD-C-02 | Overconfidence | M | `{location}` | "At {location}, a quiet entrance is well within my abilities." | - | no | AD: self-assessment, no later result. |  |  |
| AD-C-03 | Overconfidence | O | - | "I can begin without treating my shoelaces as emergency equipment." | - | no | AD: no condition or equipment action. |  |  |
| AD-C-04 | Genre tension | J | `{location}` | "A raid opening needs fewer trumpets and more shoelaces around {location}." | trumpets, shoelaces | no | AD: comparison, no carried gear, sound or boarding. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-G-01 | Genre tension | J | `{location}` | "A toy rocket would be an appropriately dramatic introduction to {location}." | rocket | no | AD: hypothetical rocket, no actual launch. |  |  |
| AD-G-02 | Bureaucracy | M | `{location}` | "At {location}, my enthusiasm files a request for unnecessary complexity." | request | yes | AD: hypothetical filing, no experiment. |  |  |
| AD-G-03 | Overconfidence | O | - | "Starting a raid is an engineering problem I can overcomplicate without tools." | - | no | AD: no equipment or later result. |  |  |
| AD-G-04 | Loot greed | J | `{location}` | "A souvenir cog would be an optimistic mascot for the raid at {location}." | cog | no | AD: hypothetical cog, no award. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-H-01 | Loot greed | J | `{location}` | "My ambition has packed a ceremonial suitcase for {location}." | `{location}` | no | AD: figurative suitcase, no inventory. |  |  |
| AD-H-02 | Genre tension | M | `{location}` | "At {location}, a raid's opening possibilities get the velvet-rope treatment." | `{location}` | no | AD: raid opening, figurative fixture only. |  |  |
| AD-H-03 | Loot greed | O | - | "Every raid begins with my pockets negotiating for room." | - | no | AD: figurative appetite, no capacity or acquisition. |  |  |
| AD-H-04 | Overconfidence | J | `{location}` | "A jeweled crown would suit the ambition heading into {location}." | crown | no | AD: hypothetical crown, no transaction. |  |  |

## PS — PackingStarted

**Required:** PackingStarted and location. The allocated cargo is being packed;
no new award, completed packing, departure, or secured extraction is implied.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-B-01 | False optimism | J | `{location}` | "With luck this cargo will fit without a wrestling match at {location}." | cargo | no | PS: wish only; packing begun, not finished. |  |  |
| PS-B-02 | Loot greed | M | `{location}` | "At {location}, this cargo deserves a velvet cushion." | cargo, cushion | no | PS: hypothetical cushion, no new award. |  |  |
| PS-B-03 | Gear obsession | O | - | "My backpack is about to meet my most intimidating technique." | backpack | no | PS: started, no completed packing. |  |  |
| PS-B-04 | Loot greed | J | `{location}` | "This haul deserves a fighting chance against gravity at {location}." | `{location}` | no | PS: existing cargo, no extraction claim. |  |  |
| PS-B-05 | Overconfidence | M | `{location}` | "My method at {location} has all the grace of a bear hug." | `{location}` | no | PS: comparison, no bear or completed work. |  |  |
| PS-B-06 | Genre tension | O | - | "Packing earns its own dramatic montage, whether it likes it or not." | - | no | PS: no act completed. |  |  |
| PS-B-07 | Gear obsession | J | `{location}` | "The backpack meets my strongest stowing opinions at {location}." | backpack, `{location}` | no | PS: no strap state. |  |  |
| PS-B-08 | Overconfidence | M | `{location}` | "At {location}, cargo gets the same respect as an opponent." | `{location}` | no | PS: comparison, no opponent or contest. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-C-01 | Gear obsession | J | `{location}` | "A sensible arrangement beats a heroic tangle at {location}." | `{location}` | no | PS: no completion or item added. |  |  |
| PS-C-02 | False optimism | M | `{location}` | "At {location}, perhaps this bundle will fit without a struggle." | bundle | no | PS: hope only; no completion or item added. |  |  |
| PS-C-03 | Gear obsession | O | - | "Packing rewards patience more reliably than a heroic speech." | - | no | PS: no outcome or condition asserted. |  |  |
| PS-C-04 | Overconfidence | J | `{location}` | "I can handle a bundle without challenging physics at {location}." | `{location}` | no | PS: existing cargo; no completion. |  |  |
| PS-C-05 | Loot greed | M | `{location}` | "At {location}, even a modest bundle deserves a bit of tenderness." | bundle | no | PS: existing cargo; no completion or extra item. |  |  |
| PS-C-06 | Loot greed | O | - | "Packing is easier when enthusiasm doesn't sit on the cargo." | - | no | PS: no travel or damage asserted. |  |  |
| PS-C-07 | Loot greed | J | `{location}` | "A velvet lining would flatter this cargo at {location}." | lining | no | PS: hypothetical lining; no completion. |  |  |
| PS-C-08 | Genre tension | M | `{location}` | "At {location}, restraint does the heavy lifting in my head." | `{location}` | no | PS: metaphor, no completion. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-G-01 | Genre tension | J | `{location}` | "A parade float would make a grand stage for these parcels at {location}." | float | no | PS: hypothetical float, allocated cargo only; no completion. |  |  |
| PS-G-02 | Genre tension | M | `{location}` | "At {location}, packing looks like a puzzle trying to escape its own box." | box | no | PS: comparison; no completion or extra item. |  |  |
| PS-G-03 | Gear obsession | O | - | "Packing is a negotiation between angles and impatience." | - | no | PS: metaphor, no completion. |  |  |
| PS-G-04 | Overconfidence | J | `{location}` | "I can give spatial reasoning a heroic sound at {location}." | `{location}` | no | PS: no packing completion. |  |  |
| PS-G-05 | False optimism | M | `{location}` | "At {location}, with luck this load fits as neatly as a toy cube." | cube | no | PS: hope only, no packing completion or added item. |  |  |
| PS-G-06 | Loot greed | O | - | "Stowing cargo makes a spare compartment sound like engineering genius." | - | no | PS: no capacity change. |  |  |
| PS-G-07 | Loot greed | J | `{location}` | "Even an imaginary jewel deserves careful packing at {location}." | jewel | no | PS: no added item or completion. |  |  |
| PS-G-08 | Overconfidence | M | `{location}` | "At {location}, I trust triangles more than optimism." | `{location}` | no | PS: geometric opinion, no success guarantee. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-H-01 | Loot greed | J | `{location}` | "Everything in this cargo merits a satin cushion at {location}." | `{location}` | no | PS: figurative cushion; no item added. |  |  |
| PS-H-02 | Genre tension | M | `{location}` | "At {location}, my affection for objects has become geometric." | `{location}` | no | PS: no new award or completion. |  |  |
| PS-H-03 | Gear obsession | O | - | "Stowing proves the perfect pouch needs a pocket for every pocket." | - | no | PS: no capacity change. |  |  |
| PS-H-04 | False optimism | J | `{location}` | "With luck this bundle will fit like royalty at {location}." | bundle | no | PS: wish only, no completion or secured exit. |  |  |
| PS-H-05 | Overconfidence | M | `{location}` | "My stowing at {location} is a monument to attachment." | `{location}` | no | PS: metaphor; no completion. |  |  |
| PS-H-06 | Genre tension | O | - | "The cargo deserves a packing ceremony with full orchestration." | - | no | PS: figurative, no new object or completion. |  |  |
| PS-H-07 | Overconfidence | J | `{location}` | "Packing is no match for my devotion to order at {location}." | - | no | PS: boast, no completion or ownership invented. |  |  |
| PS-H-08 | Gear obsession | M | `{location}` | "At {location}, packing deserves a divider made of pure devotion." | `{location}` | no | PS: metaphor, no equipment claim. |  |  |

## PF — PackingCompleted

**Required:** PackingCompleted and location. Packing of previously allocated
cargo is complete; it does not secure extraction, award new items, or imply sale.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-B-01 | False optimism | J | `{location}` | "Surely that bundle will survive anything after {location}." | bundle | no | PF: hope only, no secured extraction. |  |  |
| PF-B-02 | Gear obsession | M | `{location}` | "At {location}, my cargo has earned a respectful glare." | `{location}` | no | PF: previously allocated cargo only. |  |  |
| PF-B-03 | Gear obsession | O | - | "Even an imaginary buckle would salute that packing job." | buckle | no | PF: comparison, no extra action. |  |  |
| PF-B-04 | Loot greed | J | `{location}` | "A haul looks better with attitude at {location}." | `{location}` | no | PF: no new acquisition. |  |  |
| PF-B-05 | Overconfidence | M | `{location}` | "The bundle at {location} has survived my organizational genius." | `{location}` | no | PF: figurative survival, no extraction success. |  |  |
| PF-B-06 | Gear obsession | O | - | "Finished packing deserves a salute from every buckle." | - | no | PF: figurative buckle, no gear state. |  |  |
| PF-B-07 | Genre tension | J | `{location}` | "The final flourish belongs entirely to me at {location}." | `{location}` | no | PF: no departure or contest inferred. |  |  |
| PF-B-08 | Overconfidence | M | `{location}` | "At {location}, my handiwork gets the fiercest nod available." | `{location}` | no | PF: expression only, no secured extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-C-01 | Gear obsession | J | `{location}` | "Neatness earns a cautious nod at {location}." | `{location}` | no | PF: no item condition or successful exit. |  |  |
| PF-C-02 | False optimism | M | `{location}` | "At {location}, perhaps the packed bundle will stay this tidy." | bundle | no | PF: hope only; no departure or extraction. |  |  |
| PF-C-03 | Overconfidence | O | - | "Packed cargo is not a magic carpet home." | - | no | PF: comparison; no travel or extraction. |  |  |
| PF-C-04 | Gear obsession | J | `{location}` | "I appreciate the orderly load at {location}." | `{location}` | no | PF: no item or capacity change. |  |  |
| PF-C-05 | Loot greed | M | `{location}` | "At {location}, the packed bundle looks worth a respectful nod." | bundle | no | PF: no new award or extraction. |  |  |
| PF-C-06 | Loot greed | O | - | "This bundle earns more respect than a handful of wishes." | bundle | no | PF: comparison, previously allocated cargo; no new item. |  |  |
| PF-C-07 | Overconfidence | J | `{location}` | "I can celebrate finished stowing without declaring victory over {location}." | `{location}` | no | PF: packing complete, not safe extraction. |  |  |
| PF-C-08 | Genre tension | M | `{location}` | "At {location}, my attention has one fewer loose end." | `{location}` | no | PF: no departure or extraction implied. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-G-01 | Genre tension | J | `{location}` | "The packed bundle deserves a curtain call at {location}." | bundle, curtain | no | PF: figurative performance; no added item. |  |  |
| PF-G-02 | Gear obsession | M | `{location}` | "At {location}, my inner slide rule gives a satisfied twitch." | `{location}` | no | PF: figurative instrument, no measurement. |  |  |
| PF-G-03 | Overconfidence | O | - | "My completed cargo layout merits a commemorative cog." | - | no | PF: figurative token, no award. |  |  |
| PF-G-04 | Loot greed | J | `{location}` | "A jeweled cabinet would admire that completed bundle at {location}." | cabinet, bundle | no | PF: hypothetical cabinet, no capacity or ownership change. |  |  |
| PF-G-05 | False optimism | M | `{location}` | "At {location}, with luck that bundle stays as tidy as a toy cube." | bundle, cube | no | PF: hope about arrangement, not extraction. |  |  |
| PF-G-06 | Loot greed | O | - | "Packed cargo is geometry with excellent priorities." | - | no | PF: no contents or pocket claim. |  |  |
| PF-G-07 | Loot greed | J | `{location}` | "A velvet pillow would flatter the arranged cargo at {location}." | pillow | no | PF: hypothetical pillow, no award. |  |  |
| PF-G-08 | Overconfidence | M | `{location}` | "At {location}, my packing design is quietly smug." | `{location}` | no | PF: metaphor, no measurement. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-H-01 | Loot greed | J | `{location}` | "The cargo has achieved an extremely flattering arrangement at {location}." | `{location}` | no | PF: no item addition or secured extraction. |  |  |
| PF-H-02 | Genre tension | M | `{location}` | "At {location}, my affection for possessions looks positively architectural." | `{location}` | no | PF: no change in ownership or building. |  |  |
| PF-H-03 | Gear obsession | O | - | "Finished packing deserves a tiny curtain call." | - | no | PF: no gear condition. |  |  |
| PF-H-04 | Overconfidence | J | `{location}` | "I call this arrangement an act of devotion at {location}." | `{location}` | no | PF: opinion; no additional award. |  |  |
| PF-H-05 | Bureaucracy | M | `{location}` | "At {location}, that packed bundle deserves a certificate of devotion." | bundle, certificate | yes | PF: imagined certificate; no safety guarantee. |  |  |
| PF-H-06 | Genre tension | O | - | "That completed bundle looks ready for a coronation." | - | no | PF: comparison, no acquisition. |  |  |
| PF-H-07 | Gear obsession | J | `{location}` | "A display cabinet would envy the care taken at {location}." | `{location}` | no | PF: comparison, no actual item. |  |  |
| PF-H-08 | Overconfidence | M | `{location}` | "At {location}, that bundle has met its most devoted packer." | bundle | no | PF: boast, no change in ownership. |  |  |

## HW — HazardWarning

**Required:** HazardWarning, committed condition and location. A warning is a
forecast, not evidence that the hazard has already struck or will cause HP loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| HW-B-01 | Overconfidence | J | `{condition}` | "That warning merits my fiercest glare: {condition}." | `{condition}` | no | HW: warning only; no impact. |  |  |
| HW-B-02 | Genre tension | M | `{condition}` | "The forecast for {condition} sounds like a curtain call delivered with clenched teeth." | `{condition}`, curtain | no | HW: Overtime Red only; forecast, no impact or closing event asserted. |  |  |
| HW-B-03 | Gear obsession | O | - | "My stubbornness wears a helmet for the forecast." | - | no | HW: figurative gear; no impact. |  |  |
| HW-B-04 | Overconfidence | J | `{location}` | "Warnings are welcome to challenge my eyebrows at {location}." | `{location}` | no | HW: no hazard happened. |  |  |
| HW-B-05 | Gear obsession | M | `{condition}` | "For {condition}, a helmet would at least look suitably aggressive." | `{condition}`, helmet | no | HW: hypothetical helmet, forecast only. |  |  |
| HW-B-06 | Loot greed | O | - | "A gloomy prediction won't intimidate my appetite for trophies." | - | no | HW: no trophy, harm or loss. |  |  |
| HW-B-07 | False optimism | J | `{location}` | "With luck that forecast is all bark and no bite at {location}." | - | no | HW: hope only; no impact or outcome asserted. |  |  |
| HW-B-08 | Genre tension | M | `{condition}` | "If {condition} wants an entrance, it can wait its turn." | `{condition}` | no | HW: no occurrence asserted. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-C-01 | False optimism | J | `{condition}` | "With luck the forecast sounds worse than it is: {condition}." | `{condition}` | no | HW: hope only, no impact or safety guaranteed. |  |  |
| HW-C-02 | Gear obsession | M | `{condition}` | "For {condition}, an umbrella sounds more useful than accepting the apology." | `{condition}`, umbrella | no | HW: Apology Weather only; hypothetical umbrella, no impact or protection. |  |  |
| HW-C-03 | Overconfidence | O | - | "I can take a warning seriously without applauding it." | - | no | HW: no damage or occurrence. |  |  |
| HW-C-04 | Loot greed | J | `{location}` | "A bottle cap would be a more welcome forecast at {location}." | bottle cap | no | HW: hypothetical object, no impact or HP loss. |  |  |
| HW-C-05 | Gear obsession | M | `{condition}` | "For {condition}, a sensible coat would be more persuasive than optimism." | `{condition}` | no | HW: comparison, no protection claimed. |  |  |
| HW-C-06 | Loot greed | O | - | "A warning makes even loot sound less persuasive." | - | no | HW: no item or safe outcome. |  |  |
| HW-C-07 | Overconfidence | J | `{location}` | "I can respect a prediction without panicking at {location}." | `{location}` | no | HW: no claimed hazard outcome. |  |  |
| HW-C-08 | Genre tension | M | `{condition}` | "The mention of {condition} has earned my quieter voice." | `{condition}` | no | HW: forecast only. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-G-01 | Genre tension | J | `{condition}` | "A nervous barometer would find good company in the forecast: {condition}." | `{condition}`, barometer | no | HW: comparison, no carried instrument or impact. |  |  |
| HW-G-02 | False optimism | M | `{condition}` | "With luck {condition} will apologize to the imaginary circuit board before getting it wet." | `{condition}`, circuit board | no | HW: Apology Weather only; wish, hypothetical board, no impact predicted as fact. |  |  |
| HW-G-03 | Overconfidence | O | - | "A warning is a measurement with its eyebrows raised." | - | no | HW: comparison, no actual measurement. |  |  |
| HW-G-04 | Loot greed | J | `{location}` | "A gold-plated alarm bell would not improve that forecast at {location}." | bell | no | HW: hypothetical bell, no impact. |  |  |
| HW-G-05 | Genre tension | M | `{condition}` | "The forecast for {condition} would make a dismal stage entrance." | `{condition}` | no | HW: figurative performance; no impact. |  |  |
| HW-G-06 | Loot greed | O | - | "A spare dial would make this alert look almost collectible." | dial | no | HW: hypothetical comparison, no acquisition. |  |  |
| HW-G-07 | Overconfidence | J | `{location}` | "I can overengineer a response to this warning at {location}." | `{location}` | no | HW: no response completed. |  |  |
| HW-G-08 | Genre tension | M | `{condition}` | "The warning for {condition} has the charm of a damp fuse." | `{condition}`, fuse | no | HW: comparison, no actual fuse or impact. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-H-01 | False optimism | J | `{condition}` | "With luck this forecast is as harmless as a bottle cap: {condition}." | `{condition}`, bottle cap | no | HW: wish only, no item or impact claimed. |  |  |
| HW-H-02 | Genre tension | M | `{condition}` | "The forecast for {condition} has the atmosphere of a shop curtain falling on my wish list." | `{condition}`, curtain | no | HW: Overtime Red only; hypothetical shop, no actual loss or impact. |  |  |
| HW-H-03 | Gear obsession | O | - | "A prediction cannot fit inside a jewelry box." | - | no | HW: comparison, no acquisition. |  |  |
| HW-H-04 | Overconfidence | J | `{location}` | "I can treasure a prediction in theory at {location}." | `{location}` | no | HW: no impact or success asserted. |  |  |
| HW-H-05 | Overconfidence | M | `{condition}` | "The forecast of {condition} can't outstare my determined eyebrows." | `{condition}` | no | HW: boast, no item or impact. |  |  |
| HW-H-06 | Genre tension | O | - | "This alert has all the charm of an empty display case." | - | no | HW: comparison, no actual container. |  |  |
| HW-H-07 | Gear obsession | J | `{location}` | "Even brocade can't make that warning pretty at {location}." | `{location}` | no | HW: metaphor; forecast only. |  |  |
| HW-H-08 | Overconfidence | M | `{condition}` | "I can dislike {condition} before it does anything." | `{condition}` | no | HW: no impact asserted. |  |  |

## HI — ConditionImpact

**Required:** ConditionImpact, committed condition and location. This is raw
hazard impact; shielding or resilience may mean no HP damage. No line assumes an
injury, attacker, or future outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-B-01 | Overconfidence | J | `{condition}` | "That was an aggressively direct introduction: {condition}." | `{condition}` | no | HI: impact only, no HP loss. |  |  |
| HI-B-02 | Loot greed | M | `{condition}` | "I would trade {condition} for a cracked trophy in a heartbeat." | `{condition}`, trophy | no | HI: hypothetical trade; no acquisition or HP loss. |  |  |
| HI-B-03 | Gear obsession | O | - | "My pride wears armor against interruptions like that." | - | no | HI: metaphor, no injury. |  |  |
| HI-B-04 | Loot greed | J | `{location}` | "A trophy would be a friendlier greeting than that impact at {location}." | trophy | no | HI: hypothetical trophy, no HP loss or opponent. |  |  |
| HI-B-05 | Genre tension | M | `{condition}` | "I rate {condition} shamelessly rude for that entrance." | `{condition}` | no | HI: no further effects. |  |  |
| HI-B-06 | Loot greed | O | - | "My appetite objects to that interruption with both elbows." | - | no | HI: no item loss. |  |  |
| HI-B-07 | Gear obsession | J | `{location}` | "That impact deserves a helmet made of stubbornness at {location}." | `{location}` | no | HI: metaphor, no HP assertion. |  |  |
| HI-B-08 | Overconfidence | M | `{condition}` | "I refuse to give {condition} the satisfaction of a dramatic expression." | `{condition}` | no | HI: attitude only, no outcome. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-C-01 | Genre tension | J | `{condition}` | "That was rather less abstract than a forecast: {condition}." | `{condition}` | no | HI: impact, not necessarily HP loss. |  |  |
| HI-C-02 | Gear obsession | M | `{condition}` | "An umbrella facing {condition} would have my sympathy." | `{condition}` | no | HI: comparison, no protection claim. |  |  |
| HI-C-03 | Overconfidence | O | - | "A cushion would be a more polite introduction than that impact." | cushion | no | HI: comparison, no health consequence. |  |  |
| HI-C-04 | Genre tension | J | `{location}` | "The environment has delivered its point at {location}." | `{location}` | no | HI: figurative, no HP outcome. |  |  |
| HI-C-05 | Gear obsession | M | `{condition}` | "A raincoat facing {condition} would reconsider its career." | `{condition}` | no | HI: comparison, no condition type inferred. |  |  |
| HI-C-06 | Loot greed | O | - | "Not even a bauble renders an impact more pleasant." | - | no | HI: no actual item or injury. |  |  |
| HI-C-07 | Overconfidence | J | `{location}` | "A warning bell would have been more courteous at {location}." | bell | no | HI: hypothetical bell, no HP loss or recovery. |  |  |
| HI-C-08 | Bureaucracy | M | `{condition}` | "The incident form for {condition} would need very few exclamation marks." | `{condition}`, form | yes | HI: hypothetical form, no health delta inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-G-01 | Genre tension | J | `{condition}` | "A gong would make a quieter introduction than {condition}." | `{condition}`, gong | no | HI: comparison; no HP loss inferred. |  |  |
| HI-G-02 | False optimism | M | `{condition}` | "Here's hoping {condition} has finished its introduction." | `{condition}` | no | HI: hope, no future or HP outcome guaranteed. |  |  |
| HI-G-03 | Overconfidence | O | - | "An impact makes an excellent calibration point for caution." | - | no | HI: no equipment or measurement. |  |  |
| HI-G-04 | Gear obsession | J | `{location}` | "The shock absorber in my head has comments at {location}." | `{location}` | no | HI: metaphor, no mitigation claim. |  |  |
| HI-G-05 | Genre tension | M | `{condition}` | "The contact with {condition} had the subtlety of a gong." | `{condition}`, gong | no | HI: comparison, no HP wound or timing inferred. |  |  |
| HI-G-06 | Loot greed | O | - | "A spare spring would make a more welcome arrival than that impact." | spring | no | HI: hypothetical spring, no acquisition. |  |  |
| HI-G-07 | Overconfidence | J | `{location}` | "I can call that a data point at {location}." | `{location}` | no | HI: metaphor, no measurement. |  |  |
| HI-G-08 | Overconfidence | M | `{condition}` | "Even {condition} hasn't exhausted my supply of disapproving expressions." | `{condition}` | no | HI: expression only; no actual reading. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-H-01 | Bureaucracy | J | `{condition}` | "The complaint form needs a gold seal for that impact: {condition}." | `{condition}`, form | yes | HI: hypothetical form, no wound. |  |  |
| HI-H-02 | Genre tension | M | `{condition}` | "My collection of complaints welcomes {condition} reluctantly." | `{condition}` | no | HI: figurative collection, no item loss. |  |  |
| HI-H-03 | Gear obsession | O | - | "A silk cushion would be a more considerate interruption." | - | no | HI: comparison, no damage claim. |  |  |
| HI-H-04 | Overconfidence | J | `{location}` | "A jeweled cushion would have made that impact prettier at {location}." | cushion | no | HI: hypothetical cushion, no injury or outcome. |  |  |
| HI-H-05 | Loot greed | M | `{condition}` | "Another greeting from {condition} is worth less than a broken button." | `{condition}` | no | HI: comparison, no future or HP claim. |  |  |
| HI-H-06 | Genre tension | O | - | "That was not the sort of collectible interruption I wanted." | - | no | HI: no acquisition or item loss. |  |  |
| HI-H-07 | Gear obsession | J | `{location}` | "Even a silk lining wouldn't soften that disturbance at {location}." | `{location}` | no | HI: comparison, no mitigation. |  |  |
| HI-H-08 | Bureaucracy | M | `{condition}` | "My hunger can object to {condition} without filing a complaint." | `{condition}` | yes | HI: figurative complaint, no procedural act. |  |  |

## RD — RobotDefeated

**Required:** RobotDefeated, location and robot identity when `{robot}` is
used; the robot must occur in the committed text/thread. Defeat does not imply
any reward or that the Raider avoided injury.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-B-01 | Overconfidence | J | `{location}` | "That's one less mechanical opinion at {location}." | `{location}` | no | RD: defeated machine only; no reward. |  |  |
| RD-B-02 | Genre tension | M | `{robot}` | "The cleaning drama starring {robot} finally has a silent lead." | `{robot}` | no | RD: Roomba Prime only, committed defeat; no reward or extra action. |  |  |
| RD-B-03 | Gear obsession | O | - | "A chassis has finally lost an argument." | - | no | RD: metaphor; no injury or loot. |  |  |
| RD-B-04 | Overconfidence | J | `{location}` | "A disabled machine merits no rebuttal from me at {location}." | `{location}` | no | RD: defeated robot, no dialogue or reward claimed. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-C-01 | Genre tension | J | `{location}` | "A disabled machine is the least demanding company at {location}." | `{location}` | no | RD: robot defeated, no Raider injury or recovery inferred. |  |  |
| RD-C-02 | Overconfidence | M | `{robot}` | "Even {robot} has run out of worries before I have." | `{robot}` | no | RD: Anxietick only, defeated; figurative worries, no HP or pursuit claim. |  |  |
| RD-C-03 | Gear obsession | O | - | "A defeated chassis has fewer immediate ambitions." | - | no | RD: no salvage. |  |  |
| RD-C-04 | Genre tension | J | `{location}` | "That chassis has finally stopped volunteering for combat at {location}." | `{location}` | no | RD: robot defeated; no other threat ruled out. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-G-01 | Gear obsession | J | `{location}` | "The defeated chassis has discontinued its demonstration at {location}." | `{location}` | no | RD: no part removal or reward. |  |  |
| RD-G-02 | Loot greed | M | `{robot}` | "A hypothetical parts cabinet for {robot} would need a compartment for disapproval." | `{robot}`, cabinet | no | RD: Passive-Aggressor Drone only, defeated; no inspection or acquisition. |  |  |
| RD-G-03 | Overconfidence | O | - | "That machine has reached a remarkably decisive off state." | - | no | RD: no physical cause or loot. |  |  |
| RD-G-04 | Loot greed | J | `{location}` | "A silver cog would suit that machine's finale at {location}." | cog, machine | no | RD: hypothetical cog; no cargo allocated. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-H-01 | Loot greed | J | `{location}` | "My souvenir instincts admire that defeated machine at {location}." | `{location}` | no | RD: no reward. |  |  |
| RD-H-02 | Genre tension | M | `{robot}` | "A display pedestal for {robot} would turn aggressive housekeeping into sculpture." | `{robot}`, pedestal | no | RD: Roomba Prime only, defeated; hypothetical display, no ownership or drop. |  |  |
| RD-H-03 | Gear obsession | O | - | "A fallen machine makes my dismantling urges applaud." | - | no | RD: no disassembly. |  |  |
| RD-H-04 | Overconfidence | J | `{location}` | "Even a defeated chassis deserves my admiration at {location}." | `{location}` | no | RD: no item or cargo asserted. |  |  |

## RX — RobotEscaped

**Required:** RobotEscaped, location and committed identity for `{robot}`. The
robot escaped; do not imply that the Raider escaped or that it was defeated.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-B-01 | Overconfidence | J | `{location}` | "That machine has terrible manners about leaving {location}." | `{location}` | no | RX: machine fled; no Raider escape. |  |  |
| RX-B-02 | Genre tension | M | `{robot}` | "I hope {robot} appreciates how rude an exit that was." | `{robot}` | no | RX: robot left; no result for Raider. |  |  |
| RX-B-03 | Gear obsession | O | - | "A chassis with an escape plan is a personal insult." | - | no | RX: no human retreat or defeat. |  |  |
| RX-B-04 | Loot greed | J | `{location}` | "There goes a machine that would look grand on a trophy shelf at {location}." | machine, shelf | no | RX: hypothetical display, no capture or reward. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-C-01 | Loot greed | J | `{location}` | "A departing machine leaves my thoughts of spare parts behind at {location}." | machine | no | RX: no owned parts lost or Raider exit. |  |  |
| RX-C-02 | Overconfidence | M | `{robot}` | "I acknowledge {robot} made an exit rather than a concession." | `{robot}` | no | RX: robot escaped, no defeat or salvage. |  |  |
| RX-C-03 | Gear obsession | O | - | "A fleeing chassis is not a dismantled chassis." | - | no | RX: no defeat or salvage. |  |  |
| RX-C-04 | Genre tension | J | `{location}` | "A little distance from that machine is welcome at {location}." | `{location}` | no | RX: robot leaves; no safe outcome claimed. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-G-01 | Gear obsession | J | `{location}` | "That chassis has demonstrated the departure function at {location}." | `{location}` | no | RX: robot escaped; no damage outcome. |  |  |
| RX-G-02 | Genre tension | M | `{robot}` | "The exit by {robot} deserves a curtain made of question marks." | `{robot}` | no | RX: metaphor, no Raider escape. |  |  |
| RX-G-03 | Loot greed | O | - | "A fleeing machine is a disappointing addition to any parts collection." | machine | no | RX: hypothetical collection; no defeat or reward. |  |  |
| RX-G-04 | Loot greed | J | `{location}` | "A vanished mechanism is an inconvenient specimen at {location}." | `{location}` | no | RX: no capture or acquisition. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-H-01 | Loot greed | J | `{location}` | "An imaginary parts tray looks tragically underfed after that escape from {location}." | tray | no | RX: hypothetical tray, no cargo or pursuit. |  |  |
| RX-H-02 | Bureaucracy | M | `{robot}` | "The departure form for {robot} needs a furious stamp." | `{robot}`, form, stamp | yes | RX: hypothetical form, no defeat or owned parts lost. |  |  |
| RX-H-03 | Gear obsession | O | - | "A display cabinet is no use to a departing machine." | - | no | RX: comparison, no possession. |  |  |
| RX-H-04 | Overconfidence | J | `{location}` | "My souvenir urges protest that escape from {location}." | `{location}` | no | RX: no reward lost. |  |  |

## RC — RobotDefeatCargoResolved

**Required:** RobotDefeatCargoResolved and location. Every line must fit both
allocated cargo and no eligible cargo; no candidate implies a particular item,
recipient, or award.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-B-01 | Overconfidence | J | `{location}` | "A robot's cargo conclusion requires a more intimidating drumroll at {location}." | `{location}` | no | RC: fits allocated and no-eligible-cargo outcomes. |  |  |
| RC-B-02 | Loot greed | M | `{location}` | "At {location}, even the robot's cargo verdict makes me think of trophies." | robot, trophies | no | RC: no cargo or recipient asserted. |  |  |
| RC-B-03 | Genre tension | O | - | "A chassis's cargo verdict is a disappointingly quiet encore." | - | no | RC: neither award nor contents asserted. |  |  |
| RC-B-04 | Loot greed | J | `{location}` | "My prize ambitions demand a rematch with the machine's salvage result at {location}." | `{location}` | no | RC: no cargo or recipient. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-C-01 | Bureaucracy | J | `{location}` | "A robot cargo report deserves a calm reading at {location}." | robot, report | yes | RC: figurative report; either cargo outcome. |  |  |
| RC-C-02 | Gear obsession | M | `{location}` | "At {location}, robot salvage needs a tightly fastened temper." | `{location}` | no | RC: neither cargo outcome excluded. |  |  |
| RC-C-03 | Overconfidence | O | - | "I can accept machine salvage without naming myself the beneficiary." | - | no | RC: no personal award or departure asserted. |  |  |
| RC-C-04 | Loot greed | J | `{location}` | "My hopes for spare parts wait politely on the machine's cargo conclusion at {location}." | `{location}` | no | RC: no item or recipient asserted. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-G-01 | Loot greed | J | `{location}` | "A hypothetical golden cog would make a fine mascot for machine salvage at {location}." | cog | no | RC: no item or recipient asserted. |  |  |
| RC-G-02 | Genre tension | M | `{location}` | "At {location}, the machine's cargo ending sounds like a kettle refusing to whistle." | machine, kettle | no | RC: comparison, neither cargo outcome excluded. |  |  |
| RC-G-03 | Gear obsession | O | - | "A robot's cargo outcome deserves more knobs and fewer mysteries." | - | no | RC: no cog or acquisition asserted. |  |  |
| RC-G-04 | Overconfidence | J | `{location}` | "I can give the machine's salvage resolution a technical salute at {location}." | `{location}` | no | RC: neither cargo outcome excluded. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-H-01 | Loot greed | J | `{location}` | "I appraise robot salvage outcomes by the longing they provoke at {location}." | `{location}` | no | RC: sentimental opinion, no actual item or owner. |  |  |
| RC-H-02 | Genre tension | M | `{location}` | "At {location}, machine salvage is a subject my pockets refuse to discuss calmly." | `{location}` | no | RC: both cargo outcomes fit; no acquisition. |  |  |
| RC-H-03 | Loot greed | O | - | "Robot cargo merits a jeweled frame even when the picture disappoints." | - | no | RC: comparison, no award or contents asserted. |  |  |
| RC-H-04 | Overconfidence | J | `{location}` | "I can put a sentimental price on the machine's cargo conclusion at {location}." | `{location}` | no | RC: no prize claimed or excluded. |  |  |

## RV — RecoveryStarted or RecoveryCompleted

**Required:** RecoveryStarted or RecoveryCompleted and location. Both kinds
concern composure, not restored HP. Each line fits either phase without
claiming recovery has completed or begun anew.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-B-01 | Loot greed | J | `{location}` | "A trophy would suit this effort at composure at {location}." | trophy | no | RV: hypothetical trophy; either phase; no HP change. |  |  |
| RV-B-02 | Loot greed | M | `{location}` | "At {location}, a medal for composure would look excellent." | medal | no | RV: hypothetical medal; either phase, no HP change. |  |  |
| RV-B-03 | Gear obsession | O | - | "Composure is the hinge my temper keeps trying to bend." | - | no | RV: metaphor, no health claim. |  |  |
| RV-B-04 | Overconfidence | J | `{location}` | "Even recovery of my poise gets a stern look at {location}." | `{location}` | no | RV: composure only, neither phase implies healing. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-C-01 | Loot greed | J | `{location}` | "A quiet mind is worth more than a silver spoon at {location}." | spoon | no | RV: hypothetical spoon; either phase, no HP change. |  |  |
| RV-C-02 | Gear obsession | M | `{location}` | "At {location}, recovering my poise calls for a cushion of patience." | `{location}` | no | RV: figurative cushion; no item claim. |  |  |
| RV-C-03 | Overconfidence | O | - | "Settling my nerves is no occasion for a sprint." | - | no | RV: no completion or HP effect. |  |  |
| RV-C-04 | Genre tension | J | `{location}` | "My composure settles into recovery like a guest afraid to break the furniture at {location}." | `{location}` | no | RV: mental comparison, no actual companion, furniture or timing. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-G-01 | Gear obsession | J | `{location}` | "Calming my jitters gives my train of thought a smoother bearing at {location}." | `{location}` | no | RV: explicit mental metaphor, no carried instrument or HP gain. |  |  |
| RV-G-02 | Genre tension | M | `{location}` | "At {location}, composure deserves a quieter soundtrack than panic." | - | no | RV: either phase; no complete recovery or HP gain. |  |  |
| RV-G-03 | Overconfidence | O | - | "I can adjust my temper without replacing a fuse." | - | no | RV: no physical repair or healing. |  |  |
| RV-G-04 | Gear obsession | J | `{location}` | "Composure is lubricant for the nerves at {location}." | `{location}` | no | RV: metaphor, either phase. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-H-01 | Loot greed | J | `{location}` | "I'd keep the work of regaining calm in a velvet pouch at {location}." | `{location}` | no | RV: metaphor; no HP restoration. |  |  |
| RV-H-02 | Genre tension | M | `{location}` | "At {location}, my composure merits a protective display case." | `{location}` | no | RV: metaphor, both phases. |  |  |
| RV-H-03 | Gear obsession | O | - | "Composure deserves a priceless jar of its own." | - | no | RV: metaphor, no actual value or healing. |  |  |
| RV-H-04 | Overconfidence | J | `{location}` | "Restoring my nerve is more valuable than gold at {location}." | `{location}` | no | RV: opinion; no recovery completion. |  |  |

## DN — Downed

**Required:** Downed and committed location. Downed is not settled knockout;
no joke promises recovery, rescue, extraction, or an HP change.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| DN-B-01 | Overconfidence | J | `{location}` | "This ground hasn't heard the last of my attitude at {location}." | `{location}` | no | DN: no recovery or knockout promised. |  |  |
| DN-B-02 | Genre tension | M | `{location}` | "At {location}, my posture has become aggressively horizontal." | `{location}` | no | DN: metaphor, no settlement. |  |  |
| DN-B-03 | Gear obsession | O | - | "My pride needs a helmet for this angle." | - | no | DN: metaphor, no rescue. |  |  |
| DN-B-04 | Loot greed | J | `{location}` | "A velvet cushion would improve this position at {location}." | cushion | no | DN: hypothetical cushion, no recovery. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-C-01 | Genre tension | J | `{location}` | "The ground is a panorama with terrible seating arrangements at {location}." | ground | no | DN: downed viewpoint only; no KO, rescue or chair assumed. |  |  |
| DN-C-02 | Gear obsession | M | `{location}` | "At {location}, this floor-level setback makes upholstery look tactical." | `{location}` | no | DN: comparison, no carried gear or medical effect. |  |  |
| DN-C-03 | Overconfidence | O | - | "Falling out of vertical is no excuse to misplace my composure." | - | no | DN: downed perspective, no recovery or healing. |  |  |
| DN-C-04 | Genre tension | J | `{location}` | "A folding chair would find this position familiar at {location}." | chair | no | DN: comparison, no settled outcome. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-G-01 | Genre tension | J | `{location}` | "A toppled gyroscope would recognize this posture at {location}." | gyroscope | no | DN: hypothetical instrument; no healing. |  |  |
| DN-G-02 | Genre tension | M | `{location}` | "At {location}, my posture resembles a folding chair having doubts." | chair | no | DN: comparison, no KO or rescue. |  |  |
| DN-G-03 | Overconfidence | O | - | "I can analyze this angle without calling it a design feature." | - | no | DN: no completed recovery. |  |  |
| DN-G-04 | Loot greed | J | `{location}` | "A jeweled jack would flatter this downed posture at {location}." | jack | no | DN: comparison, no actual equipment or lift. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-H-01 | Loot greed | J | `{location}` | "I value an upright pose more than a bauble at {location}." | `{location}` | no | DN: comparison, no recovery. |  |  |
| DN-H-02 | Genre tension | M | `{location}` | "At {location}, even my ambitions are lying down." | `{location}` | no | DN: no settled KO or rescue. |  |  |
| DN-H-03 | Gear obsession | O | - | "A plush cushion would overvalue this particular position." | - | no | DN: comparison, no healing. |  |  |
| DN-H-04 | Overconfidence | J | `{location}` | "A throne would struggle to improve my dignity from this angle at {location}." | throne | no | DN: hypothetical throne; no recovery or item. |  |  |

## KO — KnockedOut

**Required:** KnockedOut and location. Knockout is settled; no line claims a
successful extraction, recovery, item salvage, or details absent from the record.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-B-01 | Genre tension | J | `{location}` | "That is an outrageously abrupt curtain at {location}." | `{location}` | no | KO: no extraction or recovery. |  |  |
| KO-B-02 | Overconfidence | M | `{location}` | "At {location}, even my pride has run out of speeches." | `{location}` | no | KO: settled knockout; no later action. |  |  |
| KO-B-03 | Gear obsession | O | - | "A helmet can't argue with a knockout finale." | - | no | KO: comparison; no equipment outcome. |  |  |
| KO-B-04 | Loot greed | J | `{location}` | "A cracked trophy would be a finer ending than this knockout at {location}." | trophy | no | KO: hypothetical trophy; no recovery. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-C-01 | Bureaucracy | J | `{location}` | "That knockout would earn an emphatic cancellation stamp at {location}." | stamp | yes | KO: hypothetical stamp, no recovery promise. |  |  |
| KO-C-02 | Gear obsession | M | `{location}` | "At {location}, even a handrail would disapprove of that ending." | `{location}` | no | KO: comparison; no escape. |  |  |
| KO-C-03 | Genre tension | O | - | "The knockout finally ran my understatement out of synonyms." | - | no | KO: settled knockout, no subsequent event claimed. |  |  |
| KO-C-04 | Genre tension | J | `{location}` | "That raid ending belongs on a postcard nobody would send from {location}." | postcard | no | KO: hypothetical postcard; settled loss, not death or extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-G-01 | Loot greed | J | `{location}` | "A gold-plated fuse wouldn't improve that knockout at {location}." | fuse | no | KO: comparison, no repair or recovery. |  |  |
| KO-G-02 | Genre tension | M | `{location}` | "At {location}, that knockout has the elegance of a dropped clock." | clock | no | KO: comparison, no later result. |  |  |
| KO-G-03 | Overconfidence | O | - | "Even my grandest optimization can't argue with being out cold." | - | no | KO: settled knockout, no later action or healing. |  |  |
| KO-G-04 | Gear obsession | J | `{location}` | "That knockout blew the fuse on my grand theory at {location}." | `{location}` | no | KO: figurative component, no equipment failure. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-H-01 | Loot greed | J | `{location}` | "I wouldn't pay a bottle cap for that ending at {location}." | `{location}` | no | KO: no transaction, salvage or extraction. |  |  |
| KO-H-02 | Genre tension | M | `{location}` | "At {location}, my appetite has met a very final curtain." | `{location}` | no | KO: settled knockout, no later recovery. |  |  |
| KO-H-03 | Gear obsession | O | - | "A decorative box is poor consolation for a knockout." | - | no | KO: comparison, no salvage. |  |  |
| KO-H-04 | Loot greed | J | `{location}` | "Being knocked out wasn't the treasure I hoped to bring back from {location}." | `{location}` | no | KO: loss, no treasure acquired or future outcome. |  |  |

## DL — DeadlineExpired

**Required:** DeadlineExpired and location. The deadline is a loss, not a
successful extraction; no ownership details or later recovery are implied.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-B-01 | Overconfidence | J | `{location}` | "The expired clock has terrible manners at {location}." | `{location}` | no | DL: loss, no successful extraction. |  |  |
| DL-B-02 | Genre tension | M | `{location}` | "At {location}, the deadline has stolen my preferred finale." | `{location}` | no | DL: figurative theft, no item loss inferred. |  |  |
| DL-B-03 | Gear obsession | O | - | "That expired deadline earned my stopwatch a glare." | - | no | DL: figurative instrument, no outcome altered. |  |  |
| DL-B-04 | Loot greed | J | `{location}` | "A gold watch would not improve that deadline at {location}." | watch | no | DL: hypothetical watch; no extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-C-01 | Genre tension | J | `{location}` | "The deadline is a door that doesn't care how politely I knock at {location}." | door | no | DL: metaphor for committed loss, no extraction or physical knock. |  |  |
| DL-C-02 | Gear obsession | M | `{location}` | "At {location}, the expired deadline reads like an accusation." | `{location}` | no | DL: no owned device or successful exit. |  |  |
| DL-C-03 | Overconfidence | O | - | "An hourglass would be no happier about that expired deadline." | hourglass | no | DL: hypothetical hourglass, no later recovery. |  |  |
| DL-C-04 | Genre tension | J | `{location}` | "The clock was less flexible than my hopes at {location}." | `{location}` | no | DL: no time or success inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-G-01 | Gear obsession | J | `{location}` | "The deadline timer has an unfriendly conclusion at {location}." | `{location}` | no | DL: deadline loss, no rescue. |  |  |
| DL-G-02 | Genre tension | M | `{location}` | "At {location}, the deadline has all the charm of a stopped watch." | watch | no | DL: comparison, no actual watch or extraction. |  |  |
| DL-G-03 | Overconfidence | O | - | "An expired time limit refuses to negotiate with arithmetic." | - | no | DL: no actual negotiation or changed time. |  |  |
| DL-G-04 | Gear obsession | J | `{location}` | "The hourglass of my optimism ran out at {location}." | `{location}` | no | DL: figurative instrument, no recovery. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-H-01 | Loot greed | J | `{location}` | "I wouldn't collect an expired deadline at {location}." | `{location}` | no | DL: no specific item loss or extraction. |  |  |
| DL-H-02 | Genre tension | M | `{location}` | "At {location}, the clock has no respect for my hunger." | `{location}` | no | DL: no cargo or later outcome. |  |  |
| DL-H-03 | Gear obsession | O | - | "Even a jeweled hourglass can't flatter an expired deadline." | - | no | DL: comparison, no acquisition. |  |  |
| DL-H-04 | Overconfidence | J | `{location}` | "A cracked marble would be more attractive than this ending at {location}." | marble | no | DL: comparison, no sale. |  |  |

## HS — HiddenPocketSalvaged

**Required:** HiddenPocketSalvaged and the committed preserved item.
`{item}` comes directly from this event's ItemName and unique item receipt,
not a general backpack item or a new draw. The owner-private event has already
committed preservation. No line repeats its value or asserts a safe extraction.
Additional exact-name eligibility: HS-B-03 and HS-C-03 require the committed
ItemName `Single Sock`; HS-G-03 requires `Left Boot`; HS-H-03 requires
`Vintage Comic Book`. These exact items exist in the current catalog, but
catalog presence alone does not make any row eligible: the recipient's
owner-private preservation receipt must name that exact item. Other item
names select a different row or fallback. No new extractor or loader exists.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-B-01 | Overconfidence | J | `{item}` | "The pocket's smallest swagger belongs to {item}." | `{item}` | no | HS: exact preserved item, no extra award. |  |  |
| HS-B-02 | Genre tension | M | `{item}` | "I'd frame {item} in a heroic close-up small enough for a seam." | `{item}` | no | HS: exact preserved item, no successful extraction. |  |  |
| HS-B-03 | Gear obsession | O | - | "Single Sock has more swagger than an imaginary helmet." | Single Sock, helmet | no | HS: exact preserved Single Sock only; hypothetical helmet. |  |  |
| HS-B-04 | Loot greed | J | `{item}` | "My prize grin is absurdly large for {item}." | `{item}` | no | HS: exact preserved item, no value inferred. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-C-01 | Genre tension | J | `{item}` | "A very small argument for sensible storage: {item}." | `{item}` | no | HS: exact preserved item, no extra award. |  |  |
| HS-C-02 | Gear obsession | M | `{item}` | "I give {item} a pocket-sized nod, not an extraction victory." | `{item}` | no | HS: no new award or safe extraction. |  |  |
| HS-C-03 | Loot greed | O | - | "Single Sock beats a hypothetical gold medal for practical comfort." | Single Sock, medal | no | HS: exact preserved Single Sock only; no medal. |  |  |
| HS-C-04 | Overconfidence | J | `{item}` | "I can appreciate a narrow escape on behalf of {item}." | `{item}` | no | HS: preservation metaphor, not Raider extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-G-01 | Gear obsession | J | `{item}` | "Elegant containment deserves a specimen like {item}." | `{item}` | no | HS: exact preserved item, no invented equipment. |  |  |
| HS-G-02 | Loot greed | M | `{item}` | "Even {item} would look smug beside a hypothetical velvet cushion." | `{item}`, cushion | no | HS: exact preserved item, no added object. |  |  |
| HS-G-03 | Overconfidence | O | - | "Left Boot makes an unusually persuasive argument for tiny storage." | Left Boot | no | HS: exact preserved Left Boot only, no pair or extra item. |  |  |
| HS-G-04 | Gear obsession | J | `{item}` | "My storage obsession has an excellent illustration: {item}." | `{item}` | no | HS: exact preserved item, no added capacity. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-H-01 | Loot greed | J | `{item}` | "A pedestal would be excessive; a pocket suits {item}." | `{item}` | no | HS: exact preserved item, no item invented. |  |  |
| HS-H-02 | Genre tension | M | `{item}` | "I regard {item} as a magnificent advertisement for concealed storage." | `{item}` | no | HS: subjective opinion, no value or extra item. |  |  |
| HS-H-03 | Gear obsession | O | - | "Vintage Comic Book deserves a pocket with a velvet lining." | Vintage Comic Book, pocket | no | HS: exact preserved Vintage Comic Book only; no new pocket. |  |  |
| HS-H-04 | Overconfidence | J | `{item}` | "I can describe a pocket-sized treasure as immense: {item}." | `{item}` | no | HS: exact preserved item, no quantity or value asserted. |  |  |

## HE — HiddenPocketEmpty

**Required:** HiddenPocketEmpty and location. Empty is not salvage; no line
invents a saved item, transfer, or other settlement outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-B-01 | Overconfidence | J | `{location}` | "That pocket has a lot of nerve being empty at {location}." | `{location}` | no | HE: no salvage or item. |  |  |
| HE-B-02 | Genre tension | M | `{location}` | "At {location}, the hidden compartment has declined its heroic cameo." | `{location}` | no | HE: no award or recovery. |  |  |
| HE-B-03 | Gear obsession | O | - | "An empty hidden compartment is a very unimpressive opponent." | - | no | HE: metaphor, no actual opponent. |  |  |
| HE-B-04 | Loot greed | J | `{location}` | "My trophy ambitions found no seat in that pocket at {location}." | `{location}` | no | HE: empty pocket, no trophy or saved item. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-C-01 | Genre tension | J | `{location}` | "My hidden pocket has cast absence in the starring role at {location}." | `{location}` | no | HE: no item saved. |  |  |
| HE-C-02 | Loot greed | M | `{location}` | "At {location}, the empty pocket offers fewer treasures than a wish." | pocket | no | HE: empty, no salvage. |  |  |
| HE-C-03 | Overconfidence | O | - | "An empty seam is no reason to panic about the pocket." | - | no | HE: no item or reward asserted. |  |  |
| HE-C-04 | Loot greed | J | `{location}` | "My appetite for salvage is staring into a seam-sized absence at {location}." | `{location}` | no | HE: empty pocket, no salvage claimed. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-G-01 | Gear obsession | J | `{location}` | "That compartment has optimized itself for emptiness at {location}." | `{location}` | no | HE: no item saved or transferred. |  |  |
| HE-G-02 | Genre tension | M | `{location}` | "At {location}, the empty pocket is less exciting than a blank wall." | pocket, wall | no | HE: comparison, no salvage. |  |  |
| HE-G-03 | Overconfidence | O | - | "Even an empty seam can disappoint with impressive precision." | seam | no | HE: no saved item. |  |  |
| HE-G-04 | Loot greed | J | `{location}` | "My appetite for components objects to that empty pocket at {location}." | `{location}` | no | HE: no acquisition or salvage. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|
| HE-H-01 | Loot greed | J | `{location}` | "An empty pocket is a personal insult at {location}." | `{location}` | no | HE: no saved item. |  |  |
| HE-H-02 | Genre tension | M | `{location}` | "At {location}, the hidden compartment offers my hunger nothing." | `{location}` | no | HE: empty, no transfer. |  |  |
| HE-H-03 | Gear obsession | O | - | "A vacant pouch is an architectural insult to a treasure enthusiast." | - | no | HE: no inventory change beyond committed empty pocket. |  |  |
| HE-H-04 | Overconfidence | J | `{location}` | "A velvet pouch would be ashamed of that empty seam at {location}." | pouch, seam | no | HE: hypothetical pouch, no salvage. |  |  |

## Automated review checks

### C3 reproducible screening

From the repository root, execute the embedded block without creating a file:

```powershell
python -c 'from pathlib import Path; s=Path(r"docs\DIARY_PILOT_LINES.md").read_text(encoding="utf-8"); exec(s.split("\n```python c3-checker\n",1)[1].split("\n```",1)[0])' --strict
```

Use `--evidence` instead of `--strict` for per-ID noun/slot evidence, or
`--legacy-strict` to reproduce the unresolved C2-only screens. This
screen is deliberately conservative, not semantic approval: an item in the
dictionary may be used figuratively or in a contradiction, and an unlisted
physical object may be missed. Human review must confirm the object is used in
the actual joke and is a fact or explicit comparison, and that each mode
describes the actual joke. Office union includes the row flag and keyword
screen; neither is a substitute for manual metaphor review. Before is the
unmodified C2 pilot at `a9ecc30a61811eac5cc4a7d8d1d0714a0766920a`; AI has no
before rows. `--strict` checks the stated C3 limits; legacy failures are
printed separately in every run, never silently counted as passes.
The physical-word screen excludes mental drawings, waveforms, soundtracks,
and direction words. `object` as a verb does not qualify; recovered objects
require a noun phrase. The row's Concrete column is an anchor note, not a
pass flag: `{location}` entries alone are discarded. Explicit hypothetical
props qualify only as comparisons, never as assertions of owned gear.

#### Recorded C3 run (2026-11-14)

These are conservative strict-grounding counts for the author-reviewed
draft, not human approval or measured runtime improvement. The same noun/slot
procedure is applied before and after; words missed by the dictionary are not
counted. The unchanged baseline has 327 location-only annotation rows, but some
of their actual jokes contain physical comparisons: simply subtracting 327
from the old 448 would also undercount. New strict totals are **358/592 (60.47%)
-> 519/624 (83.17%)**, with **44/96 -> 0/100** sets below 60%.
All 100 sets retain J/M/O, the combined change stem is at most one/set after,
and all four traits meet C3 opener, six-mode, hedge and office caps.

| Scope | N before → after | Concrete screen | My | I / including contractions | Office union | Hedge | J/M/O | Modes before → after |
|---|---:|---:|---:|---:|---:|---:|---|---|
| All | 592 → 624 | 358 → 519 | 146 → 43 | 95/114 → 41/54 | 42 → 48 | 0 → 4 | 248/192/152 → 260/204/160 | 192/145/4/142/104/5 → 136/136/41/135/135/41 |
| B | 148 → 156 | 86 → 130 | 39 → 14 | 21/27 → 8/12 | 3 → 13 | 0 → 1 | 62/48/38 → 65/51/40 | 26/57/1/47/17/0 → 34/34/10/34/34/10 |
| C | 148 → 156 | 87 → 132 | 23 → 11 | 28/31 → 13/17 | 2 → 12 | 0 → 3 | 62/48/38 → 65/51/40 | 38/31/1/61/16/1 → 34/34/10/33/34/11 |
| G | 148 → 156 | 96 → 131 | 46 → 6 | 21/25 → 8/11 | 32 → 10 | 0 → 0 | 62/48/38 → 65/51/40 | 94/33/1/3/15/2 → 34/34/11/34/33/10 |
| H | 148 → 156 | 89 → 126 | 38 → 12 | 25/31 → 12/14 | 5 → 13 | 0 → 0 | 62/48/38 → 65/51/40 | 34/24/1/31/56/2 → 34/34/10/34/34/10 |

Mode order is Gear obsession, Overconfidence, False optimism, Genre tension,
Loot greed, Bureaucracy. Actual per-trait mode shares now range 6.41%-21.79%;
before, Gadgeteer Gear obsession alone was 94/148 (63.51%). Overall Gear
obsession falls from 192/592 (32.43%) to 136/624 (21.79%); Bureaucracy rises
from 5/592 (0.84%) to 41/624 (6.57%).
My falls 24.66% -> 6.89%; whole-word I 16.05% -> 6.57%, and
contraction-inclusive I 19.26% -> 8.65%. Each trait passes even the
contraction-inclusive interpretation (maximum 17/156 = 10.90%).
Hedges are 0% -> 0.64%; office union 7.09% -> 7.69%, with every trait
at most 13/156 = 8.33%. Sign-gated PC uses actual up/down changes; it does
not expose numbers or infer attempted changes.

The change-synonym group includes revised/adjusted/shifting/reshuffled/altered
and rewrite/swap variants. PC counts are **54/64 -> 1/64**; by B/C/G/H,
12/14/13/15 -> 0/0/1/0. The single new PC hit is `adjustable` in the wrench
comparison, conservatively grouped with `adjust`; it is not an abstract
change-subject joke. Across all kinds, 58 -> 6 lines use that group.
Maximum group lines in any set: 15 -> 1 (required ceiling three).

**Unresolved, before loading:** C2's ordinary-content stem screen worsens:
maximum three/set after; trait maxima 20/10/16/13 after, versus eight each
before (excluding the combined change stem). Fifty sets exceed two and all
four traits exceed eight. `make` (40), `deserve` (38) and recurring hypothetical
prop constructions remain human-edit priorities; lexical diversity is not
proof of comedy. Thirty-seven sets exceed the earlier 12% **per-set** office
screen even though all traits pass C3's 12% office share. SH-B-03 must stay
unchanged; changing set sizes or the earlier screen requires a decision.
`--strict` exits zero for C3; `--legacy-strict` exits one and lists these
91 unresolved screens. Do not treat this as unconditional load readiness.

#### All 100 set counters: before -> after

Counter tuples are **strict concrete / change-synonym / My / I including
contractions / hedge / office union / maximum ordinary stem**. Mode tuples
use the six-mode order above. Divide each count by its set N for its share.
N and J/M/O are identical for all four traits in each family: PC **16,
6/5/5 -> 16, 6/5/5**; SH, RS, EL, ES, SI, AR, PS, PF, HW and HI **8,
3/3/2 -> 8, 3/3/2**; all other original families **4, 2/1/1 -> 4, 2/1/1**.
AI is **0, 0/0/0 -> 8, 3/3/2**. Thus this reports shapes and denominators
for every set, including the previously absent interruption sets.

| Set | Before counters | After counters | Modes before -> after |
|---|---|---|---|
| AI-B | 0/0/0/0/0/0/0 | 8/1/0/1/0/0/3 | 0/0/0/0/0/0 -> 2/2/1/2/1/0 |
| AI-C | 0/0/0/0/0/0/0 | 8/0/0/2/0/0/2 | 0/0/0/0/0/0 -> 2/2/1/2/1/0 |
| AI-G | 0/0/0/0/0/0/0 | 8/0/0/2/0/0/2 | 0/0/0/0/0/0 -> 2/2/1/2/1/0 |
| AI-H | 0/0/0/0/0/0/0 | 7/0/0/0/0/1/3 | 0/0/0/0/0/0 -> 1/3/1/2/0/1 |
| PC-B | 6/12/13/0/0/0/2 | 16/0/0/0/1/1/3 | 3/6/1/5/1/0 -> 3/4/1/5/2/1 |
| PC-C | 7/14/10/2/0/0/2 | 15/0/0/0/0/1/3 | 4/3/1/7/1/0 -> 3/3/1/5/3/1 |
| PC-G | 9/13/12/1/0/3/2 | 16/1/0/0/0/1/3 | 13/1/1/0/1/0 -> 4/3/1/4/3/1 |
| PC-H | 7/15/12/1/0/1/2 | 16/0/0/0/0/1/3 | 4/2/1/0/8/1 -> 4/2/1/3/5/1 |
| SH-B | 4/0/2/0/0/1/2 | 7/0/1/0/0/2/3 | 3/3/0/2/0/0 -> 2/1/1/2/1/1 |
| SH-C | 5/0/0/1/0/0/2 | 7/0/0/0/0/1/3 | 3/1/0/3/1/0 -> 3/1/1/1/1/1 |
| SH-G | 3/0/0/2/0/2/2 | 6/0/0/1/0/1/3 | 6/1/0/0/1/0 -> 3/0/1/2/1/1 |
| SH-H | 5/0/0/2/0/2/2 | 6/0/0/1/0/3/3 | 2/1/0/1/4/0 -> 1/1/1/1/3/1 |
| UD-B | 4/0/0/1/0/0/2 | 4/0/0/0/0/1/3 | 0/2/0/2/0/0 -> 0/1/0/2/0/1 |
| UD-C | 3/0/0/1/0/1/2 | 3/0/0/1/0/2/2 | 0/1/0/2/0/1 -> 0/1/0/1/0/2 |
| UD-G | 4/0/1/1/0/3/1 | 4/0/0/0/0/1/2 | 3/1/0/0/0/0 -> 0/1/0/1/1/1 |
| UD-H | 4/0/1/1/0/0/2 | 4/0/0/0/0/1/1 | 1/1/0/1/1/0 -> 1/1/0/1/0/1 |
| RS-B | 6/0/5/2/0/1/2 | 7/0/1/1/0/2/3 | 1/3/0/3/1/0 -> 2/1/1/1/2/1 |
| RS-C | 7/0/3/2/0/0/2 | 7/0/3/2/0/0/2 | 2/2/0/4/0/0 -> 2/2/0/3/1/0 |
| RS-G | 7/0/4/1/0/0/2 | 7/0/3/0/0/1/3 | 6/2/0/0/0/0 -> 5/1/0/1/0/1 |
| RS-H | 8/0/3/2/0/1/2 | 7/0/3/1/0/1/3 | 2/1/0/2/3/0 -> 2/2/1/2/1/0 |
| EL-B | 3/0/2/1/0/0/1 | 7/0/1/0/0/1/2 | 1/3/0/3/1/0 -> 2/0/1/2/2/1 |
| EL-C | 2/0/1/1/0/0/2 | 6/0/1/0/0/0/3 | 2/1/0/4/1/0 -> 2/1/1/1/3/0 |
| EL-G | 3/0/3/1/0/1/2 | 6/0/0/1/0/0/2 | 5/2/0/0/1/0 -> 2/3/1/1/1/0 |
| EL-H | 0/0/4/1/0/0/2 | 5/0/2/0/0/0/2 | 2/1/0/2/3/0 -> 3/2/1/1/1/0 |
| ES-B | 3/0/2/2/0/0/1 | 5/0/0/0/0/1/3 | 1/3/0/3/1/0 -> 1/1/1/2/2/1 |
| ES-C | 4/0/1/3/0/0/2 | 8/0/1/1/1/1/2 | 2/2/0/3/1/0 -> 2/2/1/1/1/1 |
| ES-G | 6/0/2/1/0/1/2 | 7/0/0/0/0/1/3 | 5/2/0/0/1/0 -> 1/2/1/2/1/1 |
| ES-H | 2/0/3/0/0/0/2 | 5/0/1/0/0/1/2 | 2/1/0/2/3/0 -> 2/1/1/2/1/1 |
| SI-B | 7/0/1/3/0/0/2 | 7/0/0/3/0/1/3 | 0/2/0/3/3/0 -> 1/1/0/2/3/1 |
| SI-C | 8/0/1/2/0/0/2 | 7/0/1/1/0/0/2 | 2/2/0/2/2/0 -> 1/2/1/0/4/0 |
| SI-G | 8/0/2/3/0/2/2 | 8/0/0/1/0/1/2 | 2/2/0/2/2/0 -> 0/1/1/2/3/1 |
| SI-H | 7/0/0/3/0/0/2 | 7/0/0/1/0/0/2 | 1/1/0/1/5/0 -> 1/2/1/1/3/0 |
| SN-B | 1/0/1/1/0/0/2 | 3/0/0/0/0/1/2 | 0/2/0/1/1/0 -> 0/1/0/1/1/1 |
| SN-C | 1/1/0/1/0/0/2 | 4/1/0/0/0/1/3 | 1/1/0/1/1/0 -> 1/1/0/0/1/1 |
| SN-G | 2/0/2/1/0/0/2 | 3/0/0/0/0/1/3 | 2/1/0/0/1/0 -> 1/0/0/1/1/1 |
| SN-H | 1/0/2/1/0/0/1 | 3/0/0/0/0/0/2 | 1/0/0/1/2/0 -> 1/1/0/1/1/0 |
| SX-B | 2/0/1/1/0/0/2 | 3/0/0/1/0/1/3 | 0/2/0/1/1/0 -> 0/2/0/0/1/1 |
| SX-C | 2/0/0/0/0/1/2 | 3/0/0/0/0/1/3 | 1/0/0/2/1/0 -> 1/0/0/1/2/0 |
| SX-G | 3/0/0/0/0/0/2 | 3/0/0/0/0/1/1 | 2/1/0/0/1/0 -> 1/1/0/0/1/1 |
| SX-H | 2/0/0/1/0/0/2 | 3/0/0/0/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| AR-B | 1/0/1/1/0/1/2 | 5/0/2/0/0/2/3 | 1/3/0/3/1/0 -> 2/0/1/2/2/1 |
| AR-C | 2/0/1/2/0/0/1 | 7/0/1/0/0/1/3 | 2/2/0/3/1/0 -> 1/2/1/0/3/1 |
| AR-G | 2/1/2/0/0/3/2 | 6/1/0/0/0/1/3 | 6/2/0/0/0/0 -> 1/2/1/1/2/1 |
| AR-H | 1/0/2/1/0/0/2 | 6/0/0/0/0/1/3 | 2/1/0/2/3/0 -> 1/1/1/2/2/1 |
| AD-B | 0/0/1/1/0/0/2 | 3/0/0/1/0/0/3 | 1/2/0/1/0/0 -> 2/2/0/0/0/0 |
| AD-C | 2/0/0/1/0/0/2 | 3/0/0/1/0/1/2 | 1/1/0/2/0/0 -> 0/2/0/1/0/1 |
| AD-G | 4/0/2/0/0/0/2 | 3/0/0/0/0/1/2 | 3/1/0/0/0/0 -> 0/1/0/1/1/1 |
| AD-H | 2/0/2/0/0/0/2 | 3/0/1/0/0/0/2 | 0/1/0/1/2/0 -> 0/1/0/1/2/0 |
| PS-B | 6/0/2/1/0/0/2 | 6/0/2/0/0/0/3 | 2/3/0/2/1/0 -> 2/2/1/1/2/0 |
| PS-C | 5/0/1/2/0/0/2 | 5/0/0/1/1/0/3 | 3/1/0/3/1/0 -> 2/1/1/1/3/0 |
| PS-G | 3/0/2/1/0/1/2 | 5/0/0/1/0/0/3 | 5/2/0/0/1/0 -> 1/2/1/2/2/0 |
| PS-H | 5/0/1/1/0/0/2 | 5/0/1/0/0/0/3 | 2/1/0/2/3/0 -> 2/2/1/2/1/0 |
| PF-B | 5/0/0/1/0/0/2 | 6/0/0/0/0/0/2 | 2/3/0/2/1/0 -> 3/2/1/1/1/0 |
| PF-C | 4/0/1/2/0/0/2 | 5/0/0/2/1/0/3 | 2/2/0/3/1/0 -> 2/2/1/1/2/0 |
| PF-G | 3/0/2/1/0/1/2 | 7/0/1/0/0/0/3 | 5/2/0/0/1/0 -> 1/2/1/1/3/0 |
| PF-H | 6/0/0/1/0/0/2 | 6/0/0/1/0/1/3 | 2/1/0/2/3/0 -> 2/2/0/2/1/1 |
| HW-B | 5/0/1/2/0/0/2 | 5/0/1/0/0/0/3 | 1/3/0/3/1/0 -> 2/2/1/2/1/0 |
| HW-C | 4/0/0/2/0/0/2 | 5/0/0/2/0/0/3 | 2/2/0/3/1/0 -> 2/2/1/1/2/0 |
| HW-G | 6/0/2/2/0/2/2 | 6/0/0/1/0/0/3 | 5/2/0/0/1/0 -> 0/2/1/3/2/0 |
| HW-H | 6/0/1/3/0/0/2 | 6/0/0/2/0/0/3 | 2/2/0/2/2/0 -> 2/3/1/2/0/0 |
| HI-B | 6/0/2/3/0/0/2 | 7/0/2/3/0/0/2 | 2/3/0/2/1/0 -> 2/2/0/1/3/0 |
| HI-C | 5/0/0/2/0/0/2 | 7/0/0/0/0/1/2 | 2/2/0/3/1/0 -> 2/2/0/2/1/1 |
| HI-G | 5/1/3/2/0/2/2 | 5/1/0/1/0/0/3 | 4/2/0/0/1/1 -> 1/3/1/2/1/0 |
| HI-H | 5/0/2/2/0/1/2 | 6/0/2/0/0/2/3 | 2/1/0/2/2/1 -> 2/1/0/2/1/2 |
| RD-B | 3/0/0/1/0/0/1 | 3/0/0/0/0/0/2 | 1/2/0/1/0/0 -> 1/2/0/1/0/0 |
| RD-C | 4/0/0/1/0/0/2 | 4/0/0/0/0/0/2 | 1/1/0/2/0/0 -> 1/1/0/2/0/0 |
| RD-G | 4/0/1/1/0/1/2 | 4/0/0/0/0/0/2 | 2/1/0/0/1/0 -> 1/1/0/0/2/0 |
| RD-H | 4/0/1/1/0/0/2 | 4/0/1/0/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| RX-B | 4/0/0/2/0/0/2 | 4/0/0/1/0/0/2 | 1/2/0/1/0/0 -> 1/1/0/1/1/0 |
| RX-C | 4/0/0/1/0/0/2 | 4/0/0/1/0/0/2 | 1/1/0/2/0/0 -> 1/1/0/1/1/0 |
| RX-G | 3/0/0/2/0/1/1 | 3/0/0/0/0/0/1 | 2/1/0/0/1/0 -> 1/0/0/1/2/0 |
| RX-H | 2/0/2/1/0/0/2 | 3/0/1/0/0/1/2 | 1/1/0/1/1/0 -> 1/1/0/0/1/1 |
| RC-B | 4/0/1/0/0/0/2 | 4/0/1/0/0/0/3 | 0/1/0/2/1/0 -> 0/1/0/1/2/0 |
| RC-C | 4/0/1/1/0/0/2 | 4/0/1/1/0/1/2 | 1/1/0/1/1/0 -> 1/1/0/0/1/1 |
| RC-G | 3/0/1/1/0/2/2 | 4/0/0/1/0/0/3 | 1/1/0/1/0/1 -> 1/1/0/1/1/0 |
| RC-H | 4/0/0/2/0/0/2 | 4/0/0/2/0/0/2 | 0/1/0/1/2/0 -> 0/1/0/1/2/0 |
| RV-B | 1/0/1/0/0/0/2 | 3/0/0/0/0/0/3 | 1/2/0/1/0/0 -> 1/1/0/0/2/0 |
| RV-C | 2/0/1/0/0/0/2 | 3/0/1/0/0/0/2 | 1/1/0/2/0/0 -> 1/1/0/1/1/0 |
| RV-G | 4/1/0/1/0/0/2 | 3/1/0/1/0/0/2 | 3/1/0/0/0/0 -> 2/1/0/1/0/0 |
| RV-H | 3/0/0/1/0/0/2 | 3/0/0/1/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| DN-B | 2/0/1/1/0/0/1 | 3/0/1/0/0/0/1 | 1/2/0/1/0/0 -> 1/1/0/1/1/0 |
| DN-C | 2/0/0/1/0/0/1 | 3/0/0/0/0/0/1 | 1/1/0/2/0/0 -> 1/1/0/2/0/0 |
| DN-G | 2/0/2/1/0/1/2 | 3/0/0/1/0/0/3 | 3/1/0/0/0/0 -> 0/1/0/2/1/0 |
| DN-H | 2/0/1/1/0/0/1 | 3/0/0/1/0/0/1 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| KO-B | 2/0/0/0/0/0/2 | 3/0/0/0/0/0/2 | 1/1/0/2/0/0 -> 1/1/0/1/1/0 |
| KO-C | 1/0/0/0/0/0/2 | 3/0/0/0/0/1/2 | 1/0/0/3/0/0 -> 1/0/0/2/0/1 |
| KO-G | 2/0/1/0/0/1/2 | 3/0/0/0/0/0/3 | 3/1/0/0/0/0 -> 1/1/0/1/1/0 |
| KO-H | 3/0/0/1/0/0/1 | 3/0/0/1/0/0/1 | 1/0/0/1/2/0 -> 1/0/0/1/2/0 |
| DL-B | 3/0/0/1/0/0/2 | 3/0/0/0/0/0/3 | 1/2/0/1/0/0 -> 1/1/0/1/1/0 |
| DL-C | 1/0/0/1/0/0/2 | 3/0/0/0/0/0/3 | 1/1/0/2/0/0 -> 1/1/0/2/0/0 |
| DL-G | 2/0/0/0/0/1/2 | 3/0/0/0/0/0/2 | 3/1/0/0/0/0 -> 2/1/0/1/0/0 |
| DL-H | 2/0/1/1/0/0/2 | 3/0/0/1/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| HS-B | 4/0/1/2/0/0/1 | 4/0/1/1/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| HS-C | 4/0/0/2/0/0/1 | 4/0/0/2/0/0/1 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| HS-G | 4/0/1/1/0/2/1 | 4/0/1/0/0/0/2 | 3/1/0/0/0/0 -> 2/1/0/0/1/0 |
| HS-H | 4/0/0/2/0/0/2 | 4/0/0/2/0/0/3 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| HE-B | 4/0/1/0/0/0/2 | 4/0/1/0/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |
| HE-C | 4/0/2/0/0/0/2 | 4/0/2/0/0/0/3 | 1/1/0/1/1/0 -> 0/1/0/1/2/0 |
| HE-G | 4/0/1/1/0/2/2 | 4/0/1/0/0/0/3 | 2/1/0/0/1/0 -> 1/1/0/1/1/0 |
| HE-H | 4/0/0/1/0/0/2 | 4/0/0/0/0/0/2 | 1/1/0/1/1/0 -> 1/1/0/1/1/0 |

```python c3-checker
import collections, json, pathlib, re, subprocess, sys

path = pathlib.Path(r"docs\DIARY_PILOT_LINES.md")
before = subprocess.check_output(
    ["git", "show", "a9ecc30a61811eac5cc4a7d8d1d0714a0766920a:docs/DIARY_PILOT_LINES.md"],
    text=True, encoding="utf-8")
after = path.read_text(encoding="utf-8")
modes = ("Gear obsession", "Overconfidence", "False optimism",
         "Genre tension", "Loot greed", "Bureaucracy")
stop = set("""a an the i me my mine myself we us our you your it its itself he his
she her they them their this that these those there here am is are was were be been
being have has had having do does did doing and or but if as than then so because
when while where who what which to of in on at by for from with without into through
out up down off over under about against between after before until during above
below around across not no nor never only just very too also even enough how much
more most less one two all any some every each both either neither other another
same such can cannot will would shall should must ought now already yet again once
twice instead together i'd i'll i've i'm don't doesn't didn't isn't aren't wasn't
weren't won't wouldn't can't couldn't shouldn't it's that's there's""".split())
physical = re.compile(
    r"\b(?:shield|barrier|backpack|pocket|pouch|compartment|seam|helmet|armor|"
    r"strap|buckle|knot|handrail|railing|umbrella|roof|brick|trumpet|shoe|shoes|"
    r"tin|wrench|cog|cogs|hinge|gear|gears|screw|rivet|flywheel|hammer|"
    r"multimeter|battery|washer|zipper|spoon|bottle|cap|shelf|case|cabinet|"
    r"box|bell|drum|glove|fireworks|lid|cape|crown|chair|compass|"
    r"cushion|button|spring|radio|clock|plug|kettle|screwdriver|ribbon|teacup|"
    r"marble|curtain|pillow|trophy|medal|sock|book|gong|megaphone|chalice|whistle|parachute|"
    r"metronome|seesaw|lantern|label|gauge|telescope|gyroscope|bolt|pedestal|"
    r"diamond|tag|goblet|chandelier|key|boots|boot|map|lever|prototype|"
    r"robot|machine|chassis|parcel|luggage|container|cargo|bundle|load|haul|"
    r"cart|crate|locker|circuit|fuse|wire|wiring|dial|knob|tool|tools|"
    r"component|components|padding|jewels|jewel|picture|jar|jacket|coat|blanket|umbrella|"
    r"hourglass|timer|wall|furniture|flagpole|"
    r"door|handle|switch|rope|film|reel|hat|ticket|seat|coin|coins|"
    r"bag|brick|paper|page|clipboard|document|stamp|receipt|"
    r"instrument|"
    r"circuitry|insulation|barometer|sensor|tape|suitcase|"
    r"carpet|vacuum|bootlace|pendulum|stopwatch|jack|shoelaces|trinket|"
    r"bauble|buttons|chest|throne|float|vault|watch|cube|belt|bench|biscuit|"
    r"rocket|goose|banana|peel|siren|ground|floor|tray|solder|bearing|"
    r"lubricant|upholstery|slide.rule|divider|mat|postcard|form|cup|"
    r"trumpets|crowns)\b", re.I)
slot = re.compile(r"\{(?:activity|condition|source|item|robot)\}")
hedge = re.compile(r"\b(?:next|perhaps|may|could|still|remain(?:s|ed|ing)?)\b", re.I)
office = re.compile(
    r"\b(?:office|paperwork|records?|checklist|forms?|policy|policies|editor|chapter|"
    r"paragraph|specification|labels?|file[ds]?|filing|invoice|receipts?|audit\w*|"
    r"manifest|ledger|stamps?|permits?|committee|reports?|minutes|procedure|"
    r"protocol|bureaucrac\w*|memos?|catalog(?:ue)?|department|management|budget|"
    r"requisition|application|approval|account\w*|clerk|contract|documentation|"
    r"document|meeting|agenda|spreadsheet|certificate|signature|warranty|pension|"
    r"payroll|overtime|notes?|noted|notation|diagrams?|schematics?|blueprints?|"
    r"charts?|sketch\w*|waveforms?|asterisks?|full.stop|billing)\b", re.I)
changes = re.compile(
    r"\b(?:revis\w*|adjust\w*|shift\w*|shuffl\w*|reshuffl\w*|alter\w*|"
    r"chang\w*|reweight\w*|recalibrat\w*|retun\w*|rearrang\w*|rebuild\w*|"
    r"rebuilt|rewir\w*|rewrit\w*|rewrote|swapp\w*|rework\w*|"
    r"reset\w*|refit\w*|updat\w*|calibrat\w*)\b", re.I)
aliases = {"thoughts":"think", "thought":"think", "thinking":"think",
           "greedy":"greed", "collector":"collect", "collection":"collect",
           "collected":"collect", "collecting":"collect", "collectible":"collect",
           "protective":"protect", "protection":"protect", "protected":"protect",
           "recovery":"recover", "recovered":"recover", "recovering":"recover",
           "gave":"give", "given":"give", "giving":"give", "got":"get",
           "made":"make", "making":"make", "took":"take", "taken":"take",
           "spent":"spend", "found":"find", "built":"build", "ran":"run"}
name_gates = {"HS-B-03":"Single Sock", "HS-C-03":"Single Sock",
              "HS-G-03":"Left Boot", "HS-H-03":"Vintage Comic Book"}

def parse(text):
    rows = []
    for line in text.splitlines():
        parts = [x.strip() for x in line.strip().strip("|").split("|")]
        if len(parts) != 10 or not re.fullmatch(r"[A-Z]{2}-[BCGH]-\d{2}", parts[0]):
            continue
        rows.append(dict(zip(("id","mode","shape","slots","text","concrete",
                              "office","fit","decision","note"), parts)))
    return rows

def evidence(r):
    text = r["text"]
    result = list(dict.fromkeys(x.lower() for x in slot.findall(text)))
    result += list(dict.fromkeys(x.lower() for x in physical.findall(
        re.sub(r"\{[^}]+\}", "", text))))
    if re.search(r"\b(?:recovered|awarded|saved) (?:object|prize|find)\b", text, re.I):
        result.append("committed recovered object")
    return result

def stem(w):
    if w.endswith("'s"): w = w[:-2]
    if w in aliases: return aliases[w]
    if changes.fullmatch(w): return "CHANGE-SYNONYM"
    if w.endswith("ies") and len(w) > 4: return w[:-3] + "y"
    for end in ("ing", "ed"):
        if w.endswith(end) and len(w) > len(end) + 3:
            w = w[:-len(end)]
            if len(w) > 2 and w[-1] == w[-2]: w = w[:-1]
            return w
    if w.endswith("s") and not w.endswith(("ss","us","is")) and len(w) > 3:
        return w[:-1]
    return w

def stems(r):
    text = re.sub(r"\{[^}]+\}", "", r["text"].lower())
    return {stem(w) for w in re.findall(r"[a-z]+(?:'[a-z]+)?", text) if w not in stop}

def metrics(group):
    n = len(group)
    lexical = collections.Counter(t for r in group for t in stems(r))
    shapes = collections.Counter(r["shape"] for r in group)
    mode = collections.Counter(r["mode"] for r in group)
    return dict(n=n, concrete=sum(bool(evidence(r)) for r in group),
        hedge=sum(bool(hedge.search(r["text"])) for r in group),
        office=sum(bool(office.search(r["text"])) or r["office"].lower()=="yes" for r in group),
        my=sum(bool(re.match(r'^\s*"?my\b', r["text"], re.I)) for r in group),
        i=sum(bool(re.match(r'^\s*"?i\b(?![’\'])', r["text"], re.I)) for r in group),
        icontract=sum(bool(re.match(r'^\s*"?i(?:\b|[’\'])', r["text"], re.I)) for r in group),
        change=sum(bool(changes.search(r["text"])) for r in group),
        stems=dict(lexical.most_common(5)), shape=[shapes[x] for x in ("J","M","O")],
        modes=[mode[x] for x in modes])

old, new = parse(before), parse(after)
def sets(rows):
    d = collections.defaultdict(list)
    for r in rows: d[r["id"].rsplit("-",1)[0]].append(r)
    return d

oldsets, newsets = sets(old), sets(new)
print("MODE ORDER: " + " / ".join(modes))
print("GLOBAL before=" + json.dumps(metrics(old), sort_keys=True))
print("GLOBAL after =" + json.dumps(metrics(new), sort_keys=True))
for trait in "BCGH":
    a = [r for r in old if r["id"].split("-")[1] == trait]
    b = [r for r in new if r["id"].split("-")[1] == trait]
    print("TRAIT " + trait + " before=" + json.dumps(metrics(a),sort_keys=True))
    print("TRAIT " + trait + " after =" + json.dumps(metrics(b),sort_keys=True))
print("SET: before -> after; metrics include N, concrete, hedge, office, opener, "
      "change synonym, top stems, J/M/O, six modes")
for key in newsets:
    print(key + ": " + json.dumps(metrics(oldsets[key]),sort_keys=True) +
          " -> " + json.dumps(metrics(newsets[key]),sort_keys=True))
if "--evidence" in sys.argv:
    for r in new:
        print("EVIDENCE " + r["id"] + ": " + (", ".join(evidence(r)) or "NONE"))
problems = []
legacy_problems = []
if len(new) != 624 or len({r["id"] for r in new}) != len(new):
    problems.append("candidate count or duplicate ID")
if not {r["id"] for r in old}.issubset({r["id"] for r in new}):
    problems.append("existing ID lost")
for key, group in newsets.items():
    m = metrics(group); n = m["n"]
    if m["concrete"] < .6*n: problems.append(key + " concrete")
    if m["hedge"] > .15*n: problems.append(key + " hedge")
    if m["office"] > .12*n: legacy_problems.append(key + " office")
    if m["modes"][2] > n//8: problems.append(key + " optimism")
    if m["change"] > 3: problems.append(key + " change synonyms")
    if min(m["shape"]) == 0: problems.append(key + " shape")
    if any(v > 2 for k,v in m["stems"].items() if k != "CHANGE-SYNONYM"):
        legacy_problems.append(key + " repeated content")
    if any(v > 3 for v in m["stems"].values()):
        problems.append(key + " C3 repeated content")
for trait in "BCGH":
    group = [r for r in new if r["id"].split("-")[1] == trait]
    m = metrics(group); n = m["n"]
    if m["my"] > .12*n or m["icontract"] > .12*n: problems.append(trait + " openers")
    if m["office"] > .12*n: problems.append(trait + " office")
    if m["hedge"] > .15*n: problems.append(trait + " hedge")
    if any(x < .06*n or x > .22*n for x in m["modes"]): problems.append(trait + " modes")
    if any(v > 8 for k,v in m["stems"].items() if k != "CHANGE-SYNONYM"):
        legacy_problems.append(trait + " repeated content")
if len({r["text"] for r in new}) != len(new):
    problems.append("duplicate candidate text")
for r in new:
    if r["decision"] or r["note"]: problems.append(r["id"] + " review cells")
    if r["mode"] not in modes: problems.append(r["id"] + " unknown mode")
    if not r["fit"].startswith(r["id"].split("-")[0] + ":"):
        problems.append(r["id"] + " fit label")
    if r["id"] in name_gates and name_gates[r["id"]] not in r["text"]:
        problems.append(r["id"] + " named eligibility")
    actual = re.findall(r"\{[^}]+\}", r["text"])
    if set(re.findall(r"\{[^}]+\}", r["slots"])) != set(actual):
        problems.append(r["id"] + " slots")
    if r["shape"] == "O" and actual:
        problems.append(r["id"] + " O slot")
    if r["shape"] in ("J", "M") and not actual:
        problems.append(r["id"] + " missing slot")
    if actual:
        first = r["text"].find(actual[0])
        last = r["text"].rfind(actual[-1]) + len(actual[-1])
        if r["shape"] == "J" and first < len(r["text"]) / 2:
            problems.append(r["id"] + " J slot not late")
        if r["shape"] == "M" and (first == 0 or not re.search(r"[a-z]", r["text"][last:], re.I)):
            problems.append(r["id"] + " M slot not internal")
keepers = ("SH-B-03", "SH-B-06", "SH-C-06", "RS-C-03", "RS-C-06", "EL-G-03", "EL-B-03")
old_by_id = {r["id"]: r for r in old}
new_by_id = {r["id"]: r for r in new}
for key in keepers:
    if new_by_id[key] != old_by_id[key]: problems.append(key + " changed keep")
priority_text = after.split("### REVIEW THESE FIRST", 1)[1].split("\n## Sizing", 1)[0]
priority = re.findall(r"\b[A-Z]{2}-[BCGH]-\d{2}\b", priority_text)
if len(priority) != 152 or len(set(priority)) != 152:
    problems.append("priority size or duplicate")
if any(key not in new_by_id for key in priority):
    problems.append("unknown priority ID")
for r in new:
    if re.search(r"\{(?:greed|mood)Direction\}", r["text"]) and not re.search(
        r"(?:Greed|Mood) (?:up|down) only", r["fit"]):
        problems.append(r["id"] + " missing sign gate")
print("C3 FAILURES: " + json.dumps(problems))
print("LEGACY UNRESOLVED SCREENS: " + json.dumps(legacy_problems))
if "--strict" in sys.argv and problems: sys.exit(1)
if "--legacy-strict" in sys.argv and (problems or legacy_problems): sys.exit(1)
```

<details>
<summary>Historical C2 report and checker (superseded; do not use on C3)</summary>

The following is the original 2026-11-13 record, retained for provenance.
Its location-inclusive grounding definition is explicitly rejected by C3.
Its checker is not the current acceptance procedure; use `c3-checker` above.

### Measured before/after (2026-11-13)

Baseline: the initial Part C document at repository revision
`d6ac41df9f3457b7fda1c665b55def1328bb4dc0`. Its saved worktree text matched
that revision after normalizing line endings. The same lexical procedure below
was used for both files; baseline rows had no grounding or shape annotations.

| Measure | Initial Part C | Revised C2 |
|---|---:|---:|
| Candidates / trait-subfamily sets | 352 / 44 | 592 / 96 |
| Actual shapes | 1: fact followed by semicolon/joke, 352 | 3: J 248, M 192, O 152 |
| Sets with all J/M/O shapes | 0/44 | 96/96 |
| Grounded concrete lines | literal-only lower bound 34/352 (9.66%) | 448/592 (75.68%); every set >=60% |
| Specified hedge-word lines | 114/352 (32.39%); 41/44 sets >15% | 0/592; every set 0% |
| False optimism | 66/352 (18.75%); 22/44 sets exceed one/eight | 4/592 (0.68%); every set within `floor(n/8)` |
| Office/record keyword hits | 44/352 (12.50%) | 37/592 (6.25%) |
| Keyword/manual office union | 61/352 (17.33%) | 42/592 (7.09%) |
| Maximum stem lines in any set | 3; four violating sets | 2; zero violating sets |
| Whole-trait stem maxima, B/C/G/H | 12/12/14/12 | 8/8/8/8 |
| Named fit labels / blank decision-note pairs | older separate review layout | 592/592 / 592/592 |
| Duplicate candidate IDs / texts | 0 / 0 | 0 / 0 |

The baseline concrete lower bound excludes its unannotated factual replacement
slots. It is **not** a fully comparable estimate of old event-grounded nouns.
The revised 448 count verifies explicit committed anchors, not generic props.
Lexical thresholds passed; fact-fit, mode and lore checks are author review,
not automatic semantic proof or designer approval. No revised pool was loaded,
so these are draft counts, not improved runtime repetition measurements.

### Recounted actual joke modes

| Mode | Initial Part C | Revised C2 |
|---|---:|---:|
| Gear obsession | 56 | 192 |
| Overconfidence | 55 | 145 |
| False optimism | 66 | 4 |
| Genre tension | 54 | 142 |
| Loot greed | 55 | 104 |
| Bureaucracy | 44 | 5 |
| Former Risk-awareness label | 22 | 0 |
| Total | 352 | 592 |

Technical drawings are screened as office/record content conservatively, but
their Mode describes the joke: fascination with a mechanism is Gear obsession,
not automatically Bureaucracy. The four false-optimism lines are one per PC
trait set, not an allocation to every eight-line pool.

### Per-set numbers

Each cell below is **concrete hits / maximum content-stem lines / office-union
hits**, in a set of N lines. J/M/O counts apply separately to each trait.
Every one of the 96 sets has **zero hedge hits** and **N/N named fit labels**.
False optimism is **1/16 for each PC set**, **zero for every other set**.
Thus the table reports all six requested counters per set without repeating
those identical columns 96 times. B/C/G/H = Brawler/Cautious/Gadgeteer/Hoarder.

| Pool | N per trait | J/M/O per trait | B: concrete/stem/office | C: concrete/stem/office | G: concrete/stem/office | H: concrete/stem/office |
|---|---:|---|---|---|---|---|
| PC | 16 | 6/5/5 | 11/2/0 | 11/2/0 | 11/2/3 | 11/2/1 |
| SH | 8 | 3/3/2 | 8/2/1 | 8/2/0 | 7/2/2 | 7/2/2 |
| UD | 4 | 2/1/1 | 4/2/0 | 3/2/1 | 3/1/3 | 3/2/0 |
| RS | 8 | 3/3/2 | 6/2/1 | 6/2/0 | 6/2/0 | 6/2/1 |
| EL | 8 | 3/3/2 | 6/1/0 | 6/2/0 | 6/2/1 | 6/2/0 |
| ES | 8 | 3/3/2 | 6/1/0 | 6/2/0 | 6/2/1 | 6/2/0 |
| SI | 8 | 3/3/2 | 6/2/0 | 6/2/0 | 6/2/2 | 6/2/0 |
| SN | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/0 | 3/1/0 |
| SX | 4 | 2/1/1 | 3/2/0 | 3/2/1 | 3/2/0 | 3/2/0 |
| AR | 8 | 3/3/2 | 6/2/1 | 6/1/0 | 6/2/3 | 6/2/0 |
| AD | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/0 | 3/2/0 |
| PS | 8 | 3/3/2 | 7/2/0 | 6/2/0 | 6/2/1 | 6/2/0 |
| PF | 8 | 3/3/2 | 6/2/0 | 6/2/0 | 6/2/1 | 6/2/0 |
| HW | 8 | 3/3/2 | 6/2/0 | 6/2/0 | 6/2/2 | 6/2/0 |
| HI | 8 | 3/3/2 | 6/2/0 | 6/2/0 | 6/2/2 | 6/2/1 |
| RD | 4 | 2/1/1 | 3/1/0 | 3/2/0 | 3/2/1 | 3/2/0 |
| RX | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/1/1 | 3/2/0 |
| RC | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/2 | 3/2/0 |
| RV | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/0 | 3/2/0 |
| DN | 4 | 2/1/1 | 3/1/0 | 3/1/0 | 3/2/1 | 3/1/0 |
| KO | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/1 | 3/1/0 |
| DL | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/1 | 3/2/0 |
| HS | 4 | 2/1/1 | 3/1/0 | 3/1/0 | 3/1/2 | 3/2/0 |
| HE | 4 | 2/1/1 | 3/2/0 | 3/2/0 | 3/2/2 | 3/2/0 |

### Human-readable office/record inventory

The conservative union is **42 IDs**, regardless of the Mode column. The
screen deliberately includes diagram/schematic/blueprint/chart/sketch/waveform
and notation imagery, not just administration. Each identifier resolves to
the candidate text in the review tables above.

- PC: PC-G-08, PC-G-11, PC-G-14, PC-H-08.
- SH: SH-B-03, SH-G-05, SH-G-06, SH-H-06, SH-H-08.
- UD: UD-C-02, UD-G-02, UD-G-03, UD-G-04.
- RS: RS-B-06, RS-H-05.
- Extraction/search: EL-G-02, ES-G-02, SI-G-01, SI-G-05, SX-C-03.
- Arrival: AR-B-08, AR-G-01, AR-G-04, AR-G-06.
- Packing: PS-G-05, PF-G-05.
- Conditions: HW-G-02, HW-G-08, HI-G-02, HI-G-05, HI-H-08.
- Robots: RD-G-02, RX-G-02, RC-G-01, RC-G-02.
- Loss: DN-G-02, KO-G-02, DL-G-02.
- Pocket: HS-G-02, HS-G-03, HE-G-02, HE-G-03.

Manual-only additions beyond the keyword screen are SH-B-03 (payment),
SH-H-06 (auction), SH-H-08 (donation), RS-B-06 (discount), and
RS-H-05 (fee). Other manually flagged metaphors overlap the keyword screen.
Reject/edit decisions may remove or change these; rerun the union afterward.

### Historical C2 procedure — replacement required

The following legacy Python is retained to explain the historical C2 counts,
not as a usable C3 checker. It must be replaced with a strict C3 before/after
procedure and executed using the requested execution-subagent when available.
Do not use it to certify this draft.

The first command prints aggregate JSON and a 96-row per-set Markdown report;
`--summary` omits that table. `--strict` fails revised rows on threshold,
slot/shape, mode, duplicate, nonblank reviewer-cell, fit-prefix or listed
contradiction-screen failures. Legacy output is comparative, not a claim that
the baseline passes the revised thresholds.

Counting rules: count **lines containing** a stem/hedge/office term, not token
occurrences; union keyword hits with manually marked Office cells. Strip
replacement slots from stems only, remove the explicit function-word list,
normalize possessives/plurals/verb endings, and apply the listed derivation
and irregular-verb aliases. This is a reproducible lexical approximation:
review unlisted derivations manually rather than treating it as a complete
English lemmatizer. Concrete annotations must name a whitelisted grounded
anchor present in the candidate, not an invented prop. J has a late contextual
slot, M has a slot within the clause and words afterward, O has none.

**Sub-family fit remains a separate author/reviewer procedure:** expand each
candidate against its section's positive and negative committed-fact contract.
Test both sides of compatible shared pools (RC cargo/explicit no cargo; RV
started/completed; AD raid admission/deployment/departure). Reject implied
ownership in shared awards, invented source/gear, delta direction, shield HP
guarantees, forecast-as-impact, robot escape-as-defeat, healing-as-composure,
downed-as-settlement, and empty pocket-as-salvage. Check each named entity against
the linked lore and each label against the actual joke. All 592 candidates
received this author fit/mode review; their designer decision/note cells remain
blank. The automated Fit count verifies labels, **not** semantic truth.

Finally check local Markdown destinations/anchors, search for the undotted
acronym, and run `git diff --check`. No new runtime repeat result is inferred.

```python
import collections
import json
import pathlib
import re
import sys

# Standard-library lexical screening, not a semantic approval or a runtime loader.
path = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else r"docs\DIARY_PILOT_LINES.md")
rows = []
legacy_manual_office = set("""SH-G-08 SH-H-08 UD-C-08 UD-H-08 RE-B-08 RE-C-08
RE-H-08 ED-B-08 ED-C-08 ED-H-08 SR-B-08 AA-B-08 AA-C-08 AA-H-08 PK-G-08
CI-C-08 RA-C-08 RA-G-08 RK-B-08 RK-G-08""".split())
for number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
    cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
    if not cells or not re.fullmatch(r"[A-Z]{2}-[BCGH]-\d{2}", cells[0]):
        continue
    if len(cells) == 6 and cells[1] == "common":
        rows.append(dict(id=cells[0], mode=cells[2], shape="F", slots=cells[3],
                         text=cells[4].strip('"\u201c\u201d'), concrete="",
                         office="yes" if cells[0] in legacy_manual_office else "no",
                         fit="", line=number, legacy=True))
    elif len(cells) == 10:
        rows.append(dict(id=cells[0], mode=cells[1], shape=cells[2], slots=cells[3],
                         text=cells[4].strip('"\u201c\u201d'), concrete=cells[5],
                         office=cells[6], fit=cells[7], decision=cells[8],
                         reviewer=cells[9], line=number, legacy=False))
    elif len(cells) != 3:
        raise ValueError(f"Malformed candidate row at line {number}: {len(cells)} cells")

if not rows:
    raise ValueError("No candidate rows found")

hedge = re.compile(r"\b(?:next|perhaps|may|could|still|remain(?:s|ed|ing)?)\b", re.I)
office = re.compile(
    r"\b(?:office|paperwork|record(?:s|ed|ing)?|checklist|forms?|policy|policies|"
    r"editor|chapter|paragraph|specification|labels?|file[ds]?|filing|invoice|"
    r"receipt|audit(?:s|ed|ing)?|manifest|ledger|stamps?|permits?|committee|reports?|minutes|"
    r"procedure|protocol|bureaucracy|bureaucratic|memo|memos|catalog(?:ue)?|"
    r"department|management|budget|requisition|application|approval|"
    r"account(?:s|ing|ant)?|clerk|contract|documentation|document|"
    r"meeting|agenda|spreadsheet|certificate|signature|warranty|"
    r"pension|payroll|overtime|customer.service|performance.review|"
    r"notes?|noted|notation|diagrams?|schematics?|blueprints?|charts?|"
    r"sketch(?:es|ed|ing)?|waveforms?|asterisks?|full.stop|billing)\b", re.I)
stop = set("""a an the i me my mine myself we us our ours you your yours it its itself
he him his she her hers they them their theirs this that these those there here
am is are was were be been being have has had having do does did doing
and or but if as than then so because when while where who what which
to of in on at by for from with without into through out up down off over under
about against between after before until during above below around across
not no nor never only just very too also even enough how much more most less
one two all any some every each both either neither other another same such
can cannot will would shall should must ought don't doesn't didn't isn't aren't
wasn't weren't won't wouldn't can't couldn't shouldn't wasn't it's that's there's
now already yet again once twice instead together""".split())
aliases = {
    "theories": "theory", "theoretical": "theory", "theoretically": "theory",
    "possibilities": "possibility", "possible": "possibility", "possibly": "possibility",
    "usefulness": "useful", "usefully": "useful", "utility": "useful",
    "testing": "test", "tested": "test", "tests": "test",
    "readings": "reading", "reading": "reading",
    "calibration": "calibrate", "calibrating": "calibrate", "calibrated": "calibrate",
    "calibrates": "calibrate", "calibrate": "calibrate",
    "calculations": "calculate", "calculation": "calculate",
    "calculating": "calculate", "calculated": "calculate",
    "diagnostics": "diagnostic", "diagnosis": "diagnostic",
    "diagrams": "diagram", "models": "model", "notes": "note",
    "variables": "variable", "futures": "future",
    "confident": "confidence", "confidently": "confidence",
    "protect": "protect", "protective": "protect", "protection": "protect",
    "protected": "protect", "protecting": "protect",
    "imaginary": "imagine", "imagined": "imagine", "imagination": "imagine",
    "imagining": "imagine", "imagine": "imagine",
    "optimistic": "optimism", "optimistically": "optimism",
    "greedy": "greed", "greedier": "greed", "greediest": "greed",
    "collector": "collect", "collectors": "collect", "collection": "collect",
    "collections": "collect", "collectible": "collect", "collectibles": "collect",
    "collecting": "collect", "collected": "collect", "collect": "collect",
    "thought": "think", "thoughts": "think", "thinking": "think",
    "hypotheses": "hypothesis",
    "revise": "revise", "revised": "revise", "revision": "revise",
    "revisions": "revise", "revising": "revise",
    "recovery": "recover", "recovering": "recover", "recovered": "recover",
    "gave": "give", "given": "give", "giving": "give",
    "got": "get", "getting": "get", "made": "make", "making": "make",
    "took": "take", "taken": "take", "taking": "take",
    "brought": "bring", "bringing": "bring", "won": "win", "winning": "win",
    "ran": "run", "running": "run", "spent": "spend", "spending": "spend",
    "held": "hold", "holding": "hold", "found": "find", "finding": "find",
    "built": "build", "building": "build",
}
contradictions = {
    "ES": r"\b(?:finished packing|started packing|safely extracted|heading for extraction|final paragraph)\b",
    "SI": r"\b(?:suspicious blank space|redeem the suspense|nothing recovered|empty.hand(?:ed|s))\b",
    "SN": r"\{item\}|\b(?:I recovered|I found|my new loot)\b",
    "HW": r"\b(?:took damage|hit me|already struck|lost HP)\b",
    "RX": r"\b(?:I defeated|we defeated|I won|my victory|we killed)\b",
    "RC": r"\b(?:my reward|I own|all mine)\b",
    "RV": r"\b(?:healed|full health|restored HP)\b",
    "HE": r"\{item\}|\b(?:preserved my loot|salvaged my loot)\b",
}

def stem(word):
    if word.endswith("'s"):
        word = word[:-2]
    if word in aliases:
        return aliases[word]
    if word.endswith("ies") and len(word) > 4:
        return word[:-3] + "y"
    for suffix in ("ing", "ed"):
        if word.endswith(suffix) and len(word) > len(suffix) + 3:
            root = word[:-len(suffix)]
            if len(root) > 2 and root[-1] == root[-2]:
                root = root[:-1]
            return root
    if word.endswith("s") and not word.endswith(("ss", "us", "is")) and len(word) > 3:
        return word[:-1]
    return word

def words(text):
    return re.findall(r"[a-z]+(?:'[a-z]+)?", text.lower())

def stems(row):
    text = re.sub(r"\{[^}]+\}", "", row["text"])
    # Multiword content stems are additionally counted as one phrase.
    result = {stem(w) for w in words(text) if w not in stop}
    if re.search(r"\bdata points?\b", text, re.I):
        result.add("data point")
    return result

def concrete(row):
    if row["legacy"]:
        # Explicit literal matches only; unannotated baseline slots are unknown.
        return bool(re.search(r"\b(?:shield|backpack|armor|straps?|kit|pockets?|"
                              r"crate|locker|chassis|weather|hardware|component|"
                              r"A\.R\.C\.|Secret Hidden Pocket)\b", row["text"], re.I))
    if row["concrete"] in ("-", ""):
        return False
    anchors = [a.strip().strip("`") for a in row["concrete"].split(",")]
    if not all(a and a.lower() in row["text"].lower() for a in anchors):
        raise ValueError(f"{row['id']}: concrete anchor absent from candidate")
    allowed = {"{location}", "{condition}", "{source}", "{item}", "{robot}",
               "shield", "backpack", "secret hidden pocket", "tea kettle", "single sock"}
    if not all(a.lower() in allowed for a in anchors):
        raise ValueError(f"{row['id']}: concrete anchor outside grounded dictionary")
    return True

groups = collections.defaultdict(list)
for row in rows:
    groups[row["id"].rsplit("-", 1)[0]].append(row)
trait_counts = collections.defaultdict(collections.Counter)
for row in rows:
    trait = row["id"].split("-")[1]
    for term in stems(row):
        trait_counts[trait][term] += 1

failures = []
table = []
office_ids = []
for key, group in groups.items():
    n = len(group)
    shapes = collections.Counter(r["shape"] for r in group)
    nouns = sum(concrete(r) for r in group)
    hedges = sum(bool(hedge.search(r["text"])) for r in group)
    office_group = [r["id"] for r in group if office.search(r["text"]) or r["office"] == "yes"]
    office_ids.extend(office_group)
    optimism = sum(r["mode"].lower() == "false optimism" for r in group)
    local = collections.Counter(t for r in group for t in stems(r))
    local_bad = {t: c for t, c in local.items() if c > 2}
    fit_count = sum(r["fit"].startswith(key.split("-")[0] + ":") for r in group)
    table.append(f"| {key} | {n} | {shapes['J']}/{shapes['M']}/{shapes['O']} | "
                 f"{nouns}/{n} | {hedges}/{n} | {max(local.values(), default=0)} | "
                 f"{len(office_group)}/{n} | {optimism}/{n} | {fit_count}/{n} |")
    if not group[0]["legacy"]:
        if not all(shapes[s] for s in ("J", "M", "O")):
            failures.append(f"{key}: missing shape")
        if nouns / n < .6 or hedges / n > .15 or optimism > n // 8:
            failures.append(f"{key}: noun/hedge/optimism threshold")
        if local_bad:
            failures.append(f"{key}: stems {local_bad}")
        if fit_count != n:
            failures.append(f"{key}: fit labels")
        for r in group:
            if r["decision"] or r["reviewer"]:
                failures.append(f"{r['id']}: reviewer cells are not blank")
            actual = re.findall(r"\{[^}]+\}", r["text"])
            declared = re.findall(r"\{[^}]+\}", r["slots"])
            if set(actual) != set(declared):
                failures.append(f"{r['id']}: slots differ")
            denied = contradictions.get(key.split("-")[0])
            if denied and re.search(denied, r["text"], re.I):
                failures.append(f"{r['id']}: sub-family contradiction screen")
            if r["shape"] == "O" and actual:
                failures.append(f"{r['id']}: O has a slot")
            if r["shape"] in ("J", "M") and not actual:
                failures.append(f"{r['id']}: contextual shape has no slot")
            if actual:
                first = r["text"].find(actual[0])
                last = r["text"].rfind(actual[-1]) + len(actual[-1])
                if r["shape"] == "J" and first < len(r["text"]) / 2:
                    failures.append(f"{r['id']}: J anchor is not late")
                if r["shape"] == "M" and (first == 0 or not words(r["text"][last:])):
                    failures.append(f"{r['id']}: M anchor is not mid-clause")
            if r["mode"] not in ("Gear obsession", "Overconfidence", "False optimism",
                                 "Genre tension", "Loot greed", "Bureaucracy"):
                failures.append(f"{r['id']}: invalid mode")

trait_bad = {t: {s: c for s, c in count.items() if c > 8}
             for t, count in trait_counts.items()}
if rows and not rows[0]["legacy"]:
    for t, violations in trait_bad.items():
        if violations:
            failures.append(f"{t}: whole-trait stems {violations}")
    if len(office_ids) / len(rows) > .12:
        failures.append("Office share exceeds 12%")
    if len({r["id"] for r in rows}) != len(rows):
        failures.append("Duplicate IDs")
    if len({r["text"] for r in rows}) != len(rows):
        failures.append("Duplicate candidates")
summary = dict(rows=len(rows), sets=len(groups),
               shapes=dict(collections.Counter(r["shape"] for r in rows)),
               concrete=sum(concrete(r) for r in rows),
               hedges=sum(bool(hedge.search(r["text"])) for r in rows),
               office=len(office_ids), office_ids=office_ids,
               office_keyword=sum(bool(office.search(r["text"])) for r in rows),
               modes=dict(collections.Counter(r["mode"] for r in rows)),
               trait_stem_max={t: max(c.values(), default=0) for t, c in trait_counts.items()},
               local_stem_max=max(max(collections.Counter(
                   t for r in g for t in stems(r)).values(), default=0) for g in groups.values()),
               local_stem_violation_sets=sum(any(c > 2 for c in collections.Counter(
                   t for r in g for t in stems(r)).values()) for g in groups.values()),
               trait_stem_violations=trait_bad, failures=failures)
print(json.dumps(summary, indent=2))
if "--summary" not in sys.argv:
    print("| Set | N | J/M/O | Concrete | Hedge | Max stem/set | Office | False optimism | Fit labels |")
    print("|---|---:|---|---|---|---:|---|---|---|")
    print("\n".join(table))
if failures and "--strict" in sys.argv:
    sys.exit(1)
```

</details>
