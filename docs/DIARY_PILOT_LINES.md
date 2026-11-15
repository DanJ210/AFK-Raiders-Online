# DIARY VARIETY: candidate asides for human review

**C4 draft: 624 candidates in 25 subfamilies; human review pending.**
No candidate is approved, catalogued, or loaded.

**C4 drafting checkpoint:** the designer's generic-line and screen rulings are
applied; lexical recounts are recorded below. Event-specificity, actual modes,
grounding and reachability still need human verification. The C3 checker and
report are historical evidence, not a semantic C4 pass. Do not load or select
these candidates without individual human approval and separate authorization.

**Lexical recount, not acceptance:** comparing the 624 current candidate
rows against C3 commit `2123251b75b9a2e02e40b97964cfefdbbe6af202`, the values
below count lines containing the specified pattern in the candidate text.
Traits are Brawler, Cautious, Gadgeteer, Hoarder, each with 156 lines.

| Screen | C3 B/C/G/H | C4 B/C/G/H |
|---|---|---|
| Initial A/An | 36/45/48/45 | 4/6/5/6 |
| Whole-word would/hypothetical/imaginary | 35/28/42/32 | 1/5/5/5 |
| Crown/trophy/medal/diamond/jewel/velvet/gold word-prefix family, including trophies | 29/3/12/35 | 0/0/0/0 |
| Curtain/drum/trumpet/brass/fanfare/ovation/fireworks/gong/megaphone/whistle word-prefix family | 17/9/13/6 | 0/0/0/0 |
| Initial `At {location},` | 26/26/30/29 | 3/0/2/4 |
| Initial My | 14/11/6/12 | 15/11/17/13 |
| Initial I, including contractions | 12/17/11/14 | 8/15/10/13 |
| Hope/luck words and “fingers crossed” (lexical subset only) | 10/10/10/12 | 0/2/0/0 |
| Broader hope-family keyword screen | 11/15/15/13 | 2/5/4/3 |
| Whole-word appetite (repetition warning, not a prop count) | 5/5/2/4 | 8/5/7/8 |
| Whole-word theory/theories (repetition warning) | 0/0/4/1 | 2/2/4/0 |
| Explicit condition/robot/item/location gates, conservative candidate count | 3/3/3/3 | 33/33/33/33 |
| Office keyword plus row flag, before manual metaphor audit | 13/12/10/13 | 17/18/18/18 |
| J/M/O shapes, by trait | 65/51/40 each | 62/50/44; 60/53/43; 60/52/44; 60/52/44 |

Labeled mode counts, ordered Gear obsession / Overconfidence / False optimism /
Genre tension / Loot greed / Bureaucracy:

| Trait | C3 labels | C4 labels |
|---|---|---|
| B | 34/34/10/34/34/10 | 34/34/10/34/34/10 |
| C | 34/34/10/33/34/11 | 34/33/11/34/33/11 |
| G | 34/34/11/34/33/10 | 34/33/12/33/34/10 |
| H | 34/34/10/34/34/10 | 34/34/10/34/34/10 |

The earlier C3 count of 77 for the royal-prop family used the original
word-prefix screen. The comparable count above is 79 because it also
normalizes `trophies` to `trophy`; this broadens the screen for both drafts.

These are labels, not the requested audited actual-mode shares.
The broader hope-family keyword screen counts candidate texts matching
`hop\w*`, `luck\w*`, `wish\w*`, `pray\w*`, fingers crossed, knock/touch wood,
fortune/fortunate, surely, hopefully, optimis*/optimist*, ideally, maybe,
perhaps, or “if all goes well”, case-insensitively with word boundaries. It
counts a line once, even for multiple matches. This intentionally overcounts
some non-hope uses, such as a wish list or optimism retreating: 54/624
(8.65%) -> 14/624 (2.24%). It is a broader lexical screen, not proof that
every implicit expression of hope has been identified.
**Individual prop-word screen:** the all-word probe below removes slots,
counts each token at most once per line, normalizes possessives and common
plurals, and prints every token appearing in more than four lines per trait
for manual classification. In C3 the highest identified concrete
prop/material word by B/C/G/H is `trophy` 20, `compass` 7, `cog` 9 and
`velvet` 13. In C4 no identified individual prop or material word exceeds
four lines per trait; examples at the ceiling include `backpack`, `barrier`,
`shield`, `pocket`, `pouch`, `bag`, `chassis` and `compartment`. Repeated
`exit` and `route` are destinations or paths, and `foot` is a body part,
not decorative props. The probe is deliberately broader than a fixed prop
dictionary, but normalization and semantic classification still require
editorial judgment; it does not determine event-related grounding.

```python c4-prop-vocabulary
import collections, pathlib, re, subprocess

before = subprocess.check_output(
    ["git", "show", "2123251b75b9a2e02e40b97964cfefdbbe6af202:docs/DIARY_PILOT_LINES.md"],
    encoding="utf-8")
after = pathlib.Path(r"docs\DIARY_PILOT_LINES.md").read_text(encoding="utf-8")
aliases = {
    "boots": "boot", "bootlaces": "bootlace", "shoelaces": "shoelace",
    "boxes": "box", "pouches": "pouch", "pockets": "pocket",
    "zippers": "zipper", "bags": "bag", "shields": "shield",
    "barriers": "barrier", "machines": "machine", "robots": "robot",
    "crates": "crate", "lockers": "locker", "compartments": "compartment",
    "components": "component", "timers": "timer", "stopwatches": "stopwatch",
    "shelves": "shelf", "socks": "sock", "bandages": "bandage",
    "paddles": "paddle", "toes": "toe", "batteries": "battery",
    "coins": "coin", "cases": "case", "crowns": "crown", "jewels": "jewel",
    "trophies": "trophy", "fuses": "fuse", "curtains": "curtain",
    "forks": "fork", "spoons": "spoon", "helmets": "helmet",
    "feet": "foot", "cogs": "cog",
}

for label, text in (("C3", before), ("C4", after)):
    print(label)
    for trait in "BCGH":
        counts = collections.Counter()
        for line in text.splitlines():
            parts = [part.strip() for part in line.strip().strip("|").split("|")]
            if len(parts) != 10 or not re.fullmatch(r"[A-Z]{2}-" + trait + r"-\d{2}", parts[0]):
                continue
            candidate = re.sub(r"\{[^}]+\}", " ", parts[4].lower())
            tokens = re.findall(r"[a-z]+(?:'[a-z]+)?", candidate)
            normalized = set()
            for token in tokens:
                token = re.sub(r"'s$", "", token)
                if token in aliases:
                    token = aliases[token]
                elif len(token) > 4 and token.endswith("ies"):
                    token = token[:-3] + "y"
                elif len(token) > 4 and re.search(r"(?:ches|shes|xes|zes|sses)$", token):
                    token = token[:-2]
                elif len(token) > 3 and token.endswith("s") and not token.endswith(("ss", "us", "is")):
                    token = token[:-1]
                normalized.add(token)
            counts.update(normalized)
        print(trait, sorted(((word, count) for word, count in counts.items() if count > 4),
                            key=lambda pair: (-pair[1], pair[0])))
```

The following standard-library probe reproduces the lexical pattern, opener,
hope, office, labeled-shape and labeled-mode counts above. Run it from the
repository root with the C3 commit available locally. It is not a semantic
mode, office-metaphor, prop or hope audit.

```python c4-pattern-probe
import collections, pathlib, re, subprocess

before = subprocess.check_output(
    ["git", "show", "2123251b75b9a2e02e40b97964cfefdbbe6af202:docs/DIARY_PILOT_LINES.md"],
    encoding="utf-8")
after = pathlib.Path(r"docs\DIARY_PILOT_LINES.md").read_text(encoding="utf-8")

def parse(text):
    rows = []
    for line in text.splitlines():
        cells = [part.strip() for part in line.strip().strip("|").split("|")]
        if len(cells) == 10 and re.fullmatch(r"[A-Z]{2}-[BCGH]-\d{2}", cells[0]):
            rows.append(dict(zip(("id", "mode", "shape", "slots", "text",
                                  "concrete", "office", "fit", "decision", "note"), cells)))
    return rows

patterns = {
    "A/An": r"^(?:a|an)\b",
    "would/hypothetical/imaginary": r"\b(?:would|hypothetical|imaginary)\b",
    "royal props": r"\b(?:crown|troph(?:y|ies)|medal|diamond|jewel|velvet|gold)\w*",
    "spectacle props": r"\b(?:curtain|drum|trumpet|brass|fanfare|ovation|firework|gong|megaphone|whistle)\w*",
    "At location": r"^at \{location\},",
    "My": r"^my\b",
    "I incl contractions": r"^i(?:\b|[’'])",
    "hope subset": r"\b(?:hop\w*|luck\w*|fingers? crossed)\b",
    "hope broader": r"\b(?:hop\w*|luck\w*|wish\w*|pray\w*|fingers? cross\w*|cross\w* (?:my )?fingers?|knock on wood|touch wood|fortune|fortunate\w*|surely|hopefully|optimis\w*|optimist\w*|ideally|maybe|perhaps|if all goes well)\b",
    "appetite": r"\bappetite\b",
    "theory/theories": r"\btheor(?:y|ies)\b",
}
office = re.compile(
    r"\b(?:office|paperwork|records?|checklist|forms?|policy|policies|editor|chapter|"
    r"paragraph|specification|labels?|file[ds]?|filing|invoice|receipts?|audit\w*|"
    r"manifest|ledger|stamps?|permits?|committee|reports?|minutes|procedure|"
    r"protocol|bureaucrac\w*|memos?|catalog(?:ue)?|department|management|budget|"
    r"requisition|application|approval|account\w*|clerk|contract|documentation|"
    r"document|meeting|agenda|spreadsheet|certificate|signature|warranty|pension|"
    r"payroll|overtime|notes?|noted|notation|diagrams?|schematics?|blueprints?|"
    r"charts?|sketch\w*|waveforms?|asterisks?|full.stop|billing)\b", re.I)
modes = ("Gear obsession", "Overconfidence", "False optimism",
         "Genre tension", "Loot greed", "Bureaucracy")

for label, text in (("C3", before), ("C4", after)):
    rows = parse(text)
    if len(rows) != 624 or len({r["id"] for r in rows}) != 624:
        raise ValueError(f"{label}: expected 624 unique rows")
    print(label)
    for trait in "BCGH":
        group = [r for r in rows if r["id"].split("-")[1] == trait]
        if len(group) != 156:
            raise ValueError(f"{label}/{trait}: expected 156 rows")
        counts = {name: sum(bool(re.search(pattern, r["text"].strip('"'), re.I))
                            for r in group) for name, pattern in patterns.items()}
        counts["office"] = sum(bool(office.search(r["text"])) or r["office"].lower() == "yes"
                               for r in group)
        shapes = collections.Counter(r["shape"] for r in group)
        labeled = collections.Counter(r["mode"] for r in group)
        print(trait, counts, "shapes", [shapes[x] for x in ("J", "M", "O")],
              "modes", [labeled[x] for x in modes])
```

**Event-related anchor probe, not a strict-concrete acceptance result:** the
same family-specific lexical screen finds 256/624 (41.03%) in C3 and
478/624 (76.60%) in this draft; by trait the counts are
71/65/63/57 -> 121/119/120/118. It ignores location-only slots, decorative
props outside each event's candidate vocabulary, and C3's unrelated empty
search display shelves. It counts used committed non-location slots, candidate
event-relevant nouns and the declared exact supply names. These are **possible
anchors**, not proof of correct gates, event truth or human-judged grounding:
the screen does not understand negation or whether a word actually refers to
the committed fact. In particular, named empty-search examples are contingent
on their unverified gate. Each C4 set reaches the numeric 60% screen below, but
manual semantic verification remains necessary before calling it a pass.
Cells give counts, not percentages; PC has 16 lines per trait, the eight-line
sets have eight, and other sets have four.

| Family | C3 B/C/G/H | C4 B/C/G/H |
|---|---|---|
| AI | 6/6/6/6 | 6/6/6/6 |
| PC | 0/0/0/0 | 10/10/10/10 |
| SH | 6/5/6/4 | 7/6/6/6 |
| UD | 4/3/3/3 | 4/4/4/3 |
| RS | 6/7/7/6 | 6/7/7/6 |
| EL | 6/5/2/2 | 7/7/6/5 |
| ES | 1/1/2/1 | 6/6/5/5 |
| SI | 6/6/8/7 | 6/6/8/7 |
| SN | 0/0/0/0 | 3/3/3/3 |
| SX | 0/0/0/0 | 3/3/3/3 |
| AR | 1/0/0/0 | 5/5/5/5 |
| AD | 0/2/0/0 | 3/4/3/3 |
| PS | 2/0/1/2 | 6/5/6/7 |
| PF | 2/0/0/0 | 5/5/7/6 |
| HW | 4/4/4/4 | 5/5/5/5 |
| HI | 4/4/4/4 | 5/5/5/5 |
| RD | 3/4/4/4 | 4/4/4/4 |
| RX | 4/4/3/2 | 4/4/4/4 |
| RC | 4/4/4/4 | 4/4/4/4 |
| RV | 0/0/0/0 | 4/3/3/3 |
| DN | 1/1/0/0 | 3/4/3/4 |
| KO | 1/0/0/0 | 4/3/3/4 |
| DL | 2/2/2/2 | 3/3/3/3 |
| HS | 4/4/4/4 | 4/4/4/4 |
| HE | 4/3/3/2 | 4/3/3/3 |

Reproduce the probe from the repository root with Python and the C3 commit
available locally:

```python c4-anchor-probe
import collections, pathlib, re, subprocess

before = subprocess.check_output(
    ["git", "show", "2123251b75b9a2e02e40b97964cfefdbbe6af202:docs/DIARY_PILOT_LINES.md"],
    encoding="utf-8")
after = pathlib.Path(r"docs\DIARY_PILOT_LINES.md").read_text(encoding="utf-8")

def parse(text):
    rows = []
    for line in text.splitlines():
        parts = [part.strip() for part in line.strip().strip("|").split("|")]
        if len(parts) == 10 and re.fullmatch(r"[A-Z]{2}-[BCGH]-\d{2}", parts[0]):
            rows.append(dict(zip(("id", "mode", "shape", "slots", "text",
                                  "concrete", "office", "fit", "decision", "note"), parts)))
    return rows

anchors = {
    "SH": r"shield|barrier|defensive rig",
    "UD": r"shield|barrier|protective layer",
    "RS": r"cushion|shock absorber|padding",
    "EL": r"exit|route|path|way (?:out|home)|boots?|feet",
    "ES": r"exit|route|path|way (?:out|home)",
    "SI": r"recovered (?:object|item)|awarded (?:object|item)",
    "SN": r"locker|shelf|lost.and.found|tool cage",
    "SX": r"locker|crate",
    "AR": r"route|trail|path|boots?|feet",
    "AD": r"boots?|feet|bootlaces?|shoelaces?",
    "PS": r"backpack|bags?|zipper|zippers|pouch(?:es)?|compartment|dividers?",
    "PF": r"backpack|bags?|zipper|zippers|pouch(?:es)?|compartment|pockets?|buckle",
    "HW": r"apology weather|overtime red|light fog",
    "HI": r"apology weather|overtime red|light fog",
    "RD": r"robot|machine|chassis|anxietick|roomba prime|passive.aggressor drone",
    "RX": r"robot|machine|chassis|anxietick|roomba prime|passive.aggressor drone",
    "RC": r"robot|machine|chassis",
    "RV": r"lungs|chest|ribs",
    "DN": r"ground|knees|feet|elbows|boots?",
    "KO": r"helmet|secret pocket",
    "DL": r"clock|stopwatch|timer|hourglass",
    "HS": r"pocket|compartment|single sock|left boot|vintage comic book",
    "HE": r"pocket|compartment",
}
slots = re.compile(r"\{(?:activity|condition|source|item|robot|opportunity)\}", re.I)
names = re.compile(r"\b(?:white bandage|blue bandage|purple bandage|panic paddles)\b", re.I)

def has_anchor(row):
    family, text = row["id"].split("-")[0], row["text"]
    if slots.search(text):
        return True
    if family == "PC" and names.search(text):
        return True
    if family == "SN" and "exact committed" not in row["fit"].lower():
        return False
    return bool(family in anchors and
                re.search(r"\b(?:" + anchors[family] + r")\b", text, re.I))

old, new = parse(before), parse(after)
for label, rows in (("C3", old), ("C4", new)):
    print(label, len(rows), sum(map(has_anchor, rows)))
    for trait in "BCGH":
        group = [r for r in rows if r["id"].split("-")[1] == trait]
        print(trait, sum(map(has_anchor, group)), "/", len(group))
for family in dict.fromkeys(r["id"].split("-")[0] for r in new):
    counts = []
    for rows in (old, new):
        counts.append([sum(has_anchor(r) for r in rows
                           if r["id"].startswith(f"{family}-{trait}-"))
                       for trait in "BCGH"])
    print(family, counts[0], "->", counts[1])
print("Needs human read, no lexical anchor:",
      ", ".join(r["id"] for r in new if not has_anchor(r)))
```

The gate count is a count of **declared candidate eligibility**, not measured
runtime reachability or implemented selection. In C3, each trait had one
explicitly gated warning, robot-defeat and preserved-item row in its gate
table. The C4 count conservatively includes, per trait: HW 01/02/05/08,
HI 01/05/08, RD 01/02/03, RX 01/02/03, HS 03, SI 02/05, PC 03/06/09/12,
AR 01/02/04/05, AD 01/02/04, RC 01/02/03/04 and SX 02/03. It excludes
the empty-search gates until their reachability is established and does not
credit PC activity reasons or the generic SX opportunity slot as named lore.
All these selectors are draft specifications, not implemented filters.

These are lexical or labeled counts, not semantic proof. The actual-mode and
shape interpretations, broader hope-family screen, semantic edge cases in the
individual prop-word screen, strict event-related concrete grounding, per-line
event-specific screen, and reachable lore gates remain unaudited. The
reselected review batch is provisional until those checks finish. `appetite`
remains frequent in Brawler and Hoarder. The keyword/flag office union also
needs manual metaphor review. No event-specific pass rate has been established;
all 624 candidates need human review, including the twelve generic-by-design
exceptions. A check sentence is not approval.

The historical checker reported 71 C2 two-per-set ordinary-stem screens and
48 per-set office screens; both screens are now retired. A recount using that
checker's `stems` and `sets` functions finds **zero** sets above three and
**zero** trait/stem pairs above eight, including the combined change-synonym
stem. This is a lexical screen, not proof of varied voice. The historical
checker still prints retired diagnostics and uses its old concrete definition;
its original output must not be presented as C4 semantic acceptance.

**Partial C3 semantic audit:** an independent, conservative editorial pass
examined AI, PC, HW, HI and HS only. The table shows provisional
pass/fail/unknown counts, including every unreviewed line as unknown; it is
not a completed baseline pass rate and requires consistency review against
the eventual C4 audit. B/C/G/H are the trait order above.

| Screen | B | C | G | H | Total |
|---|---|---|---|---|---|
| Event-specific | 17/27/112 | 20/24/112 | 19/24/113 | 19/25/112 | 75/100/449 |
| Event-related concrete | 22/22/112 | 21/23/112 | 22/22/112 | 21/23/112 | 86/90/448 |

For example, C3 `PC-B-03` compares temperament to a helmet without identifying
the personality change; `HI-B-01` could describe a warning rather than the
impact; and `HS-B-03` names the eligible item without referring to its
preservation. `HI-G-02` remains undecided on warning versus impact. The other
20 subfamilies, and the full corresponding C4 audit, remain to be adjudicated
under the same definitions before reporting comparable pass rates.

All 592 C2 IDs are retained, including seven unchanged keep lines; 32
ActivityInterrupted candidates are added. All 64 PersonalityChanged lines
were rewritten. The [C3 report](#automated-review-checks) is a historical
comparison of C2 and C3, not evidence that the present C4 draft passes. Old C2
grounding annotations are not strict evidence. No line is approved or ready
for loading.

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
`{condition}` is the committed condition; `{source}` must come from the exact
event's committed Reason or original text/thread, and `{robot}` from its exact
committed text/thread, never an opponent inferred later.
`{item}` requires an exact item **and recipient** in committed shared awards
for SI: SearchCompleted and EncounterResolved do not directly supply ItemName.
HS instead uses its directly committed preserved ItemName/instance receipt.
For SX, `{opportunity}` is the exact `OpportunityId` on that exhausted
opportunity's event, not a later lookup from world state. Its substitution and
the exact-ID gates below are not implemented.
Shared awards are not automatically the speaker's. `{weapon}` is not a generally
committed pilot field and is not authorized by these rules.

Shapes: **J** places the joke before a contextual noun slot near the end; **M**
integrates the slot into a grammatical clause; **O** uses no slot and makes an
event-specific joke or a designated generic-by-design exception. None repeats
the fact receipt. Each trait/subfamily needs
all three shapes and at least four lines. For C4, a concrete anchor must be
used in the line and related to the committed event: an event-relevant physical
object, an exact canon name supported by an eligible committed fact, or an
actually used committed non-location slot. A decorative or unrelated
hypothetical prop does not qualify just because it is physical. Location
alone never counts. Neither an ungrounded named entity nor a direction word
by itself counts. The existing Concrete cells include unaudited C3 annotations
and are not proof of C4 grounding. Decisions and reviewer notes remain blank
for human review.

The C4 event-specific screen asks what each joke says about its committed event
that a different event could not reuse. Apart from the designer-designated
generic-by-design exceptions below, reject unsupported or interchangeable
lines rather than treating a generic no-contradiction note as a pass. Those
exceptions remain subject to factual fit, gates, slots and individual human
approval; the exception alone does not approve any line. At least 15% of each trait's 156
lines must have an exact committed condition, robot, location, or item gate;
an available catalog or world projection is not itself a gate. Per trait,
each of the five designer-specified prop/opener pattern groups is capped at
8%, and each single prop word at four lines. The hoping/with-luck family is
capped at 3% of all lines. These are acceptance targets, not claims of a
completed semantic review.

The six actual modes are Gear obsession, Overconfidence, False optimism, Genre
tension, Loot greed, and Bureaucracy. Cautious practical survival understatement
must use the mode actually expressed, not a seventh “Risk-awareness” mode.
Targets for every trait/subfamily: at least 60% concrete-noun lines, no more
than `floor(n/8)` False optimism lines, and at most three lines using any
content stem, including the combined change-synonym stem. Across each trait, My and I openers are each
at most 12%, each actual mode is 6%-22%, and office/record content is at most 12%.
The designer's 2026-11-15 decision retires the **per-set** office screen:
the 12% office/record cap applies at the trait level only. One office line in
an eight-line set is 12.5%, including the unchanged keep SH-B-03; this is no
longer a per-set failure. The C2 two-per-set ordinary-content stem screen is
also retired. The retained limits are three lines per trait/subfamily set and
eight lines per trait for any content stem, including change synonyms.
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
EL-G-03, EL-B-03, EL-C-03, EL-G-06, RS-B-06, RS-H-05, RS-H-03, RS-G-06,
SI-G-04. This is separate from the human Decision and Reviewer note; it does
not approve any line for selection or loading.

### Committed lore gates and mapping flags

| Candidates | Exact committed gate | Lore use / mapping status |
|---|---|---|
| HW-B-02, HW-H-02 | Warning's Reason = `overtime_red`, with Overtime Red in its original text | Closing-time atmosphere, not an invented closing event; [field phenomena](lore/entities/field-phenomena.md), existing mapping row. |
| HW-C-02, HW-G-02 | Warning's Reason = `apology_weather`, with Apology Weather in its original text | The warning is about the named apologetic weather; same existing mapping. |
| HW-B/C/G/H-01, -05, -08 | Warning's Reason = `light_fog`, `apology_weather`, `overtime_red`, respectively, and corresponding exact condition name in the warning's original text | Conditional forecast reactions only; no impact, injury, changed visibility or actual closing is asserted. |
| HI-B/C/G/H-01, -05, -08 | Impact's Reason = `light_fog`, `apology_weather`, `overtime_red`, respectively, and corresponding exact condition name in the impact's original text | Condition-specific raw-impact reactions only; no HP injury, equipment protection or future outcome is asserted. |
| RD-B-02, RD-H-02 | Roomba Prime in this defeat's exact original text or associated committed thread | Aggressive housekeeping has stopped; [robots](lore/entities/robots.md), existing mapping. |
| RD-C-02 | Anxietick in this defeat's exact original text or associated committed thread | Nervous-machine identity; existing small-machine mapping. |
| RD-G-02 | Passive-Aggressor Drone in this defeat's exact original text or associated committed thread | Imagined disapproval at defeat; existing drone mapping. |
| RD-B-01, RD-H-01, RD-G-03 | Anxietick in this defeat's exact original text or associated committed thread | Named nervous machine only; defeat does not imply a reward. |
| RD-C-01, RD-G-01 | Roomba Prime in this defeat's exact original text or associated committed thread | Named cleaning machine only; defeat does not imply a reward. |
| RD-B-03, RD-C-03, RD-H-03 | Passive-Aggressor Drone in this defeat's exact original text or associated committed thread | Named disapproving machine only; the reactions are figurative, not asserted speech or loot. |
| RX-B-01, RX-C-02, RX-G-03, RX-H-01 | Escape-after-rounds original text, with Anxietick in that text | Nervous-machine departure only; no Raider escape or defeat. |
| RX-B-02, RX-C-01, RX-G-01, RX-H-02 | Escape-after-rounds original text, with Roomba Prime in that text | Cleaning-machine departure only; no Raider escape or defeat. |
| RX-B-03, RX-C-03, RX-G-02, RX-H-03 | Escape-after-rounds original text, with Passive-Aggressor Drone in that text | Disapproving-machine departure only; no dialogue, ownership or defeat. |
| HS-B-03, HS-C-03, HS-G-03, HS-H-03 | Preserved ItemName = Single Sock / Single Sock / Left Boot / Vintage Comic Book respectively | Exact existing items, not renamed drops or new branded items. |
| SI-B/C/G/H-02 | Visible committed award's exact item name = `Single Sock` / `Lost Diary Page` / `Half-Full Battery` / `Vintage Comic Book`, respectively, and its exact recipient is visible to the speaker | The item slot names that award; it never implies the speaker received it. |
| SI-B/C/G/H-05 | Visible committed award's exact item name = `Robot Toe` / `Left Boot` / `Broken Servo` / `Rusted Spork`, respectively, and its exact recipient is visible to the speaker | The item slot names that award; no ownership, condition change or extra award is implied. |
| PC-B/C/G/H-03, -06, -09, -12 | Exact Reason = `Applied White Bandage`, `Applied Blue Bandage`, `Used Panic Paddles`, `Applied Purple Bandage`, respectively; Mood increased in that receipt | Exact current supply names; item use and Mood change are committed to the same owner. No stock, future result or brand is inferred. |
| PC-B-04/-05/-08/-11/-13/-14; PC-C-02/-05/-07/-08/-11/-13; PC-G-02/-05/-07/-08/-11/-13; PC-H-02/-05/-07/-08/-10/-11 | Recipient's own Reason is exactly `Completed searching`, `Completed recovering`, `Completed rummaging`, `Completed salvaging`, or `Completed dismantling`; `{activity}` is the exact suffix of that reason | The receipt itself commits the completed activity and a clamped personality change, not the recipient's loot or which dimension changed. Reason parsing and substitution are not implemented. |
| AR-B/C/G/H-01, -02, -04, -05 | Exact Arrived Location = `Market`, `Salvage Yard`, `Service Depot`, `Extraction Platform`, respectively | Existing online nodes only; arrival establishes neither a find nor a transaction, service, boarding or extraction. |
| AD-B/C/G/H-01, -02, -04 | Exact kind `Departure` and Location = `Entry Tunnel` | The departure from this committed node is not an arrival or boarding; Deployment and DutyAdmission have no event Location. |
| RC-B/C/G/H-01 through -04 | Exact `RobotDefeatCargoResolved` Location = `Service Depot` | Both allocated and no-eligible-cargo resolutions remain eligible; no exact item, ownership or recipient is inferred. |
| SX-B/C/G/H-02 | Exact `OpportunityExhausted` OpportunityId = `market-locker` and Location = `Market` | The ID itself identifies the locker; exhaustion does not establish an award or a later search. |
| SX-B/C/G/H-03 | Exact `OpportunityExhausted` OpportunityId = `humming-crate` and Location = `Salvage Yard` | The ID itself names the crate; no continuing sound, award or contents are asserted. |
| SN-B-01, SN-C-03, SN-G-03, SN-H-01 | Empty-award `SearchCompleted` whose original text starts with exact completion prefix `Staycation locker search complete.` | The locker is named by this receipt, not invented; empty-award reachability remains unverified. |
| SN-B-02, SN-G-02, SN-H-04 | Empty-award `SearchCompleted` whose original text starts with exact completion prefix `Retail-shelf rummage complete.` | The shelf is named by this receipt; empty-award reachability remains unverified. |
| SN-B-03, SN-C-01 | Empty-award `SearchCompleted` whose original text starts with exact completion prefix `Lost-and-found recovery complete.` | The lost-and-found is named by this receipt; empty-award reachability remains unverified. |
| SN-C-02, SN-G-04, SN-H-02 | Empty-award `SearchCompleted` whose original text starts with exact completion prefix `Service Depot tool-cage rummage complete.` | The tool cage is named by this receipt; empty-award reachability remains unverified. |

No candidate needs a new PARODY_MAPPING row. Megaspoon Works would need one
before a future recurring named variant; it is **not used in any candidate**.
Bravado Battery, ComfortCo, Nearly Fresh, OopsAll Logistics, HushTone and Exit
Strategy Unlimited have existing mapping coverage but no matching committed
brand/item fact in these pilot kinds has been established. They are deliberately
not name-dropped. A lore zone also cannot replace a committed node such as
Market; no invented landmark is placed there. Missing exact gates exclude the
named variants, leaving generic rows or the existing narration available.
These are author-checked lore constraints, not implemented eligibility filters.

**C4 gap inventory, preliminary:** [RaidEvent](../server/AFKRaiders.Server/Domain/RaidNarrator.cs)
has neither a `ConditionName` nor a `RobotName` property. A warning commits a
condition ID in `Reason` and its name in original text; the defeat's original
text and thread carry the robot name. Gate on those exact event-bound facts,
not the current world projection. The current [danger profiles](../server/AFKRaiders.Server/Domain/RaidConditions.cs)
use Anxietick, Passive-Aggressor Drone and Roomba Prime; Receipt Printer of
Doom exists in legacy lore but is not a current danger-profile robot. A zone
is selected in world state but the pilot kinds do not themselves expose a
structured zone-name field; a node `Location` is not that zone. Shield,
resilience, generic arrival and extraction-decision receipts do not identify
branded objects, so those event kinds cannot justify brand mentions. Catalog
presence is not a committed item: SI requires a visible exact award and
recipient, HS its own preserved `ItemName`, and PC exact supply-use `Reason`.
The six mapped brands remain excluded until an eligible committed item or text
actually names them; their legacy entries alone do not satisfy that gate.
`RobotEscaped` also covers a robot overrunning all fighters or having no
remaining fighters; neither receipt says it departed. The RX departure jokes
therefore require the exact escape-after-rounds text, and require an
unimplemented event-text selector; the other outcomes retain existing
narration rather than receiving a made-up departure.
`SearchCompleted` constructs its original text by appending award text and a
final period to the completion prefix, so matching the whole event text to the
prefix would never be the specified gate. Empty-award detection itself is not
implemented and its reachability remains unverified; do not infer an empty
award merely from missing private `ItemAllocated` events.

### REVIEW THESE FIRST — reselected C4 review batch (152 IDs)

This editorial shortlist replaces the untriaged C3 list, not the candidate
pool. The designer's protected and strongest selected examples appear first;
later entries are grouped by event for comparative review rather than ranked
individually. The rest of the 624 candidates remain backlog. This is a draft
review queue, not proof that the full event-specific audit has passed. A keep
is not automatic approval.
The twelve **generic-by-design** protected analogies do not themselves distinguish the
committed event from another event: two Gadgeteer heading lines could
describe other stopping or releasing events; two other heading lines could
describe ordinary departure or travel; three shield lines lack an explicit
absorption of a hit; four resilience lines do not uniquely identify a damage
reduction; and the item-theories line could discuss an item encountered
without an award.
The designer explicitly permits these exceptions. They are eligible for
review and, after individual approval and implementation of their ordinary
event, fact and visibility gates, for selection; they are not yet approved.
No other generic line is designated by this ruling. The list is the
generic-by-design marker for each named row:

| Eight-line set | Generic-by-design IDs | Count / cap |
|---|---|---|
| SH-B | SH-B-03, SH-B-06 | 2 / 2 |
| SH-C | SH-C-06 | 1 / 2 |
| RS-C | RS-C-06 | 1 / 2 |
| RS-G | RS-G-06 | 1 / 2 |
| RS-H | RS-H-03, RS-H-05 | 2 / 2 |
| EL-B | EL-B-03 | 1 / 2 |
| EL-C | EL-C-03 | 1 / 2 |
| EL-G | EL-G-03, EL-G-06 | 2 / 2 |
| SI-G | SI-G-04 | 1 / 2 |

No set exceeds its cap; a sixteen-line set would allow four. If a future set
exceeds its applicable cap, ask the designer which line to drop rather than
silently removing protected lines.

`S` means a text slot depends on unimplemented extraction or substitution.
`G` means an unimplemented outcome, exact-event or sign selector is needed,
including on slotless PC lines and the heading/staying and positive-charge
splits. `U` marks an empty-search gate whose reachability is unverified.
`R` marks one of the protected generic-by-design lines in the table above.
The future pool itself is not implemented for **any** entry. Missing facts
must make a candidate ineligible, not guessed.

1. RS-C-03, RS-C-06 **R**, EL-G-03 **G R**, EL-B-03 **G R**, SH-C-06 **G R**,
   EL-C-03 **G R**, EL-G-06 **G R**, RS-B-06, RS-H-05 **S R**, RS-H-03 **R**,
   RS-G-06 **R**, SI-G-04 **S G R**, SH-B-03 **G R**, SH-B-06 **G R**.
2. AI-B-02 **S**, AI-C-08, AI-G-07 **S**, AI-H-06 **S**;
   SH-G-03 **G**, SH-H-06 **G**.
3. PC-B-03 **G**, PC-C-03 **G**, PC-G-09 **G**, PC-H-06 **G**.
4. HW-B-02 **S G**, HW-C-02 **S G**; RD-C-02 **S G**, RD-H-02 **S G**;
   HS-C-03 **G**, HS-G-03 **G**; SX-B-03 **G**, SX-G-03 **G**.
5. AI-B-05 **S**, AI-C-05 **S**, AI-G-06 **S**, AI-H-02 **S**.
6. PC-B-06 **G**, PC-C-06 **G**, PC-G-03 **G**, PC-H-03 **G**.
7. SH-C-03 **G**, SH-G-02 **S G**, SH-H-03 **G**.
8. UD-B-02 **S G**, UD-C-02 **S G**, UD-G-03, UD-H-02 **S G**.
9. RS-B-01 **S**, RS-C-05 **S**, RS-G-01 **S**, RS-H-01 **S**.
10. EL-B-05 **S G**, EL-C-04 **S G**, EL-G-01 **S G**, EL-H-04 **S G**.
11. ES-B-01 **S G**, ES-C-03 **G**, ES-G-03 **G**, ES-H-02 **S G**.
12. SI-B-02 **S G**, SI-C-05 **S G**, SI-G-05 **S G**, SI-H-05 **S G**.
13. SN-B-03 **G U**, SN-C-02 **S G U**, SN-G-03 **G U**, SN-H-04 **S G U**.
14. SX-B-02 **S G**, SX-C-02 **S G**, SX-H-02 **S G**, SX-H-03 **G**.
15. AR-B-02 **S G**, AR-C-07 **S**, AR-G-03, AR-H-02 **S G**.
16. AD-B-02 **S G**, AD-C-02 **S G**, AD-G-02 **S G**, AD-H-02 **S G**.
17. PS-B-04 **S**, PS-C-04 **S**, PS-G-06, PS-H-07 **S**.
18. PF-B-07 **S**, PF-C-03, PF-G-02 **S**, PF-H-02 **S**.
19. HW-B-01 **S G**, HW-C-05 **S G**, HW-G-02 **S G**, HW-H-08 **S G**.
20. HI-B-01 **S G**, HI-C-05 **S G**, HI-G-05 **S G**, HI-H-08 **S G**.
21. RD-B-02 **S G**, RD-C-01 **S G**, RD-G-01 **S G**, RD-H-03 **G**.
22. RX-B-02 **S G**, RX-C-04 **S G**, RX-G-03 **G**, RX-H-02 **S G**.
23. RC-B-02 **S G**, RC-C-03 **G**, RC-G-03 **G**, RC-H-02 **S G**.
24. RV-B-01 **S**, RV-C-01 **S**, RV-G-02 **S**, RV-H-02 **S**.
25. DN-B-04 **S G**, DN-C-03, DN-G-01 **S**, DN-H-01 **S G**.
26. KO-B-01 **S G**, KO-C-04 **S G**, KO-G-02 **S G**, KO-H-03.
27. DL-B-03, DL-C-02 **S**, DL-G-01 **S**, DL-H-01 **S**.
28. HS-B-03 **G**, HS-H-03 **G**, HS-B-02 **S**, HS-H-02 **S**.
29. HE-B-01 **S**, HE-C-03, HE-G-01 **S**, HE-H-04 **S**.
30. AI-B-01 **S**, PC-G-12 **G**, SH-G-08 **S G**, UD-C-03, RS-C-07 **S**,
    EL-C-08 **S G**, ES-H-07 **S G**, SI-B-01 **S G**, SN-G-04 **S G U**,
    SX-C-01 **S**, AR-G-04 **S G**, AD-H-01 **S G**, PS-G-04 **S**,
    PF-H-06, HW-G-05 **S G**, HI-H-03, RD-B-01 **S G**, RX-B-03 **G**,
    RC-H-03 **G**, RV-H-01 **S**, DN-C-02 **S G**.

Human approval, actual C4 acceptance checks, implementation and testing of
selection and slot dependencies, and separate runtime authorization remain
prerequisites. No candidate is loaded by this draft.

### Needs human read: event specificity

**All 624 IDs in the candidate tables below need a human read**, including
the 152-ID priority queue and the generic-by-design exceptions. No human
event-specific pass count or pass rate is claimed. The reproducible anchor
probe above flags **146 of 624** IDs without one of its lexical anchors; the
other 478 are also unverified. This is a conservative prompt for human
review, not an event-specificity classifier: a flagged line can be sound, and
a matching word can be misleading or unrelated to the event.

Flagged IDs, grouped by family:

- AI: AI-B-03, AI-B-08, AI-C-03, AI-C-08, AI-G-03, AI-G-08, AI-H-03, AI-H-08.
- PC: PC-B-01, PC-B-02, PC-B-07, PC-B-10, PC-B-15, PC-B-16, PC-C-01, PC-C-04, PC-C-10, PC-C-14, PC-C-15, PC-C-16, PC-G-01, PC-G-04, PC-G-10, PC-G-14, PC-G-15, PC-G-16, PC-H-01, PC-H-04, PC-H-13, PC-H-14, PC-H-15, PC-H-16.
- SH: SH-B-07, SH-C-01, SH-C-08, SH-G-06, SH-G-08, SH-H-01, SH-H-04.
- UD: UD-H-03.
- RS: RS-B-03, RS-B-06, RS-C-03, RS-G-06, RS-H-03, RS-H-06.
- EL: EL-B-08, EL-C-07, EL-G-03, EL-G-06, EL-H-03, EL-H-07, EL-H-08.
- ES: ES-B-02, ES-B-05, ES-C-01, ES-C-08, ES-G-05, ES-G-07, ES-G-08, ES-H-01, ES-H-03, ES-H-05.
- SI: SI-B-03, SI-B-06, SI-C-03, SI-C-06, SI-H-06.
- SN: SN-B-04, SN-C-04, SN-G-01, SN-H-03.
- SX: SX-B-04, SX-C-04, SX-G-04, SX-H-04.
- AR: AR-B-05, AR-B-06, AR-B-08, AR-C-05, AR-C-06, AR-C-08, AR-G-06, AR-G-07, AR-G-08, AR-H-05, AR-H-06, AR-H-08.
- AD: AD-B-03, AD-G-04, AD-H-03.
- PS: PS-B-05, PS-B-06, PS-C-03, PS-C-06, PS-C-08, PS-G-05, PS-G-08, PS-H-06.
- PF: PF-B-04, PF-B-05, PF-B-08, PF-C-05, PF-C-06, PF-C-08, PF-G-08, PF-H-05, PF-H-08.
- HW: HW-B-03, HW-B-06, HW-B-07, HW-C-03, HW-C-06, HW-C-07, HW-G-03, HW-G-06, HW-G-07, HW-H-03, HW-H-06, HW-H-07.
- HI: HI-B-03, HI-B-06, HI-B-07, HI-C-03, HI-C-06, HI-C-07, HI-G-03, HI-G-06, HI-G-07, HI-H-03, HI-H-06, HI-H-07.
- RV: RV-C-02, RV-G-03, RV-H-03.
- DN: DN-B-01, DN-G-03.
- KO: KO-C-03, KO-G-03.
- DL: DL-B-02, DL-C-01, DL-G-03, DL-H-04.
- HE: HE-C-04, HE-G-03, HE-H-03.

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
The activity slot extractor and this selection pool are not implemented.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-B-01 | Overconfidence | J | `{activity}` | "Even departure has to hear my opinion on interrupting {activity}." | `{activity}` | no | AI: The boast protests the exit-driven interruption of this unfinished activity. |  |  |
| AI-B-02 | Genre tension | M | `{activity}` | "Exit cut {activity} off before my enthusiasm reached the heroic part." | `{activity}` | no | AI: The imagined heroic scene is cut short with this activity, not completed. |  |  |
| AI-B-03 | Overconfidence | O | - | "Fine, leave it unfinished. I was winning the argument with it anyway." | - | no | AI: The imagined argument concerns the activity left unfinished for exit, not an actual opponent. |  |  |
| AI-B-04 | Loot greed | J | `{activity}` | "My loot-hunting ambitions were cut short along with {activity}." | `{activity}` | no | AI: The interruption ends this activity before the Raider's imagined loot hunt can continue, without claiming a find. |  |  |
| AI-B-05 | Genre tension | M | `{activity}` | "Departure cut {activity} short of the ending I expected." | `{activity}` | no | AI: The exit interruption is treated as an unsatisfying ending, not an activity result. |  |  |
| AI-B-06 | Overconfidence | J | `{activity}` | "I refuse to be intimidated by unfinished {activity}." | `{activity}` | no | AI: The boast addresses this activity's unfinished state after the interruption. |  |  |
| AI-B-07 | Genre tension | M | `{activity}` | "Apparently {activity} ends mid-sentence. Rude." | `{activity}` | no | AI: The interrupted activity, rather than a conversation, is likened to a cut-off sentence. |  |  |
| AI-B-08 | False optimism | O | - | "An unfinished activity is practically a dramatic pause, right?" | - | no | AI: The Raider optimistically reframes this interrupted activity without promising completion. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-C-01 | Genre tension | J | `{activity}` | "The exit gives an untidy ending to {activity}." | `{activity}` | no | AI: The imagined untidy ending accepts the exit interrupting this unfinished activity. |  |  |
| AI-C-02 | Gear obsession | M | `{activity}` | "My internal status display shows {activity} interrupted, not done." | `{activity}` | no | AI: The imagined display marks this interrupted activity as unfinished, not completed. |  |  |
| AI-C-03 | Overconfidence | O | - | "I can abandon an unfinished task without pretending I like it." | - | no | AI: The understated boast accepts the interruption while acknowledging the unfinished task. |  |  |
| AI-C-04 | Loot greed | J | `{activity}` | "No time to seek anything worth keeping during {activity}." | `{activity}` | no | AI: The collector laments time lost to the interruption without claiming a find. |  |  |
| AI-C-05 | Loot greed | M | `{activity}` | "The exit interrupted {activity}; I wasn't done imagining the spoils." | `{activity}` | no | AI: The Raider imagines spoils from the interrupted activity without claiming a find or later attempt. |  |  |
| AI-C-06 | Gear obsession | J | `{activity}` | "My internal task gauge reads unfinished beside {activity}." | `{activity}` | no | AI: The imagined gauge marks this activity's unfinished status, not a successful result. |  |  |
| AI-C-07 | Overconfidence | M | `{activity}` | "Abandoning {activity} mid-task doesn't require a ceremony." | `{activity}` | no | AI: The understatement concerns abandoning this activity for exit, not abandoning cargo. |  |  |
| AI-C-08 | False optimism | O | - | "Unfinished is an improvement over forgetting to leave, I suppose." | - | no | AI: The optimistic comparison relates the interrupted task to the exit decision, not a later outcome. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-G-01 | Gear obsession | J | `{activity}` | "The exit routine has overridden my interest in {activity}." | `{activity}` | no | AI: The technical metaphor describes exit interrupting this unfinished activity. |  |  |
| AI-G-02 | Genre tension | M | `{activity}` | "Heading out interrupted {activity}; the ending got cut from the script." | `{activity}` | no | AI: The imagined missing ending refers to this activity's interruption for exit. |  |  |
| AI-G-03 | Overconfidence | O | - | "I can leave a task unfinished without surrendering the right to overthink it." | - | no | AI: The boast treats the interrupted task as unfinished without promising completion. |  |  |
| AI-G-04 | Loot greed | J | `{activity}` | "No time to chase the useful parts I imagined during {activity}." | `{activity}` | no | AI: Exit interrupts this activity before the Raider can pursue imagined parts, without asserting a find. |  |  |
| AI-G-05 | Gear obsession | M | `{activity}` | "Exit has interrupted {activity}; status reads unfinished." | `{activity}` | no | AI: The status refers to this exit-interrupted activity, not equipment or a completed result. |  |  |
| AI-G-06 | Genre tension | J | `{activity}` | "The exit rewrote the finale I had planned for {activity}." | `{activity}` | no | AI: The imaginary finale describes the exit interrupting this activity, not a completed observation. |  |  |
| AI-G-07 | Overconfidence | M | `{activity}` | "I was fully prepared to overthink {activity}, but heading out dared interrupt." | `{activity}` | no | AI: The aside contrasts intended overthinking with this activity's exit interruption. |  |  |
| AI-G-08 | False optimism | O | - | "Stopping this task is obviously the ideal first step toward improving it." | - | no | AI: The optimistic opinion reframes this interrupted activity without promising its completion. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AI-H-01 | Bureaucracy | J | `{activity}` | "The exit owes my unfinished-business department one explanation: {activity}." | `{activity}` | yes | AI: The imagined department protests the activity interrupted for exit, not a completed result. |  |  |
| AI-H-02 | Genre tension | M | `{activity}` | "Exit interrupted {activity} before it could earn a proper finale." | `{activity}` | no | AI: The missing finale concerns this interrupted activity, not an item left behind. |  |  |
| AI-H-03 | Overconfidence | O | - | "My interrupted task can't outbid my talent for being offended." | - | no | AI: The figurative bid expresses offense at the interrupted activity, not a transaction. |  |  |
| AI-H-04 | Overconfidence | J | `{activity}` | "I'll dignify the interruption by complaining about {activity}." | `{activity}` | no | AI: The complaint concerns the unfinished activity interrupted for exit. |  |  |
| AI-H-05 | Overconfidence | M | `{activity}` | "Even {activity} can't compete with the size of my offended sigh." | `{activity}` | no | AI: The offended sigh reacts to this interrupted activity without claiming completion. |  |  |
| AI-H-06 | Genre tension | J | `{activity}` | "Heading out cut the collector's favorite scene short: {activity}." | `{activity}` | no | AI: The imagined scene refers to this interrupted activity, not a packed item. |  |  |
| AI-H-07 | Gear obsession | M | `{activity}` | "Departure interrupted {activity}; my inventory-minded habits object to abandoning the attempt." | `{activity}` | no | AI: Storage-minded wording refers to the activity left unfinished for exit, not owned cargo. |  |  |
| AI-H-08 | False optimism | O | - | "The unfinished task will be grateful I chose to head home." | - | no | AI: The imagined gratitude gives an optimistic spin to the activity interrupted for the exit. |  |  |

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
Every row whose fact-check cell states a direction requires that **specific
sign**: up-only or down-only. This includes slotless rows: removing a displayed
direction does not remove its unimplemented sign-extraction and selection
dependency. An opposite or unchanged value invalidates the joke. The generic
comparison rows remain eligible for either actual change.
The activity-gated variants derive the exact completed activity solely from
the recipient's committed `Reason`, not the current activity or a different
participant's thread; their reason parser and slot extractor are unimplemented.
The exact-item rows in the gate table require the recipient's own
`PersonalityChanged.Reason` to equal the indicated use string and the same
receipt to show Mood rising. [RaidSimulation.cs](../server/AFKRaiders.Server/Domain/RaidSimulation.cs)
emits those reasons on supply use; [shop-catalog.json](../server/AFKRaiders.Server/Content/shop-catalog.json)
contains the exact current names. Neither the reason-gated selector nor sign
extraction is implemented. Catalog availability alone is not eligibility.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| PC-B-01 | Overconfidence | O | - | "Appetite has found a higher gear." | - | no | PC: Greed up only; the boast concerns the increase recorded in the exact receipt. |  |  |
| PC-B-02 | Genre tension | O | - | "Good mood has temporarily misplaced its fighting words." | - | no | PC: Mood down only; the joke addresses the recorded decrease, not a fight. |  |  |
| PC-B-03 | Gear obsession | O | - | "White Bandage has done a surprisingly tidy job on my temper." | White Bandage | no | PC: Applied White Bandage and Mood up only; its use and the improved mood are the same committed change. |  |  |
| PC-B-04 | Overconfidence | J | `{activity}` | "Apparently my attitude has plenty to say about finishing {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the boast responds to that activity's committed personality change. |  |  |
| PC-B-05 | Genre tension | M | `{activity}` | "The end of {activity} has given my priorities a new argument." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the argument describes the resulting personality change, not a fight. |  |  |
| PC-B-06 | Loot greed | O | - | "Using Blue Bandage improved my mood enough to make spending it seem reasonable." | Blue Bandage | no | PC: Applied Blue Bandage and Mood up only; the used supply earns praise, not a new acquisition. |  |  |
| PC-B-07 | Overconfidence | O | - | "Even I can want less, it seems." | - | no | PC: Greed down only; the boast acknowledges the recorded reduction in desire. |  |  |
| PC-B-08 | Genre tension | M | `{activity}` | "After {activity}, even my temperament wants the last word." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the imagined last word responds to the committed change. |  |  |
| PC-B-09 | Gear obsession | O | - | "Panic Paddles have a wonderfully direct way of cheering me up." | Panic Paddles | no | PC: Used Panic Paddles and Mood up only; the used device is linked to the committed improved mood. |  |  |
| PC-B-10 | Overconfidence | O | - | "My spirits just improved, and they're hard to intimidate now." | - | no | PC: Mood up only; the boast follows the recorded increase in Mood. |  |  |
| PC-B-11 | Gear obsession | M | `{activity}` | "Finishing {activity} has my gear-minded instincts feeling different." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the gear-minded reaction concerns the resulting personality change, not an equipment action. |  |  |
| PC-B-12 | False optimism | O | - | "Purple Bandage: apparently confidence comes in adhesive form." | Purple Bandage | no | PC: Applied Purple Bandage and Mood up only; the Raider overcredits the used bandage for the better mood. |  |  |
| PC-B-13 | Loot greed | J | `{activity}` | "Even my notion of enough has a new angle after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the changed perspective follows the personality change without claiming loot or its direction. |  |  |
| PC-B-14 | Gear obsession | M | `{activity}` | "The end of {activity} has given my gear-checking instincts something to consider." | `{activity}` | no | PC: When the exact completed-activity reason is committed, gear-checking instincts react to the personality change without claiming an inspection. |  |  |
| PC-B-15 | Genre tension | O | - | "Apparently my outlook has shifted without consulting me." | - | no | PC: The complaint concerns the committed personality change, not a future outcome. |  |  |
| PC-B-16 | Bureaucracy | J | `{location}` | "An unexpected amendment has appeared in my attitude at {location}." | - | yes | PC: The imaginary amendment represents the committed personality change, not a real document. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-C-01 | Genre tension | O | - | "Optimism has quietly retreated." | - | no | PC: Mood down only; the understatement describes the decrease in the exact receipt. |  |  |
| PC-C-02 | Genre tension | M | `{activity}` | "The end of {activity} left my priorities in quiet discussion." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the imagined discussion follows its committed personality change. |  |  |
| PC-C-03 | Genre tension | O | - | "White Bandage has done more for my mood than a pep talk." | White Bandage | no | PC: Applied White Bandage and Mood up only; the joke compares the committed improvement with encouragement, not a real speaker. |  |  |
| PC-C-04 | Loot greed | O | - | "For once, wanting less feels like enough." | - | no | PC: Greed down only; the reaction follows the recorded decrease in Greed. |  |  |
| PC-C-05 | Loot greed | M | `{activity}` | "Finishing {activity} has my idea of enough reconsidering itself." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the change prompts reflection about enough without assuming a sign or dimension. |  |  |
| PC-C-06 | Loot greed | O | - | "Blue Bandage improved my mood, even if my appetite dislikes spending it." | Blue Bandage | no | PC: Applied Blue Bandage and Mood up only; the used item is valued without inventing a replacement. |  |  |
| PC-C-07 | Overconfidence | J | `{activity}` | "I can reconsider my priorities without a ceremony after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the understatement responds to its personality change, not a battle. |  |  |
| PC-C-08 | Gear obsession | M | `{activity}` | "The conclusion of {activity} has my equipment-minded instincts reconsidering things." | `{activity}` | no | PC: When the exact completed-activity reason is committed, equipment-minded instincts reflect its personality change without claiming an inspection. |  |  |
| PC-C-09 | Gear obsession | O | - | "Panic Paddles are a persuasive argument for checking the medical kit." | Panic Paddles | no | PC: Used Panic Paddles and Mood up only; the device prompts an opinion, not another supply check. |  |  |
| PC-C-10 | False optimism | O | - | "With spirits lifted, the whole raid looks suspiciously manageable." | - | no | PC: Mood up only; the qualified optimism reflects the increase without promising safety. |  |  |
| PC-C-11 | Overconfidence | M | `{activity}` | "After {activity}, I can handle a new perspective without an announcement." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the understatement reacts to the resulting personality change. |  |  |
| PC-C-12 | False optimism | O | - | "Purple Bandage has optimism sounding almost practical." | Purple Bandage | no | PC: Applied Purple Bandage and Mood up only; the improvement gives cautious optimism, not a safety guarantee. |  |  |
| PC-C-13 | Loot greed | J | `{activity}` | "A cautious collector needs time to consider that new perspective after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the collector considers the personality change without claiming a find. |  |  |
| PC-C-14 | Genre tension | M | `{location}` | "The change in my outlook at {location} has earned a cautious pause." | - | no | PC: The pause reacts to the committed change, with no direction or outcome inferred. |  |  |
| PC-C-15 | Overconfidence | O | - | "I can handle a change of outlook without demanding an explanation." | - | no | PC: The understated boast responds to a Greed or Mood change in either direction. |  |  |
| PC-C-16 | Bureaucracy | J | `{location}` | "An understated revision has arrived in my temperament at {location}." | - | yes | PC: The imaginary revision refers to the committed personality change, not an actual filing. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-G-01 | Gear obsession | O | - | "My urge to collect gear just shifted into a higher gear." | - | no | PC: Greed up only; the equipment-minded pun describes the recorded increase without claiming a find. |  |  |
| PC-G-02 | Genre tension | M | `{activity}` | "The conclusion of {activity} has rewritten my internal release notes." | `{activity}` | yes | PC: When the exact completed-activity reason is committed, the imagined notes describe its personality change, not real software. |  |  |
| PC-G-03 | Gear obsession | O | - | "White Bandage has improved both my mood and my opinion of adhesive." | White Bandage | no | PC: Applied White Bandage and Mood up only; the used material inspires an opinion, not a new test. |  |  |
| PC-G-04 | Overconfidence | O | - | "Satisfaction has exceeded its previous specification." | - | no | PC: Mood up only; the boast compares the recorded mood with its earlier value. |  |  |
| PC-G-05 | Gear obsession | M | `{activity}` | "Finishing {activity} has prompted an unexpected adjustment to my internal settings." | `{activity}` | no | PC: When the exact completed-activity reason is committed, imaginary settings represent the resulting personality change. |  |  |
| PC-G-06 | Loot greed | O | - | "Blue Bandage lifted my spirits; spending that component still feels extravagant." | Blue Bandage | no | PC: Applied Blue Bandage and Mood up only; the used supply prompts a collector's complaint, not another item. |  |  |
| PC-G-07 | Bureaucracy | J | `{activity}` | "The priorities need a new filing category after {activity}." | `{activity}` | yes | PC: When the exact completed-activity reason is committed, the imagined filing category reflects its personality change. |  |  |
| PC-G-08 | Bureaucracy | M | `{activity}` | "The conclusion of {activity} has amended my temperament's specifications." | `{activity}` | yes | PC: When the exact completed-activity reason is committed, imaginary specifications describe its personality change, not a document. |  |  |
| PC-G-09 | Overconfidence | O | - | "Panic Paddles recognized an exceptional test subject and lifted my spirits." | Panic Paddles | no | PC: Used Panic Paddles and Mood up only; the Raider boasts about the device's use, not a separate experiment. |  |  |
| PC-G-10 | Loot greed | O | - | "The urge to acquire has eased its demands." | - | no | PC: Greed down only; the collecting impulse responds to the decrease without claiming a find. |  |  |
| PC-G-11 | Genre tension | M | `{activity}` | "After {activity}, my perspective seems to have switched operating modes." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the technical metaphor describes the resulting personality change. |  |  |
| PC-G-12 | False optimism | O | - | "Purple Bandage has convinced my mood that the design is working." | Purple Bandage | no | PC: Applied Purple Bandage and Mood up only; the upbeat interpretation does not guarantee future safety. |  |  |
| PC-G-13 | Loot greed | J | `{activity}` | "Curiosity about spare bits has opinions about the new outlook after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, curiosity reacts to its personality change without claiming a part. |  |  |
| PC-G-14 | Overconfidence | M | `{location}` | "Something in my outlook at {location} has moved; I'll call that a feature." | - | no | PC: The boast describes the committed change without assuming its direction. |  |  |
| PC-G-15 | Genre tension | O | - | "The operator wasn't consulted about those new internal settings." | - | no | PC: The imagined settings change represents a real personality change, not an actual machine. |  |  |
| PC-G-16 | Loot greed | O | - | "Lower spirits have dampened my urge to collect components." | - | no | PC: Mood down only; interest in collecting reacts to the recorded decrease without claiming an item. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PC-H-01 | Loot greed | O | - | "Wanting things has become a noticeably louder pastime." | - | no | PC: Greed up only; the louder wanting describes the recorded increase, not a transaction. |  |  |
| PC-H-02 | Genre tension | M | `{activity}` | "Finishing {activity} left me with an opinion worth saving." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the new opinion reflects a personality change, not an item. |  |  |
| PC-H-03 | Loot greed | O | - | "My mood approves of spending White Bandage; my collector's pride does not." | White Bandage | no | PC: Applied White Bandage and Mood up only; the collector reluctantly values the used supply. |  |  |
| PC-H-04 | Overconfidence | O | - | "That improved temper is obviously another of my finest qualities." | - | no | PC: Mood up only; the collector's boast values the recorded improvement, not a physical treasure. |  |  |
| PC-H-05 | Loot greed | M | `{activity}` | "After {activity}, even my priorities have acquired sentimental value." | `{activity}` | no | PC: When the exact completed-activity reason is committed, sentimental value describes its personality change, not an appraisal. |  |  |
| PC-H-06 | Loot greed | O | - | "Blue Bandage made my mood better; spending it still feels extravagant." | Blue Bandage | no | PC: Applied Blue Bandage and Mood up only; the joke weighs the used supply against the committed improvement. |  |  |
| PC-H-07 | Gear obsession | J | `{activity}` | "Equipment-minded instincts have a fresh opinion after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the equipment-minded opinion reflects its personality change without claiming cargo. |  |  |
| PC-H-08 | Bureaucracy | M | `{activity}` | "The end of {activity} calls for another entry in my inventory of opinions." | `{activity}` | yes | PC: When the exact completed-activity reason is committed, the figurative inventory concerns its personality change, not possessions. |  |  |
| PC-H-09 | Gear obsession | O | - | "Panic Paddles are the rare equipment I won't begrudge using." | Panic Paddles | no | PC: Used Panic Paddles and Mood up only; the Raider approves of the used device without claiming another copy. |  |  |
| PC-H-10 | Overconfidence | J | `{activity}` | "This new perspective obviously deserves pride of place after {activity}." | `{activity}` | no | PC: When the exact completed-activity reason is committed, the boast values its personality change, not an item. |  |  |
| PC-H-11 | Loot greed | M | `{activity}` | "After {activity}, I plan to hold onto this new way of thinking." | `{activity}` | no | PC: When the exact completed-activity reason is committed, keeping a perspective describes its personality change without a physical item. |  |  |
| PC-H-12 | False optimism | O | - | "Purple Bandage has convinced me spending supplies is the secret to getting rich." | Purple Bandage | no | PC: Applied Purple Bandage and Mood up only; the upbeat belief is not a claim of actual wealth or future returns. |  |  |
| PC-H-13 | Loot greed | O | - | "Appetite has relaxed its demands, just a little." | - | no | PC: Greed down only; the line describes the recorded decrease without claiming a decision. |  |  |
| PC-H-14 | Genre tension | M | `{location}` | "At {location}, my priorities rearranged themselves without asking." | - | no | PC: The imagined rearrangement reacts to the committed change without identifying its direction. |  |  |
| PC-H-15 | Overconfidence | O | - | "I'll treasure this fresh outlook; it clearly has excellent taste in hoarders." | - | no | PC: The collecting metaphor refers to a committed personality change, not an object. |  |  |
| PC-H-16 | Loot greed | O | - | "That darker mood is not a keeper." | - | no | PC: Mood down only; the reaction to the recorded decrease does not imply item loss. |  |  |

## SH — ShieldAbsorbed

**Required:** committed kind, location, and strictly positive charge lost in the
original receipt. A zero charge loss is possible, including after resilience;
these candidates are ineligible then and existing narration remains. Charge
loss does not prove a positive HP reduction. Do not infer the attacker,
remaining charge, total protection, or HP damage. `{location}` is context; no
source is supplied by this event.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-B-01 | Bureaucracy | J | `{location}` | "That barrier just made a very assertive withdrawal at {location}." | barrier | yes | SH: A financial metaphor reacts to the barrier's charge expenditure, not an attacker or balance. |  |  |
| SH-B-02 | False optimism | M | `{location}` | "That shield bit into the hit at {location}; I call that an excellent omen." | shield | no | SH: The shield absorbs this hit; the optimistic omen claims no later safety or remaining balance. |  |  |
| SH-B-03 | Genre tension | O | - | "My shield paid the cover charge for that entrance." | shield | yes | SH: The imagined cover charge is shield charge spent on this hit, not a payment or remaining balance. |  |  |
| SH-B-04 | Gear obsession | J | `{location}` | "That defensive rig used its best manners and some power on the hit at {location}." | defensive rig | no | SH: The Raider admires the shield's charge expenditure without inferring the source. |  |  |
| SH-B-05 | Gear obsession | M | `{location}` | "The barrier at {location} is alarmingly enthusiastic about spending its power." | barrier | no | SH: Personified equipment reacts to this absorption without claiming an unshielded injury. |  |  |
| SH-B-06 | Genre tension | O | - | "That shield charge went out swinging." | shield | no | SH: The boast personifies charge spent absorbing this hit, not a separate attack. |  |  |
| SH-B-07 | Loot greed | J | `{location}` | "I'd rather spend charge on defense than part with loot at {location}." | charge | no | SH: The Raider values charge spent on defense without claiming an item was lost or saved. |  |  |
| SH-B-08 | Bureaucracy | M | `{location}` | "The barrier's output at {location} qualifies as a tactical expense." | barrier | yes | SH: A mock expense category concerns the charge already spent, not a payment or HP result. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-C-01 | Gear obsession | J | `{location}` | "Lost defensive power deserves attention at {location}." | - | no | SH: Spent shield charge prompts caution without asserting a remaining balance. |  |  |
| SH-C-02 | False optimism | M | `{location}` | "The shield at {location} spent charge; I expect that's all the excitement I'll need." | shield | no | SH: The optimistic expectation follows charge spent, without guaranteeing future safety or remaining charge. |  |  |
| SH-C-03 | Gear obsession | O | - | "Power lost from that barrier is a reminder, not an invitation to get careless." | barrier | no | SH: The Raider treats the charge expenditure as a caution rather than proof of immunity. |  |  |
| SH-C-04 | Overconfidence | J | `{location}` | "I can acknowledge the barrier's expense without panicking at {location}." | barrier | yes | SH: The calm boast follows charge spent by the barrier, not a claimed damage outcome. |  |  |
| SH-C-05 | Bureaucracy | M | `{location}` | "That shield's expense at {location} goes under 'cost of being here'." | shield | yes | SH: A mock cost category describes charge already spent, not a purchase or injury. |  |  |
| SH-C-06 | Gear obsession | O | - | "Even a good shield is an umbrella, not a roof." | shield | no | SH: The comparison treats this shield expenditure as protection without claiming immunity. |  |  |
| SH-C-07 | Loot greed | J | `{location}` | "Loot can wait; that barrier just spent charge at {location}." | barrier | no | SH: The Raider notices charge spent despite an appetite for loot, without asserting a find. |  |  |
| SH-C-08 | Genre tension | M | `{location}` | "Defensive charge spent at {location} gave my caution a dramatic entrance." | charge | no | SH: The dramatic entrance follows charge spent and guarantees no later outcome. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-G-01 | Gear obsession | J | `{location}` | "That barrier made absorbing a hit its most demanding feature at {location}." | barrier | no | SH: The feature is the barrier spending charge on this hit, without claiming a particular attacker. |  |  |
| SH-G-02 | Gear obsession | M | `{location}` | "The shield's drain at {location} leaves efficiency sounding like wishful thinking." | shield | no | SH: The equipment-minded judgment reacts to spent charge, not an actual efficiency measurement. |  |  |
| SH-G-03 | Gear obsession | O | - | "That shield just converted charge into a very persuasive argument." | shield | no | SH: The argument is this shield's spent charge, not a remaining balance or HP result. |  |  |
| SH-G-04 | Bureaucracy | J | `{location}` | "I classify that barrier expense as mandatory research at {location}." | barrier | yes | SH: The mock classification follows the barrier's expenditure without claiming a test or remaining charge. |  |  |
| SH-G-05 | False optimism | M | `{location}` | "The shield's drain on that hit at {location} surely means my power settings are perfect." | shield | no | SH: The optimistic opinion follows charge spent on this hit, not a balance or protection guarantee. |  |  |
| SH-G-06 | Genre tension | O | - | "Apparently defensive charge prefers an expensive entrance." | charge | yes | SH: The dramatic entrance is the charge expenditure, not an extra hit or actor. |  |  |
| SH-G-07 | Loot greed | J | `{location}` | "A hoarder hates seeing power leave the barrier at {location}." | barrier | no | SH: The reaction concerns the barrier's spent charge without claiming a collectible item. |  |  |
| SH-G-08 | Genre tension | M | `{location}` | "That defensive expenditure at {location} has given my power budget a dramatic pause." | - | yes | SH: The mock budget reacts to charge expenditure, not a remaining balance. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SH-H-01 | False optimism | J | `{location}` | "I'm convinced losing defensive power is all the protection I need at {location}." | - | no | SH: The optimistic belief follows charge actually spent without guaranteeing future protection or remaining charge. |  |  |
| SH-H-02 | Bureaucracy | M | `{location}` | "The barrier at {location} has submitted an energy expense for my disapproval." | barrier | yes | SH: A mock expense submission concerns spent charge, not actual paperwork or balance. |  |  |
| SH-H-03 | Loot greed | O | - | "That shield charge was a terrible thing to lose." | shield | no | SH: The collector laments charge expenditure without asserting what remains. |  |  |
| SH-H-04 | Overconfidence | J | `{location}` | "That protection was worth the charge it spent at {location}." | charge | no | SH: The boast values charge spent by the barrier, without asserting a remaining balance. |  |  |
| SH-H-05 | Gear obsession | M | `{location}` | "That shield expenditure at {location} is testing my appreciation of good equipment." | shield | no | SH: Equipment-minded reluctance follows charge expenditure without claiming damage or repair. |  |  |
| SH-H-06 | Loot greed | O | - | "Watching the barrier spend energy leaves my miserly heart wincing." | barrier | no | SH: The collector reacts to charge expenditure without claiming an item loss or remaining balance. |  |  |
| SH-H-07 | Loot greed | J | `{location}` | "Every bit of power the barrier loses feels precious at {location}." | barrier | no | SH: The appetite objects to charge spent by the barrier, without claiming a loss of loot. |  |  |
| SH-H-08 | Genre tension | M | `{location}` | "The shield at {location} has spent power with remarkable financial confidence." | shield | yes | SH: The figurative spending joke refers to charge lost, not an actual payment or balance. |  |  |

## UD — UnmitigatedDamage

**Required:** kind and committed source for J/M candidates. `{source}` must
match the original committed text/thread exactly. Resilience can reduce raw
damage to zero: no line assumes an HP wound or a later outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-B-01 | Overconfidence | J | `{source}` | "Skipping the barrier is a bold introduction from {source}." | `{source}` | no | UD: The boast responds to damage from this source taking the unshielded path, without asserting equipment or injury. |  |  |
| UD-B-02 | Genre tension | M | `{source}` | "That hit from {source} bypassed the polite layer." | `{source}` | no | UD: The shield-bypass metaphor describes this source's damage without assuming a wound or actor. |  |  |
| UD-B-03 | Genre tension | O | - | "No protective layer joined that particular argument." | - | no | UD: The unshielded damage is an imaginary argument, not a reported HP loss. |  |  |
| UD-B-04 | Bureaucracy | J | `{source}` | "The unshielded-damage complaint form names its suspect: {source}." | `{source}`, form | yes | UD: The imagined form records which source bypassed the shield without claiming an injury. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-C-01 | Bureaucracy | J | `{source}` | "The unshielded-damage form has an unwelcome entry: {source}." | `{source}`, form | yes | UD: The imagined form names the exact source whose damage bypassed the shield, not an injury. |  |  |
| UD-C-02 | Genre tension | M | `{source}` | "The defenses had no part in the damage from {source}; that's worth remembering." | `{source}` | no | UD: This source's damage used the unshielded path without establishing remaining charge or injury. |  |  |
| UD-C-03 | Gear obsession | O | - | "That unshielded hit makes an imaginary barrier sound sensible." | barrier | no | UD: The hypothetical barrier addresses the unshielded damage without claiming equipment or an HP outcome. |  |  |
| UD-C-04 | Overconfidence | J | `{source}` | "I won't mistake an uncovered hit for a warning from {source}." | `{source}` | no | UD: The understatement distinguishes this source's unshielded damage from a warning, not an injury. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| UD-G-01 | Bureaucracy | J | `{source}` | "The bypass form has an unwelcome entry: {source}." | `{source}`, form | yes | UD: The imagined form names the source of the unshielded damage without claiming injury or charge level. |  |  |
| UD-G-02 | Gear obsession | M | `{source}` | "The unshielded delivery from {source} skipped the whole protective-layer calculation." | `{source}` | no | UD: This source's damage took the unshielded path; no equipment, health or charge level is inferred. |  |  |
| UD-G-03 | Genre tension | O | - | "The damage skipped the shield step as though it wasn't in the design." | shield | no | UD: The design joke concerns the unshielded damage path, not equipment or an actual design change. |  |  |
| UD-G-04 | Overconfidence | J | `{source}` | "The unshielded hit earns my strongest technical glare, courtesy of {source}." | `{source}` | no | UD: The boast reacts to this source's unshielded damage without claiming an HP loss. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| UD-H-01 | Bureaucracy | J | `{source}` | "My shield-bypass complaint names an unwelcome source: {source}." | `{source}` | yes | UD: The imagined complaint concerns this source's unshielded damage, not a wound. |  |  |
| UD-H-02 | Genre tension | M | `{source}` | "Even {source} shouldn't get free passage for that hit past my defenses." | `{source}` | no | UD: This source's damage took the unshielded path without a claimed HP outcome. |  |  |
| UD-H-03 | Gear obsession | O | - | "That hit skipped the protective step, which seems like a poor bargain." | - | yes | UD: The unshielded damage path is called a poor bargain without inferring equipment or injury. |  |  |
| UD-H-04 | Loot greed | J | `{source}` | "My appetite objects to the unshielded hit from {source}." | `{source}` | no | UD: Appetite objects to this source's unshielded damage, not a transfer of property. |  |  |

## RS — Resilience

**Required:** kind and committed source for J/M candidates. `{source}` must
match the original committed text/thread exactly. The reduction does not
guarantee a wound, healing, or survival.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| RS-B-01 | Loot greed | J | `{source}` | "I'll treasure every bit of damage my stubbornness spared me from {source}." | `{source}` | no | RS: The Raider values the committed reduction from this source, not a physical award. |  |  |
| RS-B-02 | Bureaucracy | M | `{source}` | "For {source}, my grit filed a request for a lighter hit." | `{source}` | yes | RS: The imaginary request frames resilience reducing this source's damage, not a real filing. |  |  |
| RS-B-03 | Gear obsession | O | - | "Grit is a dented helmet for damage, not an off switch." | - | no | RS: The gear analogy explains that resilience reduced damage without guaranteeing complete prevention. |  |  |
| RS-B-04 | Overconfidence | J | `{source}` | "That reduced hit is an insult to {source}." | `{source}` | no | RS: The boast concerns reduced damage from this source without making it an actual opponent. |  |  |
| RS-B-05 | Genre tension | M | `{source}` | "The hit from {source} got a smaller role thanks to my resolve." | `{source}` | no | RS: The theatrical metaphor is resilience reducing this source's damage, not healing. |  |  |
| RS-B-06 | Loot greed | O | - | "My backbone just negotiated a discount on the damage." | - | yes | RS: The imagined discount is resilience reducing this damage, not a trade or healing. |  |  |
| RS-B-07 | False optimism | J | `{source}` | "Stubbornness calls that reduction a strong opening against {source}." | `{source}` | no | RS: The boastful optimism concerns reduced damage from this source, not a promised future effect. |  |  |
| RS-B-08 | Gear obsession | M | `{source}` | "Against {source}, resolve works like cushioning, not immunity." | `{source}` | no | RS: The protection analogy describes reducing damage from this source without claiming immunity. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-C-01 | Loot greed | J | `{source}` | "I'll take the discount on damage from {source}." | `{source}` | yes | RS: The reduction applies to this source's damage, not a find or a recovery. |  |  |
| RS-C-02 | Gear obsession | M | `{source}` | "Resolve left the damage from {source} with a little less leverage." | `{source}` | no | RS: The mechanical metaphor describes less damage from this source, not equipment use. |  |  |
| RS-C-03 | Genre tension | O | - | "Reducing a hit is not the same as declining it." | - | no | RS: The understatement distinguishes this damage reduction from canceling the hit. |  |  |
| RS-C-04 | Overconfidence | J | `{source}` | "That softened blow hardly deserves a fuss, even from {source}." | `{source}` | no | RS: The understated boast responds to reduced damage from this source without claiming survival. |  |  |
| RS-C-05 | Gear obsession | M | `{source}` | "The impact from {source} met the shock absorber in my temperament." | `{source}` | no | RS: The shock-absorber metaphor refers to resilience reducing this source's damage, not equipment. |  |  |
| RS-C-06 | Genre tension | O | - | "Resilience is a cushion, not an exemption from consequences." | cushion | no | RS: The cushion compares resilience's reduction with an impact, without promising zero damage. |  |  |
| RS-C-07 | Overconfidence | J | `{source}` | "No applause is owed for that reduced hit from {source}." | `{source}` | no | RS: The understated boast acknowledges reduced damage from this source, not a later consequence. |  |  |
| RS-C-08 | Genre tension | M | `{source}` | "Composure offered the hit from {source} a less generous reception." | `{source}` | no | RS: The reception metaphor is tied to resilience reducing this source's damage, not recovery. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| RS-G-01 | Gear obsession | J | `{source}` | "Resilience added some damping to the impact of {source}." | `{source}` | no | RS: The damping metaphor refers to the reduction of this source's damage, not installed hardware. |  |  |
| RS-G-02 | False optimism | M | `{source}` | "The smaller hit from {source} has my damping theory looking promising." | `{source}` | no | RS: The optimistic theory follows reduced damage from this source, without guaranteeing future protection. |  |  |
| RS-G-03 | Gear obsession | O | - | "Resilience is a shock absorber for damage, not an off switch." | - | no | RS: The engineering analogy distinguishes damage reduction from a guarantee of zero damage. |  |  |
| RS-G-04 | Bureaucracy | J | `{source}` | "The damage report earns a correction for reduced input from {source}." | `{source}` | yes | RS: A mock report correction reflects this source's reduced damage without asserting actual paperwork. |  |  |
| RS-G-05 | Genre tension | M | `{source}` | "The impact from {source} got a softer landing than its drama suggested." | `{source}` | no | RS: The theatrical metaphor refers to reduced damage from this source, not a recovery. |  |  |
| RS-G-06 | Gear obsession | O | - | "The damage curve just met a stubborn bend." | - | no | RS: The bend represents the committed damage reduction, not a measured curve. |  |  |
| RS-G-07 | Gear obsession | J | `{source}` | "The resilience buffer took a bite out of the hit from {source}." | `{source}` | no | RS: The imagined buffer describes resilience reducing this source's damage without claiming equipment. |  |  |
| RS-G-08 | Overconfidence | M | `{source}` | "The reduced hit from {source} gives my tolerance engineering ambitions." | `{source}` | no | RS: The boast treats the reduction of this source's damage as an engineering success, not immunity. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RS-H-01 | Loot greed | J | `{source}` | "I'll cherish every bit of damage my resilience trimmed from {source}." | `{source}` | no | RS: The collector prizes the damage reduction from this source, not an acquired object. |  |  |
| RS-H-02 | Genre tension | M | `{source}` | "My resilience left the hit from {source} gentler than its reputation." | `{source}` | no | RS: The reputation joke refers to damage reduced from this source, not a later outcome. |  |  |
| RS-H-03 | Gear obsession | O | - | "Mitigation is worth more than a suitcase full of compliments." | - | no | RS: The comparison values this damage reduction over imaginary compliments, not a physical case or healing. |  |  |
| RS-H-04 | False optimism | J | `{source}` | "I'm convinced every hit comes with a discount like the one from {source}." | `{source}` | yes | RS: The optimistic expectation follows this source's damage reduction without guaranteeing another. |  |  |
| RS-H-05 | Overconfidence | M | `{source}` | "My toughness charges {source} an outrageous entrance fee." | `{source}` | yes | RS: The fee metaphor describes resilience reducing damage from this source, not a transaction. |  |  |
| RS-H-06 | Overconfidence | O | - | "Even damage gets a smaller portion when my toughness is involved." | - | no | RS: The boast follows resilience reducing damage and does not guarantee a health outcome. |  |  |
| RS-H-07 | Genre tension | J | `{source}` | "I prize the smaller portion of damage from {source}." | `{source}` | no | RS: The Raider appreciates the reduction of this source's damage, not an acquired item. |  |  |
| RS-H-08 | Gear obsession | M | `{source}` | "The impact from {source} found the imaginary padding in my resolve." | `{source}` | no | RS: The padding analogy means this source's damage was reduced, not that equipment was used. |  |  |

## EL — ExtractionDecision, heading

**Required:** kind, committed heading decision, reason and location. The reason
and direction are already presented; neither arrival, boarding nor extraction
success follows from the decision.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| EL-B-01 | False optimism | J | `{location}` | "The easy part must be heading for the exit from {location}." | exit | no | EL: The optimistic expectation concerns the decision to head out, not an arrival or guaranteed safe journey. |  |  |
| EL-B-02 | Loot greed | M | `{location}` | "Choosing the way home from {location} gives my appetite a direction it didn't ask for." | way home | no | EL: The heading decision conflicts with the appetite for more loot, without asserting a find. |  |  |
| EL-B-03 | Gear obsession | O | - | "My boots know a dramatic direction when they hear one." | boots | no | EL: The boots personify the decision to head out, not an arrival. |  |  |
| EL-B-04 | Gear obsession | J | `{location}` | "Even my equipment-checking instincts agree to head for the exit from {location}." | exit | no | EL: Gear-checking instincts give way to the decision to head out, without a completed journey or inspection. |  |  |
| EL-B-05 | Genre tension | M | `{location}` | "The way out of {location} is less dramatic than my reasons for taking it." | way out | no | EL: The aside reacts to the chosen exit direction without asserting arrival. |  |  |
| EL-B-06 | Loot greed | O | - | "My appetite and the route home are having a serious disagreement." | route | no | EL: The decision to head out conflicts with the appetite for loot, without asserting cargo. |  |  |
| EL-B-07 | Bureaucracy | J | `{location}` | "The exit plan has one entry: leave {location}." | exit | yes | EL: The committed decision is to head out from this location, not stay or arrive. |  |  |
| EL-B-08 | Genre tension | M | `{location}` | "Heading out from {location} is a plot twist my pride didn't rehearse." | exit | no | EL: The plot twist is choosing to head out, not completing extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-C-01 | False optimism | J | `{location}` | "Choosing a route home counts as a promising start from {location}." | route | no | EL: The chosen route is described as a start, not a successful extraction. |  |  |
| EL-C-02 | Gear obsession | M | `{location}` | "The route from {location} has convinced my navigation theory to point toward extraction." | route | no | EL: The imagined theory reacts to heading out without guaranteeing navigation. |  |  |
| EL-C-03 | Genre tension | O | - | "Leaving is the part of the plan that requires actual feet." | feet | no | EL: The joke contrasts the exit decision with the travel it requires, without claiming movement has occurred. |  |  |
| EL-C-04 | Loot greed | J | `{location}` | "More loot sounds tempting, but the way out makes a better argument at {location}." | way out | no | EL: The exit decision wins over interest in more loot without asserting an item. |  |  |
| EL-C-05 | Overconfidence | M | `{location}` | "From {location}, I can choose a way home without pretending the trip is over." | - | no | EL: Choosing the direction is distinguished from reaching the destination. |  |  |
| EL-C-06 | Gear obsession | O | - | "Even my navigation theory agrees: the route home comes first." | route | no | EL: The imagined theory reflects choosing a route toward extraction, not its safety or completion. |  |  |
| EL-C-07 | Loot greed | J | `{location}` | "There may be more to find, but I'm heading out from {location}." | - | no | EL: The possibility of further finds is weighed against the decision to head out, not an actual find. |  |  |
| EL-C-08 | Loot greed | M | `{location}` | "The exit from {location} has temporarily outranked my interest in more loot." | exit | no | EL: Heading out is contrasted with the desire for more loot, with no find asserted. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-G-01 | Genre tension | M | `{location}` | "The path from {location} is the first sensible line in my exit algorithm." | path | no | EL: The imagined algorithm describes heading toward extraction, not arrival. |  |  |
| EL-G-02 | False optimism | M | `{location}` | "The route home from {location} looks like a reasonably simple problem to solve." | route | no | EL: The optimistic opinion concerns the chosen exit route and promises no arrival. |  |  |
| EL-G-03 | Gear obsession | O | - | "Even a prototype needs an off switch." | - | no | EL: The prototype analogy frames the choice to head out as stopping, not an actual equipment shutdown. |  |  |
| EL-G-04 | Overconfidence | J | `{location}` | "I've solved the exit route, starting with {location}." | route | no | EL: The boast concerns choosing a route out, not successful travel. |  |  |
| EL-G-05 | Overconfidence | M | `{location}` | "The route from {location} is obviously my finest calculation." | route | no | EL: The boast concerns the decision to head out, without claiming the trip is complete. |  |  |
| EL-G-06 | Gear obsession | O | - | "A well-designed lever also knows when to release." | - | no | EL: The release analogy reflects choosing to leave, not operating a physical lever. |  |  |
| EL-G-07 | Loot greed | J | `{location}` | "The way out has no patience for my craving to find scrap at {location}." | way out | no | EL: The decision to head out interrupts imagined scrap-seeking without asserting a find. |  |  |
| EL-G-08 | Overconfidence | M | `{location}` | "From {location}, I can at least calculate a path to extraction." | path | no | EL: The path is a heading decision, not a completed journey. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| EL-H-01 | False optimism | J | `{location}` | "Perhaps choosing the exit counts as preserving options at {location}." | exit | no | EL: The optimistic valuation concerns the decision to head out, not arrival. |  |  |
| EL-H-02 | Gear obsession | M | `{location}` | "The exit from {location} has my packing habit asking for an extension." | exit | no | EL: The request is a thought about heading out, not a change to any deadline. |  |  |
| EL-H-03 | Gear obsession | O | - | "Choosing to head out has my acquisitive streak in revolt." | - | no | EL: Storage-minded reluctance responds to heading for extraction, not a transfer or loss. |  |  |
| EL-H-04 | Overconfidence | J | `{location}` | "A strategic retreat, not surrender: that's the way home from {location}." | way home | no | EL: The Raider has chosen the exit, not completed the journey. |  |  |
| EL-H-05 | Genre tension | M | `{location}` | "The route out of {location} is the ending my appetite tried to rewrite." | route | no | EL: The imagined ending concerns the chosen route out, not a completed trip or abandoned find. |  |  |
| EL-H-06 | Overconfidence | O | - | "The way out won that argument; I reserve the right to miss the possibilities." | - | no | EL: The decision is to head out despite reluctance, not proof of extraction. |  |  |
| EL-H-07 | Loot greed | J | `{location}` | "The decision to leave means giving up the possibility of finding more at {location}." | - | no | EL: The decision trades future search possibilities for an exit; no find or arrival is claimed. |  |  |
| EL-H-08 | Gear obsession | M | `{location}` | "At {location}, my acquisitive streak has lost the argument with heading home." | - | no | EL: Storage-minded reluctance responds to heading out, not a transfer or completed departure. |  |  |

## ES — ExtractionDecision, staying

**Required:** kind, committed staying decision, reason and location. Staying
means now, not a permanent refusal to extract or a promise of future loot.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| ES-B-01 | Loot greed | J | `{location}` | "For now, the exit route lost the vote to one more look at {location}." | exit route | no | ES: The Raider prefers another potential look to leaving now without claiming a find or a permanent stay. |  |  |
| ES-B-02 | Genre tension | M | `{location}` | "Staying at {location} gives my impressive attitude a better ending than leaving." | - | no | ES: The Raider jokes about choosing to stay instead of heading out now. |  |  |
| ES-B-03 | Overconfidence | O | - | "The exit hasn't earned my attention just yet." | exit | no | ES: The boast concerns not heading out now, not a promise to stay forever. |  |  |
| ES-B-04 | Gear obsession | J | `{location}` | "The compass in my head refuses to point along the route home from {location}." | route home | no | ES: The figurative compass reflects staying now, not a broken device or a changed route. |  |  |
| ES-B-05 | False optimism | M | `{location}` | "Staying at {location} sounds like the confident chapter of this story." | - | no | ES: The optimistic interpretation is of the decision to stay now, not a predicted reward. |  |  |
| ES-B-06 | Loot greed | O | - | "The prospect of loot can make an exit feel premature." | exit | no | ES: Loot appetite colors the choice to stay, without asserting an acquisition. |  |  |
| ES-B-07 | Bureaucracy | J | `{location}` | "The route home is temporarily marked NOT YET at {location}." | route home | yes | ES: The imaginary notation delays the exit for now, not permanently or on a real document. |  |  |
| ES-B-08 | Genre tension | M | `{location}` | "The way home from {location} will have to tolerate my dramatic hesitation." | - | no | ES: The hesitation is the decision to stay, without guaranteeing a later departure. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-C-01 | False optimism | J | `{location}` | "For now, patience seems a respectable reason to stay at {location}." | - | no | ES: The optimistic opinion concerns staying now and does not promise a safe outcome. |  |  |
| ES-C-02 | Gear obsession | M | `{location}` | "The compass in my head has put the homeward route from {location} on hold." | compass | no | ES: The figurative compass describes choosing to stay, not a changed route or deadline. |  |  |
| ES-C-03 | Overconfidence | O | - | "Postponing the exit takes patience, not a promise of safety." | - | no | ES: The cautious boast concerns staying now and explicitly avoids a safety promise. |  |  |
| ES-C-04 | Genre tension | J | `{location}` | "The way home will have to tolerate a little indecision at {location}." | way home | no | ES: The decision is to stay for now, not a permanent rejection of extraction. |  |  |
| ES-C-05 | Gear obsession | M | `{location}` | "My mental route from {location} leaves room for not departing yet." | route | no | ES: The imagined route reflects the decision to stay, not a new movement. |  |  |
| ES-C-06 | Loot greed | O | - | "Staying hasn't made the exit any less important than loot." | exit | no | ES: The Raider balances the current stay against the exit, without claiming loot was found. |  |  |
| ES-C-07 | Overconfidence | J | `{location}` | "I can postpone the exit without losing my bearings at {location}." | exit | no | ES: The boast applies to this decision to stay, not to an unlimited delay. |  |  |
| ES-C-08 | Bureaucracy | M | `{location}` | "The stay at {location} gets a provisional approval, not a lifetime permit." | - | yes | ES: The mock approval is temporary, matching the current stay rather than promising a permanent one. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-G-01 | Genre tension | J | `{location}` | "The idea of another experiment has put my return route on hold at {location}." | return route | no | ES: An imagined experiment colors staying now, without changing the deadline or guaranteeing an experiment. |  |  |
| ES-G-02 | False optimism | M | `{location}` | "The exit route from {location} can wait; my curiosity insists that's a sound plan." | exit route | no | ES: The optimistic opinion justifies staying for now without claiming a discovery or later outcome. |  |  |
| ES-G-03 | Genre tension | O | - | "The exit can wait while my curiosity edits the ending." | exit | no | ES: The imagined edit refers to staying now, not an actual change to a later outcome. |  |  |
| ES-G-04 | Bureaucracy | J | `{location}` | "The route home has acquired a provisional pause at {location}." | route home | yes | ES: The provisional plan captures the decision to stay without promising how long. |  |  |
| ES-G-05 | Overconfidence | M | `{location}` | "Staying at {location} is obviously the elegant solution. For now." | - | no | ES: The boast applies only to the present decision to stay, not its eventual result. |  |  |
| ES-G-06 | Gear obsession | O | - | "The exit algorithm just paused instead of heading home." | exit | no | ES: The algorithm metaphor describes choosing not to extract now, not a real mechanism. |  |  |
| ES-G-07 | Loot greed | J | `{location}` | "The possibility of useful scrap leaves staying preferable to leaving {location}." | - | no | ES: Speculation about scrap colors the stay without asserting any item or award. |  |  |
| ES-G-08 | Overconfidence | M | `{location}` | "At {location}, I confidently calculate that leaving can wait." | - | no | ES: The confident calculation expresses this stay, not a guarantee of later reconsideration. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| ES-H-01 | False optimism | J | `{location}` | "Staying is surely an investment in possibilities at {location}." | - | yes | ES: The optimistic valuation describes staying now, not a promised item or transaction. |  |  |
| ES-H-02 | Genre tension | M | `{location}` | "The route home from {location} sounded reasonable, but I chose more time here." | route | no | ES: The contrast concerns choosing to stay now, without asserting a find. |  |  |
| ES-H-03 | Loot greed | O | - | "I'm staying so the possibilities can have a little longer. For now." | - | no | ES: Staying leaves opportunities open for now, without asserting a particular find or deadline. |  |  |
| ES-H-04 | Gear obsession | J | `{location}` | "My acquisitive streak has temporarily vetoed the route home from {location}." | route | no | ES: Storage-minded reluctance describes staying now, not packing or possessed items. |  |  |
| ES-H-05 | Overconfidence | M | `{location}` | "Staying at {location} is an excellent decision. Probably." | - | no | ES: The boast concerns staying now and does not invent the decision's actual reason. |  |  |
| ES-H-06 | Bureaucracy | O | - | "The exit plan has a space for further reconsideration." | exit | yes | ES: The mock plan makes the stay provisional rather than permanent. |  |  |
| ES-H-07 | Genre tension | J | `{location}` | "The exit can wait; there are possibilities worth exploring at {location}." | exit | no | ES: The current decision to stay preserves possibilities without asserting a find. |  |  |
| ES-H-08 | Gear obsession | M | `{location}` | "My appetite at {location} has voted against the exit." | exit | no | ES: Storage-minded reluctance reflects staying now without claiming any actual contents. |  |  |

## SI — Search or encounter, recovered

**Required:** SearchCompleted or EncounterResolved, location, and committed
shared awards demonstrating at least one recipient received an item. `{item}`
is eligible only when the exact item and recipient appear in that committed
awards text; never assume the recipient is the speaker. Every J/M candidate
requires that exact `{item}`; O candidates use no item slot and assert no owner.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SI-B-01 | Loot greed | J | `{item}` | "Now that's a find with attitude: {item}." | `{item}` | no | SI: This names the awarded find, without claiming it belongs to the speaker. |  |  |
| SI-B-02 | Loot greed | J | `{item}` | "An awarded half-pair has my full attention: {item}." | `{item}` | no | SI: Single Sock only; the awarded name inspires a pairing joke without claiming another sock or ownership. |  |  |
| SI-B-03 | Genre tension | O | - | "A recovered prize deserves an entrance worthy of a champion." | - | no | SI: The imagined entrance celebrates an awarded find without identifying its owner or value. |  |  |
| SI-B-04 | Loot greed | J | `{item}` | "My restraint looks ready to brawl over this award: {item}." | `{item}` | no | SI: The visible award provokes figurative conflict with restraint, not a fight or transfer to the speaker. |  |  |
| SI-B-05 | Loot greed | M | `{item}` | "The award of {item} is one small step for my bragging." | `{item}` | no | SI: Robot Toe only; the awarded mechanical toe inspires a footstep pun without implying ownership. |  |  |
| SI-B-06 | Overconfidence | O | - | "Even the loot gets to look tough after a find." | - | no | SI: The awarded object's imagined toughness reacts to the find, not a combat result. |  |  |
| SI-B-07 | Gear obsession | J | `{item}` | "This find raises practical-use questions about {item}." | `{item}` | no | SI: This questions the awarded item's uses without claiming ownership or testing it. |  |  |
| SI-B-08 | Loot greed | M | `{item}` | "I'd nominate newly recovered {item} for the title of loudest treasure." | `{item}` | no | SI: The figurative title celebrates this visible award without a transfer or actual sound. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SI-C-01 | Loot greed | J | `{item}` | "A find worth a quiet nod: {item}." | `{item}` | no | SI: The nod values this visible awarded item without implying its recipient. |  |  |
| SI-C-02 | False optimism | M | `{item}` | "The award of {item} makes me optimistic about unfinished stories." | `{item}` | no | SI: Lost Diary Page only; its name suggests a story without asserting its contents or the speaker's ownership. |  |  |
| SI-C-03 | Loot greed | O | - | "Even a recovered treasure is no substitute for an exit." | treasure | no | SI: The awarded find prompts a reminder that recovery is not extraction, without naming an owner. |  |  |
| SI-C-04 | Overconfidence | J | `{item}` | "A measured cheer for this discovery: {item}." | `{item}` | no | SI: The restrained cheer responds to this visible awarded item, not an extraction outcome. |  |  |
| SI-C-05 | Loot greed | M | `{item}` | "Seeing {item} awarded, I'll resist announcing a whole pair." | `{item}` | no | SI: Left Boot only; the restraint concerns one visible award without claiming another boot exists or is absent. |  |  |
| SI-C-06 | Gear obsession | O | - | "An awarded find raises equipment questions, not a need for a victory dance." | - | no | SI: The awarded item prompts equipment questions without asserting its usefulness or ownership. |  |  |
| SI-C-07 | Loot greed | J | `{item}` | "I'll value this find after a closer look at {item}." | `{item}` | no | SI: The awarded item prompts caution about its value; no appraisal or ownership is claimed. |  |  |
| SI-C-08 | Overconfidence | M | `{item}` | "My enthusiasm for the discovery of {item} has learned to use its indoor voice." | `{item}` | no | SI: The imagined quiet voice tempers enthusiasm for this visible award, not the surroundings. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-G-01 | Gear obsession | J | `{item}` | "The discovery raises technical questions about {item}." | `{item}` | no | SI: The awarded item inspires questions without an inspection or ownership claim. |  |  |
| SI-G-02 | False optimism | M | `{item}` | "The award of {item} makes my optimism exactly half full." | `{item}` | no | SI: Half-Full Battery only; the name inspires the joke without claiming charge measurements or ownership. |  |  |
| SI-G-03 | Genre tension | O | - | "Engineering is calling that recovered object a cliffhanger." | - | no | SI: The awarded object's imagined engineering mystery implies no further experiment. |  |  |
| SI-G-04 | Overconfidence | J | `{item}` | "I already have three incompatible theories about {item}." | `{item}` | no | SI: The visible awarded item prompts incompatible theories, not an inspection or ownership claim. |  |  |
| SI-G-05 | Gear obsession | M | `{item}` | "The award of {item} has me wondering why it broke." | `{item}` | no | SI: Broken Servo only; the item's name prompts speculation, not an actual repair or ownership. |  |  |
| SI-G-06 | Loot greed | O | - | "Every recovered object deserves a technical biography." | - | no | SI: The imagined biography responds to an awarded find without inventing its recipient or value. |  |  |
| SI-G-07 | Genre tension | J | `{item}` | "That find deserves a more dramatic origin story than I can invent: {item}." | `{item}` | no | SI: The awarded item prompts an imagined story, not a use or transfer. |  |  |
| SI-G-08 | Loot greed | M | `{item}` | "Seeing {item} awarded makes me wonder what else might be worth examining." | `{item}` | no | SI: The visible awarded item inspires speculation without claiming its ownership or contents. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SI-H-01 | Loot greed | J | `{item}` | "Not every find gets such an appreciative sigh: {item}." | `{item}` | no | SI: The awarded item is admired without assigning it to the speaker. |  |  |
| SI-H-02 | False optimism | M | `{item}` | "The award of {item} convinces me every hoarder's story ends happily." | `{item}` | no | SI: Vintage Comic Book only; the imagined happy ending claims neither ownership nor story details. |  |  |
| SI-H-03 | Loot greed | O | - | "A recovered object is my favorite argument against moderation." | - | no | SI: The collector uses an awarded find to argue against moderation without claiming its identity or owner. |  |  |
| SI-H-04 | Gear obsession | M | `{item}` | "Even seeing {item} awarded makes my packing instincts sit up and pay attention." | `{item}` | no | SI: The awarded item triggers a packing-minded reaction, not a transfer or ownership claim. |  |  |
| SI-H-05 | Loot greed | M | `{item}` | "Finding {item} means my appetite can argue with a fork and spoon at once." | `{item}` | no | SI: Rusted Spork only; the comparison admires its combined utensils without claiming ownership. |  |  |
| SI-H-06 | Loot greed | O | - | "Beside that find, restraint looks terribly underdressed." | - | no | SI: The clothing metaphor describes the collector's reaction to an awarded find, not a transfer. |  |  |
| SI-H-07 | Genre tension | J | `{item}` | "Restraint is looking nervous over this award: {item}." | `{item}` | no | SI: The visible award tempts the Raider without implying a transfer to the speaker. |  |  |
| SI-H-08 | Overconfidence | M | `{item}` | "Even newly recovered {item} cannot compete with my talent for admiration." | `{item}` | no | SI: The boast admires this visible awarded item without asserting ownership. |  |  |

## SN — Search or encounter, nothing recovered

**Required for these revised rows:** SearchCompleted, location, and committed
evidence that no item was recovered. Named variants additionally require the
exact event-bound completion text in the gate table; selection is not
implemented. Current generation normally creates positive draw counts, and
the observed resolution path allocates available items to present participants:
an empty-award SearchCompleted has not been demonstrated. Do not count these
as reachable until tested. These search-specific jokes are not
eligible for EncounterResolved; that outcome needs separately drafted lines
or the existing narration. Do not invent a hidden find, recipient, subsequent
search, or item loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-B-01 | Overconfidence | J | `{location}` | "Even the Staycation locker can't compete with my talent for finding nothing at {location}." | Staycation locker | no | SN: The boast concerns an empty search of the exact committed locker, not an item. |  |  |
| SN-B-02 | Gear obsession | M | `{location}` | "The retail shelf at {location} offered my equipment theories exactly nothing." | shelf | no | SN: Theories receive nothing from the empty search of the exact committed retail shelf. |  |  |
| SN-B-03 | Loot greed | O | - | "The lost-and-found lived up to half its name; I found nothing." | lost-and-found | no | SN: The exact committed lost-and-found search's empty result plays on its name without claiming an item. |  |  |
| SN-B-04 | Bureaucracy | J | `{location}` | "The empty search deserves a big zero on my imaginary manifest at {location}." | manifest | yes | SN: The imagined record states the empty search result, not an actual filing or later find. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-C-01 | Bureaucracy | J | `{location}` | "The empty lost-and-found result deserves one quiet entry in my mental notebook at {location}." | notebook | yes | SN: The imagined record describes the exact committed lost-and-found search's empty result without claiming a real document. |  |  |
| SN-C-02 | Gear obsession | M | `{location}` | "The tool cage at {location} gave me nothing to fit into a pouch." | tool cage | no | SN: The exact committed tool-cage search yielded nothing, without implying a tool, pouch contents or ownership. |  |  |
| SN-C-03 | Loot greed | O | - | "That Staycation locker gave my collecting ambitions the day off: nothing found." | Staycation locker | no | SN: The exact committed locker's empty search is contrasted with collecting ambitions, not a claim of rest or a find. |  |  |
| SN-C-04 | Overconfidence | J | `{location}` | "I can acknowledge an empty search without inventing a find at {location}." | - | no | SN: The understated boast accepts this search's empty result. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-G-01 | Genre tension | J | `{location}` | "Even my curiosity has nothing to take apart after that search at {location}." | - | no | SN: The empty search gives the Raider no object to examine or disassemble. |  |  |
| SN-G-02 | Gear obsession | M | `{location}` | "That retail shelf at {location} gives my imagined workbench nothing to inspect." | shelf | no | SN: The imagined bench has nothing from the empty search of the exact committed retail shelf. |  |  |
| SN-G-03 | Bureaucracy | O | - | "The Staycation locker supplied exactly zero for my mental search report." | Staycation locker | yes | SN: The imagined report records the exact committed locker's empty search, not an actual document. |  |  |
| SN-G-04 | Loot greed | J | `{location}` | "The tool cage left my collector's imagination zero specimens to analyze at {location}." | tool cage | no | SN: The exact committed tool-cage search yielded nothing to analyze, not a tool or actual test. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SN-H-01 | Overconfidence | J | `{location}` | "The Staycation locker failed to intimidate me with an empty result at {location}." | Staycation locker | no | SN: The boast reacts to the exact committed locker's empty search without claiming a find or later attempt. |  |  |
| SN-H-02 | Genre tension | M | `{location}` | "The tool cage at {location} yielded nothing to tempt my appetite." | tool cage | no | SN: The exact committed tool-cage search yielded nothing without claiming a tool or ownership. |  |  |
| SN-H-03 | Gear obsession | O | - | "An empty search gives a pouch nothing to celebrate." | pouch | no | SN: The hypothetical pouch emphasizes that the search added no item. |  |  |
| SN-H-04 | Loot greed | J | `{location}` | "The retail shelf left my counting skills embarrassingly unemployed at {location}." | retail shelf | no | SN: The exact committed retail-shelf search yielded nothing to count, not a claim of ownership. |  |  |

## SX — OpportunityExhausted

**Required:** kind and location. The opportunity is exhausted; no finding,
award, owner, or subsequent opportunity follows from this event. J rows
using `{opportunity}` also require this receipt's nonempty `OpportunityId`;
its exact-ID substitution and the locker/crate selectors are not implemented.
Named locker/crate rows require the exact matching ID and location, not a
container inferred from the node or an earlier event.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-B-01 | Overconfidence | J | `{opportunity}` | "That spent chance isn't afraid of my glare: {opportunity}." | `{opportunity}` | no | SX: The committed opportunity ID names the chance already exhausted, not a successful retry. |  |  |
| SX-B-02 | Bureaucracy | M | `{location}` | "The locker at {location} has filed its last chance to impress me." | locker | yes | SX: `market-locker` at Market only; the imagined filing refers to its exhausted opportunity, not a document or award. |  |  |
| SX-B-03 | Loot greed | O | - | "The humming crate's chance ran out before my appetite did." | crate | no | SX: `humming-crate` at Salvage Yard only; the exhausted chance disappoints without implying an item or ongoing sound. |  |  |
| SX-B-04 | Overconfidence | J | `{location}` | "Even my best entrance can't rewind that opportunity at {location}." | - | no | SX: The boast acknowledges that the opportunity was already exhausted. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| SX-C-01 | Loot greed | J | `{opportunity}` | "The cautious collector arrived too late for this chance: {opportunity}." | `{opportunity}` | no | SX: The committed opportunity ID identifies the already-exhausted chance, not an item or owner. |  |  |
| SX-C-02 | Genre tension | M | `{location}` | "That locker at {location} is past the point of being worth a search." | locker | no | SX: `market-locker` at Market only; the spent opportunity discourages a search without asserting an item. |  |  |
| SX-C-03 | Bureaucracy | O | - | "The humming crate's chance belongs under finished, not postponed." | crate | yes | SX: `humming-crate` at Salvage Yard only; the imagined classification distinguishes exhaustion from delay, not actual paperwork. |  |  |
| SX-C-04 | Overconfidence | J | `{location}` | "I can accept that the door has closed on this opportunity at {location}." | door | no | SX: The closed door represents the spent chance, not an actual door or another attempt. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-G-01 | Bureaucracy | J | `{opportunity}` | "My imaginary status report says this chance is exhausted: {opportunity}." | `{opportunity}` | yes | SX: The committed opportunity ID identifies the exhausted chance; the report is imaginary. |  |  |
| SX-G-02 | Gear obsession | M | `{location}` | "The locker at {location} is past troubleshooting; its opportunity is spent." | locker | no | SX: `market-locker` at Market only; the technical reaction concerns its exhausted opportunity, not a device failure. |  |  |
| SX-G-03 | Overconfidence | O | - | "The humming crate's chance is exhausted; my brilliant curiosity isn't." | crate | no | SX: `humming-crate` at Salvage Yard only; the boast concerns its exhausted chance, not a machine or discovery. |  |  |
| SX-G-04 | Loot greed | J | `{location}` | "Even my curiosity can't restart that spent opportunity at {location}." | - | no | SX: Curiosity meets an already-exhausted chance without claiming a component. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| SX-H-01 | Loot greed | J | `{opportunity}` | "I can't pocket another chance from this exhausted opportunity: {opportunity}." | `{opportunity}` | no | SX: The committed opportunity ID identifies the spent chance; the collecting metaphor claims no item. |  |  |
| SX-H-02 | Genre tension | M | `{location}` | "The locker at {location} has no more opportunity to offer me." | locker | no | SX: `market-locker` at Market only; its exhausted opportunity offers no inference about an item or recipient. |  |  |
| SX-H-03 | Bureaucracy | O | - | "The humming crate's chance is filed under things I didn't get to keep." | crate | yes | SX: `humming-crate` at Salvage Yard only; the imagined filing concerns its exhausted chance, not an owned item. |  |  |
| SX-H-04 | Overconfidence | J | `{location}` | "Even persistence won't reopen that opportunity at {location}." | - | no | SX: The boast meets an exhausted chance, without implying another attempt succeeded. |  |  |

## AR — Arrived

**Required:** Arrived and committed location. Arrival is not evidence of
subsequent travel, a search, a find, or an encounter.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-B-01 | False optimism | J | `{location}` | "The Market must be ready for my bargaining now that the route delivered me to {location}." | route | no | AR: Market only; arrival inspires an optimistic expectation, not a transaction or an identified trader. |  |  |
| AR-B-02 | Loot greed | M | `{location}` | "The path to {location} has left my salvage instincts with unreasonable expectations." | path | no | AR: Salvage Yard only; the completed journey excites appetite without asserting a find. |  |  |
| AR-B-03 | Gear obsession | O | - | "Those boots consider arriving a personal triumph." | boots | no | AR: Personified boots celebrate reaching a location, not a subsequent action. |  |  |
| AR-B-04 | Gear obsession | J | `{location}` | "My navigation theory wants credit for the path ending at the depot: {location}." | path | no | AR: Service Depot only; the theory claims credit for the completed journey, not a repair. |  |  |
| AR-B-05 | Genre tension | M | `{location}` | "Reaching {location} is an entrance, not a boarding pass." | boarding pass | yes | AR: Extraction Platform only; the figurative pass distinguishes arrival from boarding or extraction. |  |  |
| AR-B-06 | Loot greed | O | - | "Arrival has me counting treasures I haven't seen." | - | no | AR: Imagined treasures respond to reaching the location without claiming a find. |  |  |
| AR-B-07 | Loot greed | J | `{location}` | "My appetite followed the route all the way to {location}." | route | no | AR: Appetite accompanies the completed route without claiming a search or find. |  |  |
| AR-B-08 | Genre tension | M | `{location}` | "Arriving at {location} has given my confidence a change of scenery." | - | no | AR: The scenery change is the arrival itself, not evidence of another event. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-C-01 | False optimism | J | `{location}` | "This Market visit is bound to be uneventful after my route to {location}." | route | no | AR: Market only; the completed journey sparks an optimistic expectation without asserting a purchase. |  |  |
| AR-C-02 | Loot greed | M | `{location}` | "The path to {location} has me wondering what, if anything, is worth keeping." | path | no | AR: Salvage Yard only; curiosity about possible salvage follows the journey without claiming a find. |  |  |
| AR-C-03 | Overconfidence | O | - | "Those boots arrived without demanding a celebration." | boots | no | AR: The understated boast personifies footwear after arrival, not a later action. |  |  |
| AR-C-04 | Gear obsession | J | `{location}` | "My mental map marks where these feet arrived at the depot: {location}." | feet | no | AR: Service Depot only; the completed journey reaches the depot, not a repair. |  |  |
| AR-C-05 | Genre tension | M | `{location}` | "Reaching {location} isn't a boarding pass." | boarding pass | yes | AR: Extraction Platform only; the figurative pass distinguishes arrival from boarding. |  |  |
| AR-C-06 | Loot greed | O | - | "Arrival puts bearings before my appetite for once." | - | no | AR: The Raider puts orientation before appetite after arriving, without claiming a subsequent find. |  |  |
| AR-C-07 | Overconfidence | J | `{location}` | "No speech required for a route ending at {location}." | route | no | AR: The understatement concerns the completed route to this location, not a search. |  |  |
| AR-C-08 | Bureaucracy | M | `{location}` | "Arrival at {location} is the entire entry in today's travel log." | - | yes | AR: The figurative log records only arrival, without inventing a further event. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-G-01 | Genre tension | J | `{location}` | "The path delivered me to the opening scene of a shopping episode: {location}." | path | no | AR: Market only; arrival begins an imagined episode without claiming a purchase. |  |  |
| AR-G-02 | Gear obsession | M | `{location}` | "The path to {location} brought my engineering curiosity ahead of my judgment." | path | no | AR: Salvage Yard only; curiosity follows the completed journey without claiming a find. |  |  |
| AR-G-03 | Overconfidence | O | - | "Arrival confirms my theory that the route leads somewhere." | route | no | AR: The deliberately modest theory follows the completed journey, not another discovery. |  |  |
| AR-G-04 | False optimism | J | `{location}` | "These feet reached the depot, which my mental map calls promising: {location}." | feet | no | AR: Service Depot only; optimism follows the completed journey without implying a repair. |  |  |
| AR-G-05 | Bureaucracy | M | `{location}` | "The route to {location} is complete; boarding needs a separate entry." | route | yes | AR: Extraction Platform only; the mock entry distinguishes arrival from boarding. |  |  |
| AR-G-06 | Loot greed | O | - | "Arrival has my imagination rummaging before the rest of me starts." | - | no | AR: Imagined rummaging reacts to arrival without claiming an actual search or find. |  |  |
| AR-G-07 | Loot greed | J | `{location}` | "The trip has given my acquisitive streak new equations to solve at {location}." | - | no | AR: Imagined equations respond to the journey without claiming a find. |  |  |
| AR-G-08 | Overconfidence | M | `{location}` | "Reaching {location} has given my navigational calculations undeserved confidence." | - | no | AR: The boast reacts to reaching the location, not a measurement or search. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AR-H-01 | False optimism | J | `{location}` | "My acquisitive streak predicts excellent possibilities at the end of the route to {location}." | route | no | AR: Market only; the completed route inspires an optimistic prediction without asserting a purchase. |  |  |
| AR-H-02 | Genre tension | M | `{location}` | "The path to {location} has inflated my expectations beyond the evidence." | path | no | AR: Salvage Yard only; expectations follow the completed journey without asserting a find. |  |  |
| AR-H-03 | Loot greed | O | - | "My boots have brought my souvenir instincts to a fresh arrival." | boots | no | AR: The personified journey inspires souvenir instincts without claiming an acquisition. |  |  |
| AR-H-04 | Gear obsession | J | `{location}` | "The path's end earns a place on the map in my head: {location}." | path | no | AR: Service Depot only; the completed journey reaches the depot, not a service or item. |  |  |
| AR-H-05 | Overconfidence | M | `{location}` | "Reaching {location} deserves a boarding pass for my confidence, if not for me." | boarding pass | yes | AR: Extraction Platform only; the imaginary pass celebrates arrival without claiming boarding. |  |  |
| AR-H-06 | Loot greed | O | - | "At a fresh arrival, my hunger unwraps the prospects." | - | no | AR: The appetite responds to arrival without asserting a find. |  |  |
| AR-H-07 | Genre tension | J | `{location}` | "The route is behind me, but the story doesn't end upon reaching {location}." | route | no | AR: The completed journey is framed as an arrival rather than a later outcome or extraction. |  |  |
| AR-H-08 | Bureaucracy | M | `{location}` | "Arrival at {location} has earned its own entry in my mental travel log." | - | yes | AR: The figurative travel record concerns arrival, not an item or purchase. |  |  |

## AD — DutyAdmission, Departure or Deployment

**Required:** one of the three exact kinds. `Departure` commits its Entry
Tunnel location; `Deployment` and `DutyAdmission` do not commit a location in
their event receipts. Consequently location-slot lines below are ineligible
for those two kinds unless an exact event-bound location is separately
available to the recipient; no current extractor supplies one. Slotless lines
can cover all three. No line promises boarding, arrival elsewhere, or extraction.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| AD-B-01 | Overconfidence | J | `{location}` | "Even my boots are making an entrance by leaving {location}." | boots | no | AD: Departure from Entry Tunnel only; the boot metaphor celebrates leaving, not arrival. |  |  |
| AD-B-02 | Gear obsession | M | `{location}` | "Leaving {location} is the first round in my argument with my bootlaces." | bootlaces | no | AD: Departure from Entry Tunnel only; the imagined laces turn departure into a gear joke, not a fight. |  |  |
| AD-B-03 | Gear obsession | O | - | "Even my interest in equipment is ready for the start of duty." | - | no | AD: Gear-minded enthusiasm responds to beginning duty without implying equipment ownership. |  |  |
| AD-B-04 | Overconfidence | M | `{location}` | "My feet are leaving {location}; my confidence has already arrived." | feet | no | AD: Departure from Entry Tunnel only; the imagined arrival is confidence, not the Raider. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-C-01 | Bureaucracy | J | `{location}` | "The checklist in my head says my feet can leave {location}." | feet | yes | AD: Departure from Entry Tunnel only; the imaginary list concerns leaving, not a later arrival. |  |  |
| AD-C-02 | Overconfidence | M | `{location}` | "Leaving {location} is a job for careful feet, not a victory parade." | feet | no | AD: Departure from Entry Tunnel only; the understatement concerns the start of travel, not success. |  |  |
| AD-C-03 | Gear obsession | O | - | "Beginning duty is easier when my shoelaces don't receive emergency status." | shoelaces | no | AD: Imagined preparation suits admission or departure without claiming a particular equipment action. |  |  |
| AD-C-04 | Loot greed | M | `{location}` | "The boots leaving {location} have my appetite's cautious approval." | boots | no | AD: Departure from Entry Tunnel only; appetite approves leaving without claiming a find. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-G-01 | Gear obsession | J | `{location}` | "My feet and my calculations agree on one thing: leaving {location}." | feet | no | AD: Departure from Entry Tunnel only; the imagined agreement concerns leaving, not arrival. |  |  |
| AD-G-02 | Bureaucracy | M | `{location}` | "Those boots leaving {location} get the first entry on my mental travel checklist." | boots | yes | AD: Departure from Entry Tunnel only; the imaginary checklist concerns leaving, not a completed trip. |  |  |
| AD-G-03 | Overconfidence | O | - | "My feet can begin duty without consulting my calculations." | feet | no | AD: The boast concerns admission or departure without implying equipment or a later result. |  |  |
| AD-G-04 | Loot greed | J | `{location}` | "My appetite for experiments wants to come along from {location}." | - | no | AD: Departure from Entry Tunnel only; curiosity accompanies leaving without claiming a find or experiment. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| AD-H-01 | Overconfidence | J | `{location}` | "Even my feet know a hoarder can make an entrance by leaving {location}." | feet | no | AD: Departure from Entry Tunnel only; the boast celebrates leaving without claiming inventory. |  |  |
| AD-H-02 | Genre tension | M | `{location}` | "Leaving {location} gives my boots an opening scene worth remembering." | boots | no | AD: Departure from Entry Tunnel only; the imagined scene celebrates leaving, not an arrival. |  |  |
| AD-H-03 | Loot greed | O | - | "Beginning duty makes my hands unusually ambitious about keeping things." | hands | no | AD: The collector's desire follows duty admission or departure without asserting a find. |  |  |
| AD-H-04 | Gear obsession | J | `{location}` | "My feet have permission from my packing habit to leave {location}." | feet | no | AD: Departure from Entry Tunnel only; the imagined permission implies no contents. |  |  |

## PS — PackingStarted

**Required:** PackingStarted and location. The event starts a packing activity
for every participant, including one who received no item and has an empty
backpack. Do not infer cargo, a new award, completed packing, departure, or
secured extraction. Existing candidate rows still need this factual audit.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-B-01 | False optimism | J | `{location}` | "Surely the backpack and I can settle our differences at {location}." | backpack | no | PS: The hopeful negotiation concerns packing just begun, not an actual dispute or completed result. |  |  |
| PS-B-02 | Loot greed | M | `{location}` | "The packing at {location} brings out my unreasonable desire for a bigger bag." | bag | no | PS: The desired bag responds to packing starting; no item or added capacity is claimed. |  |  |
| PS-B-03 | Gear obsession | O | - | "Even the zipper gets my most intimidating voice as stowing begins." | zipper | no | PS: The imagined fastener meets the start of packing, not a completed arrangement or contents. |  |  |
| PS-B-04 | Loot greed | J | `{location}` | "The bag is being packed; greed wants a say about every inch at {location}." | bag | no | PS: Greed reacts to packing beginning without asserting the bag contains cargo. |  |  |
| PS-B-05 | Overconfidence | M | `{location}` | "The organizing technique at {location} has all the grace of my bear hug." | - | no | PS: The boast compares the newly started packing with a bear hug, not a completed result. |  |  |
| PS-B-06 | Genre tension | O | - | "Packing earns its own dramatic montage, whether it likes it or not." | - | no | PS: The imagined montage starts with the packing activity and asserts no completed action. |  |  |
| PS-B-07 | Gear obsession | J | `{location}` | "The backpack meets my strongest stowing opinions at {location}." | backpack | no | PS: The opinion concerns the packing just begun, not a strap state or contents. |  |  |
| PS-B-08 | Gear obsession | M | `{location}` | "Stowing at {location} is an excuse to argue with a zipper." | zipper | no | PS: The imagined fastener concerns packing starting, not a finished arrangement. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-C-01 | Gear obsession | J | `{location}` | "As the packing begins, my backpack deserves patience at {location}." | backpack | no | PS: The caution concerns packing just started, without asserting a completed arrangement. |  |  |
| PS-C-02 | False optimism | M | `{location}` | "Stowing at {location} is a good time to believe in orderly zippers." | zippers | no | PS: Hope about a packing fastener is figurative and claims neither equipment condition nor completion. |  |  |
| PS-C-03 | Genre tension | O | - | "Packing rewards patience more reliably than a heroic speech." | - | no | PS: The comparison responds to packing beginning, not an outcome or item. |  |  |
| PS-C-04 | Overconfidence | J | `{location}` | "Beginning to stow a bag is no reason to challenge physics at {location}." | bag | no | PS: The understatement concerns beginning to pack, not a filled bag or a completed result. |  |  |
| PS-C-05 | Loot greed | M | `{location}` | "Organizing the bag at {location} starts before greed gets a vote." | bag | no | PS: The imagined vote concerns packing beginning without claiming any contents. |  |  |
| PS-C-06 | Loot greed | O | - | "Beginning to stow is a reminder that space isn't the same as wanting more." | - | no | PS: The collector compares desire with packing space without asserting a filled bag or added capacity. |  |  |
| PS-C-07 | Gear obsession | J | `{location}` | "My bag-organizing plans give the zipper a moment to breathe at {location}." | zipper | no | PS: The personified fastener concerns the start of packing, not its condition or the bag's contents. |  |  |
| PS-C-08 | Bureaucracy | M | `{location}` | "Packing at {location} has inspired my greed to file an imaginary request for room." | - | yes | PS: The imaginary request responds to packing starting, not a capacity change or added item. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-G-01 | Genre tension | J | `{location}` | "The backpack stars in the experiment I imagine while beginning to pack at {location}." | backpack | no | PS: The imagined experiment reacts to packing beginning without claiming a completed arrangement. |  |  |
| PS-G-02 | Genre tension | M | `{location}` | "Arranging the bag at {location} feels like a puzzle before anyone has shown me the picture." | bag | no | PS: The puzzle analogy concerns starting to pack, not a real puzzle or an item award. |  |  |
| PS-G-03 | Loot greed | O | - | "Stowing brings out my appetite for one more pouch." | pouch | no | PS: The desired pouch reflects packing beginning, not added capacity or an item. |  |  |
| PS-G-04 | Overconfidence | J | `{location}` | "My first effort arranging the backpack deserves an elaborate design at {location}." | backpack | no | PS: The boast concerns an imagined design at the start of packing, not bag contents. |  |  |
| PS-G-05 | False optimism | M | `{location}` | "Starting to organize at {location} makes flawless packing look like a solved engineering problem." | - | no | PS: The optimistic assessment concerns packing just begun, not a completed arrangement. |  |  |
| PS-G-06 | Loot greed | O | - | "Starting to stow makes another compartment sound like a brilliant idea." | compartment | no | PS: The desired compartment reflects the activity beginning, not added capacity. |  |  |
| PS-G-07 | Loot greed | J | `{location}` | "An ideal pouch has my appetite dreaming as the stowing begins at {location}." | pouch | no | PS: The imagined pouch concerns packing starting without claiming a part or pouch exists. |  |  |
| PS-G-08 | Overconfidence | M | `{location}` | "Packing plans at {location} have ambitions far beyond ordinary geometry." | - | no | PS: The boast concerns a plan at the start of packing, not a measured result. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PS-H-01 | Gear obsession | J | `{location}` | "Every zipper deserves attention as I start stowing at {location}." | zipper | no | PS: An imagined fastener concerns packing beginning, not its contents or condition. |  |  |
| PS-H-02 | Gear obsession | M | `{location}` | "Planning the bag's arrangement at {location} brings my imaginary dividers under intense scrutiny." | dividers | no | PS: The imagined dividers concern packing beginning, not added capacity or completed packing. |  |  |
| PS-H-03 | Gear obsession | O | - | "Packing starts; I imagine the perfect pouch." | pouch | no | PS: The imagined packing pouch expresses a preference, not extra capacity or an item. |  |  |
| PS-H-04 | False optimism | J | `{location}` | "Surely the backpack and I can agree on where to begin at {location}." | backpack | no | PS: The hoped-for agreement concerns the start of packing, not a completed fit. |  |  |
| PS-H-05 | Overconfidence | M | `{location}` | "My packing plans at {location} include a completely reasonable number of imaginary pouches." | pouches | no | PS: The boast concerns a packing preference without claiming specific equipment or completion. |  |  |
| PS-H-06 | Genre tension | O | - | "Beginning to stow is apparently an emotional occasion." | - | no | PS: The exaggerated emotion responds to the start of packing, not a find or departure. |  |  |
| PS-H-07 | Overconfidence | J | `{location}` | "Starting to organize a bag is no match for my devotion to order at {location}." | bag | no | PS: The boast concerns packing starting, without claiming completion or ownership. |  |  |
| PS-H-08 | Gear obsession | M | `{location}` | "Beginning to stow at {location} has my mind arranging possible dividers." | dividers | no | PS: The imagined dividers relate to packing, but no actual divider or completed arrangement is claimed. |  |  |

## PF — PackingCompleted

**Required:** PackingCompleted and location. The packing activity ended; this
does not prove that cargo exists, secure extraction, award items, or imply sale.
Existing candidate rows still need this factual audit.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-B-01 | False optimism | J | `{location}` | "Surely that packing job has earned the backpack's respect at {location}." | backpack | no | PF: The hoped-for respect follows completed packing, not an item or safe extraction. |  |  |
| PF-B-02 | Gear obsession | M | `{location}` | "The backpack at {location} has endured my organizational opinions." | backpack | no | PF: The joke follows the completed packing activity without asserting any contents. |  |  |
| PF-B-03 | Gear obsession | O | - | "Even a buckle deserves a breather after my packing technique." | buckle | no | PF: The imagined buckle responds to finished packing without asserting its condition. |  |  |
| PF-B-04 | Loot greed | J | `{location}` | "Stowing is complete; my desire for more has already applied for extra space at {location}." | - | yes | PF: The imaginary application follows completed packing without claiming extra capacity or cargo. |  |  |
| PF-B-05 | Overconfidence | M | `{location}` | "That stowing effort at {location} has survived my own standards." | - | no | PF: The boast concerns completed packing, not the condition of any cargo. |  |  |
| PF-B-06 | Gear obsession | O | - | "Finished packing merits a salute from every zipper." | zipper | no | PF: The imagined salute celebrates packing's completion without asserting an actual fastener. |  |  |
| PF-B-07 | Genre tension | J | `{location}` | "The final flourish belongs to me; I've finished arranging the bag at {location}." | bag | no | PF: The flourish marks packing completed, not a full bag, departure or extraction. |  |  |
| PF-B-08 | Overconfidence | M | `{location}` | "The stowing at {location} is finished; it gets my fiercest nod." | - | no | PF: The boast reacts to completed packing without claiming its contents or outcome. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-C-01 | Gear obsession | J | `{location}` | "The backpack deserves a cautious nod for the completed organizing at {location}." | backpack | no | PF: The nod follows the completed packing activity, not an asserted item condition. |  |  |
| PF-C-02 | False optimism | M | `{location}` | "With packing complete at {location}, I choose to believe the zipper behaved perfectly." | zipper | no | PF: The optimistic belief follows completed packing without claiming the fastener's actual state. |  |  |
| PF-C-03 | Genre tension | O | - | "The bag's organization is done; the journey home isn't." | bag | no | PF: The packing activity ended; the Raider does not claim departure or extraction. |  |  |
| PF-C-04 | Gear obsession | J | `{location}` | "My packing checklist can retire its pouch entry at {location}." | checklist, pouch | yes | PF: The imagined checklist marks the completed packing activity, not an actual inventory. |  |  |
| PF-C-05 | Loot greed | M | `{location}` | "Completed stowing at {location} gives my greed one less argument to win." | - | no | PF: Greed reacts to completed packing without asserting a find or item loss. |  |  |
| PF-C-06 | Loot greed | O | - | "Stowing is finished; the part of me that wants more has no complaint." | - | yes | PF: The collector's lack of complaint concerns completed packing, not a physical item. |  |  |
| PF-C-07 | Overconfidence | J | `{location}` | "Backpack stowing is done; no need to declare victory over {location}." | backpack | no | PF: The boast distinguishes completed packing from a safe extraction. |  |  |
| PF-C-08 | Genre tension | M | `{location}` | "The packing at {location} is over; the journey is another matter." | - | no | PF: The finished activity does not establish a completed journey or extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-G-01 | Genre tension | J | `{location}` | "That backpack has survived my elaborate packing methodology at {location}." | backpack | no | PF: The joke follows the completed packing activity without claiming its contents or condition. |  |  |
| PF-G-02 | Loot greed | M | `{location}` | "The stowing at {location} is over; my appetite wants a spare compartment already." | compartment | no | PF: The imagined compartment is desired after completed packing, not claimed as added capacity. |  |  |
| PF-G-03 | Overconfidence | O | - | "My backpack and I have finished a very technical organizing conversation." | backpack | no | PF: The conversation metaphor marks packing's completion without asserting an item. |  |  |
| PF-G-04 | Loot greed | J | `{location}` | "The completed pouch procedure earns my acquisitive applause at {location}." | pouch | no | PF: The imagined procedure celebrates completed packing, not an acquired part. |  |  |
| PF-G-05 | False optimism | M | `{location}` | "Stowing complete at {location}; my imaginary zipper design is obviously flawless." | zipper | no | PF: The optimistic opinion concerns an imaginary fastener design, not the actual fastener's condition. |  |  |
| PF-G-06 | Loot greed | O | - | "A completed packing job makes a spare compartment sound even more appealing." | compartment | no | PF: The imagined compartment is desired after packing, not claimed as added capacity. |  |  |
| PF-G-07 | Loot greed | J | `{location}` | "Pockets have my appetite's applause now stowing is finished at {location}." | pockets | no | PF: The imaginary victory marks completed packing without asserting items in the pockets. |  |  |
| PF-G-08 | Overconfidence | M | `{location}` | "My packing design at {location} is quietly smug about reaching the end." | - | no | PF: The boast concerns the activity finishing, not a measured layout or secured exit. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| PF-H-01 | Overconfidence | J | `{location}` | "That backpack was organized by the finest packer at {location}." | backpack | no | PF: The boast celebrates completed packing without claiming any contents. |  |  |
| PF-H-02 | Genre tension | M | `{location}` | "The end of stowing at {location} feels oddly final for a bag that can be opened." | bag | no | PF: The reaction contrasts finished packing with the possibility of reopening a bag, without claiming contents. |  |  |
| PF-H-03 | Gear obsession | O | - | "The pouch can stop listening to my packing advice now." | pouch | no | PF: The imagined pouch hears the end of the packing activity, not a claimed inventory. |  |  |
| PF-H-04 | Overconfidence | J | `{location}` | "Even the zipper must admire my completed organizing at {location}." | zipper | no | PF: The personified fastener celebrates packing's completion, not its own condition. |  |  |
| PF-H-05 | Bureaucracy | M | `{location}` | "The packing at {location} goes in my mental ledger under 'properly cherished.'" | - | yes | PF: The imagined filing describes the completed activity, not a real document or added item. |  |  |
| PF-H-06 | Genre tension | O | - | "Stowing the bag is complete, along with our deeply emotional consultation." | bag | no | PF: The imagined consultation marks the end of packing without asserting the bag contains anything. |  |  |
| PF-H-07 | Gear obsession | J | `{location}` | "Even the zipper deserves a quiet moment when stowing ends at {location}." | zipper | no | PF: The personified fastener marks the finished packing activity, not its condition or contents. |  |  |
| PF-H-08 | Overconfidence | M | `{location}` | "The completed packing at {location} has met its most devoted organizer." | - | no | PF: The boast follows the activity's completion without claiming ownership or extraction. |  |  |

## HW — HazardWarning

**Required:** HazardWarning, committed condition and location. A warning is a
forecast, not evidence that the hazard has already struck or will cause HP loss.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| HW-B-01 | Overconfidence | J | `{condition}` | "I'll give that fog warning my most intimidating squint: {condition}." | `{condition}` | no | HW: Light Fog only; the squint answers its warning rather than reporting fog or an impact. |  |  |
| HW-B-02 | Genre tension | M | `{condition}` | "The warning for {condition} sounds like the end of a shift nobody wants to finish." | `{condition}` | no | HW: Overtime Red only; the closing-time comparison reacts to the forecast, not an actual closing. |  |  |
| HW-B-03 | Overconfidence | O | - | "The forecast can wait while I finish looking unimpressed." | - | no | HW: The boast responds to a warning without claiming the hazard has struck. |  |  |
| HW-B-04 | Genre tension | J | `{condition}` | "That forecast has terrible timing for my bravest face: {condition}." | `{condition}` | no | HW: The Raider reacts to this condition's forecast, not an impact. |  |  |
| HW-B-05 | Overconfidence | M | `{condition}` | "Tell {condition} I don't accept advance apologies from weather." | `{condition}` | no | HW: Apology Weather only; the Raider rejects the forecast's implied apology without claiming it has struck. |  |  |
| HW-B-06 | Loot greed | O | - | "That condition alert has offended my ambitions for loot." | - | no | HW: The ambitions object to the condition warning without claiming an item or impact. |  |  |
| HW-B-07 | False optimism | J | `{location}` | "I'm certain that environmental warning is the worst my glare will face at {location}." | - | no | HW: The optimistic belief reframes the condition warning without guaranteeing safety or an impact. |  |  |
| HW-B-08 | Genre tension | M | `{condition}` | "The forecast for {condition} sounds like the shift has filed a grievance." | `{condition}` | yes | HW: Overtime Red only; the grievance is an imagined reaction to the warning, not a real filing or closing. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-C-01 | False optimism | J | `{condition}` | "That warning must be all the fog has planned for me: {condition}." | `{condition}` | no | HW: Light Fog only; the optimistic belief follows its warning without guaranteeing an outcome. |  |  |
| HW-C-02 | Gear obsession | M | `{condition}` | "For {condition}, an umbrella sounds more useful than accepting the apology." | `{condition}`, umbrella | no | HW: Apology Weather only; the umbrella fits its drizzle, without claiming protection or impact. |  |  |
| HW-C-03 | Overconfidence | O | - | "I can take a forecast seriously without applauding it." | - | no | HW: The Raider treats the condition forecast seriously without claiming the hazard occurred. |  |  |
| HW-C-04 | Loot greed | J | `{condition}` | "Loot can wait until I've considered the forecast for {condition}." | `{condition}` | no | HW: This condition's warning tempers enthusiasm for loot without asserting a find. |  |  |
| HW-C-05 | Genre tension | M | `{condition}` | "The apology in {condition} is less reassuring when it comes with a warning." | `{condition}` | no | HW: Apology Weather only; the condition's name makes its warning sound politely ominous. |  |  |
| HW-C-06 | Loot greed | O | - | "Against that forecast, even loot sounds less persuasive." | - | no | HW: The forecast tempers enthusiasm for loot without asserting an item or impact. |  |  |
| HW-C-07 | Overconfidence | J | `{location}` | "I can respect an environmental prediction without panicking at {location}." | - | no | HW: The Raider responds to the condition prediction at this location without claiming an outcome. |  |  |
| HW-C-08 | Genre tension | M | `{condition}` | "The warning for {condition} makes this shift sound longer than I hoped." | `{condition}` | no | HW: Overtime Red only; the warning evokes overtime without claiming the shift has ended. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-G-01 | Genre tension | J | `{condition}` | "My visibility calculations dislike this fog forecast: {condition}." | `{condition}` | no | HW: Light Fog only; imagined calculations respond to the forecast, not measured visibility. |  |  |
| HW-G-02 | False optimism | M | `{condition}` | "If {condition} is apologizing in advance, perhaps its forecast appreciates feedback." | `{condition}` | no | HW: Apology Weather only; the optimistic complaint concerns its warning and claims no impact. |  |  |
| HW-G-03 | Overconfidence | O | - | "Even I respect an environmental alert with its eyebrows raised." | - | no | HW: The boast gives this condition's forecast imaginary eyebrows without claiming an actual reading. |  |  |
| HW-G-04 | Loot greed | J | `{condition}` | "The alert has postponed my daydream about finding parts: {condition}." | `{condition}` | no | HW: This condition's warning interrupts imagined interest in parts without claiming a find. |  |  |
| HW-G-05 | Gear obsession | M | `{condition}` | "The forecast for {condition} makes waterproofing sound worth considering." | `{condition}` | no | HW: Apology Weather only; the drizzle forecast inspires caution, not a claim of equipment or protection. |  |  |
| HW-G-06 | Genre tension | O | - | "The condition warning interrupted my theory's most dramatic chapter." | - | yes | HW: The imagined chapter concerns the condition forecast, not an actual book or impact. |  |  |
| HW-G-07 | Overconfidence | J | `{location}` | "I can overengineer a response to this warning at {location}." | - | no | HW: The boast concerns a possible response to this warning, not a completed action. |  |  |
| HW-G-08 | Gear obsession | M | `{condition}` | "Advance notice of {condition} suggests my caution algorithm should stop clocking overtime." | `{condition}` | no | HW: Overtime Red only; the imagined algorithm plays on the warning's name, not an actual device. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HW-H-01 | False optimism | J | `{condition}` | "This warning must keep me one step ahead of {condition}." | `{condition}` | no | HW: Light Fog only; the optimistic expectation follows its warning without claiming the fog has struck. |  |  |
| HW-H-02 | Genre tension | M | `{condition}` | "The forecast for {condition} leaves even my ambition feeling overworked." | `{condition}` | no | HW: Overtime Red only; the collector's reaction to the warning echoes overtime, not a real closing. |  |  |
| HW-H-03 | Loot greed | O | - | "An advance condition alert asks my appetite to exercise caution." | - | no | HW: The condition warning tempers the collector's desires, not an item loss or impact. |  |  |
| HW-H-04 | Overconfidence | J | `{condition}` | "Even my ambitions can pause for the warning about {condition}." | `{condition}` | no | HW: The boast responds to this condition's warning without claiming an item or safety. |  |  |
| HW-H-05 | Loot greed | M | `{condition}` | "The advance apology of {condition} is competing with my hunger for more." | `{condition}` | no | HW: Apology Weather only; the warning competes with the collector's desires, not an asserted impact. |  |  |
| HW-H-06 | Genre tension | O | - | "A forecast is a poor souvenir, but the notice is worth keeping in mind." | - | no | HW: The warning is remembered as information, not an actual hazard or physical souvenir. |  |  |
| HW-H-07 | Overconfidence | J | `{location}` | "Even I won't dismiss that environmental warning at {location}." | - | no | HW: The boast acknowledges the condition forecast at this location, with no safety guarantee. |  |  |
| HW-H-08 | Genre tension | M | `{condition}` | "The forecast for {condition} raises the question of who approved this much overtime." | `{condition}` | no | HW: Overtime Red only; the warning inspires an overtime joke, not an actual shift. |  |  |

## HI — ConditionImpact

**Required:** ConditionImpact, committed condition and location. This is raw
hazard impact; shielding or resilience may mean no HP damage. No line assumes an
injury, attacker, or future outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-B-01 | Overconfidence | J | `{condition}` | "That fog's blow has earned my loudest glare: {condition}." | `{condition}` | no | HI: Light Fog only; the boast reacts to its impact without implying injury. |  |  |
| HI-B-02 | Overconfidence | M | `{condition}` | "That impact from {condition} has earned my loudest disapproval." | `{condition}` | no | HI: The complaint responds to this condition's raw impact, not an HP loss. |  |  |
| HI-B-03 | Gear obsession | O | - | "My pride wants armor after that jolt from the environment." | armor | no | HI: Imagined armor reacts to the condition's impact without claiming protection or injury. |  |  |
| HI-B-04 | Loot greed | J | `{condition}` | "I'd rather collect anything other than that jolt from {condition}." | `{condition}` | no | HI: The collector rejects this condition's impact without claiming an item loss. |  |  |
| HI-B-05 | Genre tension | M | `{condition}` | "The apology in {condition} sounds less convincing after that impact." | `{condition}` | no | HI: Apology Weather only; the impact makes its name's apology ring hollow without asserting injury. |  |  |
| HI-B-06 | Loot greed | O | - | "That jolt from the conditions is not something I asked to collect." | - | no | HI: The collector protests the condition's impact without claiming a lost item. |  |  |
| HI-B-07 | Overconfidence | J | `{location}` | "That environmental hit hasn't intimidated my eyebrows at {location}." | - | no | HI: The boast responds to the condition's impact here without claiming damage was avoided. |  |  |
| HI-B-08 | Genre tension | M | `{condition}` | "The impact from {condition} has all the charm of an unpaid extra shift." | `{condition}` | no | HI: Overtime Red only; the extra-shift comparison follows its impact without asserting an actual shift. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-C-01 | Genre tension | J | `{condition}` | "That fog made its presence felt rather directly: {condition}." | `{condition}` | no | HI: Light Fog only; the understatement responds to its raw impact without claiming injury. |  |  |
| HI-C-02 | Gear obsession | M | `{condition}` | "The impact from {condition} has my imagined shock absorber reconsidering its career." | `{condition}` | no | HI: The imagined absorber comments on the raw impact without claiming equipment or mitigation. |  |  |
| HI-C-03 | Overconfidence | O | - | "That jolt from the conditions doesn't need a speech to earn my objection." | - | no | HI: The understatement responds to the condition's impact without describing injury. |  |  |
| HI-C-04 | Genre tension | J | `{condition}` | "The environment has made its point with that jolt: {condition}." | `{condition}` | no | HI: The figurative point refers to this condition's impact, not an HP outcome. |  |  |
| HI-C-05 | Gear obsession | M | `{condition}` | "After that impact, {condition} owes my imaginary raincoat an explanation." | `{condition}`, raincoat | no | HI: Apology Weather only; the raincoat suits its drizzle but is not claimed as worn or protective. |  |  |
| HI-C-06 | Loot greed | O | - | "The condition's strike interrupted my thoughts about loot." | - | no | HI: The condition's impact interrupts imagined thoughts without claiming a lost item or stopped activity. |  |  |
| HI-C-07 | Overconfidence | J | `{location}` | "I can take that environmental hit seriously without applauding it at {location}." | - | no | HI: The understatement addresses the condition's impact here without claiming it was harmless. |  |  |
| HI-C-08 | Genre tension | M | `{condition}` | "That impact from {condition} feels like the shift asking for one more minute." | `{condition}` | no | HI: Overtime Red only; the extra-shift metaphor concerns its impact, not an actual shift. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-G-01 | Genre tension | J | `{condition}` | "My fog model did not predict being struck: {condition}." | `{condition}` | no | HI: Light Fog only; the imagined model reacts to its raw impact without claiming a measurement. |  |  |
| HI-G-02 | False optimism | M | `{condition}` | "The impact of {condition} has me convinced diagnostics are about to get easier." | `{condition}` | no | HI: The optimistic technical belief reacts to this raw impact without inferring HP loss. |  |  |
| HI-G-03 | Overconfidence | O | - | "The environment's hit makes an excellent calibration point for caution." | - | no | HI: The calibration metaphor responds to the condition's impact without claiming equipment or a measurement. |  |  |
| HI-G-04 | Gear obsession | J | `{condition}` | "My imaginary shock absorber objects to the jolt from {condition}." | `{condition}` | no | HI: The imagined absorber reacts to this condition's impact without claiming actual equipment or mitigation. |  |  |
| HI-G-05 | Genre tension | M | `{condition}` | "The apology from {condition} has failed its impact test." | `{condition}` | no | HI: Apology Weather only; an imaginary test jokes about the condition's name after its impact. |  |  |
| HI-G-06 | Loot greed | O | - | "That jolt from the surroundings interrupted an excellent daydream about finding something useful." | - | no | HI: The condition's impact interrupts a daydream without asserting an acquisition or loss. |  |  |
| HI-G-07 | Overconfidence | J | `{location}` | "I can call that environmental hit a data point at {location}." | - | no | HI: The technical metaphor describes the condition's impact here without claiming a measured value. |  |  |
| HI-G-08 | Gear obsession | M | `{condition}` | "My imaginary time clock logged that impact from {condition} as overtime." | `{condition}` | yes | HI: Overtime Red only; the imaginary clock plays on the impact's condition name, not an actual shift. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HI-H-01 | Genre tension | J | `{condition}` | "That fog's hit has earned a new complaint: {condition}." | `{condition}` | yes | HI: Light Fog only; the complaint objects to its impact without claiming injury or item loss. |  |  |
| HI-H-02 | Gear obsession | M | `{condition}` | "That hit from {condition} is a rude test of my imaginary padding." | `{condition}`, padding | no | HI: Imagined protection responds to this raw impact without claiming equipment, mitigation or injury. |  |  |
| HI-H-03 | Overconfidence | O | - | "I refuse to pretend a jolt from the conditions was a welcome interruption." | - | no | HI: The boast reacts to the condition's impact, not an item or health outcome. |  |  |
| HI-H-04 | Genre tension | J | `{condition}` | "I wasn't collecting jolts like the one from {condition}." | `{condition}` | no | HI: The collecting metaphor protests this condition's impact without claiming any owned item. |  |  |
| HI-H-05 | Overconfidence | M | `{condition}` | "The apology in {condition} is no match for my disapproval after that impact." | `{condition}` | no | HI: Apology Weather only; the boast concerns the impact, not an injury or actual item. |  |  |
| HI-H-06 | Gear obsession | O | - | "This condition's impact calls for an imaginary shock absorber for my dignity." | shock absorber | no | HI: The imagined absorber responds to the condition's impact without claiming equipment, mitigation or injury. |  |  |
| HI-H-07 | Overconfidence | J | `{location}` | "Even I can't find anything worth keeping about the environment's impact at {location}." | - | no | HI: The boast judges the condition's impact here, not a physical object or injury. |  |  |
| HI-H-08 | Genre tension | M | `{condition}` | "The hit from {condition} gives my patience a double shift." | `{condition}` | no | HI: Overtime Red only; the impact inspires an extra-shift metaphor without asserting a real shift. |  |  |

## RD — RobotDefeated

**Required:** RobotDefeated, location and robot identity when `{robot}` is
used; the robot must occur in the committed text/thread. Defeat does not imply
any reward or that the Raider avoided injury.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-B-01 | Overconfidence | J | `{robot}` | "My confidence calls this worried machine's defeat excellent news: {robot}." | `{robot}` | no | RD: Anxietick only; the boast responds to its committed defeat without claiming a reward. |  |  |
| RD-B-02 | Genre tension | M | `{robot}` | "In the cleaning drama, {robot} gets the role of defeated star." | `{robot}` | no | RD: Roomba Prime only, committed defeat; no reward or extra action. |  |  |
| RD-B-03 | Genre tension | O | - | "Passive-Aggressor Drone lost the fight but probably not its opinion of me." | Passive-Aggressor Drone | no | RD: Passive-Aggressor Drone only; its defeat prompts a figurative opinion, not actual speech. |  |  |
| RD-B-04 | Gear obsession | J | `{location}` | "That defeated chassis has my gear instincts itching to inspect it at {location}." | chassis | no | RD: Interest in the defeated robot does not claim an inspection or component award. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-C-01 | Genre tension | J | `{robot}` | "The defeated cleaning machine is finally off duty: {robot}." | `{robot}` | no | RD: Roomba Prime only; off duty describes its defeat, not an actual workplace shift. |  |  |
| RD-C-02 | Overconfidence | M | `{robot}` | "Even {robot}'s defeat cannot put an end to my worrying." | `{robot}` | no | RD: Anxietick only, defeated; cautious understatement, no HP or pursuit claim. |  |  |
| RD-C-03 | Genre tension | O | - | "Even defeated, Passive-Aggressor Drone probably disapproves of my composure." | Passive-Aggressor Drone | no | RD: Passive-Aggressor Drone only; the imagined judgment follows its defeat, not a spoken exchange. |  |  |
| RD-C-04 | Gear obsession | J | `{location}` | "That disabled chassis has my equipment instincts considering its design at {location}." | chassis | no | RD: The equipment-minded reaction concerns the defeated chassis without awarding parts. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-G-01 | Loot greed | J | `{robot}` | "Even after defeat, I'm curious what might be worth salvaging from {robot}." | `{robot}` | no | RD: Roomba Prime only; speculation follows defeat without claiming salvage or ownership. |  |  |
| RD-G-02 | Genre tension | M | `{robot}` | "The defeated {robot} has reached a remarkably disapproving off state." | `{robot}` | no | RD: Passive-Aggressor Drone only; the off-state joke evokes its disapproving character, not a real expression. |  |  |
| RD-G-03 | Loot greed | O | - | "Anxietick is down; my imagination is already eyeing the components." | Anxietick | no | RD: Anxietick only; curiosity follows its defeat without asserting a component award. |  |  |
| RD-G-04 | Loot greed | J | `{location}` | "That defeated machine has inspired my curiosity about salvage at {location}." | machine | no | RD: Curiosity reacts to the defeated machine without claiming salvage or a reward. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RD-H-01 | Gear obsession | J | `{robot}` | "That defeated worry machine has me pondering its mechanisms: {robot}." | `{robot}` | no | RD: Anxietick only; interest follows defeat without claiming it or its parts. |  |  |
| RD-H-02 | Genre tension | M | `{robot}` | "The defeated {robot} has finally retired from aggressive housekeeping." | `{robot}` | no | RD: Roomba Prime only; retirement describes its defeat, not ownership or a later action. |  |  |
| RD-H-03 | Gear obsession | O | - | "Passive-Aggressor Drone's defeated chassis looks disapproving to my inner mechanic." | Passive-Aggressor Drone | no | RD: Passive-Aggressor Drone only; the imagined disapproval follows its defeat without implying disassembly. |  |  |
| RD-H-04 | Overconfidence | J | `{location}` | "That beaten chassis should admire my composure at {location}." | chassis | no | RD: The boast addresses the defeated chassis without asserting a cargo award. |  |  |

## RX — RobotEscaped

**Required:** RobotEscaped with the exact escape-after-rounds original text,
location and committed identity for `{robot}`. The text selector and name
extraction are not implemented. Other RobotEscaped resolutions do not qualify:
they can say the robot overran the fighters or had no remaining fighters, not
that it departed. Do not imply that the Raider escaped or the robot was defeated.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-B-01 | Overconfidence | J | `{robot}` | "That worried machine chose the wrong moment to leave: {robot}." | `{robot}` | no | RX: Anxietick only; the boast protests the named robot's escape, not its defeat. |  |  |
| RX-B-02 | Genre tension | M | `{robot}` | "Apparently {robot} calls that escape a cleaning appointment." | `{robot}` | no | RX: Roomba Prime only; the imagined appointment jokes about the robot leaving, not an actual destination. |  |  |
| RX-B-03 | Gear obsession | O | - | "Passive-Aggressor Drone's chassis escaped my excellent inspection plan." | Passive-Aggressor Drone | no | RX: Passive-Aggressor Drone only; the escaped machine frustrates an imagined inspection, not an actual capture. |  |  |
| RX-B-04 | Loot greed | J | `{location}` | "My salvage ambitions just lost an argument with the departing machine at {location}." | machine | no | RX: The robot's escape frustrates imagined salvage without implying a lost reward or actual argument. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-C-01 | Gear obsession | J | `{robot}` | "My curiosity about the cleaning mechanism just lost its subject: {robot}." | `{robot}` | no | RX: Roomba Prime only; interest in its mechanism follows the robot's escape, not a salvage or inspection. |  |  |
| RX-C-02 | Overconfidence | M | `{robot}` | "I can handle {robot} leaving without adding to its worries." | `{robot}` | no | RX: Anxietick only; the boast reacts to its escape without asserting an actual conversation. |  |  |
| RX-C-03 | Genre tension | O | - | "Passive-Aggressor Drone has taken its silent judgment elsewhere." | Passive-Aggressor Drone | no | RX: Passive-Aggressor Drone only; the figurative judgment departs with the robot. |  |  |
| RX-C-04 | Loot greed | J | `{location}` | "There goes a machine my collector's instincts wanted to inspect for parts at {location}." | machine | no | RX: The machine escapes before an imagined inspection, without acquiring or losing parts. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-G-01 | Gear obsession | J | `{robot}` | "The cleaning mechanism has activated its departure function: {robot}." | `{robot}` | no | RX: Roomba Prime only; the imagined function describes its escape, not a component action. |  |  |
| RX-G-02 | Genre tension | M | `{robot}` | "The departure by {robot} feels like an especially pointed disagreement." | `{robot}` | no | RX: Passive-Aggressor Drone only; the imagined disagreement accompanies its escape, not speech. |  |  |
| RX-G-03 | Overconfidence | O | - | "Anxietick escaped; my diagnosis of its worries is flawless anyway." | Anxietick | no | RX: Anxietick only; the boast about imagined worries responds to the robot's escape, not its destination. |  |  |
| RX-G-04 | Loot greed | J | `{location}` | "My plans to collect parts got left behind when that machine escaped at {location}." | machine | no | RX: The robot's escape ends imagined collecting plans without claiming capture or acquisition. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RX-H-01 | Loot greed | J | `{robot}` | "There goes the nervous machine I imagined as a souvenir: {robot}." | `{robot}` | no | RX: Anxietick only; the robot's escape disappoints appetite without claiming ownership. |  |  |
| RX-H-02 | Bureaucracy | M | `{robot}` | "My formal objection follows {robot}'s housekeeping exit." | `{robot}` | yes | RX: Roomba Prime only; the objection protests the robot's escape without claiming an actual filing. |  |  |
| RX-H-03 | Gear obsession | O | - | "Passive-Aggressor Drone escaped; I wanted a closer look at that chassis." | Passive-Aggressor Drone | no | RX: Passive-Aggressor Drone only; interest in its chassis follows its escape without claiming an inspection. |  |  |
| RX-H-04 | Overconfidence | J | `{location}` | "Even that machine couldn't outrun my opinion of its departure from {location}." | machine | no | RX: The boast follows the robot's escape without claiming capture or a lost reward. |  |  |

## RC — RobotDefeatCargoResolved

**Required:** RobotDefeatCargoResolved and location. Every line must fit both
allocated cargo and no eligible cargo; no candidate implies a particular item,
recipient, or award.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-B-01 | Overconfidence | J | `{location}` | "I can glare at the post-robot salvage verdict at {location}." | robot | no | RC: Service Depot only; the boast reacts to the defeat-cargo resolution whether or not it awarded anything. |  |  |
| RC-B-02 | Loot greed | M | `{location}` | "The machine fell at {location}; the cargo outcome has my undivided greed." | machine | no | RC: Service Depot only; greed responds to the resolved cargo outcome without asserting its contents. |  |  |
| RC-B-03 | Genre tension | O | - | "The chassis fell, the loot is decided, and my greed demands a sequel." | chassis | no | RC: Service Depot only; the imagined sequel follows either cargo outcome without claiming another event. |  |  |
| RC-B-04 | Gear obsession | J | `{location}` | "My equipment instincts have strong opinions about the salvage outcome after that robot fell at {location}." | robot | no | RC: Service Depot only; the equipment-minded opinion concerns the cargo outcome without claiming an appraisal or award. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-C-01 | Bureaucracy | J | `{location}` | "The loot report after that robot's defeat deserves a calm reading at {location}." | robot, report | yes | RC: Service Depot only; the imagined report fits either cargo outcome and identifies no recipient. |  |  |
| RC-C-02 | Gear obsession | M | `{location}` | "The salvage resolution after that machine went down at {location} calls for caution, not dismantling." | machine | no | RC: Service Depot only; the response fits either outcome without claiming an inspection or salvage. |  |  |
| RC-C-03 | Overconfidence | O | - | "My composure can handle the cargo verdict after that chassis fell without claiming a share." | chassis | no | RC: Service Depot only; the restraint fits both outcomes without asserting a personal award. |  |  |
| RC-C-04 | Loot greed | J | `{location}` | "The robot is down; my collector's patience is listening to the salvage outcome at {location}." | robot | no | RC: Service Depot only; the collector reacts to either resolution without claiming a particular award. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-G-01 | Loot greed | J | `{location}` | "The cargo verdict after that robot's defeat has my collector's instincts calculating at {location}." | robot | no | RC: Service Depot only; curiosity responds to either resolution without inventing a component. |  |  |
| RC-G-02 | Genre tension | M | `{location}` | "The cargo conclusion after that machine's defeat at {location} deserves a more dramatic explanation." | machine | no | RC: Service Depot only; the dramatic reaction fits either outcome, not a particular award. |  |  |
| RC-G-03 | Gear obsession | O | - | "The salvage verdict after that chassis went down gives my inner engineer plenty to overthink." | chassis | no | RC: Service Depot only; the verdict concerns either cargo outcome, not a claimed component. |  |  |
| RC-G-04 | Overconfidence | J | `{location}` | "I can give the cargo resolution after that robot's defeat a technical salute at {location}." | robot | no | RC: Service Depot only; the boast reacts to either cargo outcome without naming an award. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RC-H-01 | Loot greed | J | `{location}` | "The salvage decision after that robot's defeat has my greed on edge at {location}." | robot | no | RC: Service Depot only; the collector reacts to either cargo outcome without asserting an item or owner. |  |  |
| RC-H-02 | Genre tension | M | `{location}` | "The cargo decision after the machine fell at {location} wasn't the dramatic finale I imagined." | machine | no | RC: Service Depot only; the imagined finale contrasts with the cargo resolution without asserting an acquisition. |  |  |
| RC-H-03 | Gear obsession | O | - | "The cargo decision is in, and I'm pondering the defeated chassis more than its verdict." | chassis | no | RC: Service Depot only; interest in the defeated robot follows either cargo outcome without claiming its contents. |  |  |
| RC-H-04 | Bureaucracy | J | `{location}` | "The loot resolution after that robot's defeat deserves an appeal at {location}." | robot | yes | RC: Service Depot only; the imagined appeal responds to either outcome, not a real document or prize. |  |  |

## RV — RecoveryStarted or RecoveryCompleted

**Required:** RecoveryStarted or RecoveryCompleted and location. Both kinds
concern composure, not restored HP. Each line fits either phase without
claiming recovery has completed or begun anew.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-B-01 | Loot greed | J | `{location}` | "After that contest, my lungs demand the time my greed wanted at {location}." | lungs | no | RV: Catching breath after the contest takes time from greed, without claiming healing or a prize. |  |  |
| RV-B-02 | Gear obsession | M | `{location}` | "At {location}, my lungs require a quieter operating mode after that contest." | lungs | no | RV: The technical metaphor describes catching breath, not a device, HP change or victory. |  |  |
| RV-B-03 | Gear obsession | O | - | "Lungs are stubborn machinery; they want a pause after competing." | lungs | no | RV: Imagined machinery describes catching breath after the contest, not physical repair or healing. |  |  |
| RV-B-04 | Overconfidence | J | `{location}` | "Catching my breath after the contest earns my ribs a stern look at {location}." | ribs | no | RV: The imagined stern look accompanies recovery, not an injury or restored HP. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-C-01 | Loot greed | J | `{location}` | "After that contest, my lungs get priority over my appetite at {location}." | lungs | no | RV: Breathing after the contest takes precedence over imagined appetite, not a find or HP change. |  |  |
| RV-C-02 | Gear obsession | M | `{location}` | "Breathing room at {location} gives my nerves a lower operating setting after competing." | - | no | RV: The imagined setting describes a pause after the contest, not medical treatment. |  |  |
| RV-C-03 | Overconfidence | O | - | "These lungs deserve a pause after that contest, without an announcement." | lungs | no | RV: The understated boast concerns catching breath after the contest, not healing or a completed journey. |  |  |
| RV-C-04 | Genre tension | J | `{location}` | "A quiet moment for my chest after that contest at {location}." | chest | no | RV: The pause follows the contest without asserting a victory, injury or HP change. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-G-01 | Genre tension | J | `{location}` | "After that contest, even my lungs demand a pause at {location}." | lungs | no | RV: Catching breath follows the contest without asserting a victory or HP gain. |  |  |
| RV-G-02 | Genre tension | M | `{location}` | "After that contest, my lungs at {location} recommend low-power mode." | lungs | no | RV: A pause to catch breath inspires the lungs' imagined advice, not an injury. |  |  |
| RV-G-03 | Overconfidence | O | - | "I can catch my breath without replacing an imaginary fuse." | - | no | RV: The technical metaphor describes catching breath, not physical repair or healing. |  |  |
| RV-G-04 | Gear obsession | J | `{location}` | "My ribs need a maintenance pause after that contest at {location}." | ribs | no | RV: The body's imagined maintenance is breathing room, not actual repair, injury or healing. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| RV-H-01 | Loot greed | J | `{location}` | "After the contest, my lungs can have some precious breathing room at {location}." | lungs | no | RV: Catching breath after the contest is valued like a possession, not an HP restoration. |  |  |
| RV-H-02 | Genre tension | M | `{location}` | "After competing at {location}, my chest gets breathing room no collection can improve." | chest | no | RV: A pause to recover after the contest is not a collectible item, victory or injury. |  |  |
| RV-H-03 | Gear obsession | O | - | "Even my hunger has to let my breathing equipment cool down after the contest." | - | no | RV: Imagined breathing equipment gets a pause after the contest, not a repair or health effect. |  |  |
| RV-H-04 | Overconfidence | J | `{location}` | "After that contest, my ribs have earned a quiet moment at {location}." | ribs | no | RV: The boast values a pause after the contest without claiming healing or victory. |  |  |

## DN — Downed

**Required:** Downed and committed location. Downed is not settled knockout;
no joke promises recovery, rescue, extraction, or an HP change. DN-B-04,
DN-C-02, DN-G-02 and DN-H-01 also require `{source}` from this exact Downed
receipt's `Reason`; source extraction and selection are not implemented.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|---|
| DN-B-01 | Overconfidence | J | `{location}` | "My attitude refuses to lie down just because I did at {location}." | - | no | DN: The downed perspective boasts about attitude without promising recovery or knockout. |  |  |
| DN-B-02 | Bureaucracy | M | `{location}` | "At {location}, my knees have lodged a formal complaint about going horizontal." | knees | yes | DN: The imagined complaint describes the downed posture, not a specific injury or real filing. |  |  |
| DN-B-03 | Gear obsession | O | - | "Those knees demonstrated their suspension's limits by putting me on the ground." | knees | no | DN: Imagined suspension describes the downed posture, not equipment damage or rescue. |  |  |
| DN-B-04 | Loot greed | J | `{source}` | "At least my greed wasn't knocked out by {source}." | `{source}` | no | DN: The joke objects to the committed knockdown without claiming recovery or an item. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-C-01 | Genre tension | J | `{location}` | "The ground is a panorama with terrible seating arrangements at {location}." | ground | no | DN: The downed viewpoint makes the ground an uncomfortable panorama without implying a chair, knockout or rescue. |  |  |
| DN-C-02 | Gear obsession | M | `{source}` | "After {source}, my knees have very little to say about standing." | `{source}` | no | DN: The committed source caused the downed posture, not a specific injury or recovery. |  |  |
| DN-C-03 | Overconfidence | O | - | "Knees on the ground don't mean composure has to follow." | knees, ground | no | DN: The understated boast responds to being downed, without claiming recovery. |  |  |
| DN-C-04 | Genre tension | J | `{location}` | "Feet and ground have reached an uneasy agreement on my posture at {location}." | feet, ground | no | DN: The downed posture is an imagined agreement, not a settled knockout. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-G-01 | Genre tension | J | `{location}` | "My plan for staying upright has met the ground at {location}." | ground | no | DN: The downed perspective disputes the imagined plan, without claiming recovery. |  |  |
| DN-G-02 | Genre tension | M | `{source}` | "The impact of {source} has put my posture in low-power mode." | `{source}` | no | DN: The committed damage source caused the downed state, not a separate device failure or rescue. |  |  |
| DN-G-03 | Overconfidence | O | - | "I can analyze being down without calling it a design feature." | - | no | DN: The boast concerns the Raider's downed state, not an equipment feature or completed recovery. |  |  |
| DN-G-04 | Loot greed | J | `{location}` | "Being downed hasn't dampened my ground-level appetite for components at {location}." | ground | no | DN: Appetite persists despite the downed state, not a lost component or recovery. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DN-H-01 | Loot greed | J | `{source}` | "That knockdown is not a collectible I wanted from {source}." | `{source}` | no | DN: The committed source caused this downed state; no item or recovery is claimed. |  |  |
| DN-H-02 | Genre tension | M | `{location}` | "At {location}, my knees have joined my ambitions in lying down." | knees | no | DN: The imagined ambitions follow the downed posture, not a settled knockout or rescue. |  |  |
| DN-H-03 | Gear obsession | O | - | "Being down has my elbows questioning their protective design." | elbows | no | DN: The downed perspective inspires an imagined design question, not an equipment change. |  |  |
| DN-H-04 | Overconfidence | J | `{location}` | "I'm down, but even my boots can't look as disappointed as I do at {location}." | boots | no | DN: The personified boots react to being downed without promising recovery. |  |  |

## KO — KnockedOut

**Required:** KnockedOut and location. Knockout is settled; no line claims a
successful extraction, recovery, item salvage, or details absent from the record.
J/M variants additionally require `{source}` from this exact knockout receipt's
nonempty `Reason`; source extraction and selection are not implemented. Missing
reasons exclude those variants.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-B-01 | Genre tension | J | `{source}` | "The knockout has given my glare a specific target: {source}." | `{source}` | no | KO: This settled knockout's committed cause inspires a figurative glare, not another fight. |  |  |
| KO-B-02 | Overconfidence | M | `{source}` | "The reason is '{source}'; my pride refuses to make a speech." | `{source}` | no | KO: The settled knockout's committed cause leaves pride figuratively speechless, not acting later. |  |  |
| KO-B-03 | Gear obsession | O | - | "Even the sturdiest helmet can't negotiate away a knockout." | helmet | no | KO: The hypothetical helmet cannot undo this settled knockout; no equipment or recovery is claimed. |  |  |
| KO-B-04 | Loot greed | J | `{source}` | "Even my appetite has harsh words for this knockout's cause: {source}." | `{source}` | no | KO: Appetite objects to the committed knockout cause, not a lost or saved item. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-C-01 | Bureaucracy | J | `{source}` | "The knockout's cause deserves an imaginary cancellation stamp: {source}." | `{source}` | yes | KO: The imagined stamp responds to the committed knockout cause, not a real filing or recovery. |  |  |
| KO-C-02 | Gear obsession | M | `{source}` | "The knockout's reason is '{source}'; my mental safety checklist missed it." | `{source}` | yes | KO: The imagined checklist responds to the settled knockout's cause, not an actual inspection. |  |  |
| KO-C-03 | Genre tension | O | - | "The knockout finally ran my understatement out of synonyms." | - | no | KO: The Raider's understated voice runs out of words for this settled knockout, not a later event. |  |  |
| KO-C-04 | Genre tension | M | `{source}` | "The reason is '{source}'; that is a remarkably unhelpful way to end a raid." | `{source}` | no | KO: The committed cause explains a settled knockout, not death or extraction. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-G-01 | Loot greed | J | `{source}` | "My inventory predictions have no flattering answer for this knockout: {source}." | `{source}` | no | KO: Curiosity reacts to the knockout's committed cause without claiming a transfer or recovery. |  |  |
| KO-G-02 | Genre tension | M | `{source}` | "The reason is '{source}'; that knockout put my latest calculations to sleep." | `{source}` | no | KO: The imagined calculations react to the settled knockout's cause, not a later result. |  |  |
| KO-G-03 | Overconfidence | O | - | "Even my grandest optimization can't argue with being out cold." | - | no | KO: The boast concedes the settled knockout without implying later action or healing. |  |  |
| KO-G-04 | Gear obsession | J | `{source}` | "The calculations in my head didn't predict this knockout's cause: {source}." | `{source}` | no | KO: The imagined calculations react to the committed knockout cause, not a physical equipment failure. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| KO-H-01 | Loot greed | J | `{source}` | "A knockout is one experience I didn't want to collect: {source}." | `{source}` | no | KO: The settled knockout's committed cause is an unwanted experience, not a transaction or salvage. |  |  |
| KO-H-02 | Genre tension | M | `{source}` | "The reason is '{source}'; I object to adding that knockout to my history." | `{source}` | no | KO: The settled knockout's committed cause gets an objection, not a later recovery. |  |  |
| KO-H-03 | Gear obsession | O | - | "Not even a secret pocket can stow away being out cold." | pocket | no | KO: The imagined use of the secret pocket reacts to this settled knockout without claiming salvage. |  |  |
| KO-H-04 | Loot greed | J | `{source}` | "I can't add this knockout's cause to anything worth keeping: {source}." | `{source}` | no | KO: The collector rejects the committed knockout cause, not a claimed item or extraction. |  |  |

## DL — DeadlineExpired

**Required:** DeadlineExpired and location. The deadline is a loss, not a
successful extraction; no ownership details or later recovery are implied.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-B-01 | Overconfidence | J | `{location}` | "The deadline's clock has terrible manners at {location}." | clock | no | DL: The figurative clock marks the expired deadline, not a successful extraction. |  |  |
| DL-B-02 | Genre tension | M | `{location}` | "At {location}, time ran out and stole my preferred finale." | - | no | DL: Figurative theft makes the expired deadline an unwanted ending without claiming item loss. |  |  |
| DL-B-03 | Gear obsession | O | - | "That expired deadline earned the stopwatch in my head a glare." | stopwatch | no | DL: The imagined timer marks the expired deadline without altering its outcome. |  |  |
| DL-B-04 | Loot greed | J | `{location}` | "Not even my appetite can squeeze another minute from the deadline's stopped clock at {location}." | clock | no | DL: The expired deadline defeats the appetite for more time, not an actual extraction. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-C-01 | Genre tension | J | `{location}` | "The deadline is a door that doesn't care how politely I knock at {location}." | door | no | DL: metaphor for committed loss, no extraction or physical knock. |  |  |
| DL-C-02 | Gear obsession | M | `{location}` | "My imaginary timer at {location} has delivered an unhelpful final reading." | timer | no | DL: The imagined instrument marks the expired deadline, not a successful exit. |  |  |
| DL-C-03 | Overconfidence | O | - | "Even a stopped clock has to respect that expired deadline." | clock | no | DL: The imagined clock reacts to the expired deadline, not a later recovery or changed time. |  |  |
| DL-C-04 | Genre tension | J | `{location}` | "The deadline's clock ran out before my hopes at {location}." | clock | no | DL: The figurative clock reflects the expired deadline, not a later success. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-G-01 | Gear obsession | J | `{location}` | "The deadline timer in my head has an unfriendly conclusion at {location}." | timer | no | DL: The imagined timer marks the deadline loss without implying a rescue. |  |  |
| DL-G-02 | Genre tension | M | `{location}` | "At {location}, the deadline has all the charm of a stopwatch at zero." | stopwatch | no | DL: The timer comparison concerns an expired deadline, not an actual instrument or extraction. |  |  |
| DL-G-03 | Overconfidence | O | - | "An expired time limit refuses to negotiate with arithmetic." | - | no | DL: The arithmetic joke concerns the expired deadline, not an actual negotiation or changed time. |  |  |
| DL-G-04 | Loot greed | J | `{location}` | "The deadline's hourglass leaves my appetite no sand to collect at {location}." | hourglass | no | DL: The imagined timer reflects the expired deadline, not an actual acquisition or recovery. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| DL-H-01 | Loot greed | J | `{location}` | "The deadline's clock ran out before my appetite for more at {location}." | clock | no | DL: The figurative clock sets the expired deadline against desire, not item loss or extraction. |  |  |
| DL-H-02 | Genre tension | M | `{location}` | "At {location}, my hunger watches the last grain fall from the deadline's hourglass." | hourglass | no | DL: The expired deadline cuts off desire without claiming cargo or a later outcome. |  |  |
| DL-H-03 | Gear obsession | O | - | "Even an hourglass can't turn back an expired limit." | hourglass | no | DL: The imagined timer reflects the expired deadline, not an acquired object. |  |  |
| DL-H-04 | Overconfidence | J | `{location}` | "Even I can't coax another minute from the expired deadline at {location}." | - | no | DL: The boast acknowledges the expired deadline without implying an actual purchase or extraction. |  |  |

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

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-B-01 | Overconfidence | J | `{item}` | "The hidden survivor gets my loudest introduction: {item}." | `{item}` | no | HS: The boast celebrates this exact item preserved in the hidden pocket, not another award. |  |  |
| HS-B-02 | Genre tension | M | `{item}` | "Even {item} gets a dramatic escape inside a seam." | `{item}` | no | HS: The dramatic escape describes this item being preserved, not the Raider extracting. |  |  |
| HS-B-03 | Gear obsession | O | - | "Single Sock has made the hidden pocket its own one-footed victory." | Single Sock | no | HS: Single Sock only; the foot-themed victory concerns its committed preservation, not a second sock. |  |  |
| HS-B-04 | Loot greed | J | `{item}` | "The secret compartment's survivor has earned an absurdly large grin: {item}." | `{item}` | no | HS: Appetite celebrates this preserved item without inferring its value. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-C-01 | Genre tension | J | `{item}` | "The hidden pocket has made one sensible argument: {item}." | `{item}` | no | HS: The argument is this item's committed preservation, not an extra award. |  |  |
| HS-C-02 | Gear obsession | M | `{item}` | "For {item}, a pocket-sized nod, not an extraction victory." | `{item}` | no | HS: The nod distinguishes this item's preservation from the Raider's extraction. |  |  |
| HS-C-03 | Loot greed | O | - | "Single Sock gives secret storage a rather lopsided success story." | Single Sock | no | HS: Single Sock only; the one-sock joke follows its preservation without claiming a missing partner. |  |  |
| HS-C-04 | Overconfidence | J | `{item}` | "I can appreciate a narrow escape on behalf of {item}." | `{item}` | no | HS: The escape metaphor means this exact item was preserved, not that the Raider extracted. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-G-01 | Gear obsession | J | `{item}` | "Elegant containment has one successful specimen: {item}." | `{item}` | no | HS: The containment metaphor refers to this exact item being preserved in the pocket. |  |  |
| HS-G-02 | Loot greed | M | `{item}` | "The pocket spared {item}; my collector's instincts can relax." | `{item}` | no | HS: The pocket spared this preserved item, not the Raider from knockout. |  |  |
| HS-G-03 | Overconfidence | O | - | "Left Boot survived in tiny storage; clearly, my taste in hiding places is excellent." | Left Boot | no | HS: Left Boot only; the preserved item illustrates the hidden pocket without implying a pair. |  |  |
| HS-G-04 | Gear obsession | J | `{item}` | "My storage obsession has a successful demonstration: {item}." | `{item}` | no | HS: This item's preservation demonstrates the secret pocket without adding capacity. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HS-H-01 | Loot greed | J | `{item}` | "The hidden compartment produced a very personal selection: {item}." | `{item}` | no | HS: The selection is this exact preserved item, not an additional acquisition. |  |  |
| HS-H-02 | Genre tension | M | `{item}` | "I regard {item} as a magnificent advertisement for concealed storage." | `{item}` | no | HS: The opinion celebrates this preserved item without asserting its value. |  |  |
| HS-H-03 | Gear obsession | O | - | "Vintage Comic Book has found its most devoted secret hideout." | Vintage Comic Book | no | HS: Vintage Comic Book only; the hideout refers to its committed pocket preservation, not another container. |  |  |
| HS-H-04 | Overconfidence | J | `{item}` | "From a secret compartment comes something worthy of an immense introduction: {item}." | `{item}` | no | HS: The boast values this preserved item without claiming its quantity or actual worth. |  |  |

## HE — HiddenPocketEmpty

**Required:** HiddenPocketEmpty and location. Empty is not salvage; no line
invents a saved item, transfer, or other settlement outcome.

### Brawler

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-B-01 | Overconfidence | J | `{location}` | "That pocket has a lot of nerve being empty at {location}." | pocket | no | HE: The boast protests the hidden pocket being empty, not a lost or saved item. |  |  |
| HE-B-02 | Genre tension | M | `{location}` | "The hidden compartment at {location} declined to produce a heroic cameo." | compartment | no | HE: The imagined cameo refers to the empty hidden pocket, not a recovered object. |  |  |
| HE-B-03 | Gear obsession | O | - | "The hidden compartment put up a very poor defense against emptiness." | compartment | no | HE: The defensive metaphor describes the hidden pocket being empty, not a fight. |  |  |
| HE-B-04 | Loot greed | J | `{location}` | "The secret pocket returned no souvenir for my appetite at {location}." | pocket | no | HE: The appetite reacts to the empty hidden pocket without inventing a saved item. |  |  |

### Cautious

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-C-01 | Genre tension | J | `{location}` | "My hidden pocket has cast absence in the starring role at {location}." | pocket | no | HE: The theatrical absence is the committed empty pocket, not a saved item. |  |  |
| HE-C-02 | Loot greed | M | `{location}` | "The compartment at {location} has offered my appetite precisely nothing." | compartment | no | HE: Appetite reacts to the empty hidden pocket without claiming a transfer. |  |  |
| HE-C-03 | Overconfidence | O | - | "The empty pocket is no reason to panic about my secret storage." | pocket | no | HE: The understatement concerns the empty hidden pocket without asserting an item. |  |  |
| HE-C-04 | Gear obsession | J | `{location}` | "There's no saved prize hiding behind that seam at {location}." | seam | no | HE: The seam metaphor describes the hidden pocket's emptiness, not an item recovered later. |  |  |

### Gadgeteer

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|---|
| HE-G-01 | Gear obsession | J | `{location}` | "That compartment has optimized itself for emptiness at {location}." | compartment | no | HE: The optimization joke describes the empty hidden pocket, not an item saved. |  |  |
| HE-G-02 | Genre tension | M | `{location}` | "The hidden pocket at {location} has achieved a beautifully uninformative result: nothing." | pocket | no | HE: The technical understatement refers to the empty secret pocket, not a discovery. |  |  |
| HE-G-03 | Overconfidence | O | - | "Even an empty seam can disappoint with impressive precision." | seam | no | HE: The boast concerns the hidden pocket being empty, not a saved item. |  |  |
| HE-G-04 | Loot greed | J | `{location}` | "Even a meticulous collector can't coax anything from that vacant pocket at {location}." | pocket | no | HE: The boast reacts to the empty hidden pocket without claiming an acquisition. |  |  |

### Hoarder

| ID | Mode | Shape | Slots | Candidate line | Concrete | Office | Draft fact and event-specific check | Decision | Reviewer note |
|---|---|---|---|---|---|---|---|
| HE-H-01 | Loot greed | J | `{location}` | "That empty compartment is a personal insult at {location}." | compartment | no | HE: Appetite protests the empty hidden pocket without claiming a saved item. |  |  |
| HE-H-02 | Genre tension | M | `{location}` | "The hidden compartment at {location} offers my hunger nothing." | compartment | no | HE: Hunger reacts to the empty secret pocket without a transfer. |  |  |
| HE-H-03 | Gear obsession | O | - | "Vacant secret storage is an architectural insult to a collector." | storage | no | HE: The architectural metaphor describes the empty hidden pocket, not an inventory change. |  |  |
| HE-H-04 | Overconfidence | J | `{location}` | "Even I can't make an empty hidden pocket produce a keepsake at {location}." | pocket | no | HE: The boast acknowledges the hidden pocket is empty without asserting a later search or find. |  |  |

## Automated review checks

### C3 reproducible screening

**Historical C3 procedure, not the C4 acceptance checker.** The current
working draft is partly revised, and this procedure still counts unrelated
physical props as concrete and still reports the retired per-set office
screen. A replacement must apply event-related grounding to both the
unchanged C3 baseline and the completed C4 draft; do not use its output as
C4 acceptance evidence.

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
The reselected C4 shortlist again contains 152 unique IDs. The historical
whole-row keep check flags notes edited after C3; C4 must separately verify
the protected wording and mode against C3 rather than compare entire rows.
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

**Historical C3 result, not current C4 counts:** C2's ordinary-content stem screen worsened:
maximum three/set after; trait maxima 20/10/16/13 after, versus eight each
before (excluding the combined change stem). Fifty sets exceed two and all
four traits exceed eight. `make` (40), `deserve` (38) and recurring hypothetical
prop constructions remain human-edit priorities; lexical diversity is not
proof of comedy. Thirty-seven sets exceed the earlier 12% **per-set** office
screen even though all traits pass C3's 12% office share. SH-B-03 remains
unchanged.
`--strict` exits zero for C3; `--legacy-strict` exits one and lists these
91 screens at the C3 checkpoint. The 2026-11-15 ruling retired the per-set
office and two-per-set ordinary-stem screens; the C4 recount above reports the
retained three-per-set and eight-per-trait screens separately. Do not treat
either historical or current lexical counts as load readiness.

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
priority_text = after.split("\n1. RS-C-03", 1)[1].split("\nHuman approval", 1)[0]
priority = ["RS-C-03"] + re.findall(r"\b[A-Z]{2}-[BCGH]-\d{2}\b", priority_text)
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
