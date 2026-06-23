# Zork I Eval Fixture Design

Same discipline as Planetfall ([../planetfall/04-eval.md](../planetfall/04-eval.md)) — deterministic
`(state, question) → expected hint properties`, real `Repository`, `Repository.Reset()`, mocked
`IRandomChooser`, assert on the deterministic core (localization / blocker / soft-lock / source
selection), keyword-only on hint text. This doc lists only what's Zork-specific.

## What changes vs Planetfall

The open treasure-hunt shape means the eval emphasis shifts:

### A. Localization — "where am I / what have I got?"
- Per treasure, drive to each of `{unfound, held, deposited}` and assert the engine reports the right
  state. The **held-but-not-deposited** case is the one most likely to regress (don't re-hint
  "find the diamond" when it's in the pack).
- Trophy-case progress: N treasures deposited → "how close am I?" reports the right remaining set.

### B. Blocker inference — "what's blocking me?"
- Player chasing the reservoir treasures but the dam isn't drained → blocker = dam (wrench + button).
- Player at the cyclops → blocker = the magic word.
- **Open-set, not single-step:** mid-game player with many chains available asks "what now?" → engine
  offers the **set of open chains**, not one forced treasure. This is the core Zork-vs-Planetfall
  guard (the "DAG-not-a-line" test matters more here).

### C. Branch / optionality
- Treasures are independent: a hint toward Treasure A must not imply B is a prerequisite unless it
  genuinely is (shared gate). Assert no false prerequisite coupling between unrelated chains.

### Ev. Soft-lock — Zork's signature cases ([03](03-softlock.md))
- **Egg about to be force-opened** (player holds egg + issues open/pry) → **preventive** warning
  *before* the action; `softlock: best-score-only`.
- **Egg already destroyed** (`Egg.IsDestroyed`) → hint acknowledges score is capped, does **not** say
  "restore" unless the player wants 350.
- **Open flame + gas room** → preventive death warning.
- **Sharp object + boarding boat** → preventive warning.
- **Lamp life low in the dark** → escalating warning hint.

### F. Grounding guarantee (negative cases)
- Off-corpus / lore question → `grounded: false`, declines; **no fabrication**.
- Exploratory/negative question routed to the hint source, not invented.

### G. Navigation
- "Where do I find a light source?" early game → points at the lamp in the house (via BFS).

## Build order

Author **A (treasure three-state localization)** and **Ev #1 (the egg)** first — they pin the two
things most particular to Zork: the held-vs-deposited distinction, and the franchise's signature
soft-lock. Both are confirmable directly from in-repo code (`TrophyCase`, `Egg.IsDestroyed`).

## Size estimate

~35–50 fixtures: A is large (19 treasures × a few states), B/Ev/F/G smaller. As with Planetfall,
the goal is making each pipeline stage measurable, not enumerating every state.
