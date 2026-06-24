using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

// =====================================================================================
// Common (game-agnostic) hint engine contracts.
// See Docs/hints/07-common-architecture.md for the design. A game contributes one
// IHintProvider; the engine (HintService) is written once and never forks per game.
// =====================================================================================

/// <summary>Status of a single puzzle node, computed from live game state.</summary>
public enum NodeStatus
{
    /// <summary>Prerequisites are not yet met.</summary>
    Locked,

    /// <summary>Prerequisites met, but the player hasn't done it yet — a candidate "next move".</summary>
    Available,

    /// <summary>Completed.</summary>
    Done
}

/// <summary>
///     The game-agnostic reduction of live state: a status per puzzle-DAG node, plus a bag of
///     game-specific extras (e.g. survival-clock levels). Both Zork and Planetfall reduce to this.
/// </summary>
public sealed record ProgressState(
    IReadOnlyDictionary<string, NodeStatus> Nodes,
    IReadOnlyDictionary<string, object> Extras);

/// <summary>A node in the puzzle dependency graph (DAG).</summary>
public sealed record PuzzleNode(
    string Id,
    string[] Prerequisites,
    bool Optional,
    string Title,
    string Location);

/// <summary>The game's puzzle graph: nodes + prerequisite edges, reasoned over generically.</summary>
public interface IPuzzleGraph
{
    IReadOnlyCollection<PuzzleNode> Nodes { get; }

    /// <summary>Nodes that are Available (prereqs met) but not Done — the open set.</summary>
    IReadOnlyCollection<string> OpenSet(ProgressState state);

    /// <summary>
    ///     The node(s) most likely gating the player right now, ordered best-first. Uses the live
    ///     state for location proximity / recent trajectory.
    /// </summary>
    IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState);
}

/// <summary>Maps live game state (read-only) to the game-agnostic ProgressState.</summary>
public interface IProgressMapper
{
    ProgressState Map(IContext liveState);
}

/// <summary>An ordered set of progressively-more-specific hint rungs for one puzzle node.</summary>
public sealed record RungLadder(string NodeId, IReadOnlyList<string> Rungs);

/// <summary>The authored puzzle-hint corpus (the planetfall/06 &amp; zorkone/06 ladders, as data).</summary>
public interface IHintCorpus
{
    bool TryGetLadder(string nodeId, out RungLadder ladder);
}

/// <summary>Spoiler tiers — lore answers are gated to what the player has discovered.</summary>
public enum SpoilerTier
{
    Observable = 0,    // T0 — visible immediately
    Environmental = 1, // T1 — read the diary / examined nearby items
    Investigated = 2,  // T2 — used the library terminal etc.
    Endgame = 3        // T3 — completed / reached the finale
}

/// <summary>A lore/world answer; Grounded=false means "decline / not yet discovered" rather than confabulate.</summary>
public sealed record LoreAnswer(bool Grounded, string Text);

/// <summary>
///     The lore/world-knowledge source (in-context digest, or RAG over a corpus). The game-specific
///     implementation derives its own spoiler tier from the live state / progress, so the engine stays
///     game-agnostic. Returns Grounded=false to decline (e.g. "you haven't discovered that yet").
/// </summary>
public interface ILoreSource
{
    Task<LoreAnswer> Answer(string question, IContext liveState, ProgressState progress, IHintLanguageModel llm);
}

/// <summary>
///     Answers "mechanic" questions — "why am I sick?", "why can't I carry this?" — grounded in the
///     live state + the game's own rules (not the puzzle corpus, not lore). Returns Grounded=false to
///     defer when it doesn't recognise the question.
/// </summary>
public interface IMechanicExplainer
{
    Task<LoreAnswer> Explain(string question, IContext liveState, IHintLanguageModel llm);
}

public enum SoftLockKind
{
    None,
    Warning,        // recoverable; surface as a caution
    BestEndingOnly, // can still finish, but the best ending/score is foreclosed
    Hard            // victory now impossible — advise restore
}

public sealed record SoftLockVerdict(SoftLockKind Kind, string? Message)
{
    public static readonly SoftLockVerdict None = new(SoftLockKind.None, null);
}

/// <summary>A per-game predicate detecting one unwinnable/soft-locked situation.</summary>
public interface ISoftLockRule
{
    SoftLockVerdict Evaluate(IContext liveState, ProgressState progress);
}

/// <summary>A proactive (push) nudge — survival clocks, "you seem stuck", etc.</summary>
public sealed record ProactiveNudge(string Category, string Message, int Priority);

/// <summary>A per-game rule that may surface a proactive nudge based on live state.</summary>
public interface IProactiveRule
{
    ProactiveNudge? Evaluate(IContext liveState);
}

/// <summary>
///     The single per-game plug point. A game supplies data + a few small implementations; the
///     engine consumes only this. Registered via <see cref="IInfocomGame" />.
/// </summary>
public interface IHintProvider
{
    IPuzzleGraph PuzzleGraph { get; }
    IProgressMapper ProgressMapper { get; }
    IHintCorpus PuzzleCorpus { get; }
    ILoreSource LoreSource { get; }
    IMechanicExplainer Mechanics { get; }
    IReadOnlyList<ISoftLockRule> SoftLockRules { get; }
    IReadOnlyList<IProactiveRule> ProactiveRules { get; }
    HintPersona Persona { get; }

    /// <summary>
    ///     Known red herrings / dead ends, keyed by a lowercase noun that may appear in the player's
    ///     question, mapping to a grounded honest answer ("you can't / it does nothing"). Checked before
    ///     intent routing so a question about a dead end gets the truth instead of a confabulated puzzle
    ///     hint — the negative answers the invisiclues are famous for. Empty if the game has none wired.
    /// </summary>
    IReadOnlyDictionary<string, string> RedHerrings { get; }
}

// ---- request / response -------------------------------------------------------------

public enum HintKind
{
    Progress,
    Mechanic,
    Lore,
    SoftLock,
    Proactive,
    Decline
}

/// <summary>
///     A hint request. The engine treats <see cref="StateSnapshot" /> as read-only — it never
///     consumes a turn or mutates game state.
/// </summary>
public sealed record HintRequest(
    string SessionId,
    IContext StateSnapshot,
    string? Question,
    bool More,
    string? Topic);

public sealed record HintResponse(
    HintKind Kind,
    string Text,
    string? Topic,
    int Rung,
    int TotalRungs,
    SoftLockKind SoftLock);
