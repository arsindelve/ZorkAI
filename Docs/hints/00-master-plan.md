# Master Plan — AI-Generated Hints

Tracks GitHub issue #144. This is the design of record for the rebuilt hint system, covering
**both Planetfall and Zork I**. The architecture below is shared; per-game content lives in
[`planetfall/`](planetfall/) and [`zorkone/`](zorkone/).

---

## 1. Post-mortem: why the first attempt failed

The first cut ([`planetfall-hints`](https://github.com/arsindelve/planetfall-hints)) was an
external Python/LangGraph two-stage agent (a ReAct "research" stage over a ChromaDB
invisiclues vector store + a GitHub MCP into the ZorkAI source, then a hint-presenter stage).
It was ineffective for three structural reasons:

1. **Unanchored retrieval.** Players ask vaguely/situationally ("stuck in the rec area, what
   now?"); the invisiclues are terse canned Q&A. With `text-embedding-3-small` and **no score
   threshold**, `similarity_search` always returned k chunks even when all were irrelevant.
2. **The architecture amplified retrieval errors.** The research prompt ("provide the FULL
   answer, don't hold back") fed thin/wrong context to a cheap model (`claude-3-5-haiku`),
   which confabulated; the second stage faithfully repackaged the hallucination with no way to
   verify it. Two LLM hops compounded the first stage's error instead of catching it.
3. **No live game state.** The external agent never saw the player's actual `Context`, so the
   *only* thing it could do was fuzzy semantic lookup — the weakest tool for the most important
   job (figuring out where the player is and what's blocking them).

**Lesson:** we were using an external, fuzzy corpus for a problem we already have *verified,
in-repo* data for, and we had no grounding guarantee, so confident wrong hints were the
default failure mode.

---

## 2. Key insight: three sources, each for a different job

| Question type | Best source |
|---|---|
| "What's the correct next move, valid in *this* engine?" | **Walkthrough tests** — verified against the real engine, ordered, so prerequisites/timing are encoded for free |
| "Where am I in the solution right now? / what's blocking me?" | **Live `Context` + Repository state** — full world state (see state audits: [Planetfall](planetfall/02-state-audit.md) · [Zork I](zorkone/02-state-audit.md)) |
| "Ladder me a vague→specific hint / why does this work / is X a red herring?" | **Invisiclues** — authored as A/B/C progressive hints; covers exploratory & negative questions |

The walkthrough tests (`Planetfall.Tests/Walkthrough/*.cs`, `ZorkOne.Tests/Walkthrough/*.cs`) are
`[TestCase]` rows feeding a helper `Walkthrough(string input, string? setup, params string[]
expectedResponses)` — i.e. `(input command, optional god-mode setup, one-or-more expected response
substrings)`, run in order, each asserted against the actual engine — so they
**cannot be wrong for this port**, unlike the invisiclues (which describe the original 1980s
games and can diverge from our C# behavior). The walkthrough is the backbone; the **invisiclues
remain the laddering layer for both games** (and for Planetfall's survival hints — see §5).

---

## 3. Target architecture

> **The concrete plug-in design is its own doc: [07 — Common architecture](07-common-architecture.md)**
> (shared engine + the `IHintProvider` interface each game implements). This section is the high-level
> sketch; 07 has the interfaces, the engine pipeline, and how Zork/Planetfall plug in without forking
> the engine.

Rebuild **server-side inside ZorkAI**, where live `Context` exists — this is the structural
fix that made the external RAG necessary in the first place.

```
player asks for a hint
        │
        ▼
[1] Is this a hint request?  ──no──▶ normal command path
        │ yes
        ▼
[2] LOCALIZE: read full world state (Context + Repository), compute the
    player's "progress vector" of milestone flags → position in the puzzle DAG
        │
        ▼
[3] UNWINNABLE CHECK: is the game already in a soft-locked state? ──yes──▶ "restore an earlier save"
        │ no
        ▼
[4] BLOCKER INFERENCE: which DAG node is the active gate (prereqs met, not yet done)?
        │
        ▼
[5] GROUNDED RETRIEVAL: pull the matching walkthrough step(s) + invisiclue ladder
        │  (if nothing grounds it → honest "I don't have a hint for that")
        ▼
[6] LADDER: reveal one rung; "more" escalates; never over-reveal; never leak future puzzles
        │  (LLM used ONLY for phrasing/laddering over verified content — never to invent it)
        ▼
       hint
```

**Principles**
- **Deterministic-first.** Steps 2–5 are code, not the model. The LLM only phrases/ladders.
- **Grounding guarantee.** Every hint traces to a walkthrough step or invisiclue entry, or the
  system declines. The model never invents content. (This is the architectural fix for failure #2.)
- **DAG, not a line.** The solution has optional branches (Planetfall) or many parallel independent
  chains (Zork I) — see the per-game DAGs ([Planetfall](planetfall/01-puzzle-dag.md) ·
  [Zork I](zorkone/01-structure-dag.md)); model puzzles as a prerequisite graph so "what can I do
  now / what's blocking me" is answerable.
- **Eval-driven.** Build the regression harness first (failure #2 was invisible because there
  was no eval); measure each stage.

---

## 4. Staged build plan

Both games are in scope. **Build Planetfall first** (it has the worked-out DAG and the survival-clock
category), proving the shared pipeline, then apply the same engine to Zork's per-game data. Each
stage is independently shippable. The shared **engine** (localization, DAG query, retrieval,
laddering, soft-lock framework) is written once; each game supplies its own **data** (DAG, state map,
soft-locks, fixtures, invisiclues content).

| Stage | Deliverable | Per-game artifact |
|---|---|---|
| **0** | Pre-planning artifacts (this folder) | — |
| **1** | Eval harness: deterministic `(state, question) → expected hint` fixtures, red | [PF 04](planetfall/04-eval.md) · [Z1 04](zorkone/04-eval.md) |
| **2** | DAG as data + the "what's open / what's blocking" query engine (shared engine, per-game graph) | [PF 01](planetfall/01-puzzle-dag.md) · [Z1 01](zorkone/01-structure-dag.md) |
| **3** | Localization: full-state → progress vector → DAG position | [PF 02](planetfall/02-state-audit.md) · [Z1 02](zorkone/02-state-audit.md) |
| **4** | Grounded retrieval + laddering over walkthrough + invisiclues (incl. PF survival hints) | both |
| **5** | Unwinnable-state detection (known traps first) | [PF 03](planetfall/03-softlock.md) · [Z1 03](zorkone/03-softlock.md) |
| **6** | Navigation hints (reuse BFS pathfinding); coverage logging | both |
| **7** | Second game on the same engine; feedback loop (did the hint unstick the player?) | both |

---

## 5. Completeness checklist

What a *complete, satisfying* hint system needs, beyond the happy path. Ordered by impact.

**Correctness core**
- [ ] **Full-state blocker inference** — infer the *active gate* from the whole world state, not
      score+location. Score is coarse; two players at score 12 can be blocked on different things.
- [ ] **DAG / branch modeling** — optional systems + multiple open puzzles; don't insist on path A
      when the player is on path B.
- [ ] **Unwinnable / soft-lock detection** — detect "you can't win from here, restore" instead of
      hinting toward an impossible solution. The most *satisfying* feature; the most neglected.

**Trustworthiness**
- [ ] **Grounding guarantee** — never emit an ungrounded hint; decline instead of confabulating.
- [ ] **Eval / regression harness** — deterministic fixtures; the thing that turns "felt fine" into "known-good."
- [ ] **Coverage hierarchy + uncovered-question logging** — graceful fallback order; log every miss.

**Game-specific categories**
- [ ] **Survival-clock hints (Planetfall)** — sleep / eat / sickness are a first-class hint category,
      triggered by clock state, not map position. Proactive nudges + on-demand "how do I handle being
      tired/hungry/sick?", laddered from the invisiclues. See [PF 01 § Survival-clock hints](planetfall/01-puzzle-dag.md).
- [ ] **Treasure three-state tracking (Zork I)** — every treasure is `unfound | held | deposited`;
      don't re-hint "find X" when it's in the player's pack. See [Z1 01](zorkone/01-structure-dag.md).

**Finished feel**
- [ ] **Navigation hints** — "the thing you need is in a room you haven't visited" (reuse BFS pathfinding).
- [ ] **Progressive-disclosure contract** — one rung at a time; never over-reveal; don't re-hint done work.
- [ ] **Feedback loop** — did the player progress within N turns of the hint?
- [ ] **Voice** — in-world (Floyd) vs fourth-wall; deliberate, not accidental.

The three that most determine success: **full-state blocker inference**, **unwinnable detection**,
**eval harness**.

---

## 6. Open decisions (yours to make)

These gate the build; capture answers here.

1. **Prerequisite edges + Zork treasure→chain mapping** — verify the DAG drafts:
   [Planetfall](planetfall/01-puzzle-dag.md) and [Zork I](zorkone/01-structure-dag.md) — true
   dependency vs incidental order. For Zork specifically, the **treasure→chain mapping** in the
   structure doc is still placeholder-grade (e.g. the Maze row) and the access-gate set is provisional;
   both need an owner walk-through before the eval-fixture sizing derived from them is trusted.
   (Claude proposes from ZIL; you confirm.)
2. **Soft-lock lists** — confirm/extend the candidates: [Planetfall](planetfall/03-softlock.md) and
   [Zork I](zorkone/03-softlock.md). (Zork's egg/canary trap is already confirmed in code.)
3. **Hint-content source — now built for both games (in-repo, walkthrough-derived).** ✅ The
   progress-hint corpora exist: 3-rung (nudge → approach → solution) ladders per puzzle, authored from
   the verified walkthroughs — [Planetfall 06](planetfall/06-puzzle-solution-corpus.md) (80-pt path) and
   [Zork I 06](zorkone/06-puzzle-solution-corpus.md) (350-pt path). Both are derived from in-repo test
   content, so they're correct-by-construction for this port with no external-corpus or licensing
   question — they are the invisiclues-equivalent. **Remaining (optional, not blocking):** the external
   Planetfall invisiclues can be folded in later as supplementary phrasing/coverage, but are no longer
   required to ship progress hints. Lore sources are also both sourced (see below).
   - **Lore content (in-world-guide direction) — both games now sourced:**
     - *Planetfall:* **buildable from the port's own descriptive text** — see [05 lore source](planetfall/05-lore.md),
       a proof-of-concept corpus extracted from the library terminal, diary, and item descriptions,
       with spoiler tiers. Open call: curated-doc vs live-retrieval-from-descriptions.
     - *Zork I:* **already exists** — two lore PDFs (`Encyclopedia Frobozzica`, `The History of Zork`)
       and a working RAG assistant (`ZorkLore`, file_search) — see [zorkone 05 lore source](zorkone/05-lore.md).
       The Zork *lore* gap is **closed**; open calls are integration (lift corpus in-engine vs call the
       assistant) and PDF **licensing**. The Zork *puzzle-hint* source above remains separate.
4. **Product contracts:**
   - Disclosure: how stuck before revealing how much? Fixed ladder vs frustration-sensing?
   - Voice: in-world (Floyd in Planetfall) vs out-of-world guide? (Zork has no companion narrator.)
   - Fallback order when nothing is grounded: decline silently, or offer a generic nudge?
5. **Architecture sign-off** — server-side-in-ZorkAI, deterministic-first, shared-engine/per-game-data,
   Planetfall-first. Confirm.
6. **Scope of unwinnable detection** — known-trap list only (tractable) vs general proof (hard).
   Recommend: known-trap list for v1 (especially important for Zork, which is full of them).

---

## 7. Locked build decisions (v1)

Owner decisions, captured to drive the build. These **supersede the recommendations** in §6.

1. **LLM / provider: all OpenAI.** Hint phrasing, the intent router, and lore answering all go through
   OpenAI (consistent with the existing Floyd assistants and `ZorkLore`) — **not** the engine's Claude/
   Bedrock generation path. The Planetfall provider follows the existing C#→OpenAI-assistant pattern that
   Floyd already uses. (Updates [07 architecture](07-common-architecture.md), which had sketched Claude.)
2. **Voice: always the snarky narrator — both games, never Floyd.** A single sarcastic, incorporeal-
   narrator persona (the `ZorkLore` style) is the `HintPersona` for Planetfall too. Floyd is never the
   hint voice.
3. **Disclosure: frustration-sensing.** Escalate the rung based on state signals — death count, turns
   since progress, repeated failed/`can't` commands — rather than a fixed ladder. **Deterministic-
   testable** because the signals are read from state, so eval fixtures set them explicitly.
4. **v1 scope: all three modes, Planetfall only, API only.**
   - **All three answer modes** in v1 — progress (puzzle hints), lore (world Q&A), mechanic (why am I
     sick/stuck). The intent router + `ILoreSource` + mechanic path are real in v1, not stubbed.
   - **Planetfall provider only** — no Zork provider yet (architecture stays generic; only Planetfall is
     registered).
   - **API only** — the `POST /hint` endpoint is the front door; **no web UI panel yet** (the eval
     harness and the endpoint exercise the engine).

**What v1 therefore is:** the shared `GameEngine.Hints` engine (all three modes, frustration-sensing
disclosure, persistent `HintMemory`) + `PlanetfallHintProvider` + the `/hint` API, all phrased/routed via
OpenAI in the snarky-narrator voice. Zork and the web UI are deferred behind the same interfaces.

---

## 8. Status

- [x] Stage 0 — pre-planning artifacts drafted (this folder)
- [x] Build decisions locked (§7)
- [ ] Stage 1 — eval harness (Planetfall; all three modes; frustration signals set per-fixture)
- [ ] Stage 2 — engine + `PlanetfallHintProvider` + `/hint` API
