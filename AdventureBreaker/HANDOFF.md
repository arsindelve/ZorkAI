# HANDOFF — AdventureBreaker (resume here in a new session)

> **You are Claude, resuming work on AdventureBreaker.** Read this top to bottom;
> it contains everything needed to continue without re-deriving anything. This
> file lives at the repo root on purpose — it's the cold-start entry point.

## TL;DR / status

- **What this is:** an *adversarial playtester* for the [ZorkAI](https://github.com/arsindelve/zorkai)
  engine. It plays **Zork I** / **Planetfall** through the real production HTTP
  backend, follows the walkthrough "spine" to reach deep states, and probes each
  state to break the engine, parser, and especially the **AI narrator**. Goal is
  to *break things*, not to win.
- **Sibling project:** [PlayZork](https://github.com/arsindelve/PlayZork) tries to
  *beat* Zork with an LLM. AdventureBreaker reuses its plumbing ideas (HTTP client,
  backend registry, reporting) but swaps the "how to win" brain for a walkthrough
  spine + an adversarial, judging driver (Claude = the L3 critic).
- **State:** harness is **built, working, and proven against live prod Zork.**
  Full mechanic demonstrated end-to-end: spine navigation → checkpoint → narrator
  probing → deterministic oracles → findings ledger → restore-to-continue.
  **6 findings** recorded in the first deep run (2 medium, 4 low) — see below.
- **Why this handoff exists:** the original build session was scoped to
  `arsindelve/zorkai` only; it could not create/push `arsindelve/AdventureBreaker`
  (git proxy + GitHub MCP both hard-scoped). The repo now exists on GitHub
  (public, default branch `main`). This handoff + the code were carried over via a
  git bundle.

## First moves when resuming

1. **Confirm scope.** This session should have `arsindelve/AdventureBreaker` in
   scope (and ideally `arsindelve/zorkai` too, for engine-source verification).
   If AdventureBreaker isn't in scope, the push/work can't happen — tell the user.
2. **Sanity check the harness still talks to prod:**
   ```bash
   cd <repo>
   export AB_RUNS_DIR=runs
   python3 -m adventurebreaker.harness new --game zork --target prod --name resume
   python3 -m adventurebreaker.harness spine-run --count 6        # narrator OFF, cheap
   ```
   Expect: West Of House → … → Kitchen (score 10). Stdlib-only, no install needed.
3. **Pick up probing** where the last run left off (next ideas in "Backlog").

## Repo layout

```
adventurebreaker/
  config.py     backend registry (Zork/Planetfall × prod/local) + score facts
  client.py     stdlib HTTP client; play/init/save/restore/list; captures status+latency+raw
  models.py     full GameResponse envelope (inventory/exits/actions/time) + Direction int map
  oracles.py    L0 contract / L1 consistency / L2 anchors (deterministic, free)
  ledger.py     Run state, transcript, Markdown+JSON findings
  harness.py    the CLI the interactive driver (you) drives
  spine/        extracted walkthroughs: zork1.json (387 steps), planetfall.json (358)
tools/
  extract_spine.py   one-time spine extractor from ZorkAI [TestCase] fixtures
runs/           GITIGNORED runtime output (transcripts/ledgers/checkpoints) — ephemeral!
```

## How the harness works (the mechanic)

- **Spine = progress.** `spine-run` replays the known-good walkthrough (extracted
  from ZorkAI's NUnit `[TestCase]` fixtures) to drive the game into deep states.
  Each step also carries the test's expected-output substrings, so the spine
  doubles as a **golden transcript** (free L2 regression check).
- **Ribs = attack.** At a state, before advancing, fire adversarial `play`
  commands to break engine/parser/narrator.
- **Fork via save/restore.** `save <name>` checkpoints; probe destructively;
  `restore <id>` rolls the server state back. Proven: a restore rolled moves
  13→9 and removed a taken item. Probing never derails progress.
- **Narrator A/B.** `play` = narrator ON (`NoGeneratedResponses:false`),
  `quiet` = narrator OFF. Off+wrong ⇒ engine bug; on-wrong/off-right ⇒ narrator bug.

### CLI quick reference
```
new --game {zork|planetfall} [--target prod|local] [--name NAME]
use NAME                       # switch current run
state                          # GET session state (structured)
play <cmd...>                  # probe, narrator ON, auto-oracles
quiet <cmd...>                 # engine only, narrator OFF
spine [--from N] [--count M]   # show upcoming spine steps
spine-run [--count N] [--narrator] [--verbose]   # auto-advance via walkthrough
save NAME / saves / restore ID
finding --severity {info,low,medium,high,critical} --category C --title T \
        [--detail D] [--evidence E] [--command CMD] [--repro R]
report
```

## Oracle layers
| Layer | Cost | Catches |
|---|---|---|
| L0 contract | free | non-2xx/5xx, malformed JSON, leaked stack traces, score∉[0,max], empty/silent narrator |
| L1 consistency | free | prose vs envelope: `Taken` but inv didn't grow, moved but loc unchanged, score/moves regress |
| L2 anchors | free | static strings, narrator-leak/character-break heuristics, spine golden-transcript divergence |
| L3 critic | **you** | hallucinated objects/lore, state contradictions, character breaks, injection, spoilers, "that's not right" |

## CRITICAL gotchas (learned the hard way)

- **Verify before logging.** Use the engine source (ZorkAI repo) — and the
  original ZIL when available — as ground truth before recording a narrator
  "bug". This already prevented a false positive: a narrator mention of a
  "gothic door to the west" looked wrong but is real per `ZorkOne/Location/LivingRoom.cs:21`
  (nailed shut, so absent from the live `exits` list). Keep the ledger credible.
- **Check inventory/state before judging.** "give sword to troll → non-existent
  weapon" was *correct* because the sword wasn't held; dying from a wasted turn
  while the troll attacks is *correct* Zork. Don't cry wolf.
- **Narrator calls cost the user's OpenAI budget.** Use `quiet`/`spine-run`
  (narrator OFF) to *advance*; only turn the narrator ON at states you're
  actually attacking.
- **Spine can diverge on live prod.** The walkthrough was deterministic via
  mocked RNG (troll/thief/Floyd/laser). Over prod, RNG is live, so combat/Floyd
  steps may diverge — `spine-run` flags this as a low "spine_divergence" hit;
  adjudicate (bug vs luck) and re-route manually (e.g. re-attack the troll).
- **3 spine steps need god-mode** (reflection state setup) and are NOT
  HTTP-replayable; `spine-run` auto-skips them (`http_replayable:false`).
- **Prod endpoints (in `config.py`):** Zork
  `https://bxqzfka0hc.execute-api.us-east-1.amazonaws.com/Prod/ZorkOne`,
  Planetfall `https://6kvs9n5pj4.execute-api.us-east-1.amazonaws.com/Prod/Planetfall`.
  POST body `{input, sessionId, noGeneratedResponses}` (camelCase; ASP.NET is
  case-insensitive). `exits` come back as **integers** (Direction enum) — mapped
  in `models.py:DIRECTION_BY_INT`.
- **`runs/` is gitignored & ephemeral.** Findings below are embedded here so they
  survive. If you want transcripts to persist, commit them explicitly.

## Findings so far (run `smoke`, prod Zork) — embedded so they survive

**[MEDIUM] GET session-state endpoint returns empty inventory while items are held**
At an identical state (Cellar, score 35, holding the lantern), `GET /ZorkOne?sessionId=`
returns `inventory:[]` but `POST` returns `inventory:['lantern']`. The web client
uses GET to rehydrate on reconnect → returning player sees empty inventory.
Deterministic. Likely the GET/Index(sessionId) path doesn't populate inventory.
**This is the best candidate for a TDD fix via the `find-and-fix-bug` skill in the ZorkAI repo.**

**[MEDIUM] Death has no consequence: inventory not scattered, infinite resurrections**
On death the player keeps full inventory and is resurrected unconditionally.
Confirmed known-incomplete at `ZorkOne/Command/DeathProcessor.cs:29-31`
(`// TODO: Scatter inventory.` + `// TODO: Die ?? number of times…`, cites 1actions.zil).
Original Zork scatters possessions and eventually ends the game; current behavior
removes core risk/tension.

**[LOW] `take it` with no player antecedent silently picks up an item** — pronoun
resolver bound "it" to a noun that appeared only in the *narrator's own flavor
text* the prior turn; `take it` then silently took the sword ("Taken.").

**[LOW] Narrator calls the house "non-existent" while player is inside it** —
in the Living Room (inside the white house), `burn down the house` → "setting fire
to a non-existent house".

**[LOW] Narrator volunteers a puzzle hint unprompted** — `examine the dragon`
deflection suggested "check under the oriental rug" (the rug hides the trap door;
`LivingRoom.cs:24-26`). Spoiler nudge.

**[LOW] Structured `exits` & `actionsAvailableFromLocation` exposed in pitch dark** —
prose says "It's too dark to see!" but envelope still returns `exits=['S','N']`
and the action chips, so the web UI leaks info darkness should hide.

## Backlog / next steps (in rough priority order)

1. **Turn the GET-inventory bug into a PR.** It lives in `arsindelve/zorkai`
   (in scope there). Use the `find-and-fix-bug` skill: failing test → minimal
   fix → PR. Deterministic, no RNG/AI — ideal TDD target.
2. **Add a GET/POST state-parity oracle** to `oracles.py` (auto-catch the class
   of bug above: after a turn, GET should mirror POST's inventory/score/location).
3. **Implement true narrator A/B** (`ab <cmd>`): save → `quiet` → restore →
   `play`, then diff. Currently A/B is manual via save/restore.
4. **Probe library** (`adventurebreaker/probes/`): static adversarial sets —
   injection, pronoun/antecedent, nonsense nouns, verb-on-absent-noun, container
   logic, light/dark, give/throw, scope ("take all"), repetition/verbatim.
5. **Go deeper in Zork:** thief & treasures (trophy case scoring), egg/canary,
   dam/water, mazes (reuse a pathfinder), grue death in the dark.
6. **Planetfall run:** stress the **Floyd** companion conversational narrator
   (richest AI surface) + time-gated events; `new --game planetfall`.
7. **Optional UX:** `rich` TUI (mirror PlayZork display), batch/non-interactive
   mode with an embedded Anthropic critic (`anthropic` extra) for unattended runs.

## Pushing to GitHub (the original blocker)

The repo `arsindelve/AdventureBreaker` exists. To push from a session it must be
**in that session's allowed repositories** (creating it on GitHub is not enough).
The local default branch is `master`; GitHub default is `main`.
```bash
git push -u origin master:main        # add --force if the repo was auto-initialized
```
If you committed this handoff in a session that still lacks scope, hand the user
the bundle route:
```bash
git clone AdventureBreaker.bundle AdventureBreaker && cd AdventureBreaker
git remote add origin https://github.com/arsindelve/AdventureBreaker.git
git push -u origin master:main
```
