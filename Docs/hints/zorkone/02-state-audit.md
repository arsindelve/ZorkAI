# Zork I State-Availability Audit

Same conclusion as Planetfall: **the full world serializes, so localization needs no engine
change.** This doc notes only what's Zork-specific. See
[../planetfall/02-state-audit.md](../planetfall/02-state-audit.md) for the shared serialization facts
(`SaveGame()` → `JsonConvert.SerializeObject` with `TypeNameHandling.All` over Context + Repository).

## Tier 1 — `ZorkIContext`

| Field | Use |
|---|---|
| `CurrentLocation` | Position |
| `Score` (of 350) | Coarse progress; `GameOver` flag set at 350 |
| `Moves` | Pacing |
| `Items` (inventory) | Tools/treasures currently held |
| `GameOver` | Endgame reached. **Sticky** — set once `Score` first hits 350 and **never cleared**, even though `Score` can drop afterward (e.g. taking a treasure back out of the case). So **do not treat `GameOver` as `Score == 350`** — they diverge once a deposited treasure is removed (`ZorkIContext.cs`, field comment). |
| (death/lamp/grue state) | Light-source survival (see below) |

## Tier 2 — the real localization key: treasure + gate state

Zork's progress is **not** captured by score alone (two-stage scoring makes score ambiguous about
*which* treasures are where). The precise signal is per-object state, all serialized:

- **Per treasure: `{unfound, held, deposited}`** — computed from the treasure item's
  `CurrentLocation` (its original room / inventory / inside the `TrophyCase`) and its
  `HasEverBeenPickedUp` flag. The two-stage scoring means "held but not deposited" is a real,
  common, distinct state the hint engine must recognize (don't re-hint "find the X" when it's in
  the player's pack).
- **Trophy Case contents** — the canonical "how close to 350" measure; iterate the case's items.
- **Access-gate flags** — window open, trap door open/closed, troll alive/dead, cyclops fled, dam
  drained, exorcism done, boat inflated, rainbow solid, etc. These live on the relevant item/location
  objects and serialize with everything else.
- **Light source state** — lamp on/off and remaining life, candles lit/burned, torch location.
  Central to soft-lock detection ([03](03-softlock.md)) and to "it's dark, what do I do?" hints.
- **Egg/Canary state** — `Egg.IsDestroyed` (confirmed in `ZorkOne/Item/Egg.cs`) is directly
  readable and is the soft-lock signal for the best-score path ([03](03-softlock.md)#1).

## Recommended progress vector

```
treasures: { Diamond: deposited, Egg: held, Emerald: unfound, ... }   // 19 entries
gates:     { houseOpen, trapDoorOpen, trollDead, cyclopsFled, damDrained, exorcismDone,
             boatInflated, rainbowSolid, ... }
light:     { lampOn, lampLifeRemaining, torchLocation, candlesLit }
flags:     { eggDestroyed, ... }
score, currentLocation, moves
```

- **Position** = which chains are open (gates satisfied) and which treasures are still `unfound`/`held`.
- **"What can I do now?"** = open chains with at least one treasure not yet `deposited`.
- **"What's blocking me?"** = nearest unmet gate toward the treasure the player appears to be chasing.

## Gaps to confirm before Stage 3

1. Exact lamp-life model and whether the engine exposes remaining life (for the grue/darkness hint
   and soft-lock).
2. A clean way to enumerate Trophy Case contents and each treasure's three-state status.
3. Canonical readable flags for each access gate (most exist; locate and name them).
4. Reachability graph (for "dropped a treasure somewhere you can't get back to" — soft-lock [03](03-softlock.md)#5).
