# BAL-4 tuning proposal

**Status: first experimental arm implemented and measured under user
direction; superseded in part by the measured follow-up arms 1-3
(2026-11-03) in [BAL-4-arms.md](BAL-4-arms.md); not balance-certified.**
The first arm was High 40/18/4, High reward
weights 25/25/15/35, and a deterministic four-owned-weapon cap with 50%-value
overflow liquidation. It met the late all-loot value-order target but missed
both ends of the High risk curve and left Medium's early/late conflict
unresolved. The follow-up arms restored High HP 45 (Arm 1), lowered the
sell-back to 25% (Arm 2), and added a first-24h High readiness gate (Arm 3);
the worktree now holds the cumulative Arm 3 state under
`desperanza-bal-4-arm3-v1`. Medium risk staging and shield tuning remain
deferred. BAL-3 and ECON-3 are out of scope.

## Designer decisions (2026-11-03)

The designer reviewed the measured first arm and set this follow-up contract.
Separately measured experiment arms, one lever per arm, are run as deltas from
the implemented first arm; results live in [BAL-4-arms.md](BAL-4-arms.md).

1. **KO targets use two experimental bands.** Mature play (settlement time 48h
   and later): Low <=10%, Medium 15-25%, High 25-40%. Onboarding (first 24h,
   softer): High <=45%, Medium <=30%, Low <=10%. A 20-30% High
   participant-fight defeat rate over 96h is accepted as the provisional
   target.
2. **Medical ranges** stay Low 0-1, Medium 1-2, High 2-4 per raid (mature
   window).
3. **"Value order" means secured loot value per raid** (High > Medium > Low)
   in the mature window. Guardrail: High mean net asset change must not stay
   negative in the mature window — report it, do not tune to it yet.
4. **Medium robot stats are not staged or retuned in BAL-4.** Medium's
   early/late conflict is hazard-driven and is deferred to BAL-3. Medium stays
   22/12/3.
5. **Shields are held** (price 300, 60% curve, wear x1/x1.5/x2).
6. **Keep the four-weapon cap** and its deterministic ranking.
7. **Keep High reward weights 25/25/15/35 for now**; re-measure after Arm 1
   before judging them.
8. **One lever per arm. No bundling.**

The approved follow-up arms are: Arm 1 — restore High HP 45 (reversal of the
first arm's 45->40 HP change only); Arm 2 — weapon overflow sell-back 50% ->
25% of catalog value on top of Arm 1; Arm 3 — a first-24h new-Raider readiness
gate for High admission on top of Arm 2. Arm 4 (jackpot re-check) is not
started; Arm 1's measurements only report whether that re-check is necessary.

## Summary and recommendation

The rerun confirms a time-shaped BAL-2 risk curve, not a single 96h balance
point. High KO is **54.0%** at 0-8h and **43.0%** at 8-24h (above the
experimental 25-40% band), easing to **30.6%** at 48-96h (in band). Medium is
the reverse problem: **29.0%** at 0-8h is above its 15-25% band, but **9.3%**
at 48-96h is below it. The often-cited High **16.3%** is the pre-BAL-2
BAL-1S aggregate, not current main. The old 57.9% starter High extraction
result is a separate BAL-1S-era cohort and needs rerunning after BAL-2.

**First-arm candidate results (64 matched seeds, 2 Raiders/seed):** In the
candidate, High KO was **64.0%** at 0-8h (71/111; seed-bootstrap 95% CI
55.8-72.5%) and **22.9%** at 48-96h (445/1,943; 19.8-26.2%). Both miss the
25-40% target in opposite directions, and the early estimate is worse than
the BAL-2 control. Medium remained **29.7%** early (398/1,340; 26.0-33.5%)
and fell to **7.7%** late (352/4,565; 6.0-9.6%), so this arm does not solve
the early/late conflict. The changes were bundled; these comparisons do not
isolate causality.

The candidate's 96h all-loot secured value order is High **241.05** >
Medium **102.41** > Low **100.05** per raid. That is not the same as net
asset change: in [0,8) High/Medium/Low mean net changes were **-152.40 /
5.55 / 69.50**, while in [48,96) they were **71.68 / -2.62 / 66.73**.
High reward changes therefore improve secured-value ordering but do not
establish a consistently ordered net-asset outcome. The cap liquidated 2,551
weapons for 97,130 coins over the cohort; inventory stayed at or below four,
but the cash injection is a material inflation risk.

**Selected first experimental implementation arm:**

1. The High HP reduction (45 to 40; keep 18 raw/4 rounds) is implemented, but
   the measured first arm does not support accepting it as a balance fix.
   Opening High KO rose and late High KO fell below target. Test a separate
   cause-focused High candidate before selecting another value.
2. No one static Medium value can lower early KO and raise late KO at once.
   The one-change check should include a global increase and decrease as
   controls, then a clearly labeled staged/readiness candidate; the late
   Medium failures are predominantly hazard-caused, so robot damage alone is
   unlikely to reach 15%.
3. For High value, the two-slot High reward-table tail increase to a 35%
   weight on the existing value-900 item, retaining two shared rewards, the
   12% single-weapon slot, and all ownership rules. This estimates above the
   current Low/Medium all-loot values only under explicit unchanged-defeat/
   unchanged-retention assumptions; it is not a guaranteed result.
4. To address unbounded owned weapon count, the arm implemented an autonomous **four-weapon
   cap with 50% catalog-value liquidation of overflow**. Preserve deterministic
   all-source accounting and protect deployed/needed gear. This is a new
   proposal, not the previously unselected full-price cap.
5. Hold the BAL-1S shield curve and mitigation for the first candidate pass.
   Higher wear/cost may worsen the early KO spikes; lower wear may push mature
   KO below target. Add them as isolated candidates only after readiness and
   cause data are measured.

**Implemented experimental portfolio:** High 40/18/4 (A1/D2), High
reward weights 25/25/15/35 (C3), and the all-source four-weapon cap with
50%-value overflow sale (E3), while holding shields (S1). Keep Medium at
current 22/12/3 as the matched control; run B3 separately only after choosing
the shared-world clock. These are separate experimental arms, not a single
combined tuning batch.

Every candidate remains **experimental**. The first-arm results below are
measured in a 64-seed cohort; other candidate options in this document remain
unrun unless explicitly stated. Confidence intervals are seed-cluster
percentile intervals, not guarantees of future-player outcomes.

## Implemented first arm: before/after measurements

The three selected changes were implemented together; Medium and shields were
held at their BAL-2 values. The candidate uses the same 64 seed IDs and two
Raiders per seed. The 96h candidate provides both time windows below. Candidate
95% intervals are 20,000-resample seed-cluster percentile intervals. Because
the levers interact, these results do not isolate any one change.

| Surface | BAL-2 control | Implemented candidate | Measured result |
|---|---|---|---|
| High profile | 45 HP / 18 raw / 4 rounds | 40 / 18 / 4 | Robot defeats rose overall to 1,070/2,927 fights (36.6%; CI 30.6-42.5%); opening 4/71 (5.6%; 1.3-11.5%), late 833/1,692 (49.2%; 41.3-56.9%). |
| Medium profile | 22 / 12 / 3 | Unchanged | KO remains 29.7% early and 7.7% late; the opposing-window problem remains. |
| High reward table | 30 / 50 / 18 / 2% at item values 35 / 60 / 150 / 900 | 25 / 25 / 15 / 35%; still exactly two shared slots | 96h secured value/raid: Low 100.05, Medium 102.41, High 241.05. Late-window seed-bootstrap 95% CIs: 94.58-97.12 / 108.09-119.89 / 271.67-366.81. |
| Weapon inventory | Uncapped; BAL-2 recorded +2,171 net weapons and 17.80 usable per Raider | At most four owned instances; deterministic overflow sale at 50% catalog value | 2,551 liquidations / 97,130 coins; 5,141 inflows minus 2,279 KO losses and 2,551 liquidations leaves +311 net weapon stock. |
| Heavy shield | Price 300; 60% full-repair curve; wear ×1/×1.5/×2 | Held unchanged | Late High deployments were 44.6% Heavy; shield spend was 21.48 coins/settled raid across all dangers. No shield tuning was tested. |

### KO, healing, and value by danger

| Window | Danger | BAL-2 KO (n; 95% CI) | Candidate KO (n; 95% CI) | Candidate medical/raid | Candidate secured value/raid | Candidate net asset change/raid |
|---|---|---:|---:|---:|---:|---:|
| [0,8) | Low | 0.0% (782) | 0.0% (802; 0-0%) | 0.20 | 79.49 | 69.50 |
| [0,8) | Medium | 29.0% (1,359; 25.23-32.61%) | 29.7% (1,340; 26.0-33.5%) | 0.91 | 55.07 | 5.55 |
| [0,8) | High | 54.0% (100; 44.00-64.42%) | 64.0% (111; 55.8-72.5%) | 1.79 | 21.10 | -152.40 |
| [48,96) | Low | 0.1% (5,177; 0.06-0.23%) | 0.0% (4,987; 0-0%) | 0.09 | 95.88 | 66.73 |
| [48,96) | Medium | 9.3% (4,676; 7.98-10.65%) | 7.7% (4,565; 6.0-9.6%) | 1.66 | 114.10 | -2.62 |
| [48,96) | High | 30.6% (1,802; 27.68-33.73%) | 22.9% (1,943; 19.8-26.2%) | 2.39 | 319.10 | 71.68 |

This makes the early window materially different from the mature window.
Candidate High is too lethal early and too easy late; candidate Medium misses
both KO bands. Late High medical use is within its proposed 2-4 range, while
early High is below it. The candidate's late secured-value ordering is
High > Medium > Low, but neither window's net-asset ordering is consistently
High > Medium > Low.

### Trait-level KO outcomes

Each cell is KO rate (settlements; seed-bootstrap 95% CI). Low KO was 0% for
each trait in both windows; its measured trait counts are in the generated
reports.

| Danger | Trait | [0,8) | [48,96) |
|---|---|---:|---:|
| Medium | Brawler | 29.1% (337; 22.2-35.6%) | 8.2% (1,152; 4.6-12.1%) |
| Medium | Cautious | 28.7% (341; 23.4-34.2%) | 7.4% (1,148; 4.4-10.7%) |
| Medium | Gadgeteer | 27.3% (326; 21.1-33.8%) | 6.9% (1,136; 3.6-10.8%) |
| Medium | Hoarder | 33.6% (336; 26.7-40.5%) | 8.4% (1,129; 5.4-11.7%) |
| High | Brawler | 71.4% (28; 50.0-92.0%) | 24.5% (485; 19.2-30.7%) |
| High | Cautious | 58.6% (29; 45.5-72.0%) | 22.1% (475; 16.2-28.9%) |
| High | Gadgeteer | 60.0% (30; 44.0-78.6%) | 15.8% (514; 10.9-21.5%) |
| High | Hoarder | 66.7% (24; 44.8-86.4%) | 29.9% (469; 23.1-36.9%) |

Small opening High trait cells have wide intervals. These are descriptive
seed-cluster estimates, not trait-specific tuning guarantees.

## Evidence read

This proposal reconciles the checkpoint and BAL-0/BAL-1/BAL-1S/BAL-2 ledger
entries with the [BAL-0 baseline](BAL-0-baseline.md),
[BAL-1S measurements](BAL-1S-measurements.md),
[post-BAL-1S gate](BAL-1S-post-BAL-1-gate.md),
[BAL-2 measurements](BAL-2-measurements.md), and
[approved BAL-2 proposal](BAL-2-proposal.md). The [progress ledger](../IMPLEMENTATION_PROGRESS.md)
remains the handoff authority.

## Scope and frozen contracts

Tuning may consider BAL-1, BAL-1S, and BAL-2 values and the cost curves or
weapon sinks described here. Keep frozen:

- Trader-only Temptrest; no Temptrest field/reward-table drops.
- Exactly once-per-successful-raid weapon wear; no per-hit or per-round weapon
  wear.
- Deterministic ownership and allocation, and once-only settlement.
- The autonomous Raider economy; no Handler buy/sell/equip/repair controls.
- No direct PvP, theft, or saved opponents presented as live participants.
- **Superseded 2026-11-06:** the former no-AI-authority limit on combat, decisions, purchases, or outcomes was removed; see the [decision authority checkpoint](../IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint). This historical proposal remains unchanged otherwise.

No Handler Signal, simulation scheduling, extraction/KO/pocket semantics,
shared-world ownership, or reward count is changed by this proposal.

## Baseline verification and comparison

The BAL-2 control rerun was on branch `main`, HEAD
`ea2515aa2fffc31e20d0d5c206ac5e0cfa05d1e8`, with pre-existing uncommitted
BAL-2/runtime and documentation edits. It measured the current **worktree**
under `desperanza-bal-2-v1`; this is not a clean-HEAD reproduction. After the
user redirected the task to implementation, the selected first arm was
implemented and measured under `desperanza-bal-4-v1`. No backend was restarted.

Both horizons used 64 seeds, two unattended Raiders, pinned IDs/trader seed,
five-second scheduling, recurring autonomous economy, and no Handler Signals.
The 8h run is standalone. The 96h run also supplies true-settlement-time
half-open windows. The 96h per-seed raw telemetry was written to a temporary
file, not the repository. KO and net-asset intervals below use 20,000
seed-cluster bootstrap resamples (64 seed clusters, resampling both Raiders
and their raids together); intervals are approximate percentile 95% intervals.
The BAL-0 report itself has no trait breakdown.

| Danger | Standalone 8h n | KO 8h | Medical/raid 8h | 96h n | KO 96h (seed-bootstrap 95%) | Medical/raid 96h | Net asset change/raid 96h (seed-bootstrap 95%) |
|---|---:|---:|---:|---:|---:|---:|---:|
| Low | 782 | 0.0% | 0.20 | 9,966 | 0.1% (0.06-0.19%) | 0.12 | 80.09 (79.14-81.03) |
| Medium | 1,360 | 29.0% | 0.96 | 10,571 | 13.5% (12.36-14.72%) | 1.58 | 0.71 (-2.07-3.55) |
| High | 100 | 54.0% | 1.68 | 3,240 | 34.4% (32.16-36.63%) | 2.45 | -174.78 (-183.38 to -166.01) |

The confidence intervals are clustered by simulation seed, but do not capture
model uncertainty or uncertainty from changing the rules. The first-raid High
extraction comparison in the ledger is a separate 120-seed x 2-Raider,
BAL-1S-era cohort: 3/240 (1.25%) before BAL-1S versus 139/240 (57.9%) after
BAL-1S. BAL-2 later changed the robot profile, rewards, and Depot timing, so
57.9% must be rerun before treating it as current-main extraction evidence. A simple
binomial Wilson 95% interval for 139/240 is 51.6-64.0%; its seed-cluster
interval was not reported. This is extraction, not KO, and it is not the
recurring staged-cohort KO metric.

### Early versus mature BAL-2 control outcomes

The 96h run's [0,8) rows are close to the standalone 8h results; the one
Medium settlement at exactly 8h explains its n of 1,359 rather than 1,360.
This first-hours window is not close to 48-96h:

| 96h settlement-time window | Danger | n | KO rate (seed-bootstrap 95%) | Medical used/raid |
|---|---|---:|---:|---:|
| [0,8) | Low | 782 | 0.0% (0.00-0.00%) | 0.20 |
| [0,8) | Medium | 1,359 | 29.0% (25.23-32.61%) | 0.96 |
| [0,8) | High | 100 | 54.0% (44.00-64.42%) | 1.68 |
| [8,24) | Low | 1,582 | 0.2% (0.00-0.43%) | 0.13 |
| [8,24) | Medium | 1,969 | 16.2% (13.82-18.54%) | 1.51 |
| [8,24) | High | 423 | 43.0% (38.10-48.31%) | 2.34 |
| [24,48) | Low | 2,425 | 0.1% (0.00-0.21%) | 0.11 |
| [24,48) | Medium | 2,567 | 11.0% (9.28-12.77%) | 1.66 |
| [24,48) | High | 915 | 35.6% (31.82-39.57%) | 2.48 |
| [48,96) | Low | 5,177 | 0.1% (0.06-0.23%) | 0.10 |
| [48,96) | Medium | 4,676 | 9.3% (7.98-10.65%) | 1.75 |
| [48,96) | High | 1,802 | 30.6% (27.68-33.73%) | 2.51 |

### Unconfirmed target contract

For review, the stated experimental bands are Low KO <=10%, Medium KO
15-25%, High KO 25-40%; medical items used per raid Low 0-1, Medium 1-2,
High 2-4; and mean net value High > Medium > Low. Confirm whether KO bands
apply to each time window as well as the horizon aggregate. The current 96h
medical means meet those ranges (0.12 / 1.58 / 2.45), but the opening-window
Medium mean is 0.96 and High is 1.68, both below their proposed floors.
The value-order target is ambiguous until “net value” is explicitly defined;
neither current all-loot secured value nor net asset change has High > Medium
> Low.

Thus the 96h aggregate masks a High KO fall of 54.0% to 30.6% from opening
to mature play. The proposed medical ranges are met by the 96h aggregates
(0-1 / 1-2 / 2-4), but opening-window Medium is slightly below its medical
range and High is below its 2-item floor. If the bands are intended to apply
to each window, current values do not meet that contract.

### KO is not extraction; net value is not gross loot

The 96h High KO result now meets the stated band, but the first-raid extraction
rate remains much higher than the pre-shield result. Do not treat either number
as a substitute for the other. The 8h/96h evidence also shows that “dangerous”
is a windowed outcome, not a single cohort average.

Using the BAL-2 definition of net asset change (coins, stash, owned weapons,
owned supplies, and shield at catalog value, with inter-raid preparation
attributed to the following raid), the order is Low **80.09** > Medium **0.71**
> High **-174.78** coins/raid. Seed-cluster intervals do not overlap. This
fails the proposed High > Medium > Low objective by a wide margin. Gross
secured value is a different measure (Low/Medium/High 99.16/97.50/59.47 per
raid) and also does not have the requested order.

There are no trait-stratified results in the requested reports or current
measurement telemetry. Trait-specific forecasts are therefore **unavailable**,
not evidence of no trait effect. The four traits have different reserves and
shield-repair thresholds (Cautious 40% reserve / 75% repair threshold; Hoarder
50% / 50%; Gadgeteer 25% / 50%; Brawler 15% / 25%), so overall means cannot
predict individual trait outcomes.

## A. High risk is too lethal early

The BAL-2 control profile is High **45 HP / 18 raw / 4 rounds**. Its observed KO
curve is 54.0% at 0-8h (n=100; seed-bootstrap 95% 44.00-64.42%), 43.0% at
8-24h (n=423; 38.10-48.31%), and 30.6% at 48-96h (n=1,802; 27.68-33.73%).
The opening [0,8) High KO causes were Overtime Red 35, robot swipe 18, and
deadline 1. A robot-only adjustment cannot remove the hazard-caused share.
The BAL-1S-era 57.9% first-raid extraction rate is not a current BAL-2 result;
rerun it with the current profile/reward rules.

| Candidate | Exact change | Predicted by danger | Evidence, seed count, uncertainty |
|---|---|---|---|
| A1 — Lower High HP (implemented first arm) | High HP 45 -> 40; raw 18 and 4 rounds stay fixed. Low/Medium unchanged. | The hypothesis was fewer late retaliations and more defeats. | Measured with 64 seeds: High KO 64.0% early (n=111; 55.8-72.5%) and 22.9% late (n=1,943; 19.8-26.2%). It worsened the early miss and pushed the late result below target. |
| A2 — Lower High impact | High raw damage 18 -> 16; HP 45 and 4 rounds stay fixed. | Low/Medium direct outcomes unchanged. High robot HP loss should fall; hazard-caused KO remains. Direction: High KO down in every window, with late result at risk of dropping below 25%. | Unrun (0 candidate seeds). Current 0-8 High robot/hazard HP loss is 18.24/36.29 per raid; 96h is 24.07/38.32. Hold-hit-count arithmetic gives about 21.4 robot HP loss/High raid, not a KO forecast. |
| A3 — Readiness-gated High admission | During a Raider's first 24h, start/admit to a High world only if every participant has a usable weapon, at least one medical item, and an unbroken shield; keep one combat profile for the shared world. | Intended to lower early High KO while leaving mature High untouched. Low/Medium frequencies and denominators may shift; it does not make an unready High instance safer if the gate is bypassed. | Unrun (0 candidate seeds). Requires a deterministic age clock, coordinator admission rule, and multi-Raider fairness design; completion/queue effects unknown. |

Do not stack A2 with another unmeasured High change. A3 is a larger
product/pacing change and could starve High worlds or route the same actual
Raiders into lower dangers. The measured A1 result means the earlier
"fewer retaliation rounds" hypothesis is not sufficient to predict KO.

## B. Medium is too lethal early and too easy late

Medium KO is 29.0% at 0-8h (n=1,359; seed-bootstrap 95% 25.23-32.61%) and
falls to 9.3% at 48-96h (n=4,676; 7.98-10.65%). This is an opposing-direction
miss. In the late window, 393 of 435 KOs were Apology Weather, 39 were robot
swipes, and 3 were deadline failures. In the opening window, 330 of 394 KOs
were weather and 64 were robot swipes.

| Candidate | Exact change | Predicted by danger | Assessment |
|---|---|---|---|
| B1 — Global robot increase | Medium raw damage 12 -> 13; HP 22/3 rounds unchanged. | Low/High unchanged directly. Medium KO likely rises in both early and late windows. | May help late 9.3%, but worsens early 29.0%; cannot satisfy both windows alone. |
| B2 — Global robot decrease | Medium raw damage 12 -> 10; HP 22/3 rounds unchanged. | Low/High unchanged directly. Medium KO likely falls in both windows. | May help early, but worsens late; not a single-change solution. |
| B3 — Staged risk/readiness trial | During first 24h, Medium raw 10; at 48h+, restore raw 12 and raise Medium shield wear 1.5 -> 2.0. Keep HP 22/3 rounds. | Intended to lower early Medium robot KO and raise late shield attrition; late hazard KO is dominant, so the late 15% target may still be missed. | Two phase-specific levers, not one static value. Requires one deterministic clock basis for every participant in a shared world; that choice is unresolved. Unrun (0 seeds). |

The data reject a claim that one static Medium robot value can fix both
windows: early needs less KO while late needs more. A global shield mitigation
increase also moves both rates down; a global wear/price increase moves risk
and affordability the wrong way for early Medium. If the designer requires
both bands by window, approve a staged risk/readiness rule or revise the
window target; do not hide the conflict in an aggregate.

The BAL-4 harness now records the primary trait at each settlement. Candidate
trait-level KO outcomes and seed-cluster uncertainty are shown in the measured
table above; those do not establish causal trait effects because all changes
were combined.

## C. High reward and value order

Current all-loot secured value per raid is Low 99.16, Medium 97.50, High
59.47. The current two-slot High table has expected gross bundle value
177.54 per defeated world, but only 384/2,701 participant fights were
defeated (14.2%; 64-seed bootstrap 95% 11.33-17.36%). Its observed High
robot-cargo value secured is about 17.05 per raid when weighted across the
96h windows; the remaining approximately 42.42 is other secured loot.

Keep the approved structure: two shared world-scoped defeat rewards, one
nonweapon in slot A, slot B's existing 12% single-weapon chance, at most one
weapon/world, no extra Depot draw, no guaranteed currency, and unchanged
ownership/extraction/pocket rules. The candidates change only High cargo
weights and use the existing value-900 item.

| Candidate | High weights by value 35 / 60 / 150 / 900 | Expected bundle gross value (Low / Medium / High) | High all-loot value/raid |
|---|---|---:|---:|
| C1 — Current control | 30 / 50 / 18 / 2% | 25.06 / 76.805 / 177.54 | BAL-2 observed 59.47 |
| C2 — Medium tail | 25 / 35 / 15 / 25% | 25.06 / 76.805 / 538.03 | First-order estimate about 94.1; still below BAL-2 Low/Medium |
| C3 — Raise High above the other two (implemented first arm) | 25 / 25 / 15 / 35% | 25.06 / 76.805 / 695.95 | Candidate measured 241.05 overall; [48,96) 319.10 (n=1,943; 271.67-366.81) |

The first-order value estimate scales the current secured High robot cargo
(17.05) by the new/current expected bundle ratio and leaves other High loot
(42.42) fixed. It is not a simulation and has no campaign-value confidence
interval. As a separate gross proxy, bundle expectation times the observed
14.2% defeat rate gives 25.2 / 76.4 / 98.8 per participant fight for current /
C2 / C3. Applying only the observed defeat-rate interval (11.33-17.36%) gives
20.1-30.8 / 60.9-93.4 / 78.9-120.8. These are arithmetic intervals, not
candidate confidence intervals: they assume defeat frequency stays fixed and
ignore shared-world correlation, cargo allocation, KO, and extraction. C3's
35% jackpot weight makes at least one value-900 item appear in about 55% of
defeated High worlds (current table: about 3.7%). This is a large inflation
risk. The 64-seed candidate reached the requested late secured-value order
(High 319.10, Medium 114.10, Low 95.88 per raid in [48,96)); opening
secured value still ranked Low 79.49, Medium 55.07, High 21.10. Report
p50/p90/p99 and a leave-jackpot-out result before treating C3 as accepted.

## D. High robot defeats are rare

BAL-2 had 0/59 High defeats in the standalone 8h cohort and 384/2,701 at 96h
(14.2%). In the implemented first arm, High defeats rose to 4/71 in [0,8)
(5.6%; seed-bootstrap 95% 1.3-11.5%) and 833/1,692 in [48,96) (49.2%;
41.3-56.9%); across 96h there were 1,070/2,927 (36.6%; 30.6-42.5%). Thus
the top-tier reward is still uncommon in the opening cohort but much more
common late; the late rate may now be excessive.

| Candidate | Exact change | Predicted High defeat outcome | Risk |
|---|---|---|---|
| D1 — Current BAL-2 control | 45 HP / 18 raw / 4 rounds | Observed 0/59 at 8h and 14.2% at 96h (11.33-17.36% seed-bootstrap interval). | Top-tier loot remains rare. |
| D2 — Shorter defeat threshold (implemented first arm) | High robot HP 45 -> 40; keep 18 raw, 4 rounds. | Measured 5.6% opening and 49.2% late High fight defeat rates; 36.6% across 96h. | Increased late reward exposure alongside late KO below target; the combined arm does not show whether HP alone caused this. |
| D3 — More time to defeat | Keep 45 HP / 18 raw; rounds 4 -> 5. | Expected defeat frequency up from an extra Raider attack round, but adds an extra robot retaliation to failures. | Conflicts with reducing early High KO; do not combine with D2 without a separate experiment. |

For review, a provisional “sensible” diagnostic is 20-30% High participant-fight
defeats over 96h, plus a nonzero opening-window rate, while keeping High KO
within 25-40% in each accepted window. This is experimental and needs designer
confirmation. The measured first arm exceeds the provisional late defeat
range and misses KO in both windows; do not assume further HP reduction is
the answer.

## E. Weapon accumulation: three alternatives

Current BAL-2 still has positive net weapon inflow: 4,727 in versus 2,556
equipped-weapon KO losses over 96h (+2,171); closing stock averages 17.80
usable and 0.16 broken weapons per Raider. The 8h cohort is already +224.
Wear alone is not a stock sink: broken owned guns remain owned, and un-equipped
guns do not receive the once-per-successful-raid wear.

| Candidate | Exact policy | Predicted inventory/economy effect | Trade-off |
|---|---|---|---|
| E1 — Repair cost only | Keep uncapped stock; multiply existing weapon repair costs 18/30/55 by 1.25, rounding up: Aspperigo 23, Vernerider 38, Temptrest 69. | No bound on inventory; no candidate runs. More coin spent at repairs, but Raiders may use loaner Kettles or keep more unrepairable spares. | A possible coin sink, not a solution to unlimited owned weapons. Not recommended as the sole change. |
| E2 — Four-slot, full-value sale (unselected BAL-2 alternative) | Maximum four unique owned weapon instances across starter, purchase, loaner, and secured field/pocket sources. Normalize after securing; protect deployed/needed fallback gear; retire overflow deterministically at full catalog value (Aspperigo 65, Vernerider 140, Temptrest 280; Kettle 0). | Hard maximum of four after each committed addition. Exact sale proceeds depend on source, loss, and equipment ranking; no BAL-4 candidate run. | Existing unselected alternative. Strong cash injection and potential shield/weapon progression spike; not the recommendation. |
| E3 — Four-slot, half-value sell-back (implemented first arm) | Same all-source four-instance cap and deterministic retention as E2. Retire overflow at `floor(0.50 × catalog value)`: Aspperigo 32, Vernerider 70, Temptrest 140; Kettle 0. No Handler sale control. | Hard maximum of four after each committed addition. Measured 2,551 liquidations / 97,130 coins over 64 seeds x 96h; mean final owned weapons 3.43/Raider. | Bounds stock, but the measured cash flow is substantial and needs inflation review. |

| Danger | Current 96h raids | Weapons found / KO-lost | E2/E3 expected direction |
|---|---:|---:|---|
| Low | 9,966 | 2,086 / 12 | More surplus likely reaches the cap because KO loss is rare; sale cash and later preparation change are unmeasured. |
| Medium | 10,571 | 1,601 / 1,430 | Less net inflow than Low; cap sale/replacement effects unmeasured. |
| High | 3,240 | 281 / 1,114 | KO already removes many equipped guns; cap also retires unequipped surplus, but cannot protect the current equipped gun from KO. |

Use the BAL-2 cap alternative's deterministic retention order if a cap is
approved: protect the currently equipped instance and any needed usable
fallback, then prefer usable weapons, higher expected BAL-1 damage, higher
durability ratio, and ordinal catalog/instance IDs. Never protect a deployed
weapon from normal KO loss. Count broken/duplicate catalog weapons as owned
instances; do not count trader stock or unsecured backpack loot. Enforce the
limit at purchases, loaner creation, field/pocket conversion, and home
normalization—not only at final settlement.

The implemented E3 sale event is journaled once, attributed to the owner, and
included in a stock-conservation equation. Use measured receipts, not an
assumed number of overflow weapons, to forecast coins. Danger-level results
are reported by the harness; candidate cap variants and E2 remain unrun, and
sales are not yet broken down by trait.

## Shield cost, repair curve, and wear candidates

Current Light/Medium/Heavy prices are 30/90/300 coins. The current repair
curve is `ceil(price × (0.25s + 0.35s²))`, where `s` is the repaired fraction:
full repair is 60% of price (18/54/180); a 50-point Heavy repair costs 64.
Wear multipliers are Low/Medium/High ×1/×1.5/×2. The current shield data show:

- BAL-1S 96h: Heavy is 39.2% of High deployments; total shield purchase and
  repair spend is 20.67 coins per settled raid.
- Current BAL-2 96h: High Heavy share is 26.0%; total spend is 18.95 coins per
  settled raid. This is a materially different baseline from the 39.2% figure.
- BAL-2 opening High sample: n=100, Heavy 1.0%, Light 46.0%, Medium 53.0%;
  no started-broken shields. High sample size is small.

| Candidate | Heavy price | Repair curve (full repair %) | Wear multipliers Low/Med/High | Exact Heavy repair examples | Expected effect / risk |
|---|---:|---|---|---|---|
| S1 — Hold (first-arm control) | 300 | 0.25s + 0.35s² (60%) | 1 / 1.5 / 2 | Full 180; 50 points 64 | Preserves the measured policy while diagnosing risk; it does not resolve the opening High KO miss. |
| S2 — Moderate cost/wear | 360 | 0.30s + 0.40s² (70%) | 1 / 1.5 / 2.25 | Full 252; 50 points 90 | Raises Heavy purchase and repair burden; likely reduces Heavy upgrades and increases High durability failures. |
| S3 — Strong cost/wear | 450 | 0.35s + 0.45s² (80%) | 1 / 1.5 / 2.5 | Full 360; 50 points 130 | Strongest pressure on Heavy use and repair; highest unshielded-streak/death-spiral risk. |

S2/S3 are unrun; there is no supportable numeric prediction for Heavy share,
shield spend, KO, or unshielded streaks. Apply any selected price/curve
uniformly unless a separate trait-specific rule is approved. Cautious reaches
the 75% repair trigger first; Hoarder/Gadgeteer use 50%, Brawler 25%. A higher
cost can push each trait into different partial-repair or unshielded behavior;
the existing aggregate report cannot quantify those differences. In the
implemented 96h first arm, Heavy was 44.6% of all High deployments (but 0% in
the opening High sample); total shield spend was 21.48 coins/settled raid.
Prices, repair curve, mitigation, and wear were held unchanged, so these
outcomes do not estimate S2/S3.

| Danger | Current 96h KO (n; seed-bootstrap 95%) | S2/S3 expected direction | Trait-specific result |
|---|---|---|---|
| Low | 0.1% (9,966; 0.06-0.19%) | Lower affordability can shift deployments down-tier; direct KO change likely smaller than Medium/High but is unmeasured. | Not reported. |
| Medium | 13.5% (10,571; 12.36-14.72%) | Less repair/upgrade access can push KO up; already below the proposed 15% floor. | Cautious repair trigger is earliest; exact effect unmeasured. |
| High | 34.4% (3,240; 32.16-36.63%); opening window 54.0% (n=100) | Less Heavy access and faster wear can push KO/unshielded streaks up; opening window is already above target. | Brawler repairs later than the other traits; costs/reserves still differ by trait. No per-trait sample. |

## BAL-2 profile/reward interaction cross-check

The BAL-2 control profiles are Low **12 HP / 8 raw / 3 rounds**, Medium **22 / 12 / 3**,
and High **45 / 18 / 4**, with a 5-second round interval. The implemented
candidate changes only High to **40 / 18 / 4**. Rewards are two
shared slots on a committed world defeat, not guaranteed per Raider. The
High table's current expected bundle is 177.54 per defeated world; C2/C3
increase that to 538.03/695.95 without adding slots or changing the weapon
chance. at the control's 14.2% defeat rate this change was expected to be attenuated
per participant fight and further by sharing, KO, and extraction. The
implemented combined arm instead observed 36.6% High defeats across 96h and
secured-value order High > Medium > Low. Its 49.2% late defeat rate and 22.9%
late High KO warrant a follow-up before accepting those values.

## F. Net asset and coin trend

BAL-2 mean net asset gain falls from **22.16 coins/raid** in [0,8h) to
**6.41** in [48,96h), while remaining positive. Closing coins per Raider fell
from **377.09** under BAL-1S to **221.29** under BAL-2 (about 41% lower);
closing stash rose 119.45 -> 160.23 and supplies fell 103.23 -> 53.66.
This is not automatically unhealthy: slowing positive asset growth can curb
runaway accumulation, but lower cash and supplies may signal liquidity or
readiness pressure. The aggregate means do not show the coin-poor tail,
missed purchases, or unpaid repairs, so they cannot settle that question.

The cap-four sale-back did add substantial coin flow: 97,130 coins across the
64-seed, 96h cohort (about 759 per Raider). The report's end-of-run endpoint
averaged 4,295.94 coins per Raider, versus BAL-2's 221.29 closing-stock
snapshot; the endpoints are not perfectly identical, but the gap is large
enough to flag inflation. The High jackpot table also creates uneven asset
value. Treat the cap as successful at bounding count, not yet as an acceptable
economy sink. Before keeping 50% sell-back, measure cash percentiles,
purchase/repair failures, reserve violations, and trait-specific stock; a
lower sell-back rate or a non-cash sink remains an untested alternative.

| Candidate | Expected direction | Danger/trait limits |
|---|---|---|
| F1 — Hold BAL-2 economy | Net gain stays positive but may continue to taper; no new coin source. | BAL-2 96h net change Low 80.09, Medium 0.71, High -174.78 per raid; closing coins 221.29. |
| F2 — E3 half-value cap-four (implemented first arm) | Weapon stock bounded; overflow creates coins. | Measured 97,130 liquidation coins; first-arm net asset change by window is above. Mean final cash reached 4,295.94/Raider at the run endpoint. |
| F3 — Raise repair costs 25% | Coin drain rises if repairs continue; may reduce cash and equipment readiness. | Unrun. Cannot bound weapon count; effects on KO, danger mix, and trait reserves are unknown. |

This coin outcome is not healthy as an unexamined side effect. It may ease
liquidity, but the magnitude risks restarting the accumulation loop that the
cap was meant to control. Do not count this as an approved value; the sell-back
rate is an explicit designer decision.

## G. Why High settlements fell from 216 to 100 in the first 8h

This is a **settlement-count difference**, not a proven admission-rate
difference. In matched standalone 8h cohorts, total settlements increased
2,189 -> 2,242 while High settlements fell 216 -> 100; High participant
fights fell 147 -> 59. The first-arm candidate recorded 111 High settlements
and 71 participant fights in its 96h run's [0,8) window, a small rebound from
100/59 but still far below 216/147. BAL-2 also changed robot outcomes sharply: High
defeats went 47/147 -> 0/59, while High KO rose 38.9% -> 54.0%. More High
escapes and KOs can alter how quickly Raiders return to the autonomous
economy and make another danger choice; the changed risk/reward profile can
also alter the danger mix. These mechanisms are plausible, not isolated
causes. The reports count settled raids, not offers, admissions, abandoned
worlds, or unfinished raids at hour eight. Add those events and Raider-level
time-to-next-raid before claiming that BAL-2 reduced High admissions.

## H. Robot damage unit

Robot damage is **per round, per active Raider**, not per weapon hit. In each
5-second `ResolveRobotRound`, active Raiders attack the shared robot first;
if it survives, each active Raider receives one robot retaliation at the
profile's raw damage (High currently 18). The killing round has no retaliation.
In a shared fight, each active Raider can receive that round's damage. Shield
mitigation applies to the incoming impact. The 18 value is not a random
per-round range and is not multiplied by the number of weapon attacks.

## Exact implemented values and file map

These are the values in the experimental first arm, not a balance approval.
The BAL-4 rules version and compatibility fingerprint were advanced for the
changed reward/profile behavior.

| Implemented value or behavior | Files changed |
|---|---|
| High robot HP 45 -> 40; raw 18 and 4 rounds unchanged | [RaidConditions.cs](../../server/AFKRaiders.Server/Domain/RaidConditions.cs) |
| High reward weights 30/50/18/2 -> 25/25/15/35 by value 35/60/150/900; two shared slots and existing weapon chance retained | [RobotDefeatRewards.cs](../../server/AFKRaiders.Server/Domain/RobotDefeatRewards.cs) |
| BAL-4 rules version `desperanza-bal-4-v1`, reward version `bal-4-defeat-v1`, and compatibility fingerprint `ABCA8643017ABF32A6D5709CAFB63E387C34A442D8F5D61E368221AA604BA6E6` | [RaidSimulation.cs](../../server/AFKRaiders.Server/Domain/RaidSimulation.cs), [RobotDefeatRewards.cs](../../server/AFKRaiders.Server/Domain/RobotDefeatRewards.cs), [NarrationCompatibilityTests.cs](../../tests/AFKRaiders.Server.Tests/NarrationCompatibilityTests.cs) |
| Four unique owned weapon instances across sources; deterministic retention and overflow sale at floor(50% catalog value), once-only owner receipts and coins | [WeaponInventoryCap.cs](../../server/AFKRaiders.Server/Domain/WeaponInventoryCap.cs), [RaidCoordinator.cs](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs), [RaiderStatistics.cs](../../server/AFKRaiders.Server/Domain/RaiderStatistics.cs) |
| Seed-cluster balance reports, trait-per-settlement and cap conservation checks | [BalanceMeasurementTests.cs](../../tests/AFKRaiders.Server.Tests/BalanceMeasurementTests.cs) |
| Cap/risk/reward regression coverage and compatibility fingerprint | [WeaponInventoryCapTests.cs](../../tests/AFKRaiders.Server.Tests/WeaponInventoryCapTests.cs), [WeaponCombatTests.cs](../../tests/AFKRaiders.Server.Tests/WeaponCombatTests.cs), [Bal2RobotDefeatTests.cs](../../tests/AFKRaiders.Server.Tests/Bal2RobotDefeatTests.cs), [ShieldTests.cs](../../tests/AFKRaiders.Server.Tests/ShieldTests.cs), [NarrationCompatibilityTests.cs](../../tests/AFKRaiders.Server.Tests/NarrationCompatibilityTests.cs) |
| Liquidation totals in the read-only stats panel; current behavior and experimental status | [RaiderStatsPanel.vue](../../web/src/components/RaiderStatsPanel.vue), [README.md](../../README.md), [GAME_DESIGN.md](../GAME_DESIGN.md), [MULTIPLAYER_REBUILD_BRIEF.md](../MULTIPLAYER_REBUILD_BRIEF.md), [IMPLEMENTATION_PROGRESS.md](../IMPLEMENTATION_PROGRESS.md) |

Medium remains 22/12/3. Shield price, repair curve, mitigation, and wear
remain unchanged. A2/A3, B1/B2/B3, C2, D3, E1/E2, F3, and S2/S3 are proposal
alternatives only; no BAL-3 or ECON-3 work is included.

## Risks and what could go wrong

- **Economy inflation:** full-price sales, excessive High jackpot frequency, or
  new liquid value can buy better weapons/shields earlier, feeding back into
  survival and later loot. Half-price liquidation still creates coins.
- **Death spirals:** higher Heavy prices/repair costs can consume the protected
  reserve, delay healing/weapon repairs, trigger weaker shields, and cause more
  KO losses. A cap must not discard the Raider's needed fallback or repair
  target.
- **Unshielded streaks:** reduced Heavy affordability or faster High wear may
  increase streak count/length; existing max 10 is a measurement, not a
  guarantee after changes.
- **Risk overshoot:** the implemented first arm measured 64.0% opening High
  KO and 22.9% in [48,96). It worsened the opening miss and pushed the late
  result below target. Any next change needs a separate candidate rather than
  assuming lower HP/raw improves every window.
- **Reward not secured:** BAL-2 cargo is risky until extraction, shared by
  contributors, and may be lost on KO except for the existing designated
  pocket item. Gross table expectation is not retained net value.
- **Candidate uncertainty:** the harness has 64 seeds and two Raiders per
  seed. Trait is now recorded, but the small opening High trait cells have
  wide intervals. It does not estimate player populations, broader online
  concurrency, or loadout-specific staged KO without a separate cohort.

## Follow-up test and measurement plan

1. Decide whether the KO target applies in each time window and define whether
   the value-order target means secured loot or net asset change. Treat the
   current arm as experimental evidence, not an accepted balance state.
2. Keep the 64-seed BAL-2 control and first-arm run as references. For any
   next candidate, change one risk/reward lever at a time where possible;
   use matched seeds 1-64, two unattended Raiders, pinned IDs/trader seed,
   no Signal, and report 8h/96h with [0,8), [8,24), [24,48), [48,96) windows.
3. Add a first-raid starter and staged High cohort with starting shield,
   weapon, staged supplies, trait, and outcomes recorded. Keep the existing
   120-seed x 2-Raider sample as reference; target at least 64 seed clusters
   for each candidate and report uncertainty, not just point estimates.
4. Capture trait with every settled raid and report by danger x trait with
   denominators. If cells are sparse, retain the cells and report their low
   sample size and uncertainty; do not imply an absent result is zero.
5. Report KO and extraction separately; KO cause/time; robot versus hazard HP;
   rounds and solo/shared weapon/shield outcomes; medical used, carried,
   restored and wasted; shield tier/wear/repairs, spend and unshielded streaks.
6. For cap/sell-back candidates, assert all-source inventory conservation and
   the selected cap after every committed addition; verify deterministic
   retention, exact sale receipts, no duplicate sale, absent-versus-polled
   parity, retry and repeated-settlement idempotency. Distinguish broken from
   retired.
7. Report net asset change by danger/window and seed-cluster intervals, gross
   secured value, cash, stock, and High reward median/p90/p99 plus a
   leave-jackpot-out diagnostic. Do not count sale proceeds or gross haul
   twice.
8. Re-run focused tests, the full server suite, client build, `git diff
   --check`, and the Markdown path-link check for any follow-up edits. No
   shared backend restart is needed for harness evidence.

## Decisions for designer review

1. Are KO targets required in each window, or only across 96h? Is the
   provisional 20-30% High defeat target acceptable?
2. For follow-up High risk work, keep HP 40, restore 45, test raw damage 16,
   or investigate readiness admission? The measured HP-40 arm misses both
   early and late High KO targets.
3. Medium misses both windows in opposite directions. Do you want a staged
   B3 experiment, or should the KO target be aggregate-only? If staged, which
   clock should control the shared world's profile: Raider age, first-raid
   time, or another deterministic clock?
4. Confirm the KO and medical bands. Should the value-order objective mean
   secured loot value or net asset change? The first arm meets late secured
   value order but not consistent net-asset order.
5. Keep C3's High weights, choose C2, or restore the current table? C3's
   expected 55% chance of a value-900 item per defeated world is a large tail;
   the late High defeat rate also rose to 49.2%.
6. Confirm the four-weapon cap and deterministic ranking. Is a 50% catalog
   sell-back acceptable despite 97,130 coins generated, or should the next
   arm test a lower/no-cash rate? E2 full-value liquidation remains unselected.
7. Hold shield price 300, 60% full-repair curve and ×1/×1.5/×2 wear, or
   authorize S2/S3 as a separate experiment? The first arm changed none of
   these, but late High Heavy deployment reached 44.6%.
8. Is the net asset/coin trend acceptable? Require cash percentiles,
   purchase/repair failures and reserve violations before deciding whether
   the sale proceeds are healthy liquidity?
9. Keep risk, rewards and weapon sinks as separately measured arms from here;
   the first arm bundled them, limiting causal interpretation?

## Out of scope

No BAL-3 shared chip-damage activities, ECON-3 Spend/Save Signal nudges,
additional tuning beyond this first arm, content/lore additions, Handler
economy controls, weapon/shield loot additions, direct PvP/theft, AI decision
authority, persistence/account work, browser multiplayer verification,
backend restart, commit, or push.

## Verification performed for this proposal

The BAL-2 control reruns passed. These commands set the opt-in report path and
ran only `BalanceMeasurementTests.WriteBalanceReportWhenRequested`:

```powershell
$env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='8'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-current-main-64x8h.md'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

Output: **Passed 1/1**, 15 seconds; rules `desperanza-bal-2-v1`, 2,242
settlements.

```powershell
$env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='96'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-current-main-64x96h-raw-run.md'; $env:AFK_BALANCE_RAW_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-current-main-64x96h.raw.json'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

Output: **Passed 1/1**, 2 minutes 25 seconds; rules
`desperanza-bal-2-v1`, 23,777 settlements. This exactly matches the existing
[BAL-2 measurement report](BAL-2-measurements.md) for the corresponding
96h aggregate and window results. The BAL-1S figure 16.3% is a historical
pre-BAL-2 baseline, not a mismatch in this rerun.

Seed-cluster intervals were calculated from the temporary raw per-raid file
with a deterministic 20,000-resample bootstrap.

The implemented first arm was measured with the same harness. The 8h standalone
command was:

```powershell
$env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='8'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-candidate-64x8h.md'; $env:AFK_BALANCE_RAW_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-candidate-64x8h.raw.json'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

Output: **Passed 1/1**, rules `desperanza-bal-4-v1`, 2,254 settlements.
The 96h command was:

```powershell
$env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='96'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-candidate-64x96h.md'; $env:AFK_BALANCE_RAW_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-candidate-64x96h.raw.json'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

Output: **Passed 1/1**, rules `desperanza-bal-4-v1`, 23,550 settlements,
3 minutes 29 seconds. Candidate and control used seeds 1-64; the 96h candidate
report records trait-specific outcomes, liquidation receipts, shield use and
seed-cluster raw telemetry.

- `git diff --check`: **PASS**.
- Proposal path-link scan: **26 local paths checked, 0 missing**.
- A broader scan of both Markdown files checked 313 local paths and found two
  occurrences of the pre-existing missing
  `../tests/AFKRaiders.Server.Tests/EquipmentAndShopTests.cs` link in historical
  ledger sections. These were already listed in the ledger's documentation
  cleanup note as stale historical references; they were not introduced or
  changed here.

The full server suite passed **476/476** with `DOTNET_PROCESSOR_COUNT=2`; the
web production build passed; `git diff --check` passed. No backend restart,
commit, or push was performed.
