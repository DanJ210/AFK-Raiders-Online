# Weapons

AFK Raiders weapons are improvised tools, misfiled hardware, and aggressively overconfident objects that the raider insists are "fine." The Tea Kettle is always freely available in Desperanza; one non-starter weapon may appear in finite trader stock each Hub cycle. Aspperigo and Vernerider can also be found in field opportunities and become Raider-owned equipment on safe settlement. Temptrest AR remains trader-only.

## Current Arsenal

| Weapon | ID | Damage | Cost | Repair | Lore hook |
|---|---|---|---|---|---|
| Tea Kettle | `tea_kettle` | 3-6 | 0 | 0 | Default survival tool. Whistles, complains, and still somehow gets the job done. |
| Aspperigo | `aspperigo` | 4-7 | 65 | 18 | Eddie's favorite and we don't know why. |
| Vernerider | `vernerider` | 5-9 | 140 | 30 | No one can pronounce it right. |
| Temptrest AR | `temptrest_ar` | 7-11 | 280 | 55 | The favorite among Raiders. The most prized possession. |

## Design Rules

- Weapons should sound practical, but only just.
- Higher tiers should feel like upgraded bad decisions, not heroic armaments.
- Damage and durability are gameplay values; the joke lives in the naming and flavor.
- Found weapons start at full durability and are owned equipment, not sellable loot or stash contents. The raider can own and repair weapons, but a failed raid can still force a loss before settlement.
- Do not use source-game weapon names or copy source flavor text.

## Runtime Notes

- Trader weapons are bought with coins from shared finite cycle stock at catalog value; the free Tea Kettle remains available.
- The currently equipped weapon controls robot encounter damage ranges unless an activity overrides them.
- Healing items are selected separately and consumed when the raid starts.
