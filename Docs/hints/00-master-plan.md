# Master Plan — AI-Generated Hints (Planetfall)

Tracks GitHub issue #144. This is the design of record for the rebuilt hint system.

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
| "Where am I in the solution right now? / what's blocking me?" | **Live `Context` + Repository state** — full world state (see [02](02-state-availability-audit.md)) |
| "Ladder me a vague→specific hint / why does this work / is X a red herring?" | **Invisiclues** — authored as A/B/C progressive hints; covers exploratory & negative questions |

The walkthrough tests (`Planetfall.Tests/Walkthrough/*.cs`) are
`(command, item, expected-response)` triples that pass CI against the actual engine — they
**cannot be wrong for this port**, unlike the invisiclues (which describe the original 1982
game and can diverge from our C# behavior). The walkthrough is the backbone; the invisiclues
are the laddering layer.

---

## 3. Target architecture

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
- **DAG, not a line.** The solution has optional branches (see [01](01-planetfall-puzzle-dag.md));
  model puzzles as a prerequisite graph so "what can I do now / what's blocking me" is answerable.
- **Eval-driven.** Build the regression harness first (failure #2 was invisible because there
  was no eval); measure each stage.

---

## 4. Staged build plan

Planetfall-first; prove it, then generalize. Each stage is independently shippable.

| Stage | Deliverable | Depends on |
|---|---|---|
| **0** | Pre-planning artifacts (this folder) | — |
| **1** | Eval harness: deterministic `(state, question) → expected hint direction` fixtures, red | [04](04-eval-fixture-design.md) |
| **2** | Puzzle DAG as data + the "what's open / what's blocking" query engine | [01](01-planetfall-puzzle-dag.md) |
| **3** | Localization: full-state → progress vector → DAG position | [02](02-state-availability-audit.md) |
| **4** | Grounded retrieval + laddering over walkthrough + invisiclues | 2, 3 |
| **5** | Unwinnable-state detection (known traps first) | [03](03-softlock-candidates.md) |
| **6** | Navigation hints (reuse BFS pathfinding); coverage logging | 4 |
| **7** | Generalize to Zork I; feedback loop (did the hint unstick the player?) | 1–6 |

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

1. **Prerequisite edges** — verify the DAG draft in [01](01-planetfall-puzzle-dag.md):
   true dependency vs incidental walkthrough order. (Claude proposes from ZIL; you confirm.)
2. **Soft-lock list** — confirm/extend the candidates in [03](03-softlock-candidates.md).
3. **Product contracts:**
   - Disclosure: how stuck before revealing how much? Fixed ladder vs frustration-sensing?
   - Voice: hints delivered by Floyd (in-world) or as an out-of-world guide?
   - Fallback order when nothing is grounded: decline silently, or offer a generic nudge?
4. **Architecture sign-off** — server-side-in-ZorkAI, deterministic-first, Planetfall-first. Confirm.
5. **Scope of unwinnable detection** — known-trap list only (tractable) vs general proof (hard).
   Recommend: known-trap list for v1.

---

## 7. Status

- [x] Stage 0 — pre-planning artifacts drafted (this folder)
- [ ] Open decisions answered
- [ ] Stage 1 — eval harness
