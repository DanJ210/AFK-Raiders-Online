# BAL-2 danger robot tiers and defeat loot proposal

Date: 2026-11-01. **Approved bounded implementation; technical verification complete 2026-11-01. Balance remains experimental.**

The user approved this revised proposal on 2026-11-01. This records scope
authorization, not balance certification. BAL-1 and BAL-1S remain frozen
technical slices with experimental defaults. BAL-3 activities and BAL-4 tuning
are not included.

**Review location (2026-11-01):** the actual proposal was found at
`C:\Users\DanJ2\source\repos\AFK-Raiders-Online\docs\balance\BAL-2-proposal.md`,
as an **untracked file** in the sole registered worktree
`C:\Users\DanJ2\source\repos\AFK-Raiders-Online`, branch `main`, HEAD
`d522970bb53d5e74a9717ad3334cf2974a316cbf`. Related planning edits are
uncommitted there; no separate BAL-2 branch or committed proposal was found.
The approval applies to the bounded decisions below; it does not authorize
balance retuning or scope expansion.

## 1. Settled contract and decision boundaries

- Keep BAL-1 weapon snapshots, seeded integer damage and once-per-successful-raid
  weapon wear; no per-hit wear. Keep LOSS-1 equipped-weapon loss on KO.
- Freeze BAL-1S charge, mitigation, wear, recharge, repairs, prices, initial Light,
  trait thresholds, reserves and upgrade-before-repair policy. No shield retuning.
- Temptrest remains trader-only, including on High. No new weapons, shields,
  consumable rewards, direct PvP/theft, Handler equipment controls or AI decisions.
- One authoritative clock, shared robot health and finite reward inventory per
  world. Only actual participants contribute. Unique ownership and once-only
  generation, allocation, loss, settlement and economic transactions are required.
  Reading projections, retries and Handler absence must not alter outcomes.
- The earlier solo-High wording is a **balance aspiration**, not a runtime
  guarantee: lesser-loadout wins remain legal, with no equipment gate or
  nonlethal floor.

**Approved experimental choices:** danger profiles and replacement bundle in
sections 3-4; immediate contribution-ranked allocation and current pocket
exception in section 5; **preserve current weapon inventory rules and defer
accumulation tuning to BAL-4** in section 6. The original four-owned-weapon cap
is retained as an unapproved future alternative, not the implementation
default. Approval does not validate balance; the paired measurements are
available in the [BAL-2 measurement report](BAL-2-measurements.md) and remain
experimental diagnostics.

## 2. Evidence and source map

Online source inspected at `d522970bb53d5e74a9717ad3334cf2974a316cbf`
(clean worktree before this documentation task). Relative code links below refer
to that inspected checkout; pin the approved implementation baseline later.

| Evidence | Finding and consequence |
|---|---|
| [BAL-1 measurements](BAL-1-measurements.md), rules `desperanza-bal-1-v1` | 24 seeds, two unattended Raiders, 8h, 785 settlements. Low/Medium/High KO 0/25.7/43.3%; robot defeats 235/258, 149/276, 22/55 participant-fights. Do not compare this smaller cohort directly as a paired BAL-1S control. |
| [BAL-1S production measurements](BAL-1S-measurements.md) | 64 seeds, two unattended Raiders, 96h, 22,508 settlements; n Low/Medium/High 8,367/9,345/4,796. KO 0.2/7.7/16.3%; robot HP lost per raid 2.95/6.03/6.08 versus hazard 1.90/28.42/46.21. Medical used 0.06/1.54/2.17, carried 2.42/2.42/3.06. High risk is dominated by hazards, not current robot hits. |
| Same production report | Robot defeats 7,996/8,074 (99.0%), 7,347/8,281 (88.7%), 2,467/4,043 (61.0%). Shared participant-fights 78/112/38: sparse cooperation, not evidence of public-population fairness. Secured gross value/raid 131.64/112.00/83.19; these are not net values. |
| Same production report | Found weapons 6,290, bought 259, loaners 264 versus 1,514 equipped weapons lost: 6,813 in / 1,514 out. Low alone finds 3,108 versus 14 lost. These are secured found weapons, not all field drops; broken weapons are not counted as destroyed. Stock is not bounded by wear or KO alone. |
| [BAL-1S gate note](BAL-1S-post-BAL-1-gate.md), production section | Same 64-seed harness: 8h KO 0.1/25.3/38.9% (n 720/1,253/216); 96h 0.2/7.7/16.3%. The production early/late feedback differs markedly. Lab 48-96h Medium/High KO 4.04/14.17% is **lab**, not production window evidence. High 96h is below the experimental BAL-4 25-40% band; do not fix it by changing BAL-1S. |
| [RaidConditions.Profile](../../server/AFKRaiders.Server/Domain/RaidConditions.cs) | Current Low/Medium/High HP 12/19/25 and raw damage 8/10/10. Hazards, extraction eligibility 900/960/1020s and 1320s deadline are outside this proposal's changes. |
| [ResolveRobotRound / FinishRobotEncounter](../../server/AFKRaiders.Server/Domain/RaidSimulation.cs), lines 583-735 | Three rounds, five seconds apart. Slot-ordered, capped actual damage; all outgoing damage precedes retaliation. Defeat has no last retaliation and starts Depot searches; escape resolves/burns unclaimed Depot loot. Historical damage includes fighters removed on Downed; history alone is not reward eligibility. |
| [ResolveSearch / Distribute](../../server/AFKRaiders.Server/Domain/RaidSimulation.cs), lines 738-797 and 937-957; [LootCatalog](../../server/AFKRaiders.Server/Domain/LootCatalog.cs), lines 356-405 | Current Depot reward is a later two-draw opportunity. Generic allocation sorts backpack value then slot and round-robins descending-value items, **not contribution**. Nonweapon quality is danger/exposure/appetite-biased; weapon probability is preserved by scaling. Danger already affects nonweapon selection, but there is no robot-specific tier table. |
| [Loot catalog](../../server/AFKRaiders.Server/Content/loot-catalog.json) | Three Depot variants, each two draws; Market and Salvage Yard each retain two draws. Current field weapons have catalog values 65/140 and loot rarity 3/4 (different from equipment rarity 2/3). Maximum existing nonweapon loot rarity is 3. |
| [Terminal simulation / extraction](../../server/AFKRaiders.Server/Domain/RaidSimulation.cs), lines 392-445 and 1046-1063; [coordinator settlement](../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs), lines 408-475 | KO loses backpack and carried supplies except the already designated single pocket item; that item enters the secured haul even on KO. Successful extraction secures the backpack. Equipped weapons/shields are lost on KO; unused staged supplies return only on extraction. Settlement deduplicates by raid per owner. |
| [FoundWeaponSettlement](../../server/AFKRaiders.Server/Domain/FoundWeaponSettlement.cs); [HomeStash](../../server/AFKRaiders.Server/Domain/HomeStash.cs) | Secured field weapons become owned equipment at seeded 5-60% durability, not stash items. No owned-weapon cap. No field-backpack capacity enforcement in current allocation. Ordinary home stash capacity is 120 units: lowest value, then ordinal instance ID is sold at catalog value on overflow. |
| [BalanceMeasurementTests / BalanceReport](../../tests/AFKRaiders.Server.Tests/BalanceMeasurementTests.cs); [RaiderStatistics](../../server/AFKRaiders.Server/Domain/RaiderStatistics.cs) | Five-second scheduling, pinned IDs/trader seeds, optional 24-seed/8h defaults and 64-seed/96h runs. Current report aggregates horizons; economy spend is all dangers. Robot counts are per settled participant, not distinct world robots. Windows, per-danger net value, all-source weapon sinks and round counts need focused measurement extensions during implementation. |

### Read-only legacy intake and legal reconciliation

Inspected legacy revision:
`f62d088a41da951a1fbe9ed7215d2957a933ce50`, with no local modifications to the
inspected robots/loot tables. Sources:

- [robots.json](https://github.com/DanJ210/afk-raiders/blob/f62d088a41da951a1fbe9ed7215d2957a933ce50/src/content/robots.json):
  current Low/Medium/High identities have weak/moderate/deadly roles;
  nested robot parts mix common weighted drops and rare value spikes. For example,
  their main part values are 5/15/300, with weights 85. This is role/economic
  evidence, not an online stat formula, approved distribution or copied prose.
- [arc_tech.json](https://github.com/DanJ210/afk-raiders/blob/f62d088a41da951a1fbe9ed7215d2957a933ce50/src/content/loot-tables/arc_tech.json),
  lines 4-56: intact data shard rarity 3/value 60, overcharged core rarity 4/value
  150, prime-archive shard rarity 5/value 900. Only these narrowly selected
  definitions are candidates below; no whole-table import.
- [weapons_parts.json](https://github.com/DanJ210/afk-raiders/blob/f62d088a41da951a1fbe9ed7215d2957a933ce50/src/content/loot-tables/weapons_parts.json),
  lines 275-307: frames have rarity/value tiers 2/25 through 5/2000.
  Frames are trade loot, not usable guns. Prefer tech instead of adding crafting
  or converting frame references into equipment.

Follow the [lore index](../lore/README.md),
[mapping](../lore/PARODY_MAPPING.md), [robots](../lore/entities/robots.md),
[legal guardrails](../lore/content-guidelines/LEGAL_GUARDRAILS.md) and
[tone guide](../lore/content-guidelines/TONE_VOICE.md).
Retain current online robot identities; do not author new names/flavor here.
Source IDs below are lookup references, not approval to copy external runtime
names or prose. No legacy ticker, lethal-floor rule, greed gate, saves, stack
sizes or loot-event bonus consumables are imported.

**Conflict:** lore describes weak/moderate robots as nonlethal; current online
damage can down any wounded Raider. Recommend preserving current damage
semantics and Low starter viability through measurements, not adding a hidden
nonlethal floor. The user explicitly approved this interpretation on
2026-11-01; see section 8 and the [online lore clarification](../lore/entities/robots.md#online-implementation-clarification-2026-11-01).
The [game design](../GAME_DESIGN.md) and [brief](../MULTIPLAYER_REBUILD_BRIEF.md)
retain wider aspirations; the ledger and this scoped contract govern this slice.
No runtime content was imported now; any later intake must record the actual
revision/paths and approved transformations in README provenance.

## 3. Recommended experimental robot profiles

| Danger | Current HP / raw hit / rounds | Proposed HP / raw hit / rounds | Interval | Role |
|---|---:|---:|---:|---|
| Low | 12 / 8 / 3 | **12 / 8 / 3** | 5s | Leave starter combat untouched; change only reward backing. |
| Medium | 19 / 10 / 3 | **22 / 12 / 3** | 5s | Partial upgrade helps shared damage; stronger owned weapons improve solo completion. |
| High | 25 / 10 / 3 | **45 / 18 / 4** | 5s | Best trader weapon or actual cooperation; longer exposure and scarce healing/charge matter. |

Keep current outgoing-before-retaliation order, capped damage records, no attack
on a defeated robot, no final retaliation on the killing round, no added patrol,
and no extension of world lifetime. The round limit belongs to the danger profile
in the normal risk/reward mode, not a global change to legacy fast-pacing fixtures.

BAL-1 rounded damage ranges remain Kettle 3-6, Aspperigo 4-8, Vernerider 6-11,
Temptrest 9-15. Thus in three rounds Kettle reaches at most 18, Aspperigo 24,
Vernerider 33, Temptrest 45; High's fourth round allows Temptrest 36-60, while
Vernerider still cannot exceed 44. Two actual fighters combine damage; they do
not each get a fresh robot or a fresh round budget.

Damage-only enumeration of independent, equiprobable base rolls gives Low Kettle
54/64 = **84.38%** completion and proposed Medium Aspperigo 4/64 = **6.25%**.
High Temptrest has 501/625 = **80.16%** completion in four rounds.
These are arithmetic plausibility checks, **not seeded simulation survival
measurements**: hazards, arrival HP, mood, supplies, shared timing and withdrawal
are excluded. Medium Aspperigo solo success is deliberately uncommon; if an
intermediate gun is expected to clear Medium reliably, 22 HP needs reconsideration
before approval, not BAL-1 damage retuning.

Successful solo High needs at least three rounds: two or three retaliations,
36-54 raw damage. A surviving escape can receive four hits, 72 raw. At full
Heavy charge and zero mood trim, 54 raw costs about 28 HP and 21.6 durability
points, versus 72 costing about 37 HP and 28.8 points. Earlier hazards or a weak
shield can make this much worse. High loot helps fund subsequent preparation,
but cannot heal this fight because it is trade/equipment cargo only.

### Solo High: approved balance aspiration; strict enforcement not selected

| Choice | Contract | Consequence / approval needed |
|---|---|---|
| **Approved balance aspiration** | Stronger healing and a shield that holds should improve sustainable unattended solo High survival and net value; favorable lesser-loadout wins remain legal. | Approved 2026-11-01. Compare matched starting HP/mood/weapon/shield cohorts by medical quantity/quality, plus recurring windows. Do not promise best healing is always necessary, or infer a gate from measurements. |
| **Strict enforcement (not selected)** | Solo High defeat must be impossible unless the Raider meets a precisely defined best-healing condition and shield condition. | Not authorized. It would require separate design approval defining healing, shield state and solo status, and would change autonomy or encounter mechanics. The proposed profiles do not establish this contract. |

A fresh Heavy shield can leave a healthy Raider alive after the two or three
retaliations of a successful High fight even without medical. Sampling cannot
prove a universal impossibility. BAL-2 therefore adds no equipment gate, health
floor, reward denial, or BAL-1/BAL-1S retuning. A balance aspiration is the
approved interpretation; it remains a measured goal, not a guarantee.

## 4. Defeat bundle: replace Depot loot, do not supplement it

Recommend replacing the selected Service Depot opportunity's **two generic
draws** with exactly **two world-scoped defeat cargo slots**. Market and Salvage
Yard remain unchanged. Do not also award a robot bonus, coins, supplies, XP bonus
or the old Depot table. Six total possible cargo items per world remain six,
not six per Raider. No participants means no allocation; escape/unresolved
robot means zero defeat cargo and the Depot remains exhausted.

Creation and allocation occur atomically at committed defeat, not at a subsequent
10s search completion. Mark the selected Depot opportunity resolved and skip its
reward search, retaining the existing 2s Packing transition. This intentionally
removes that 10s search/activity and its +2 greed/+1 mood completion effect,
not combat effects; record this timing/personality change in comparisons and
narration compatibility. Do not narrate a locker search or corpse items before
the authoritative event. Retaining a cosmetic 10s task is an alternative requiring
approval, not the recommended default or a BAL-3 activity.

Each nonweapon draw uses the following fixed weights (each column totals 100);
draw with replacement, giving distinct instances even when definitions repeat.
No exposure/appetite reweighting for this bundle. Existing generic opportunity
weights outside Depot are unchanged.

| Cargo definition reference | Value / loot rarity | Low weight | Medium weight | High weight |
|---|---:|---:|---:|---:|
| Existing `energy_core_spent` | 10 / 1 | 70 | 0 | 0 |
| Existing `robot_toe` | 15 / 2 | 20 | 0 | 0 |
| Existing `robot_knuckle_joint` | 20 / 2 | 10 | 0 | 0 |
| Existing `circuit_bundle_messy` | 35 / 3 | 0 | 50 | 30 |
| Existing `arc_data_shard_corrupted` | 25 / 2 | 0 | 30 | 0 |
| Legacy lookup `arc_data_shard` (new selective intake) | 60 / 3 | 0 | 20 | 50 |
| Legacy lookup `energy_core_overcharged` (new selective intake) | 150 / 4 | 0 | 0 | 18 |
| Legacy lookup `arc_data_shard_legendary` (new selective intake) | 900 / 5 | 0 | 0 | 2 |

Slot A always draws nonweapon cargo. Slot B first rolls the following weapon
replacement; on failure it draws nonweapon cargo from the same danger column.

| Danger | Slot B weapon probability | Conditional weapon selection | Max weapons / defeated world | Expected gross bundle value |
|---|---:|---|---:|---:|
| Low | 2% | 100% existing Aspperigo (65) | 1 | 25.06 |
| Medium | 6% | 75% Aspperigo (65), 25% Vernerider (140) | 1 | 76.805 |
| High | 12% | 100% Vernerider (140) | 1 | 177.54 |

Nonweapon draw means are 12/37/85.5; bundle expectation is
`mean + (1 - weaponChance) * mean + weaponChance * weaponMean`.
Values are per **defeated world before losses**, not per participant or settled
raid. No guaranteed rare reward and no Temptrest roll. High's 2% rarity-5 tail
can accelerate wealth/shield progression; inspect median/p90/p99 and a
leave-jackpot-out diagnostic, not just the mean. Rejecting the 900-value tail is
an open content/table choice; do not change catalog prices implicitly.

Low's cheaper bundle may slow starter purchases relative to generic Depot loot.
That is the cost of reducing Low gun farming without adding more damage.
Medium/High grow reward quality, not item count; High pays only when both the
robot and the owner's extraction problem are solved (subject to the pocket).

## 5. Participants, allocation, ownership and terminal outcomes

**Recommended eligibility snapshot at defeat:** actual Raider in this raid,
at the robot location, still `FightingRobot`, not Downed/KO/extracted or committed
to extraction, and recorded positive capped damage against this robot.
Joining with zero damage, observing nearby, arriving after defeat or historical
participation followed by withdrawal does not qualify. Removed fighters retain
combat/stat history but no claim. Capture this snapshot before
`FinishRobotEncounter` resets fighters to `Ready`, not from their later search
or Packing state. Zero eligible recipients terminally exhausts the bundle without
creating cargo; do not reserve rewards for a future arrival.

Rank eligible recipients by cumulative **actual damage dealt descending**,
then `JoinedAt` ascending, then stable ordinal Slot, then ordinal PlayerId.
Keep BAL-1's slot-ordered damage/overkill attribution; do not claim it is unbiased
for equal shots. This deliberately replaces the generic backpack-value ranking
for robot rewards only. Sort the two generated items by value descending then
ordinal InstanceId; distribute round-robin over that fixed ranking. With two
fighters each receives one item; the higher contributor gets the more valuable
item, not both. A sole eligible fighter receives both. No last-hit bonus,
proportional item splitting, Handler claim command or theft.

Seed generation by world seed plus unambiguous raid/robot/slot/purpose keys;
weapon-gate, weapon-selection and cargo-selection rolls need distinct keys.
Version the approved rules/content. Instance identity includes raid, robot and
cargo slot, not owner, poll time or attempt number. A serialized guarded
`Defeated -> reward resolved` transition creates both instances, assigns each
once via the existing world-item owner/backpack path, resolves Depot, and emits
the shared outcome plus owner-private item receipts. Repeated callbacks,
advance calls, late joins and reads must never regenerate or reallocate.
Invalid tables/ownership conflicts must fail explicitly, not give default loot.

Eligibility is local to the **robot-defeat transition**, not a judgment about
which Handler attended. Current slot-ordered capped damage can give a later
fighter zero credit on the only round they attend; exclude them under this
proposal even if they rolled a shot. Approve this contribution bias explicitly;
do not change BAL-1 attribution to make allocation appear fairer.

| Outcome | Reward consequence |
|---|---|
| Robot escapes, no fighters, world deadline before defeat | No defeat items, currency, consolation or later Depot reward. Existing owned cargo is not erased merely because the robot escaped. |
| Robot defeated | Eligible owners receive risky backpack cargo immediately; **no home items or coins yet**. |
| Owner escapes the fight/commits to extraction before defeat | No new robot claim; already owned cargo follows normal extraction/KO rules. |
| Owner successfully extracts, including distress extraction | Their allocated cargo is secured once; nonweapons enter stash/overflow, weapons pass existing found-weapon conversion. The other fighter's outcome cannot duplicate or revoke it. |
| Owner Downed after allocation, then KO | All allocated cargo lost **except one already designated Secret Hidden Pocket item**, if any; no automatic BAL-2 pocket choice. The protected item is secured through existing KO settlement, including a field weapon. All other rewards require successful extraction. |
| Owner rescued/revived before KO | Keep allocated ownership; no second award. Later extraction/KO determines retention. |
| Retried/concurrent settlement | Same settlement ID, no duplicate cargo, equipment, pocket salvage, overflow coins, wear, loss or statistics. |

Precise extraction requirement: **every defeat nonweapon and field weapon
requires successful extraction to become a home asset, except the one pre-KO
designated pocket instance**. There is no immediate bounty/payment.
Overflow sale proceeds require the same securing event; sales never happen in
the field. Existing equipped-weapon/shield loss and staged-supply return rules
are unchanged. A disconnected Handler gains no extra disadvantage or automatic
reward; polled and absent runs with identical intents must match.

### Nonlethal lore versus robot defeat and Raider defeat

These are separate outcomes: `RobotDefeated` means shared robot HP reached zero;
Raider `Downed`/`KnockedOut` is the survival/settlement lifecycle. Defeating the
robot generates risky cargo, not healing, revival, immediate coins or immunity.
Raider KO never creates robot rewards. A withdrawn or Downed contributor cannot
claim a later robot defeat; an already allocated item may survive KO only through
the existing designated pocket exception.

- **Approved:** preserve current damage/Downed/KO semantics in every danger;
  document lower-tier nonlethal language as legacy, not an online guarantee.
  Low starter viability remains a statistical measure, not a health floor.
  Update the lore's online clarification without removing its legacy explanation.
- **Alternative, not selected:** enforce a Low/Medium robot-only nonlethal
  floor. Define whether it prevents Downed or only terminal KO and how later
  hazards, shield charge/wear, healing and robot escape interact. No floor for
  hazards and no consolation loot may be assumed. This changes survival semantics
  and needs a separately scoped contract before using the implementation prompt.

## 6. Inventory overflow and bounded weapon accumulation

Finite robot drops alone cannot bound total equipment: unchanged Market/Salvage
and trader inflow persists. Lowering robot weapon odds is a rate limit, not a
long-run stock guarantee; worn/broken home guns are not a sink.

### Approved path: preserve inventory, defer tuning to BAL-4

Keep all secured unique field weapon instances through current 5-60% durability
conversion, including the one pocket-secured weapon on KO. No weapon-cap
normalization, automatic weapon sales, discard, field overflow or new equipment
controls. Home weapons, including broken guns, remain safe on KO; only the
equipped deployed gun is lost. Ordinary cargo still enters the 120-unit stash
with lowest-value/ordinal-ID overflow sales. Weapons do not consume stash units.
Current autonomous repair/equip/trader/fallback behavior remains unchanged.

BAL-2 limits **robot weapon creation** to one possible gun per defeated world,
with 2/6/12% slot probability; it does **not** guarantee bounded total home stock.
The approved scope defers global accumulation/sinks and their prices to BAL-4,
recording the original four-cap proposal and the measured inflow/outflow problem
there. The approved deferral does not satisfy a bounded-stock guarantee. Measure
stock growth and shield progression now so BAL-4 has evidence rather than
assuming weapon wear destroys home inventory.

### Alternative: four-owned-weapon cap (unapproved, not default)

If a hard cap is required now, approve this broader economy change separately:

1. **Count:** distinct authoritative `OwnedWeapon.InstanceId` entries, all
   sources, duplicates by catalog still separate, equipped and unequipped,
   full/damaged/broken, starter/loaner/purchased/secured field/pocket. The equipped
   reference is not an extra item. Unsecured backpack guns, ordinary weapon
   parts, trader stock, supplies and shields do not count. A repeated instance
   is deduplicated, not a second slot or sale.
2. **Protect and rank:** apply normal extraction wear or KO equipped loss first.
   Preserve the remaining equipped instance even if broken; the cap cannot
   circumvent LOSS-1 by protecting a gun already lost. Keep a newly needed usable
   fallback as a second protected instance until normal preparation equips it.
   Rank remaining candidates usable first, then expected BAL-1 rounded-roll
   damage descending, current/max durability ratio descending (exact ratio
   comparison), then ordinal CatalogId/InstanceId. Fill remaining slots to four;
   never sell equipped gear or auto-equip in settlement.
3. **Overflow:** combine all current and incoming instances only after cargo
   is secured. Retire every nonretained instance exactly once at Desperanza,
   at authoritative shop catalog value (field guns 65/140, Kettle zero), with
   owner-private source/instance/value/removal receipts. Broken guns get the
   same catalog price in this alternative: that is a new resale policy, not
   inherited stash behavior. No field sale, pocket bypass, dual stash/equipment
   ownership or currency for lost/unsecured cargo. Existing home surplus can
   be retired even after KO; those proceeds are not rewards for losing.
4. **Timing and all-source enforcement:** normalize an already-over-cap home
   inventory once before the next preparation plan; never mutate an active
   deployed inventory merely on reads. Enforce at field/pocket conversion and
   each trader/fallback addition atomically, not only at final settlement.
   An in-flight errand plan must not sell/repair/equip an instance the cap retires;
   approve explicit plan reconciliation and receipts before implementation.
   Check purchase affordability/reserves against committed pre-purchase cash,
   allocate finite stock once, then addition/retirement as one transaction.
   Do not pre-spend expected surplus proceeds. These are new coordinator and
   economy-policy surfaces, not a small loot-catalog adjustment.
5. **Account:** retain gross secured haul receipts independent of final retained
   equipment. Show secured, retained, retired and sale coins separately; closing
   stock equations include retirement. Assert <=4 at each completed addition
   transaction after normalization. Do not count becoming broken as destruction.

### Wealth and shield feedback

The cap reduces stock but makes previously illiquid safely stored guns liquid.
At saturation, selling just the newly secured robot weapon yields an expected
1.30/5.025/16.80 coins per Low/Medium/High defeated world before securing losses
(weapon probability times conditional value); retiring older guns can yield more.
These are hypothetical cash flows, not measured net gains. Full-value conversion
does not create marked catalog asset value from nothing, but bypasses repair
cost/durability discounts and makes coins available for supplies and shields.
Large existing surplus also creates a one-off liquidation spike.

Settlement liquidation precedes preparation and trader liquidation can precede
`DecideShield`; unchanged BAL-1S policy can therefore buy higher shields or
repair sooner. Coins freed by cap sales may reduce later KO and increase future
secured loot. Shield constants staying frozen does not freeze shield progression.
Compare shield tier by window, time-to-first Medium/Heavy, sale cash, repair spend,
KO and net assets with/without cap at identical seeds. Current inventory deferral
avoids this **additional** liquidity change, but new High tech sale income still
affects progression and must be measured.

Discard instead of sale bounds stock without the same cash injection but destroys
secured value and changes reward retention; discounts add tuning. Neither is an
approved substitute. The approved inventory deferral isolates BAL-2
combat/rewards; retain cap/sale/discard as BAL-4 decision context, not hidden code.

## 7. Focused implementation verification and measurement record

### Deterministic correctness gates (verified)

The focused BAL-2/combat/narration suite passed 41/41 before the final
backpack-retention regression, which passed 1/1; the final full server suite
passed 472/472 with `DOTNET_PROCESSOR_COUNT=2`. Two earlier default-parallel
full runs hit the existing timing-sensitive CommsAndPacing cancellation test;
it passed in isolation 1/1 and in the final full run. The focused coverage
includes reward replacement without
stacked Depot draws, active actual-participant eligibility, capped damage and
deterministic ties/allocation, appending rewards without displacing existing
backpack loot, stash overflow, escape, extraction/KO/pocket ownership, retry
idempotency, and absent-versus-polled telemetry parity. The
web Node tests passed 24/24 and the production build passed. No live backend
was restarted. The paired results and evidence boundaries are in the
[measurement report](BAL-2-measurements.md).

1. Profile HP/damage/rounds, fixed weights, sums, references, weapon mappings,
   unique IDs, roll boundaries, versioning and invalid-content behavior are
   covered. Temptrest remains absent from field tables.
2. Combat coverage verifies shared HP/round limits, capped contribution,
   deterministic ties/replay and no retaliation on the killing round. Existing
   weapon tests cover seeded outcomes; fast-pacing fixtures retain their legacy
   profile semantics.
3. BAL-2 tests verify exactly two rewards on eligible defeat, no stacked Depot
   search reward/personality gain, no rewards on escape or no eligible fighter,
   and unchanged non-Depot behavior. Actual positive-damage participants,
   withdrawal, capped damage, contribution ranking and deterministic allocation
   are covered.
4. Ownership tests cover independent extraction, post-award KO, designated
   pocket survival, unprotected cargo loss, extracted-weapon conversion and
   repeated advances without duplicate allocation. No lost owner's cargo is
   reassigned.
5. Tests cover appending defeat cargo to existing field loot and a full home
   stash's normal overflow rules, plus preservation of secured weapons under the
   approved no-cap/no-liquidation contract. The field backpack has no capacity
   limit; no field-backpack overflow rule was added. Existing equipment, finite
   stock, reserves and errands remain unchanged.
6. No-Signal absent-versus-polled stats and per-raid telemetry are compared;
   owner-private receipts and authored event truth remain subject to the focused
   reward/narration tests. Optional AI narration cannot create or delay rewards.

Use existing [weapon](../../tests/AFKRaiders.Server.Tests/WeaponCombatTests.cs),
[shield](../../tests/AFKRaiders.Server.Tests/ShieldTests.cs),
[catalog](../../tests/AFKRaiders.Server.Tests/CatalogContentTests.cs),
[settlement](../../tests/AFKRaiders.Server.Tests/FoundWeaponSettlementTests.cs),
[stash](../../tests/AFKRaiders.Server.Tests/HomeStashTests.cs),
[economy](../../tests/AFKRaiders.Server.Tests/RaiderEconomyTests.cs),
[survival](../../tests/AFKRaiders.Server.Tests/SurvivalMechanicsTests.cs),
[G6](../../tests/AFKRaiders.Server.Tests/G6RiskRewardTests.cs),
[coordinator](../../tests/AFKRaiders.Server.Tests/RaidCoordinatorTests.cs) and
[measurement](../../tests/AFKRaiders.Server.Tests/BalanceMeasurementTests.cs)
coverage; extend the smallest relevant surfaces. The focused/full server tests
and web tests/build passed as recorded above. Independent two-session browser
verification from the original plan was not run; this is a validation
limitation, not a gameplay or balance result. No live backend restart occurred.

### Paired before/after evidence (completed; diagnostic only)

The pinned BAL-1S baseline and approved BAL-2 were run with the same seed IDs
1-64, two unattended Raiders, fixed identities/trader seed, five-second
scheduling, and no Handler Signals, as separate 8h and 96h cohorts. The
baseline source is `d522970bb53d5e74a9717ad3334cf2974a316cbf` with telemetry-only
instrumentation. BAL-2 was measured from the uncommitted implementation
worktree based on that same HEAD, under `desperanza-bal-2-v1`; no commit was
created. The four final raw per-raid JSON files and reproducible aggregate
reports are retained as session artifacts; the repository comparison is
[BAL-2-measurements.md](BAL-2-measurements.md). Historical BAL-1/BAL-1S reports
were not overwritten and lab data was not substituted.

The comparison uses half-open **settlement-time** windows [0,8), [8,24),
[24,48), [48,96)h and separate standalone 8h cohorts. It uses true terminal
timestamps and per-raid telemetry, not poll-observation time. The standalone
8h aggregate includes one exact-boundary settlement while its `[0,8)` window
excludes it; the report calls out this distinction.

The following was the approved diagnostic checklist, not a claim that every
desired cut was collected. The report contains the requested KO/time windows,
robot outcomes, healing, reward value, weapon flows, economy and shield
progression. It does **not** include trait-stratified tables, seed-cluster
uncertainty, or independent browser-session verification; those remain
limitations rather than implicit passes.

| Metric by danger, window and trait | Approved diagnostic comparison and criterion |
|---|---|
| n, extraction/KO rate, cause, time-to-KO and raid duration | Show absolute/percentage-point delta and seed-cluster uncertainty, not rounded aggregate subtraction. Experimental BAL-4 bands remain Low <=10%, Medium 15-25%, High 25-40%. These are diagnostic bands, not automatic retuning authority. Separately use a fresh solo Low Kettle+initial-Light, no-Signal cohort to test <=10% KO; mature Low alone is not starter evidence. |
| Robot distinct worlds and participant-fights, defeated/escaped/unresolved, rounds, solo/shared by weapon and shield | Both denominators; count one shared robot once in world totals. Verify all-world cap two items/one weapon; stronger weapons must improve damage-only completion at equal starts. Measure staged High best-prepared versus lesser healing and weak/broken shields, without asserting a strict prohibition from samples. |
| HP lost robot/hazard, medical carried/used, revives, actual HP restored, wasted healing, rechargers, shield wear/repair spend | Compare against BAL-1S 0.06/1.54/2.17 medical use and HP-source table; use 0-1/1-2/2-4 as experimental medical bands. More item consumption with mostly overheal is not meaningful added risk. |
| Weapon generated/allocated/lost/secured by source, bought/loaner, retired/sold, newly broken and owned usable/broken counts | Close `end stock - start stock = secured + bought + loaner - KO loss - retired` for each owner. Broken is a state transition, not destruction. Assert <=4 at every committed addition if cap approved; report 48-96h stock/turnover slopes, inflow/outflow per 100 raids and coin feedback. |
| Gross/lost cargo, robot-only secured value, coins, stash, equipment/supply stock, preparation cost | Compute per-raid net by danger/window including failures and zero-reward escapes. Record medians/p90/p99 as well as means and seeds; High mean net > Medium > Low is an experimental BAL-4 objective, not a current finding. |

Define net **economic asset change** as `(coins + ordinary stash catalog value +
owned weapons at catalog value + owned supplies at unit cost + owned shield at
catalog price) after settlement - before its admission preparation`. Report
weapon/shield durability separately (catalog marking is not realizable damaged
sale value or a damage-adjusted wealth estimate). Purchases/sales move value
between buckets, repairs/consumption/KO reduce it; avoid adding gross secured
value or sale proceeds a second time. Also report liquid net coins and
preparation spend separately. For the existing equipped shield lost on KO,
subtract its catalog value once. The measurement report includes telemetry net
asset change by danger and window, but does not establish a validated net-value
ordering or confidence bounds.

Optional isolated ablations (same seeds, no production flags): rewards with
current robots, profiles with current rewards, then full recommended proposal
with unchanged inventory. A separate cap-versus-no-cap experiment is useful only
if explicitly authorized; it is not required gameplay implementation for BAL-2.
These distinguish combat, removed Depot delay/personality and wealth feedback.
Never promote a
measurement-only variant as runtime defaults. If uncertainty overlaps or High
means depend on a few jackpots, state inconclusive evidence and carry the
specific shortfall to BAL-4; do not claim balanced outcomes.

## 8. Approved decisions and implementation status

### User-approved decision table (2026-11-01)

| Approved decision | Rationale | Runtime contract | Status |
|---|---|---|---|
| Experimental profiles 12/8/3, 22/12/3, 45/18/4 at 5s | Rebalance danger without changing BAL-1 damage or BAL-1S | Medium Aspperigo and High lesser-loadout wins remain legal; observed rates are diagnostics, not gates | Approved |
| Replace Depot draws with two shared defeat rewards | Fixed world total; no stacked loot | Atomic defeat-time allocation, no Depot 10s search or its +2 greed/+1 mood, at most one field gun/world | Approved |
| Contribution-ranked round-robin | Rewards actual capped contribution while distributing two slots | Positive capped damage, active actual participants; deterministic join/slot/ID ties; no zero-damage, withdrawn, Downed, KO, extracted, or late claim | Approved |
| Keep extraction and designated pocket exception | Matches existing secured-haul and exactly-once settlement rules | New cargo follows current ownership/loss rules; no bounty or auto-pocket selection | Approved |
| Preserve weapon inventory; defer caps/liquidation to BAL-4 | Avoids unmeasured liquidation and shield feedback | No global stock guarantee; all secured unique guns retained; ordinary stash overflow is unchanged | Approved deferral |
| Solo High is a balance aspiration; retain current KO semantics | Keeps valid autonomy and existing survival rules | Lesser-healing wins remain legal; no equipment gate or robot nonlethal floor | Approved |
| Paired cohorts/windows and diagnostics | Separates hard invariants from experimental balance bands | Report departures and limitations without automatic retuning or milestone completion | Approved |

### Completion status and limitations

The bounded implementation and requested automated validation are complete;
the approved values are not validated balance. See the [measurement
report](BAL-2-measurements.md) for experimental departures and limitations.
The strict best-healing gate, Low/Medium health floor, and four-owned-weapon cap
remain unselected. No BAL-3 activities or BAL-4 tuning were started.

The approved checklist:

- [x] Authorize the bounded BAL-2 implementation, without BAL-3/BAL-4 scope.
- [x] Use Low 12/8/3, Medium 22/12/3, High 45/18/4, all at 5s; preserve BAL-1
      damage and BAL-1S values.
- [x] Replace, rather than supplement, Depot loot with two fixed-table cargo
      slots; remove its 10s search and +2 greed/+1 mood completion effect.
- [x] Use 2/6/12% single-weapon-slot chance and 75/25 Medium selection;
      Temptrest remains trader-only.
- [x] Selectively intake three tech definitions, values/rarities 60/3, 150/4,
      900/5, and record actual provenance.
- [x] Allocate to eligible live positive-damage contributors by capped
      contribution, deterministic join/slot/ID ties and round-robin slots.
- [x] Preserve extraction-required cargo and the existing designated single
      pocket exception; no automatic pocket selection or bounty.
- [x] Preserve weapon inventory and defer caps/liquidation to BAL-4; do not
      claim bounded stock.
- [x] Treat best healing as an aspiration and preserve current KO semantics;
      no equipment gate or nonlethal floor.
- [x] Run paired 64-seed 8h/96h cohorts; report outcomes, economy and shield
      progression, limitations and departures without retuning.

## 9. Self-contained implementation prompt (approved and executed 2026-11-01)

The prompt below records the approved execution contract. Its implementation
and automated verification are complete; the experimental balance results and
remaining measurement limits are recorded in the [measurement
report](BAL-2-measurements.md) and [implementation ledger](../IMPLEMENTATION_PROGRESS.md).

```text
Implement only the approved BAL-2 danger robot/reward slice in AFK Raiders Online.
First read docs\IMPLEMENTATION_PROGRESS.md (checkpoint and stable BAL checklist),
docs\balance\BAL-2-proposal.md, docs\GAME_DESIGN.md,
docs\MULTIPLAYER_REBUILD_BRIEF.md, docs\balance\BAL-1-measurements.md,
docs\balance\BAL-1S-measurements.md and docs\balance\BAL-1S-post-BAL-1-gate.md.
The user approved this bounded proposal on 2026-11-01; do not request approval
again. Record the approved decisions, bounded scope, prerequisites and next
action in the ledger, then mark BAL-2 In progress. Preserve unrelated dirty
changes. If any implementation requirement conflicts with the approved scope,
stop and reconcile the contract before changing behavior. Strict best-healing-
only High, nonlethal Low/Medium and cap-now are explicitly not selected; do not
implement an eligibility gate, health floor or automatic weapon sale.
Record approved decisions, bounded scope, prerequisites and next action in the
ledger, then mark BAL-2 In progress. Preserve unrelated dirty changes.

Frozen: BAL-1 equipped snapshots, seeded rounded rolls, shared capped actual
damage, slot order, outgoing before retaliation, no killing-round retaliation,
once-per-extraction weapon wear and KO equipment loss. All BAL-1S defaults and
autonomous preparation prices/reserves/thresholds remain unchanged. Temptrest
is trader-only. No BAL-3/4, new controls, PvP/theft, auth/persistence, AI authority,
dependencies or user-managed backend restart.

Approved experimental profiles: Low HP12/raw8/3 rounds, Medium HP22/raw12/3,
High HP45/raw18/4; rounds every 5s. Apply to normal risk/reward mode only;
preserve historical fast-pacing fixture semantics. One robot/HP/round budget.

Replace (do not supplement) selected Depot two generic draws with two defeat
world cargo slots. Market/Salvage unchanged. At committed defeat atomically
create/allocate cargo, resolve Depot and use 2s Packing; no later 10s reward
search or its +2 greed/+1 mood. No reward on escape/unresolved/no recipients.
No currency/consumable/XP bonus. Fixed nonweapon draws, with replacement:
Low: spent core value10 rarity1 weight70; robot toe 15/2 weight20; knuckle
20/2 weight10. Medium: messy circuits 35/3 weight50; corrupted shard 25/2
weight30; intact shard 60/3 weight20. High: intact shard60/3 weight50;
messy circuits35/3 weight30; overcharged core150/4 weight18; prime-archive
shard900/5 weight2. Slot A always nonweapon; Slot B weapon chance Low2%,
Medium6%, High12%; conditional Low Aspperigo100%, Medium Aspperigo75% and
Vernerider25%, High Vernerider100%; otherwise nonweapon. At most one gun/world.
No exposure/appetite bias on defeat table; unrelated pools unchanged.

Inspect read-only legacy robots.json and loot-tables\arc_tech.json and
weapons_parts.json at f62d088a41da951a1fbe9ed7215d2957a933ce50; follow lore index,
mapping, robots, legal/tone guides. These are reference facts, not copied prose
or source runtime names. Keep existing online robot identities; selectively
review only three new tech definitions. Use approved online content loading,
validate references/schema/rarity/weights/weapon mapping, record actual source
revision and paths plus transformations in README provenance on runtime intake.

Eligible at defeat: actual positive capped-damage contributor in this raid,
at robot location, still FightingRobot, not Downed/KO/extracted/withdrawing.
Snapshot before FinishRobotEncounter resets fighters to Ready. Empty eligibility
resolves reward without cargo creation. Historical/zero-damage/late contributors
get no claim. Rank actual damage desc,
JoinedAt asc, ordinal Slot then PlayerId. Items value desc then InstanceId asc;
round-robin fixed ranking. Keep overkill attribution unchanged. Owner/backpack
assignment once, seeded distinct purpose rolls, unique raid/robot/cargo-slot
instances, explicit validation failures. No Handler claim race.

Cargo stays risky until successful extraction; only existing pre-KO designated
single pocket instance is secured on KO (including field gun). No auto pocket
choice. Lost cargo never reallocates. Equipped weapon/shield loss and unused
staged supply return unchanged; ownership/revival/retries cannot duplicate.
RobotDefeated means robot HP zero; Raider KO never earns robot rewards.
Preserve current robot damage/Downed/KO; no new Low/Medium nonlethal floor.
Record approved online lore clarification instead of importing legacy floors.
Solo High preparation is a statistical aspiration, not a permission gate;
lesser-healing favorable wins are legal and need reward/ownership tests.

Preserve current weapon inventory rules: all unique secured field/pocket guns
convert at existing seeded 5-60% durability and remain owned, even above four.
No global cap, normalization, automatic weapon sale/discard, field overflow or
auto-equip in settlement. Existing equipped loss/wear, home gun safety, trader/
repair/fallback policy and paced errands unchanged. Protect ownership, not
equipped guns from normal KO loss. Defer accumulation/sinks/cap economics to
BAL-4 explicitly; report growth, do not claim bounded stock. Retain section6's
unapproved cap alternative as future context, not implementation instructions.
Ordinary stash remains120, lowest value then ordinal InstanceId overflow sale.

Add focused deterministic tests for exact roll boundaries, profiles/rounds,
shared/solo/timing ties, eligibility/overkill, two items/one gun cap, no escape
or old Depot rewards, private receipts, ownership/pocket/KO/independent extracts,
full stash overflow, above-four inventory preservation, no weapon liquidation,
trader/reserves/errands, repeated/concurrent
generation/settlement, absent-versus-polled parity and narration truth.
Extend existing measurement helpers only as needed: paired seeds1-64, two
unattended Raiders, pinned IDs/trader seed, 5s scheduling, 8h and96h;
true settlement-time windows [0,8),[8,24),[24,48),[48,96) plus8h bands.
Record baseline and approved rules/revisions, n by danger/trait, KO/cause/time,
robot worlds versus participant outcomes/rounds/weapon/shield, healing/overheal,
shield spend/wear, all-source weapon inflow/loss/retirement/broken stock and
gross/net danger value. Assert ownership/stock conservation, not stock<=4.
Report windowed usable/broken stock growth and time-to-first Medium/Heavy shield.
Net asset delta spans pre-admission preparation to settlement: coins+stash+
weapons+owned supplies+shield at catalog value; no double-counting sales/gross
haul, report durability and liquid cash separately. Save raw per-raid evidence.
Run fresh Low solo Kettle+initial-Light starter cohort; mature Low is not proof.
BAL-4 diagnostic KO bands <=10/15-25/25-40%, medical0-1/1-2/2-4 and net
High>Medium>Low remain experimental. Report misses; do not retune frozen rules.

Run smallest relevant server selectors first, then full
dotnet test tests\AFKRaiders.Server.Tests\AFKRaiders.Server.Tests.csproj
Run existing web Node tests and npm run build in web. Verify two independent
browser sessions on an isolated verification server; never restart shared live
backend. Update current behavior/commands/provenance in README, measured report
and ledger with status, evidence, limitations and exact handoff. Do not overwrite
historical reports or mark broader BAL work balanced/complete. If unfinished,
record changed-file links, resume point, pending checks and blockers.
Verify links and git diff --check. Do not commit/push unless requested.
```
