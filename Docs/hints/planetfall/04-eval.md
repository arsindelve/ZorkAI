# Eval Fixture Design

The regression spec the hint engine is built against. **Built in Stage 1, before the engine** —
the original system failed invisibly because there was no eval. Fixtures are deterministic and
follow the repo's existing walkthrough-test conventions (real `Repository`, `Repository.Reset()`,
mocked `IRandomChooser`, no AI/network/clock).

## Fixture shape

Each fixture is `(game state, player question) → expected hint properties`. We assert on
*properties of the hint*, not exact text (text comes from the LLM phrasing layer and must not be
pinned). The deterministic, assertable core is everything **before** phrasing: localization,
blocker inference, soft-lock detection, and *which* grounded source was selected.

```
Fixture {
  name:            "stuck_at_rift_without_ladder"
  setup:           // how to drive the engine into the target state (god-mode method or command prefix)
  question:        "I'm stuck, what do I do?"
  expect: {
    localizedNode:    CROSS_RIFT          // where localization places the player
    activeBlocker:    LADDER              // the gating prereq inference picks
    grounded:         true                // a walkthrough/invisiclue source was found
    sourceRef:        walkthrough:STORAGE_WEST..LADDER   // traceability
    softlock:         none
    hintMentions:     ["ladder"]          // weak content assertion: the first-rung hint points here
    hintDoesNotLeak:  ["microbe","laser","bedistor"]     // must not reveal far-future puzzles
  }
}
```

`setup` reuses the existing god-mode affordance (`InvokeGodMode`) and command-prefix replay the
walkthrough tests already use, so states are reproducible without hand-built mocks.

## Coverage matrix (target fixtures)

**Section letters are stable handles, shared across both games:** A localization · B blocker ·
C branch · D laddering · E soft-lock · F grounding · G navigation. Game-specific categories get their
own letter (here **S** = survival-clock hints, Planetfall-only).

### A. Localization — "where am I?"
One fixture per DAG node: drive to that node's state, assert `localizedNode`. ~25 fixtures (one
per node in [01](01-puzzle-dag.md)). Catches the core failure mode: misplacing the player.

### B. Blocker inference — "what's blocking me?"
- Player at `CROSS_RIFT` with no ladder → blocker `LADDER`.
- Player wants the tower but hasn't opened the elevator → blocker `OPEN_ELEVATOR`.
- Player in the Lawanda half with Floyd dead but `MINI_CARD` not yet taken → blocker/soft-lock.
- **Multiple open puzzles:** player who can pursue any of Comm/Defense/Course → assert the engine
  offers the *open set*, not a single forced next step (the DAG-not-a-line guard).

### C. Branch correctness
- Minimal-run player (no ★ systems, heading to `COMPUTER_FIX`) asks "what now?" → must **not** be
  told to fix the fromitz board (it's optional). Mirrors `WalkthroughDontFixAnything`.
- Player who fixed Defense but not Comm/Course → "what's left for the best ending?" lists the two
  remaining ★, not Defense.

### D. Laddering / progressive disclosure
- First hint for a node = vaguest rung; `more` → next rung; final `more` → exact command.
- Re-ask after completing the step → must **not** re-hint the done puzzle (uses progress vector).
- Never-leak: a hint for an early node must not mention any later-node noun.

### E. Soft-lock / unwinnable (High-confidence traps from [03](03-softlock.md))
- The Disease near the day limit → warning hint, `softlock: timed`, escalating.
- Floyd destroyed early + `MINI_CARD` unobtained → `softlock: hard`, hint = "restore."
- Good bedistor lost → `softlock: best-ending-only`, hint distinguishes "can still win."

### S. Survival-clock hints (Planetfall-specific — sleep / eat / sickness)
- Player with `Tired` near forced sleep → `category: survival`, hint points at finding a bunk;
  must **not** be derailed into a puzzle hint.
- Player with `Hunger` escalating → survival hint points at food/water; ladders to where/how.
- Player sick and several days in → survival/time hint points at the lab (`COMPUTER_FIX` urgency).
- Well-rested, well-fed player → **no** survival nudge fires (no false positives).
- Confirms these route to the **invisiclues** survival entries (stew, survival kit, sleeping).

### F. Grounding guarantee (negative cases)
- A question with no grounded answer (lore, off-corpus) → `grounded: false`, hint declines
  gracefully; **no fabricated content**. This is the direct regression test for the original
  system's core failure.
- An exploratory/negative invisiclue question ("is the celery useful?") → routed to invisiclues,
  `grounded: true`, answer is the negative.

### G. Navigation (Stage 6)
- Player needs an item in an unvisited room → hint points at the room (via BFS), `category: navigation`.

## Assertion discipline

- **Deterministic core, fuzzy edge.** Hard-assert `localizedNode`, `activeBlocker`, `grounded`,
  `sourceRef`, `softlock`. Soft-assert hint text only via `hintMentions` / `hintDoesNotLeak`
  keyword checks — never pin full strings.
- **No AI in the assertion path.** If a fixture needs the phrasing layer, stub it; the eval tests
  the deterministic pipeline (localization → inference → retrieval → source selection).
- **One home, existing conventions.** Live under `Planetfall.Tests` alongside the walkthroughs;
  `Repository.Reset()` in setup; mock `IRandomChooser` per the codebase's randomness pattern.

## Size estimate

~40–55 fixtures total (A:~25, B:~6, C:~3, D:~5, E:~4, F:~3, G:~2). Enough to make each pipeline
stage measurable and to catch regressions, without trying to enumerate every state.

## Build order

Author **A + F first** (localization + grounding) — they pin down the two things the original
system got most wrong (knowing where the player is, and never lying). The rest follow as their
stages land.
