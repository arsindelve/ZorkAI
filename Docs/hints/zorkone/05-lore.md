# Zork I Lore Source — existing reference implementation

Unlike Planetfall (whose lore is [extracted from the game's own C# text](../planetfall/05-lore.md)),
Zork I's lore lives in **two external lore PDFs** the owner already has — and there is **already a
working RAG assistant over them**. So the Zork *lore* content gap is effectively **solved**; this doc
records what exists and how it maps into the ZorkAI in-world guide.

## What exists today (OpenAI Assistants API)

Discovered in the owner's OpenAI account (the same account behind the Floyd assistants):

| Assistant | ID | Model | Tool | Role |
|---|---|---|---|---|
| **`ZorkLore`** | `asst_O8FVrzIn31BR7cdJ2oqSNrbP` | gpt-4o-mini | `file_search` | Answers lore/history questions, grounded in the PDFs |
| **`Zork`** | `asst_k7JSNbgQ1qy1j6kdJYO8ByM3` | gpt-4o | `file_search` | Flavor narrator — 1–2 sarcastic sentences, **no hints** |

**Corpus** — vector store "Vector store for Zork" (`vs_uk0fU5H0lcM2ITDsIgyT2mKN`, ~2.3 MB, 2 files):

- **`Encyclopedia Frobozzica.pdf`** (≈ 292 KB)
- **`The History Of Zork.pdf`** (≈ 2.5 MB)

### `ZorkLore` persona (its system instructions)

> "You are the invisible, incorporeal Narrator within … Zork. When the player asks a question, if it's
> about the lore or history of Zork, use file search to answer in one paragraph or less. If the
> question is not about the lore, remind them, sarcastically, that you are just a narrator. If their
> question references anything contemporary, sarcastically remind them this is a fantasy game."

This already encodes, in-world, the two disciplines the in-world guide needs: **ground every answer in
the corpus** (file_search) and **refuse out-of-scope rather than confabulate** (the sarcastic "I'm
just a narrator" deflection).

## Verified live (gpt-4o-mini, file_search over the PDFs)

| Question | Answer (abridged) |
|---|---|
| "What is Double Fanucci?" | "A card game invented by Zilbo III… completed in 670 GUE… declared the National Sport of Quendor under King Mumberthrax the Insignificant." |
| "Who was Megaboz?" | "A powerful… wizard… cursed Lord Dimwit Flathead… in 789 GUE… credited with the downfall of the Great Underground Empire and the creation of the first Dungeon Master." |
| "What's the wifi password?" | "I'm just a narrator in this fantasy game. WiFi passwords aren't exactly in my wheelhouse!" |

Grounded, in-voice, and the out-of-scope guard holds.

## Why RAG is the *right* tool here (and was wrong in the original `planetfall-hints`)

This validates the source-shape distinction in the master plan:

- **Small, structured sources** (the walkthrough; the Planetfall invisiclues; the [extracted Planetfall
  lore digest](../planetfall/05-lore.md)) → **direct, in-context, no retrieval.** The original
  `planetfall-hints` misapplied vector RAG here, which is why it underperformed.
- **Large, unstructured corpus** (these two Zork PDFs, ~2.3 MB of encyclopedia prose) → **chunk + embed
  + retrieve.** This is the case where `file_search`/RAG is genuinely correct — too big for context,
  not pre-structured. The owner's `ZorkLore` assistant is exactly this, done right.

## How it slots into the ZorkAI in-world guide

- **The durable asset is the two PDFs**, not the assistant. The OpenAI assistant is one front-end; the
  ZorkAI design is Claude / in-engine / server-side. Two integration options:
  1. **Lift the corpus** into whatever grounding store the in-world guide uses (re-embed the PDFs;
     retrieve at query time, phrase with our model). Keeps everything in one stack.
  2. **Call `ZorkLore` as a service** for the lore mode (fastest path; keeps the dependency on OpenAI +
     that account).
- The `ZorkLore` **persona is a ready-made spec** for the lore mode's prompt (in-world narrator,
  one-paragraph, ground-or-refuse).

## Gaps vs. the full in-world-guide design (all expected)

1. **No spoiler-tier / discovered-state gating.** `ZorkLore` answers all lore regardless of progress.
   For an encyclopedia this is low-stakes (mostly flavor), but our guide adds the discovered-state gate
   (see [00 architecture](../00-master-plan.md)) — e.g. don't hand out the Dungeon-Master endgame lore
   to a player still in the white house.
2. **Lore mode only — not puzzle hints.** This corpus answers "what/who/why about the world," not
   "what do I do next." The **puzzle-hint laddering source remains the open Zork item** (invisiclues-
   equivalent, or LLM-1-generated rungs from the verified walkthrough). Lore ≠ progress.
3. **External + different model** (OpenAI gpt-4o-mini) vs. the engine's Claude/in-engine grain — an
   integration choice, not a blocker.

## Licensing note

`Encyclopedia Frobozzica` and `The History of Zork` are copyrighted Infocom/Activision lore. Owning the
PDFs for personal grounding is one thing; shipping them inside a product is a deliberate **source +
licensing decision** (master-plan open-decision #3). Flagged, not resolved.

## Net

The Zork **lore** content gap is **closed** — a corpus exists *and* a working grounded assistant proves
the retrieval approach. What remains open for Zork is the **puzzle-hint laddering** source, tracked
separately in [00 § open decisions](../00-master-plan.md#6-open-decisions-yours-to-make).
