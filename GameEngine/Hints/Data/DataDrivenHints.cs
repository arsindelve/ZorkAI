using Model.Interface;

namespace GameEngine.Hints.Data;

/// <summary>
///     The puzzle graph and progress mapper for a generated corpus. Completion is whatever the data says —
///     predicates observed from the game's own state during the walkthrough — back-filled along the
///     prerequisites: a puzzle whose dependent is done is done, since the walkthrough proves the dependent
///     could not have been reached without it. That is what lets a signal be transient (a flask that is
///     later emptied, a shuttle that later stops) and still be the right one.
/// </summary>
public sealed class DataPuzzleGraph : IPuzzleGraph, IProgressMapper
{
    private readonly HintData _data;
    private readonly Dictionary<string, HintNodeData> _byId;
    private readonly Func<IContext, IReadOnlyDictionary<string, string>> _flatten;

    /// <param name="data">The generated corpus.</param>
    /// <param name="flatten">How to read the live state; defaults to <see cref="StateFlattener.Flatten" /> over the Repository.</param>
    public DataPuzzleGraph(HintData data, Func<IContext, IReadOnlyDictionary<string, string>>? flatten = null)
    {
        _data = data;
        _flatten = flatten ?? StateFlattener.Flatten;
        _byId = data.Nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);
        Nodes = data.Nodes
            .Select(n => new PuzzleNode(n.Id, n.Prerequisites.ToArray(), n.Optional, n.Title, n.Location, n.Aliases.ToArray()))
            .ToList();
    }

    public IReadOnlyCollection<PuzzleNode> Nodes { get; }

    public ProgressState Map(IContext liveState)
    {
        var flat = _flatten(liveState);
        var done = _data.Nodes
            .Where(n => n.Done.Count > 0 && n.Done.All(p => p.Holds(flat)))
            .Select(n => n.Id)
            .ToHashSet(StringComparer.Ordinal);

        // Back-fill: everything a done node requires is done too, all the way down.
        var queue = new Queue<string>(done);
        while (queue.Count > 0)
            foreach (var prerequisite in _byId[queue.Dequeue()].Prerequisites)
                if (done.Add(prerequisite))
                    queue.Enqueue(prerequisite);

        var nodes = new Dictionary<string, NodeStatus>(StringComparer.Ordinal);
        foreach (var node in _data.Nodes)
            nodes[node.Id] = done.Contains(node.Id) ? NodeStatus.Done
                : node.Prerequisites.All(done.Contains) ? NodeStatus.Available
                : NodeStatus.Locked;

        return new ProgressState(nodes);
    }

    public IReadOnlyList<string> ActiveBlockers(ProgressState state, IContext liveState)
    {
        // The puzzle in the room the player is standing in first, then the mandatory spine, then
        // walkthrough order — the same policy as the hand-written graph.
        var here = liveState.CurrentLocation?.Name ?? string.Empty;
        return _data.Nodes
            .Where(n => state.StatusOf(n.Id) == NodeStatus.Available)
            .OrderBy(n => here.Length > 0 && n.Location.Contains(here, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(n => n.Optional ? 1 : 0)
            .Select(n => n.Id)
            .ToList();
    }

    public HintNodeData? Node(string id) => _byId.GetValueOrDefault(id);
}

/// <summary>The rung ladders of a generated corpus.</summary>
public sealed class DataHintCorpus : IHintCorpus
{
    private readonly Dictionary<string, RungLadder> _ladders;

    public DataHintCorpus(HintData data)
    {
        _ladders = data.Nodes
            .Where(n => n.Rungs.Count > 0)
            .ToDictionary(n => n.Id, n => new RungLadder(n.Id, n.Rungs), StringComparer.Ordinal);
    }

    public bool TryGetLadder(string nodeId, out RungLadder ladder) => _ladders.TryGetValue(nodeId, out ladder!);
}
