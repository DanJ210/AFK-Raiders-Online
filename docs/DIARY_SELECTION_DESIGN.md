# Diary selection: D1 slot extraction

This is infrastructure only; no draft lines are loaded. A slot is missing
unless its committed value can be identified without guessing. Extraction
uses only the event supplied to narration and the intended speaking recipient;
it never reads simulation state or consumes randomness.

| Slot | Committed source and form |
|---|---|
| `{location}` | Exact nonblank `RaidEvent.Location`, visible to the participant. |
| `{item}` | Exact `ItemName` for an owner-private item event, or an exact `Awards` entry whose `RecipientId` is the speaker and whose item and recipient are visible in the receipt. Ambiguous awards are missing. Shared resolution events currently pass no structured awards to the narrator; do not infer ownership from another participant's name. |
| `{source}` | Exact source substring of the committed damage/resilience receipt, or knockout `Reason` when the exact `Cause: ...` receipt suffix confirms it. The Downed receipt does not identify a source; incidental word matches do not establish it. A whole sentence is not automatically a grammatical source name. |
| `{condition}` | Exact name at the start of the committed warning or impact receipt, bounded by its known receipt structure and exact committed location. |
| `{robot}` | Exact name in a recognized committed robot-event receipt, bounded by its known receipt structure; ambiguous delimiters are missing. A thread ID is not a name and a later world projection is not evidence. |
| `{activity}` | Lowercase gerund of the exact owner-private `Activity`, or the exact gerund suffix of `PersonalityChanged.Reason` matching `Completed ...`. Interruption must match its receipt. Existing gerunds are lowercased; `FightingRobot` becomes `fighting a robot` and `WaitingForExitDecision` becomes `waiting for an exit decision`. Recognize known activity forms only; unknown or non-gerund forms are missing. |
| `{opportunity}` | Approved explicit `OpportunityId` to display-name mapping only; never output the ID. No mapping is approved yet, so this slot is missing. |
| `{greedDirection}`, `{moodDirection}` | No structured old/new fields exist. Parse the exact `PersonalityChanged` receipt and require its reason to match `Reason`; compare each actual old/new pair after clamping. Equal values are missing, not up or down. Never infer from reason or attempted change. |

Proposal for designer approval, not enabled: use exact titles in
[loot-catalog.json](../server/AFKRaiders.Server/Content/loot-catalog.json).
Titles can describe an activity rather than a container; their grammatical
fit must be reviewed before approving a mapping. A test-only supplied mapping
exercises display-name and raw-ID rejection; it does not approve any entry.

| Opportunity ID | Proposed display name |
|---|---|
| `market-locker` | Staycation Locker |
| `market-lost-property` | Lost-and-Found Recovery |
| `market-shelf-rummage` | Retail-Shelf Rummage |
| `humming-crate` | Humming Crate |
| `yard-parts-salvage` | A.R.C. Parts Salvage |
| `yard-cache-recovery` | Questionable Cache Recovery |
| `depot-locker` | Service Depot Locker |
| `depot-tool-rummage` | Tool-Cage Rummage |
| `depot-drive-salvage` | Records-Terminal Dismantling |

The recipient must be an event participant. No other Raider's private state
is used. A shared event currently gets one shared aside and a null Raider
projection; recipient-specific item asides cannot safely be selected on that
path. Missing structured awards and absent source text are not repaired by
changing gameplay event payloads. Future selection must resolve eligibility
before the existing Part B governor, and preserve the original receipt.

## Extraction measurement (2026-11-15)

The read-only `--slots` run used the actual event delivered to the narrator,
one Raider per cohort, three seeds (101/202/303) and four traits for 24 hours.
There were 82,977 events across 35 observed kinds. Each numerator below is
divided by that row's event count. Unlisted slots have numerator zero. This
measures extractor output, **not candidate eligibility**: the latter also
requires per-row facts, gates and grammatical review. Shared encounters are
not represented by these solo cohorts; unobserved kinds are unmeasured.

| Event kind | Events | Nonzero extractable slots (numerator / Events) |
|---|---:|---|
| ActivityInterrupted | 619 | location 619; activity 608 |
| AreaActivityCompleted | 7345 | location 7345; activity 7345 |
| AreaActivityStarted | 7908 | location 7908; activity 7908 |
| AreaDiary | 21357 | location 21357; activity 21357 |
| Arrived | 3100 | location 3100 |
| ConditionImpact | 1070 | location 1070; condition 1070 |
| ConsumableUsed | 1128 | location 1128 |
| Downed | 79 | location 79 |
| DutyAdmission | 677 | none |
| EncounterResolved | 567 | location 567 |
| Extracted | 591 | location 591 |
| ExtractionDecision | 1859 | location 1859 |
| ExtractionStarted | 623 | location 623 |
| ExtractionVehicleArrived | 604 | location 604 |
| HazardWarning | 1020 | location 1020; condition 1020 |
| HiddenPocketEmpty | 79 | location 79 |
| ItemAllocated | 3304 | location 3304; item 3304 |
| KnockedOut | 79 | location 79; source 79 |
| PackingCompleted | 1652 | location 1652 |
| PackingStarted | 1652 | location 1652 |
| PersonalityChanged | 13590 | location 13590; activity 1244; greedDirection 10519; moodDirection 4235 |
| Resilience | 1793 | location 1793; source 1793 |
| RobotCombatStarted | 630 | location 630; robot 630 |
| RobotDefeatCargoResolved | 408 | location 408 |
| RobotDefeated | 408 | location 408; robot 408 |
| RobotEscaped | 222 | location 222; robot 222 |
| RobotParticipantJoined | 630 | location 630; robot 630 |
| RobotRound | 1294 | location 1294; robot 1294 |
| Search | 1247 | location 1247 |
| SearchCompleted | 677 | location 677 |
| SharedExtractionCalled | 623 | location 623 |
| ShieldAbsorbed | 2272 | location 2272 |
| Travel | 3101 | location 3101 |
| UnmitigatedDamage | 92 | location 92; source 92 |
| WorldCondition | 677 | location 677 |

All 2,272 observed shield receipts had positive charge loss. That is not proof
that zero-loss receipts cannot occur. `CrossedPaths`, `DeadlineExpired`,
`Departure`, `Deployment`, `ExtractionJoined`, `HandlerAction`,
`HiddenPocketSalvaged`, `OpportunityExhausted`, `RecoveryCompleted` and
`RecoveryStarted` were not observed; zero observed is not zero reachable.
Among interruptions, `Ready` accounted for all 11 missing activity values;
it cannot safely be converted to an activity gerund.

In the current narrator input, `Downed` has a source `Reason` but its original
text does not identify that source, and the associated thread is not passed to
the extractor. Thus DN-B-04, DN-C-02, DN-G-02 and DN-H-01 are ineligible under
the visibility rule. `ShieldAbsorbed` likewise does not carry its damage source.
`DutyAdmission` has no event location. Search/encounter resolution emits
awards in receipt text but does not pass structured `Awards` to narration;
the extractor conservatively leaves `{item}` missing without recipient-verified
structured awards. This excludes SI-B-01/02/04/05/07/08,
SI-C-01/02/04/05/07/08, SI-G-01/02/04/05/07/08 and
SI-H-01/02/04/05/07/08 on the current path. A safe receipt-only parser or
access to already committed, recipient-visible thread data could change these
limitations without changing gameplay events; neither is implemented here.
Unapproved opportunity mapping excludes SX-B-01, SX-C-01, SX-G-01 and
SX-H-01 for now. This is an approval gap, not missing gameplay data.
Direction slots are missing for equal values; activity is missing for unknown
or non-gerund activities. No other slot is categorically unsupported: actual
availability is event-specific. These limitations apply to the existing
narrator input, not a proposal to alter gameplay payloads.

## Candidate fact-gate audit

Run `dotnet run --project tools\DiaryVarietyReadOnlyAnalysis\DiaryVarietyReadOnlyAnalysis.csproj -c Release -- --slots`
from the repository root. The tool reads the C5 table and the numbered priority
list without modifying either, checks their 624/152 counts, and evaluates each
candidate against its own trait's actual events and the explicit C5 family,
outcome, slot, sign, positive-charge and named gates. It does not import any
draft line into the production catalog. This is hypothetical fact/slot
eligibility before approval: **497/624** candidates and **108/152** priority
IDs match at least one event. None is approved for selection. The checks do
not replace human review of joke truth, grammar, voice or event specificity;
unobserved is not unreachable, and the solo cohorts do not establish multiplayer
privacy or cross-Raider eligibility.

The 127 IDs with no eligible event in these cohorts are:

```text
AD-B-01 AD-B-02 AD-B-04 AD-C-01 AD-C-02 AD-C-04 AD-G-01 AD-G-02 AD-G-04 AD-H-01 AD-H-02 AD-H-04
DL-B-01 DL-B-02 DL-B-03 DL-B-04 DL-C-01 DL-C-02 DL-C-03 DL-C-04 DL-G-01 DL-G-02 DL-G-03 DL-G-04 DL-H-01 DL-H-02 DL-H-03 DL-H-04
DN-B-04 DN-C-02 DN-G-02 DN-H-01
HS-B-01 HS-B-02 HS-B-03 HS-B-04 HS-C-01 HS-C-02 HS-C-03 HS-C-04 HS-G-01 HS-G-02 HS-G-03 HS-G-04 HS-H-01 HS-H-02 HS-H-03 HS-H-04
PC-B-07 PC-C-04 PC-G-10 PC-H-13 RD-B-02 RD-C-01 RD-H-02
RV-B-01 RV-B-02 RV-B-03 RV-B-04 RV-C-01 RV-C-02 RV-C-03 RV-C-04 RV-G-01 RV-G-02 RV-G-03 RV-G-04 RV-H-01 RV-H-02 RV-H-03 RV-H-04
SI-B-01 SI-B-02 SI-B-04 SI-B-05 SI-B-07 SI-B-08 SI-C-01 SI-C-02 SI-C-04 SI-C-05 SI-C-07 SI-C-08
SI-G-01 SI-G-02 SI-G-04 SI-G-05 SI-G-07 SI-G-08 SI-H-01 SI-H-02 SI-H-04 SI-H-05 SI-H-07 SI-H-08
SN-B-01 SN-B-02 SN-B-03 SN-B-04 SN-C-01 SN-C-02 SN-C-03 SN-C-04 SN-G-01 SN-G-02 SN-G-03 SN-G-04 SN-H-01 SN-H-02 SN-H-03 SN-H-04
SX-B-01 SX-B-02 SX-B-03 SX-B-04 SX-C-01 SX-C-02 SX-C-03 SX-C-04 SX-G-01 SX-G-02 SX-G-03 SX-G-04 SX-H-01 SX-H-02 SX-H-03 SX-H-04
```

The 44 priority IDs with no eligible event are:

```text
AD-B-02 AD-C-02 AD-G-02 AD-H-01 AD-H-02 DL-B-03 DL-C-02 DL-G-01 DL-H-01
DN-B-04 DN-C-02 DN-H-01 HS-B-02 HS-B-03 HS-C-03 HS-G-03 HS-H-02 HS-H-03
RD-B-02 RD-C-01 RD-H-02 RV-B-01 RV-C-01 RV-G-02 RV-H-01 RV-H-02
SI-B-01 SI-B-02 SI-C-05 SI-G-04 SI-G-05 SI-H-05 SN-B-03 SN-C-02 SN-G-03 SN-G-04 SN-H-04
SX-B-02 SX-B-03 SX-C-01 SX-C-02 SX-G-03 SX-H-02 SX-H-03
```
