# The Hint Oracle (as built)

**This supersedes the architecture in [00](00-master-plan.md) and [07](07-common-architecture.md).** Those
documents, and the per-game `01`–`06` files, are kept as the record of how we got here; the running system is
what this page describes.

## The idea

Someone who has finished Planetfall can sit beside a new player and help: they understand the game, they can see
where the player is, and they use judgment about how much to say. The hint system is that, and nothing else —
**one strong model that understands the whole game, sees the player's actual situation, and answers.**

There is no puzzle graph, no progress mapper, no authored hint ladders, no intent router, no topic aliases, no
spoiler tiers, no soft-lock rule table. Each of those was an *index* of the game, and an index can only answer what
it has an entry for; every miss became another entry. Three generations of that (a RAG agent, a two-call
"docs + LLM" design on a small model, a deterministic DAG/ladder engine with a generated corpus) are in the git
history. What those attempts got wrong was never the absence of structure — it was a weak model, fed source code
and a save-file dump instead of understanding and the player's experience.

## What it is

```
POST /hint { sessionId, question, history[], transcript }
        │
        ▼
HintOracleService ── provider.DescribeSituation(state) + the transcript
        │
        ▼
IHintOracle.Answer( game bible , persona , situation , conversation , question )  →  text
```

| Piece | Where | What |
|---|---|---|
| **The game bible** | `Planetfall/Hints/Oracle/planetfall-bible.md` (embedded) | A veteran's understanding of the whole game, in prose (~70K tokens): how it plays and its clocks; the story *and when each part becomes knowable*; geography and transport; every situation in each act — what the player sees, the idea, why the solution works, exact commands, order constraints, the usual confusions; Floyd; dead ends, deaths and unwinnable states. Static, so the provider's prompt cache absorbs it. |
| **The situation** | `PlanetfallOracleProvider.DescribeSituation` | Facts, read mechanically: location, score, day/time, health/hunger/tiredness, Floyd, inventory, rooms visited, and **what is different in the world from how it began** (`WorldDelta` over `StateFlattener`, against a first-seen baseline). No puzzle is named anywhere in this code. |
| **The transcript** | sent by the client (`recentTranscript`) | The recent stretch of the game as the player saw it. It is how the oracle knows what they have actually encountered — and so what would be a spoiler. |
| **The conversation** | sent by the client (`history`) | What was asked and answered so far. Progressive disclosure is the model reading this and going one step further; there are no rung counters. |
| **The brief** | `OpenAiHintOracle.Brief` | How to be a good hint-giver: answer what was asked; work out where they really are; give away as little as will get them moving, more each time they come back; never spoil what they have not met; look out for them; be truthful; the narrator's voice. One page. It contains no game knowledge. |
| **The model** | `OpenAiHintOracle` | The strongest available (`gpt-6-astra`; `HINT_ORACLE_MODEL` overrides). Hints are rare and their whole value is judgment. Fails closed: an unavailable model is a decline, never a guess. |

The endpoint stays stateless and read-only; asking costs no turn.

## Writing the bible

`Planetfall.Tests/Hints/Oracle/GameBibleGenerator.WriteTheBible` (`[Explicit]`) comes to understand the game the way
a person would:

1. **Plays it** — replays the verified walkthrough through the real engine and keeps every response.
2. **Reads how it works** — the game's source, area by area, digested by the model into study notes that describe the
   *game*, never the code.
3. **Reads the hint book and the lore notes** (`invisiclues.md`, `05-lore.md`).
4. **Writes the bible** section by section from all of that, as the briefing one would give a friend about to coach
   new players.

Review the result and check it in. To improve the oracle's knowledge, edit the prose (or the section briefs) — not code.
`WriteBaseline` (no model calls) regenerates `planetfall-initial-state.json`.

## How we know it works

`OraclePlaythroughEval.Grade_Oracle` (`[Explicit]`): sixty moments along the verified walkthrough
(`HintCheckpoints`), a stuck player's questions at each — vague, then "more", then "just tell me"; story questions;
dead ends; mid-puzzle; survival — and a **grader** (`HintGrader`, a different model) that reads every exchange with
ground truth the oracle never sees: the author's note on the moment, the player's real state, the walkthrough's next
commands (including where the test script cheats with a god-mode hook), the hint book and lore notes. Verdicts:
GOOD / WEAK / WRONG / SPOILER, with a one-line reason each; `HINT_EVAL_OUT` writes the graded transcript.

First results, same sixty moments, same grader:

| system | GOOD | WEAK | WRONG | SPOILER |
|---|---|---|---|---|
| index engine (DAG + generated ladders + router, `gpt-5.4-mini`) | 41 | 8 | 11 | 0 |
| **oracle** | **55** | 3 | 2 | 0 |

Both of the oracle's "WRONG"s were the grader's error on inspection (the defense panel does hold four boards; sleeping
until morning is the cure for the shuttle curfew). The oracle also caught a real rule the walkthrough script cheats
past — the shuttle refuses to start after 6000 — which no index had an entry for.

## Adding a game

Implement `IOracleProvider` (persona, embedded bible, `DescribeSituation`), point a copy of the bible generator at that
game's source, walkthrough and hint book, register `IHintOracle` and the `/hint` endpoint. Write sixty checkpoints.

## To do

- Cost gating (per-session/day limits) — deliberately deferred.
- The Zork I provider.
