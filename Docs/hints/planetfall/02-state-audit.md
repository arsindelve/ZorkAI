# State-Availability Audit

**Question this answers:** can we localize the player against the puzzle DAG from live state
alone — and if so, what do we key on? **Answer: yes, fully.** The engine already serializes the
entire world, so localization needs no engine changes.

## How state is persisted

`GameEngine.SaveGame()` (in `GameEngine/GameEngine.cs`) does:

```csharp
JsonConvert.SerializeObject(savedGame, JsonSettings());   // SavedGame<TContext>
// JsonSettings(): TypeNameHandling = TypeNameHandling.All
```

(Symbols cited by name rather than line number — line numbers drift across branches.)

`SavedGame<TContext>` (`GameEngine/SavedGame.cs`) holds three fields: `Context`, and
`AllItems` / `AllLocations` — `Dictionary<Type, IItem>` / `Dictionary<Type, ILocation>` **extracted
from the Repository at save time** (it does not hold the `Repository` object itself). `TypeNameHandling.All`
means concrete subtypes (and their puzzle flags) round-trip. So a saved game *is* a complete,
machine-readable snapshot of everything the hint engine could need: the Context plus every item and
location with all its state. Localization can run against the live in-memory object graph (the
Repository's items/locations + the Context) — no new persistence, no engine change.

## What's available, by tier

### Tier 1 — Context (coarse, always present)
The **Scope** column matters for Stage 7: `base` fields live on `Context` (`GameEngine/Context.cs`)
and **transfer to Zork I for free**; `PF` fields are Planetfall-specific (`PlanetfallContext.cs`) and
have no Zork analog.

| Field | Scope | Type | Use for localization |
|---|---|---|---|
| `CurrentLocation` | base | `ILocation` | Primary position signal |
| `Score` | base | `int` (of 80 here) | **Coarse** progress index — tiebreaker only (see DAG score map) |
| `Moves` | base | `int` | Pacing / "how long stuck" |
| `Items` (inventory) | base | `List<IItem>` | Which cards/tools/keys the player holds |
| `PreviousLocationName`, `LastMovementDirection` | base | | Recent trajectory |
| `LastInput` / `LastResponse` | base | `string?` | What they just tried (intent of a vague "what now?") |
| `Day` | PF | `int` | **Drives The Disease clock** — central to soft-lock/time pressure |
| `Hunger` / `Tired` | PF | enums | Survival-clock pressure (death risk, not progress) |
| `CurrentTime` (Chronometer) | PF | `int` | Time-gated events |
| `HasTakenExperimentalMedicine` | PF | `bool` | Disease-related branch |
| `DeathCounter` | PF | `int` | How many restarts — frustration signal for disclosure pacing |

### Tier 2 — per-item / per-location flags (precise, the real signal)
Because the whole Repository serializes, every puzzle's completion flag is readable on its item
or location object. Examples already visible in the codebase:

- `ComputerRoom.FloydHasExpressedConcern` (used by `WalkthroughBioLock.SetupBioLock`)
- Panel open / fromitz placed / cube state (Planetary Defense, Course Control)
- Floyd's alive/present/on state (`Floyd.IsHereAndIsOn`, used in `GetSaveGameRequest`)
- Card items' `CurrentLocation` (held vs dropped vs still in a desk)
- `Chronometer.BeingWorn`, `Chronometer.CurrentTime`
- The Comm/Defense/Course "fixed" booleans — `CommunicationsFixed` / `CourseControlFixed` /
  `PlanetaryDefenseFixed` on `SystemsMonitors` (`Planetfall/Location/Kalamontee/Admin/SystemsMonitors.cs:65-69`),
  each backed by a `Fixed` string set. Ending selection reads them in `CryoAnteroomLocation.cs`.
  (The endings table in `PlanetfallContext.cs` is a reference **comment block**, not executable
  logic — don't start Stage 3 there.)

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

1. **A canonical flag per ★ system + the cure.** ✅ Located: `SystemsMonitors.{CommunicationsFixed,
   CourseControlFixed, PlanetaryDefenseFixed}` for the three ★ systems, and the mandatory cure
   (the `COMPUTER_FIX` spine) is signalled by **`Relay.SpeckDestroyed`** (`Planetfall/Item/Computer/Relay.cs`)
   — destroying the speck/microbe is the cure. Confirm `SpeckDestroyed` is the right read for "cure done."
2. **Floyd liveness flag** for soft-lock detection (he must be alive until `BIOLOCK`).
3. **Card location vs possession** — distinguish "card still in desk" / "held" / "dropped in an
   inaccessible room" (matters for soft-lock and re-hint suppression).
4. **A stable serialization entry point for read-only access.** We want to localize without
   triggering a real save; confirm we can snapshot Context+Repository in-process cheaply.

None of these require engine changes — they're "find and name the existing flag" tasks.
