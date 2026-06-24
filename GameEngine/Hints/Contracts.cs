using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

// =====================================================================================
// Two-tier hint architecture (Docs/hints/07). The engine is game-agnostic; a game supplies
// an IHintProvider. The intelligence lives in two LLM calls, not hard-coded tables:
//   LLM 1 (solver)   : all docs + live player context  -> the COMPLETE solution to the situation.
//   LLM 2 (revealer) : player context + solution + chat history -> what to reveal NEXT (paced).
// Grounding is by provision (LLM 1 reasons over the supplied docs, never its own memory);
// progressive disclosure is the chat history, not a counter.
// =====================================================================================

/// <summary>The single per-game plug point. All of it is data the two LLM calls reason over.</summary>
public interface IHintProvider
{
    /// <summary>
    ///     Everything the solver may need, as text: the verified solution walkthrough, the lore, the
    ///     known dead ends / red herrings, the death traps, the goal. Fed to LLM 1 wholesale (the
    ///     Planetfall corpus is small enough to sit in context — no retrieval needed).
    /// </summary>
    string Docs { get; }

    /// <summary>
    ///     A readable snapshot of the player's live situation (location, inventory, key flags such as
    ///     Floyd alive/dead and which systems are fixed, day, survival levels, score). Built fresh and
    ///     read-only from <see cref="IContext" /> each request — this is what keeps answers context-aware.
    /// </summary>
    string DescribePlayerContext(IContext state);

    /// <summary>Voice for both LLM calls (v1: one snarky incorporeal narrator).</summary>
    HintPersona Persona { get; }

    /// <summary>Proactive (push) nudges — survival clocks etc. Evaluated read-only; surfaced by the UI.</summary>
    IReadOnlyList<IProactiveRule> ProactiveRules { get; }
}

/// <summary>A proactive (push) nudge — survival clocks, "you seem stuck", etc.</summary>
public sealed record ProactiveNudge(string Category, string Message, int Priority);

/// <summary>A per-game rule that may surface a proactive nudge based on live state.</summary>
public interface IProactiveRule
{
    ProactiveNudge? Evaluate(IContext liveState);
}

// ---- request / response -------------------------------------------------------------

/// <summary>
///     A hint request. The engine treats <see cref="StateSnapshot" /> as read-only — asking for a
///     hint consumes no turn and mutates no game state.
/// </summary>
public sealed record HintRequest(string SessionId, IContext StateSnapshot, string Question);

public sealed record HintResponse(string Text);
