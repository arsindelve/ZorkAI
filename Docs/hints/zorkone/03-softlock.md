# Zork I Soft-Lock / Unwinnable Candidates

Zork I is **famous** for unwinnable states — more so than Planetfall. Detecting "you can no longer
reach 350 / you're about to die in the dark" is a big part of a satisfying Zork hint system. Draft
list; verify against `zork1/` ZIL.

> Confidence is my estimate. Recommend implementing **High** ones in v1 as per-trap predicates.
> Categories: **Hard soft-lock** (victory/350 now impossible), **Best-score-only** (can still finish
> but not max), **Timed/death** (handled by death/restart; hint surfaces as a warning).

| # | Candidate | Type | Conf. | Detect via | Notes |
|---|-----------|------|-------|-----------|-------|
| 1 | **Egg force-opened / canary ruined** — opening the jewel-encrusted egg yourself (or by force) damages it and the canary inside; you can never re-open it cleanly, losing Bauble/full points | Best-score-only | **High — confirmed in code** | `Egg.IsDestroyed` (`ZorkOne/Item/Egg.cs`); deposit value drops 5→2 when destroyed | The classic Zork soft-lock. Must give the egg to the **thief** to open delicately. Hint must warn *before* the player force-opens it, and after, explain the score is now capped (not "restore unless you care about 350"). |
| 2 | **Open flame in the gas room** — bringing the lit torch, lit candles, or a match into the coal-gas room causes an explosion (death) | Death (avoidable) | **High** | Light-source type carried + location = gas room | The torch is itself a flame, so the deep mine must be done with the **lamp**, lowering the torch+screwdriver via the **basket/shaft** to bypass the gas room. Hint: "don't carry an open flame in here." |
| 3 | **Light exhaustion in the dark** — the lamp has finite life; candles burn down; matches are limited. No light in a dark room → eaten by a grue (death) | Death (avoidable) → can also strand items | **High [verify]** | `lampOn` + remaining life + current room darkness | Surface as escalating warning ("the lamp seems dimmer"). Confirm the engine's lamp-life model. If the lamp dies underground with no backup, it can also be an effective soft-lock. |
| 4 | **Sharp objects in the inflatable boat** — carrying the sword/axe/knife/screwdriver in the boat punctures it (death / lost boat) | Hard soft-lock / death | **High** | Inventory contains an `IAmPointyAndPunctureThings` item while boarding/in the boat | The boat is the **`PileOfPlastic`** class (`ZorkOne/Item/PileOfPlastic.cs`) — there is no `Boat`/`InflatableBoat` class; the puncture check `context.GetItems<IAmPointyAndPunctureThings>().Any()` lives in `ISubLocation.GetIn` (boarding the boat), not in `InflateTheBoat`. Hint: "empty your hands of anything sharp before boarding." |
| 5 | **Treasure lost to an unreachable place** — dropped in the river (carried over the falls), left behind a one-way gate, or stolen and unrecoverable | Hard soft-lock | Medium **[verify]** | treasure `CurrentLocation` not reachable from player (needs reachability graph) | River especially: items dropped in moving water are gone. Requires the reachability check (see [02](02-state-audit.md) gap #4). |
| 6 | **Exorcism mis-sequenced / candles consumed** — the bell/candles/book ritual must be done correctly; candles can be blown out or burned, and matches are limited; failing may block access to the temple treasures | Hard soft-lock (region) | Medium **[verify]** | candle/bell/book state + temple progress | Confirm whether the engine permits an unrecoverable ritual failure. |
| 7 | **Trap door closes / trapped underground without light or path** | Hard soft-lock / death | Medium **[verify]** | trap-door state + light state + position | Confirm whether the player can be sealed below with no way back. |
| 8 | **Thief killed at the wrong time / with treasures undeposited** — or the egg never opened because the thief died first | Best-score-only / hard | Medium **[verify]** | thief state + egg `IsDestroyed`/opened + stolen-treasure recovery | The thief is both a threat and a required tool (he opens the egg). Confirm the failure windows. |

## Detection strategy (v1)

Same as Planetfall: **per-trap predicates over the progress vector**, not a general solver.

- **#1, #2, #3, #4 first** (High). #1 and #4 are partly *preventive* — the best hint fires
  **before** the irreversible action, keyed on "player is about to / is holding the dangerous combo."
- **Distinguish total loss from best-score-only.** #1 and #8 usually only cap the score; the hint
  says "you can still finish, but 350 is no longer reachable" — not "restore." #2/#3/#4 are deaths or
  hard locks → "restore."
- Several Zork soft-locks are **preventable with a timely warning** — lean into proactive hints here
  more than in Planetfall, where most traps are discovered after the fact.

## To confirm against ZIL (`zork1/`)

1. Lamp-life model and grue/darkness death conditions (#3).
2. River/falls item-loss behavior and any other one-way item sinks (#5).
3. Whether the exorcism / trap door / thief failures are truly unrecoverable (#6, #7, #8).
4. The full set of "sharp" objects that puncture the boat (#4).
