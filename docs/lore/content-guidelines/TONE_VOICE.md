# Tone Voice

AFK Raiders is deadpan, absurd, and affectionate. The raider is brave enough to deploy, foolish enough to keep looting, and self-aware only at the least useful moments.

## Voice Pillars

- Deadpan danger: describe ridiculous survival failures as routine operations.
- Loot appetite: the raider can justify almost any object as part of a strategy.
- Handler distance: the player can advise, but cannot fully control the disaster.
- Machine pettiness: A.R.C. units are threatening and emotionally weird.
- Consequence first: jokes should still tell the player what happened mechanically.

## Reliable Patterns

| Pattern | Shape | Example direction |
|---|---|---|
| Tactical rationalization | Bad choice, official-sounding excuse | The raider calls hiding behind vending equipment a defensive doctrine. |
| Object overvaluation | Junk item, intense justification | A cracked mug becomes morale infrastructure. |
| Machine bureaucracy | Robot action, petty administrative framing | A drone files the raider under avoidable incidents. |
| Survival understatement | Serious danger, calm phrasing | The LZ is compromised, which the raider describes as socially inconvenient. |
| Handler friction | Radio advice, raider interpretation | Calm advice becomes permission to loot one more room. |

## Runtime Writing Rules

- Keep event text readable in the comms log at a glance.
- Prefer one strong joke over three clauses fighting for oxygen.
- Include mechanical changes through existing engine suffixes where possible, such as `Greed +5.` or `Mood -1.`
- Do not explain the joke in UI text.
- Do not hardcode reusable comedy in TypeScript; add it to content JSON.

## DIARY VARIETY pilot voices (2026-11-11 decision; drafts require review)

Event fit and committed consequences come before trait colour. The existing general
pool is not a dedicated Cautious voice; new trait/family lines are deferred to the
review-gated pilot, not enabled by this guide.

- **Brawler:** direct, confrontational overconfidence; a close call is an argument
  the Raider expected to win. Do not imply a fight or charged shield without evidence.
- **Cautious:** attentive, practical risk assessment and dry survival understatement.
  Notices exits, uncertain footing and kit limits; cautiously optimistic only when
  the event supports it. Not a coward, not an omniscient predictor, and not merely
  the general paperwork voice.
- **Gadgeteer:** gear obsession and technical rationalization; trusts the tool more
  than the theory. Never invent carried gear, working equipment or successful fixes.
- **Hoarder:** loot appetite and object overvaluation; future usefulness competes
  with sensible departure. Never invent acquisitions, value or ownership.

Rotate gear obsession, overconfidence, false optimism, genre tension, loot greed
and bureaucracy. At most a quarter of pilot candidates may depend on office or
paperwork metaphors. Hub resident references and live AI are outside slice 1.