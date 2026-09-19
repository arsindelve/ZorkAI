# Common Hint Architecture (shared engine + per-game plug-ins)

> **Superseded.** The engine this document designs was built, measured, and replaced by the hint oracle —
> see [08 — The Hint Oracle](08-oracle.md). Kept as the design record.

**Shared doc** (lives alongside [00 master plan](00-master-plan.md)). Defines the *one* game-agnostic
hint engine and the small set of interfaces each game implements to plug in its unique solution. The
per-game content we've already drafted (the `01`–`06` docs for [Planetfall](planetfall/) and
[Zork I](zorkone/)) is exactly the data these plug-ins wrap.

> **Design rule:** the **engine is written once**; a game contributes a **provider** (data + a few
> small implementations). Adding a third game = implement the provider, supply the corpora — **no
> engine change.** Both games reduce to the same abstraction: *node statuses over a DAG, plus
> grounded rung-ladders and lore.*

> ## As built
>
> The implementation (`GameEngine/Hints`, `Planetfall/Hints`) follows this document with these
> departures, all in the direction of *less state and fewer seams*:
>
> - **Stateless.** There is no `IHintMemoryStore`. Each `HintExchange` the client replays carries the
>   `kind`, `topic` and `rung` the engine returned for it; the next rung is one past the highest replayed
>   for that topic, a continuation ("more") follows the kind of the last answer (lore stays lore, a
>   fallback conversation continues as fallback), and "closed" topics are simply those live state reports
>   `Done`. Any Lambda container can answer any request. (`HintMemory.TopicStartMove` has no stateless
>   equivalent, so the frustration floor is death-count only — and capped below the solution rung.)
> - **One LLM seam.** The router is `IHintLanguageModel.Route`, not a separate `IIntentRouter`; it returns
>   the intent, whether the message continues the thread, and — for progress questions — which node from
>   the catalog, or that the question is about something specific the catalog has no puzzle for
>   (`Unlisted`: a dead end, a red herring), which goes to the fallback solver rather than the active
>   blocker's ladder. A bare question (the Hint button) skips routing and continues the thread or hints the
>   active blocker. If the router is unavailable the engine declines; it never guesses "what do I do?".
> - **Progress predicates are monotonic.** Completion is read from inventory, item flags, the systems
>   monitors and `ILocation.VisitCount` (has the player ever entered the Tower Core, the platforms, the
>   kitchen) — never from where they are standing now, which they can walk away from, and never from door
>   flags the game only sets for an explicit `open`. A locked topic redirects to its nearest open
>   prerequisite along its own chain, not the globally first open node.
> - **`ILoreSource` returns text, the engine phrases it.** The provider decides what the player may know
>   (the `05` digest by tier, the invisiclues by area reached, orienting rungs only) and hands back only
>   that; `AnswerLore` answers from it and nothing else. Mechanic questions use the same source. There is
>   no separate `IMechanicExplainer`. When the source says *nothing* about the question (a device, a memo
>   — a question the router misread as lore), `AnswerLore` returns the `NOT_IN_SOURCE` signal and the
>   engine falls through to the puzzle the router saw in the question, or to the solver — it never
>   improvises a story.
> - **LLM-1 is the whole-source solver, and it is the fallback.** `Solve`/`Reveal` over the embedded game
>   source + walkthrough + the tier-gated invisiclues run only when the chosen topic has no authored
>   ladder. Red herrings and dead ends are answered this way, from the invisiclues' own negative answers,
>   rather than from a keyword table.
> - **Fail closed.** A failed phrase call returns the authored rung; a failed lore, solve or reveal call
>   returns empty and the engine declines. Nothing ever returns the complete solution on error.
> - **Locked topics redirect.** Asking about a puzzle whose prerequisites aren't met gets "that's further
>   down the road" plus the real blocker's rung — never the later puzzle's ladder.
> - **The corpus is generated, not hand-written.** Three live evaluation passes along the walkthrough
>   showed that every fix to the hand-written DAG and ladders was a hand-crafted answer for one scenario
>   — not a scalable way to cover a game. So the per-game content (`01` DAG, `02` state map, `06`
>   ladders) is now *derived from the game itself* by `Planetfall.Tests/Hints/Generator`:
>   - **Segments.** Both verified walkthroughs (`WalkthroughTestOne`, `WalkthroughDontFixAnything`) are
>     replayed through the real engine; after every command the whole world is flattened
>     (`StateFlattener`: every Repository item and location, every persisted scalar property). A command
>     that is not a move and leaves a *mark* — some object's state no longer at its default — is a puzzle
>     step; puzzle steps in one room, with the room's waits and looks around them, are one puzzle; the
>     moves between puzzles are the next one's approach, and a long approach to a room never seen before
>     is a navigation puzzle of its own (`REACH_*`).
>   - **Completion predicates** are observed, not authored: the state that changed during the segment and
>     still held when the next puzzle began — preferring objects the segment's own commands name — as
>     `eq` / `gte` / `contains` over the flattened paths (`Item:Magnet.HasEverBeenPickedUp eq True`,
>     `Location:SystemsMonitors.Fixed contains KUMUUNIKAASHUNZ`). Transient signals are fine because
>     `DataPuzzleGraph` back-fills: a done node's prerequisites are done.
>   - **Optional** = absent from the minimal walkthrough (segments aligned in order, exact commands).
>     **Aliases** = the nouns of the segment's commands plus the room's words, for the router's catalog.
>   - **Rungs** are written offline by the model from the segment's exact commands and the engine's actual
>     responses, with a do-not-mention list of every later puzzle's words; rung C is the commands
>     themselves. Fed, not generated.
>   - The output is one reviewable JSON file per game (`Planetfall/Hints/Generated/planetfall-hints.json`,
>     `HintData`), embedded and loaded by `DataPuzzleGraph` / `DataHintCorpus` — the same engine over data.
>     Regenerate with `HintCorpusGenerator.Generate` ([Explicit]; `HINT_GEN_DRY=1` skips the model).
>     The acceptance test is `PlanetfallHintPlaythroughEval.CompareHandAndGenerated_AllSixtyCheckpoints`:
>     does the puzzle the engine chose have the walkthrough's actual next command in its solution rung.
>     First result (2026-09-17, live router): generated **45/60** vs hand-written 39/60 (puzzles 16 vs
>     15, in-between 15 vs 16, mid-puzzle 14 vs 8). The generated corpus is the production default
>     (`new PlanetfallHintProvider()`); `PlanetfallHintProvider.HandWritten()` keeps the baseline.

> **Provider note (locked build decision — [00 §7](00-master-plan.md#7-locked-build-decisions-v1)):**
> the LLM bits (phrasing, intent router, lore answering) run on **OpenAI**, following the C#→OpenAI-
> assistant pattern Floyd already uses — *not* the engine's Claude `IGenerationClient`. Read the
> `IGenerationClient` references below as "the configured LLM client (OpenAI for v1)." The `HintPersona`
> is the single snarky-narrator voice for both games. Disclosure is **frustration-sensing** (rung chosen
> from death count / turns-stuck / failed commands), not the fixed ladder shown in §6.

---

## 1. Component map

```
┌───────────────────────── ENGINE (game-agnostic, GameEngine.Hints) ─────────────────────────┐
│                                                                                              │
│  HintService.GetHint(request)                                                                │
│     ├─ IIntentRouter         classify: Progress | Mechanic | Lore | OutOfScope               │
│     ├─ IHintMemoryStore      per-session rung memory (read-write, separate from SavedGame)   │
│     ├─ progressive-disclosure state machine (rung counter, "more")                           │
│     ├─ two-tier LLM           LLM-1 rung-gen (fallback) · LLM-2 phraser (always)             │
│     └─ proactive channel      runs ProactiveRules each turn (push, read-only)                │
│                                                                                              │
│   consumes the live game state READ-ONLY (IContext + Repository) — never mutates, no turn    │
└───────────────────────────────────────────┬──────────────────────────────────────────────────┘
                                             │ IHintProvider  (the single plug point)
                ┌────────────────────────────┴────────────────────────────┐
        ZorkOne.Hints.ZorkOneHintProvider               Planetfall.Hints.PlanetfallHintProvider
          • IPuzzleGraph (treasure-hunt DAG)               • IPuzzleGraph (spine + ★ branches)
          • IProgressMapper (treasure find/deposit         • IProgressMapper (node flags +
            + gates → ProgressState)                          survival clocks → ProgressState)
          • IHintCorpus (zorkone/06 ladders)               • IHintCorpus (planetfall/06 ladders)
          • ILoreSource (PDF RAG / ZorkLore)               • ILoreSource (planetfall/05 in-context, tiered)
          • ISoftLockRule[] (egg, gas, boat…)              • ISoftLockRule[] (Floyd, disease, laser…)
          • IProactiveRule[] (none)                        • IProactiveRule[] (sleep/eat/sickness)
          • HintPersona (sarcastic narrator)               • HintPersona (in-world / Floyd-flavored)
```

The provider is reached through the existing game type:

```csharp
public interface IInfocomGame            // existing engine interface
{
    // … existing members …
    IHintProvider Hints { get; }         // NEW — the game's hint plug-in
}
```

---

## 2. The plug point: `IHintProvider`

One interface groups everything a game supplies. All members are data or small pure functions —
easy to test, no engine logic.

```csharp
public interface IHintProvider
{
    IPuzzleGraph              PuzzleGraph    { get; }   // the DAG: nodes + prereqs + optional/goal
    IProgressMapper           ProgressMapper { get; }   // live state  -> ProgressState
    IHintCorpus               PuzzleCorpus   { get; }   // node id      -> rung ladder (the 06 docs as data)
    ILoreSource               LoreSource     { get; }   // lore/world Q&A (05 in-context, or RAG over PDFs)
    IReadOnlyList<ISoftLockRule>  SoftLockRules  { get; }
    IReadOnlyList<IProactiveRule> ProactiveRules { get; }   // survival/stuck nudges (may be empty)
    HintPersona               Persona        { get; }   // voice for the phrasing LLM
}
```

---

## 3. The abstraction that makes it game-agnostic: `ProgressState`

Both games — a linear-ish plot and an open treasure hunt — reduce to **per-node status over a DAG**.
That's the whole trick: the engine only ever reasons about node statuses; the game-specific *reading*
of live state lives behind `IProgressMapper`.

```csharp
public enum NodeStatus { Locked, Available, Done }      // Locked = prerequisites unmet

public sealed record ProgressState(
    IReadOnlyDictionary<string, NodeStatus> Nodes,      // NodeId -> status
    IReadOnlyDictionary<string, object>     Extras);    // game-specific extras (survival levels, etc.)

public interface IProgressMapper
{
    ProgressState Map(IContext liveState);              // READ-ONLY over Context + Repository
}
```

- **Zork** maps each treasure as a **find/deposit node pair** (`FIND_DIAMOND` Done when held,
  `DEPOSIT_DIAMOND` Done when in the case) plus a node per access gate — so "held but not deposited" is
  just `FIND_*=Done, DEPOSIT_*=Available`. Score is a tiebreaker, never the key (and `GameOver` is **not**
  `Score==350` — see [zorkone/02](zorkone/02-state-audit.md)).
- **Planetfall** maps each DAG node from its milestone flags (`SystemsMonitors.CommunicationsFixed`,
  `Relay.SpeckDestroyed`, Floyd liveness, card locations — see [planetfall/02](planetfall/02-state-audit.md))
  and puts survival-clock levels in `Extras`.

The graph reasons over `ProgressState` generically:

```csharp
public interface IPuzzleGraph
{
    IReadOnlyCollection<PuzzleNode> Nodes { get; }
    IReadOnlyCollection<string> OpenSet(ProgressState s);                 // Available & not Done
    IReadOnlyCollection<string> ActiveBlockers(ProgressState s, IContext where); // gating nodes near the player
}
public sealed record PuzzleNode(string Id, string[] Prerequisites, bool Optional, string Title, string Location);
```

---

## 4. The content interfaces (wrap the 05/06 corpora)

```csharp
// Puzzle-hint rungs — the planetfall/06 & zorkone/06 ladders, as data.
public interface IHintCorpus { bool TryGetLadder(string nodeId, out RungLadder ladder); }
public sealed record RungLadder(string NodeId, IReadOnlyList<string> Rungs);   // [A nudge, B approach, C solution, …]

// Lore / world Q&A — the in-world-guide's lore mode.
public interface ILoreSource
{
    Task<LoreAnswer> Answer(string question, SpoilerTier maxTier, IGenerationClient llm);
}
public enum SpoilerTier { Observable, Environmental, Investigated, Endgame }   // T0..T3, gated by discovered state
public sealed record LoreAnswer(bool Grounded, string Text);
```

- **Planetfall `ILoreSource`** = the [in-repo digest](planetfall/05-lore.md) held in context, filtered to
  `maxTier`. Small + structured → no retrieval.
- **Zork `ILoreSource`** = RAG over the two lore PDFs (or call the existing `ZorkLore` assistant). Large +
  unstructured → retrieval is the right tool. Same interface, different implementation — the engine
  doesn't care which.

---

## 5. Soft-locks and proactive nudges (per-game rules)

```csharp
public enum SoftLockKind { None, Warning, BestEndingOnly, Hard }   // Hard => "restore"; Warning/BestEndingOnly => caveat
public sealed record SoftLockVerdict(SoftLockKind Kind, string? Message);
public interface ISoftLockRule { SoftLockVerdict Evaluate(IContext liveState, ProgressState progress); }

public sealed record ProactiveNudge(string Category, string Message, int Priority);
public interface IProactiveRule { ProactiveNudge? Evaluate(IContext liveState); }  // null = nothing to surface
```

- Soft-lock rules are the predicates from the `03` docs (Zork: egg force-open, gas-room flame, boat
  punctures; Planetfall: Floyd-dead-before-biolock, disease clock, laser battery).
- Proactive rules are the **push channel**: Planetfall supplies sleep/eat/sickness ([planetfall/01 §survival](planetfall/01-puzzle-dag.md));
  Zork supplies an empty list (no survival clocks). The engine evaluates them read-only each turn and
  surfaces nudges in the Hints panel — without touching the game transcript.

---

## 6. The engine pipeline: `HintService.GetHint`

```csharp
public sealed record HintRequest(string SessionId, IContext StateSnapshot, string? Question, bool More, string? Topic);
public sealed record HintResponse(HintKind Kind, string Text, string? Topic, int Rung, int TotalRungs, SoftLockKind SoftLock);

public sealed class HintService            // game-agnostic; one instance per game via DI
{
    public HintService(IHintProvider provider, IHintMemoryStore memory, IGenerationClient llm, IIntentRouter router);
    public Task<HintResponse> GetHint(HintRequest request);
}
```

Inside `GetHint` (read-only over `StateSnapshot` throughout):

1. **Route intent.** `More==true` or the explicit Hint button ⇒ `Progress`. Otherwise
   `router.Classify(question)` ⇒ `Progress | Mechanic | Lore | OutOfScope`.
2. **Map progress.** `progress = provider.ProgressMapper.Map(snapshot)` — fresh every call.
3. **Branch:**
   - **OutOfScope** → persona-flavored decline (the "I'm just a narrator" guard).
   - **Lore** → `provider.LoreSource.Answer(question, TierFrom(progress), llm)`; if `!Grounded` → "you
     haven't discovered that yet" nudge. Spoiler tier derived from discovered state.
   - **Mechanic** ("why am I sick / why can't I carry this") → explain from `snapshot` + provider (a thin
     mechanic explainer; for Planetfall it reads the survival clocks, points gently at the goal).
   - **Progress** →
     a. **Soft-lock check.** Run `provider.SoftLockRules`; a `Hard` verdict short-circuits to a
        restore message; `Warning`/`BestEndingOnly` attach a caveat.
     b. **Pick topic.** `request.Topic ?? provider.PuzzleGraph.ActiveBlockers(progress, snapshot).First()`.
     c. **Rung (memory).** `mem = memory.Load(sessionId)`. If `request.More` → advance
        `mem.RungReached[topic]`; if topic is a fresh subject → start at rung 0; **if the topic is now
        `Done` in `progress` → close it and re-pick** (memory drives laddering, *live state drives
        localization*).
     d. **Get rungs.** `provider.PuzzleCorpus.TryGetLadder(topic)`; if absent → **LLM-1** generates a
        ladder from the verified solution (fallback only).
     e. **Phrase.** **LLM-2** renders `ladder.Rungs[rung]` in `provider.Persona` — one rung, bounded by
        what's in its context (can't over-reveal).
     f. **Persist.** Save `mem`; return rung / totalRungs so the UI can show "Hint 2 of 3".

```csharp
public enum HintIntent { Progress, Mechanic, Lore, OutOfScope }
public interface IIntentRouter { Task<HintIntent> Classify(string question, IGenerationClient llm); }

public interface IHintMemoryStore                       // separate store, NOT the SavedGame
{
    Task<HintMemory> Load(string sessionId);
    Task Save(string sessionId, HintMemory memory);
}
public sealed class HintMemory
{
    public string? ActiveTopic;
    public Dictionary<string,int> RungReached = new();  // per-topic rung position (persists across sessions)
    public HashSet<string> Closed = new();              // topics the player has since solved
}
```

---

## 7. Where the two-tier LLM lives

- **LLM-2 (phraser)** — engine-side, always used; input = one rung + `provider.Persona`. The persona is the
  only game-specific piece (Planetfall in-world/Floyd-flavored; Zork sarcastic narrator, lifted from the
  `ZorkLore` instructions). It cannot over-reveal because it only holds the current rung.
- **LLM-1 (rung generator)** — engine-side, **fallback only**, invoked when `PuzzleCorpus` has no ladder
  for a node. Input = the *verified* solution (walkthrough step), never a blank page → it re-shapes ground
  truth, it doesn't invent it. With the `06` corpora authored, LLM-1 rarely fires for these two games.

This is the "fed, not generated" rule made structural: the progression is authored data; the LLM only
phrases it.

---

## 8. Front doors & state access (engine-side)

The hint subsystem is **independent and read-only over game state** — asking for help never consumes a
turn or advances Planetfall's clocks.

- **`POST /hint`** — loads the session's `SavedGame` snapshot, runs `HintService`, writes only to
  `IHintMemoryStore`. Stateless, fits the existing Lambda pattern.
- **Hints panel** (web) — primary UI; the "More" button = `HintRequest{More:true}`; shows the rung
  counter; surfaces proactive nudges. (`shared-web-types` lets both clients share it.)
- **`hint` text command** — thin fallback front door for console/pure-text play; same `HintService`.

`HintMemory` persists as a per-session record alongside the save (not inside it), so "I need more help"
resumes the ladder across sessions.

---

## 9. What's reused vs. new

| Reused (exists today) | New (this design) |
|---|---|
| `IContext` + `Repository` serialization (`TypeNameHandling.All`) → the read-only snapshot | `GameEngine.Hints`: `HintService`, `IHintProvider` + the interfaces above, `IHintMemoryStore`, `IIntentRouter` |
| `IInfocomGame` registration → add `Hints` member | `ZorkOne.Hints` / `Planetfall.Hints` providers (wrap the `01`–`06` content as data) |
| `IGenerationClient` (the AI client Floyd uses) → LLM-1/LLM-2 | `/hint` endpoint + Hints panel + text `hint` command |
| Walkthrough tests → the `06` corpora; `05`/PDFs → lore | DynamoDB hint-memory record (per session) |
| `IRandomChooser`-style DI for deterministic tests | Eval harness ([04 docs](planetfall/04-eval.md)) drives `HintService` with real providers + a stubbed LLM |

---

## 10. Adding a third game

1. Implement `IHintProvider` in `<Game>.Hints` — an `ILoreSource`, `ISoftLockRule[]`, `IProactiveRule[]`
   (possibly empty), a `HintPersona`, and the fallback solver's docs.
2. Generate the puzzle graph, state map and ladders: point a copy of `HintCorpusGenerator` at that game's
   full and minimal walkthrough tests, review the JSON, check it in, and load it with `DataPuzzleGraph` /
   `DataHintCorpus`. Author only what cannot be observed (the `05` lore digest and its tiers, the `03`
   soft-lock flags).
3. Register it via `IInfocomGame.Hints`.

No change to `HintService`, the memory store, the LLM tiers, the front doors, or the eval harness. That
is the test of this design: **the unique Zork and Planetfall solutions plug in; the engine never forks.**
