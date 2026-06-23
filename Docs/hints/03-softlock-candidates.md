# Soft-Lock / Unwinnable-State Candidates

A *satisfying* hint system detects "you can no longer win — restore an earlier save" instead of
hinting toward a now-impossible solution. This is the draft list of Planetfall states to detect.

> ⚠️ **All entries are CANDIDATES pending ZIL confirmation** (`planetfall-source/`). Some may turn
> out to be non-issues (the engine prevents them) or deaths (handled by restart, not hints).
> Confidence is my estimate, not a verified fact. Recommend implementing only the **High**
> confidence ones in v1, behind explicit per-trap checks (not a general solver).

## Categories

- **Hard soft-lock** — game continues but victory is now impossible. *This is what we must detect.*
- **Timed failure** — a clock will kill/fail you if you dawdle; surfaceable as a *warning*, not a restore.
- **Death** — already handled by the death/restart system; not a hint concern.

## Candidates

| # | Candidate | Type | Conf. | Detect via | Notes / verify |
|---|-----------|------|-------|-----------|----------------|
| 1 | **The Disease clock** — `Day` advances; the player sickens over days and dies if the cure (`COMPUTER_FIX`) isn't completed in time | Timed failure | High | `Day`, sickness level, `COMPUTER_FIX` flag | Central time pressure. Surface as escalating warning + "prioritize the lab"; becomes terminal at the death threshold. Confirm the day limit. |
| 2 | **Floyd destroyed before `BIOLOCK`** — Floyd must be alive & present to retrieve the miniaturization card; the card is otherwise unobtainable | Hard soft-lock | High **[verify]** | Floyd liveness flag + `MINI_CARD` not yet obtained + Floyd not in a state to reach the bio lab | If the engine scripts Floyd's survival until the bio lock, this can't happen → downgrade. Confirm whether Floyd can die early. |
| 3 | **Laser fired / battery exhausted before the microbe** — the laser needs the fresh battery (Lab Storage); if charge is spent or the fresh battery is lost, the speck can't be destroyed | Hard soft-lock | Medium **[verify]** | Laser charge/battery state + `COMPUTER_FIX` incomplete | Walkthrough explicitly swaps in the fresh battery (lines 335-336). Confirm the original battery is dead and charges are finite. |
| 4 | **Good bedistor destroyed** — needed for `COURSE_FIX`; if the only good one is fried/lost, course control can't be fixed | Hard soft-lock (optional system) | Medium **[verify]** | bedistor inventory/state + `COURSE_FIX` incomplete | Only blocks the *optional* Course-Control ending, not victory. So it's a "you can't get the best ending" soft-lock, not a total loss — disclose accordingly. |
| 5 | **A required card dropped in an unreachable room** — e.g. dropped across the rift after the ladder is gone, or in a one-way teleport station | Hard soft-lock | Medium **[verify]** | card `CurrentLocation` = a room not currently reachable from player position (needs reachability check) | Requires the pathfinding/reachability graph. Tractable but more work. |
| 6 | **Survival clocks (hunger/thirst/sleep)** — running out kills the player | Death | High | `Hunger`, `Tired` | Already handled by `DeathProcessor` → restart. Not a hint concern beyond a "you should eat/sleep" nudge. |
| 7 | **Auxiliary booth is receive-only** — after exiting via the auxiliary booth, confirm you can't strand yourself | Hard soft-lock | Low **[verify]** | location reachability after `COMPUTER_FIX` | Walkthrough note line 354 ("only as a receiving station"). Likely fine on the intended path; confirm no dead-end. |

## Detection strategy (v1)

For each **High**-confidence hard soft-lock, a dedicated predicate over the progress vector:

```
isUnwinnable(state):
  if floydDeadEarly(state) and not state.MINI_CARD: return ("Floyd is gone and the miniaturization
      card is now unreachable — restore an earlier save.")
  ...
```

- **Per-trap predicates**, not a general "can the goal still be reached" solver. The general
  version (prove no path to `ENDING` exists) is the hard, deferred version (see master plan
  open-decision #5).
- **Distinguish total loss from best-ending loss.** #4 only forecloses the optimal ending; the
  hint should say "you can still win, but the perfect ending is no longer reachable" — not
  "restore."
- **Timed failures (#1) are warnings, not restores** — until they cross the terminal threshold.

## To confirm against ZIL

1. The Disease day limit (#1) and whether sickness is reversible after a point.
2. Whether Floyd can be destroyed before the bio lock (#2) or is script-protected.
3. Laser charge model and battery deadness (#3).
4. Good-bedistor provenance and whether it's destructible (#4).
5. Any other consumable/one-shot items the walkthrough quietly depends on.
