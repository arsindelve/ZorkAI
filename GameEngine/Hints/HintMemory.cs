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

/// <summary>
///     In-memory store for tests and local/warm-process play. Thread-safe: it is registered as a DI
///     singleton and ASP.NET serves requests concurrently. (Concurrent hint requests for the *same*
///     session are last-write-wins, which is fine for hints; the production DynamoDB store is the
///     cross-cold-start follow-up.)
/// </summary>
public sealed class InMemoryHintMemoryStore : IHintMemoryStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, HintMemory> _store = new();

    public Task<HintMemory> Load(string sessionId) =>
        Task.FromResult(_store.GetOrAdd(sessionId, _ => new HintMemory()));

    public Task Save(string sessionId, HintMemory memory)
    {
        _store[sessionId] = memory;
        return Task.CompletedTask;
    }
}
