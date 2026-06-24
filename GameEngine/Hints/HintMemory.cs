using System.Collections.Concurrent;
using Model.Hints;

namespace GameEngine.Hints;

/// <summary>
///     Per-session hint memory — the chat history that LLM 2 (the revealer) uses to decide what to
///     reveal next. Persisted alongside the SavedGame (not inside it), so "I need more help" keeps
///     escalating across sessions.
/// </summary>
public sealed class HintMemory
{
    public List<HintExchange> History { get; set; } = new();
}

/// <summary>Persistence for <see cref="HintMemory" />, keyed by session id.</summary>
public interface IHintMemoryStore
{
    Task<HintMemory> Load(string sessionId);
    Task Save(string sessionId, HintMemory memory);
}

/// <summary>
///     In-memory store for tests and local/warm-process play. Thread-safe (registered as a DI singleton;
///     ASP.NET serves requests concurrently). The production DynamoDB store is the cross-cold-start follow-up.
/// </summary>
public sealed class InMemoryHintMemoryStore : IHintMemoryStore
{
    private readonly ConcurrentDictionary<string, HintMemory> _store = new();

    public Task<HintMemory> Load(string sessionId) =>
        Task.FromResult(_store.GetOrAdd(sessionId, _ => new HintMemory()));

    public Task Save(string sessionId, HintMemory memory)
    {
        _store[sessionId] = memory;
        return Task.CompletedTask;
    }
}
