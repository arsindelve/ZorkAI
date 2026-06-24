using GameEngine.Hints;
using Model.Hints;
using Model.Interface;

namespace UnitTests.Hints;

/// <summary>Deterministic stub LLM: echoes the rung verbatim so tests can assert which rung was shown.</summary>
internal sealed class StubLlm : IHintLanguageModel
{
    public HintIntent Intent { get; init; } = HintIntent.Progress;

    public Task<HintIntent> ClassifyIntent(string question) => Task.FromResult(Intent);

    public Task<string> PhraseRung(string rung, HintPersona persona) => Task.FromResult(rung);

    public Task<string> PhraseLore(string question, string groundedSource, HintPersona persona) =>
        Task.FromResult(groundedSource);
}

/// <summary>A configurable in-memory provider for engine tests.</summary>
internal sealed class FakeProvider : IHintProvider
{
    private readonly List<string> _order = new();
    private readonly Dictionary<string, NodeStatus> _statuses = new();
    private readonly Dictionary<string, RungLadder> _ladders = new();

    public List<ISoftLockRule> SoftLocks { get; } = new();
    public List<IProactiveRule> Proactive { get; } = new();
    public ILoreSource Lore { get; set; } = new FakeLore(new LoreAnswer(false, ""));
    public IMechanicExplainer Mechanic { get; set; } = new FakeMechanic(new LoreAnswer(false, ""));

    public IPuzzleGraph PuzzleGraph => new FakeGraph(_order, _statuses);
    public IProgressMapper ProgressMapper => new FakeMapper(_statuses);
    public IHintCorpus PuzzleCorpus => new FakeCorpus(_ladders);
    public ILoreSource LoreSource => Lore;
    public IMechanicExplainer Mechanics => Mechanic;
    public IReadOnlyList<ISoftLockRule> SoftLockRules => SoftLocks;
    public IReadOnlyList<IProactiveRule> ProactiveRules => Proactive;
    public HintPersona Persona => new("test narrator");
    public Dictionary<string, string> Herrings { get; } = new();
    public IReadOnlyDictionary<string, string> RedHerrings => Herrings;

    private void Add(string id, NodeStatus status, string[] rungs)
    {
        _order.Add(id);
        _statuses[id] = status;
        _ladders[id] = new RungLadder(id, rungs);
    }

    public static FakeProvider WithOpenPuzzle(string id, params string[] rungs)
    {
        var p = new FakeProvider();
        p.Add(id, NodeStatus.Available, rungs);
        return p;
    }

    public static FakeProvider WithOpenPuzzles(params (string id, string[] rungs)[] puzzles)
    {
        var p = new FakeProvider();
        foreach (var (id, rungs) in puzzles) p.Add(id, NodeStatus.Available, rungs);
        return p;
    }

    public static FakeProvider WithStatuses(params (string id, NodeStatus status, string[] rungs)[] nodes)
    {
        var p = new FakeProvider();
        foreach (var (id, status, rungs) in nodes) p.Add(id, status, rungs);
        return p;
    }
}

internal sealed class FakeGraph : IPuzzleGraph
{
    private readonly List<string> _order;
    private readonly Dictionary<string, NodeStatus> _statuses;

    public FakeGraph(List<string> order, Dictionary<string, NodeStatus> statuses)
    {
        _order = order;
        _statuses = statuses;
    }

    public IReadOnlyCollection<PuzzleNode> Nodes =>
        _order.Select(id => new PuzzleNode(id, Array.Empty<string>(), false, id, "")).ToList();

    public IReadOnlyCollection<string> OpenSet(ProgressState state) =>
        _order.Where(id => state.Nodes.GetValueOrDefault(id) == NodeStatus.Available).ToList();

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState) =>
        _order.Where(id => state.Nodes.GetValueOrDefault(id) == NodeStatus.Available).ToList();
}

internal sealed class FakeMapper : IProgressMapper
{
    private readonly Dictionary<string, NodeStatus> _statuses;
    public FakeMapper(Dictionary<string, NodeStatus> statuses) => _statuses = statuses;

    public ProgressState Map(IContext liveState) =>
        new(new Dictionary<string, NodeStatus>(_statuses), new Dictionary<string, object>());
}

internal sealed class FakeCorpus : IHintCorpus
{
    private readonly Dictionary<string, RungLadder> _ladders;
    public FakeCorpus(Dictionary<string, RungLadder> ladders) => _ladders = ladders;

    public bool TryGetLadder(string nodeId, out RungLadder ladder) => _ladders.TryGetValue(nodeId, out ladder!);
}

internal sealed class FakeSoftLock : ISoftLockRule
{
    private readonly SoftLockVerdict _verdict;
    public FakeSoftLock(SoftLockVerdict verdict) => _verdict = verdict;
    public SoftLockVerdict Evaluate(IContext liveState, ProgressState progress) => _verdict;
}

internal sealed class FakeLore : ILoreSource
{
    private readonly LoreAnswer _answer;
    public FakeLore(LoreAnswer answer) => _answer = answer;

    public Task<LoreAnswer> Answer(string question, IContext liveState, ProgressState progress, IHintLanguageModel llm) =>
        Task.FromResult(_answer);
}

internal sealed class FakeMechanic : IMechanicExplainer
{
    private readonly LoreAnswer _answer;
    public FakeMechanic(LoreAnswer answer) => _answer = answer;

    public Task<LoreAnswer> Explain(string question, IContext liveState, IHintLanguageModel llm) =>
        Task.FromResult(_answer);
}

internal sealed class FakeProactive : IProactiveRule
{
    private readonly ProactiveNudge _nudge;
    public FakeProactive(ProactiveNudge nudge) => _nudge = nudge;
    public ProactiveNudge? Evaluate(IContext liveState) => _nudge;
}
