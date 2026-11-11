# StellarPatrol

Code shared by the two Stellar Patrol games — **Planetfall** and its sequel **Stationfall**.

Infocom built the sequel on the original's codebase, so the two games do not merely resemble each
other: whole mechanics are carried over with their text intact. The hunger and fatigue ladders, for
instance, use *word-for-word identical* warning strings in both originals. Anything in that
category belongs here rather than being duplicated per game.

What lives here:

| Area | Why it is shared |
|---|---|
| `Survival/` | The hunger/thirst and fatigue ladders, and their tick-scheduled warnings |
| `Time/` | The wrist chronometer and Galactic Standard Time |

What does **not** live here: anything only one game has (Planetfall's disease, for example), and
any prose specific to one game's voice. The base classes hold the mechanism; each game supplies its
own numbers and its own words by deriving from them.

**Adding to this library:** wait until there are two real implementations before abstracting one.
An abstraction derived from a single example tends to encode that example's accidents.

Applying that test to what exists so far: the *ladders* are shared, because they are genuinely the
same mechanic down to the wording. The per-turn glue that runs them — each game's
`ProcessBeginningOfTurn` — is not, and deliberately stays duplicated. It is thin, and the two
versions differ in ways that matter (one carries a disease and a set of god-mode clock toggles the
other has no notion of). Merging them would mean parameterising away most of what they are.
