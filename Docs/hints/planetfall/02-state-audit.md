# State-Availability Audit

**Question this answers:** can we localize the player against the puzzle DAG from live state
alone — and if so, what do we key on? **Answer: yes, fully.** The engine already serializes the
entire world, so localization needs no engine changes.

## How state is persisted

`GameEngine.SaveGame()` (`GameEngine/GameEngine.cs:579`) does:

```csharp
JsonConvert.SerializeObject(savedGame, JsonSettings());   // SavedGame<TContext>
// JsonSettings(): TypeNameHandling = TypeNameHandling.All   (GameEngine.cs:895)
```

`SavedGame<TContext>` is the **Context plus the full Repository** — every item and location with
all its state. `TypeNameHandling.All` means concrete subtypes (and their puzzle flags)
round-trip. So a saved game *is* a complete, machine-readable snapshot of everything the hint
engine could need. Localization can run against the same object graph the game already has in
memory — no new persistence, no engine change.

## What's available, by tier

### Tier 1 — `PlanetfallContext` (coarse, always present)
From `Planetfall/PlanetfallContext.cs` and the base `Context` (`GameEngine/Context.cs`):

| Field | Type | Use for localization |
|---|---|---|
| `CurrentLocation` | `ILocation` | Primary position signal |
| `Score` | `int` (of 80) | **Coarse** progress index — tiebreaker only (see DAG score map) |
| `Moves` | `int` | Pacing / "how long stuck" |
| `Items` (inventory) | `List<IItem>` | Which cards/tools/keys the player holds |
| `Day` | `int` | **Drives The Disease clock** — central to soft-lock/time pressure |
| `Hunger` / `Tired` | enums | Survival-clock pressure (death risk, not progress) |
| `CurrentTime` (Chronometer) | `int` | Time-gated events |
| `HasTakenExperimentalMedicine` | `bool` | Disease-related branch |
| `DeathCounter` | `int` | How many restarts — frustration signal for disclosure pacing |
| `PreviousLocationName`, `LastMovementDirection` | | Recent trajectory |
| `LastInput` / `LastResponse` | `string?` | What they just tried (intent of a vague "what now?") |

### Tier 2 — per-item / per-location flags (precise, the real signal)
Because the whole Repository serializes, every puzzle's completion flag is readable on its item
or location object. Examples already visible in the codebase:

- `ComputerRoom.FloydHasExpressedConcern` (used by `WalkthroughBioLock.SetupBioLock`)
- Panel open / fromitz placed / cube state (Planetary Defense, Course Control)
- Floyd's alive/present/on state (`Floyd.IsHereAndIsOn`, used in `GetSaveGameRequest`)
- Card items' `CurrentLocation` (held vs dropped vs still in a desk)
- `Chronometer.BeingWorn`, `Chronometer.CurrentTime`
- The Comm/Defense/Course "fixed" conditions (the endings logic in `PlanetfallContext.cs`
  references global fixed-state per system)

**These boolean milestone flags — not score — are the right localization key.** Score jumps in
coarse chunks and is identical for players blocked on different things; the per-puzzle flags are
exact.

## Recommended localization model: a "progress vector"

Define one boolean (or small enum) per DAG node, computed from Tier-2 flags + inventory +
location:

```
progress = {
  ESCAPE_POD: done, LAND: done, FLOYD: alive&awake, STEEL_KEY: held, ...
  COMM_FIX: commFixedFlag, DEFENSE_FIX: defenseFixedFlag, COURSE_FIX: courseFixedFlag,
  COMPUTER_FIX: computerFixedFlag, FLOYD_DEAD: !floyd.Alive, ...
}
```

Then:
- **Position** = the frontier of the DAG (nodes done vs not).
- **"What can I do now?"** = nodes whose prerequisites are all `done` but the node isn't.
- **"What's blocking me?"** = the unmet prerequisite between the player and their apparent goal.
- **Score** is only a disambiguator when two flag-states are otherwise indistinguishable.

## Gaps / things to confirm before Stage 3

1. **A canonical flag per ★ system.** Confirm there's a single readable boolean for each of
   COMM-FIXED / DEFENSE-FIXED / COURSE-CONTROL-FIXED and for the computer fix (the endings code
   reads them, so they exist — locate and name them).
2. **Floyd liveness flag** for soft-lock detection (he must be alive until `BIOLOCK`).
3. **Card location vs possession** — distinguish "card still in desk" / "held" / "dropped in an
   inaccessible room" (matters for soft-lock and re-hint suppression).
4. **A stable serialization entry point for read-only access.** We want to localize without
   triggering a real save; confirm we can snapshot Context+Repository in-process cheaply.

None of these require engine changes — they're "find and name the existing flag" tasks.
