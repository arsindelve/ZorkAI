namespace GameEngine.Hints;

/// <summary>
///     Per-session hint memory — the subsystem's OWN persistent state (separate from the SavedGame).
///     Drives laddering ("I need more help" resumes where it left off) and frustration sensing.
///     Live game state remains authoritative for localization; this only records what's been revealed.
/// </summary>
public sealed class HintMemory
{
    /// <summary>The topic "I need more help" currently refers to.</summary>
    public string? ActiveTopic { get; set; }

    /// <summary>Highest rung index already revealed, per topic.</summary>
    public Dictionary<string, int> RungReached { get; set; } = new();

    /// <summary>Number of hint requests made against a topic (a frustration signal).</summary>
    public Dictionary<string, int> AskCount { get; set; } = new();

    /// <summary>The move count when the player first asked about a topic (for "turns stuck").</summary>
    public Dictionary<string, int> TopicStartMove { get; set; } = new();

    /// <summary>Topics the player has since solved — never re-hinted.</summary>
    public HashSet<string> Closed { get; set; } = new();
}

/// <summary>
///     Persistence for <see cref="HintMemory" />, keyed by session id. Production impl is DynamoDB
///     (a record alongside the SavedGame, not inside it); tests use an in-memory impl.
/// </summary>
public interface IHintMemoryStore
{
    Task<HintMemory> Load(string sessionId);
    Task Save(string sessionId, HintMemory memory);
}

/// <summary>Simple in-memory store for tests and local play.</summary>
public sealed class InMemoryHintMemoryStore : IHintMemoryStore
{
    private readonly Dictionary<string, HintMemory> _store = new();

    public Task<HintMemory> Load(string sessionId)
    {
        if (!_store.TryGetValue(sessionId, out var memory))
        {
            memory = new HintMemory();
            _store[sessionId] = memory;
        }

        return Task.FromResult(memory);
    }

    public Task Save(string sessionId, HintMemory memory)
    {
        _store[sessionId] = memory;
        return Task.CompletedTask;
    }
}
