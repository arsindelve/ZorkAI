# AI-Generated Hints — Pre-Planning

Design artifacts for the rebuilt in-engine hint system, covering **both Planetfall and Zork I**
(GitHub issue #144).

These are **pre-code planning documents**: analysis drafts and design specs produced before any
implementation, so the build is driven by a verified plan rather than vibes. Nothing here ships in
the engine yet.

## Background

A first attempt was built as an external Python/LangGraph agent
([`planetfall-hints`](https://github.com/arsindelve/planetfall-hints)): a two-stage ReAct agent over
a ChromaDB invisiclues vector store plus a GitHub MCP into the ZorkAI source. It was ineffective —
unanchored retrieval, an architecture that amplified retrieval errors into confident hallucinations,
and no access to live game state. The rebuild moves the system **into the ZorkAI engine**, where live
`Context` exists. See [00-master-plan.md](00-master-plan.md) for the full post-mortem.

## Structure

The **architecture is shared** across games (localization → DAG → grounded retrieval → laddering →
soft-lock detection). The **content is per-game** (the puzzle graph, state mapping, soft-locks, and
eval fixtures differ). So:

| Scope | Document |
|---|---|
| **Shared** | [00 — Master plan](00-master-plan.md): post-mortem, architecture, staged build plan, completeness checklist, open decisions |
| **Planetfall** | [01 puzzle DAG](planetfall/01-puzzle-dag.md) · [02 state audit](planetfall/02-state-audit.md) · [03 soft-locks](planetfall/03-softlock.md) · [04 eval](planetfall/04-eval.md) |
| **Zork I** | [01 structure & DAG](zorkone/01-structure-dag.md) · [02 state audit](zorkone/02-state-audit.md) · [03 soft-locks](zorkone/03-softlock.md) · [04 eval](zorkone/04-eval.md) |

## The two games are different shapes

- **Planetfall** — a guided plot: a mostly-linear spine with a few optional branches, **plus
  survival clocks** (sleep / eat / sickness) that are a first-class hint category of their own.
- **Zork I** — an open treasure hunt: a broad, flat graph of independent puzzle chains converging on
  "19 treasures in the trophy case = 350 points." The DAG-not-a-line design matters even more here,
  and Zork's famous unwinnable states make soft-lock detection central.

Both reuse the same engine pipeline; only the per-game data differs.

## Who does what next

- **Claude can produce/refine:** DAG edges (proposed, from ZIL evidence), state audits, soft-lock
  candidates, eval fixtures, and the invisiclues laddering integration.
- **You must rule on:** the prerequisite edges (true dependency vs incidental order), the soft-lock
  lists, the product contracts (voice, disclosure thresholds), the architecture sign-off, and the
  **hint-content source** (the invisiclues content needs to come in-repo for both games — see
  [00 § Open decisions](00-master-plan.md#6-open-decisions-yours-to-make)).
