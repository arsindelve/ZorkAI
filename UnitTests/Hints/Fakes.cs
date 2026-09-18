using GameEngine.Hints;
using Model.Hints;
using Model.Interface;

namespace UnitTests.Hints;

/// <summary>
///     Deterministic stub LLM, shared with Planetfall.Tests. Routes however the test says, and echoes what
///     it is handed — the rung, the grounded source, the solution — so a test can assert exactly what
///     reached the model and what came back. Records every input.
/// </summary>
public sealed class StubLlm : IHintLanguageModel
{
    /// <summary>What Route returns; null simulates an unavailable router.</summary>
    public RoutedIntent? Routed { get; set; } = RoutedIntent.OpenEnded;

    public string SolveResult { get; set; } = "THE COMPLETE SOLUTION";

    /// <summary>When set, returned instead of the echo — "" simulates a degraded/unavailable model.</summary>
    public string? ForcePhrase { get; set; }

    public string? ForceLore { get; set; }
    public string? ForceReveal { get; set; }

    public int RouteCalls;
    public string? LastRouteQuestion;
    public IReadOnlyList<HintTopic> LastTopics = new List<HintTopic>();
    public string? LastRung, LastKeyState, LastLoreSource, LastDocs, LastSolveContext, LastRevealContext, LastSolution;
    public IReadOnlyList<HintExchange> LastHistory = new List<HintExchange>();

    public Task<RoutedIntent?> Route(string question, IReadOnlyList<HintExchange> history,
        IReadOnlyList<HintTopic> topics)
    {
        RouteCalls++;
        LastRouteQuestion = question;
        LastTopics = topics;
        LastHistory = history;
        return Task.FromResult(Routed);
    }

    public Task<string> PhraseRung(string rung, string keyState, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastRung = rung;
        LastKeyState = keyState;
        LastHistory = history;
        return Task.FromResult(ForcePhrase ?? rung);
    }

    public Task<string> AnswerLore(string question, string groundedSource, string keyState,
        IReadOnlyList<HintExchange> history, HintPersona persona)
    {
        LastLoreSource = groundedSource;
        LastKeyState = keyState;
        LastHistory = history;
        return Task.FromResult(ForceLore ?? "lore:" + groundedSource);
    }

    public Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastDocs = docs;
        LastSolveContext = playerContext;
        LastHistory = history;
        return Task.FromResult(SolveResult);
    }

    public Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastRevealContext = playerContext;
        LastSolution = solution;
        LastHistory = history;
        // The revealed amount scales with how often they've asked — proves the history reaches the revealer.
        return Task.FromResult(ForceReveal ?? $"reveal#{history.Count}:{solution}");
    }
}

/// <summary>A configurable in-memory provider for engine tests.</summary>
internal sealed class FakeProvider : IHintProvider
{
    private readonly List<PuzzleNode> _nodes = new();
    private readonly Dictionary<string, NodeStatus> _statuses = new();
    private readonly Dictionary<string, RungLadder> _ladders = new();

    public List<ISoftLockRule> SoftLocks { get; } = new();
    public List<IProactiveRule> Proactive { get; } = new();
    public string LoreText { get; set; } = "GROUNDED LORE";
    public string Docs { get; set; } = "DOCS";
    public string PlayerContext { get; set; } = "FULL CONTEXT";
    public string KeyState { get; set; } = "KEYSTATE";

    public IPuzzleGraph PuzzleGraph => new FakeGraph(_nodes, _statuses);
    public IProgressMapper ProgressMapper => new FakeMapper(_statuses);
    public IHintCorpus PuzzleCorpus => new FakeCorpus(_ladders);
    public ILoreSource LoreSource => new FakeLore(LoreText);
    public IReadOnlyList<ISoftLockRule> SoftLockRules => SoftLocks;
    public IReadOnlyList<IProactiveRule> ProactiveRules => Proactive;
    public HintPersona Persona => new("test narrator");

    public string DescribePlayerContext(IContext state) => PlayerContext;
    public string DescribeKeyState(IContext state) => KeyState;

    public FakeProvider Add(string id, NodeStatus status, params string[] rungs)
    {
        return AddWithPrerequisites(id, status, Array.Empty<string>(), rungs);
    }

    public FakeProvider AddWithPrerequisites(string id, NodeStatus status, string[] prerequisites,
        params string[] rungs)
    {
        _nodes.Add(new PuzzleNode(id, prerequisites, false, "Puzzle " + id, "Somewhere"));
        _statuses[id] = status;
        if (rungs.Length > 0) _ladders[id] = new RungLadder(id, rungs);
        return this;
    }

    public static FakeProvider WithOpenPuzzle(string id, params string[] rungs)
    {
        return new FakeProvider().Add(id, NodeStatus.Available, rungs);
    }
}

internal sealed class FakeGraph(List<PuzzleNode> nodes, Dictionary<string, NodeStatus> statuses) : IPuzzleGraph
{
    public IReadOnlyCollection<PuzzleNode> Nodes => nodes;

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState) =>
        nodes.Select(n => n.Id).Where(id => state.StatusOf(id) == NodeStatus.Available).ToList();
}

internal sealed class FakeMapper(Dictionary<string, NodeStatus> statuses) : IProgressMapper
{
    public ProgressState Map(IContext liveState) => new(new Dictionary<string, NodeStatus>(statuses));
}

internal sealed class FakeCorpus(Dictionary<string, RungLadder> ladders) : IHintCorpus
{
    public bool TryGetLadder(string nodeId, out RungLadder ladder) => ladders.TryGetValue(nodeId, out ladder!);
}

internal sealed class FakeLore(string text) : ILoreSource
{
    public string GroundedText(IContext liveState, ProgressState progress) => text;
}

internal sealed class FakeSoftLock(SoftLockVerdict verdict) : ISoftLockRule
{
    public SoftLockVerdict Evaluate(IContext liveState, ProgressState progress) => verdict;
}

internal sealed class FakeProactive(ProactiveNudge nudge) : IProactiveRule
{
    public ProactiveNudge? Evaluate(IContext liveState) => nudge;
}
