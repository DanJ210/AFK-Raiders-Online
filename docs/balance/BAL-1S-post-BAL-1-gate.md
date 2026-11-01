# BAL-1S post-BAL-1 prerequisite gate (2026-11-01)

**Status: Gate 3 passed with documented departures; BAL-1S implemented.** The
isolated lab rebased cleanly onto BAL-1 and ran. Its post-BAL-1 results depart
materially from the earlier pre-BAL-1 plausibility numbers: KO is lower, the
Heavy share is higher, and shield spend is higher. They do **not** contradict the
accepted defaults themselves. Shields still reduce KO in the intended direction,
deployments still progress Light to Medium to Heavy, the reserve and emergency
rules still bound unshielded streaks, and no default needed to change for the
system to function. The defaults were accepted as experimental starting values,
with tuning owned by BAL-4. The departures are therefore recorded here as
experimental evidence, not used as a reason to retune. An earlier draft of this
note stopped at this gate; that stop was superseded by the follow-up
implementation request. All balance remains experimental until BAL-4.
## Gates and bounded scope

1. **Passed:** `git fetch origin` confirmed latest `origin/main` is
   `e6df3904c3117d279d542c3cbbf175eb8cd259f5`, the BAL-1 implementation commit.
   Its changes include weapon snapshots/seeded damage, robot HP, compatibility,
   tests, and the verified BAL-1 ledger. An ancestry check against `origin/main`
   succeeded.
2. **Passed:** the workspace was clean, with local `main` equal to that commit.
   Created clean branch `bal-1s-shield-tiers`. No unrelated working-tree changes
   were imported or overwritten.
3. **Passed with departures:** rebased the original throwaway lab from `f723a9d`
   onto the above commit in a separate local clone. No lab code entered the
   implementation branch. The accepted 30/90/300 prices and other defaults
   were not changed.

Production code was implemented from the section 9 contract after the gate,
not from lab code. No BAL-2/3/4 implementation or tuning was started.

## Method and rebase

The original `BAL-1S-review-and-handoff.md` reproducibility bundle supplied
`lab-reference.patch`, `ShieldLab.Decide`, and `ShieldLabTests.RunLabScenarios`.
Direct application failed on BAL-1's changed admission signatures. Three-way
application resolved everything except three conflict regions:

- Coordinator admission now passes **both** the BAL-1 weapon snapshot and the
  lab shield loadout.
- Simulation admission accepts both optional parameters.
- Raider creation retains the BAL-1 equipped-weapon argument before applying
  the lab shield loadout.

All other lab logic remained unchanged. The scratch build passed with zero
warnings/errors, and its staged diff passed `git diff --check`.

Paired scenarios ran seeds 1-64, two Raiders each, 96 simulated hours each
(12,288 Raider-hours per scenario), five-second scheduling, thirty-second
projection sampling, pinned IDs/trader seeds, and no Handler Signals:

- `bal1`: current production shield, reset each raid.
- `pdwH300`: persistent tier lab, prices 30/90/300, wear multipliers 1/1.5/2,
  capacity 40/70/100, mitigation 421/462/496, carry, full partial-charge
  mitigation, thresholds/reserves/emergency Light and standing stock as in the
  original lab.

The lab runner passed **1/1** and captured **22,800** production-baseline and
**22,646** shield-lab settlements. The tables use half-open windows by observed
settlement time; two settlements in each scenario observed exactly at 96h are
excluded from the window tables.

## Comparison with pre-BAL-1 evidence

Pre-BAL-1 values are the accepted report's `30/90/300 + danger wear` results;
they are not newly certified balance. The original report used mixed cohort
sizes for some baseline comparisons; this run's two scenarios are paired.

| Window | Earlier shield lab KO Medium / High % | Post-BAL-1 production KO Medium / High % | Post-BAL-1 shield lab KO Medium / High % | Shield lab sample Low / Medium / High |
|---|---:|---:|---:|---:|
| 0-8h | 26.7 / 42.3 | 25.87 / 48.95 | 24.01 / 38.61 | 710 / 1262 / 202 |
| 8-24h | 17.5 / 29.1 | 13.31 / 33.51 | 10.12 / 25.55 | 1341 / 1819 / 681 |
| 24-48h | 17.0 / 26.8 | 10.38 / 30.96 | 5.62 / 19.80 | 2143 / 2137 / 1318 |
| 48-96h | 14.8 / 27.1 | 7.17 / 25.83 | 4.04 / 14.17 | 4299 / 4233 / 2499 |

Late shield-lab KO changed by **-10.76 Medium / -12.93 High percentage points**,
well beyond the original report's approximate window noise. Relative to the
paired current production scenario, shields lower late KO by another
**3.13 / 11.66 points**. Low shield-lab KO is 0.14/0.15/0.00/0.21% in the four
windows respectively.

### Deployment tiers and spending

Tier shares below describe deployments **associated with settlements in each
window**, matching the original analysis method, not an independently timestamped
admission cohort. Shield-shop visits start at visit **1** on enrollment; visits
are joined in order to each Raider's following settlement for window spend.
These are attributed preparation costs, not exact timestamped cash-flow windows.

| Window | Settlements | None / Light / Medium / Heavy % | Shield coins attributed | Coins / settled raid |
|---|---:|---:|---:|---:|
| 0-8h | 2174 | 11.04 / 57.04 / 31.55 / 0.37 | 29764 | 13.69 |
| 8-24h | 3841 | 1.67 / 16.97 / 75.42 / 5.94 | 68938 | 17.95 |
| 24-48h | 5598 | 0.34 / 9.95 / 72.79 / 16.92 | 113630 | 20.30 |
| 48-96h | 11031 | 0.25 / 5.96 / 58.25 / 35.54 | 236747 | 21.46 |

The earlier late shares were approximately **1.1 / 21.6 / 71.5 / 5.8%** and
late shield spend approximately **16 coins/raid**. Heavy is now **29.74 points
higher**; late spend is approximately **5.46 coins/raid higher**. Total shop
spend across all visits is **451,390 coins**, or **19.93 per captured settlement**.
Of this, 449,079 is attributed inside the half-open window table; the remainder
includes endpoint/unsettled visits. Across the full captured cohort there are
196 unshielded streaks, mean **1.79**, maximum **10** (earlier mean 1.43/max 5).

### Individual eight-hour windows

| Window | Shield lab n Low / Medium / High | Production KO Medium / High % | Shield lab KO Medium / High % |
|---|---:|---:|---:|
| 0-8h | 710 / 1262 / 202 | 25.87 / 48.95 | 24.01 / 38.61 |
| 8-16h | 591 / 1026 / 319 | 14.52 / 32.98 | 12.09 / 28.84 |
| 16-24h | 750 / 793 / 362 | 11.80 / 34.01 | 7.57 / 22.65 |
| 24-32h | 749 / 711 / 408 | 10.36 / 33.43 | 6.19 / 20.10 |
| 32-40h | 699 / 680 / 482 | 10.12 / 31.16 | 5.15 / 21.16 |
| 40-48h | 695 / 746 / 428 | 10.64 / 28.38 | 5.50 / 17.99 |
| 48-56h | 711 / 764 / 375 | 8.00 / 29.41 | 4.58 / 15.47 |
| 56-64h | 705 / 714 / 415 | 6.22 / 22.22 | 3.36 / 13.49 |
| 64-72h | 773 / 686 / 401 | 7.78 / 27.35 | 4.37 / 14.71 |
| 72-80h | 680 / 726 / 409 | 6.02 / 23.30 | 3.17 / 10.51 |
| 80-88h | 717 / 701 / 400 | 6.37 / 25.54 | 4.85 / 14.50 |
| 88-96h | 713 / 642 / 499 | 8.68 / 27.50 | 3.89 / 16.03 |

## Interpretation and recommended decision

BAL-1's weapon relevance already makes mature Medium cohorts much safer:
production late Medium KO is **7.17%**, versus the older report's approximately
14.3%. Better combat results plausibly improve retained loot/wealth, allow
earlier upgrades, and reinforce survival. Heavy saturation and higher repair
spend are consistent with that feedback. This is an explanation supported by
the paired baseline, not an isolated causal attribution to one rule.

The old lab also has known production-policy differences: it repairs **before**
upgrading (171 visits here did both), uses floating-point wear, and plans
supplies before committing the inserted shield stop. Production must instead
evaluate upgrades first, use centi-point wear, and reserve shield spending before
supplies. Neither variant models the new production purchase journal/invariants.
Finite weapon/supply stock remains unchanged; shield stock remains unlimited.

**Gate decision:** do not change prices or mitigation to chase the old bands.
Implement BAL-1S as specified with the same experimental defaults, replace the
old plausibility expectations with the measurements below, and leave tuning to
BAL-4.

## Production BAL-1S measurements (BAL-0 harness, `desperanza-bal-1s-v1`)

Same harness and cohort shape as the BAL-1 production baseline: 64 seeds x two
unattended Raiders, no Handler Signals. The full 96h report is
[BAL-1S-measurements.md](BAL-1S-measurements.md).

| Horizon | Settlements | n Low / Medium / High | BAL-1 KO Low / Medium / High % | BAL-1S KO Low / Medium / High % |
|---|---:|---:|---:|---:|
| 8h | 2,189 (BAL-1 2,140) | 720 / 1253 / 216 | 0.0 / 25.9 / 48.9 | 0.1 / 25.3 / 38.9 |
| 96h | 22,508 (BAL-1 22,800) | 8367 / 9345 / 4796 | 0.1 / 11.4 / 29.4 | 0.2 / 7.7 / 16.3 |

By difference, the 8-96h production KO is about 5.0% Medium (n=8092) and about
15.2% High (n=4580). The lab's 48-96h figures were 4.04 / 14.17%. Production
agrees with the lab's direction and magnitude; it is not a separately
timestamped window table.

| Horizon | Deployed none / Light / Medium / Heavy % (all dangers) | Shield + repair coins per settled raid | Unshielded streaks (count, mean / max) | Shields lost on KO |
|---|---:|---:|---:|---:|
| 8h | 11.0 / 55.2 / 33.0 / 0.8 | 14.37 | 128, 1.88 / 10 | 327 |
| 96h | 1.4 / 12.0 / 57.4 / 29.1 | 20.67 | 176, 1.84 / 10 | 1415 |

At 96h, Raiders bought 975 Light, 1110 Medium and 260 Heavy shields (345
emergency Lights, 930 upgrades), for 207,150 coins. They also made 5,799 repairs
restoring 351,722 whole durability points (about 61 per repair) for 258,122
coins. High deployments are 39.2% Heavy. No deployment started broken,
and at most 0.2% started weak (25 points or less), because trait thresholds
(25-75) trigger repairs earlier. Production versus lab: the Heavy share is
29.1% versus 35.5% late, and spend is 20.67 versus 21.46 late. Production
evaluates upgrades before repairs and uses integer centi-point wear, which
plausibly accounts for the small differences.

## Validation

See the [ledger](../IMPLEMENTATION_PROGRESS.md#bal-1s---shield-tiers-wear-and-paid-autonomous-repair-2026-11-01)
for test counts, changed existing expectations and limitations. The lab
reproducibility bundle (original report, rebased patch, scenario JSON, CSVs,
Node analysis) is preserved in session artifacts outside the repository. Python
was unavailable, so equivalent CSV analysis used Node, with no added
dependencies.
