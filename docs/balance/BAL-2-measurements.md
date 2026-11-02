# BAL-2 matched measurements

Generated 2026-11-01. These are experimental before/after diagnostics, not
balance certification or authorization to tune BAL-1/BAL-1S or start BAL-3/4.

## Method and interpretation

- Baseline rules: `desperanza-bal-1s-v1`, detached at the pre-BAL-2 implementation
  commit `d522970bb53d5e74a9717ad3334cf2974a316cbf`. Only settlement telemetry
  and its report harness were added to that worktree.
- BAL-2 rules: `desperanza-bal-2-v1`.
- Each cohort uses the same 64 seeds, two unattended Raiders, pinned identities
  and trader seed, five-second scheduling, recurring autonomous economy, and no
  Handler Signal use. Seed IDs are 1-64; the report harness is
  `BalanceMeasurementTests.WriteBalanceReportWhenRequested`, with raw
  per-raid telemetry captured through `AFK_BALANCE_RAW_PATH`. The 8h and 96h
  runs are separate horizons.
- Danger sample sizes are not fixed: world admission, settlement timing and
  survival change the number of settled raids. Matched seeds control inputs,
  not per-raid outcomes or sample counts.
- 96h interval rows use true terminal-event timestamps and half-open windows
  `[0,8)`, `[8,24)`, `[24,48)`, `[48,96)`. The baseline 96h aggregate includes
  one settlement exactly at 96h that is excluded from the windows (22,508
  aggregate vs 22,507 windowed); BAL-2 has no exact-96h settlement (23,777 in
  both). Each standalone 8h aggregate similarly includes one exact-8h
  settlement beyond its `[0,8)` window.
- Robot worlds are deduplicated by RaidId; participant fights count each
  settled Raider. Reward values are secured value per settled Raider, not the
  fixed bundle's expected gross value. Weapon net is found + bought + loaner
  minus KO losses; it does not imply bounded inventory.
- The raw per-raid JSON for baseline/BAL-2 at both horizons is retained as
  session artifacts. The repository report keeps the reviewable aggregate
  evidence without committing the approximately 89 MB raw files.

## Standalone 8h cohorts

### KO and healing

| Danger | Settlements, baseline → BAL-2 | KO rate, baseline → BAL-2 | Medical used/raid, baseline → BAL-2 |
|---|---:|---:|---:|
| Low | 720 → 782 | 0.1% → 0.0% | 0.19 → 0.20 |
| Medium | 1,253 → 1,360 | 25.3% → 29.0% | 0.90 → 0.96 |
| High | 216 → 100 | 38.9% → 54.0% | 1.84 → 1.68 |
| All | 2,189 → 2,242 | 18.4% → 20.0% | 0.76 → 0.73 |

### Robots and replacement rewards

| Danger | Participant fights, baseline → BAL-2 | Defeated / escaped, baseline → BAL-2 | Robot items secured/allocated, BAL-2 `[0,8)` | Robot value secured/raid, BAL-2 `[0,8)` |
|---|---:|---:|---:|---:|
| Low | 595 → 643 | 530 / 65 → 578 / 65 | 1,132 / 1,132 | 18.00 |
| Medium | 864 → 960 | 353 / 511 → 269 / 691 | 402 / 466 | 11.34 |
| High | 147 → 59 | 47 / 100 → 0 / 59 | 0 / 0 | 0.00 |
| All | 1,606 → 1,662 | 930 / 676 → 847 / 815 | 1,534 / 1,598 | 13.16 |

The fight counts and outcomes above are aggregate standalone-8h totals.
Reward-item counts and values are from that cohort's half-open `[0,8)` window;
the exact-boundary settlement is excluded. The baseline had no robot-defeat
bundle. BAL-2 allocated two shared slots only on eligible defeats;
allocated-but-not-secured cargo follows the existing KO and extraction rules.
There were 23 robot guns secured from 27 allocations (four later lost), with no
High robot defeat in this short cohort.

### Weapons, economy and shields

| Measure | BAL-1S baseline | BAL-2 |
|---|---:|---:|
| Weapon inflow / KO outflow | 731 / 402 | 672 / 448 |
| Net weapon change | +329 | +224 |
| All-loot value secured per raid | 72.57 | 63.39 |
| Shield spend per settled raid | 14.37 | 13.45 |
| Closing coins / stash value / supply value per Raider | 95.37 / 89.70 / 23.60 | 88.57 / 91.71 / 22.70 |
| Closing usable / broken weapons per Raider | 3.48 / 0.02 | 2.59 / 0.08 |
| Closing shield count, none / Light / Medium / Heavy | 27 / 20 / 76 / 5 | 24 / 40 / 63 / 1 |
| Raiders reaching Medium+ shield; Heavy shield | 109/128; 5/128 | 98/128; 2/128 |
| Median time to Medium+; Heavy | 3.94h; 6.60h | 4.53h; 6.62h |

## 96h cohorts

### KO and healing by settlement-time window

Each cell is baseline → BAL-2. `n` is settled raids; medical is items used per
settled raid.

| Window | Danger | n | KO rate | Medical/raid |
|---|---|---:|---:|---:|
| 0–8h | Low | 720 → 782 | 0.1% → 0.0% | 0.19 → 0.20 |
| 0–8h | Medium | 1,252 → 1,359 | 25.3% → 29.0% | 0.90 → 0.96 |
| 0–8h | High | 216 → 100 | 38.9% → 54.0% | 1.84 → 1.68 |
| 8–24h | Low | 1,318 → 1,582 | 0.3% → 0.2% | 0.08 → 0.13 |
| 8–24h | Medium | 1,822 → 1,969 | 8.9% → 16.2% | 1.56 → 1.51 |
| 8–24h | High | 686 → 423 | 21.3% → 43.0% | 2.13 → 2.34 |
| 24–48h | Low | 2,124 → 2,425 | 0.0% → 0.1% | 0.04 → 0.11 |
| 24–48h | Medium | 2,070 → 2,567 | 5.5% → 11.0% | 1.71 → 1.66 |
| 24–48h | High | 1,359 → 915 | 17.4% → 35.6% | 2.28 → 2.48 |
| 48–96h | Low | 4,204 → 5,177 | 0.2% → 0.1% | 0.04 → 0.10 |
| 48–96h | Medium | 4,201 → 4,676 | 3.0% → 9.3% | 1.64 → 1.75 |
| 48–96h | High | 2,535 → 1,802 | 12.4% → 30.6% | 2.14 → 2.51 |

### Robot outcomes and rewards by window

| Window | Robot worlds, baseline → BAL-2 | Participant fights, baseline → BAL-2 | Defeated / escaped, baseline → BAL-2 | Shared fights, baseline → BAL-2 | Robot items secured/allocated, BAL-2 | Robot value secured/raid, BAL-2 | Robot guns secured/allocated/lost, BAL-2 |
|---|---:|---:|---:|---:|---:|---:|---:|
| 0–8h | 1,558 → 1,604 | 1,605 → 1,661 | 929 / 676 → 847 / 814 | 94 → 114 | 1,534 / 1,598 | 13.16 | 23 / 27 / 4 |
| 8–24h | 3,393 → 3,470 | 3,411 → 3,481 | 2,777 / 634 → 2,393 / 1,088 | 36 → 22 | 4,612 / 4,764 | 25.96 | 88 / 91 / 3 |
| 24–48h | 5,097 → 5,249 | 5,122 → 5,278 | 4,601 / 521 → 3,929 / 1,349 | 50 → 58 | 7,584 / 7,800 | 30.96 | 162 / 166 / 4 |
| 48–96h | 10,235 → 10,593 | 10,259 → 10,649 | 9,502 / 757 → 8,091 / 2,558 | 48 → 112 | 15,716 / 16,070 | 31.64 | 303 / 317 / 14 |

Across the full 96h cohort, Low/Medium/High participant-fight defeat counts
were 7,996/7,347/2,467 baseline and 9,323/5,553/384 with BAL-2. Their fight
counts were 8,074/8,281/4,043 and 9,423/8,945/2,701 respectively; no
unresolved-at-settlement fights were reported. Robot defeats fell from
17,810/20,398 (87.3%) to 15,260/21,069 (72.4%) overall. High defeats fell from
2,467/4,043 (61.0%) to 384/2,701 (14.2%).

### Weapon flow and net assets by window

Weapon inflow is found + bought + loaner; outflow is weapons lost on KO. Net
asset change is the telemetry's mean per-settlement delta.

| Window | Settlements, baseline → BAL-2 | Weapon inflow / outflow, baseline → BAL-2 | Net asset change/raid, baseline → BAL-2 | Shield spend/raid, baseline → BAL-2 |
|---|---:|---:|---:|---:|
| 0–8h | 2,188 → 2,241 | 722 / 402 → 662 / 448 | 28.64 → 22.16 | 13.59 → 12.50 |
| 8–24h | 3,826 → 3,974 | 1,244 / 313 → 956 / 503 | 28.71 → 15.57 | 18.51 → 19.50 |
| 24–48h | 5,553 → 5,907 | 1,794 / 350 → 1,150 / 611 | 27.34 → 8.99 | 20.60 → 19.60 |
| 48–96h | 10,940 → 11,655 | 3,050 / 449 → 1,938 / 994 | 25.93 → 6.41 | 22.67 → 19.52 |

### Full-horizon outcomes and progression

| Measure | BAL-1S baseline | BAL-2 |
|---|---:|---:|
| Settled raids | 22,508 | 23,777 |
| KO rate, Low / Medium / High / all | 0.2% / 7.7% / 16.3% / 6.7% | 0.1% / 13.5% / 34.4% / 10.7% |
| Medical used/raid, Low / Medium / High / all | 0.06 / 1.54 / 2.17 / 1.12 | 0.12 / 1.58 / 2.45 / 1.09 |
| All-loot value secured/raid, Low / Medium / High / all | 131.64 / 112.00 / 83.19 / 113.16 | 99.16 / 97.50 / 59.47 / 93.01 |
| Weapon inflow / KO outflow | 6,813 / 1,514 | 4,727 / 2,556 |
| Net weapon change | +5,299 | +2,171 |
| Shield spend/settled raid | 20.67 | 18.95 |
| Closing coins / stash / supplies per Raider | 377.09 / 119.45 / 103.23 | 221.29 / 160.23 / 53.66 |
| Closing usable / broken weapons per Raider | 42.34 / 0.03 | 17.80 / 0.16 |
| Closing shields, none / Light / Medium / Heavy | 4 / 2 / 60 / 62 | 14 / 19 / 76 / 19 |
| Raiders reaching Medium+; Heavy shield | 128/128; 116/128 | 128/128; 99/128 |
| Median time to Medium+; Heavy | 4.37h; 37.41h | 5.46h; 36.48h |

Both cohorts still show positive net weapon accumulation; BAL-2 does not cap,
liquidate or guarantee bounded owned stock. The end-of-run mean asset values
were 4,677.23 baseline and 1,824.07 BAL-2; these include owned catalog-valued
assets and are experimental cohort diagnostics.

## Findings and limitations

- BAL-2 makes Medium/High robots materially harder to defeat and increases
  danger-tier KO rates, especially in early windows. High KO is 54.0% in the
  first 8h and 43.0% in 8–24h, above the experimental 25–40% BAL-4 band. The
  96h High aggregate is 34.4%, while Medium falls to 13.5% against its
  experimental 15–25% band. These departures are recorded, not retuned.
- Medical use rises for High overall (2.17 → 2.45 per settled raid) but not
  monotonically in every short window. Initial High samples are small and
  danger counts differ; the results do not prove a particular loadout is
  required.
- Replacement rewards generate measurable value, but all-loot value secured
  per raid is lower in the BAL-2 cohort. Reward value is not a balance target
  and does not include hypothetical stacked Depot draws.
- Weapon stock grows in both cohorts despite lower BAL-2 net growth. BAL-4
  remains responsible for any cap/liquidation decision. Shield acquisition
  shifts away from Heavy at 96h; BAL-1S rules and prices were not changed.
- A deterministic BAL-2 test also confirms a High robot can be defeated with
  no healing supplies. This is a legality check, not an estimated win rate or
  a requirement to win without a working shield.
- The fresh solo Low starter cohort was 64/64 fights, 58 defeats and 0% KO in
  both rulesets. It does not validate High balance.
- Telemetry is in-memory simulation evidence, not live-population testing.
  There was no browser multiplayer session or user-managed backend restart.
