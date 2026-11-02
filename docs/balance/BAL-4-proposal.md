# BAL-4 tuning proposal

**Status: proposed, experimental, awaiting approval. BAL-4 is not active.**
This is a documentation-only proposal. It changes no runtime values, code,
tests, rules versions, fingerprints, backend state, or approval boundaries.
BAL-3 and ECON-3 are out of scope.

## Summary and recommendation

The rerun confirms a time-shaped BAL-2 risk curve, not a single 96h balance
point. High KO is **54.0%** at 0-8h and **43.0%** at 8-24h (above the
experimental 25-40% band), easing to **30.6%** at 48-96h (in band). Medium is
the reverse problem: **29.0%** at 0-8h is above its 15-25% band, but **9.3%**
at 48-96h is below it. The often-cited High **16.3%** is the pre-BAL-2
BAL-1S aggregate, not current main. The old 57.9% starter High extraction
result is a separate BAL-1S-era cohort and needs rerunning after BAL-2.

**Recommended review package (not approved for implementation):**

1. Do not apply a single global danger or shield adjustment. Trial a modest
   High HP reduction (45 to 40; keep 18 raw/4 rounds) as a measurement
   candidate: earlier robot defeat could reduce early robot retaliation and
   make the two shared rewards reachable more often. Separately test a
   readiness/admission option if hazard KOs keep the opening window above
   target. Keep the late High profile as the control.
2. No one static Medium value can lower early KO and raise late KO at once.
   The one-change check should include a global increase and decrease as
   controls, then a clearly labeled staged/readiness candidate; the late
   Medium failures are predominantly hazard-caused, so robot damage alone is
   unlikely to reach 15%.
3. For High value, test a two-slot High reward-table tail increase to a 35%
   weight on the existing value-900 item, retaining two shared rewards, the
   12% single-weapon slot, and all ownership rules. This estimates above the
   current Low/Medium all-loot values only under explicit unchanged-defeat/
   unchanged-retention assumptions; it is not a guaranteed result.
4. To address unbounded owned weapon count, prefer an autonomous **four-weapon
   cap with 50% catalog-value liquidation of overflow**. Preserve deterministic
   all-source accounting and protect deployed/needed gear. This is a new
   proposal, not the previously unselected full-price cap.
5. Hold the BAL-1S shield curve and mitigation for the first candidate pass.
   Higher wear/cost may worsen the early KO spikes; lower wear may push mature
   KO below target. Add them as isolated candidates only after readiness and
   cause data are measured.

**Recommended first measurement portfolio:** High 40/18/4 (A1/D2), High
reward weights 25/25/15/35 (C3), and the all-source four-weapon cap with
50%-value overflow sale (E3), while holding shields (S1). Keep Medium at
current 22/12/3 as the matched control; run B3 separately only after choosing
the shared-world clock. These are separate experimental arms, not a single
combined tuning batch.

Every candidate remains **experimental**. Baseline intervals describe
observed cohorts; candidate projections are explicitly marked as
mechanistic/unrun rather than presented as simulated results.

## Recommended before/after (measurement proposal only)

This is a set of first experiments, **not an approved combined implementation**.
Run each lever separately before any combined arm, because risk, rewards,
repair affordability, and inventory liquidation can interact.

| Surface | Current BAL-2 | First proposed experimental comparison | Expected direction; no simulated outcome |
|---|---|---|---|
| High danger/defeat rate | 45 HP / 18 raw / 4 rounds | 40 / 18 / 4 | High defeats likely increase; early KO may fall; late KO may drop below band. |
| Medium danger | 22 / 12 / 3 at all times | Control unchanged; separately test B3's first-24h raw 10 and 48h+ shield wear ×2.0 | Early robot KOs may fall and late shield attrition may rise; late hazard KOs may remain below target. |
| High reward table | 30 / 50 / 18 / 2% at item values 35 / 60 / 150 / 900 | 25 / 25 / 15 / 35%, exactly two shared slots retained | Expected gross bundle 177.54 -> 695.95 per defeated world; per-raid secured value is unrun and sensitive to defeat, sharing, and extraction. |
| Weapon inventory | Uncapped; +2,171 net weapons and 17.80 usable per Raider at 96h | At most four owned instances; deterministic overflow sale at 50% catalog value | Bounds stock, adds some coins; turnover and cash inflation unmeasured. |
| Heavy shield | Price 300; 60% full-repair curve; wear ×1/×1.5/×2 | Hold as control; do not combine S2/S3 with initial risk trial | Avoid worsening the current early High KO spike before its cause is separated. |

No expected KO percentages or candidate confidence intervals are claimed for
this combination: it has zero candidate seeds. Low remains a comparison
danger, and per-trait predictions await new telemetry.

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
- No AI authority over combat, decisions, purchases, or outcomes.

No Handler Signal, simulation scheduling, extraction/KO/pocket semantics,
shared-world ownership, or reward count is changed by this proposal.

## Baseline verification and comparison

The workspace was on branch `main`, HEAD
`ea2515aa2fffc31e20d0d5c206ac5e0cfa05d1e8`, with pre-existing uncommitted
BAL-2/runtime and documentation edits. The harness therefore measured the
current **worktree**, under `desperanza-bal-2-v1`; this is not a clean-HEAD
reproduction. No files outside the two proposal/ledger documentation edits
were changed for this task, and no backend was restarted.

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

### Early versus mature outcomes

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

The current profile is High **45 HP / 18 raw / 4 rounds**. Its observed KO
curve is 54.0% at 0-8h (n=100; seed-bootstrap 95% 44.00-64.42%), 43.0% at
8-24h (n=423; 38.10-48.31%), and 30.6% at 48-96h (n=1,802; 27.68-33.73%).
The opening [0,8) High KO causes were Overtime Red 35, robot swipe 18, and
deadline 1. A robot-only adjustment cannot remove the hazard-caused share.
The BAL-1S-era 57.9% first-raid extraction rate is not a current BAL-2 result;
rerun it with the current profile/reward rules.

| Candidate | Exact change | Predicted by danger | Evidence, seed count, uncertainty |
|---|---|---|---|
| A1 — Lower High HP (recommended test) | High HP 45 -> 40; raw 18 and 4 rounds stay fixed. Low/Medium unchanged. | Low/Medium direct profile outcomes unchanged. High kill may arrive earlier, suppressing a later retaliation and increasing access to the existing shared reward. Direction: High KO down, robot defeats up; late High may fall below 25%. | No candidate runs (0 seeds). Reference High: 0-8 54.0%, 8-24 43.0%, 48-96 30.6%; baseline windows use 64 seeds. No point forecast. |
| A2 — Lower High impact | High raw damage 18 -> 16; HP 45 and 4 rounds stay fixed. | Low/Medium direct outcomes unchanged. High robot HP loss should fall; hazard-caused KO remains. Direction: High KO down in every window, with late result at risk of dropping below 25%. | No candidate runs (0 seeds). Current 0-8 High robot/hazard HP loss is 18.24/36.29 per raid; 96h is 24.07/38.32. Hold-hit-count arithmetic gives about 21.4 robot HP loss/High raid, not a KO forecast. |
| A3 — Readiness-gated High admission | During a Raider's first 24h, start/admit to a High world only if every participant has a usable weapon, at least one medical item, and an unbroken shield; keep one combat profile for the shared world. | Intended to lower early High KO while leaving mature High (current 30.6%) untouched. Low/Medium frequencies and denominators may shift; it does not make an unready High instance safer if the gate is bypassed. | No candidate runs (0 seeds). Requires a deterministic age clock, coordinator admission rule, and multi-Raider fairness design; completion/queue effects unknown. |

Do not stack A1 and A2 initially. A1 is the best first measurement because
the current High fight already averages 3.85/4 rounds and no High robot was
defeated in the opening sample. A3 is a larger product/pacing change and could
starve High worlds or route the same actual Raiders into lower dangers.

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

Trait-stratified predictions are unavailable for both A and B. Existing
trait thresholds/reserves affect supplies and shields, but no current report
records trait with each settlement. All candidate seed counts are zero; the
baseline seed-cluster intervals are not candidate confidence intervals.

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

| Candidate | High weights by value 35 / 60 / 150 / 900 | Expected bundle gross value (Low / Medium / High) | First-order High all-loot value/raid |
|---|---|---:|---:|
| C1 — Current control | 30 / 50 / 18 / 2% | 25.06 / 76.805 / 177.54 | Observed 59.47 |
| C2 — Medium tail | 25 / 35 / 15 / 25% | 25.06 / 76.805 / 538.03 | About 94.1; still below Low/Medium |
| C3 — Raise High above the other two (recommended test) | 25 / 25 / 15 / 35% | 25.06 / 76.805 / 695.95 | About 109.3; above 99.16/97.50 if defeat and retention stay unchanged |

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
risk. Candidate seed count is zero; test at 64 seed clusters and report
p50/p90/p99 and a leave-jackpot-out result before approval.

## D. High robot defeats are rare

There were zero High defeats in the standalone 8h cohort: 0/59 participant
fights (all 59 escaped). At 96h, 384/2,701 were defeated (14.2%). The opening
sample is small; zero of 59 does not establish that a defeat is impossible.
The BAL-2 reward table only pays on committed defeat, so even the C3 reward
uplift is rarely seen unless the defeat frequency also rises.

| Candidate | Exact change | Predicted High defeat outcome | Risk |
|---|---|---|---|
| D1 — Current | 45 HP / 18 raw / 4 rounds | Observed 0/59 at 8h and 14.2% at 96h (11.33-17.36% seed-bootstrap interval). | Top-tier loot remains rare. |
| D2 — Shorter defeat threshold (recommended test) | High robot HP 45 -> 40; keep 18 raw, 4 rounds. | Defeat frequency expected up because fewer capped weapon rounds are needed; KO may fall from fewer killing-round retaliations. No numeric candidate estimate. | Could make late High too easy or lower late KO below 25%. |
| D3 — More time to defeat | Keep 45 HP / 18 raw; rounds 4 -> 5. | Expected defeat frequency up from an extra Raider attack round, but adds an extra robot retaliation to failures. | Conflicts with reducing early High KO; do not combine with D2 without a separate experiment. |

For review, a provisional “sensible” diagnostic is 20-30% High participant-fight
defeats over 96h, plus a nonzero opening-window rate, while keeping High KO
within 25-40% in each accepted window. This is experimental and needs designer
confirmation. D2 is the only candidate direction that plausibly improves
defeats and early KO together; its late effect must be measured.

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
| E3 — Four-slot, half-value sell-back (recommended for review) | Same all-source four-instance cap and deterministic retention as E2. Retire overflow at `floor(0.50 × catalog value)`: Aspperigo 32, Vernerider 70, Temptrest 140; Kettle 0. No Handler sale control. | Hard maximum of four after each committed addition. Converts surplus to less cash than E2; exact turnover and danger effects are unknown until run. | Bounds stock while reducing—but not eliminating—liquidation-driven inflation. Requires transactional policy for every incoming source. |

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

The E2/E3 sale event must be journaled once, attributed to the owner, and
included in a stock-conservation equation. Use measured receipts, not an
assumed number of overflow weapons, to forecast coins. High/Medium/Low impacts
cannot be numerically separated from current evidence because weapon inflow,
sales, and retirements are not currently reported by trait and proposed cap
variants have zero runs.

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
| S1 — Hold (recommended) | 300 | 0.25s + 0.35s² (60%) | 1 / 1.5 / 2 | Full 180; 50 points 64 | Preserves current measured policy; avoids further reducing High Heavy access while opening High KO already exceeds band. |
| S2 — Moderate cost/wear | 360 | 0.30s + 0.40s² (70%) | 1 / 1.5 / 2.25 | Full 252; 50 points 90 | Raises Heavy purchase and repair burden; likely reduces Heavy upgrades and increases High durability failures. |
| S3 — Strong cost/wear | 450 | 0.35s + 0.45s² (80%) | 1 / 1.5 / 2.5 | Full 360; 50 points 130 | Strongest pressure on Heavy use and repair; highest unshielded-streak/death-spiral risk. |

S2/S3 are unrun; there is no supportable numeric prediction for Heavy share,
shield spend, KO, or unshielded streaks. Apply any selected price/curve
uniformly unless a separate trait-specific rule is approved. Cautious reaches
the 75% repair trigger first; Hoarder/Gadgeteer use 50%, Brawler 25%. A higher
cost can push each trait into different partial-repair or unshielded behavior;
the existing aggregate report cannot quantify those differences.

| Danger | Current 96h KO (n; seed-bootstrap 95%) | S2/S3 expected direction | Trait-specific result |
|---|---|---|---|
| Low | 0.1% (9,966; 0.06-0.19%) | Lower affordability can shift deployments down-tier; direct KO change likely smaller than Medium/High but is unmeasured. | Not reported. |
| Medium | 13.5% (10,571; 12.36-14.72%) | Less repair/upgrade access can push KO up; already below the proposed 15% floor. | Cautious repair trigger is earliest; exact effect unmeasured. |
| High | 34.4% (3,240; 32.16-36.63%); opening window 54.0% (n=100) | Less Heavy access and faster wear can push KO/unshielded streaks up; opening window is already above target. | Brawler repairs later than the other traits; costs/reserves still differ by trait. No per-trait sample. |

## BAL-2 profile/reward interaction cross-check

The current profiles are Low **12 HP / 8 raw / 3 rounds**, Medium **22 / 12 / 3**,
and High **45 / 18 / 4**, with a 5-second round interval. Rewards are two
shared slots on a committed world defeat, not guaranteed per Raider. The
High table's current expected bundle is 177.54 per defeated world; C2/C3
increase that to 538.03/695.95 without adding slots or changing the weapon
chance. At the observed 14.2% defeat rate this change is attenuated per
participant fight and further by sharing, KO, and extraction. A1/D2 lowers
High HP to 40 and is the candidate intended to increase defeats; its new
frequency and resulting all-loot order must be measured, not multiplied into
the current rate as if independent.

## F. Net asset and coin trend

BAL-2 mean net asset gain falls from **22.16 coins/raid** in [0,8h) to
**6.41** in [48,96h), while remaining positive. Closing coins per Raider fell
from **377.09** under BAL-1S to **221.29** under BAL-2 (about 41% lower);
closing stash rose 119.45 -> 160.23 and supplies fell 103.23 -> 53.66.
This is not automatically unhealthy: slowing positive asset growth can curb
runaway accumulation, but lower cash and supplies may signal liquidity or
readiness pressure. The aggregate means do not show the coin-poor tail,
missed purchases, or unpaid repairs, so they cannot settle that question.

The cap-four sale-back would add coins; the proposed High jackpot table can
add large, uneven asset value. Neither should be presented as a remedy for
the trend until cash percentiles, purchase/repair failures, reserve violations,
and trait-specific stock are measured. If a change is needed, prefer a
bounded earned sink or adjust item repair prices over raising routine rewards.

| Candidate | Expected direction | Danger/trait limits |
|---|---|---|
| F1 — Hold current economy (recommended control) | Net gain stays positive but may continue to taper; no new coin source. | Current 96h net change Low 80.09, Medium 0.71, High -174.78 per raid. No per-trait results. |
| F2 — E3 half-value cap-four | Weapon stock bounded; overflow creates some cash, so closing coins likely rise versus hold. | Inflow and sale receipts by danger/trait are unmeasured; candidate runs 0. |
| F3 — Raise repair costs 25% | Coin drain rises if repairs continue; may reduce cash and equipment readiness. | Cannot bound weapon count. Effects on KO, danger mix, and trait reserves are unknown; candidate runs 0. |

## G. Why High settlements fell from 216 to 100 in the first 8h

This is a **settlement-count difference**, not a proven admission-rate
difference. In matched standalone 8h cohorts, total settlements increased
2,189 -> 2,242 while High settlements fell 216 -> 100; High participant
fights fell 147 -> 59. BAL-2 also changed robot outcomes sharply: High
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

## Exact proposed value/file map

No files below are changed by this proposal. If approved, select only the
accepted candidates and preserve compatibility/versioning review:

| Proposed value or behavior | File(s) that would change after approval |
|---|---|
| High/Medium profile HP/raw damage/round limit; any danger-specific time phase | [RaidConditions.cs](../../server/AFKRaiders.Server/Domain/RaidConditions.cs); a time/readiness gate would also need [RaidCoordinator.cs](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs) |
| High readiness admission gate | [RaidCoordinator.cs](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs), with authorization/admission and shared-world tests |
| High defeat-cargo weights | [RobotDefeatRewards.cs](../../server/AFKRaiders.Server/Domain/RobotDefeatRewards.cs) |
| Shield prices | [shop-catalog.json](../../server/AFKRaiders.Server/Content/shop-catalog.json) |
| Shield repair curve and danger wear multipliers | [Shields.cs](../../server/AFKRaiders.Server/Domain/Shields.cs), with autonomous decisions in [RaiderEconomyPolicy.cs](../../server/AFKRaiders.Server/Domain/RaiderEconomyPolicy.cs) |
| Weapon repair costs (E1) | [shop-catalog.json](../../server/AFKRaiders.Server/Content/shop-catalog.json) |
| Four-weapon cap, protection/ranking, 50% or full-value disposition, once-only journal and receipts | [RaidCoordinator.cs](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs), [FoundWeaponSettlement.cs](../../server/AFKRaiders.Server/Domain/FoundWeaponSettlement.cs), and [RaiderEconomyPolicy.cs](../../server/AFKRaiders.Server/Domain/RaiderEconomyPolicy.cs); inspect every weapon-addition/settlement path before implementing |
| Trait/window telemetry, KO/healing/net-value and stock/sink reports | [RaiderStatistics.cs](../../server/AFKRaiders.Server/Domain/RaiderStatistics.cs), [BalanceMeasurementTests.cs](../../tests/AFKRaiders.Server.Tests/BalanceMeasurementTests.cs) |
| Focused regression tests for changed values, ownership, idempotency, and economy | [ShieldTests.cs](../../tests/AFKRaiders.Server.Tests/ShieldTests.cs), [WeaponCombatTests.cs](../../tests/AFKRaiders.Server.Tests/WeaponCombatTests.cs), [FoundWeaponSettlementTests.cs](../../tests/AFKRaiders.Server.Tests/FoundWeaponSettlementTests.cs), [RaiderEconomyTests.cs](../../tests/AFKRaiders.Server.Tests/RaiderEconomyTests.cs), and [G6RiskRewardTests.cs](../../tests/AFKRaiders.Server.Tests/G6RiskRewardTests.cs) |

If implementation changes rules or narrated facts, identify the necessary
rules-version/fingerprint and README updates then. This proposal does not
authorize those changes.

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
- **Risk overshoot:** current opening High KO is already 54% and the lower
  bound of its seed-cluster interval is above 40%. Lowering robot HP/raw may
  lower early KO but could also push late High below 25%; extra rounds or
  higher wear can make the opening spike worse.
- **Reward not secured:** BAL-2 cargo is risky until extraction, shared by
  contributors, and may be lost on KO except for the existing designated
  pocket item. Gross table expectation is not retained net value.
- **Candidate uncertainty:** the harness has 64 seeds, two Raiders, and no
  trait field. It does not estimate player populations, broader online
  concurrency, or loadout-specific staged KO without a separate cohort.

## Test and measurement plan (only after approval)

1. Confirm targets and metric definitions below before selecting a candidate.
2. Add measurement-only candidate coverage after authorization; don't mutate
   production values for a proposal. Run matched seeds 1-64, two unattended
   Raiders, pinned IDs/trader seed, no Signal, at 8h and 96h. Preserve the
   current baseline as control and report half-open [0,8), [8,24), [24,48),
   [48,96) windows.
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
6. For E2/E3, assert all-source inventory conservation and the selected cap
   after every committed addition, and verify deterministic retention, exact
   sale receipts, no duplicate sale, absent-versus-polled parity, retry and
   repeated-settlement idempotency. Distinguish broken from retired.
7. Report net asset change by danger/window and seed-cluster intervals, gross
   secured value, cash, stock, and High reward median/p90/p99 plus a
   leave-jackpot-out diagnostic. Do not count sale proceeds or gross haul
   twice.
8. Re-run smallest focused tests, the full server suite if required by the
   changed surfaces, `git diff --check`, and the Markdown path-link check.
   No shared backend restart is needed for harness evidence.

## Decisions for designer review

1. Must each time window meet KO targets, or are 96h aggregate targets enough?
   Is the provisional 20-30% High defeat target acceptable?
2. Should A1/D2 (High HP 40) be the first candidate, or is a first-24h
   readiness gate preferable? Is 57.9% first-raid High extraction an
   independent concern, despite being a BAL-1S-era measurement?
3. Since no one static Medium value fixes 29.0% early and 9.3% late, do you
   approve a staged B3 experiment, or prefer changing the window target? If
   staged, should phase use Raider age, first-raid time, or another clock, with
   one profile applied uniformly to all Raiders in a shared world?
4. Are the KO, medical-consumption, and mean-net-value definitions/bands still
   the intended experimental targets? Define “mean net value” explicitly.
5. For High rewards, choose current control, C2, or C3. Is C3's estimated
   55% chance of at least one value-900 item per defeated world acceptable?
6. For weapons, choose repair-cost-only E1, unselected full-value cap-four E2,
   or half-value cap-four E3. Confirm the four-instance cap and ranking rule.
7. For shields, keep S1 or authorize S2/S3 as a separate experiment? Current
   BAL-2 High Heavy share is 26.0%, unlike BAL-1S's 39.2%.
8. Is the fall in net asset gain/closing coins acceptable as slowing
   accumulation, or should the next measurement prioritize liquidity?
   Require cash percentiles and failed-purchase/repair telemetry before a
   policy decision?
9. Should the risk, reward, and weapon-sink experiments be staged separately
   rather than combined into a single BAL-4 approval?

## Out of scope

No BAL-3 shared chip-damage activities, ECON-3 Spend/Save Signal nudges,
implementation, runtime/test edits, rules-version/fingerprint changes,
content/lore additions, Handler economy controls, weapon/shield loot additions,
direct PvP/theft, AI decision authority, persistence/account work, browser
verification, backend restart, commit, or push.

## Verification performed for this proposal

The current-worktree harness runs both passed. The commands set the opt-in
report path and ran only `BalanceMeasurementTests.WriteBalanceReportWhenRequested`:

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
with a deterministic 20,000-resample bootstrap; trait was not present in the
telemetry.

- `git diff --check`: **PASS**.
- Proposal path-link scan: **25 local paths checked, 0 missing**. The new
  ledger pointer also resolves.
- A broader scan of both Markdown files checked 304 local paths and found two
  occurrences of the pre-existing missing
  `../tests/AFKRaiders.Server.Tests/EquipmentAndShopTests.cs` link in historical
  ledger sections. These were already listed in the ledger's documentation
  cleanup note as stale historical references; they were not introduced or
  changed here.

No runtime build/test, backend restart, commit, or push was performed.
