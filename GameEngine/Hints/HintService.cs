using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     The game-agnostic two-tier hint engine. Read-only over the game state — asking for a hint never
///     consumes a turn or mutates the world. See Docs/hints/07-common-architecture.md.
///
///     Flow per request:
///       1. Describe the player's live context (read-only).
///       2. LLM 1 (Solve): docs + context + question -> the complete solution / honest truth.
///       3. LLM 2 (Reveal): context + solution + chat history + question -> what to reveal next.
///       4. Append to the chat history.
/// </summary>
public sealed class HintService
{
    private readonly IHintLanguageModel _llm;
    private readonly IHintMemoryStore _memory;
    private readonly IHintProvider _provider;

    public HintService(IHintProvider provider, IHintMemoryStore memory, IHintLanguageModel llm)
    {
        _provider = provider;
        _memory = memory;
        _llm = llm;
    }

    public async Task<HintResponse> GetHint(HintRequest request)
    {
        var memory = await _memory.Load(request.SessionId);
        var context = _provider.DescribePlayerContext(request.StateSnapshot);

        // LLM 1: work out the complete solution to the player's current situation from the docs + state.
        // History is passed so it can resolve follow-ups ("it", "more") to the subject still in play.
        var solution = await _llm.Solve(_provider.Docs, context, memory.History, request.Question, _provider.Persona);

        // LLM 2: decide what to reveal next, paced from the conversation so far.
        var revealed = await _llm.Reveal(context, solution, memory.History, request.Question, _provider.Persona);

        memory.History.Add(new HintExchange(request.Question, revealed));
        await _memory.Save(request.SessionId, memory);

        return new HintResponse(revealed);
    }

    /// <summary>Proactive (push) nudges — survival clocks etc. Evaluated read-only; surfaced by the UI.</summary>
    public IReadOnlyList<ProactiveNudge> ProactiveNudges(IContext state)
    {
        return _provider.ProactiveRules
            .Select(r => r.Evaluate(state))
            .Where(n => n is not null)
            .Select(n => n!)
            .OrderByDescending(n => n.Priority)
            .ToList();
    }
}
