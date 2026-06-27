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
    private readonly IHintProvider _provider;

    public HintService(IHintProvider provider, IHintLanguageModel llm)
    {
        _provider = provider;
        _llm = llm;
    }

    public async Task<HintResponse> GetHint(HintRequest request)
    {
        // The prior conversation comes from the caller (the client replays it), so the service is fully
        // stateless — no server-side memory, works no matter which container answers.
        var history = request.History;

        // LLM 1 (solve) gets the FULL situation (key state + complete save game); LLM 2 (reveal) gets only
        // the salient key state — it must not contradict where the player is, but doesn't need the whole
        // JSON, which would just inflate tokens on every hint.
        var fullState = _provider.DescribePlayerContext(request.StateSnapshot);
        var keyState = _provider.DescribeKeyState(request.StateSnapshot);

        // History is passed to both so they can resolve follow-ups ("it", "more") to the subject in play.
        var solution = await _llm.Solve(_provider.Docs, fullState, history, request.Question, _provider.Persona);
        var revealed = await _llm.Reveal(keyState, solution, history, request.Question, _provider.Persona);

        // Fail visibly, not silently: if the model degraded to nothing, tell the player rather than
        // returning a blank hint. (The client should not append an "unavailable" reply to its history.)
        if (string.IsNullOrWhiteSpace(revealed))
            return new HintResponse("The hint system is unavailable right now. Try again in a moment.");

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
