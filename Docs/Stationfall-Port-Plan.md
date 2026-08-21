# Stationfall Port — Plan of Record

Status doc for porting Infocom's *Stationfall* to this engine. Safe to read: **this file contains
no spoilers** and is written to stay that way.

---

## Rule 0 — No spoilers for the repo owner

The owner of this repo **has never played Stationfall** and is building this port in order to play
it fresh. Every agent and contributor working on it must treat that as a hard constraint.

**Never reveal**, in chat or in anything he is likely to read (PR titles and bodies, commit
messages, issue text, release notes): puzzles or their solutions, what is in a room, characters,
the goal, the ending, deaths, or what awards points.

**Spoiler-bearing paths — do not point him at these, and do not quote them:**

| Path | Why |
|---|---|
| `Stationfall/Walkthroughs/` | Complete solutions, the full scoring table, the death catalogue |
| `stationfall-source/` | The original ZIL (gitignored reference checkout) |
| `Stationfall/**`, `Stationfall.Tests/**` | Class and test names necessarily name rooms and objects |

Report progress as **mechanics only**: room and object counts, systems landed, test counts, and the
*reachable score ceiling* — never the trigger that awards a point.

He should **not playtest before Phase 7**. A half-built game means placeholder walls and dead ends,
and a puzzle brute-forced against a broken implementation cannot be un-seen.

---

## Ground rules inherited from `CLAUDE.md`

- The ZIL is **read-only behavioural reference**. Never copy, transliterate, or transcribe it —
  read it to confirm intended behaviour, then write original C# in this engine's idiom.
- **Bug fixes are TDD, no exceptions**: failing test first, minimal fix, then re-run every suite
  that touches the changed code.
- Tests must be **deterministic** — no real clock, randomness, network, or AI. Pin the seams.

---

## Status

Measured against the original, not from memory. Update this table as phases land.

| | Done | Total |
|---|---:|---:|
| Rooms | 6 | 104 |
| Region objects | 20 | 116 |
| Background daemons | ~4 | 32 |
| **Reachable score** | **8** | **80** |
| Tests | 85 | — |

Landed so far:

- **Phase 1** — project scaffold, boot, console entry (`--game Stationfall`), AWS prompt secret.
- **Phase 2** — the opening act: 6 rooms, ~20 objects, its full puzzle chain, all its deaths, and
  the documented walkthrough for that segment running verbatim as a test oracle.
- **Phase 3** — the survival clocks and the day boundary, built on a new shared `StellarPatrol`
  library extracted from Planetfall. Also restored per-action time costs, which the port had been
  flattening to a single value per turn.

For scale: Planetfall in this engine is 127 location files and 142 item files. Stationfall is the
bigger game.

---

## Phases

### Phase 3 — Survival and time systems
No map content. The clocks the rest of the game schedules itself against: the master day-cycle
daemon, the hunger ladder, the tiredness/sleep ladder and waking. Planetfall already implements
close equivalents (`Planetfall/SleepEngine.cs`, `HungerLevel`, `HungerNotifications`,
`SleepNotifications`, `TiredLevel`, `Dreams`) — promote the reusable core to shared code rather
than copy-pasting it, and keep each game's prose and thresholds its own. Save/restore must
round-trip the new state.
**Ceiling: 5 → 8.**

### Phase 4 — Engine capabilities that do not exist yet
Blocking work for Phases 5–6, no score movement:

- **Commanding an NPC** (`<name>, <command>`). The engine has no intent for this at all; Planetfall
  approximates its equivalent with bespoke per-scene code. Needs a real intent plus routing.
- **The companion, into `StellarPatrol`.** He is literally the same character in both games and
  Planetfall's implementation runs to ~1800 lines across nine files. The persona prompts, the canned
  social responses, following the player between rooms and carrying things are all common. What is
  *not* common: power management (only one game's companion starts switched off) and every scripted
  scene, which belong to their own game. Worth doing before Region A needs him, so the work is not
  written twice and then merged.
- **Generalising the elevator/vehicle pattern** — `Planetfall/Location/Kalamontee/ElevatorBase.cs`
  is the closest existing model.
- **Atmosphere / pressure gating**, including its failure daemon.
- **Verb audit.** The original defines ~130 verbs; the engine covers roughly two-thirds. Find and
  fill the gaps.

### Phase 5 — Region A
67 rooms, 59 objects, 114 handler routines. The bulk of the game. Cut into ~6 batches of 10–12
rooms; each batch is one PR that extends the walkthrough oracle before it merges.
**Ceiling: 8 → ~55.**

### Phase 6 — Region B
31 rooms, 37 objects. Follows Region A for dependency reasons.
**Ceiling: ~55 → ~75.**

### Phase 7 — Endgame and the full oracle
The complete 80-point run as a single end-to-end test, a fast critical-path regression, the
death/trap negative tests, and unwinnable-state fixtures (mirror `Planetfall.Tests/Walkthrough/
WalkthroughUnwinnable*.cs` — those are what catch softlocks).
**Ceiling: 80.** This is the gate for playtesting.

### Phase 8 — Making it a pleasure to play
AI narrator prompt tuning for this game's voice, the companion wired for real AI conversation, and
— optional — a web client and Lambda alongside `zorkweb.client` / `planetfallweb.client`. Console
plus AI works without the web tier.

---

## Working method

- One PR per batch, green CI, walkthrough oracle extended before merge.
- **An adversarial review pass at every phase boundary.** On Phase 2 this found 13 real defects
  including a hard softlock and a state that silently made the game unwinnable — precisely the
  bugs that ruin a first playthrough and that ordinary testing does not surface.
- Roughly 12–16 PRs remaining.

## Definition of done

1. The full-score walkthrough runs end to end as a test.
2. Critical-path and death-catalogue tests pass.
3. No known softlock or unwinnable state.
4. Playable from the console with the AI narrator.
