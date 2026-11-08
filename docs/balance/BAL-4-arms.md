# BAL-4 experiment arms (2026-11-04)

**Status: Arms 1-3 and independent follow-up arms 4A, 4B and 5 are measured.
Every arm is experimental and nothing here is balance-certified.** Arms 4A,
4B and 5 are separate, one-lever deltas from the cumulative Arm 3 state;
they are not stacked on each other. Each new arm has its own rules version,
compatibility fingerprint and matched-seed 8h/96h measurement.

**Current served rules version:** `desperanza-bal-4-arm3-v1`, matching the
web client's compatibility check. Arms 4A, 4B and 5 remain independent
measurement candidates, not the default served rules. Earlier versions below
identify historical measurement baselines.

The decisions governing this follow-up are recorded in the
[progress ledger's](../IMPLEMENTATION_PROGRESS.md) decision-authority row and
here before implementation:

## Designer decisions (2026-11-04)

1. **Accept 25% overflow sell-back** as the standing experimental value. A
   no-cash retirement sink remains a possible later arm, not this task.
2. **Keep High robot HP/raw/rounds 45/18/4 and the four-weapon cap.**
3. **Mature High KO of about 24-25% is in band**, even when its interval
   straddles the 25% floor. Do not tune mature High risk; differences under
   about 2 percentage points are noise and are not chased.
4. **The [0,8) High onboarding cell (55-61% KO) is unacceptable.** Test an
   early-High block in Arm 5.
5. **Run the jackpot re-check as one lever** in separate candidates 4A and 4B.
6. Medium robot stats, shields and the Arm 3 kit gate stay unchanged, except
   for Arm 5's explicit age rule. Medium hazard work is deferred to BAL-3.
7. Each new arm is an independent delta from Arm 3 (High 45/18/4, 25%
   sell-back, kit gate, rewards 25/25/15/35), not stacked on another new arm.

Frozen: trader-only Temptrest, once-per-raid weapon wear, deterministic
ownership/allocation, once-only settlement, two shared defeat-reward slots
per world, the 12% single-weapon slot chance and weapon rules, no Handler
economy controls, no PvP/theft, and existing extraction/KO/pocket semantics.
The historical “no AI authority over outcomes” clause was superseded on
2026-11-06; see the [decision authority checkpoint](../IMPLEMENTATION_PROGRESS.md#agent-handoff-checkpoint).

## Protocol

Every arm used matched seeds 1-64, two unattended Raiders per seed, pinned
IDs/trader seed, five-second scheduling, the recurring autonomous economy, and
no Handler Signals; one standalone 8h run and one 96h run per arm, with
half-open true-settlement-time windows [0,8), [8,24), [24,48), [48,96).
Recurring-cohort confidence intervals are deterministic 20,000-resample
seed-cluster percentile intervals (both Raiders and their raids resampled
with their seed); the fixed first-raid cohort uses the same resample count
over its 120 seeds. The
harness added these previously missing diagnostics for every arm: closing-coin
percentiles per Raider (p10/p50/p90/p99), failed weapon/supply purchases,
unaffordable due repairs, below-reserve commits, High secured-value
p50/p90/p99 plus a leave-jackpot-out result, a High admission funnel (offered
vs started vs settled, plus readiness-gate blocks), time-to-next-admission
after settlement, and a fresh 120-seed x 2-Raider first-raid High cohort
(starter and staged variants). Raw per-raid telemetry went to temporary files,
not the repository.

Baselines for comparison: the **BAL-2 control** (`desperanza-bal-2-v1`,
High 45/18/4, uncapped weapons, original reward weights) and the **BAL-4
first arm** (`desperanza-bal-4-v1`, High 40/18/4, weights 25/25/15/35, cap
four at 50% sell-back), both measured earlier with the same seeds and
reported in [BAL-4-proposal.md](BAL-4-proposal.md) and
[BAL-2-measurements.md](BAL-2-measurements.md). All new arms in this document
use Arm 3 as their matched reference state.

## Arm 1 — restore High robot HP 45 (`desperanza-bal-4-arm1-v1`)

**Change:** High robot HP 40 -> 45 (reversal of the first arm's only profile
change); raw 18 and 4 rounds unchanged. Nothing else moved: reward weights
stay 25/25/15/35, cap four at 50% sell-back, Medium 22/12/3, shields held.
Files: [RaidConditions.cs](../../server/AFKRaiders.Server/Domain/RaidConditions.cs),
[RaidSimulation.cs](../../server/AFKRaiders.Server/Domain/RaidSimulation.cs)
(rules version), updated expectations in
[Bal2RobotDefeatTests.cs](../../tests/AFKRaiders.Server.Tests/Bal2RobotDefeatTests.cs),
[WeaponCombatTests.cs](../../tests/AFKRaiders.Server.Tests/WeaponCombatTests.cs)
(Vernerider max 4x11 = 44 < 45 cannot solo High again), and the
[compatibility fingerprint](../../tests/AFKRaiders.Server.Tests/NarrationCompatibilityTests.cs).

**Measurements (8h: 2,252 settlements; 96h: 23,537):**

| Window | Danger | n | KO rate (95% CI) | Medical/raid | Secured value/raid (95% CI) | Net asset change/raid (95% CI) | Participant-fight defeat rate |
|---|---|---:|---|---:|---|---|---|
| [0,8) | Low | 800 | 0.0% (0.0-0.0%) | 0.20 | 79.47 | 69.39 | 90.3% |
| [0,8) | Medium | 1,336 | 29.7% (26.1-33.4%) | 0.91 | 55.10 | 5.62 | 27.3% |
| [0,8) | High | 115 | 60.9% (51.3-70.7%) | 1.83 | 30.01 | -137.44 | 4.1% (0.0-12.9%) |
| [8,24) | Medium | 1,964 | 15.2% (12.8-17.7%) | 1.53 | 94.42 | -1.13 | 53.6% |
| [8,24) | High | 439 | 41.0% (36.3-46.0%) | 2.37 | 55.18 | -188.93 | 4.1% (1.3-7.9%) |
| [24,48) | High | 960 | 32.6% (29.3-36.1%) | 2.56 | 134.27 | -121.21 | 18.4% |
| [48,96) | Low | 4,952 | 0.1% (0.0-0.2%) | 0.10 | 96.34 (95.22-97.39) | 67.07 | 99.2% |
| [48,96) | Medium | 4,582 | 7.6% (6.1-9.3%) | 1.72 | 115.56 (109.99-121.00) | -0.12 | 74.0% |
| [48,96) | High | 1,934 | 24.2% (21.3-27.4%) | 2.40 | 225.65 (186.57-264.52) | -18.58 (-59.79 to 23.33) | 32.4% (26.2-38.4%) |

Whole-horizon High participant-fight defeat rate: **714/2,916 = 24.5%
(19.7-29.3%)** — inside the accepted provisional 20-30% band (first arm:
36.6%; BAL-2 control: 14.2%). KO causes stay hazard-dominant everywhere
(late High: Overtime Red 356 vs robot swipe 111). High leave-jackpot-out
secured value in [48,96) is 66.76 vs 225.65 with jackpots (n 1,669 of 1,934).

**Bands:** onboarding High KO misses at [0,8) (60.9% > 45%) and meets at
[8,24) (41.0%); the first-24h aggregate is 250/554 = 45.1%, right at the
line. Onboarding Medium meets (29.7%/15.2% <= 30%). Mature High is 24.2%
(21.3-27.4%) — the point estimate sits just below the 25-40% band, with the
interval straddling the floor. Mature Medium (7.6%) stays below its band:
hazard-driven and deferred to BAL-3 by decision 4. Mature medical meets 0-1 /
1-2 / 2-4 (0.10 / 1.72 / 2.40). Mature secured value order **meets**
High 225.65 > Medium 115.56 > Low 96.34. The report-only guardrail does not
pass: mature High mean net is -18.58, though its interval spans zero.

**Arm 4 (jackpot re-check) verdict:** not forced by defeat frequency — the
restored 45 HP brings High defeats back inside 20-30% and late defeat
exposure down from the first arm's 49.2% to 32.4%. However, High value is
heavily jackpot-concentrated (late secured value drops from 225.65 to 66.76
without 900-value shards; closing coins p90 is ~10,532 vs p50 162), and
mature High net assets stay negative without jackpots (-183.98). If the
designer wants High value that is not tail-driven, re-weighing C3 is the
next value lever; nothing in Arm 1 requires doing it before the risk/economy
decisions below.

**Not isolated/proven:** Arm 1 vs the first arm isolates the HP lever given
the already-changed rewards and cap; it does not measure HP 45 against the
BAL-2 reward table. Early High cells stay small (n 115) and the [0,8) economy
still reflects enrollment ramp-up.

## Arm 2 — overflow sell-back 50% -> 25% (`desperanza-bal-4-arm2-v1`)

**Change (on top of Arm 1):** cap-overflow liquidation pays
`floor(0.25 x catalog value)` — Aspperigo 16, Vernerider 35, Temptrest 70,
Kettle 0 — via `WeaponInventoryCap.SellBackPermille` 500 -> 250 in
[WeaponInventoryCap.cs](../../server/AFKRaiders.Server/Domain/WeaponInventoryCap.cs).
The cap of four and the deterministic retention order are unchanged.
[WeaponInventoryCapTests.cs](../../tests/AFKRaiders.Server.Tests/WeaponInventoryCapTests.cs)
asserts the exact quarter-value receipts, the Kettle-zero sale, dedup, and
deterministic ties; stock conservation and the cap are asserted on every
settlement by the harness.

**Measurements (8h: 2,252 settlements; 96h: 23,698):** risk outcomes track
Arm 1 closely, as expected from a cash-only lever — [0,8) High KO 60.9%
(identical cohort), [8,24) 43.5% (39.2-48.0%), [48,96) High 24.6% / 24.8%
(21.5-28.3%), mature Medium 9.5%, mature secured order High 228.10 >
Medium 108.75 > Low 95.17, mature High net -12.85 (-59.46 to 33.05),
whole-horizon High defeat rate 24.6% (19.4-29.6%). Late-window drift vs Arm 1
(e.g. Medium 7.6% -> 9.5%) is downstream economy knock-on, not a second lever.

**Economy effect (the lever's target):** liquidation receipts fell
90,430 -> 42,797 coins (2,440 -> 2,329 instances); closing coins per Raider
at 96h moved p50 162 -> 138, p90 10,531.70 -> 9,549.50, p99
15,480.02 -> 13,068.57. Failed weapon purchases 170 -> 137, failed supply
units 3,655 -> 3,816, unaffordable due repairs 65 -> 83, below-reserve
commits 521 -> 545 — no liquidity collapse appears at 25%. The surviving
coin tail is jackpot-driven, not sell-back-driven.

**Bands:** same compliance picture as Arm 1 (onboarding High misses only at
[0,8); mature High point 24.8% straddles the 25% floor; value order met;
guardrail negative with interval spanning zero).

**Not isolated/proven:** 25% is one point, not a dose-response curve; a
no-cash retirement sink remains unmeasured. The risk deltas between Arms 1
and 2 are seed-path drift and must not be read as effects of the sell-back.

## Arm 3 — first-24h High readiness gate (`desperanza-bal-4-arm3-v1`)

**Change (on top of Arm 2):** a per-Raider readiness gate in
[RaidCoordinator.cs](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs).

- **Age clock:** the Raider's deterministic, server-owned creation
  (enrollment) time, recorded once on `EnrollHandler`. Because the clock is
  per-Raider, shared worlds need no world-clock rule and no combat profile
  changes anywhere.
- **Rule:** while Raider age < 24h, the Raider is not offered or admitted to
  a High world unless it would deploy ready: a usable (not broken) equipped
  weapon, at least one Medical-category supply it would actually carry
  (staged and backed by owned stock), and an unbroken owned shield. Mature
  Raiders are unaffected.
- **Fallback:** a blocked Raider stays home and loses nothing; it is admitted
  to the next world it is allowed into. World creation never rolls High when
  every currently eligible Raider is gated (the condition roll clamps to the
  autonomous policy's next-best dangers, Low/Medium), so a gated Raider
  always has an eligible danger at world creation and an empty world can
  never idle the coordinator. **If no eligible danger exists at a pulse** —
  i.e. the single active world is High and admitting — the gated Raider
  waits at most until that world closes; eligibility is re-checked on every
  admission pulse.
- Absent and polled Handlers produce identical outcomes because the gate runs
  entirely inside the coordinator's admission pulse;
  [HighReadinessGateTests.cs](../../tests/AFKRaiders.Server.Tests/HighReadinessGateTests.cs)
  asserts the eligibility truth table, that young High deployments always
  carry the kit, non-starvation (every Raider settles within its first 24h),
  funnel consistency, determinism, and absent-vs-polled parity.

**Measurements (8h: 2,235 settlements; 96h: 23,548):**

| Window | Danger | n | KO rate (95% CI) | Medical/raid | Secured value/raid | Net/raid (95% CI) |
|---|---|---:|---|---:|---:|---|
| [0,8) | Medium | 1,342 | 30.4% (26.8-34.0%) | 0.92 | 55.60 | 3.53 |
| [0,8) | High | 83 | 55.4% (44.2-67.6%) | 2.75 | 25.80 | -165.87 |
| [8,24) | Medium | 1,972 | 15.6% (13.3-18.0%) | 1.54 | 93.38 | -8.84 |
| [8,24) | High | 379 | 34.8% (28.9-41.2%) | 2.60 | 78.90 | -165.99 |
| [48,96) | Low | 5,022 | 0.1% (0.1-0.2%) | 0.09 | 95.42 | 65.36 |
| [48,96) | Medium | 4,663 | 8.2% (6.7-9.7%) | 1.74 | 112.00 | -6.32 |
| [48,96) | High | 1,870 | 25.1% (22.0-28.3%) | 2.48 | 199.71 | -46.74 (-84.03 to -10.87) |

High admission funnel: **3,318 offered / 3,305 started / 3,298 settled / 13
gate blocks**. Early High settlements fell 115 -> 83 versus Arms 1/2 mostly
through the world-creation clamp (High worlds that only gated Raiders could
populate are never offered, so they do not appear in the funnel), not through
the 13 per-admission blocks. Young High deployments now carry the kit: [0,8)
High medical carried/used rose to 2.76/2.75 from 1.87/1.83 in Arm 1.
Whole-horizon High defeat rate 23.2% (17.8-28.6%) stays in the provisional
band.

**Bands:** onboarding High improves materially — [8,24) falls to 34.8% and
the first-24h aggregate to 178/462 = **38.5% (meets <=45%)**; the [0,8) cell
alone remains above (55.4%, n 83, CI 44.2-67.6%). Onboarding Medium ticks up
to 30.4% at [0,8) (point just above 30%, CI spans it) because gated Raiders
divert into Medium. Mature High 25.1% (22.0-28.3%) sits on the band floor;
mature medical and secured-value order still meet (High 199.71 >
Medium 112.00 > Low 95.42). The guardrail worsens and is now clearly
negative: mature High net -46.74 (CI excludes zero) — report-only, not tuned.

**Not isolated/proven:** the gate rarely binds as a per-admission block in
this harness (13 blocks) because the autonomous economy usually equips young
Raiders; its measured effect flows through world-creation clamping plus seed
drift, so Arm 3 vs Arm 2 deltas in the mature window (e.g. the more negative
High net) are not causal gate effects. The harness enrolls all Raiders at
t=0, so Raider age coincides with world time here; a live mixed-age
population is not simulated. Handler-visible "offer" messaging is unchanged;
the gate is admission-side only.

## Arm 4A — High jackpot re-check A (`desperanza-bal-4-arm4a-v1`)

**Change from Arm 3 only:** High cargo weights become **30/30/20/20** on
catalog values **35/60/150/900**. The expected two-slot reward bundle is
465.18 value versus Arm 3's 695.95; the 12% High weapon-slot chance, weapon
table, shared two-slot allocation, and all other rules remain unchanged.
Reward version: `bal-4-defeat-arm4a-v1`. Compatibility fingerprint:
`6BCECA6CE60AC87DAE8EB5D53FDD3635C1104E0A27F8615AFBEE78CC53F70C87`.

The candidate is selected only by the internal test measurement harness;
normal coordinator construction remains Arm 3. Focused table validation and
expected-value tests are in
[Bal2RobotDefeatTests.cs](../../tests/AFKRaiders.Server.Tests/Bal2RobotDefeatTests.cs).

**Measured results:**

| Window | Danger | n | KO rate (95% CI); causes | Medical/raid | Robot defeats/fights (95% CI) | Secured value/raid (95% CI) | Net assets/raid (95% CI) |
|---|---|---:|---|---:|---|---|---|
| [0,8) | Low | 810 | 0.0% (0.0-0.0); — | 0.2 | 597/662 (90.2%; 87.5-92.7) | 80.1 (76.6-83.5) | 70.4 (67.0-73.7) |
| [0,8) | Medium | 1,342 | 30.4% (26.9-34.1); Apology Weather 331, kinetic swipe 77 | 0.9 | 268/946 (28.3%; 23.7-33.2) | 55.6 (50.2-61.1) | 3.5 (-1.0 to 8.1) |
| [0,8) | High | 83 | 55.4% (44.2-67.5); Overtime Red 26, kinetic swipe 19, deadline 1 | 2.7 | 0/61 (0.0%; 0.0-0.0) | 25.8 (18.0-33.4) | -165.9 (-195.0 to -139.2) |
| [8,24) | Low | 1,587 | 0.1% (0.0-0.2); deadline 1 | 0.1 | 1,511/1,543 (97.9%; 96.9-98.8) | 113.5 (110.6-116.2) | 89.5 (87.3-91.7) |
| [8,24) | Medium | 1,967 | 15.3% (13.0-17.8); Apology Weather 264, kinetic swipe 36, deadline 1 | 1.6 | 886/1,676 (52.9%; 47.5-58.2) | 94.3 (88.2-100.3) | -7.9 (-12.3 to -3.4) |
| [8,24) | High | 383 | 35.2% (29.4-41.6); Overtime Red 93, kinetic swipe 42 | 2.6 | 19/306 (6.2%; 2.2-10.6) | 67.3 (48.3-89.2) | -179.2 (-209.2 to -148.3) |
| [24,48) | Low | 2,378 | 0.0% (0.0-0.0); — | 0.1 | 2,271/2,284 (99.4%; 98.9-99.8) | 107.3 (104.8-109.8) | 79.1 (76.7-81.5) |
| [24,48) | Medium | 2,506 | 10.8% (8.8-13.0); Apology Weather 248, kinetic swipe 21, deadline 2 | 1.7 | 1,635/2,232 (73.3%; 70.0-76.4) | 111.4 (106.2-116.8) | -1.1 (-5.6 to 3.4) |
| [24,48) | High | 951 | 33.1% (29.7-36.7); Overtime Red 222, kinetic swipe 93 | 2.5 | 119/806 (14.8%; 9.0-21.2) | 97.4 (71.5-127.0) | -156.5 (-184.7 to -125.8) |
| [48,96) | Low | 5,093 | 0.1% (0.0-0.2); deadline 6 | 0.1 | 4,878/4,903 (99.5%; 99.3-99.7) | 95.5 (94.3-96.7) | 66.6 (64.6-68.5) |
| [48,96) | Medium | 4,618 | 8.4% (6.8-10.0); Apology Weather 352, kinetic swipe 30, deadline 6 | 1.8 | 2,941/4,093 (71.9%; 68.0-75.7) | 111.6 (106.4-116.9) | -5.5 (-9.4 to -1.4) |
| [48,96) | High | 1,845 | 27.5% (24.5-30.7); Overtime Red 393, kinetic swipe 115 | 2.5 | 409/1,622 (25.2%; 19.4-31.1) | 139.1 (112.3-166.4) | -112.2 (-139.4 to -84.7) |

| Window close | Raiders | Closing coins p10 | p50 | p90 | p99 |
|---|---:|---:|---:|---:|---:|
| 8h | 128 | 18 (10-27) | 66 (56-78) | 147 (128-159) | 297 (177-371) |
| 24h | 128 | 42 (36-46) | 102 (86-119) | 253 (229-293) | 634 (300-2,578) |
| 48h | 128 | 43 (32-60) | 138 (112-166) | 436 (322-1,722) | 4,984 (2,257-7,717) |
| 96h | 128 | 36 (32-43) | 127 (111-150) | 3,324 (2,288-6,168) | 9,737 (7,394-12,793) |

Mature High secured value p50/p90/p99 is 54 (48-58)/256 (200-355)/1,149
(1,123-1,848), n=1,845. Excluding 900-award worlds leaves n=1,696,
secured value 64.9 (57.7-72.5) and net assets -181.7 (-195.7 to -168.1);
all-High net is -112.2 (-139.4 to -84.7), report-only. Defeated High worlds
awarding at least one 900-value item: 7/18 in [8,24), 42/118 in [24,48),
and 149/405 in [48,96); the mature share is 36.8% (32.1-41.3). Whole-run
High defeats are 547/2,795 = 19.6% (14.9-24.3), just below the 20%
point-estimate floor.

The per-window High funnel offered/started/settled/readiness-blocked/age-
excluded is 95/85/83/10/0, 391/388/383/3/0, 949/949/951/0/0, and
1,847/1,847/1,845/0/0. Across the run, 2,327 weapons were liquidated for
41,867 coins; failed weapon/supply/repair purchases were 157/4,001/76 and
there were 504 below-reserve commits. Mature secured value order meets:
High 139.1 > Medium 111.6 > Low 95.5; the mature High net guardrail remains
negative. First-24h KO is Low 0.0% (n=2,397), Medium 21.4% (n=3,309), High
38.8% (n=466).

## Arm 4B — High jackpot re-check B (`desperanza-bal-4-arm4b-v1`)

**Change from Arm 3 only:** High cargo weights become **25/30/20/25** on
catalog values **35/60/150/900**. The expected two-slot reward bundle is
546.49 value. All weapon-slot and ownership/allocation rules remain frozen.
Reward version: `bal-4-defeat-arm4b-v1`. Compatibility fingerprint:
`6285BA484563A01061EFC86D38060166A8AADF646C991D57837858AC8C94F5F5`.

This is a separate candidate, not an incremental change from 4A. It is
selected only by the internal test measurement harness; normal coordinator
construction remains Arm 3. Focused table validation and expected-value tests
are in [Bal2RobotDefeatTests.cs](../../tests/AFKRaiders.Server.Tests/Bal2RobotDefeatTests.cs).

**Measured results:**

| Window | Danger | n | KO rate (95% CI); causes | Medical/raid | Robot defeats/fights (95% CI) | Secured value/raid (95% CI) | Net assets/raid (95% CI) |
|---|---|---:|---|---:|---|---|---|
| [0,8) | Low | 810 | 0.0% (0.0-0.0); — | 0.2 | 597/662 (90.2%; 87.5-92.7) | 80.1 (76.6-83.5) | 70.4 (67.0-73.7) |
| [0,8) | Medium | 1,342 | 30.4% (26.9-34.1); Apology Weather 331, kinetic swipe 77 | 0.9 | 268/946 (28.3%; 23.7-33.2) | 55.6 (50.2-61.1) | 3.5 (-1.0 to 8.1) |
| [0,8) | High | 83 | 55.4% (44.2-67.5); Overtime Red 26, kinetic swipe 19, deadline 1 | 2.7 | 0/61 (0.0%; 0.0-0.0) | 25.8 (18.0-33.4) | -165.9 (-195.0 to -139.2) |
| [8,24) | Low | 1,587 | 0.1% (0.0-0.2); deadline 1 | 0.1 | 1,511/1,543 (97.9%; 96.9-98.8) | 113.5 (110.6-116.2) | 89.5 (87.3-91.7) |
| [8,24) | Medium | 1,967 | 15.3% (13.0-17.8); Apology Weather 264, kinetic swipe 36, deadline 1 | 1.6 | 886/1,676 (52.9%; 47.5-58.2) | 94.3 (88.2-100.3) | -7.9 (-12.3 to -3.4) |
| [8,24) | High | 383 | 35.2% (29.4-41.6); Overtime Red 93, kinetic swipe 42 | 2.6 | 19/306 (6.2%; 2.2-10.6) | 71.7 (49.2-98.3) | -174.8 (-207.4 to -140.0) |
| [24,48) | Low | 2,374 | 0.0% (0.0-0.0); — | 0.1 | 2,267/2,280 (99.4%; 98.9-99.8) | 107.4 (104.9-109.8) | 79.1 (76.7-81.5) |
| [24,48) | Medium | 2,508 | 10.8% (8.7-12.9); Apology Weather 247, kinetic swipe 21, deadline 2 | 1.7 | 1,644/2,235 (73.6%; 70.3-76.8) | 111.7 (106.4-117.0) | -0.8 (-5.4 to 4.0) |
| [24,48) | High | 950 | 33.3% (30.0-36.7); Overtime Red 222, kinetic swipe 94 | 2.6 | 117/804 (14.6%; 9.4-20.3) | 106.7 (79.0-138.1) | -149.3 (-177.8 to -118.0) |
| [48,96) | Low | 5,044 | 0.1% (0.0-0.1); deadline 3 | 0.1 | 4,848/4,872 (99.5%; 99.3-99.7) | 95.9 (94.8-96.9) | 66.7 (64.6-68.7) |
| [48,96) | Medium | 4,618 | 8.1% (6.6-9.6); Apology Weather 333, kinetic swipe 34, deadline 7 | 1.8 | 2,980/4,109 (72.5%; 68.8-76.3) | 112.5 (107.6-117.5) | -5.4 (-9.1 to -1.6) |
| [48,96) | High | 1,874 | 25.8% (22.8-28.9); Overtime Red 370, kinetic swipe 113 | 2.5 | 465/1,650 (28.2%; 21.9-34.4) | 166.2 (135.1-197.2) | -82.3 (-114.3 to -50.4) |

| Window close | Raiders | Closing coins p10 | p50 | p90 | p99 |
|---|---:|---:|---:|---:|---:|
| 8h | 128 | 18 (10-27) | 66 (56-78) | 147 (128-159) | 297 (177-371) |
| 24h | 128 | 42 (36-46) | 103 (88-125) | 258 (232-293) | 1,181 (300-3,328) |
| 48h | 128 | 48 (32-61) | 140 (113-166) | 991 (366-1,804) | 5,396 (2,437-8,582) |
| 96h | 128 | 35 (32-40) | 134 (115-160) | 5,444 (3,417-7,887) | 12,115 (8,583-13,543) |

Mature High secured value p50/p90/p99 is 55 (50-59)/334 (250-995)/1,822
(1,135-1,860), n=1,874. Excluding 900-award worlds leaves n=1,677,
secured value 66.6 (59.6-73.9) and net assets -178.9 (-193.0 to -165.4);
all-High net is -82.3 (-114.3 to -50.4), report-only. Defeated High worlds
awarding at least one 900-value item: 9/18 in [8,24), 51/116 in [24,48),
and 194/462 in [48,96); the mature share is 42.0% (38.0-46.3). Whole-run
High defeats are 601/2,821 = 21.3% (16.4-26.3), inside the provisional
20-30% band by point estimate.

The per-window High funnel offered/started/settled/readiness-blocked/age-
excluded is 95/85/83/10/0, 391/388/383/3/0, 950/950/950/0/0, and
1,873/1,873/1,874/0/0. Across the run, 2,378 weapons were liquidated for
43,215 coins; failed weapon/supply/repair purchases were 139/3,948/72 and
there were 500 below-reserve commits. Mature secured value order meets:
High 166.2 > Medium 112.5 > Low 95.9; the mature High net guardrail remains
negative. First-24h KO is Low 0.0% (n=2,397), Medium 21.4% (n=3,309), High
38.8% (n=466).

## Arm 5 — early-High block (`desperanza-bal-4-arm5-v1`)

**Change from Arm 3 only:** use the deterministic server-owned enrollment
clock. Under 8h, do not offer or admit a Raider to any High world regardless
of readiness. From 8h through ages under 24h, apply the existing Arm 3 kit
gate unchanged. At 24h and older, impose no High gate.

World creation clamps a High roll to the next-best Low/Medium dangers when
every eligible Raider is blocked, so the age gate does not create an empty
High world. If a blocked Raider encounters an already-active High world, it
waits at home until that world's admission window or world closes and then
re-enters the normal admission queue; it retains normal Low/Medium
eligibility. A world expires after 1,320 seconds, and the coordinator retries
admission on its 30-second pulse. No blocked Raider may remain stranded.
Under-8h exclusions are recorded separately and are not counted as High
offers. Absent and polled Handlers must remain observationally identical.

Reward version remains `bal-4-defeat-v1`. Compatibility fingerprint:
`A26935F4A799EA192DC3999489C0A6161CF1D1011A1BEF564FDB160D0C8D6744`.
The focused age-boundary, no-starvation, funnel, determinism and parity checks
are in [HighReadinessGateTests.cs](../../tests/AFKRaiders.Server.Tests/HighReadinessGateTests.cs).

**Measured results:**

| Window | Danger | n | KO rate (95% CI); causes | Medical/raid | Robot defeats/fights (95% CI) | Secured value/raid (95% CI) | Net assets/raid (95% CI) |
|---|---|---:|---|---:|---|---|---|
| [0,8) | Low | 823 | 0.0% (0.0-0.0); — | 0.2 | 595/655 (90.8%; 88.2-93.4) | 77.9 (74.2-81.8) | 67.2 (63.7-70.8) |
| [0,8) | Medium | 1,372 | 28.6% (24.9-32.4); Apology Weather 324, kinetic swipe 68 | 1.0 | 325/971 (33.5%; 27.8-39.5) | 59.3 (53.1-65.9) | 3.6 (-1.0 to 8.2) |
| [0,8) | High | 0 | No High offers or settlements under the age rule | — | — | — | — |
| [8,24) | Low | 1,548 | 0.2% (0.0-0.4); deadline 3 | 0.1 | 1,488/1,505 (98.9%; 98.1-99.5) | 113.6 (110.7-116.3) | 89.1 (86.1-91.9) |
| [8,24) | Medium | 1,963 | 14.0% (11.6-16.4); Apology Weather 238, kinetic swipe 34, deadline 2 | 1.5 | 925/1,642 (56.3%; 51.3-61.3) | 96.7 (90.7-102.6) | -2.9 (-7.0 to 1.5) |
| [8,24) | High | 439 | 36.0% (30.9-41.4); Overtime Red 104, kinetic swipe 54 | 2.7 | 19/364 (5.2%; 1.5-10.1) | 61.4 (42.0-87.3) | -193.8 (-223.5 to -159.2) |
| [24,48) | Low | 2,366 | 0.0% (0.0-0.1); deadline 1 | 0.1 | 2,271/2,286 (99.3%; 98.8-99.8) | 108.7 (106.4-110.9) | 79.2 (77.0-81.3) |
| [24,48) | Medium | 2,508 | 9.9% (8.3-11.5); Apology Weather 235, kinetic swipe 11, deadline 2 | 1.7 | 1,683/2,257 (74.6%; 70.8-78.3) | 113.3 (108.1-118.7) | -2.6 (-7.3 to 2.3) |
| [24,48) | High | 945 | 32.7% (28.7-36.9); Overtime Red 219, kinetic swipe 90 | 2.6 | 132/802 (16.5%; 9.9-23.6) | 128.9 (89.5-172.5) | -125.5 (-167.6 to -79.6) |
| [48,96) | Low | 5,058 | 0.1% (0.0-0.2); deadline 4 | 0.1 | 4,826/4,860 (99.3%; 99.0-99.6) | 95.4 (94.2-96.6) | 65.8 (63.5-67.9) |
| [48,96) | Medium | 4,629 | 8.3% (6.9-9.7); Apology Weather 346, kinetic swipe 35, deadline 3 | 1.8 | 2,960/4,118 (71.9%; 68.0-75.8) | 112.2 (107.4-117.0) | -6.0 (-9.8 to -2.0) |
| [48,96) | High | 1,832 | 26.9% (24.0-29.9); Overtime Red 374, kinetic swipe 119 | 2.5 | 483/1,607 (30.1%; 23.8-36.2) | 200.8 (164.8-236.6) | -51.1 (-87.9 to -14.0) |

| Window close | Raiders | Closing coins p10 | p50 | p90 | p99 |
|---|---:|---:|---:|---:|---:|
| 8h | 128 | 26 (18-33) | 78 (70-89) | 173 (146-226) | 298 (252-348) |
| 24h | 128 | 41 (38-50) | 118 (95-127) | 265 (222-329) | 822 (342-3,498) |
| 48h | 128 | 34 (27-49) | 130 (98-159) | 1,807 (367-3,482) | 6,573 (3,482-8,731) |
| 96h | 128 | 38 (32-49) | 154 (118-191) | 7,364 (5,353-9,326) | 12,712 (10,713-22,008) |

Mature High secured value p50/p90/p99 is 55 (50-58)/992 (315-1,017)/1,867
(1,855-1,885), n=1,832. Excluding 900-award worlds leaves n=1,572,
secured value 62.2 (55.4-69.3) and net assets -185.3 (-199.0 to -172.3);
all-High net is -51.1 (-87.9 to -14.0), report-only. Defeated High worlds
awarding at least one 900-value item: 11/18 in [8,24), 64/131 in [24,48),
and 251/477 in [48,96); the mature share is 52.6% (47.0-58.3).

The per-window High funnel offered/started/settled/readiness-blocked/age-
excluded is 0/0/0/0/0, 452/446/439/6/0, 945/945/945/0/0, and
1,830/1,830/1,832/0/0. Under-8h ineligibility was handled by the world-
creation clamp; no admission-side age-block event occurred, so no
age-block-induced idle state was observed. Normal between-raid waits remain:
time to the next admission after a settlement in [0,8) was mean/p50/p90
813/630/1,140 seconds (n=2,195); the p90 in later windows was 1,200 seconds.
All 128 Raiders settled at least one Low or Medium raid in [0,8), so none
starved.
Across the run, 2,357 weapons were liquidated for 43,165 coins; failed
weapon/supply/repair purchases were 135/3,810/59 and there were 466
below-reserve commits. Mature secured value order meets:
High 200.8 > Medium 112.2 > Low 95.4; mature High net remains negative,
report-only. First-24h KO is Low 0.1% (n=2,371), Medium 20.0% (n=3,335),
High 36.0% (n=439).

Arm 5's [0,8) Medium KO is 28.6% (24.9-32.4) versus Arm 3's 30.4%
(26.9-34.1), a matched delta of -1.8 percentage points (-3.9 to +0.2;
n=1,372 vs 1,342). The point shift is within the stated ~2-point noise
floor and the interval includes no change; this is not evidence that the
age rule reduces Medium risk.

## Cross-arm comparison

The original BAL-2, first-arm, and Arms 1-3 columns are retained; Arms 4A,
4B and 5 extend the same comparison. New-arm cells use the 64-seed cohort.
Full per-window denominators, causes, 95% intervals, coins and funnels are in
the arm sections above; the trait-cell appendix follows this table.

| Measure | BAL-2 control | First arm (bal-4-v1) | Arm 1 | Arm 2 | Arm 3 | Arm 4A | Arm 4B | Arm 5 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| High KO [0,8) | 54.0% (n 100) | 64.0% (n 111) | 60.9% (n 115) | 60.9% (n 115) | 55.4% (n 83) | 55.4% (n 83) | 55.4% (n 83) | blocked; n 0 |
| High KO [8,24) | 43.0% (n 423) | - | 41.0% (n 439) | 43.5% (n 446) | 34.8% (n 379) | 35.2% (n 383) | 35.2% (n 383) | 36.0% (n 439) |
| High KO first-24h aggregate | 45.0% | - | 45.1% | 47.1% | **38.5%** | 38.8% | 38.8% | 36.0% |
| High KO [48,96) | 30.6% (n 1,802) | 22.9% (n 1,943) | 24.2% (n 1,934) | 24.8% (n 1,864) | 25.1% (n 1,870) | 27.5% (n 1,845) | 25.8% (n 1,874) | 26.9% (n 1,832) |
| Medium KO [0,8) | 29.0% | 29.7% | 29.7% | 29.7% | 30.4% | 30.4% | 30.4% | 28.6% |
| Medium KO [48,96) | 9.3% | 7.7% | 7.6% | 9.5% | 8.2% | 8.4% | 8.1% | 8.3% |
| Mature secured value L/M/H | (96h: 99.16/97.50/59.47) | 95.88/114.10/319.10 | 96.34/115.56/225.65 | 95.17/108.75/228.10 | 95.42/112.00/199.71 | 95.5/111.6/139.1 | 95.9/112.5/166.2 | 95.4/112.2/200.8 |
| Mature High net assets | -174.78 (96h) | +71.68 | -18.58 | -12.85 | -46.74 | -112.2 | -82.3 | -51.1 |
| High defeat rate (96h) | 14.2% | 36.6% | 24.5% | 24.6% | 23.2% | 19.6% | 21.3% | 22.9% |
| Mature High secured p50/p90/p99 | — | — | — | — | 56/988/1,868 | 54/256/1,149 | 55/334/1,822 | 55/992/1,867 |
| Mature defeated High worlds with a 900-value award | — | — | — | — | not instrumented | 149/405 (36.8%) | 194/462 (42.0%) | 251/477 (52.6%) |
| Cap liquidations / coins | uncapped (+2,171 net) | 2,551 / 97,130 | 2,440 / 90,430 | 2,329 / 42,797 | 2,394 / 43,896 | 2,327 / 41,867 | 2,378 / 43,215 | 2,357 / 43,165 |
| Closing coins p50 / p90 (96h) | mean 221.29 | mean 4,295.94 | 162 / 10,531.70 | 138 / 9,549.50 | 147.50 / 6,613.40 | 127 / 3,324 | 134 / 5,444 | 154 / 7,364 |
| First-raid High extraction (starter/staged) | stale 57.9% (BAL-1S era) | not rerun | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%) | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%) | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%) | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%) | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%) | 32.5% (25.4-40.0%) / 20.0% (15.0-25.4%)* |

*The Arm 5 first-raid High diagnostic is a fixed-world combat cohort, not a
coordinator admission test; under the age rule, an actual under-8h Raider
cannot enter that High world.

### Trait-level KO rates by window

Each cell is `settlements; KO rate (95% seed-cluster CI)`. Cells with fewer
than 50 settlements are omitted; this excludes the small [0,8) High trait
cells (n=18-24), not the aggregate High row above.

| Arm | Window | Danger | Brawler | Cautious | Gadgeteer | Hoarder |
|---|---|---|---|---|---|---|
| 4A | [0,8) | Low | 201; 0.0% (0.0-0.0) | 198; 0.0% (0.0-0.0) | 195; 0.0% (0.0-0.0) | 216; 0.0% (0.0-0.0) |
| 4A | [0,8) | Medium | 345; 33.0% (26.9-39.1) | 336; 28.6% (22.9-34.3) | 327; 26.3% (20.0-32.9) | 334; 33.5% (26.5-40.4) |
| 4A | [8,24) | Low | 397; 0.0% (0.0-0.0) | 414; 0.0% (0.0-0.0) | 393; 0.0% (0.0-0.0) | 383; 0.3% (0.0-0.9) |
| 4A | [8,24) | Medium | 483; 15.3% (11.0-20.0) | 489; 18.4% (13.6-23.8) | 507; 16.2% (11.6-20.8) | 488; 11.3% (7.8-15.1) |
| 4A | [8,24) | High | 101; 32.7% (22.7-44.4) | 84; 33.3% (22.1-47.0) | 91; 36.3% (26.7-47.1) | 107; 38.3% (28.3-49.5) |
| 4A | [24,48) | Low | 593; 0.0% (0.0-0.0) | 598; 0.0% (0.0-0.0) | 573; 0.0% (0.0-0.0) | 614; 0.0% (0.0-0.0) |
| 4A | [24,48) | Medium | 616; 10.7% (6.3-15.7) | 638; 14.3% (9.2-19.9) | 653; 9.0% (5.4-12.9) | 599; 9.2% (6.7-11.7) |
| 4A | [24,48) | High | 240; 32.5% (25.0-40.5) | 232; 36.2% (30.5-42.6) | 241; 26.1% (19.0-34.0) | 238; 37.8% (31.3-45.2) |
| 4A | [48,96) | Low | 1,251; 0.1% (0.0-0.3) | 1,249; 0.1% (0.0-0.3) | 1,325; 0.2% (0.0-0.4) | 1,268; 0.2% (0.0-0.4) |
| 4A | [48,96) | Medium | 1,176; 7.5% (4.2-11.1) | 1,205; 9.5% (6.8-12.1) | 1,095; 9.4% (6.0-13.2) | 1,142; 7.3% (5.2-9.4) |
| 4A | [48,96) | High | 462; 26.6% (20.8-33.1) | 447; 29.1% (22.8-35.6) | 475; 27.2% (20.3-34.7) | 461; 27.3% (22.5-32.5) |
| 4B | [0,8) | Low | 201; 0.0% (0.0-0.0) | 198; 0.0% (0.0-0.0) | 195; 0.0% (0.0-0.0) | 216; 0.0% (0.0-0.0) |
| 4B | [0,8) | Medium | 345; 33.0% (26.9-39.1) | 336; 28.6% (22.9-34.3) | 327; 26.3% (20.0-32.9) | 334; 33.5% (26.5-40.4) |
| 4B | [8,24) | Low | 397; 0.0% (0.0-0.0) | 414; 0.0% (0.0-0.0) | 393; 0.0% (0.0-0.0) | 383; 0.3% (0.0-0.9) |
| 4B | [8,24) | Medium | 483; 15.3% (11.0-20.0) | 489; 18.4% (13.6-23.8) | 507; 16.2% (11.6-20.8) | 488; 11.3% (7.8-15.1) |
| 4B | [8,24) | High | 101; 32.7% (22.7-44.4) | 84; 33.3% (22.1-47.0) | 91; 36.3% (26.7-47.1) | 107; 38.3% (28.3-49.5) |
| 4B | [24,48) | Low | 591; 0.0% (0.0-0.0) | 598; 0.0% (0.0-0.0) | 570; 0.0% (0.0-0.0) | 615; 0.0% (0.0-0.0) |
| 4B | [24,48) | Medium | 616; 10.4% (5.9-15.4) | 638; 14.3% (9.2-19.9) | 657; 9.4% (5.8-13.3) | 597; 8.9% (6.4-11.4) |
| 4B | [24,48) | High | 241; 32.0% (24.5-39.8) | 231; 36.4% (30.7-42.7) | 238; 26.9% (19.9-34.6) | 240; 37.9% (31.4-45.2) |
| 4B | [48,96) | Low | 1,247; 0.0% (0.0-0.0) | 1,239; 0.1% (0.0-0.3) | 1,305; 0.1% (0.0-0.3) | 1,253; 0.1% (0.0-0.3) |
| 4B | [48,96) | Medium | 1,166; 7.5% (4.3-11.1) | 1,225; 9.6% (7.0-12.1) | 1,079; 8.4% (5.1-12.2) | 1,148; 6.8% (4.7-9.0) |
| 4B | [48,96) | High | 467; 26.3% (20.2-33.1) | 442; 29.6% (23.4-36.3) | 494; 24.1% (18.0-30.8) | 471; 23.4% (18.2-28.9) |
| 5 | [0,8) | Low | 200; 0.0% (0.0-0.0) | 211; 0.0% (0.0-0.0) | 195; 0.0% (0.0-0.0) | 217; 0.0% (0.0-0.0) |
| 5 | [0,8) | Medium | 353; 32.3% (25.3-39.0) | 334; 26.9% (21.4-32.4) | 340; 22.6% (16.0-29.4) | 345; 32.2% (25.8-38.8) |
| 5 | [8,24) | Low | 411; 0.0% (0.0-0.0) | 371; 0.3% (0.0-0.9) | 373; 0.0% (0.0-0.0) | 393; 0.5% (0.0-1.3) |
| 5 | [8,24) | Medium | 489; 20.2% (15.5-25.3) | 499; 9.0% (5.6-13.4) | 490; 13.3% (9.5-17.2) | 485; 13.4% (9.1-17.9) |
| 5 | [8,24) | High | 100; 32.0% (24.1-41.1) | 105; 37.1% (26.7-50.0) | 121; 36.4% (26.9-46.9) | 113; 38.1% (29.3-47.7) |
| 5 | [24,48) | Low | 579; 0.2% (0.0-0.6) | 578; 0.0% (0.0-0.0) | 600; 0.0% (0.0-0.0) | 609; 0.0% (0.0-0.0) |
| 5 | [24,48) | Medium | 624; 9.8% (6.8-12.8) | 642; 9.8% (6.5-13.5) | 623; 11.6% (8.0-15.1) | 619; 8.4% (6.0-10.9) |
| 5 | [24,48) | High | 248; 28.2% (21.0-36.3) | 238; 32.4% (25.0-41.0) | 228; 32.0% (24.9-39.6) | 231; 38.5% (31.3-46.4) |
| 5 | [48,96) | Low | 1,261; 0.0% (0.0-0.0) | 1,306; 0.2% (0.0-0.4) | 1,226; 0.2% (0.0-0.4) | 1,265; 0.0% (0.0-0.0) |
| 5 | [48,96) | Medium | 1,138; 8.3% (5.1-11.8) | 1,121; 8.4% (5.4-11.6) | 1,191; 8.7% (5.9-11.6) | 1,179; 7.8% (5.7-10.0) |
| 5 | [48,96) | High | 475; 22.9% (17.6-29.2) | 464; 27.4% (21.0-34.5) | 451; 27.5% (21.9-33.6) | 442; 30.1% (24.2-36.6) |

The fresh first-raid High cohort (120 seeds x 2 Raiders, unattended)
**replaces the stale 57.9% figure**: with the current rules (45 HP High,
BAL-4 rewards), starter extraction is 78/240 = 32.5% (25.4-40.0%) and the
staged-kit variant is 48/240 = 20.0% (15.0-25.4%), both 95% seed-cluster
intervals with zero robot defeats. The staged kit is
*worse* for extraction because medical keeps young Raiders alive into more
robot fights (188 vs 128) that they cannot win; this cohort is a solo fixed
world and identical across the reward tables. For Arm 5 it is only a fixed-
world combat diagnostic: it bypasses the coordinator age gate and does not
represent an under-8h Raider being admitted to High.

**Band judgments (point estimates):** Arm 4A and 4B both meet the first-24h
High KO ceiling (38.8%) but miss the explicitly unacceptable [0,8) High cell
(55.4%); they meet first-24h Medium in aggregate (21.4%) but miss its [0,8)
ceiling narrowly (30.4%). Their onboarding Low rates meet the <=10% ceiling.
Arm 5 meets onboarding High (36.0%), Medium (20.0%) and Low (0.1%); it has
no [0,8) High offers or settlements. In mature play, all three arms meet Low
(0.1%) and High (4A 27.5%, 4B 25.8%, Arm 5 26.9%), while all miss Medium
(4A 8.4%, 4B 8.1%, Arm 5 8.3%; the hazard work remains deferred to BAL-3).
Mature secured value order meets for all three: High > Medium > Low. Whole-
run High defeat rate misses the 20% lower bound for 4A (19.6%) and meets it
for 4B (21.3%) and Arm 5 (22.9%). The mature High net-asset guardrail is
report-only and negative in all three.

## What these arms did not isolate or prove

- Each follow-up is still a 64-seed simulation, not a player population.
  The new arm differences are measured from the Arm 3 state but can still
  contain seed-path effects through shared worlds and the autonomous
  economy; the 20,000-resample intervals preserve the seed cluster.
- The reward tables do not change fight mechanics directly. Differences in
  KO or fight outcomes are indirect economy/world-path effects; the mature
  High KO deltas versus Arm 3 are +2.5 pp for 4A, +0.7 pp for 4B and +1.8 pp
  for Arm 5. Treat changes under about 2 points as noise and do not tune
  mature High risk.
- Arms 4A and 4B lower jackpot frequency by changing the cargo weights, but
  do not eliminate the jackpot tail. Arm 4A cuts mature High p90/p99 value
  more sharply than 4B but has a 19.6% whole-run High defeat point estimate
  and a more negative mature High net result. Neither resolves the
  report-only negative mature High net guardrail.
- The Arm 5 cohort enrolls both Raiders at t=0. It proves the age truth
  table, funnel accounting and non-starvation behavior under that harness,
  not behavior in a live mixed-age population or deliberately broke Raiders.
  No admission-side age-block event occurred in the measured run because
  the world-creation clamp prevented an ineligible High offer.
- The separate fixed-world first-raid High cohort bypasses Arm 5's gate;
  it cannot certify that an under-8h Raider may enter High.
- The source-content search covered the original `src/content` and server
  content and found existing A.R.C./Temptrest-related material. No new
  player-facing text or content was authored or imported in these arms, so no
  provenance row was needed.
- The Arm 3 baseline raw file predates per-window admission-event telemetry
  and the world-level 900-award flag. Arm 3 funnel comparisons are whole-run
  only, and its jackpot-award share is not instrumented; do not infer a
  zero-award rate from the missing flag.
- 25% sell-back is the accepted standing experimental value, but no-cash
  retirement and other rates remain untested. No combined 4B+5 confirmation
  run was performed. No browser multiplayer session was run.

## Recommendation

Prefer **Arm 4B's reward table** over 4A if the designer wants to reduce
jackpot concentration: it lowers mature High p90 secured value from 988
(Arm 3) to 334 while preserving the provisional whole-run High defeat band
(21.3%); 4A lowers p90 further to 256 but its High defeat point estimate is
below band and its mature High net is more negative. Keep Arm 4B
experimental, not certified.

Prefer **Arm 5's early-High block** for onboarding if the designer approves
it: there are no under-8h High settlements, first-24h High KO is 36.0%,
all 128 Raiders settle a Low or Medium raid in the first window, and mature
High KO remains in the designer-accepted band. It is not stacked with Arm
4B in these measurements.

The recommended next experiment, only after approval, is one combined
confirmation of **Arm 4B + Arm 5**. No combined run was started. Do not tune
mature High risk for the observed <2-point changes; keep Medium hazard work
deferred to BAL-3. The 25% sell-back and High 45/18/4 decisions remain
settled, experimental defaults.

Decisions still open for the designer:

1. Retain the Arm 3 reward weights, select Arm 4A, or prefer Arm 4B? The
   recommendation is 4B; 4A is the stronger tail cut but misses the High
   defeat-rate point floor and worsens mature High net.
2. Replace Arm 3's first-24h readiness-only rule with Arm 5's under-8h age
   block plus unchanged 8-24h kit gate? The recommendation is Arm 5.
3. If both are selected, authorize the separately measured Arm 4B + Arm 5
   confirmation run? It has not been run.
4. No-cash overflow retirement remains a possible later arm, not part of
   this work.

## Verification

Per-arm rules versions and fingerprints are recorded above. The focused
measurement tests and final full-suite/web build results are recorded in the
progress ledger. Each measured candidate used the same command shape with
`arm` set to `arm4a`, `arm4b`, or `arm5` and `hours` set to 8 or 96:

```powershell
$env:AFK_BALANCE_ARM='arm'; $env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='hours'; $env:AFK_BALANCE_REPORT_PATH="$env:LOCALAPPDATA\Temp\bal4-arm-64xhours.md"; $env:AFK_BALANCE_RAW_PATH="$env:LOCALAPPDATA\Temp\bal4-arm-64xhours.raw.json"; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

The six cohort runs were independent (one 8h and one 96h run for each
candidate); the 96h windows include no events at or beyond 96h. A deterministic
20,000-resample seed-cluster percentile bootstrap (NumPy default RNG seed
20261103) produced the intervals. The separate 8h and 96h [0,8) settlement
counts matched for every candidate. No backend restart, commit, or push.

## Out of scope

No combined confirmation run, BAL-3 shared-hazard work, ECON-3, shield
retuning, Medium staging, Handler economy controls, persistence/lore work,
or browser multiplayer verification.
