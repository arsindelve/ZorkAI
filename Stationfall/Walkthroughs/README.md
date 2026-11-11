> # ⛔ STOP — SPOILERS ⛔
>
> **This file solves the game.** It exists only as the ZIL-verified oracle the port's tests are
> written against. If you intend to *play* Stationfall, close this file now and do not read the
> rest of this folder. See `Docs/Stationfall-Port-Plan.md` for a spoiler-free status of the port.

# Stationfall — Walkthroughs (ZIL-verified reference)

Reference walkthroughs for the Stationfall port. These are the **ground truth** for the eventual
test oracle: the C# `Stationfall.Tests` walkthrough tests will encode these command sequences and
assert on the **checkpoints** called out here (room names, score deltas, key state changes) — exactly
the pattern `Planetfall.Tests/Walkthrough/*` already uses (`Do("command", "expected substring")`
through `GameEngine<StationfallGame, StationfallContext>.GetResponse()`).

## Provenance & method

Synthesized, not copied. The command *order* was reconstructed from public solutions
(the-spoiler.com; the official InvisiClues hint file) and then **every room, exit, object noun, verb,
prerequisite, score award, and death was verified against the original Infocom ZIL** in the local
read-only reference checkout `stationfall-source/` (`historicalsource/stationfall`). Where the fan
walkthroughs drifted from the source, the ZIL wins and the divergence is noted. `file:line` citations
throughout point at that ZIL so the port author can confirm intended behavior (never transcribe it).

Sources that were **not** usable: GameFAQs (bot-blocked), walkthroughking.com (explicit
no-reproduction notice). Nothing from those was copied.

## Files

| File | What it is | Port use |
|---|---|---|
| [`Walkthrough-Full-80.md`](Walkthrough-Full-80.md) | Complete 100% run — all **80** points, `Intergalactic Mega-Hero` | Primary oracle: the long walkthrough test |
| [`Walkthrough-ShipDeparture.md`](Walkthrough-ShipDeparture.md) | Just the opening (Deck Twelve → dock at Bay #2) | Phase-2 smoke test — small, self-contained |
| [`Walkthrough-CriticalPath.md`](Walkthrough-CriticalPath.md) | Shortest route to destroy the pyramid | Fast end-to-end regression |
| [`Deaths-and-Traps.md`](Deaths-and-Traps.md) | Death / trap / red-herring catalogue | Negative-path tests |

## The 80-point scoring table (all ZIL-verified; sum = 80)

| Pts | Trigger | ZIL |
|---:|---|---|
| 5 | Spacetruck docks at **Docking Bay #2** | `ship.zil:1205` |
| 3 | Wake into **Day 2** | `globals.zil:1072` |
| 3 | Take the **medium drill bit** | `station.zil:2530` |
| 6 | Validated form opens the **iris hatch** | `globals.zil:742` |
| 4 | **Set the roulette wheel** (Casino) | `village.zil:734` |
| 3 | Take the **ostrich nip** | `village.zil:540` |
| 4 | Take the **reflective (platinum) foil** | `village.zil:291` |
| 5 | First enter the **Armory** | `station.zil:1054` |
| 5 | Take the **coin** (galakmid) | `village.zil:1628` |
| 6 | **Timer** knocked out of the dispenser by the ostrich | `station.zil:322` |
| 7 | Get the **M-series hyperdiode** (take the seven-pointed star) | `station.zil:1509` |
| 7 | **Survive Plato's attack** | `station.zil:3588` |
| 3 | First enter **Vacuum Storage** ("In Space") | `village.zil:1339` |
| 3 | Put the explosive in the **drilled safe hole** | `station.zil:984` |
| 7 | Take the **key** (from the blown safe) | `station.zil:1024` |
| 2 | First enter **Top of Air Shaft** | `station.zil:3693` |
| 2 | First enter the **Factory** (Level Eight) | `station.zil:3868` |
| 5 | **Win** — foil on the pyramid | `station.zil:3950` |

Rank ladder (`verbs.zil:73-88`): 0 Insignificant Nobody · 1–16 Rising Young Insignificant Nobody ·
17–26 One-Day Flash on the Evening News · 27–39 Footnote in History · 40–49 International VIP ·
50–64 Interplanetary Star · 65–79 Interstellar Superstar · **80 Intergalactic Mega-Hero**.

## Determinism notes for the oracle (critical)

These are the non-obvious things a deterministic test must pin:

1. **Chronometer / spacetruck course.** The correct course is computed from the chronometer reading
   `T` (= `INTERNAL-MOVES`): `course = ((T ÷ 50 − 132)² ÷ 4) + 103` (integer math, `verbs.zil:2136`).
   `T` is seeded randomly at game start (`4430 + RANDOM(1220)`, `misc.zil:190`) and the chronometer
   **stops after Day 2**. A test must fix the start time (as Planetfall's tests reset the
   `Chronometer`) and compute the course from the pinned value.
2. **Days advance only by sleeping** (`WAKING-UP`, `globals.zil:1059`) — there is no fixed turn count
   per day. Sleep is *forced* by the sleep daemon; hunger/sleep will kill you if ignored.
3. **The "Day 4" station-destruction deadline is a countdown that starts when you break into the
   endgame** (opening the Dome bin queues `I-ANNOUNCEMENT`, `station.zil:2124`) — not a fixed
   from-start turn limit. Do everything else first.
4. **Randomness seams** to mock (à la Planetfall): welder appearances (`I-WELDER`, `PROB`), Floyd's
   behavior/wander, dream selection (`PROB 60`), and any `RANDOM` in combat. Pin the chronometer seed.

## The Floyd divergence (port design flag)

Stationfall's ending requires the player to **`SHOOT FLOYD`** in the Factory (`FLOYD-SHOT`,
`ship.zil:503`) — the pyramid has corrupted him and he is firing a stun ray at you — and only then
`PUT FOIL ON PYRAMID` to win (`PYRAMID-F`, `station.zil:3915`). Floyd dies in your arms; Oliver
arrives. Note: the "S.P.S. Flathead / promotion to Lieutenant First Class" passage at
`misc.zil:166-185` is **commented-out dead code**, not the shipped ending. Decide the faithful-vs-AI
handling of this scene deliberately (see the Floyd faithful-divergence pattern in `.claude/CLAUDE.md`).
