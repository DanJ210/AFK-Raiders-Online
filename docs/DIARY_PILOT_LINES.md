# DIARY VARIETY: candidate asides for human review

**Revised draft only: 592 candidates in 24 subfamilies; human review pending.**
No candidate is approved, catalogued, or loaded.

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
all three shapes and at least four lines. Concrete anchors identify committed
slots, guaranteed event objects, or exact canon comparisons actually used by
the candidate. Ordinary metaphorical props do not count as grounding. Decisions and reviewer
notes remain blank for human review.

The six actual modes are Gear obsession, Overconfidence, False optimism, Genre
tension, Loot greed, and Bureaucracy. Cautious practical survival understatement
must use the mode actually expressed, not a seventh “Risk-awareness” mode.
Targets for every trait/subfamily: at least 60% concrete-noun lines, no more
than `floor(n/8)` False optimism lines, and at most two lines using the same
content stem. Across each trait, at most eight lines use the same content stem.
For the stem count only, exclude committed replacement slots; count all other
content words, including canon anchors and gag words.
Per set and across the draft, at most 15% of lines may contain the whole words `next`,
`perhaps`, `may`, `could`, `still`, or `remain`, including their inflections.
Office content is at most 12% both by keyword and manual metaphor review,
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
seven-line event-fit subset of the existing exit pool; ActivityInterrupted,
153–156 / 0.74–0.75%, remains on trait/general fallback. Travel, ItemAllocated,
RobotRound and SharedExtractionCalled had zero previous-20/50 repeats in the
24h audit. ActivityInterrupted is a disproportionate remaining repeat driver:
in Cautious/Hoarder/Brawler/Gadgeteer order, previous-20 repeats were
1/155, 57/153, 67/155, 64/156; previous-50 were 90/155, 153/153, 155/155,
156/156 (58.06% for Cautious, 100% for the other three). These area/activity
pools and fallthroughs are not covered by this pilot. No new gameplay audit or
post-draft repeat measurement is implied.

## PC — PersonalityChanged

**Required:** kind, committed reason and location. The reason is already
presented, and the original receipt retains numeric details. There are no
structured old/new personality fields to reconstruct here. Use `{location}`
only as the committed context; do not infer a cause or direction.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| PC-B-01 | Overconfidence | J | `{location}` | "My temper found a fresh wrestling hold for {location}." | `{location}` | no | PC: changed perspective, no delta direction or fight. |  |  |
| PC-B-02 | Genre tension | M | `{location}` | "Today's revised bravado gives {location} a theatrical pause." | `{location}` | no | PC: internal response, no cause or actor inferred. |  |  |
| PC-B-03 | Gear obsession | O | - | "My changing temper fits like a helmet two sizes too proud." | - | no | PC: comparison, no equipment claimed. |  |  |
| PC-B-04 | Overconfidence | J | `{location}` | "My recalibrated nerve refuses to lose a staring contest with {location}." | `{location}` | no | PC: figurative challenge, no delta direction. |  |  |
| PC-B-05 | Genre tension | M | `{location}` | "My revised thoughts at {location} enter shoulder first." | `{location}` | no | PC: internal revision, not a physical encounter. |  |  |
| PC-B-06 | Loot greed | O | - | "My emotional shuffle has elbow room for trophies, not modesty." | - | no | PC: opinion, no acquisition or greed increase claimed. |  |  |
| PC-B-07 | Overconfidence | J | `{location}` | "My adjusted attitude can fill the whole of {location}." | `{location}` | no | PC: figurative attitude, no direction or gear claim. |  |  |
| PC-B-08 | Genre tension | M | `{location}` | "My inner announcer has rewritten {location}'s introduction in a less flattering key." | `{location}` | no | PC: internal narration only. |  |  |
| PC-B-09 | Gear obsession | O | - | "My shifting resolve has the aerodynamics of a thrown brick." | - | no | PC: comparison, no thrown object claimed. |  |  |
| PC-B-10 | Overconfidence | J | `{location}` | "My adjusted expression gives no consolation prize to {location}." | `{location}` | no | PC: figurative expression, no award or delta direction. |  |  |
| PC-B-11 | Genre tension | M | `{location}` | "My nerves face {location} with fresh terms, mostly glaring." | `{location}` | no | PC: internal stance, not an actual opponent or agreement. |  |  |
| PC-B-12 | False optimism | O | - | "This new outlook surely merits heroic music." | - | no | PC: expectation, not a promised outcome. |  |  |
| PC-B-13 | Overconfidence | J | `{location}` | "My pride has rearranged its flagpole at {location}." | `{location}` | no | PC: figurative revision, no physical act. |  |  |
| PC-B-14 | Genre tension | M | `{location}` | "Even {location} gets a front-row seat to my reshuffled stubbornness." | `{location}` | no | PC: internal revision, figurative audience only. |  |  |
| PC-B-15 | Gear obsession | O | - | "My new outlook buckles less readily than a cheap strap." | - | no | PC: comparison, no equipment state. |  |  |
| PC-B-16 | Overconfidence | J | `{location}` | "My shifting self-respect insists on a dramatic close-up at {location}." | `{location}` | no | PC: subjective revision, no cause or delta direction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-C-01 | Genre tension | J | `{location}` | "My revised composure travels with fewer brass instruments through {location}." | `{location}` | no | PC: internal revision, not travel or equipment. |  |  |
| PC-C-02 | Gear obsession | M | `{location}` | "At {location}, my altered priorities rate a buckle above a flourish." | `{location}` | no | PC: comparison, not carried gear or a numeric direction. |  |  |
| PC-C-03 | Genre tension | O | - | "My shifting outlook prefers a handrail to an ovation." | - | no | PC: comparison, no handrail asserted. |  |  |
| PC-C-04 | Overconfidence | J | `{location}` | "My updated judgment keeps its victory speech away from {location}." | `{location}` | no | PC: internal revision, no victory or terrain implied. |  |  |
| PC-C-05 | Genre tension | M | `{location}` | "My adjusted nerves give {location} their indoor voice." | `{location}` | no | PC: subjective stance, no danger or delta direction. |  |  |
| PC-C-06 | Gear obsession | O | - | "My new perspective needs fewer flourishes and sturdier knots." | - | no | PC: metaphor, no inventory claim. |  |  |
| PC-C-07 | Overconfidence | J | `{location}` | "Recalibrated poise is lighter than bravado on the way through {location}." | `{location}` | no | PC: internal comparison, no travel started. |  |  |
| PC-C-08 | Genre tension | M | `{location}` | "My rearranged thoughts at {location} have agreed to walk, not sprint." | `{location}` | no | PC: internal revision, no movement asserted. |  |  |
| PC-C-09 | Loot greed | O | - | "My reweighted appetite treats loot and exits like an unbalanced seesaw." | - | no | PC: figurative perspective, no item or decision. |  |  |
| PC-C-10 | Gear obsession | J | `{location}` | "I'd rather trust a sound knot than these reshuffled feelings at {location}." | `{location}` | no | PC: comparison, no gear claim or delta direction. |  |  |
| PC-C-11 | Genre tension | M | `{location}` | "At {location}, my refitted instincts favor quiet shoes over a trumpet." | `{location}` | no | PC: internal comparison, no equipment or direction inferred. |  |  |
| PC-C-12 | False optimism | O | - | "This adjusted outlook might be the sensible one." | - | no | PC: hope, no mechanical effect. |  |  |
| PC-C-13 | Overconfidence | J | `{location}` | "I can carry this new disposition past {location} without a parade." | `{location}` | no | PC: self-assessment, not committed movement or a delta. |  |  |
| PC-C-14 | Genre tension | M | `{location}` | "My caution at {location} has changed shoes, not marching bands." | `{location}` | no | PC: figurative revision, no equipment or threat inferred. |  |  |
| PC-C-15 | Gear obsession | O | - | "My revised instincts prefer a first-aid tin to a trumpet." | - | no | PC: comparison, no item or injury. |  |  |
| PC-C-16 | Genre tension | J | `{location}` | "My shuffled restraint would rather whisper than salute at {location}." | `{location}` | no | PC: internal revision, no outcome or numeric direction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-G-01 | Gear obsession | J | `{location}` | "My retuned thoughts need a smaller wrench at {location}." | `{location}` | no | PC: internal revision, figurative tool, not inventory. |  |  |
| PC-G-02 | Gear obsession | M | `{location}` | "At {location}, my altered brain runs on mismatched cogs." | `{location}` | no | PC: internal revision, no actual mechanism or delta direction. |  |  |
| PC-G-03 | Gear obsession | O | - | "My revised instincts have all the elegance of a bent hinge." | - | no | PC: comparison, no damaged gear. |  |  |
| PC-G-04 | Gear obsession | J | `{location}` | "My self-control now has a different gear ratio at {location}." | `{location}` | no | PC: figurative internal adjustment, no equipment or stat bonus. |  |  |
| PC-G-05 | Gear obsession | M | `{location}` | "My altered outlook solders {location} to an interesting theory." | `{location}` | no | PC: metaphor, no repair. |  |  |
| PC-G-06 | Loot greed | O | - | "My appetite's latest revision treats spare components as emotional support." | - | no | PC: subjective perspective, no acquisition or increased greed. |  |  |
| PC-G-07 | Gear obsession | J | `{location}` | "My rebuilt reasoning is held together with rivets at {location}." | `{location}` | no | PC: internal revision, metaphorical hardware only. |  |  |
| PC-G-08 | Gear obsession | M | `{location}` | "My inquisitiveness about {location} has swapped dials without improving the labels." | `{location}` | yes | PC: mental controls, no physical label or equipment change. |  |  |
| PC-G-09 | Overconfidence | O | - | "I can troubleshoot these reshuffled urges without a multimeter." | - | no | PC: internal revision, no owned tool or mood-only delta inferred. |  |  |
| PC-G-10 | Gear obsession | J | `{location}` | "My reweighted beliefs need grease, not applause, at {location}." | `{location}` | no | PC: metaphorical parts, no repair. |  |  |
| PC-G-11 | Gear obsession | M | `{location}` | "The diagram of {location} in my head has acquired fresh arrows." | `{location}` | no | PC: revised mental image, no new surveyed features. |  |  |
| PC-G-12 | False optimism | O | - | "Surely this latest outlook is the version that behaves." | - | no | PC: hope, no future outcome. |  |  |
| PC-G-13 | Gear obsession | J | `{location}` | "My reset judgment calls a loose screw an artistic choice at {location}." | `{location}` | no | PC: internal revision, no equipment state. |  |  |
| PC-G-14 | Gear obsession | M | `{location}` | "My rewired intuition gives {location} a circuit diagram with no warranty." | `{location}` | yes | PC: mental image and warranty metaphor, no finding or real contract. |  |  |
| PC-G-15 | Gear obsession | O | - | "My changing intuition wobbles like an unbalanced flywheel." | - | no | PC: comparison, no machine asserted. |  |  |
| PC-G-16 | Gear obsession | J | `{location}` | "My changing calibration requires fewer hammers at {location}." | `{location}` | no | PC: figurative calibration. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-H-01 | Loot greed | J | `{location}` | "My rearranged priorities demand a display shelf at {location}." | `{location}` | no | PC: internal revision, figurative shelf, no possession. |  |  |
| PC-H-02 | Loot greed | M | `{location}` | "My reshuffled thoughts at {location} are priced like rare antiques." | `{location}` | no | PC: internal revision, figurative value, no real appraisal. |  |  |
| PC-H-03 | Loot greed | O | - | "My altered priorities price a bent spoon above restraint." | - | no | PC: figurative valuation, no acquisition. |  |  |
| PC-H-04 | Overconfidence | J | `{location}` | "My revised judgment is a collectible, particularly at {location}." | `{location}` | no | PC: internal revision, figurative value only. |  |  |
| PC-H-05 | Gear obsession | M | `{location}` | "At {location}, my new outlook declares every pouch essential." | `{location}` | no | PC: no pouch or inventory claim. |  |  |
| PC-H-06 | Loot greed | O | - | "My appetite's latest adjustment rates a bottle cap above restraint." | - | no | PC: opinion only, no award or increased greed asserted. |  |  |
| PC-H-07 | Loot greed | J | `{location}` | "My altered tastes have the resale appeal of {location}." | `{location}` | no | PC: comparison only, no valuation. |  |  |
| PC-H-08 | Bureaucracy | M | `{location}` | "My changed outlook grants {location} pride of place in my catalogue." | `{location}` | yes | PC: figurative catalogue, not ownership. |  |  |
| PC-H-09 | Gear obsession | O | - | "My revised impulses need a backpack with a special compartment." | - | no | PC: comparison, no capacity claim. |  |  |
| PC-H-10 | Overconfidence | J | `{location}` | "My retuned discernment turns even an opinion into treasure at {location}." | `{location}` | no | PC: internal revision, no loot produced. |  |  |
| PC-H-11 | Loot greed | M | `{location}` | "At {location}, my adjusted inclinations find sentimental value in a washer." | `{location}` | no | PC: subjective comparison, no mood-only delta or real value claimed. |  |  |
| PC-H-12 | False optimism | O | - | "This updated appetite surely has excellent taste." | - | no | PC: hope, no future result. |  |  |
| PC-H-13 | Gear obsession | J | `{location}` | "My changing urges have the storage habits of {location}." | `{location}` | no | PC: comparison only, no stored goods. |  |  |
| PC-H-14 | Loot greed | M | `{location}` | "I give {location} a fresh appraisal from my reshuffled point of view." | `{location}` | no | PC: revised perspective, no actual appraisal. |  |  |
| PC-H-15 | Gear obsession | O | - | "My shifting desires treat a spare zipper like medicine." | - | no | PC: comparison, no healing or item. |  |  |
| PC-H-16 | Loot greed | J | `{location}` | "My rearranged sensibilities treat doubts as souvenirs from {location}." | `{location}` | no | PC: mental revision, no decision or acquisition asserted. |  |  |

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
| SH-B-02 | Overconfidence | M | `{location}` | "At {location}, my protection has joined the argument." | `{location}` | no | SH: figurative argument, no attacker inferred. |  |  |
| SH-B-03 | Genre tension | O | - | "My shield paid the cover charge for that entrance." | shield | yes | SH: figurative payment; no remaining balance asserted. |  |  |
| SH-B-04 | Gear obsession | J | `{location}` | "That defensive expense met a louder opinion at {location}." | `{location}` | no | SH: no shield balance. |  |  |
| SH-B-05 | Overconfidence | M | `{location}` | "At {location}, my barrier insists on arguing first." | `{location}` | no | SH: figurative shield, no source asserted. |  |  |
| SH-B-06 | Genre tension | O | - | "That shield charge went out swinging." | shield | no | SH: absorption only, no additional hit. |  |  |
| SH-B-07 | Gear obsession | J | `{location}` | "My protective layer earns a nod for grit at {location}." | `{location}` | no | SH: no equipment beyond shield implied. |  |  |
| SH-B-08 | Overconfidence | M | `{location}` | "At {location}, that defensive discharge can take a bow." | `{location}` | no | SH: charge loss, no complete prevention claimed. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-C-01 | Gear obsession | J | `{location}` | "A barrier is useful, not a promise, at {location}." | `{location}` | no | SH: no balance or immunity claim. |  |  |
| SH-C-02 | Genre tension | M | `{location}` | "At {location}, that expenditure made caution less theoretical." | `{location}` | no | SH: no source inferred. |  |  |
| SH-C-03 | Gear obsession | O | - | "A shield spending charge is not an invitation to get careless." | shield | no | SH: no remaining charge inferred. |  |  |
| SH-C-04 | Overconfidence | J | `{location}` | "I can respect that energy expense without panicking at {location}." | `{location}` | no | SH: no numeric margin asserted. |  |  |
| SH-C-05 | Genre tension | M | `{location}` | "The barrier at {location} offered a pointed reminder about limits." | `{location}` | no | SH: no origin or damage outcome. |  |  |
| SH-C-06 | Gear obsession | O | - | "Even a good shield is an umbrella, not a roof." | shield | no | SH: comparison, no immunity claimed. |  |  |
| SH-C-07 | Loot greed | J | `{location}` | "No trinket is worth forgetting that protection at {location}." | `{location}` | no | SH: no acquired item or decision. |  |  |
| SH-C-08 | Genre tension | M | `{location}` | "The charge spent at {location} bought a moment of caution." | `{location}` | no | SH: figurative purchase, no future event. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-G-01 | Gear obsession | J | `{location}` | "That barrier spent energy with impeccable timing at {location}." | `{location}` | no | SH: no mechanism or balance inferred. |  |  |
| SH-G-02 | Gear obsession | M | `{location}` | "At {location}, the defensive discharge contributed to my field test." | `{location}` | no | SH: metaphorical experiment, no source. |  |  |
| SH-G-03 | Gear obsession | O | - | "A shield converts charge into a very persuasive argument." | shield | no | SH: no charge remaining or HP result. |  |  |
| SH-G-04 | Overconfidence | J | `{location}` | "I approve that burst of protective expenditure at {location}." | `{location}` | no | SH: no test or remaining charge. |  |  |
| SH-G-05 | Gear obsession | M | `{location}` | "The barrier at {location} earned its own circuit sketch." | `{location}` | no | SH: figurative drawing, no added equipment. |  |  |
| SH-G-06 | Gear obsession | O | - | "That spent charge deserves a waveform with exclamation marks." | - | no | SH: metaphor, no voltage measurement. |  |  |
| SH-G-07 | Loot greed | J | `{location}` | "I'd frame that energy cost as a fine specimen at {location}." | `{location}` | no | SH: no remaining charge or duration inferred. |  |  |
| SH-G-08 | Gear obsession | M | `{location}` | "At {location}, the power expense made an electrical argument." | `{location}` | no | SH: positive charge loss, no balance claim. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-H-01 | Loot greed | J | `{location}` | "I'd hoard every spent spark if allowed at {location}." | `{location}` | no | SH: figurative hoarding; no balance claim. |  |  |
| SH-H-02 | Gear obsession | M | `{location}` | "At {location}, that barrier reminds me why protection is precious." | `{location}` | no | SH: no additional layers or charge claimed. |  |  |
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
| UD-B-04 | Overconfidence | J | `{source}` | "I object to that unshielded greeting from {source}." | `{source}` | no | UD: committed source; no injury inferred. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-C-01 | Genre tension | J | `{source}` | "The unshielded route is less reassuring with {source}." | `{source}` | no | UD: committed source; no HP loss assumed. |  |  |
| UD-C-02 | Bureaucracy | M | `{source}` | "The shield's absence from the equation involving {source} is noted." | `{source}` | yes | UD: record metaphor, no remaining charge or injury claimed. |  |  |
| UD-C-03 | Genre tension | O | - | "Unshielded damage is an inconvenient way to test caution." | - | no | UD: raw damage path only; no HP amount. |  |  |
| UD-C-04 | Overconfidence | J | `{source}` | "I won't mistake an uncovered hit for a warning from {source}." | `{source}` | no | UD: source exact; no wound or forecast. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-G-01 | Gear obsession | J | `{source}` | "The shield bypass has an unwelcome input: {source}." | `{source}` | no | UD: exact source; no HP loss or charge level. |  |  |
| UD-G-02 | Gear obsession | M | `{source}` | "My diagram routes {source} straight past the barrier." | `{source}` | no | UD: figurative diagram; raw damage can be reduced to zero. |  |  |
| UD-G-03 | Gear obsession | O | - | "Unshielded damage is a remarkably rude circuit sketch." | - | no | UD: no equipment failure or HP claim. |  |  |
| UD-G-04 | Overconfidence | J | `{source}` | "I can chart the missing protective step for {source}." | `{source}` | no | UD: no actual test or HP loss. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| UD-H-01 | Loot greed | J | `{source}` | "I'd trade that unshielded attention for a souvenir from {source}." | `{source}` | no | UD: source only; no transaction or wound. |  |  |
| UD-H-02 | Genre tension | M | `{source}` | "Even {source} shouldn't get free passage past a shield." | `{source}`, shield | no | UD: unshielded path only; no HP outcome. |  |  |
| UD-H-03 | Gear obsession | O | - | "An unshielded hit makes every scrap of padding look priceless." | - | no | UD: no padding possessed or injury assumed. |  |  |
| UD-H-04 | Overconfidence | J | `{source}` | "My greed refuses to admire that uncovered visit from {source}." | `{source}` | no | UD: source only; no outcome inferred. |  |  |

## RS — Resilience

**Required:** kind and committed source for J/M candidates. `{source}` must
match the original committed text/thread exactly. The reduction does not
guarantee a wound, healing, or survival.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| RS-B-01 | Overconfidence | J | `{source}` | "My stubbornness shaved the edge off {source}." | `{source}` | no | RS: committed source; no HP result. |  |  |
| RS-B-02 | Genre tension | M | `{source}` | "I gave {source} less room to make its point." | `{source}` | no | RS: mitigation only, no survival claim. |  |  |
| RS-B-03 | Gear obsession | O | - | "My grit makes damage take the scenic route." | - | no | RS: reduction, not complete prevention. |  |  |
| RS-B-04 | Overconfidence | J | `{source}` | "I refuse to give the full satisfaction to {source}." | `{source}` | no | RS: exact source; no opponent inferred. |  |  |
| RS-B-05 | Genre tension | M | `{source}` | "The worst of {source} got an argument from my resolve." | `{source}` | no | RS: mitigation only; no zero-damage claim. |  |  |
| RS-B-06 | Loot greed | O | - | "My backbone just negotiated a discount on the damage." | - | yes | RS: figurative discount, no health or trade. |  |  |
| RS-B-07 | Overconfidence | J | `{source}` | "My grit took the sharpness out of {source}." | `{source}` | no | RS: reduction only, no full protection. |  |  |
| RS-B-08 | Genre tension | M | `{source}` | "My resolve denied {source} an enthusiastic reception." | `{source}` | no | RS: source exact; no later outcome. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-C-01 | Genre tension | J | `{source}` | "Less damage is a modest courtesy from {source}." | `{source}` | no | RS: source exact; no healing or zero damage. |  |  |
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
| RS-G-04 | Overconfidence | J | `{source}` | "I call that reduced input a design improvement over {source}." | `{source}` | no | RS: figurative, no invention or test. |  |  |
| RS-G-05 | Gear obsession | M | `{source}` | "My suspension of disbelief softened {source} a little." | `{source}` | no | RS: figurative mechanism, no actual repair. |  |  |
| RS-G-06 | Gear obsession | O | - | "The damage curve just met a stubborn bend." | - | no | RS: reduction only; no measurement. |  |  |
| RS-G-07 | Gear obsession | J | `{source}` | "My improvised resilience adds a little damping to {source}." | `{source}` | no | RS: metaphor, no item held. |  |  |
| RS-G-08 | Overconfidence | M | `{source}` | "Against {source}, my tolerance for impact has engineering ambitions." | `{source}` | no | RS: reduction only, no outcome. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-H-01 | Loot greed | J | `{source}` | "That saved sliver of damage deserves a velvet box: {source}." | `{source}` | no | RS: reduction only; no purchase or item. |  |  |
| RS-H-02 | Genre tension | M | `{source}` | "My resilience made {source} less costly than advertised." | `{source}` | no | RS: figurative cost; no acquired item. |  |  |
| RS-H-03 | Gear obsession | O | - | "Reduced damage is worth more than a suitcase full of compliments." | - | no | RS: opinion, no physical case or healing. |  |  |
| RS-H-04 | Loot greed | J | `{source}` | "I'll treasure the little bit spared by my grit against {source}." | `{source}` | no | RS: reduction only, no full prevention. |  |  |
| RS-H-05 | Overconfidence | M | `{source}` | "My toughness charges {source} an outrageous entrance fee." | `{source}` | yes | RS: figurative fee, no transaction. |  |  |
| RS-H-06 | Loot greed | O | - | "Every fraction of hurt shaved off belongs among the family jewels." | - | no | RS: no actual valuables or amount claimed. |  |  |
| RS-H-07 | Genre tension | J | `{source}` | "I prize a smaller share of the unpleasantness from {source}." | `{source}` | no | RS: no health assertion. |  |  |
| RS-H-08 | Gear obsession | M | `{source}` | "My resolve wraps {source} in bargain padding before it lands." | `{source}` | no | RS: figurative mitigation, no equipment. |  |  |

## EL — ExtractionDecision, heading

**Required:** kind, committed heading decision, reason and location. The reason
and direction are already presented; neither arrival, boarding nor extraction
success follows from the decision.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| EL-B-01 | Overconfidence | J | `{location}` | "Retreat has never looked this confident from {location}." | `{location}` | no | EL: heading only, no arrival or success. |  |  |
| EL-B-02 | Genre tension | M | `{location}` | "At {location}, I call this the heroic exit cue." | `{location}` | no | EL: decision, not completed extraction. |  |  |
| EL-B-03 | Gear obsession | O | - | "My boots know a dramatic direction when they hear one." | - | no | EL: figurative boots, no arrival. |  |  |
| EL-B-04 | Overconfidence | J | `{location}` | "I plan to leave the scenery undefeated by my expression at {location}." | `{location}` | no | EL: heading decision, no completed journey. |  |  |
| EL-B-05 | Genre tension | M | `{location}` | "My farewell to {location} deserves a drumroll." | `{location}` | no | EL: figurative farewell, no physical departure. |  |  |
| EL-B-06 | Loot greed | O | - | "Even a trophy needs a route home." | - | no | EL: comparison, no cargo claim. |  |  |
| EL-B-07 | Overconfidence | J | `{location}` | "Even my withdrawal can stare down {location}." | `{location}` | no | EL: no fight or arrival inferred. |  |  |
| EL-B-08 | Genre tension | M | `{location}` | "The curtain over {location} can wait for my bow." | `{location}` | no | EL: figurative curtain; no extraction success. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-C-01 | Genre tension | J | `{location}` | "A sensible direction is a comfort at {location}." | `{location}` | no | EL: direction only, no successful exit. |  |  |
| EL-C-02 | Gear obsession | M | `{location}` | "At {location}, my internal compass points toward discretion." | `{location}` | no | EL: figurative compass, no navigation result. |  |  |
| EL-C-03 | Genre tension | O | - | "Leaving is the part of the plan that requires actual feet." | - | no | EL: no arrival or movement completed. |  |  |
| EL-C-04 | Overconfidence | J | `{location}` | "I can be brave somewhere other than {location}." | `{location}` | no | EL: no completed departure. |  |  |
| EL-C-05 | Genre tension | M | `{location}` | "The view of {location} can improve without my company." | `{location}` | no | EL: intention, no arrival. |  |  |
| EL-C-06 | Gear obsession | O | - | "A map is most persuasive when it points toward home." | - | no | EL: comparison, no route safety implied. |  |  |
| EL-C-07 | Loot greed | J | `{location}` | "My appetite will have to argue with the exit from {location}." | `{location}` | no | EL: no item or abandoned loot. |  |  |
| EL-C-08 | Genre tension | M | `{location}` | "From {location}, the exit sounds like a sensible sentence." | `{location}` | no | EL: heading only, not success. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-G-01 | Gear obsession | J | `{location}` | "My departure calculus has finally found the off switch for {location}." | `{location}` | no | EL: mental controls, no real device or arrival. |  |  |
| EL-G-02 | Gear obsession | M | `{location}` | "At {location}, my blueprint now includes an arrow out." | `{location}` | no | EL: thought only; no route guarantee. |  |  |
| EL-G-03 | Gear obsession | O | - | "Even a prototype needs an off switch." | - | no | EL: analogy, no equipment or shutdown. |  |  |
| EL-G-04 | Overconfidence | J | `{location}` | "I've solved the direction problem, starting with {location}." | `{location}` | no | EL: direction decision, not successful travel. |  |  |
| EL-G-05 | Gear obsession | M | `{location}` | "My exit vector from {location} has mathematical elegance." | `{location}` | no | EL: opinion, no safe route asserted. |  |  |
| EL-G-06 | Gear obsession | O | - | "A well-designed lever also knows when to release." | - | no | EL: analogy, no action. |  |  |
| EL-G-07 | Loot greed | J | `{location}` | "My parts obsession can argue with the exit from {location}." | `{location}` | no | EL: no loot left behind. |  |  |
| EL-G-08 | Overconfidence | M | `{location}` | "From {location}, I can at least calculate which way is out." | `{location}` | no | EL: heading decision only, no completed journey. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-H-01 | Loot greed | J | `{location}` | "Leaving is an expensive feeling, especially at {location}." | `{location}` | no | EL: feeling only, no loss claimed. |  |  |
| EL-H-02 | Genre tension | M | `{location}` | "At {location}, my longing is writing a tragic farewell to opportunity." | `{location}` | no | EL: no items or abandonment asserted. |  |  |
| EL-H-03 | Gear obsession | O | - | "My pockets object to the very idea of departure." | - | no | EL: figurative, no inventory claim. |  |  |
| EL-H-04 | Loot greed | J | `{location}` | "My souvenir urges are negotiating with departure from {location}." | `{location}` | no | EL: figurative negotiation; no item claimed. |  |  |
| EL-H-05 | Genre tension | M | `{location}` | "My curiosity about {location} will miss having an audience." | `{location}` | no | EL: no discovery or arrival. |  |  |
| EL-H-06 | Overconfidence | O | - | "I can point toward extraction without applauding restraint." | - | no | EL: heading only, no remaining item asserted. |  |  |
| EL-H-07 | Loot greed | J | `{location}` | "My appetite for keepsakes disagrees politely with leaving {location}." | `{location}` | no | EL: no keepsake or successful departure. |  |  |
| EL-H-08 | Gear obsession | M | `{location}` | "At {location}, my hoarding instinct protests the exit arrow." | `{location}` | no | EL: metaphor; no actual contents. |  |  |

## ES — ExtractionDecision, staying

**Required:** kind, committed staying decision, reason and location. Staying
means now, not a permanent refusal to extract or a promise of future loot.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| ES-B-01 | Overconfidence | J | `{location}` | "My boots aren't done expressing themselves at {location}." | `{location}` | no | ES: staying now, no later result. |  |  |
| ES-B-02 | Genre tension | M | `{location}` | "At {location}, the curtain can wait for my better angle." | `{location}` | no | ES: figurative curtain, no threat. |  |  |
| ES-B-03 | Overconfidence | O | - | "An early bow would spoil my dramatic posture." | - | no | ES: no departure or outcome claimed. |  |  |
| ES-B-04 | Gear obsession | J | `{location}` | "My helmet of stubbornness votes to stay at {location}." | `{location}` | no | ES: metaphor, no future event. |  |  |
| ES-B-05 | Genre tension | M | `{location}` | "I have unfinished staring at {location} to do." | `{location}` | no | ES: expression only, no opponent. |  |  |
| ES-B-06 | Loot greed | O | - | "The prospect of loot can make an exit feel premature." | - | no | ES: no acquisition or future discovery. |  |  |
| ES-B-07 | Overconfidence | J | `{location}` | "I decline to be hurried by the concept of {location}." | `{location}` | no | ES: no external pressure or encounter. |  |  |
| ES-B-08 | Genre tension | M | `{location}` | "The scenery at {location} hasn't heard my closing argument." | `{location}` | no | ES: figurative, no actual argument. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-C-01 | Genre tension | J | `{location}` | "A pause is not a lifetime commitment to {location}." | `{location}` | no | ES: staying now only. |  |  |
| ES-C-02 | Gear obsession | M | `{location}` | "At {location}, my internal compass isn't a stopwatch." | `{location}` | no | ES: figurative instruments, no deadline invented. |  |  |
| ES-C-03 | Overconfidence | O | - | "I can wait without confusing patience for armor." | - | no | ES: comparison, no equipment asserted. |  |  |
| ES-C-04 | Genre tension | J | `{location}` | "I prefer a deliberate breath here at {location}." | `{location}` | no | ES: no healing or permanent stay. |  |  |
| ES-C-05 | Gear obsession | M | `{location}` | "My map of {location} has room for a pause." | `{location}` | no | ES: figurative map, no route or discovery. |  |  |
| ES-C-06 | Loot greed | O | - | "A trinket is not a reason to forget the way out." | - | no | ES: comparison, no acquisition. |  |  |
| ES-C-07 | Overconfidence | J | `{location}` | "I can keep perspective without rushing away from {location}." | `{location}` | no | ES: no future decision inferred. |  |  |
| ES-C-08 | Genre tension | M | `{location}` | "At {location}, a little patience has volunteered for duty." | `{location}` | no | ES: figurative attitude only. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-G-01 | Gear obsession | J | `{location}` | "My departure plan has a pause button at {location}." | `{location}` | no | ES: figurative timer, no actual deadline change. |  |  |
| ES-G-02 | Gear obsession | M | `{location}` | "At {location}, the diagram has acquired a thoughtful hesitation." | `{location}` | no | ES: figurative diagram, no future outcome. |  |  |
| ES-G-03 | Gear obsession | O | - | "The exit switch can wait while I reconsider its wiring." | - | no | ES: figurative, no action or change. |  |  |
| ES-G-04 | Overconfidence | J | `{location}` | "I can optimize this delay without a wrench at {location}." | `{location}` | no | ES: no physical repair or timing promise. |  |  |
| ES-G-05 | Gear obsession | M | `{location}` | "My decision dial at {location} points to deliberation." | `{location}` | no | ES: figurative device, no new event. |  |  |
| ES-G-06 | Gear obsession | O | - | "Even an exit algorithm needs a pause button." | - | no | ES: metaphor, no mechanism. |  |  |
| ES-G-07 | Loot greed | J | `{location}` | "The possibility of spare components argues for {location}." | `{location}` | no | ES: no component or award. |  |  |
| ES-G-08 | Overconfidence | M | `{location}` | "At {location}, my theory reserves the right to reconsider." | `{location}` | no | ES: no later decision guaranteed. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-H-01 | Loot greed | J | `{location}` | "My acquisitive streak found another excuse to admire {location}." | `{location}` | no | ES: no item or later discovery. |  |  |
| ES-H-02 | Genre tension | M | `{location}` | "At {location}, my appetite has reserved a shelf for prospects." | `{location}` | no | ES: metaphor, no inventory. |  |  |
| ES-H-03 | Loot greed | O | - | "The possibility of a bottle cap is a powerful argument." | - | no | ES: no object or award claimed. |  |  |
| ES-H-04 | Gear obsession | J | `{location}` | "My packing urges applaud the pause at {location}." | `{location}` | no | ES: no promised duration or packing action. |  |  |
| ES-H-05 | Overconfidence | M | `{location}` | "At {location}, I consider restraint a negotiable collectible." | `{location}` | no | ES: no item or permanent decision. |  |  |
| ES-H-06 | Loot greed | O | - | "An exit interrupts the beautiful prospect of loot." | - | no | ES: no observed loot or permanent stay. |  |  |
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
| SI-B-02 | Overconfidence | M | `{item}` | "I'd challenge {item} to look any more impressive." | `{item}` | no | SI: exact award; figurative challenge. |  |  |
| SI-B-03 | Genre tension | O | - | "A recovered prize gets an entrance worthy of a champion." | - | no | SI: award exists; no owner or value inferred. |  |  |
| SI-B-04 | Genre tension | J | `{item}` | "Give the victory fanfare to the object itself: {item}." | `{item}` | no | SI: exact award; figurative fanfare, no fight asserted. |  |  |
| SI-B-05 | Loot greed | M | `{item}` | "I respect {item} for arriving with trophy-level ambition." | `{item}` | no | SI: exact award; opinion, not ownership. |  |  |
| SI-B-06 | Overconfidence | O | - | "Even the loot gets to look tough after a find." | - | no | SI: awarded object exists; no combat claimed. |  |  |
| SI-B-07 | Genre tension | J | `{item}` | "My applause has a worthy target: {item}." | `{item}` | no | SI: exact award; no item claimed by speaker. |  |  |
| SI-B-08 | Loot greed | M | `{item}` | "I'd nominate {item} for the title of loudest treasure." | `{item}` | no | SI: exact award; figurative title, no transfer. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SI-C-01 | Loot greed | J | `{item}` | "A find worth a quiet nod: {item}." | `{item}` | no | SI: exact award; no recipient inferred. |  |  |
| SI-C-02 | Genre tension | M | `{item}` | "I can admire {item} without staging a parade." | `{item}` | no | SI: exact award; no personal ownership. |  |  |
| SI-C-03 | Gear obsession | O | - | "A recovered object doesn't make the exit any shorter." | - | no | SI: no route or safe extraction claimed. |  |  |
| SI-C-04 | Overconfidence | J | `{item}` | "A measured cheer seems appropriate for {item}." | `{item}` | no | SI: exact award; no extraction result. |  |  |
| SI-C-05 | Loot greed | M | `{item}` | "I'll give {item} a nod, not a parade." | `{item}` | no | SI: exact award; no further find. |  |  |
| SI-C-06 | Gear obsession | O | - | "An awarded find doesn't need a victory dance." | - | no | SI: no travel or item usefulness guaranteed. |  |  |
| SI-C-07 | Genre tension | J | `{item}` | "The surprise gets one respectful eyebrow: {item}." | `{item}` | no | SI: exact award; no recipient inferred. |  |  |
| SI-C-08 | Overconfidence | M | `{item}` | "My enthusiasm for {item} has learned to use its indoor voice." | `{item}` | no | SI: exact award; no building inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-G-01 | Loot greed | J | `{item}` | "My curiosity wants an exploded diagram of {item}." | `{item}` | no | SI: exact award; no disassembly or ownership. |  |  |
| SI-G-02 | Gear obsession | M | `{item}` | "I give {item} full marks for having measurable edges." | `{item}` | no | SI: exact award; no actual measurement. |  |  |
| SI-G-03 | Genre tension | O | - | "A recovered object is engineering's favorite cliffhanger." | - | no | SI: award exists; no further experiment. |  |  |
| SI-G-04 | Overconfidence | J | `{item}` | "I already have three incompatible theories about {item}." | `{item}` | no | SI: exact award; theories only. |  |  |
| SI-G-05 | Gear obsession | M | `{item}` | "The silhouette of {item} demands a schematic and several arrows." | `{item}` | no | SI: exact award; figurative demand, no drawing. |  |  |
| SI-G-06 | Loot greed | O | - | "Every recovered object deserves a technical biography." | - | no | SI: award exists; no recipient or valuation. |  |  |
| SI-G-07 | Genre tension | J | `{item}` | "My curiosity has found an entirely new obsession: {item}." | `{item}` | no | SI: exact award; no use or transfer. |  |  |
| SI-G-08 | Overconfidence | M | `{item}` | "I can design six unnecessary attachments for {item} before lunch." | `{item}` | no | SI: exact award; no actual design or attachment. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-H-01 | Loot greed | J | `{item}` | "A velvet pedestal is almost good enough for {item}." | `{item}` | no | SI: exact award; no ownership or pedestal. |  |  |
| SI-H-02 | Loot greed | M | `{item}` | "I would introduce {item} as royalty at a very exclusive banquet." | `{item}` | no | SI: exact award; figurative introduction. |  |  |
| SI-H-03 | Loot greed | O | - | "A recovered object is my favorite argument against moderation." | - | no | SI: no item identity or owner inferred. |  |  |
| SI-H-04 | Gear obsession | J | `{item}` | "The finest display case would be improved by {item}." | `{item}` | no | SI: exact award; no item moved or owned. |  |  |
| SI-H-05 | Overconfidence | M | `{item}` | "I can make {item} sound like a national treasure." | `{item}` | no | SI: exact award; no actual appraisal. |  |  |
| SI-H-06 | Loot greed | O | - | "A find like that makes restraint look terribly underdressed." | - | no | SI: no transfer or recipient inferred. |  |  |
| SI-H-07 | Genre tension | J | `{item}` | "A choir of angels ought to announce {item}." | `{item}` | no | SI: exact award; figurative choir. |  |  |
| SI-H-08 | Loot greed | M | `{item}` | "I rate {item} higher than the entire concept of enough." | `{item}` | no | SI: exact award; opinion, not price or ownership. |  |  |

## SN — Search or encounter, nothing recovered

**Required:** SearchCompleted or EncounterResolved, location, and committed
evidence that no item was recovered. Do not invent a hidden find, recipient,
subsequent search, or item loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-B-01 | Overconfidence | J | `{location}` | "An empty result has some nerve at {location}." | `{location}` | no | SN: no find or fight. |  |  |
| SN-B-02 | Genre tension | M | `{location}` | "At {location}, an empty search has the nerve to look proud." | `{location}` | no | SN: no equipment or find. |  |  |
| SN-B-03 | Loot greed | O | - | "My trophy shelf is offended by that fruitless search." | - | no | SN: figurative shelf, no award. |  |  |
| SN-B-04 | Overconfidence | J | `{location}` | "I refuse to applaud the absence of loot at {location}." | `{location}` | no | SN: no recipient or later find. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-C-01 | Genre tension | J | `{location}` | "At least disappointment packs light at {location}." | `{location}` | no | SN: no item or subsequent action. |  |  |
| SN-C-02 | Gear obsession | M | `{location}` | "The search at {location} offers no reason to rearrange a pouch." | `{location}` | no | SN: no pouch or inventory claim. |  |  |
| SN-C-03 | Loot greed | O | - | "An empty search saves me from counting my treasures too early." | - | no | SN: no actual inventory state. |  |  |
| SN-C-04 | Overconfidence | J | `{location}` | "I can survive the disappointment of {location}." | `{location}` | no | SN: emotional expression, not a future survival guarantee. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-G-01 | Gear obsession | J | `{location}` | "My curiosity measures a distinct lack of material at {location}." | `{location}` | no | SN: metaphor, no hidden item. |  |  |
| SN-G-02 | Gear obsession | M | `{location}` | "At {location}, the parts tray of my ambition comes up empty." | `{location}` | no | SN: figurative tray, no inventory assertion. |  |  |
| SN-G-03 | Overconfidence | O | - | "I can build an explanation out of absolutely nothing." | - | no | SN: no physical construction or discovery. |  |  |
| SN-G-04 | Loot greed | J | `{location}` | "My appetite for components objects to the empty result at {location}." | `{location}` | no | SN: no award. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-H-01 | Loot greed | J | `{location}` | "My greed objects to the vacancy at {location}." | `{location}` | no | SN: no item or recipient. |  |  |
| SN-H-02 | Genre tension | M | `{location}` | "At {location}, the absence deserves a very small velvet cushion." | `{location}` | no | SN: comparison, no acquisition. |  |  |
| SN-H-03 | Gear obsession | O | - | "My storage urges mourn an empty search in formal attire." | - | no | SN: figurative, no inventory claim. |  |  |
| SN-H-04 | Loot greed | J | `{location}` | "I assign an outrageous emotional value to nothing at {location}." | `{location}` | no | SN: no actual value or recovered item. |  |  |

## SX — OpportunityExhausted

**Required:** kind and location. The opportunity is exhausted; no finding,
award, owner, or subsequent opportunity follows from this event.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-B-01 | Overconfidence | J | `{location}` | "That's a bold way to run out of answers at {location}." | `{location}` | no | SX: no discovery or combat. |  |  |
| SX-B-02 | Genre tension | M | `{location}` | "The curtain at {location} has closed on this particular chance." | `{location}` | no | SX: figurative curtain, no further result. |  |  |
| SX-B-03 | Loot greed | O | - | "My trophy shelf objects to that opportunity running out." | - | no | SX: metaphor, no award. |  |  |
| SX-B-04 | Overconfidence | J | `{location}` | "I refuse to be impressed by a dead end at {location}." | `{location}` | no | SX: metaphor, no blocked route. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-C-01 | Genre tension | J | `{location}` | "There is no profit in interrogating an ending at {location}." | `{location}` | no | SX: no profit or item outcome asserted. |  |  |
| SX-C-02 | Gear obsession | M | `{location}` | "At {location}, even a compass can't extend that opportunity." | `{location}` | no | SX: comparison, no route. |  |  |
| SX-C-03 | Genre tension | O | - | "An exhausted chance is a finished page, not another chapter." | - | yes | SX: literary comparison, conservatively office-keyword flagged. |  |  |
| SX-C-04 | Loot greed | J | `{location}` | "No appetite can extend that spent opportunity at {location}." | `{location}` | no | SX: no item or future search. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-G-01 | Gear obsession | J | `{location}` | "This exhausted opportunity has stripped its last gear at {location}." | `{location}` | no | SX: metaphor, no machine or discovery. |  |  |
| SX-G-02 | Gear obsession | M | `{location}` | "At {location}, this experiment has run out of buttons." | `{location}` | no | SX: metaphor, no actual device or experiment. |  |  |
| SX-G-03 | Overconfidence | O | - | "Even an elegant equation has a last term." | - | no | SX: no new event or object. |  |  |
| SX-G-04 | Loot greed | J | `{location}` | "No amount of wishful solder extends that chance at {location}." | `{location}` | no | SX: no item created or recovered. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-H-01 | Loot greed | J | `{location}` | "The word exhausted has an ugly sound at {location}." | `{location}` | no | SX: no actual sound or item. |  |  |
| SX-H-02 | Genre tension | M | `{location}` | "At {location}, my hopes have met a very firm lid." | `{location}` | no | SX: figurative lid, no container asserted. |  |  |
| SX-H-03 | Gear obsession | O | - | "No display case can preserve an exhausted possibility." | - | no | SX: comparison, no possession. |  |  |
| SX-H-04 | Overconfidence | J | `{location}` | "I can dislike an ending with great precision at {location}." | `{location}` | no | SX: opinion only, no further opportunity. |  |  |

## AR — Arrived

**Required:** Arrived and committed location. Arrival is not evidence of
subsequent travel, a search, a find, or an encounter.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-B-01 | Overconfidence | J | `{location}` | "The scenery gets the full benefit of my presence at {location}." | `{location}` | no | AR: arrival only; no encounter. |  |  |
| AR-B-02 | Genre tension | M | `{location}` | "At {location}, my entrance deserves a brass fanfare." | `{location}` | no | AR: no actual music. |  |  |
| AR-B-03 | Gear obsession | O | - | "My boots consider every arrival a personal triumph." | - | no | AR: no new gear or ground condition. |  |  |
| AR-B-04 | Overconfidence | J | `{location}` | "Consider this a strongly worded hello to {location}." | `{location}` | no | AR: no opponent present. |  |  |
| AR-B-05 | Genre tension | M | `{location}` | "The welcome at {location} exists entirely in my head." | `{location}` | no | AR: no other actor or greeting. |  |  |
| AR-B-06 | Loot greed | O | - | "Arrival is a fine opening act for my appetite." | - | no | AR: no item or award. |  |  |
| AR-B-07 | Overconfidence | J | `{location}` | "I have arrived with enough eyebrows for {location}." | `{location}` | no | AR: no threat or action inferred. |  |  |
| AR-B-08 | Genre tension | M | `{location}` | "At {location}, my posture insists on top billing." | `{location}` | yes | AR: figurative billing, no performance. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-C-01 | Genre tension | J | `{location}` | "A quiet introduction suits me at {location}." | `{location}` | no | AR: no claim about actual sound. |  |  |
| AR-C-02 | Gear obsession | M | `{location}` | "At {location}, I would rather have a map than a speech." | `{location}` | no | AR: comparison, no possession. |  |  |
| AR-C-03 | Overconfidence | O | - | "I can arrive without making a ceremony of my ankles." | - | no | AR: no terrain or movement beyond arrival. |  |  |
| AR-C-04 | Genre tension | J | `{location}` | "A little attention travels well into {location}." | `{location}` | no | AR: no continued travel. |  |  |
| AR-C-05 | Gear obsession | M | `{location}` | "My internal compass gives {location} a respectful glance." | `{location}` | no | AR: figurative instrument, no route. |  |  |
| AR-C-06 | Loot greed | O | - | "After arrival, greed can wait for my bearings." | - | no | AR: no item or discovery. |  |  |
| AR-C-07 | Overconfidence | J | `{location}` | "I can manage an entrance without rushing around {location}." | `{location}` | no | AR: no continued movement or search. |  |  |
| AR-C-08 | Genre tension | M | `{location}` | "At {location}, my curiosity remembers its indoor voice." | `{location}` | no | AR: no indoor setting inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-G-01 | Gear obsession | J | `{location}` | "My enthusiasm pins a fresh schematic to {location}." | `{location}` | no | AR: figurative schematic, no device used. |  |  |
| AR-G-02 | Gear obsession | M | `{location}` | "At {location}, my curiosity unfolds its measuring tape." | `{location}` | no | AR: figurative tool, no inspection. |  |  |
| AR-G-03 | Overconfidence | O | - | "Arrival calibrates my enthusiasm without a screwdriver." | - | no | AR: metaphor, no tool or adjustment. |  |  |
| AR-G-04 | Gear obsession | J | `{location}` | "This calls for a blueprint in my head of {location}." | `{location}` | no | AR: figurative drawing, no surveyed features. |  |  |
| AR-G-05 | Gear obsession | M | `{location}` | "The idea of {location} now has coordinates in my head." | `{location}` | no | AR: no actual coordinates asserted. |  |  |
| AR-G-06 | Gear obsession | O | - | "Arrival is the first dot on my engineering sketch." | - | no | AR: figurative sketch, no survey or find. |  |  |
| AR-G-07 | Gear obsession | J | `{location}` | "My curiosity draws an oscilloscope squiggle for {location}." | `{location}` | no | AR: metaphor, no measurement. |  |  |
| AR-G-08 | Overconfidence | M | `{location}` | "At {location}, I am ready to overthink the geometry." | `{location}` | no | AR: attitude only, no search. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-H-01 | Loot greed | J | `{location}` | "My curiosity has already priced the scenery at {location}." | `{location}` | no | AR: opinion, no purchase or item. |  |  |
| AR-H-02 | Genre tension | M | `{location}` | "At {location}, anticipation puts on a velvet cape." | `{location}` | no | AR: figurative cape, no equipment. |  |  |
| AR-H-03 | Loot greed | O | - | "Arriving gives my souvenir instincts a lavish reception." | - | no | AR: no item or acquisition. |  |  |
| AR-H-04 | Gear obsession | J | `{location}` | "My packing impulses applaud the arrival at {location}." | `{location}` | no | AR: no luggage or contents asserted. |  |  |
| AR-H-05 | Overconfidence | M | `{location}` | "The prospects of {location} look excellent from inside my head." | `{location}` | no | AR: possibilities, no discovery. |  |  |
| AR-H-06 | Loot greed | O | - | "A fresh arrival makes my hunger unwrap the prospects." | - | no | AR: no item or award. |  |  |
| AR-H-07 | Genre tension | J | `{location}` | "I have brought my most acquisitive expression to {location}." | `{location}` | no | AR: expression only, no item or equipment. |  |  |
| AR-H-08 | Gear obsession | M | `{location}` | "At {location}, my storage urges feel right at home." | `{location}` | no | AR: no bag or ownership. |  |  |

## AD — DutyAdmission, Departure or Deployment

**Required:** one of the three exact kinds and its committed location. These
lines fit each kind: no line promises boarding, an arrived Raider, or extraction.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AD-B-01 | Overconfidence | J | `{location}` | "My entrance strategy includes glaring at {location}." | `{location}` | no | AD: admission, departure or deployment; no travel completion. |  |  |
| AD-B-02 | Genre tension | M | `{location}` | "At {location}, my confidence demands a louder overture." | `{location}` | no | AD: imagined music, no actual performance. |  |  |
| AD-B-03 | Gear obsession | O | - | "A new raid deserves dramatic lighting for my attitude." | - | no | AD: no transport claim. |  |  |
| AD-B-04 | Overconfidence | J | `{location}` | "I'll bring the raid-opening attitude, starting around {location}." | `{location}` | no | AD: start of raid, no arrival elsewhere or encounter. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-C-01 | Genre tension | J | `{location}` | "A composed beginning is a useful luxury at {location}." | `{location}` | no | AD: no subsequent travel or outcome. |  |  |
| AD-C-02 | Gear obsession | M | `{location}` | "At {location}, I'd choose a compass over a rousing speech." | `{location}` | no | AD: comparison, no possession. |  |  |
| AD-C-03 | Overconfidence | O | - | "I can begin without treating my shoelaces as emergency equipment." | - | no | AD: no condition or equipment action. |  |  |
| AD-C-04 | Genre tension | J | `{location}` | "No need to make a parade of starting a raid at {location}." | `{location}` | no | AD: no later result or boarding. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-G-01 | Gear obsession | J | `{location}` | "My curiosity clicks its starter switch for {location}." | `{location}` | no | AD: metaphorical switch, no real device. |  |  |
| AD-G-02 | Gear obsession | M | `{location}` | "At {location}, curiosity unfolds a paper circuit in my head." | `{location}` | no | AD: figurative circuit, no experiment. |  |  |
| AD-G-03 | Overconfidence | O | - | "Starting a raid is an engineering problem I can overcomplicate without tools." | - | no | AD: no equipment or later result. |  |  |
| AD-G-04 | Gear obsession | J | `{location}` | "My enthusiasm pulls the starting lever at {location}." | `{location}` | no | AD: figurative lever, no actual procedure. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-H-01 | Loot greed | J | `{location}` | "My ambition has packed a ceremonial suitcase for {location}." | `{location}` | no | AD: figurative suitcase, no inventory. |  |  |
| AD-H-02 | Genre tension | M | `{location}` | "At {location}, a raid's opening possibilities get the velvet-rope treatment." | `{location}` | no | AD: raid opening, figurative fixture only. |  |  |
| AD-H-03 | Loot greed | O | - | "Every raid begins with my pockets negotiating for room." | - | no | AD: figurative appetite, no capacity or acquisition. |  |  |
| AD-H-04 | Overconfidence | J | `{location}` | "My expectations are already pricing the prospects at {location}." | `{location}` | no | AD: figurative price, no transaction. |  |  |

## PS — PackingStarted

**Required:** PackingStarted and location. The allocated cargo is being packed;
no new award, completed packing, departure, or secured extraction is implied.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-B-01 | Overconfidence | J | `{location}` | "I'm giving this cargo the most intimidating fold at {location}." | `{location}` | no | PS: packing begun, no completion. |  |  |
| PS-B-02 | Genre tension | M | `{location}` | "At {location}, even my luggage deserves an entrance theme." | `{location}` | no | PS: figurative theme, no new award. |  |  |
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
| PS-C-02 | Genre tension | M | `{location}` | "At {location}, the cargo deserves a little patience." | `{location}` | no | PS: already allocated goods only. |  |  |
| PS-C-03 | Gear obsession | O | - | "Packing rewards patience more reliably than a heroic speech." | - | no | PS: no outcome or condition asserted. |  |  |
| PS-C-04 | Overconfidence | J | `{location}` | "I can handle a bundle without challenging physics at {location}." | `{location}` | no | PS: existing cargo; no completion. |  |  |
| PS-C-05 | Genre tension | M | `{location}` | "My ambitions at {location} fit inside a modest parcel." | `{location}` | no | PS: figurative parcel, no new item. |  |  |
| PS-C-06 | Loot greed | O | - | "Packing is easier when enthusiasm doesn't sit on the cargo." | - | no | PS: no travel or damage asserted. |  |  |
| PS-C-07 | Gear obsession | J | `{location}` | "I respect a well-considered knot, even around {location}." | `{location}` | no | PS: comparison, no action claimed. |  |  |
| PS-C-08 | Genre tension | M | `{location}` | "At {location}, restraint does the heavy lifting in my head." | `{location}` | no | PS: metaphor, no completion. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-G-01 | Gear obsession | J | `{location}` | "My preferred geometry has opinions about cargo at {location}." | `{location}` | no | PS: no added item or completed packing. |  |  |
| PS-G-02 | Gear obsession | M | `{location}` | "At {location}, packing is a small exercise in structural engineering." | `{location}` | no | PS: comparison, no actual machine. |  |  |
| PS-G-03 | Gear obsession | O | - | "Packing is a negotiation between angles and impatience." | - | no | PS: metaphor, no completion. |  |  |
| PS-G-04 | Overconfidence | J | `{location}` | "I can give spatial reasoning a heroic sound at {location}." | `{location}` | no | PS: no packing completion. |  |  |
| PS-G-05 | Gear obsession | M | `{location}` | "The load at {location} deserves a beautifully unnecessary diagram." | `{location}` | no | PS: figurative diagram, no new cargo. |  |  |
| PS-G-06 | Loot greed | O | - | "Stowing cargo makes a spare compartment sound like engineering genius." | - | no | PS: no capacity change. |  |  |
| PS-G-07 | Gear obsession | J | `{location}` | "My brain is machining a model of parcels at {location}." | `{location}` | no | PS: metaphorical machining, no actual work beyond event. |  |  |
| PS-G-08 | Overconfidence | M | `{location}` | "At {location}, I trust triangles more than optimism." | `{location}` | no | PS: geometric opinion, no success guarantee. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-H-01 | Loot greed | J | `{location}` | "Everything in this cargo merits a satin cushion at {location}." | `{location}` | no | PS: figurative cushion; no item added. |  |  |
| PS-H-02 | Genre tension | M | `{location}` | "At {location}, my affection for objects has become geometric." | `{location}` | no | PS: no new award or completion. |  |  |
| PS-H-03 | Gear obsession | O | - | "Stowing proves the perfect pouch needs a pocket for every pocket." | - | no | PS: no capacity change. |  |  |
| PS-H-04 | Loot greed | J | `{location}` | "I prefer treating a bundle like royalty at {location}." | `{location}` | no | PS: existing cargo only, no secured exit. |  |  |
| PS-H-05 | Overconfidence | M | `{location}` | "My stowing at {location} is a monument to attachment." | `{location}` | no | PS: metaphor; no completion. |  |  |
| PS-H-06 | Genre tension | O | - | "The cargo deserves a packing ceremony with full orchestration." | - | no | PS: figurative, no new object or completion. |  |  |
| PS-H-07 | Loot greed | J | `{location}` | "The idea of leaving anything behind offends me at {location}." | `{location}` | no | PS: no loss, decision or ownership invented. |  |  |
| PS-H-08 | Gear obsession | M | `{location}` | "At {location}, packing deserves a divider made of pure devotion." | `{location}` | no | PS: metaphor, no equipment claim. |  |  |

## PF — PackingCompleted

**Required:** PackingCompleted and location. Packing of previously allocated
cargo is complete; it does not secure extraction, award new items, or imply sale.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-B-01 | Overconfidence | J | `{location}` | "I have intimidated the concept of packing at {location}." | `{location}` | no | PF: completed packing, no departure or victory. |  |  |
| PF-B-02 | Gear obsession | M | `{location}` | "At {location}, my cargo has earned a respectful glare." | `{location}` | no | PF: previously allocated cargo only. |  |  |
| PF-B-03 | Genre tension | O | - | "The luggage montage deserves a more aggressive soundtrack." | - | no | PF: figurative montage, no extra action. |  |  |
| PF-B-04 | Loot greed | J | `{location}` | "A haul looks better with attitude at {location}." | `{location}` | no | PF: no new acquisition. |  |  |
| PF-B-05 | Overconfidence | M | `{location}` | "The bundle at {location} has survived my organizational genius." | `{location}` | no | PF: figurative survival, no extraction success. |  |  |
| PF-B-06 | Gear obsession | O | - | "Finished packing deserves a salute from every buckle." | - | no | PF: figurative buckle, no gear state. |  |  |
| PF-B-07 | Genre tension | J | `{location}` | "The final flourish belongs entirely to me at {location}." | `{location}` | no | PF: no departure or contest inferred. |  |  |
| PF-B-08 | Overconfidence | M | `{location}` | "At {location}, my handiwork gets the fiercest nod available." | `{location}` | no | PF: expression only, no secured extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-C-01 | Gear obsession | J | `{location}` | "Neatness earns a cautious nod at {location}." | `{location}` | no | PF: no item condition or successful exit. |  |  |
| PF-C-02 | Genre tension | M | `{location}` | "At {location}, the bundle looks less philosophical now." | `{location}` | no | PF: figurative impression, no extra item. |  |  |
| PF-C-03 | Overconfidence | O | - | "Packed cargo is not a magic carpet home." | - | no | PF: comparison; no travel or extraction. |  |  |
| PF-C-04 | Gear obsession | J | `{location}` | "I appreciate the orderly load at {location}." | `{location}` | no | PF: no item or capacity change. |  |  |
| PF-C-05 | Genre tension | M | `{location}` | "My nerves at {location} approve of finished work." | `{location}` | no | PF: emotional response, no further result. |  |  |
| PF-C-06 | Loot greed | O | - | "Packed cargo is easier to respect than a handful of wishes." | - | no | PF: comparison, no new item. |  |  |
| PF-C-07 | Overconfidence | J | `{location}` | "I can celebrate finished stowing without declaring victory over {location}." | `{location}` | no | PF: packing complete, not safe extraction. |  |  |
| PF-C-08 | Genre tension | M | `{location}` | "At {location}, my attention has one fewer loose end." | `{location}` | no | PF: no departure or extraction implied. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-G-01 | Gear obsession | J | `{location}` | "The geometry has stopped arguing with me at {location}." | `{location}` | no | PF: figurative; no invented mechanism. |  |  |
| PF-G-02 | Gear obsession | M | `{location}` | "At {location}, my inner slide rule gives a satisfied twitch." | `{location}` | no | PF: figurative instrument, no measurement. |  |  |
| PF-G-03 | Overconfidence | O | - | "My completed cargo layout merits a commemorative cog." | - | no | PF: figurative token, no award. |  |  |
| PF-G-04 | Gear obsession | J | `{location}` | "I applaud the idea of efficient containment at {location}." | `{location}` | no | PF: no capacity or item change. |  |  |
| PF-G-05 | Gear obsession | M | `{location}` | "The arrangement at {location} has earned its own schematic." | `{location}` | no | PF: figurative schematic, no added cargo. |  |  |
| PF-G-06 | Loot greed | O | - | "Packed cargo is geometry with excellent priorities." | - | no | PF: no contents or pocket claim. |  |  |
| PF-G-07 | Gear obsession | J | `{location}` | "My finished arrangement deserves applause at {location}." | `{location}` | no | PF: opinion, no measurement or award. |  |  |
| PF-G-08 | Overconfidence | M | `{location}` | "At {location}, my packing design is quietly smug." | `{location}` | no | PF: metaphor, no measurement. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-H-01 | Loot greed | J | `{location}` | "The cargo has achieved an extremely flattering arrangement at {location}." | `{location}` | no | PF: no item addition or secured extraction. |  |  |
| PF-H-02 | Genre tension | M | `{location}` | "At {location}, my affection for possessions looks positively architectural." | `{location}` | no | PF: no change in ownership or building. |  |  |
| PF-H-03 | Gear obsession | O | - | "Finished packing deserves a tiny curtain call." | - | no | PF: no gear condition. |  |  |
| PF-H-04 | Overconfidence | J | `{location}` | "I call this arrangement an act of devotion at {location}." | `{location}` | no | PF: opinion; no additional award. |  |  |
| PF-H-05 | Loot greed | M | `{location}` | "The bundle at {location} has my most protective admiration." | `{location}` | no | PF: already allocated cargo, no safety guarantee. |  |  |
| PF-H-06 | Genre tension | O | - | "That completed bundle looks ready for a coronation." | - | no | PF: comparison, no acquisition. |  |  |
| PF-H-07 | Gear obsession | J | `{location}` | "A display cabinet would envy the care taken at {location}." | `{location}` | no | PF: comparison, no actual item. |  |  |
| PF-H-08 | Loot greed | M | `{location}` | "At {location}, my admiration fits neatly around the load." | `{location}` | no | PF: no change in item ownership. |  |  |

## HW — HazardWarning

**Required:** HazardWarning, committed condition and location. A warning is a
forecast, not evidence that the hazard has already struck or will cause HP loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| HW-B-01 | Overconfidence | J | `{condition}` | "That warning merits my fiercest glare: {condition}." | `{condition}` | no | HW: warning only; no impact. |  |  |
| HW-B-02 | Genre tension | M | `{condition}` | "The forecast for {condition} needs a more intimidating narrator." | `{condition}` | no | HW: forecast only, no harm. |  |  |
| HW-B-03 | Gear obsession | O | - | "My stubbornness wears a helmet for the forecast." | - | no | HW: figurative gear; no impact. |  |  |
| HW-B-04 | Overconfidence | J | `{location}` | "Warnings are welcome to challenge my eyebrows at {location}." | `{location}` | no | HW: no hazard happened. |  |  |
| HW-B-05 | Genre tension | M | `{condition}` | "I rate the prediction of {condition} ambitious and badly timed." | `{condition}` | no | HW: forecast, no impact claimed. |  |  |
| HW-B-06 | Loot greed | O | - | "A gloomy prediction won't intimidate my appetite for trophies." | - | no | HW: no trophy, harm or loss. |  |  |
| HW-B-07 | Overconfidence | J | `{location}` | "I can look unimpressed by a warning at {location}." | `{location}` | no | HW: no success or encounter. |  |  |
| HW-B-08 | Genre tension | M | `{condition}` | "If {condition} wants an entrance, it can wait its turn." | `{condition}` | no | HW: no occurrence asserted. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-C-01 | Genre tension | J | `{condition}` | "A forecast is a useful reason to pay attention: {condition}." | `{condition}` | no | HW: warning, not impact. |  |  |
| HW-C-02 | Gear obsession | M | `{condition}` | "For {condition}, an umbrella sounds more useful than an argument." | `{condition}` | no | HW: comparison; no impact. |  |  |
| HW-C-03 | Overconfidence | O | - | "I can take a warning seriously without applauding it." | - | no | HW: no damage or occurrence. |  |  |
| HW-C-04 | Genre tension | J | `{location}` | "The forecast has my attention, including at {location}." | `{location}` | no | HW: no impact or HP loss. |  |  |
| HW-C-05 | Gear obsession | M | `{condition}` | "For {condition}, a sensible coat would be more persuasive than optimism." | `{condition}` | no | HW: comparison, no protection claimed. |  |  |
| HW-C-06 | Loot greed | O | - | "A warning makes even loot sound less persuasive." | - | no | HW: no item or safe outcome. |  |  |
| HW-C-07 | Overconfidence | J | `{location}` | "I can respect a prediction without panicking at {location}." | `{location}` | no | HW: no claimed hazard outcome. |  |  |
| HW-C-08 | Genre tension | M | `{condition}` | "The mention of {condition} has earned my quieter voice." | `{condition}` | no | HW: forecast only. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-G-01 | Gear obsession | J | `{condition}` | "My nerves behave like a barometer around this forecast: {condition}." | `{condition}` | no | HW: explicit comparison, no carried instrument or impact. |  |  |
| HW-G-02 | Gear obsession | M | `{condition}` | "I'd draw {condition} as a very nervous waveform." | `{condition}` | no | HW: figurative sketch, no measurement. |  |  |
| HW-G-03 | Overconfidence | O | - | "A warning is a measurement with its eyebrows raised." | - | no | HW: comparison, no actual measurement. |  |  |
| HW-G-04 | Gear obsession | J | `{location}` | "My threat sensor of a brain dislikes this alert at {location}." | `{location}` | no | HW: metaphor, no impact. |  |  |
| HW-G-05 | Gear obsession | M | `{condition}` | "The prediction of {condition} needs a larger caution symbol." | `{condition}` | no | HW: no outcome predicted. |  |  |
| HW-G-06 | Loot greed | O | - | "A spare dial would make this forecast look almost collectible." | - | no | HW: comparison, no acquisition. |  |  |
| HW-G-07 | Overconfidence | J | `{location}` | "I can overengineer a response to this warning at {location}." | `{location}` | no | HW: no response completed. |  |  |
| HW-G-08 | Gear obsession | M | `{condition}` | "For {condition}, my schematic of caution uses extra insulation." | `{condition}` | no | HW: figurative schematic, no protection. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-H-01 | Loot greed | J | `{condition}` | "I value a warning less than a bottle cap: {condition}." | `{condition}` | no | HW: opinion; no condition impact. |  |  |
| HW-H-02 | Genre tension | M | `{condition}` | "My collector's instinct hopes {condition} isn't interested in souvenirs." | `{condition}` | no | HW: no item loss or hazard impact. |  |  |
| HW-H-03 | Gear obsession | O | - | "A prediction cannot fit inside a jewelry box." | - | no | HW: comparison, no acquisition. |  |  |
| HW-H-04 | Overconfidence | J | `{location}` | "I can treasure a forecast in theory at {location}." | `{location}` | no | HW: no impact or success asserted. |  |  |
| HW-H-05 | Loot greed | M | `{condition}` | "The forecast of {condition} is worth less than a cracked marble." | `{condition}` | no | HW: comparison, no item or impact. |  |  |
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
| HI-B-02 | Genre tension | M | `{condition}` | "Apparently {condition} prefers action to introductions." | `{condition}` | no | HI: impact only, no attacker. |  |  |
| HI-B-03 | Gear obsession | O | - | "My pride wears armor against interruptions like that." | - | no | HI: metaphor, no injury. |  |  |
| HI-B-04 | Overconfidence | J | `{location}` | "I object to that environmental attitude at {location}." | `{location}` | no | HI: no damage amount or opponent. |  |  |
| HI-B-05 | Genre tension | M | `{condition}` | "I rate {condition} shamelessly rude for that entrance." | `{condition}` | no | HI: no further effects. |  |  |
| HI-B-06 | Loot greed | O | - | "My appetite objects to that interruption with both elbows." | - | no | HI: no item loss. |  |  |
| HI-B-07 | Gear obsession | J | `{location}` | "That impact deserves a helmet made of stubbornness at {location}." | `{location}` | no | HI: metaphor, no HP assertion. |  |  |
| HI-B-08 | Overconfidence | M | `{condition}` | "I refuse to give {condition} the satisfaction of a dramatic expression." | `{condition}` | no | HI: attitude only, no outcome. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-C-01 | Genre tension | J | `{condition}` | "That was rather less abstract than a forecast: {condition}." | `{condition}` | no | HI: impact, not necessarily HP loss. |  |  |
| HI-C-02 | Gear obsession | M | `{condition}` | "An umbrella facing {condition} would have my sympathy." | `{condition}` | no | HI: comparison, no protection claim. |  |  |
| HI-C-03 | Overconfidence | O | - | "I can acknowledge an impact without pretending it was polite." | - | no | HI: no health consequence. |  |  |
| HI-C-04 | Genre tension | J | `{location}` | "The environment has delivered its point at {location}." | `{location}` | no | HI: figurative, no HP outcome. |  |  |
| HI-C-05 | Gear obsession | M | `{condition}` | "A raincoat facing {condition} would reconsider its career." | `{condition}` | no | HI: comparison, no condition type inferred. |  |  |
| HI-C-06 | Loot greed | O | - | "Not even a bauble renders an impact more pleasant." | - | no | HI: no actual item or injury. |  |  |
| HI-C-07 | Overconfidence | J | `{location}` | "I can treat this interruption with appropriate caution at {location}." | `{location}` | no | HI: no HP loss or recovery. |  |  |
| HI-C-08 | Genre tension | M | `{condition}` | "That contact with {condition} has earned a quiet grimace." | `{condition}` | no | HI: reaction only; no health delta inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-G-01 | Gear obsession | J | `{condition}` | "My nerves register the jolt like a seismograph: {condition}." | `{condition}` | no | HI: figurative instrument, no condition type. |  |  |
| HI-G-02 | Gear obsession | M | `{condition}` | "I would draw {condition} as a very emphatic waveform." | `{condition}` | no | HI: no actual measurement or HP loss. |  |  |
| HI-G-03 | Overconfidence | O | - | "An impact makes an excellent calibration point for caution." | - | no | HI: no equipment or measurement. |  |  |
| HI-G-04 | Gear obsession | J | `{location}` | "The shock absorber in my head has comments at {location}." | `{location}` | no | HI: metaphor, no mitigation claim. |  |  |
| HI-G-05 | Bureaucracy | M | `{condition}` | "The contact with {condition} deserves an oversized asterisk." | `{condition}` | yes | HI: annotation metaphor, no HP wound or timing inferred. |  |  |
| HI-G-06 | Loot greed | O | - | "My appetite would trade that impact for a replacement spring." | - | no | HI: no component acquired or damaged. |  |  |
| HI-G-07 | Overconfidence | J | `{location}` | "I can call that a data point at {location}." | `{location}` | no | HI: metaphor, no measurement. |  |  |
| HI-G-08 | Gear obsession | M | `{condition}` | "My inner shock gauge gives {condition} a disapproving squint." | `{condition}` | no | HI: figurative instrument, no actual reading. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-H-01 | Loot greed | J | `{condition}` | "I'd rather receive a trinket than that impact: {condition}." | `{condition}` | no | HI: comparison, no wound. |  |  |
| HI-H-02 | Genre tension | M | `{condition}` | "My collection of complaints welcomes {condition} reluctantly." | `{condition}` | no | HI: figurative collection, no item loss. |  |  |
| HI-H-03 | Gear obsession | O | - | "A silk cushion would be a more considerate interruption." | - | no | HI: comparison, no damage claim. |  |  |
| HI-H-04 | Overconfidence | J | `{location}` | "I can dislike that impact without selling my optimism at {location}." | `{location}` | no | HI: no sale, injury or outcome. |  |  |
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
| RD-B-02 | Genre tension | M | `{robot}` | "I'd give {robot} a standing ovation for knowing when to stop." | `{robot}` | no | RD: exact committed robot; no extra action. |  |  |
| RD-B-03 | Gear obsession | O | - | "A chassis has finally lost an argument." | - | no | RD: metaphor; no injury or loot. |  |  |
| RD-B-04 | Overconfidence | J | `{location}` | "A disabled machine merits no rebuttal from me at {location}." | `{location}` | no | RD: defeated robot, no dialogue or reward claimed. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-C-01 | Genre tension | J | `{location}` | "A disabled machine is the least demanding company at {location}." | `{location}` | no | RD: robot defeated, no Raider injury or recovery inferred. |  |  |
| RD-C-02 | Overconfidence | M | `{robot}` | "I prefer {robot} in the non-pursuing category." | `{robot}` | no | RD: defeated robot; no earlier pursuit asserted. |  |  |
| RD-C-03 | Gear obsession | O | - | "A defeated chassis has fewer immediate ambitions." | - | no | RD: no salvage. |  |  |
| RD-C-04 | Genre tension | J | `{location}` | "That chassis has finally stopped volunteering for combat at {location}." | `{location}` | no | RD: robot defeated; no other threat ruled out. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-G-01 | Gear obsession | J | `{location}` | "The defeated chassis has discontinued its demonstration at {location}." | `{location}` | no | RD: no part removal or reward. |  |  |
| RD-G-02 | Gear obsession | M | `{robot}` | "I would love to see {robot} explained by a wiring diagram." | `{robot}` | no | RD: interest only; no inspection. |  |  |
| RD-G-03 | Overconfidence | O | - | "That machine has reached a remarkably decisive off state." | - | no | RD: no physical cause or loot. |  |  |
| RD-G-04 | Loot greed | J | `{location}` | "My appetite for components admires that defeat at {location}." | `{location}` | no | RD: no cargo allocated. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-H-01 | Loot greed | J | `{location}` | "My souvenir instincts admire that defeated machine at {location}." | `{location}` | no | RD: no reward. |  |  |
| RD-H-02 | Genre tension | M | `{robot}` | "I regard {robot} as a magnificent advertisement for parts." | `{robot}` | no | RD: no acquisition or ownership. |  |  |
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
| RX-B-04 | Overconfidence | J | `{location}` | "I reserve a glare for that departing machine at {location}." | `{location}` | no | RX: no chase or subsequent encounter. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-C-01 | Genre tension | J | `{location}` | "A departing machine is preferable company at {location}." | `{location}` | no | RX: robot escaped, no Raider exit. |  |  |
| RX-C-02 | Overconfidence | M | `{robot}` | "I acknowledge {robot} made an exit rather than a concession." | `{robot}` | no | RX: robot escaped, no defeat or salvage. |  |  |
| RX-C-03 | Gear obsession | O | - | "A fleeing chassis is not a dismantled chassis." | - | no | RX: no defeat or salvage. |  |  |
| RX-C-04 | Genre tension | J | `{location}` | "A little distance from that machine is welcome at {location}." | `{location}` | no | RX: robot leaves; no safe outcome claimed. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-G-01 | Gear obsession | J | `{location}` | "That chassis has demonstrated the departure function at {location}." | `{location}` | no | RX: robot escaped; no damage outcome. |  |  |
| RX-G-02 | Gear obsession | M | `{robot}` | "I'd put {robot} on the schematic's disappearing arrow." | `{robot}` | no | RX: figurative diagram, no Raider escape. |  |  |
| RX-G-03 | Overconfidence | O | - | "I can explain a machine's retreat without calling it a repair." | - | no | RX: no defeat or repair. |  |  |
| RX-G-04 | Loot greed | J | `{location}` | "A vanished mechanism is an inconvenient specimen at {location}." | `{location}` | no | RX: no capture or acquisition. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-H-01 | Loot greed | J | `{location}` | "My hunger for parts dislikes that escape from {location}." | `{location}` | no | RX: no actual cargo or pursuit. |  |  |
| RX-H-02 | Genre tension | M | `{robot}` | "I resent {robot} taking its components elsewhere." | `{robot}` | no | RX: robot leaves, no owned parts lost. |  |  |
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
| RC-B-02 | Genre tension | M | `{location}` | "At {location}, I reserve my fiercest glare for the machine's loot aftermath." | `{location}` | no | RC: no cargo or recipient asserted. |  |  |
| RC-B-03 | Genre tension | O | - | "A chassis's cargo verdict is a disappointingly quiet encore." | - | no | RC: neither award nor contents asserted. |  |  |
| RC-B-04 | Loot greed | J | `{location}` | "My prize ambitions demand a rematch with the machine's salvage result at {location}." | `{location}` | no | RC: no cargo or recipient. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-C-01 | Genre tension | J | `{location}` | "A robot's cargo result deserves a level head at {location}." | `{location}` | no | RC: allocated or no eligible cargo. |  |  |
| RC-C-02 | Gear obsession | M | `{location}` | "At {location}, robot salvage needs a tightly fastened temper." | `{location}` | no | RC: neither cargo outcome excluded. |  |  |
| RC-C-03 | Overconfidence | O | - | "I can accept machine salvage without naming myself the beneficiary." | - | no | RC: no personal award or departure asserted. |  |  |
| RC-C-04 | Loot greed | J | `{location}` | "My hopes for spare parts wait politely on the machine's cargo conclusion at {location}." | `{location}` | no | RC: no item or recipient asserted. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-G-01 | Bureaucracy | J | `{location}` | "My curiosity audits the machine's cargo decision at {location}." | `{location}` | yes | RC: figurative audit, neither cargo outcome excluded. |  |  |
| RC-G-02 | Genre tension | M | `{location}` | "At {location}, the salvage decision has a satisfyingly mechanical full stop." | `{location}` | yes | RC: ending/record metaphor, no item award inferred. |  |  |
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
| RV-B-01 | Overconfidence | J | `{location}` | "My composure recovery has sharp elbows at {location}." | `{location}` | no | RV: either phase; no HP change. |  |  |
| RV-B-02 | Genre tension | M | `{location}` | "At {location}, calming my nerves merits fighting music." | `{location}` | no | RV: figurative only; either phase. |  |  |
| RV-B-03 | Gear obsession | O | - | "Composure is the hinge my temper keeps trying to bend." | - | no | RV: metaphor, no health claim. |  |  |
| RV-B-04 | Overconfidence | J | `{location}` | "Even recovery of my poise gets a stern look at {location}." | `{location}` | no | RV: composure only, neither phase implies healing. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-C-01 | Genre tension | J | `{location}` | "Composure is worth a quiet breath at {location}." | `{location}` | no | RV: either phase; no restored health. |  |  |
| RV-C-02 | Gear obsession | M | `{location}` | "At {location}, recovering my poise calls for a cushion of patience." | `{location}` | no | RV: figurative cushion; no item claim. |  |  |
| RV-C-03 | Overconfidence | O | - | "Settling my nerves is no occasion for a sprint." | - | no | RV: no completion or HP effect. |  |  |
| RV-C-04 | Genre tension | J | `{location}` | "My composure settles into recovery like a guest afraid to break the furniture at {location}." | `{location}` | no | RV: mental comparison, no actual companion, furniture or timing. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-G-01 | Gear obsession | J | `{location}` | "Calming my jitters gives my train of thought a smoother bearing at {location}." | `{location}` | no | RV: explicit mental metaphor, no carried instrument or HP gain. |  |  |
| RV-G-02 | Gear obsession | M | `{location}` | "At {location}, recovering my nerve is like damping a pendulum." | `{location}` | no | RV: comparison, no complete recovery. |  |  |
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
| DN-B-04 | Overconfidence | J | `{location}` | "I can glare just fine from down here at {location}." | `{location}` | no | DN: reaction only; no recovery. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-C-01 | Genre tension | J | `{location}` | "Being downed offers a distinctly inconvenient viewpoint at {location}." | `{location}` | no | DN: downed posture, no KO or rescue assumed. |  |  |
| DN-C-02 | Gear obsession | M | `{location}` | "At {location}, this floor-level setback makes upholstery look tactical." | `{location}` | no | DN: comparison, no carried gear or medical effect. |  |  |
| DN-C-03 | Overconfidence | O | - | "Falling out of vertical is no excuse to misplace my composure." | - | no | DN: downed perspective, no recovery or healing. |  |  |
| DN-C-04 | Genre tension | J | `{location}` | "I don't recommend this arrangement with the ground at {location}." | `{location}` | no | DN: no settled outcome. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-G-01 | Gear obsession | J | `{location}` | "My internal gyroscope dislikes this orientation at {location}." | `{location}` | no | DN: figurative instrument; no healing. |  |  |
| DN-G-02 | Gear obsession | M | `{location}` | "At {location}, my posture has become a badly folded blueprint." | `{location}` | no | DN: comparison, no KO or rescue. |  |  |
| DN-G-03 | Overconfidence | O | - | "I can analyze this angle without calling it a design feature." | - | no | DN: no completed recovery. |  |  |
| DN-G-04 | Gear obsession | J | `{location}` | "My downed posture needs a hydraulic jack at {location}." | `{location}` | no | DN: comparison, no actual equipment or lift. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-H-01 | Loot greed | J | `{location}` | "I value an upright pose more than a bauble at {location}." | `{location}` | no | DN: comparison, no recovery. |  |  |
| DN-H-02 | Genre tension | M | `{location}` | "At {location}, even my ambitions are lying down." | `{location}` | no | DN: no settled KO or rescue. |  |  |
| DN-H-03 | Gear obsession | O | - | "A plush cushion would overvalue this particular position." | - | no | DN: comparison, no healing. |  |  |
| DN-H-04 | Overconfidence | J | `{location}` | "My greed objects to this view of {location}." | `{location}` | no | DN: no item or later result. |  |  |

## KO — KnockedOut

**Required:** KnockedOut and location. Knockout is settled; no line claims a
successful extraction, recovery, item salvage, or details absent from the record.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-B-01 | Genre tension | J | `{location}` | "That is an outrageously abrupt curtain at {location}." | `{location}` | no | KO: no extraction or recovery. |  |  |
| KO-B-02 | Overconfidence | M | `{location}` | "At {location}, even my pride has run out of speeches." | `{location}` | no | KO: settled knockout; no later action. |  |  |
| KO-B-03 | Gear obsession | O | - | "A helmet can't argue with a knockout finale." | - | no | KO: comparison; no equipment outcome. |  |  |
| KO-B-04 | Genre tension | J | `{location}` | "That knockout wasn't my preferred kind of dramatic close at {location}." | `{location}` | no | KO: settled knockout, no actual sound or recovery. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-C-01 | Genre tension | J | `{location}` | "A knockout is an unusually thorough way to cancel my sightseeing at {location}." | `{location}` | no | KO: raid-ending outcome, no recovery promise. |  |  |
| KO-C-02 | Gear obsession | M | `{location}` | "At {location}, even a handrail would disapprove of that ending." | `{location}` | no | KO: comparison; no escape. |  |  |
| KO-C-03 | Genre tension | O | - | "The knockout finally ran my understatement out of synonyms." | - | no | KO: settled knockout, no subsequent event claimed. |  |  |
| KO-C-04 | Genre tension | J | `{location}` | "A raid-ending collapse makes a poor recommendation for {location}." | `{location}` | no | KO: settled raid loss, not permanent death or extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-G-01 | Gear obsession | J | `{location}` | "My internal circuit has reached an abrupt termination at {location}." | `{location}` | no | KO: metaphor, no repair or recovery. |  |  |
| KO-G-02 | Gear obsession | M | `{location}` | "At {location}, my schematic ends in a rather blunt stop symbol." | `{location}` | no | KO: figurative schematic, no later result. |  |  |
| KO-G-03 | Overconfidence | O | - | "Even my grandest optimization can't argue with a knockout." | - | no | KO: settled knockout, no later action or healing. |  |  |
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
| DL-B-04 | Overconfidence | J | `{location}` | "I dislike being outshouted by a timer at {location}." | `{location}` | no | DL: deadline expired; no extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-C-01 | Genre tension | J | `{location}` | "The deadline has offered an extremely firm answer at {location}." | `{location}` | no | DL: loss, not extraction. |  |  |
| DL-C-02 | Gear obsession | M | `{location}` | "At {location}, the expired deadline reads like an accusation." | `{location}` | no | DL: no owned device or successful exit. |  |  |
| DL-C-03 | Overconfidence | O | - | "I can recognize an expired time limit without liking it." | - | no | DL: no later recovery. |  |  |
| DL-C-04 | Genre tension | J | `{location}` | "The clock was less flexible than my hopes at {location}." | `{location}` | no | DL: no time or success inferred. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-G-01 | Gear obsession | J | `{location}` | "The deadline timer has an unfriendly conclusion at {location}." | `{location}` | no | DL: deadline loss, no rescue. |  |  |
| DL-G-02 | Gear obsession | M | `{location}` | "At {location}, the deadline has turned my blueprint into a full stop." | `{location}` | no | DL: metaphor, no extraction. |  |  |
| DL-G-03 | Overconfidence | O | - | "An expired time limit refuses to negotiate with arithmetic." | - | no | DL: no actual negotiation or changed time. |  |  |
| DL-G-04 | Gear obsession | J | `{location}` | "The hourglass of my optimism ran out at {location}." | `{location}` | no | DL: figurative instrument, no recovery. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-H-01 | Loot greed | J | `{location}` | "I wouldn't collect an expired deadline at {location}." | `{location}` | no | DL: no specific item loss or extraction. |  |  |
| DL-H-02 | Genre tension | M | `{location}` | "At {location}, the clock has no respect for my hunger." | `{location}` | no | DL: no cargo or later outcome. |  |  |
| DL-H-03 | Gear obsession | O | - | "Even a jeweled hourglass can't flatter an expired deadline." | - | no | DL: comparison, no acquisition. |  |  |
| DL-H-04 | Overconfidence | J | `{location}` | "My enthusiasm puts a low price on that ending at {location}." | `{location}` | no | DL: figurative price, no sale. |  |  |

## HS — HiddenPocketSalvaged

**Required:** HiddenPocketSalvaged and the committed preserved item.
`{item}` comes directly from this event's ItemName and unique item receipt,
not a general backpack item or a new draw. The owner-private event has already
committed preservation. No line repeats its value or asserts a safe extraction.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-B-01 | Overconfidence | J | `{item}` | "The pocket's smallest swagger belongs to {item}." | `{item}` | no | HS: exact preserved item, no extra award. |  |  |
| HS-B-02 | Genre tension | M | `{item}` | "I'd frame {item} in a heroic close-up small enough for a seam." | `{item}` | no | HS: exact preserved item, no successful extraction. |  |  |
| HS-B-03 | Gear obsession | O | - | "I respect a hidden compartment with that much nerve." | - | no | HS: no item identity. |  |  |
| HS-B-04 | Loot greed | J | `{item}` | "My prize grin is absurdly large for {item}." | `{item}` | no | HS: exact preserved item, no value inferred. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-C-01 | Genre tension | J | `{item}` | "A very small argument for sensible storage: {item}." | `{item}` | no | HS: exact preserved item, no extra award. |  |  |
| HS-C-02 | Gear obsession | M | `{item}` | "I give {item} a pocket-sized nod, not an extraction victory." | `{item}` | no | HS: no new award or safe extraction. |  |  |
| HS-C-03 | Loot greed | O | - | "A saved object is better than an elaborate excuse." | - | no | HS: no type or recipient invented. |  |  |
| HS-C-04 | Overconfidence | J | `{item}` | "I can appreciate a narrow escape on behalf of {item}." | `{item}` | no | HS: preservation metaphor, not Raider extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-G-01 | Gear obsession | J | `{item}` | "Elegant containment deserves a specimen like {item}." | `{item}` | no | HS: exact preserved item, no invented equipment. |  |  |
| HS-G-02 | Gear obsession | M | `{item}` | "I would use {item} to explain why seams beat complicated blueprints." | `{item}` | no | HS: opinion, no new tool or extraction. |  |  |
| HS-G-03 | Overconfidence | O | - | "That hidden pocket outperformed a complex schematic." | - | no | HS: comparison; no item specified. |  |  |
| HS-G-04 | Gear obsession | J | `{item}` | "My storage obsession has an excellent illustration: {item}." | `{item}` | no | HS: exact preserved item, no added capacity. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-H-01 | Loot greed | J | `{item}` | "A pedestal would be excessive; a pocket suits {item}." | `{item}` | no | HS: exact preserved item, no item invented. |  |  |
| HS-H-02 | Genre tension | M | `{item}` | "I regard {item} as a magnificent advertisement for concealed storage." | `{item}` | no | HS: subjective opinion, no value or extra item. |  |  |
| HS-H-03 | Gear obsession | O | - | "A secret compartment is a beautiful little argument for hoarding." | - | no | HS: no item identity or extra award. |  |  |
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
| HE-C-02 | Gear obsession | M | `{location}` | "At {location}, the hidden compartment has no encouraging surprises." | `{location}` | no | HE: empty, not salvage. |  |  |
| HE-C-03 | Overconfidence | O | - | "An empty seam is no reason to panic about the pocket." | - | no | HE: no item or reward asserted. |  |  |
| HE-C-04 | Loot greed | J | `{location}` | "My appetite for salvage is staring into a seam-sized absence at {location}." | `{location}` | no | HE: empty pocket, no salvage claimed. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-G-01 | Gear obsession | J | `{location}` | "That compartment has optimized itself for emptiness at {location}." | `{location}` | no | HE: no item saved or transferred. |  |  |
| HE-G-02 | Gear obsession | M | `{location}` | "At {location}, the pocket schematic has a rather blank center." | `{location}` | no | HE: figurative schematic, no salvage. |  |  |
| HE-G-03 | Overconfidence | O | - | "I can diagram an empty seam with impressive precision." | - | no | HE: no real diagram or saved item. |  |  |
| HE-G-04 | Loot greed | J | `{location}` | "My appetite for components objects to that empty pocket at {location}." | `{location}` | no | HE: no acquisition or salvage. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Fact-contradiction check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|
| HE-H-01 | Loot greed | J | `{location}` | "An empty pocket is a personal insult at {location}." | `{location}` | no | HE: no saved item. |  |  |
| HE-H-02 | Genre tension | M | `{location}` | "At {location}, the hidden compartment offers my hunger nothing." | `{location}` | no | HE: empty, no transfer. |  |  |
| HE-H-03 | Gear obsession | O | - | "A vacant pouch is an architectural insult to a treasure enthusiast." | - | no | HE: no inventory change beyond committed empty pocket. |  |  |
| HE-H-04 | Overconfidence | J | `{location}` | "I can be offended by an empty seam at {location}." | `{location}` | no | HE: no salvage or other settlement claim. |  |  |

## Automated review checks

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

### Reproducible procedure

Copy the Python block below to a temporary file **outside the repository**.
It uses only the Python standard library and does not load content or change
any project file. From the workspace root in PowerShell:

```powershell
python "$env:TEMP\diary-pilot-check.py" docs\DIARY_PILOT_LINES.md --strict
git --no-pager show d6ac41df9f3457b7fda1c665b55def1328bb4dc0:docs/DIARY_PILOT_LINES.md |
    Set-Content -Encoding utf8 "$env:TEMP\diary-pilot-before.md"
python "$env:TEMP\diary-pilot-check.py" "$env:TEMP\diary-pilot-before.md" --summary
```

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
