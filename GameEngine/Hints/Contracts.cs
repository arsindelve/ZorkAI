using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

// =====================================================================================
// Common (game-agnostic) hint engine contracts. See Docs/hints/07-common-architecture.md.
//
// The engine is deterministic-first: live state -> progress over the puzzle DAG -> the active
// blocker -> one authored rung. The model only phrases what it is handed (one rung, or the
// tier-gated lore source), so it cannot over-reveal or invent. Whole-docs solving is the
// fallback for questions no authored content covers. A game contributes one IHintProvider;
// HintService is written once.
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
///     game-specific extras (e.g. survival-clock levels).
/// </summary>
public sealed record ProgressState(
    IReadOnlyDictionary<string, NodeStatus> Nodes,
    IReadOnlyDictionary<string, object> Extras)
{
    public NodeStatus StatusOf(string nodeId) => Nodes.GetValueOrDefault(nodeId, NodeStatus.Locked);

    public bool IsDone(string nodeId) => StatusOf(nodeId) == NodeStatus.Done;
}

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
    ///     The node(s) most likely gating the player right now, best-first. May use the live state for
    ///     location proximity / recent trajectory.
    /// </summary>
    IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState);
}

/// <summary>Maps live game state (read-only) to the game-agnostic <see cref="ProgressState" />.</summary>
public interface IProgressMapper
{
    ProgressState Map(IContext liveState);
}

/// <summary>An ordered set of progressively-more-specific hint rungs for one puzzle node.</summary>
public sealed record RungLadder(string NodeId, IReadOnlyList<string> Rungs);

/// <summary>The authored puzzle-hint corpus (Docs/hints/&lt;game&gt;/06, as data).</summary>
public interface IHintCorpus
{
    bool TryGetLadder(string nodeId, out RungLadder ladder);
}

/// <summary>
///     The lore / world-knowledge source. Returns the text the lore answerer may draw on for THIS
///     player — already filtered to what they could know, so a spoiler that isn't in the text can't
///     be revealed. The game decides the tiering from live state and progress.
/// </summary>
public interface ILoreSource
{
    string GroundedText(IContext liveState, ProgressState progress);
}

public enum SoftLockKind
{
    None,

    /// <summary>Recoverable; surface as a caution.</summary>
    Warning,

    /// <summary>Can still finish, but the best ending/score is foreclosed.</summary>
    BestEndingOnly,

    /// <summary>Victory now impossible — advise restore.</summary>
    Hard
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
///     engine consumes only this.
/// </summary>
public interface IHintProvider
{
    IPuzzleGraph PuzzleGraph { get; }
    IProgressMapper ProgressMapper { get; }
    IHintCorpus PuzzleCorpus { get; }
    ILoreSource LoreSource { get; }
    IReadOnlyList<ISoftLockRule> SoftLockRules { get; }
    IReadOnlyList<IProactiveRule> ProactiveRules { get; }
    HintPersona Persona { get; }

    /// <summary>
    ///     Everything the fallback solver may need, as text — for Planetfall the complete game source plus
    ///     the verified walkthrough. Used only when no authored ladder covers the question.
    /// </summary>
    string Docs { get; }

    /// <summary>
    ///     The FULL player situation for the fallback solver: a key-state highlight followed by the complete
    ///     serialized save game. Built fresh and read-only from <see cref="IContext" /> each request.
    /// </summary>
    string DescribePlayerContext(IContext state);

    /// <summary>
    ///     The salient key-state highlight ONLY (location, companion alive/dead, systems fixed, health) —
    ///     what every phrasing call gets so it stays consistent with where the player is.
    /// </summary>
    string DescribeKeyState(IContext state);
}

// ---- request / response -------------------------------------------------------------

public enum HintKind
{
    /// <summary>One rung of an authored ladder.</summary>
    Progress,

    /// <summary>An explanation of something happening to the player, from state.</summary>
    Mechanic,

    /// <summary>A world/story answer from the tier-gated lore source.</summary>
    Lore,

    /// <summary>The player cannot win from here.</summary>
    SoftLock,

    /// <summary>The fallback path: solved over the full docs, then paced by the conversation.</summary>
    Grounded,

    /// <summary>Nothing to say — out of scope, already done, nothing left, or the model was unavailable.</summary>
    Decline
}

/// <summary>
///     A hint request. The engine treats <see cref="StateSnapshot" /> as read-only — asking for a hint
///     consumes no turn and mutates no game state. <see cref="History" /> is the prior hint conversation
///     supplied by the caller; the service is otherwise stateless.
/// </summary>
public sealed record HintRequest(
    string SessionId, IContext StateSnapshot, string Question, IReadOnlyList<HintExchange> History);

/// <summary>
///     The answer. For a <see cref="HintKind.Progress" /> rung, <see cref="Topic" /> and <see cref="Rung" />
///     must be echoed back in the next request's history so the ladder resumes; <see cref="TotalRungs" />
///     lets a UI show "hint 2 of 3".
/// </summary>
public sealed record HintResponse(
    HintKind Kind,
    string Text,
    string? Topic,
    int Rung,
    int TotalRungs,
    SoftLockKind SoftLock);
