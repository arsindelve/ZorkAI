> # ⛔ STOP — SPOILERS ⛔
>
> **This file solves the game.** It exists only as the ZIL-verified oracle the port's tests are
> written against. If you intend to *play* Stationfall, close this file now and do not read the
> rest of this folder. See `Docs/Stationfall-Port-Plan.md` for a spoiler-free status of the port.

# Stationfall — Ship Departure segment (Deck Twelve → Docking Bay #2)

The small, self-contained opening aboard the S.P.S. *Duffy*. This is the recommended **first oracle**
for the port (the Phase-2 smoke test): six rooms, no combat, one computed puzzle, and a clean scoring
checkpoint (+5) at the end. Everything here is verified against `ship.zil` / `verbs.zil` / `misc.zil`.

## Fixtures the test must pin (determinism)

- **Chronometer start value `T`.** Production seeds it `4430 + RANDOM(1220)` (`misc.zil:190`). The
  test must set a fixed `T` (mirror `Planetfall.Tests` resetting the `Chronometer`), then compute the
  expected course from it.
- **Course formula** (`verbs.zil:2136-2152`, integer arithmetic):
  `course = ((T ÷ 50 − 132)² ÷ 4) + 103`.
  Worked example — if the test pins `T = 6600`: `6600÷50 = 132`; `132−132 = 0`; `0² = 0`; `0÷4 = 0`;
  `0 + 103 = 103`. So `type 103` is correct at `T = 6600`. (Pick a `T` that yields a tidy course, or
  compute it in the test from the pinned value — don't hard-code a magic number without deriving it.)
- **Floyd's random seams** mocked so he doesn't wander before boarding (as in `WalkthroughTestBase`).

## Starting inventory (given, not fetched)

Chronometer (worn), Patrol uniform (ID card inside), and all three forms — Assignment Completion
(QX-17-T), **Robot Use Authorization** (JZ-59-G), **Class Three Spacecraft Activation** (HB-56-V) —
all `IN PROTAGONIST` (`ship.zil:36-61`).

## Command sequence

```
                              [Deck Twelve]           ← start
east                          [Cargo Bay Entrance]
north                         [Robot Pool]
put robot use authorization form in slot   → "Authorization approved. Type the bin number…"
type 3                                     → Floyd: "Yippee!"  (bin 3; ROBOT-PICKED = Floyd)
south                         [Cargo Bay Entrance]
east                          [Cargo Bay]
open hatch
enter truck                   (Floyd follows)         [Spacetruck]
close hatch
read time                     ← reads T (= INTERNAL-MOVES)
enter pilot seat              → Floyd auto-boards the copilot seat (PILOT-SEAT-F ship.zil:1070)
put class three activation form in slot    → "Spacecraft activated. Type in the course heading."
type <course>                 ← = ((T÷50−132)²÷4)+103
wait                          → "Launch in approximately 30 millichrons…"
wait                          → auxiliary rockets / fuel messages
wait                          → the station resolves into view
wait                          → "Docking bay one is occupied. Defaulting to bay two."  **[+5]**
```
Docking asserts: room becomes **[Docking Bay #2]**, gravity engages, the whisper **"Stationfall,"**
score = **5** (`ship.zil:1197-1220`).

```
get up
take kit                      (survival kit — keep the THERMOS for later phases)
open hatch
exit                          [Docking Bay #2]
east                          [Level Five]            ← you're now in the station proper
```

## What the oracle should assert

| After | Assert |
|---|---|
| `type 3` | Floyd selected (a "Yippee"/selection string; `ROBOT-PICKED`) |
| launch `wait`s | docking text + room **[Docking Bay #2]** + **score == 5** |
| `east` | room **[Level Five]** |

## Failure cases to also cover (see `Deaths-and-Traps.md`)

- `type 1` (Rex) → later death; `type 2` (Helen) → shreds forms (softlock). Only `type 3` is safe.
- Launch with the hatch open, or without both seats filled → death / refusal (`ship.zil:1164`,
  `globals.zil:766`).
- A wrong `type <course>` → stranded, `I-SUFFOCATE` queued (lethal).
