# AI-Generated Hints — Pre-Planning

Design artifacts for the rebuilt Planetfall hint system (GitHub issue #144).

These are **pre-code planning documents**: analysis drafts and design specs produced
before any implementation, so the build is driven by a verified plan rather than vibes.
Nothing here ships in the engine yet.

## Background

A first attempt was built as an external Python/LangGraph agent
([`planetfall-hints`](https://github.com/arsindelve/planetfall-hints)): a two-stage ReAct
agent over a ChromaDB invisiclues vector store plus a GitHub MCP into the ZorkAI source.
It was ineffective — unanchored retrieval, an architecture that amplified retrieval errors
into confident hallucinations, and no access to live game state. See
[00-master-plan.md](00-master-plan.md) for the full post-mortem and the new approach.

## Documents

| # | Document | What it is | Status |
|---|----------|-----------|--------|
| 00 | [Master plan](00-master-plan.md) | Post-mortem, architecture, staged build plan, completeness checklist, open decisions | Draft |
| 01 | [Planetfall puzzle DAG](01-planetfall-puzzle-dag.md) | Puzzle nodes + provisional prerequisite edges + score-checkpoint map, from the walkthroughs | Draft — **needs edge verification** |
| 02 | [State-availability audit](02-state-availability-audit.md) | What the serialized `Context`/`Repository` exposes for localization | Done |
| 03 | [Soft-lock candidates](03-softlock-candidates.md) | Unwinnable-state traps to detect | Draft — **needs ZIL confirmation** |
| 04 | [Eval fixture design](04-eval-fixture-design.md) | The `(state, question) → expected hint` regression spec | Draft |

## Who does what next

- **Claude can produce/refine:** 01 edges (proposed, from ZIL evidence), 02, 03 candidates, 04 fixtures.
- **You must rule on:** the prerequisite edges (true dependency vs incidental order),
  the soft-lock list, the product contracts (voice, disclosure thresholds), and the architecture sign-off.

See [00-master-plan.md § Open decisions](00-master-plan.md#open-decisions-yours-to-make).
