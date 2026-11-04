# BAL-4 experiment arms (2026-11-03)

**Status: three separately measured experimental arms, one lever per arm, on
top of the measured BAL-4 first arm. Nothing here is balance-certified.** The
worktree holds all three arms cumulatively; each arm has its own rules
version, compatibility fingerprint, and matched 64-seed measurements so any
single lever is easy to revert. Arm 4 (the jackpot re-check) was **not**
started; its necessity is assessed from Arm 1 below.

The designer decisions that govern these arms are recorded in the
[BAL-4 proposal](BAL-4-proposal.md) ("Designer decisions (2026-11-03)") and in
the [progress ledger's](../IMPLEMENTATION_PROGRESS.md) decision-authority row:
two KO bands (mature 48h+: Low <=10%, Medium 15-25%, High 25-40%; onboarding
first 24h: High <=45%, Medium <=30%, Low <=10%), medical ranges unchanged,
value order = secured loot value per raid in the mature window with a
report-only High net-asset guardrail, Medium robot stats untouched (hazard
conflict deferred to BAL-3), shields held, the four-weapon cap kept, High
reward weights 25/25/15/35 held pending Arm 1 re-measurement, and strictly one
lever per arm.

## Protocol

Every arm used matched seeds 1-64, two unattended Raiders per seed, pinned
IDs/trader seed, five-second scheduling, the recurring autonomous economy, and
no Handler Signals; one standalone 8h run and one 96h run per arm, with
half-open true-settlement-time windows [0,8), [8,24), [24,48), [48,96).
Confidence intervals are deterministic 20,000-resample seed-cluster percentile
intervals (both Raiders and their raids resampled with their seed). The
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
[BAL-2-measurements.md](BAL-2-measurements.md).

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
(identical cohort), [8,24) 43.5% (39.2-48.0%), [48,96) High 24.6%* / 24.8%
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

## Cross-arm comparison

Early/late High and Medium KO against both baselines (point estimates;
intervals above and in the arm bootstrap outputs):

| Measure | BAL-2 control | First arm (bal-4-v1) | Arm 1 | Arm 2 | Arm 3 |
|---|---:|---:|---:|---:|---:|
| High KO [0,8) | 54.0% (n 100) | 64.0% (n 111) | 60.9% (n 115) | 60.9% (n 115) | 55.4% (n 83) |
| High KO [8,24) | 43.0% (n 423) | - | 41.0% (n 439) | 43.5% (n 446) | 34.8% (n 379) |
| High KO first-24h aggregate | 45.0% | - | 45.1% | 47.1% | **38.5%** |
| High KO [48,96) | 30.6% (n 1,802) | 22.9% (n 1,943) | 24.2% (n 1,934) | 24.8% (n 1,864) | 25.1% (n 1,870) |
| Medium KO [0,8) | 29.0% | 29.7% | 29.7% | 29.7% | 30.4% |
| Medium KO [48,96) | 9.3% | 7.7% | 7.6% | 9.5% | 8.2% |
| Mature secured value L/M/H | (96h: 99.16/97.50/59.47) | 95.88/114.10/319.10 | 96.34/115.56/225.65 | 95.17/108.75/228.10 | 95.42/112.00/199.71 |
| Mature High net assets | -174.78 (96h) | +71.68 | -18.58 | -12.85 | -46.74 |
| High defeat rate (96h) | 14.2% | 36.6% | 24.5% | 24.6% | 23.2% |
| Cap liquidations / coins | uncapped (+2,171 net) | 2,551 / 97,130 | 2,440 / 90,430 | 2,329 / 42,797 | 2,394 / 43,896 |
| Closing coins p50 / p90 (96h) | mean 221.29 | mean 4,295.94 | 162 / 10,531.70 | 138 / 9,549.50 | 147.50 / 6,613.40 |
| First-raid High extraction (starter/staged) | stale 57.9% (BAL-1S era) | not rerun | 32.5% / 20.0% | 32.5% / 20.0% | 32.5% / 20.0% |

The fresh first-raid High cohort (120 seeds x 2 Raiders, unattended)
**replaces the stale 57.9% figure**: with the current rules (45 HP High,
BAL-4 rewards), starter extraction is 78/240 = 32.5% and the staged-kit
variant is 48/240 = 20.0%, both with zero robot defeats. The staged kit is
*worse* for extraction because medical keeps young Raiders alive into more
robot fights (188 vs 128) that they cannot win; this cohort is a solo fixed
world and identical across the three arms.

## What these arms did not isolate or prove

- No arm was measured against the BAL-2 reward table; every arm inherits the
  25/25/15/35 weights and the cap, so "Arm 1 effect" means "HP lever given
  the rest of the first arm".
- Arm-to-arm deltas in windows an arm's lever cannot touch (e.g. Medium
  mature KO 7.6% -> 9.5% -> 8.2%) are seed-path knock-on through the shared
  economy, and bound the noise floor of this 64-seed design.
- The [0,8) High cell stays small (83-115 settlements) and hazard-dominated;
  no measured lever brings it under the 45% onboarding band.
- 25% sell-back is a single measured point; no-cash retirement and other
  rates are untested. Inflation's main residual driver (jackpot tail) was
  not changed by any arm.
- The gate was measured on a cohort enrolled at t=0 with an economy that
  usually self-equips; live mixed-age populations, deliberately broke
  Raiders, and Handler-facing messaging are unvalidated.
- 96h x 64 seeds of two-Raider worlds is simulation evidence, not a player
  population; no browser session was run.

## Recommendation

Keep **Arm 3 cumulative state** (High 45/18/4 + 25% sell-back + readiness
gate) as the standing experimental candidate:

1. Arm 1's HP restore is clearly right by the designer's own bands: it moves
   the High defeat rate into the accepted 20-30% band and restores the
   mature secured-value order without the first arm's 49.2% late defeat
   inflation. Nothing argues for returning to 40 HP.
2. Arm 2 halves liquidation receipts with no measurable liquidity harm
   (failed purchases/repairs and below-reserve commits are flat). The
   remaining coin tail is jackpot-driven, so sell-back alone cannot close
   the inflation question — accept 25% or test a no-cash sink later.
3. Arm 3 is the only lever that moved onboarding High inside the <=45% band
   (first-24h aggregate 38.5%) while leaving mature play statistically where
   Arm 2 put it, at the cost of a slightly hotter [0,8) Medium cell.

Next step, in order of information value: decide the three open accept/hold
calls below, then — if High value concentration matters — run the deferred
**Arm 4 jackpot re-check** as the next single lever (the 35% weight drives
both the value order and the coin tail). Do not start BAL-3 Medium hazard
work inside BAL-4.

Decisions this leaves with the designer:

1. Accept 25% sell-back permanently, or request a no-cash sink arm?
2. Is mature High KO "in band" with a point estimate at 24-25% and intervals
   straddling the 25% floor, or should a small risk lever (e.g. hazard, not
   HP) lift it?
3. Is the [0,8) High cell (55-61% KO, n ~100, hazard-caused) acceptable as
   the price of the softer aggregate, or should the gate's kit be extended
   (e.g. minimum shield tier) in a future arm?
4. Jackpot weights: keep 25/25/15/35 despite tail-driven value and a
   guardrail that stays negative, or schedule Arm 4?

## Verification

Per arm: `NarrationCompatibilityTests` regenerated the fingerprint; the full
server suite passed with `DOTNET_PROCESSOR_COUNT=2` — Arm 1 **476/476**,
Arm 2 **476/476**, Arm 3 **486/486** (ten new readiness-gate tests). The
measurement commands per arm (8h and 96h) were:

```powershell
$env:AFK_BALANCE_SEEDS='64'; $env:AFK_BALANCE_HOURS='8'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-<arm>-64x8h.md'; $env:AFK_BALANCE_RAW_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-<arm>-64x8h.raw.json'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
$env:AFK_BALANCE_HOURS='96'; $env:AFK_BALANCE_REPORT_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-<arm>-64x96h.md'; $env:AFK_BALANCE_RAW_PATH='C:\Users\DanJ2\AppData\Local\Temp\bal4-<arm>-64x96h.raw.json'; dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj -c Release -p:UseAppHost=false --no-restore --filter "FullyQualifiedName~BalanceMeasurementTests.WriteBalanceReportWhenRequested"
```

All six runs passed (settlement counts: Arm 1 2,252/23,537; Arm 2
2,252/23,698; Arm 3 2,235/23,548). Seed-cluster intervals were computed from
the temporary raw files with a deterministic 20,000-resample bootstrap
(rng seed 20261103). The web production build, `git diff --check`, and the
Markdown path-link checks are recorded in the ledger entry. No backend
restart, commit, or push.

## Out of scope

No BAL-3 shared-hazard work, ECON-3, Arm 4 jackpot changes, shield retuning,
Medium staging, Handler economy controls, persistence/lore work, or browser
multiplayer verification.
