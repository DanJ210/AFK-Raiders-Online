# DIARY VARIETY: pilot lines for human review

**Status: draft only. Nothing here is loaded into the narration catalog or runtime.**

Part C follows the 2026-11-13 direction to proceed after the Part B review gate.
The [Part A/B audit](DIARY_VARIETY_AUDIT.md) identifies 11 pilot families. This
document contains **8 candidates for each family and each of the four traits**
(11 x 4 x 8 = 352). “Family” means the family header below; a family may cover
several listed event kinds, but its `{phase}` / `{outcome}` slot must describe
the exact committed kind. Candidates are not automatically interchangeable
between kinds.

**Draft provenance:** drafted in this session by the AI assistant using Copilot
SDK in VS Code; no external model or source prose was used. Exact model identity
was not exposed in the session. Human review remains pending.

## Slot and eligibility contract

Each table row inherits the required event fields and fact-check rule from its
family header; its **Slots** cell lists every additional replacement slot used
by that candidate. Rarity is explicitly `common` for every line. A candidate is
eligible only when its required committed fields and every slot are present and
consistent. Missing optional fields make that candidate ineligible; use another
eligible candidate or retain the existing authored narration, never invent a
default fact. Slot phrases below are abstract, finite renderings of committed
facts, not free-form model output. Facts and mechanical numbers remain in the
event record/detail. Each aside starts with the consequence, then uses one joke.

Six joke modes rotate: gear obsession, overconfidence, false optimism, genre
tension, loot greed, and bureaucracy. Bureaucracy/paperwork is deliberately rare
(at most one candidate in any eight-line trait/family set). Brawler is
confrontational confidence, Cautious is practical risk-awareness, Gadgeteer is
device/engineering fixation, and Hoarder overvalues potential future loot.
Event truth outranks trait voice.

No new location, robot, faction, resident, brand, or phenomenon name is coined
or used. Existing canon terms in lines are limited to A.R.C. and Secret Hidden
Pocket, checked against [PARODY_MAPPING.md](lore/PARODY_MAPPING.md),
[Robots](lore/entities/robots.md), [Zones](lore/entities/zones.md),
[Field Phenomena](lore/entities/field-phenomena.md), [Weapons](lore/entities/weapons.md),
and [Desperanza](lore/entities/desperanza.md). There are no unmapped-name flags.
Line wording was checked against [tone](lore/content-guidelines/TONE_VOICE.md)
and [legal guardrails](lore/content-guidelines/LEGAL_GUARDRAILS.md).

## 1. Personality change — `PersonalityChanged`

**Required committed fields:** event kind; reason; old/new Greed and Mood;
committed resilience. **Slot:** `{change}` is a concise direction-only summary
derived from the deltas (for example, “Greed rose” or “Mood fell”), never an
inferred motivation. Exact numeric deltas remain in the event detail.
**Fact-check rule:** `{change}` must match the old/new values; do not infer why
the change occurred beyond the committed reason.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PC-B-01 | common | Overconfidence | `{change}` | “{change}; I remain the loudest argument in the room.” | Delta direction only. |
| PC-B-02 | common | Genre tension | `{change}` | “{change}; the raid has added another chapter without asking my editor.” | No duration implied. |
| PC-B-03 | common | False optimism | `{change}` | “{change}; clearly this is the part where the plan improves.” | Improvement is framed as hope, not outcome. |
| PC-B-04 | common | Gear obsession | `{change}` | “{change}; I trust the straps to keep my confidence attached.” | Does not claim a specific item. |
| PC-B-05 | common | Loot greed | `{change}` | “{change}; there is still room in my strategy for one more object.” | No acquisition claimed. |
| PC-B-06 | common | Overconfidence | `{change}` | “{change}; I have not lost the argument with tomorrow yet.” | No future result claimed. |
| PC-B-07 | common | Genre tension | `{change}` | “{change}; my nerves have entered the arena before I did.” | Does not assert a fight. |
| PC-B-08 | common | Bureaucracy | `{change}` | “{change}; I have recorded this as a temporary concession.” | No actual record or procedure asserted. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PC-C-01 | common | Risk-awareness | `{change}` | “{change}; I will update the risk estimate, not the facts.” | Delta direction only. |
| PC-C-02 | common | False optimism | `{change}` | “{change}; cautiously, this may be manageable.” | Conditional optimism only. |
| PC-C-03 | common | Gear obsession | `{change}` | “{change}; I am checking the kit before trusting the feeling.” | Does not claim equipment condition. |
| PC-C-04 | common | Genre tension | `{change}` | “{change}; the quiet still deserves a second look.” | No threat inferred. |
| PC-C-05 | common | Loot greed | `{change}` | “{change}; any further search needs to justify its exposure.” | Does not claim a search started. |
| PC-C-06 | common | Risk-awareness | `{change}` | “{change}; I will keep the exit in the calculation.” | No exit decision claimed. |
| PC-C-07 | common | Overconfidence | `{change}` | “{change}; I can be prepared without being impressed with myself.” | No success claimed. |
| PC-C-08 | common | Bureaucracy | `{change}` | “{change}; I have revised the checklist, not the weather.” | No actual checklist asserted. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PC-G-01 | common | Gear obsession | `{change}` | “{change}; I am recalibrating the theory around the available hardware.” | No hardware is named or claimed. |
| PC-G-02 | common | Overconfidence | `{change}` | “{change}; the data has not yet disproved my preferred explanation.” | Explanation is opinion, not event fact. |
| PC-G-03 | common | False optimism | `{change}` | “{change}; this adjustment may become useful after testing.” | No test is claimed complete. |
| PC-G-04 | common | Genre tension | `{change}` | “{change}; I am listening for a diagnostic beep that may not exist.” | No device or beep asserted. |
| PC-G-05 | common | Loot greed | `{change}` | “{change}; perhaps another component will make the pattern obvious.” | No component acquired. |
| PC-G-06 | common | Gear obsession | `{change}` | “{change}; my mental schematic now has an extra arrow.” | Schematic is figurative. |
| PC-G-07 | common | False optimism | `{change}` | “{change}; iteration is progress if I label it carefully.” | No gameplay progress claimed. |
| PC-G-08 | common | Bureaucracy | `{change}` | “{change}; the specification has been warned about my interpretation.” | No actual specification procedure asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PC-H-01 | common | Loot greed | `{change}` | “{change}; I can still imagine a use for the next thing.” | No item acquired. |
| PC-H-02 | common | False optimism | `{change}` | “{change}; the plan may yet appreciate in value.” | No asset-value claim. |
| PC-H-03 | common | Overconfidence | `{change}` | “{change}; restraint has not won custody of my pockets.” | No pocket contents asserted. |
| PC-H-04 | common | Gear obsession | `{change}` | “{change}; I am making room in the theory for more storage.” | No capacity or item change claimed. |
| PC-H-05 | common | Genre tension | `{change}` | “{change}; leaving now would interrupt several hypothetical acquisitions.” | No decision or acquisition asserted. |
| PC-H-06 | common | Loot greed | `{change}` | “{change}; potential usefulness remains a very roomy category.” | No value or item claim. |
| PC-H-07 | common | False optimism | `{change}` | “{change}; I am sure the next opportunity will clarify everything.” | Hope only; no next event promised. |
| PC-H-08 | common | Bureaucracy | `{change}` | “{change}; my inventory policy continues to oppose its own limits.” | No inventory-limit change claimed. |

## 2. Shield absorption — `ShieldAbsorbed`

**Required committed fields:** event kind; shield charge lost; HP damage/health
delta. **Slot:** `{shield_fact}` is restricted to “the shield spent charge” or
“the shield spent charge and some damage got through,” selected from the committed
HP delta; no amount is repeated. **Fact-check rule:** never say the Raider took
HP damage when the recorded delta is zero, or that the shield prevented all
damage when HP damage is positive.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SH-B-01 | common | Overconfidence | `{shield_fact}` | “{shield_fact}; I consider that a warning shot from my own equipment.” | Shield receipt determines the fact. |
| SH-B-02 | common | Genre tension | `{shield_fact}` | “{shield_fact}; the chassis has started negotiating through the armor.” | No robot/attacker asserted. |
| SH-B-03 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; I am keeping the argument pointed outward.” | No shield durability inferred. |
| SH-B-04 | common | False optimism | `{shield_fact}` | “{shield_fact}; the next part of this plan is obviously quieter.” | Hope only. |
| SH-B-05 | common | Loot greed | `{shield_fact}` | “{shield_fact}; I would hate to leave before the equipment review.” | No departure decision. |
| SH-B-06 | common | Overconfidence | `{shield_fact}` | “{shield_fact}; I am calling that contact, not a setback.” | No outcome claim. |
| SH-B-07 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; my shield has made its opening statement.” | No extra action asserted. |
| SH-B-08 | common | Bureaucracy | `{shield_fact}` | “{shield_fact}; I dispute the equipment's version of events.” | Dispute is figurative. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SH-C-01 | common | Risk-awareness | `{shield_fact}` | “{shield_fact}; I will treat the margin as smaller now.” | No specific remaining durability claimed. |
| SH-C-02 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; I am checking what protection remains before proceeding.” | Check is intent, not completed inspection. |
| SH-C-03 | common | Genre tension | `{shield_fact}` | “{shield_fact}; that was enough noise to reconsider the route.” | No route change claimed. |
| SH-C-04 | common | False optimism | `{shield_fact}` | “{shield_fact}; perhaps the next stretch will be less persuasive.” | Hope only. |
| SH-C-05 | common | Loot greed | `{shield_fact}` | “{shield_fact}; anything else must earn its risk.” | No item or decision claimed. |
| SH-C-06 | common | Risk-awareness | `{shield_fact}` | “{shield_fact}; I will not confuse protection with immunity.” | General observation. |
| SH-C-07 | common | Overconfidence | `{shield_fact}` | “{shield_fact}; I remain calm in a strictly provisional sense.” | Describes voice, not status. |
| SH-C-08 | common | Bureaucracy | `{shield_fact}` | “{shield_fact}; the safety estimate needs a quiet revision.” | No actual paperwork asserted. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SH-G-01 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; the shield is now part of the test results.” | No test action asserted. |
| SH-G-02 | common | Overconfidence | `{shield_fact}` | “{shield_fact}; my model remains confident and increasingly specific.” | Model is figurative. |
| SH-G-03 | common | False optimism | `{shield_fact}` | “{shield_fact}; this is useful data, if not my preferred data.” | No experiment asserted. |
| SH-G-04 | common | Genre tension | `{shield_fact}` | “{shield_fact}; I heard the armor make a point.” | Sound is figurative, not literal audio. |
| SH-G-05 | common | Loot greed | `{shield_fact}` | “{shield_fact}; I may need a second shield for the diagram.” | No purchase or inventory claim. |
| SH-G-06 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; the protection curve has developed a personality.” | No system attribute asserted. |
| SH-G-07 | common | False optimism | `{shield_fact}` | “{shield_fact}; the next measurement could be friendlier.” | No next hit predicted. |
| SH-G-08 | common | Bureaucracy | `{shield_fact}` | “{shield_fact}; I am annotating the margin in my head.” | No actual notes asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SH-H-01 | common | Loot greed | `{shield_fact}` | “{shield_fact}; I would prefer to keep every useful layer.” | No layer count claimed. |
| SH-H-02 | common | False optimism | `{shield_fact}` | “{shield_fact}; the shield may still have future potential.” | Does not assert it remains active. |
| SH-H-03 | common | Gear obsession | `{shield_fact}` | “{shield_fact}; I am emotionally retaining the protection.” | No durability claim. |
| SH-H-04 | common | Overconfidence | `{shield_fact}` | “{shield_fact}; that is not permission to stop collecting options.” | No collection event. |
| SH-H-05 | common | Genre tension | `{shield_fact}` | “{shield_fact}; the exit can wait until the equipment finishes speaking.” | Does not claim staying. |
| SH-H-06 | common | Loot greed | `{shield_fact}` | “{shield_fact}; I have not ruled out a backup for the backup.” | Hypothetical only. |
| SH-H-07 | common | False optimism | `{shield_fact}` | “{shield_fact}; there is still a theoretical best-case scenario.” | No outcome guaranteed. |
| SH-H-08 | common | Bureaucracy | `{shield_fact}` | “{shield_fact}; I am appealing the concept of disposable gear.” | Figurative appeal only. |

## 3. Unmitigated damage — `UnmitigatedDamage`

**Required committed fields:** event kind; source; HP damage/health delta.
**Slot:** `{source_fact}` is “damage came through from {source},” using the
committed source only. **Fact-check rule:** this kind means the damage was
unmitigated by the shield path; do not claim a specific attacker unless `{source}`
names it. Do not repeat amounts in the aside.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| UD-B-01 | common | Overconfidence | `{source_fact}` | “{source_fact}; I have accepted the challenge on behalf of my ribs.” | Source only; no fight outcome. |
| UD-B-02 | common | Genre tension | `{source_fact}` | “{source_fact}; the threat has skipped the introduction.” | No actor beyond source. |
| UD-B-03 | common | False optimism | `{source_fact}` | “{source_fact}; I am choosing to call the next moment a comeback.” | Hope only. |
| UD-B-04 | common | Gear obsession | `{source_fact}` | “{source_fact}; I will ask my kit why it stayed out of this.” | Does not claim gear failed. |
| UD-B-05 | common | Loot greed | `{source_fact}` | “{source_fact}; I remain committed to the expedition's less comfortable goals.” | No specific goal/item. |
| UD-B-06 | common | Overconfidence | `{source_fact}` | “{source_fact}; I am still willing to argue with the source.” | No counterattack asserted. |
| UD-B-07 | common | Genre tension | `{source_fact}` | “{source_fact}; this is the portion where the soundtrack gets ideas.” | No soundtrack exists. |
| UD-B-08 | common | Bureaucracy | `{source_fact}` | “{source_fact}; I am filing a personal objection to momentum.” | Figurative filing. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| UD-C-01 | common | Risk-awareness | `{source_fact}` | “{source_fact}; I will reassess exposure before assuming it is over.” | No follow-up damage inferred. |
| UD-C-02 | common | False optimism | `{source_fact}` | “{source_fact}; recovery remains possible, not promised.” | No recovery outcome claimed. |
| UD-C-03 | common | Genre tension | `{source_fact}` | “{source_fact}; I am listening for whether the danger has passed.” | No extra event asserted. |
| UD-C-04 | common | Gear obsession | `{source_fact}` | “{source_fact}; I will check the equipment after finding cover.” | Intent only; no cover assertion. |
| UD-C-05 | common | Loot greed | `{source_fact}` | “{source_fact}; the remaining haul must justify staying exposed.” | No decision asserted. |
| UD-C-06 | common | Risk-awareness | `{source_fact}` | “{source_fact}; I have revised my definition of safe distance.” | No distance measurement. |
| UD-C-07 | common | Overconfidence | `{source_fact}` | “{source_fact}; I remain functional in a carefully limited sense.” | No health threshold inferred. |
| UD-C-08 | common | Bureaucracy | `{source_fact}` | “{source_fact}; the risk list has gained a very direct example.” | Figurative list only. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| UD-G-01 | common | Gear obsession | `{source_fact}` | “{source_fact}; my theory did not include direct contact.” | No equipment claim. |
| UD-G-02 | common | Overconfidence | `{source_fact}` | “{source_fact}; the model predicted a more abstract problem.” | Opinion, not prediction record. |
| UD-G-03 | common | False optimism | `{source_fact}` | “{source_fact}; this is still a useful data point.” | No test setup implied. |
| UD-G-04 | common | Genre tension | `{source_fact}` | “{source_fact}; the warning arrived in a very physical format.” | No warning event claimed. |
| UD-G-05 | common | Loot greed | `{source_fact}` | “{source_fact}; I may need another part to explain this.” | No part acquired. |
| UD-G-06 | common | Gear obsession | `{source_fact}` | “{source_fact}; I am revising the diagram from memory.” | No actual diagram asserted. |
| UD-G-07 | common | False optimism | `{source_fact}` | “{source_fact}; iteration is easier when the subject remains mobile.” | Does not claim mobility state. |
| UD-G-08 | common | Bureaucracy | `{source_fact}` | “{source_fact}; the specification has been informed of my disappointment.” | Figurative only. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| UD-H-01 | common | Loot greed | `{source_fact}` | “{source_fact}; I hope the rest of the haul learned from that.” | No other item implicated. |
| UD-H-02 | common | False optimism | `{source_fact}` | “{source_fact}; the expedition still has hypothetical upside.” | No reward guaranteed. |
| UD-H-03 | common | Overconfidence | `{source_fact}` | “{source_fact}; I refuse to let one incident price the whole trip.” | No value calculation. |
| UD-H-04 | common | Gear obsession | `{source_fact}` | “{source_fact}; my future packing plan will account for this.” | No actual packing claim. |
| UD-H-05 | common | Genre tension | `{source_fact}` | “{source_fact}; the loot is not allowed to become a distraction.” | No decision or loot claim. |
| UD-H-06 | common | Loot greed | `{source_fact}` | “{source_fact}; there may still be something worth carrying home.” | Possibility only. |
| UD-H-07 | common | False optimism | `{source_fact}` | “{source_fact}; I am retaining optimism in a compact container.” | Metaphor, no item. |
| UD-H-08 | common | Bureaucracy | `{source_fact}` | “{source_fact}; I contest the damage's claim on my attention.” | No actual procedure. |

## 4. Resilience trim — `Resilience`

**Required committed fields:** event kind; raw damage source; positive damage
trimmed by resilience. **Slot:** `{resilience_fact}` says “resilience trimmed
part of the raw hit from {source}.” **Fact-check rule:** only this event kind
establishes a positive trim; do not claim that all damage was prevented or that
the Raider is safe.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RE-B-01 | common | Overconfidence | `{resilience_fact}` | “{resilience_fact}; even my nerves have learned to brace.” | No additional mitigation claimed. |
| RE-B-02 | common | Genre tension | `{resilience_fact}` | “{resilience_fact}; the hit had to negotiate for its entrance.” | Figurative only. |
| RE-B-03 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; I will call that a favorable opening.” | No outcome forecast. |
| RE-B-04 | common | Gear obsession | `{resilience_fact}` | “{resilience_fact}; my body has become part of the kit.” | No equipment or armor claim. |
| RE-B-05 | common | Loot greed | `{resilience_fact}` | “{resilience_fact}; I have saved attention for the next opportunity.” | No item or safety claim. |
| RE-B-06 | common | Overconfidence | `{resilience_fact}` | “{resilience_fact}; I remain difficult to discourage.” | Mood/status not asserted. |
| RE-B-07 | common | Genre tension | `{resilience_fact}` | “{resilience_fact}; the danger has received a less dramatic entrance.” | No threat severity asserted. |
| RE-B-08 | common | Bureaucracy | `{resilience_fact}` | “{resilience_fact}; my composure has denied the hit a speaking slot.” | Metaphor, not event procedure. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RE-C-01 | common | Risk-awareness | `{resilience_fact}` | “{resilience_fact}; the remaining risk still counts.” | Does not imply no damage. |
| RE-C-02 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; a small margin is still a margin.” | No quantified margin. |
| RE-C-03 | common | Gear obsession | `{resilience_fact}` | “{resilience_fact}; I will not mistake resilience for a spare shield.” | No shield state inferred. |
| RE-C-04 | common | Genre tension | `{resilience_fact}` | “{resilience_fact}; I am waiting for the rest of the situation.” | No next event predicted. |
| RE-C-05 | common | Loot greed | `{resilience_fact}` | “{resilience_fact}; further searching still needs a reason.” | No search decision. |
| RE-C-06 | common | Risk-awareness | `{resilience_fact}` | “{resilience_fact}; I will keep the exit in view.” | No actual exit view/location asserted. |
| RE-C-07 | common | Overconfidence | `{resilience_fact}` | “{resilience_fact}; composure is useful, not proof.” | No safety claim. |
| RE-C-08 | common | Bureaucracy | `{resilience_fact}` | “{resilience_fact}; the risk assessment stays open.” | Figurative status only. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RE-G-01 | common | Gear obsession | `{resilience_fact}` | “{resilience_fact}; the internal dampener remains an unverified theory.” | Does not claim an actual device. |
| RE-G-02 | common | Overconfidence | `{resilience_fact}` | “{resilience_fact}; my model has found one variable it likes.” | Figurative model only. |
| RE-G-03 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; perhaps the next result will be equally cooperative.” | No future result promised. |
| RE-G-04 | common | Genre tension | `{resilience_fact}` | “{resilience_fact}; biology has joined the field test without consent.” | No literal experiment. |
| RE-G-05 | common | Loot greed | `{resilience_fact}` | “{resilience_fact}; now I want to know whether equipment can do that.” | No equipment effect asserted. |
| RE-G-06 | common | Gear obsession | `{resilience_fact}` | “{resilience_fact}; I am adding a human-shaped component to the diagram.” | Figurative diagram. |
| RE-G-07 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; the result is encouraging at a very narrow scale.” | No broad success claim. |
| RE-G-08 | common | Bureaucracy | `{resilience_fact}` | “{resilience_fact}; the theory has requested another trial.” | No actual test scheduled. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RE-H-01 | common | Loot greed | `{resilience_fact}` | “{resilience_fact}; I would like to preserve this useful margin.” | No item or full prevention claimed. |
| RE-H-02 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; the trip may still return something worth keeping.” | No reward predicted as fact. |
| RE-H-03 | common | Gear obsession | `{resilience_fact}` | “{resilience_fact}; I am counting myself among the equipment.” | Metaphor only. |
| RE-H-04 | common | Overconfidence | `{resilience_fact}` | “{resilience_fact}; I remain in the category of potentially useful.” | No health status beyond event. |
| RE-H-05 | common | Genre tension | `{resilience_fact}` | “{resilience_fact}; leaving would end the inventory possibilities.” | No departure decision. |
| RE-H-06 | common | Loot greed | `{resilience_fact}` | “{resilience_fact}; any spare capacity is still a promise.” | No capacity claim. |
| RE-H-07 | common | False optimism | `{resilience_fact}` | “{resilience_fact}; maybe the next object will appreciate the effort.” | No object exists by implication? "next object" hypothetical. |
| RE-H-08 | common | Bureaucracy | `{resilience_fact}` | “{resilience_fact}; I have assigned this margin a retention category.” | Figurative category only. |

## 5. Extraction decision — `ExtractionDecision`

**Required committed fields:** event kind; reason; committed decision text.
**Slot:** `{decision_fact}` states either that the Raider is heading for
extraction or staying for now, exactly as recorded. **Fact-check rule:** do not
turn a stay into a departure or imply arrival, extraction success, or a cause
other than the committed reason.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| ED-B-01 | common | Overconfidence | `{decision_fact}` | “{decision_fact}; I expect the exit to keep up.” | No extraction completion claimed. |
| ED-B-02 | common | Genre tension | `{decision_fact}` | “{decision_fact}; the raid has entered its dramatic final paragraph.” | No deadline outcome claimed. |
| ED-B-03 | common | False optimism | `{decision_fact}` | “{decision_fact}; the next part of the plan may be the sensible part.” | Hope only. |
| ED-B-04 | common | Gear obsession | `{decision_fact}` | “{decision_fact}; my equipment has been briefed on the change.” | No gear or briefing event asserted. |
| ED-B-05 | common | Loot greed | `{decision_fact}` | “{decision_fact}; I have not stopped valuing the remaining possibilities.” | No loot or further search claimed. |
| ED-B-06 | common | Overconfidence | `{decision_fact}` | “{decision_fact}; I remain prepared to argue with the clock.” | No timer value or result asserted. |
| ED-B-07 | common | Genre tension | `{decision_fact}` | “{decision_fact}; even the atmosphere has started packing.” | Metaphor, not a packing event. |
| ED-B-08 | common | Bureaucracy | `{decision_fact}` | “{decision_fact}; I have entered this under decisive behavior.” | No actual record or procedure. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| ED-C-01 | common | Risk-awareness | `{decision_fact}` | “{decision_fact}; I will treat the recorded choice as current, not permanent.” | No future decision inferred. |
| ED-C-02 | common | False optimism | `{decision_fact}` | “{decision_fact}; there may still be time to make the careful choice.” | Conditional; does not change the recorded decision. |
| ED-C-03 | common | Gear obsession | `{decision_fact}` | “{decision_fact}; I am checking that the kit matches the route.” | Intent only; no gear state claimed. |
| ED-C-04 | common | Genre tension | `{decision_fact}` | “{decision_fact}; the quiet part of this plan remains unverified.” | No threat asserted. |
| ED-C-05 | common | Loot greed | `{decision_fact}` | “{decision_fact}; anything left behind must still be worth the exposure.” | No remaining loot claimed. |
| ED-C-06 | common | Risk-awareness | `{decision_fact}` | “{decision_fact}; I will keep the deadline in the calculation.” | No deadline timing stated. |
| ED-C-07 | common | Overconfidence | `{decision_fact}` | “{decision_fact}; caution and confidence can share the same route.” | No route or success inferred. |
| ED-C-08 | common | Bureaucracy | `{decision_fact}` | “{decision_fact}; the risk assessment remains open to new facts.” | Figurative assessment only. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| ED-G-01 | common | Gear obsession | `{decision_fact}` | “{decision_fact}; the route diagram is adjusting to the evidence.” | No actual diagram asserted. |
| ED-G-02 | common | Overconfidence | `{decision_fact}` | “{decision_fact}; my model recognizes this as an intentional transition.” | Opinion, not a model result. |
| ED-G-03 | common | False optimism | `{decision_fact}` | “{decision_fact}; the next variable may be more cooperative.” | No next event predicted. |
| ED-G-04 | common | Genre tension | `{decision_fact}` | “{decision_fact}; the extraction sequence has become unusually theoretical.” | No extraction result asserted. |
| ED-G-05 | common | Loot greed | `{decision_fact}` | “{decision_fact}; I would like to preserve the remaining sample size.” | No items or count asserted. |
| ED-G-06 | common | Gear obsession | `{decision_fact}` | “{decision_fact}; the plan now has a very visible switch.” | Metaphor; no control input claimed. |
| ED-G-07 | common | False optimism | `{decision_fact}` | “{decision_fact}; perhaps the next reading will explain the last one.” | Hope only. |
| ED-G-08 | common | Bureaucracy | `{decision_fact}` | “{decision_fact}; the specification has been updated in my head.” | No actual document asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| ED-H-01 | common | Loot greed | `{decision_fact}` | “{decision_fact}; the possible haul remains emotionally present.” | No haul claimed. |
| ED-H-02 | common | False optimism | `{decision_fact}` | “{decision_fact}; there could still be something worth carrying.” | Possibility only; no item or decision. |
| ED-H-03 | common | Overconfidence | `{decision_fact}` | “{decision_fact}; I have not run out of reasons to be optimistic.” | No specific opportunity asserted. |
| ED-H-04 | common | Gear obsession | `{decision_fact}` | “{decision_fact}; the kit will have to tolerate another revision.” | No change to equipment claimed. |
| ED-H-05 | common | Genre tension | `{decision_fact}` | “{decision_fact}; every exit makes the uncollected future louder.” | Metaphorical future; no event asserted. |
| ED-H-06 | common | Loot greed | `{decision_fact}` | “{decision_fact}; I remain interested in whatever was not counted.” | No uncounted loot asserted. |
| ED-H-07 | common | False optimism | `{decision_fact}` | “{decision_fact}; I am sure the next option will be better defined.” | Hope only. |
| ED-H-08 | common | Bureaucracy | `{decision_fact}` | “{decision_fact}; I have reserved a category for later.” | Figurative category only. |

## 6. Search resolution — `SearchCompleted`, `EncounterResolved`, `OpportunityExhausted`

**Required committed fields:** event kind and committed resolution description;
awards and recipient/item fields when present. **Slot:** `{search_fact}` says
only that the search/encounter resolved, or names a recovered item and recipient
only when the event records that award. **Fact-check rule:** absent awards do not
mean an empty backpack, and a participant is not necessarily an award recipient.
For `OpportunityExhausted`, the slot must state that this opportunity was already
exhausted, not that the Raider searched it successfully.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SR-B-01 | common | Overconfidence | `{search_fact}` | “{search_fact}; I remain ready for the next argument.” | Does not claim another event. |
| SR-B-02 | common | Genre tension | `{search_fact}` | “{search_fact}; the result has arrived with suspiciously little fanfare.” | No fanfare event asserted. |
| SR-B-03 | common | False optimism | `{search_fact}` | “{search_fact}; the next result may be more encouraging.” | No future result guaranteed. |
| SR-B-04 | common | Gear obsession | `{search_fact}` | “{search_fact}; my kit is waiting for its review.” | No gear check asserted. |
| SR-B-05 | common | Loot greed | `{search_fact}` | “{search_fact}; I remain interested in the opportunity after this one.” | Does not assert another opportunity exists. |
| SR-B-06 | common | Overconfidence | `{search_fact}` | “{search_fact}; that is one result, not a verdict on my career.” | No career or combat result claimed. |
| SR-B-07 | common | Genre tension | `{search_fact}` | “{search_fact}; suspense has been included at no extra charge.” | Joke only; no price claim. |
| SR-B-08 | common | Bureaucracy | `{search_fact}` | “{search_fact}; I have logged a formal opinion about the suspense.” | No actual log or procedure. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SR-C-01 | common | Risk-awareness | `{search_fact}` | “{search_fact}; I will not infer more than the record says.” | Explicitly limits inference. |
| SR-C-02 | common | False optimism | `{search_fact}` | “{search_fact}; the next opportunity may be clearer.” | No next opportunity promised. |
| SR-C-03 | common | Gear obsession | `{search_fact}` | “{search_fact}; I will check the kit before changing the plan.” | Intent only. |
| SR-C-04 | common | Genre tension | `{search_fact}` | “{search_fact}; uncertainty remains a surprisingly active participant.” | No additional participant asserted. |
| SR-C-05 | common | Loot greed | `{search_fact}` | “{search_fact}; any more searching must justify its cost.” | No cost or decision asserted. |
| SR-C-06 | common | Risk-awareness | `{search_fact}` | “{search_fact}; I will keep the unresolved parts unresolved.” | No unrecorded outcome supplied. |
| SR-C-07 | common | Overconfidence | `{search_fact}` | “{search_fact}; I can proceed without pretending to know the rest.” | No progress or route claimed. |
| SR-C-08 | common | Bureaucracy | `{search_fact}` | “{search_fact}; the evidence is sufficient for exactly this sentence.” | Figurative evidence review. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SR-G-01 | common | Gear obsession | `{search_fact}` | “{search_fact}; I am revising the expected-output diagram.” | No actual diagram or output claim. |
| SR-G-02 | common | Overconfidence | `{search_fact}` | “{search_fact}; my theory remains compatible with these facts.” | No theory presented as fact. |
| SR-G-03 | common | False optimism | `{search_fact}` | “{search_fact}; perhaps the next sample will be less cryptic.” | Hope only. |
| SR-G-04 | common | Genre tension | `{search_fact}` | “{search_fact}; the data has chosen a dramatic pause.” | Metaphorical data. |
| SR-G-05 | common | Loot greed | `{search_fact}` | “{search_fact}; I would like to inspect any recorded payload.” | Desire, not inspection or award. |
| SR-G-06 | common | Gear obsession | `{search_fact}` | “{search_fact}; my notes have acquired a suspicious blank space.” | No actual notes or missing award inferred. |
| SR-G-07 | common | False optimism | `{search_fact}` | “{search_fact}; the system may yet produce a tidier explanation.” | No system behavior predicted. |
| SR-G-08 | common | Bureaucracy | `{search_fact}` | “{search_fact}; the test report will have to live with that.” | No test/report asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| SR-H-01 | common | Loot greed | `{search_fact}` | “{search_fact}; I have preserved room for further possibility.” | No capacity state asserted. |
| SR-H-02 | common | False optimism | `{search_fact}` | “{search_fact}; the next find could still redeem the suspense.” | No next find guaranteed. |
| SR-H-03 | common | Overconfidence | `{search_fact}` | “{search_fact}; I remain an excellent judge of future usefulness.” | Self-opinion, no item value claim. |
| SR-H-04 | common | Gear obsession | `{search_fact}` | “{search_fact}; the backpack is part of the ongoing theory.” | No backpack change claimed. |
| SR-H-05 | common | Genre tension | `{search_fact}` | “{search_fact}; the unclaimed possibilities are making a scene.” | No unclaimed item asserted. |
| SR-H-06 | common | Loot greed | `{search_fact}` | “{search_fact}; I would like the opportunity to keep producing options.” | No continuing opportunity claimed. |
| SR-H-07 | common | False optimism | `{search_fact}` | “{search_fact}; useful things have been known to appear eventually.” | General hope; no award claim. |
| SR-H-08 | common | Bureaucracy | `{search_fact}` | “{search_fact}; I have reserved shelf space for the result.” | Figurative; no storage change. |

## 7. Arrival and admission — `Deployment`, `Departure`, `DutyAdmission`, `Arrived`

**Required committed fields:** event kind; location when present. **Slot:**
`{arrival_fact}` states only the exact transition represented by the kind
(deployed, departed the Entry Tunnel, admitted to an ongoing raid, or arrived at
the committed location). **Fact-check rule:** keep the kinds distinct; do not
imply a new raid, another Raider's presence, or arrival at a destination that is
not recorded.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| AA-B-01 | common | Overconfidence | `{arrival_fact}` | “{arrival_fact}; I was expecting a more difficult introduction.” | No encounter claimed. |
| AA-B-02 | common | Genre tension | `{arrival_fact}` | “{arrival_fact}; the field has made its opening move.” | No adversary asserted. |
| AA-B-03 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; perhaps the next part will be orderly.” | Hope only. |
| AA-B-04 | common | Gear obsession | `{arrival_fact}` | “{arrival_fact}; my equipment has reached the same chapter.” | No gear condition claimed. |
| AA-B-05 | common | Loot greed | `{arrival_fact}` | “{arrival_fact}; I am prepared to notice anything useful.” | No loot claimed. |
| AA-B-06 | common | Overconfidence | `{arrival_fact}` | “{arrival_fact}; the location can make its case.” | No confrontation asserted. |
| AA-B-07 | common | Genre tension | `{arrival_fact}` | “{arrival_fact}; the atmosphere has started counting down.” | No timer asserted. |
| AA-B-08 | common | Bureaucracy | `{arrival_fact}` | “{arrival_fact}; I have marked the beginning in my head.” | No actual record. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| AA-C-01 | common | Risk-awareness | `{arrival_fact}` | “{arrival_fact}; I will verify the surroundings before assuming anything.” | No threat assumed. |
| AA-C-02 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; there may be a quiet minute to get oriented.” | No quiet period asserted. |
| AA-C-03 | common | Gear obsession | `{arrival_fact}` | “{arrival_fact}; I will check the kit against the actual conditions.” | Intent only; no conditions named. |
| AA-C-04 | common | Genre tension | `{arrival_fact}` | “{arrival_fact}; the silence has not yet submitted evidence.” | No threat implied as fact. |
| AA-C-05 | common | Loot greed | `{arrival_fact}` | “{arrival_fact}; useful finds still need a safe reason.” | No item or search claimed. |
| AA-C-06 | common | Risk-awareness | `{arrival_fact}` | “{arrival_fact}; I will keep the known route distinct from the unknown.” | No route asserted. |
| AA-C-07 | common | Overconfidence | `{arrival_fact}` | “{arrival_fact}; preparation and caution remain compatible.” | No success claimed. |
| AA-C-08 | common | Bureaucracy | `{arrival_fact}` | “{arrival_fact}; the risk list begins with what is confirmed.” | Figurative list only. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| AA-G-01 | common | Gear obsession | `{arrival_fact}` | “{arrival_fact}; the environment is now part of the test bench.” | No actual test claimed. |
| AA-G-02 | common | Overconfidence | `{arrival_fact}` | “{arrival_fact}; I anticipated this transition in principle.” | No specific prediction asserted. |
| AA-G-03 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; the next reading may be more informative.” | No reading or result claimed. |
| AA-G-04 | common | Genre tension | `{arrival_fact}` | “{arrival_fact}; the system has switched to field conditions.” | Figurative system only. |
| AA-G-05 | common | Loot greed | `{arrival_fact}` | “{arrival_fact}; I am interested in every unclassified object.” | No objects asserted. |
| AA-G-06 | common | Gear obsession | `{arrival_fact}` | “{arrival_fact}; the map is about to become empirical.” | No map or exploration claim. |
| AA-G-07 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; perhaps the variables will introduce themselves.” | No discovery implied. |
| AA-G-08 | common | Bureaucracy | `{arrival_fact}` | “{arrival_fact}; the lab has expanded without prior notice.” | Metaphorical lab. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| AA-H-01 | common | Loot greed | `{arrival_fact}` | “{arrival_fact}; every possibility is now in range of my attention.” | No item or search claimed. |
| AA-H-02 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; something useful could be nearby.” | Possibility only. |
| AA-H-03 | common | Overconfidence | `{arrival_fact}` | “{arrival_fact}; I am already prepared to recognize value.” | No value or item claimed. |
| AA-H-04 | common | Gear obsession | `{arrival_fact}` | “{arrival_fact}; storage concerns may become relevant later.” | No capacity issue asserted. |
| AA-H-05 | common | Genre tension | `{arrival_fact}` | “{arrival_fact}; the uncollected future has entered the room.” | Figurative future only. |
| AA-H-06 | common | Loot greed | `{arrival_fact}` | “{arrival_fact}; I intend to remain curious about the surroundings.” | No action or loot asserted. |
| AA-H-07 | common | False optimism | `{arrival_fact}` | “{arrival_fact}; the best possibility may be the next one.” | Hope only. |
| AA-H-08 | common | Bureaucracy | `{arrival_fact}` | “{arrival_fact}; I have mentally opened a category for prospects.” | No actual record. |

## 8. Packing — `PackingStarted`, `PackingCompleted`

**Required committed fields:** event kind; location when present; duration when
present. **Slot:** `{packing_fact}` states that packing began or finished,
matching the kind. **Fact-check rule:** packing is not a new award, sale,
ownership transfer, or proof that cargo was extracted; do not name a value or
item unless that committed event includes it.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PK-B-01 | common | Overconfidence | `{packing_fact}` | “{packing_fact}; the backpack can keep up.” | No capacity or speed claimed. |
| PK-B-02 | common | Genre tension | `{packing_fact}` | “{packing_fact}; the raid has reached its less glamorous montage.” | Metaphor only. |
| PK-B-03 | common | False optimism | `{packing_fact}` | “{packing_fact}; the next step may involve fewer straps.” | Hope, no next-step outcome. |
| PK-B-04 | common | Gear obsession | `{packing_fact}` | “{packing_fact}; the equipment is getting its moment.” | No particular item asserted. |
| PK-B-05 | common | Loot greed | `{packing_fact}` | “{packing_fact}; I remain open to further opportunities.” | No additional loot claimed. |
| PK-B-06 | common | Overconfidence | `{packing_fact}` | “{packing_fact}; even the cargo has to wait its turn.” | No cargo delay or type asserted. |
| PK-B-07 | common | Genre tension | `{packing_fact}` | “{packing_fact}; the dramatic pause has acquired pockets.” | Metaphorical pockets. |
| PK-B-08 | common | Bureaucracy | `{packing_fact}` | “{packing_fact}; I have declared the straps temporarily relevant.” | Figurative declaration. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PK-C-01 | common | Risk-awareness | `{packing_fact}` | “{packing_fact}; I will keep track of what this event actually secures.” | No secured ownership inferred. |
| PK-C-02 | common | False optimism | `{packing_fact}` | “{packing_fact}; the next step may be straightforward.” | No next step guaranteed. |
| PK-C-03 | common | Gear obsession | `{packing_fact}` | “{packing_fact}; I will check the straps before trusting the arrangement.” | Intent only. |
| PK-C-04 | common | Genre tension | `{packing_fact}` | “{packing_fact}; even routine work has a raid around it.” | No danger level asserted. |
| PK-C-05 | common | Loot greed | `{packing_fact}` | “{packing_fact}; anything carried must still earn the risk.” | No item or decision claimed. |
| PK-C-06 | common | Risk-awareness | `{packing_fact}` | “{packing_fact}; I will not confuse packing with extraction.” | States no outcome beyond kind. |
| PK-C-07 | common | Overconfidence | `{packing_fact}` | “{packing_fact}; organization is a form of confidence.” | No success claimed. |
| PK-C-08 | common | Bureaucracy | `{packing_fact}` | “{packing_fact}; the inventory can wait for its own facts.” | No actual inventory inspection. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PK-G-01 | common | Gear obsession | `{packing_fact}` | “{packing_fact}; the storage system is now under observation.” | No system or test claimed. |
| PK-G-02 | common | Overconfidence | `{packing_fact}` | “{packing_fact}; the arrangement is probably consistent with my theory.” | Opinion only. |
| PK-G-03 | common | False optimism | `{packing_fact}` | “{packing_fact}; the next configuration may be elegant.” | No configuration result promised. |
| PK-G-04 | common | Genre tension | `{packing_fact}` | “{packing_fact}; the field test has become unusually portable.” | Metaphor, no test asserted. |
| PK-G-05 | common | Loot greed | `{packing_fact}` | “{packing_fact}; I am leaving room in the model for another component.” | No space or component claimed. |
| PK-G-06 | common | Gear obsession | `{packing_fact}` | “{packing_fact}; the straps have entered the design discussion.” | No straps altered. |
| PK-G-07 | common | False optimism | `{packing_fact}` | “{packing_fact}; perhaps the next layout will explain this one.” | Hope only. |
| PK-G-08 | common | Bureaucracy | `{packing_fact}` | “{packing_fact}; I have added a theoretical appendix.” | No actual document asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| PK-H-01 | common | Loot greed | `{packing_fact}` | “{packing_fact}; I am preserving the possibility of more.” | No more cargo claimed. |
| PK-H-02 | common | False optimism | `{packing_fact}` | “{packing_fact}; the backpack may yet surprise me.” | No capacity or item asserted. |
| PK-H-03 | common | Overconfidence | `{packing_fact}` | “{packing_fact}; I have a plan for nearly every object.” | No object or plan detail claimed. |
| PK-H-04 | common | Gear obsession | `{packing_fact}` | “{packing_fact}; storage remains a matter of personal philosophy.” | No capacity state claimed. |
| PK-H-05 | common | Genre tension | `{packing_fact}` | “{packing_fact}; time is doing that thing where it keeps moving.” | No timer assertion. |
| PK-H-06 | common | Loot greed | `{packing_fact}` | “{packing_fact}; I would like the cargo to know it is appreciated.” | No cargo or emotion asserted as fact. |
| PK-H-07 | common | False optimism | `{packing_fact}` | “{packing_fact}; surely every pocket has a future.” | Figurative; no pocket count. |
| PK-H-08 | common | Bureaucracy | `{packing_fact}` | “{packing_fact}; I have mentally reserved space for the paperwork.” | No actual paperwork or space. |

## 9. Condition warning and impact — `HazardWarning`, `ConditionImpact`

**Required committed fields:** event kind; condition/reason; location; raw
damage when recorded. **Slot:** `{condition_fact}` distinguishes a warning from
an impact and uses the committed condition/location only. **Fact-check rule:**
a warning is not an impact; an impact does not prove a specific Raider took HP
damage unless their own committed event says so. Do not repeat numeric damage in
the aside.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| CI-B-01 | common | Overconfidence | `{condition_fact}` | “{condition_fact}; I remain willing to negotiate with the terrain.” | No action or outcome claimed. |
| CI-B-02 | common | Genre tension | `{condition_fact}` | “{condition_fact}; the environment has made its intentions unusually clear.” | No intention inferred beyond event. |
| CI-B-03 | common | False optimism | `{condition_fact}` | “{condition_fact}; the next moment may be less dramatic.” | Hope only. |
| CI-B-04 | common | Gear obsession | `{condition_fact}` | “{condition_fact}; my kit has been added to the conversation.” | No gear effect asserted. |
| CI-B-05 | common | Loot greed | `{condition_fact}` | “{condition_fact}; any detour still needs a worthwhile reason.” | No detour or loot claimed. |
| CI-B-06 | common | Overconfidence | `{condition_fact}` | “{condition_fact}; I am not impressed by the floor's timing.” | No timing mechanic inferred. |
| CI-B-07 | common | Genre tension | `{condition_fact}` | “{condition_fact}; the warning has excellent dramatic timing.” | Joke; no deadline claim. |
| CI-B-08 | common | Bureaucracy | `{condition_fact}` | “{condition_fact}; I have objected to the conditions in principle.” | No actual objection submitted. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| CI-C-01 | common | Risk-awareness | `{condition_fact}` | “{condition_fact}; I will treat the warning and the impact as different facts.” | Explicitly preserves distinction. |
| CI-C-02 | common | False optimism | `{condition_fact}` | “{condition_fact}; there may be time to choose a safer position.” | No movement or time claimed. |
| CI-C-03 | common | Gear obsession | `{condition_fact}` | “{condition_fact}; I will check what protection is actually recorded.” | Intent only; no protection state. |
| CI-C-04 | common | Genre tension | `{condition_fact}` | “{condition_fact}; the surroundings have become difficult to ignore.” | No threat beyond condition event. |
| CI-C-05 | common | Loot greed | `{condition_fact}` | “{condition_fact}; further searching needs to account for this risk.” | No search decision. |
| CI-C-06 | common | Risk-awareness | `{condition_fact}` | “{condition_fact}; I will not assume everyone experienced it equally.” | No participant consequence generalized. |
| CI-C-07 | common | Overconfidence | `{condition_fact}` | “{condition_fact}; I can stay calm without calling it safe.” | No safety claim. |
| CI-C-08 | common | Bureaucracy | `{condition_fact}` | “{condition_fact}; the risk notes will contain only confirmed entries.” | Figurative notes only. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| CI-G-01 | common | Gear obsession | `{condition_fact}` | “{condition_fact}; the environment has entered the test results.” | No test claimed. |
| CI-G-02 | common | Overconfidence | `{condition_fact}` | “{condition_fact}; my model was prepared for a less literal variable.” | Opinion, no prediction record. |
| CI-G-03 | common | False optimism | `{condition_fact}` | “{condition_fact}; this may improve the next diagram.” | No diagram or improvement claimed. |
| CI-G-04 | common | Genre tension | `{condition_fact}` | “{condition_fact}; the field has supplied unsolicited feedback.” | Metaphor only. |
| CI-G-05 | common | Loot greed | `{condition_fact}` | “{condition_fact}; I am considering whether equipment can argue back.” | No item or device asserted. |
| CI-G-06 | common | Gear obsession | `{condition_fact}` | “{condition_fact}; the instruments would have opinions if present.” | Explicitly does not claim instruments. |
| CI-G-07 | common | False optimism | `{condition_fact}` | “{condition_fact}; the next reading could be less personal.” | No reading predicted. |
| CI-G-08 | common | Bureaucracy | `{condition_fact}` | “{condition_fact}; I have marked the field as difficult to reproduce.” | No experiment or record claimed. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| CI-H-01 | common | Loot greed | `{condition_fact}` | “{condition_fact}; I would prefer the surroundings left my options intact.” | No item or option state claimed. |
| CI-H-02 | common | False optimism | `{condition_fact}` | “{condition_fact}; there could still be a useful side to this.” | Hope only. |
| CI-H-03 | common | Overconfidence | `{condition_fact}` | “{condition_fact}; I have a contingency for being inconvenienced.” | No actual contingency claimed. |
| CI-H-04 | common | Gear obsession | `{condition_fact}` | “{condition_fact}; I am mentally protecting the equipment from the concept.” | No damage to equipment claimed. |
| CI-H-05 | common | Genre tension | `{condition_fact}` | “{condition_fact}; the field has become very involved in the itinerary.” | No itinerary asserted. |
| CI-H-06 | common | Loot greed | `{condition_fact}` | “{condition_fact}; I would like to keep the route open for later.” | No route decision. |
| CI-H-07 | common | False optimism | `{condition_fact}` | “{condition_fact}; perhaps the next patch of ground is friendlier.” | No next location predicted. |
| CI-H-08 | common | Bureaucracy | `{condition_fact}` | “{condition_fact}; I have placed the risk in a very small mental box.” | Figurative only. |

## 10. Robot aftermath — `RobotDefeated`, `RobotEscaped`, `RobotDefeatCargoResolved`

**Required committed fields:** event kind; robot resolution reason; cargo
resolution description and awards when present. **Slot:** `{robot_fact}`
distinguishes defeat from escape and reports cargo only from committed award
records; cargo-resolution events may explicitly say that no cargo was allocated.
**Fact-check rule:** never infer who dealt damage, who owns an award, or that an
escaped robot was defeated. Avoid naming a specific robot unless it is an
explicitly committed field.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RA-B-01 | common | Overconfidence | `{robot_fact}` | “{robot_fact}; I am prepared to take credit only where listed.” | Does not claim contribution. |
| RA-B-02 | common | Genre tension | `{robot_fact}` | “{robot_fact}; the machine has chosen an emphatic ending.” | No ending beyond resolution. |
| RA-B-03 | common | False optimism | `{robot_fact}` | “{robot_fact}; the next encounter may be less theatrical.” | No encounter promised. |
| RA-B-04 | common | Gear obsession | `{robot_fact}` | “{robot_fact}; my equipment has heard the report.” | No equipment effect asserted. |
| RA-B-05 | common | Loot greed | `{robot_fact}` | “{robot_fact}; I remain interested in any recorded cargo.” | No award claimed. |
| RA-B-06 | common | Overconfidence | `{robot_fact}` | “{robot_fact}; I can respect a result without embellishing it.” | No extra result. |
| RA-B-07 | common | Genre tension | `{robot_fact}` | “{robot_fact}; the quiet afterward is doing suspiciously good work.” | No danger inferred. |
| RA-B-08 | common | Bureaucracy | `{robot_fact}` | “{robot_fact}; the record gets the final word this time.” | No actual record process. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RA-C-01 | common | Risk-awareness | `{robot_fact}` | “{robot_fact}; I will separate the resolution from the cargo result.” | Keeps event kinds distinct. |
| RA-C-02 | common | False optimism | `{robot_fact}` | “{robot_fact}; the next stretch may be quieter.” | Hope only. |
| RA-C-03 | common | Gear obsession | `{robot_fact}` | “{robot_fact}; I will check the kit before drawing conclusions.” | No inspection or condition claimed. |
| RA-C-04 | common | Genre tension | `{robot_fact}` | “{robot_fact}; the area is allowed to remain uncertain.” | No future threat asserted. |
| RA-C-05 | common | Loot greed | `{robot_fact}` | “{robot_fact}; any cargo must have its own recorded recipient.” | Does not assert an award. |
| RA-C-06 | common | Risk-awareness | `{robot_fact}` | “{robot_fact}; I will not assume the danger has ended everywhere.” | Does not assert continued danger. |
| RA-C-07 | common | Overconfidence | `{robot_fact}` | “{robot_fact}; I can be satisfied with exactly what happened.” | No extra outcome. |
| RA-C-08 | common | Bureaucracy | `{robot_fact}` | “{robot_fact}; the evidence will not be promoted beyond its rank.” | Figurative evidence handling. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RA-G-01 | common | Gear obsession | `{robot_fact}` | “{robot_fact}; I am updating the machine-behavior diagram.” | No actual diagram. |
| RA-G-02 | common | Overconfidence | `{robot_fact}` | “{robot_fact}; my model remains technically unembarrassed.” | Opinion only. |
| RA-G-03 | common | False optimism | `{robot_fact}` | “{robot_fact}; the next sample could be easier to classify.” | No sample claimed. |
| RA-G-04 | common | Genre tension | `{robot_fact}` | “{robot_fact}; the field test has reached its ominous conclusion.” | No test asserted. |
| RA-G-05 | common | Loot greed | `{robot_fact}` | “{robot_fact}; I would like to inspect only the cargo that exists.” | No inspection or cargo asserted. |
| RA-G-06 | common | Gear obsession | `{robot_fact}` | “{robot_fact}; one more variable has become difficult to ignore.” | No variable or device claim. |
| RA-G-07 | common | False optimism | `{robot_fact}` | “{robot_fact}; perhaps the next data point will be tidier.” | Hope only. |
| RA-G-08 | common | Bureaucracy | `{robot_fact}` | “{robot_fact}; my notes have been asked to remain factual.” | No actual notes or request. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RA-H-01 | common | Loot greed | `{robot_fact}` | “{robot_fact}; I am interested in only the cargo that was awarded.” | No ownership inferred. |
| RA-H-02 | common | False optimism | `{robot_fact}` | “{robot_fact}; there may be another opportunity someday.” | No opportunity promised. |
| RA-H-03 | common | Overconfidence | `{robot_fact}` | “{robot_fact}; my interest remains independent of the result.” | No claim about loot. |
| RA-H-04 | common | Gear obsession | `{robot_fact}` | “{robot_fact}; storage planning will wait for actual cargo.” | No storage change. |
| RA-H-05 | common | Genre tension | `{robot_fact}` | “{robot_fact}; the machine's exit has left an awkward silence.” | Applies to either resolution metaphorically. |
| RA-H-06 | common | Loot greed | `{robot_fact}` | “{robot_fact}; I have not stopped appreciating possibilities.” | No possibility defined as fact. |
| RA-H-07 | common | False optimism | `{robot_fact}` | “{robot_fact}; the future may yet contain something worth keeping.” | No reward predicted as fact. |
| RA-H-08 | common | Bureaucracy | `{robot_fact}` | “{robot_fact}; I have reserved no credit that the record cannot support.” | No contribution claim. |

## 11. Recovery, knockout and pocket outcomes — `RecoveryStarted`, `RecoveryCompleted`, `Downed`, `DeadlineExpired`, `KnockedOut`, `HiddenPocketSalvaged`, `HiddenPocketEmpty`

**Required committed fields:** exact event kind; reason/source when present;
location; item and recipient/value only for a recorded salvage. **Slot:**
`{survival_fact}` states only the named transition: recovery began/completed,
downed, deadline expired, knocked out, a hidden-pocket item was salvaged, or the
pocket was empty. **Fact-check rule:** do not conflate downed with knockout,
recovery with healing, deadline expiry with a specific cause, or a pocket salvage
with survival/extraction. Item names and values require the corresponding event.

### Brawler

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RK-B-01 | common | Overconfidence | `{survival_fact}` | “{survival_fact}; I remain ready to dispute the timing.” | No recovery or counteraction claimed. |
| RK-B-02 | common | Genre tension | `{survival_fact}` | “{survival_fact}; the raid has stopped being theoretical.” | No extra consequence. |
| RK-B-03 | common | False optimism | `{survival_fact}` | “{survival_fact}; the next chapter may be kinder.” | Hope only. |
| RK-B-04 | common | Gear obsession | `{survival_fact}` | “{survival_fact}; I will ask the kit to review its role.” | Intent, no gear effect. |
| RK-B-05 | common | Loot greed | `{survival_fact}` | “{survival_fact}; the remaining possibilities still have my attention.” | No loot or continued play asserted. |
| RK-B-06 | common | Overconfidence | `{survival_fact}` | “{survival_fact}; I refuse to let the event narrate my attitude.” | No mood change claimed. |
| RK-B-07 | common | Genre tension | `{survival_fact}` | “{survival_fact}; the dramatic pause has become personal.” | Figurative only. |
| RK-B-08 | common | Bureaucracy | `{survival_fact}` | “{survival_fact}; I have lodged an imaginary appeal.” | No actual appeal or reversal. |

### Cautious

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RK-C-01 | common | Risk-awareness | `{survival_fact}` | “{survival_fact}; I will keep the transition distinct from what follows.” | No future state inferred. |
| RK-C-02 | common | False optimism | `{survival_fact}` | “{survival_fact}; a safer next step may still be available.” | Conditional, no step claimed. |
| RK-C-03 | common | Gear obsession | `{survival_fact}` | “{survival_fact}; I will rely on recorded condition, not wishful maintenance.” | No repair asserted. |
| RK-C-04 | common | Genre tension | `{survival_fact}` | “{survival_fact}; the situation has earned a careful pause.” | No timing change claimed. |
| RK-C-05 | common | Loot greed | `{survival_fact}` | “{survival_fact}; any recovered item still needs a recorded owner.” | No award asserted. |
| RK-C-06 | common | Risk-awareness | `{survival_fact}` | “{survival_fact}; I will not label this safer than it is.” | No safety estimate. |
| RK-C-07 | common | Overconfidence | `{survival_fact}` | “{survival_fact}; I can stay composed without rewriting the outcome.” | No mood or outcome change. |
| RK-C-08 | common | Bureaucracy | `{survival_fact}` | “{survival_fact}; the facts remain within their assigned boundaries.” | Figurative boundaries. |

### Gadgeteer

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RK-G-01 | common | Gear obsession | `{survival_fact}` | “{survival_fact}; the system diagram needs a human-sized annotation.” | No diagram asserted. |
| RK-G-02 | common | Overconfidence | `{survival_fact}` | “{survival_fact}; my theory can accommodate this without improving.” | Opinion, not an outcome claim. |
| RK-G-03 | common | False optimism | `{survival_fact}` | “{survival_fact}; the next reading may be more cooperative.” | No reading predicted. |
| RK-G-04 | common | Genre tension | `{survival_fact}` | “{survival_fact}; the field has produced an uncomfortable data point.” | No test or data collection asserted. |
| RK-G-05 | common | Loot greed | `{survival_fact}` | “{survival_fact}; I am curious only about items the record assigns.” | No item or award asserted. |
| RK-G-06 | common | Gear obsession | `{survival_fact}` | “{survival_fact}; the equipment model has acquired an awkward edge case.” | Figurative model only. |
| RK-G-07 | common | False optimism | `{survival_fact}` | “{survival_fact}; perhaps the next iteration will be less literal.” | No iteration promised. |
| RK-G-08 | common | Bureaucracy | `{survival_fact}` | “{survival_fact}; the technical notes have been asked to stay calm.” | No notes or request asserted. |

### Hoarder

| ID | Rarity | Mode | Slots | Candidate line | One-line fact check |
|---|---|---|---|---|---|
| RK-H-01 | common | Loot greed | `{survival_fact}` | “{survival_fact}; I am counting only the item the event actually preserves.” | No ownership beyond the event. |
| RK-H-02 | common | False optimism | `{survival_fact}` | “{survival_fact}; there may still be a use for this chapter.” | No item or recovery claimed. |
| RK-H-03 | common | Overconfidence | `{survival_fact}` | “{survival_fact}; I remain an optimist with carefully limited evidence.” | No future result promised. |
| RK-H-04 | common | Gear obsession | `{survival_fact}` | “{survival_fact}; storage plans will wait for the confirmed contents.” | No contents inferred. |
| RK-H-05 | common | Genre tension | `{survival_fact}` | “{survival_fact}; the adventure has developed an inconvenient footnote.” | Metaphor; no extra event. |
| RK-H-06 | common | Loot greed | `{survival_fact}` | “{survival_fact}; I would like to preserve whatever is actually preserved.” | No item or survival state. |
| RK-H-07 | common | False optimism | `{survival_fact}` | “{survival_fact}; the next opportunity could still be worth keeping.” | No opportunity or reward claimed. |
| RK-H-08 | common | Bureaucracy | `{survival_fact}` | “{survival_fact}; I have put the hypothetical contents on hold.” | No actual contents or procedure. |

## Human review table

Enter exactly `approve`, `edit`, or `reject` for each line; use the note column for requested changes or rationale.

| ID | Decision (approve/edit/reject) | Reviewer note |
|---|---|---|
| PC-B-01 |  |  |
| PC-B-02 |  |  |
| PC-B-03 |  |  |
| PC-B-04 |  |  |
| PC-B-05 |  |  |
| PC-B-06 |  |  |
| PC-B-07 |  |  |
| PC-B-08 |  |  |
| PC-C-01 |  |  |
| PC-C-02 |  |  |
| PC-C-03 |  |  |
| PC-C-04 |  |  |
| PC-C-05 |  |  |
| PC-C-06 |  |  |
| PC-C-07 |  |  |
| PC-C-08 |  |  |
| PC-G-01 |  |  |
| PC-G-02 |  |  |
| PC-G-03 |  |  |
| PC-G-04 |  |  |
| PC-G-05 |  |  |
| PC-G-06 |  |  |
| PC-G-07 |  |  |
| PC-G-08 |  |  |
| PC-H-01 |  |  |
| PC-H-02 |  |  |
| PC-H-03 |  |  |
| PC-H-04 |  |  |
| PC-H-05 |  |  |
| PC-H-06 |  |  |
| PC-H-07 |  |  |
| PC-H-08 |  |  |
| SH-B-01 |  |  |
| SH-B-02 |  |  |
| SH-B-03 |  |  |
| SH-B-04 |  |  |
| SH-B-05 |  |  |
| SH-B-06 |  |  |
| SH-B-07 |  |  |
| SH-B-08 |  |  |
| SH-C-01 |  |  |
| SH-C-02 |  |  |
| SH-C-03 |  |  |
| SH-C-04 |  |  |
| SH-C-05 |  |  |
| SH-C-06 |  |  |
| SH-C-07 |  |  |
| SH-C-08 |  |  |
| SH-G-01 |  |  |
| SH-G-02 |  |  |
| SH-G-03 |  |  |
| SH-G-04 |  |  |
| SH-G-05 |  |  |
| SH-G-06 |  |  |
| SH-G-07 |  |  |
| SH-G-08 |  |  |
| SH-H-01 |  |  |
| SH-H-02 |  |  |
| SH-H-03 |  |  |
| SH-H-04 |  |  |
| SH-H-05 |  |  |
| SH-H-06 |  |  |
| SH-H-07 |  |  |
| SH-H-08 |  |  |
| UD-B-01 |  |  |
| UD-B-02 |  |  |
| UD-B-03 |  |  |
| UD-B-04 |  |  |
| UD-B-05 |  |  |
| UD-B-06 |  |  |
| UD-B-07 |  |  |
| UD-B-08 |  |  |
| UD-C-01 |  |  |
| UD-C-02 |  |  |
| UD-C-03 |  |  |
| UD-C-04 |  |  |
| UD-C-05 |  |  |
| UD-C-06 |  |  |
| UD-C-07 |  |  |
| UD-C-08 |  |  |
| UD-G-01 |  |  |
| UD-G-02 |  |  |
| UD-G-03 |  |  |
| UD-G-04 |  |  |
| UD-G-05 |  |  |
| UD-G-06 |  |  |
| UD-G-07 |  |  |
| UD-G-08 |  |  |
| UD-H-01 |  |  |
| UD-H-02 |  |  |
| UD-H-03 |  |  |
| UD-H-04 |  |  |
| UD-H-05 |  |  |
| UD-H-06 |  |  |
| UD-H-07 |  |  |
| UD-H-08 |  |  |
| RE-B-01 |  |  |
| RE-B-02 |  |  |
| RE-B-03 |  |  |
| RE-B-04 |  |  |
| RE-B-05 |  |  |
| RE-B-06 |  |  |
| RE-B-07 |  |  |
| RE-B-08 |  |  |
| RE-C-01 |  |  |
| RE-C-02 |  |  |
| RE-C-03 |  |  |
| RE-C-04 |  |  |
| RE-C-05 |  |  |
| RE-C-06 |  |  |
| RE-C-07 |  |  |
| RE-C-08 |  |  |
| RE-G-01 |  |  |
| RE-G-02 |  |  |
| RE-G-03 |  |  |
| RE-G-04 |  |  |
| RE-G-05 |  |  |
| RE-G-06 |  |  |
| RE-G-07 |  |  |
| RE-G-08 |  |  |
| RE-H-01 |  |  |
| RE-H-02 |  |  |
| RE-H-03 |  |  |
| RE-H-04 |  |  |
| RE-H-05 |  |  |
| RE-H-06 |  |  |
| RE-H-07 |  |  |
| RE-H-08 |  |  |
| ED-B-01 |  |  |
| ED-B-02 |  |  |
| ED-B-03 |  |  |
| ED-B-04 |  |  |
| ED-B-05 |  |  |
| ED-B-06 |  |  |
| ED-B-07 |  |  |
| ED-B-08 |  |  |
| ED-C-01 |  |  |
| ED-C-02 |  |  |
| ED-C-03 |  |  |
| ED-C-04 |  |  |
| ED-C-05 |  |  |
| ED-C-06 |  |  |
| ED-C-07 |  |  |
| ED-C-08 |  |  |
| ED-G-01 |  |  |
| ED-G-02 |  |  |
| ED-G-03 |  |  |
| ED-G-04 |  |  |
| ED-G-05 |  |  |
| ED-G-06 |  |  |
| ED-G-07 |  |  |
| ED-G-08 |  |  |
| ED-H-01 |  |  |
| ED-H-02 |  |  |
| ED-H-03 |  |  |
| ED-H-04 |  |  |
| ED-H-05 |  |  |
| ED-H-06 |  |  |
| ED-H-07 |  |  |
| ED-H-08 |  |  |
| SR-B-01 |  |  |
| SR-B-02 |  |  |
| SR-B-03 |  |  |
| SR-B-04 |  |  |
| SR-B-05 |  |  |
| SR-B-06 |  |  |
| SR-B-07 |  |  |
| SR-B-08 |  |  |
| SR-C-01 |  |  |
| SR-C-02 |  |  |
| SR-C-03 |  |  |
| SR-C-04 |  |  |
| SR-C-05 |  |  |
| SR-C-06 |  |  |
| SR-C-07 |  |  |
| SR-C-08 |  |  |
| SR-G-01 |  |  |
| SR-G-02 |  |  |
| SR-G-03 |  |  |
| SR-G-04 |  |  |
| SR-G-05 |  |  |
| SR-G-06 |  |  |
| SR-G-07 |  |  |
| SR-G-08 |  |  |
| SR-H-01 |  |  |
| SR-H-02 |  |  |
| SR-H-03 |  |  |
| SR-H-04 |  |  |
| SR-H-05 |  |  |
| SR-H-06 |  |  |
| SR-H-07 |  |  |
| SR-H-08 |  |  |
| AA-B-01 |  |  |
| AA-B-02 |  |  |
| AA-B-03 |  |  |
| AA-B-04 |  |  |
| AA-B-05 |  |  |
| AA-B-06 |  |  |
| AA-B-07 |  |  |
| AA-B-08 |  |  |
| AA-C-01 |  |  |
| AA-C-02 |  |  |
| AA-C-03 |  |  |
| AA-C-04 |  |  |
| AA-C-05 |  |  |
| AA-C-06 |  |  |
| AA-C-07 |  |  |
| AA-C-08 |  |  |
| AA-G-01 |  |  |
| AA-G-02 |  |  |
| AA-G-03 |  |  |
| AA-G-04 |  |  |
| AA-G-05 |  |  |
| AA-G-06 |  |  |
| AA-G-07 |  |  |
| AA-G-08 |  |  |
| AA-H-01 |  |  |
| AA-H-02 |  |  |
| AA-H-03 |  |  |
| AA-H-04 |  |  |
| AA-H-05 |  |  |
| AA-H-06 |  |  |
| AA-H-07 |  |  |
| AA-H-08 |  |  |
| PK-B-01 |  |  |
| PK-B-02 |  |  |
| PK-B-03 |  |  |
| PK-B-04 |  |  |
| PK-B-05 |  |  |
| PK-B-06 |  |  |
| PK-B-07 |  |  |
| PK-B-08 |  |  |
| PK-C-01 |  |  |
| PK-C-02 |  |  |
| PK-C-03 |  |  |
| PK-C-04 |  |  |
| PK-C-05 |  |  |
| PK-C-06 |  |  |
| PK-C-07 |  |  |
| PK-C-08 |  |  |
| PK-G-01 |  |  |
| PK-G-02 |  |  |
| PK-G-03 |  |  |
| PK-G-04 |  |  |
| PK-G-05 |  |  |
| PK-G-06 |  |  |
| PK-G-07 |  |  |
| PK-G-08 |  |  |
| PK-H-01 |  |  |
| PK-H-02 |  |  |
| PK-H-03 |  |  |
| PK-H-04 |  |  |
| PK-H-05 |  |  |
| PK-H-06 |  |  |
| PK-H-07 |  |  |
| PK-H-08 |  |  |
| CI-B-01 |  |  |
| CI-B-02 |  |  |
| CI-B-03 |  |  |
| CI-B-04 |  |  |
| CI-B-05 |  |  |
| CI-B-06 |  |  |
| CI-B-07 |  |  |
| CI-B-08 |  |  |
| CI-C-01 |  |  |
| CI-C-02 |  |  |
| CI-C-03 |  |  |
| CI-C-04 |  |  |
| CI-C-05 |  |  |
| CI-C-06 |  |  |
| CI-C-07 |  |  |
| CI-C-08 |  |  |
| CI-G-01 |  |  |
| CI-G-02 |  |  |
| CI-G-03 |  |  |
| CI-G-04 |  |  |
| CI-G-05 |  |  |
| CI-G-06 |  |  |
| CI-G-07 |  |  |
| CI-G-08 |  |  |
| CI-H-01 |  |  |
| CI-H-02 |  |  |
| CI-H-03 |  |  |
| CI-H-04 |  |  |
| CI-H-05 |  |  |
| CI-H-06 |  |  |
| CI-H-07 |  |  |
| CI-H-08 |  |  |
| RA-B-01 |  |  |
| RA-B-02 |  |  |
| RA-B-03 |  |  |
| RA-B-04 |  |  |
| RA-B-05 |  |  |
| RA-B-06 |  |  |
| RA-B-07 |  |  |
| RA-B-08 |  |  |
| RA-C-01 |  |  |
| RA-C-02 |  |  |
| RA-C-03 |  |  |
| RA-C-04 |  |  |
| RA-C-05 |  |  |
| RA-C-06 |  |  |
| RA-C-07 |  |  |
| RA-C-08 |  |  |
| RA-G-01 |  |  |
| RA-G-02 |  |  |
| RA-G-03 |  |  |
| RA-G-04 |  |  |
| RA-G-05 |  |  |
| RA-G-06 |  |  |
| RA-G-07 |  |  |
| RA-G-08 |  |  |
| RA-H-01 |  |  |
| RA-H-02 |  |  |
| RA-H-03 |  |  |
| RA-H-04 |  |  |
| RA-H-05 |  |  |
| RA-H-06 |  |  |
| RA-H-07 |  |  |
| RA-H-08 |  |  |
| RK-B-01 |  |  |
| RK-B-02 |  |  |
| RK-B-03 |  |  |
| RK-B-04 |  |  |
| RK-B-05 |  |  |
| RK-B-06 |  |  |
| RK-B-07 |  |  |
| RK-B-08 |  |  |
| RK-C-01 |  |  |
| RK-C-02 |  |  |
| RK-C-03 |  |  |
| RK-C-04 |  |  |
| RK-C-05 |  |  |
| RK-C-06 |  |  |
| RK-C-07 |  |  |
| RK-C-08 |  |  |
| RK-G-01 |  |  |
| RK-G-02 |  |  |
| RK-G-03 |  |  |
| RK-G-04 |  |  |
| RK-G-05 |  |  |
| RK-G-06 |  |  |
| RK-G-07 |  |  |
| RK-G-08 |  |  |
| RK-H-01 |  |  |
| RK-H-02 |  |  |
| RK-H-03 |  |  |
| RK-H-04 |  |  |
| RK-H-05 |  |  |
| RK-H-06 |  |  |
| RK-H-07 |  |  |
| RK-H-08 |  |  |
