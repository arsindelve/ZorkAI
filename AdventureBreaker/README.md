# AdventureBreaker

An **adversarial playtester** for the [ZorkAI](https://github.com/arsindelve/zorkai)
engine. It plays **Zork I** and **Planetfall** through the same production HTTP
backend a real browser uses, follows the game's walkthrough to reach deep
states, and then attacks each state to find anything a player would call a
bug — broken engine behavior, parser mistakes, and especially **AI‑narrator**
slips (hallucinations, state contradictions, character breaks, prompt‑injection,
spoilers, "that's not right").

Its goal is **not** to win. The walkthrough is a GPS to reach interesting
states; the point is to break things along the way.

> Sibling project: [PlayZork](https://github.com/arsindelve/PlayZork) tries to
> *beat* Zork with an LLM. AdventureBreaker reuses PlayZork's plumbing (HTTP
> client, config/backends, pathfinding, reporting) but replaces the "how to
> win" brain with a walkthrough spine + an adversarial, judging driver.

## The mechanic: Spine + Ribs

- **Spine (progress):** replay the known‑good walkthrough (extracted from the
  ZorkAI NUnit `[TestCase]` fixtures) to drive the game into deep, varied
  states. Each step also carries the test's expected output, so the spine
  doubles as a **golden transcript**.
- **Ribs (attack):** at each state, before advancing, fire adversarial inputs
  to break the engine, parser, and narrator.
- **Fork via save/restore:** the backend supports save/restore, so the harness
  checkpoints, probes *destructively*, then restores — probing never derails
  progress.
- **Narrator A/B:** every command can run with the narrator on or off
  (`NoGeneratedResponses`). Off + wrong ⇒ engine bug; on + wrong but off + right
  ⇒ narrator bug.

## Oracles (cheap → expensive)

| Layer | Cost | What it catches |
|---|---|---|
| **L0 contract** | free | non‑2xx/5xx, malformed JSON, leaked stack traces, score out of `[0,max]`, empty/silent narrator |
| **L1 consistency** | free | prose vs. the structured state envelope — `Taken` but inventory didn't grow, moved but location didn't change, score/moves regressions |
| **L2 anchors** | free | known static strings; narrator‑leak/character‑break heuristics; spine golden‑transcript divergence |
| **L3 critic** | the driver (Claude) | hallucinated objects/lore, state contradictions, character breaks, injection compliance, spoilers, "that's not right" |

The full envelope (`inventory`, `exits`, `time`, `actionsAvailableFrom*`) is the
free ground truth the L0/L1 oracles check the prose against. The engine source
(and the original ZIL, read‑only) is the authority for factual disputes — verify
before logging, to keep the ledger credible.

## Findings ledger

Confirmed issues are written per run to `runs/<name>/`:

- `findings.jsonl` / `findings.md` — deduped, severity‑sorted, with repro
- `transcript.jsonl` — every turn: command, full envelope, oracle hits
- `state.json` — session id, turn, spine position (lets the CLI resume)

## Usage

Dependency‑free (Python 3.11+, stdlib only):

```bash
# one-time: extract the walkthrough spines from a local ZorkAI checkout
python3 tools/extract_spine.py --game zork \
  --src /path/to/ZorkAI/ZorkOne.Tests/Walkthrough/WalkthroughTestOne.cs \
  --out adventurebreaker/spine/zork1.json
python3 tools/extract_spine.py --game planetfall \
  --src /path/to/ZorkAI/Planetfall.Tests/Walkthrough/WalkthroughTestOne.cs \
  --out adventurebreaker/spine/planetfall.json

# play & probe (prod by default; --target local for a local backend)
python3 -m adventurebreaker.harness new --game zork
python3 -m adventurebreaker.harness spine-run --count 12        # advance via walkthrough
python3 -m adventurebreaker.harness save checkpoint
python3 -m adventurebreaker.harness play examine the troll      # probe (narrator on)
python3 -m adventurebreaker.harness quiet open mailbox          # engine only (no narrator)
python3 -m adventurebreaker.harness restore <save-id>           # fork back
python3 -m adventurebreaker.harness finding --severity high --category narrator \
    --title "..." --command "..." --detail "..." --evidence "..."
python3 -m adventurebreaker.harness report
```

## Layout

```
adventurebreaker/
  config.py     backend registry (Zork/Planetfall × prod/local) + score facts
  client.py     stdlib HTTP client; play/init/save/restore/list; captures status+latency+raw
  models.py     full GameResponse envelope (incl. inventory/exits/actions/time)
  oracles.py    L0 contract / L1 consistency / L2 anchors
  ledger.py     run state, transcript, Markdown+JSON findings
  harness.py    the CLI the interactive driver drives
  spine/        extracted walkthroughs (zork1.json, planetfall.json)
tools/
  extract_spine.py   one-time spine extractor from ZorkAI [TestCase] fixtures
```
