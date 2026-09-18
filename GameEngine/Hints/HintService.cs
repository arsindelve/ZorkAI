using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     The game-agnostic hint engine. Read-only over the game state — asking for a hint never consumes a
///     turn or mutates the world. See Docs/hints/07-common-architecture.md.
///
///     Per request:
///       1. Map live state onto the puzzle DAG (deterministic).
///       2. Route the message: progress / mechanic / lore / out of scope, plus which puzzle if any.
///       3. Progress: soft-lock check -> pick the topic (asked-about, continued, or the active blocker)
///          -> the next rung of its authored ladder, phrased in voice. The model holds only that rung.
///          Lore / mechanic: answered from the tier-gated source only.
///          No authored content: fall back to solving over the full docs, paced by the conversation.
///     Disclosure state rides in the client-replayed history (topic + rung per exchange); there is no
///     server-side memory, so any container can answer any request.
/// </summary>
public sealed class HintService
{
    internal const string DeclineOutOfScope =
        "That is hardly a matter for a humble narrator. Ask me about the game you're in.";

    internal const string DeclineNothingLeft =
        "You appear to have done everything there is to do. The narrator is, frankly, impressed.";

    internal const string DeclineAlreadyDone = "You've already taken care of that one.";

    internal const string DeclineUnavailable =
        "The hint system is unavailable right now. Try again in a moment.";

    internal const string PrefaceNotYet = "That's further down the road than you are. ";

    internal const string PrefaceExhausted = "That's everything there is to say on that one. ";

    private readonly IHintLanguageModel _llm;
    private readonly IHintProvider _provider;

    public HintService(IHintProvider provider, IHintLanguageModel llm)
    {
        _provider = provider;
        _llm = llm;
    }

    public async Task<HintResponse> GetHint(HintRequest request)
    {
        var state = request.StateSnapshot;
        var history = request.History;
        var question = request.Question?.Trim() ?? string.Empty;

        var progress = _provider.ProgressMapper.Map(state);
        var keyState = _provider.DescribeKeyState(state);

        // A bare hint request (the Hint button) needs no routing: carry on the current thread if there is
        // one, otherwise hint the active blocker.
        var routed = question.Length == 0
            ? RoutedIntent.More
            : await _llm.Route(question, history, Topics()) ?? RoutedIntent.OpenEnded;

        // "More" after a lore/mechanic answer means more of THAT, not a jump back to the puzzle ladder.
        // Those answers carry no topic in the history, which is how we can tell — and a continuation
        // refers to the last thing said whatever puzzle the router guessed from the catalog.
        if (routed is { Intent: HintIntent.Progress, ContinuesThread: true } &&
            history.Count > 0 && history[^1].Topic is null && question.Length > 0)
            routed = new RoutedIntent(HintIntent.Lore, true, null);

        return routed.Intent switch
        {
            HintIntent.OutOfScope => Decline(DeclineOutOfScope),
            HintIntent.Lore => await AnswerFromLore(HintKind.Lore, question, state, progress, keyState, history),
            HintIntent.Mechanic => await AnswerFromLore(HintKind.Mechanic, question, state, progress, keyState,
                history),
            _ => await AnswerProgress(routed, question, state, progress, keyState, history)
        };
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

    // ---- progress -------------------------------------------------------------------------

    private async Task<HintResponse> AnswerProgress(RoutedIntent routed, string question, IContext state,
        ProgressState progress, string keyState, IReadOnlyList<HintExchange> history)
    {
        // Soft-lock check. A Hard verdict short-circuits to a "restore" message; a softer verdict becomes
        // a caveat attached to whatever hint follows.
        var caveat = SoftLockKind.None;
        string? caveatMessage = null;
        foreach (var rule in _provider.SoftLockRules)
        {
            var verdict = rule.Evaluate(state, progress);
            if (verdict.Kind == SoftLockKind.Hard)
                return new HintResponse(HintKind.SoftLock, verdict.Message ?? string.Empty, null, 0, 0,
                    SoftLockKind.Hard);

            if (verdict.Kind != SoftLockKind.None && caveat == SoftLockKind.None)
            {
                caveat = verdict.Kind;
                caveatMessage = verdict.Message;
            }
        }

        // Topic: the puzzle they asked about, else the one under discussion, else the active blocker.
        var known = _provider.PuzzleGraph.Nodes.Select(n => n.Id).ToHashSet();
        var topic = routed.TopicId is not null && known.Contains(routed.TopicId) ? routed.TopicId : null;
        var askedAbout = topic is not null;

        if (topic is null && routed.ContinuesThread)
            topic = LastTopic(history);

        topic ??= PickBlocker(progress, state);
        if (topic is null)
            return Decline(DeclineNothingLeft, caveat);

        // Live state is authoritative for "is this still open".
        var preface = string.Empty;
        switch (progress.StatusOf(topic))
        {
            case NodeStatus.Done when askedAbout:
                return Decline(DeclineAlreadyDone, caveat);
            case NodeStatus.Done:
                topic = PickBlocker(progress, state);
                if (topic is null)
                    return Decline(DeclineNothingLeft, caveat);
                break;
            case NodeStatus.Locked when askedAbout:
                // They're asking about something they can't reach yet. Hinting it would leak a future
                // puzzle; say so and hint what's actually in their way.
                preface = PrefaceNotYet;
                topic = PickBlocker(progress, state);
                if (topic is null)
                    return Decline(DeclineNothingLeft, caveat);
                break;
        }

        if (!_provider.PuzzleCorpus.TryGetLadder(topic, out var ladder) || ladder.Rungs.Count == 0)
            return await Fallback(question, state, progress, keyState, history, caveat, caveatMessage);

        // Rung: one past the highest already revealed for this topic; a fresh topic starts at the
        // frustration floor. Clamped, so repeated asks at the end repeat the solution rather than run off.
        var previous = history
            .Where(h => h.Topic == topic && h.Rung is not null)
            .Select(h => h.Rung!.Value)
            .DefaultIfEmpty(-1)
            .Max();
        var rung = previous >= 0 ? previous + 1 : FrustrationModel.RungFloor(state);
        var exhausted = previous >= ladder.Rungs.Count - 1;
        rung = Math.Clamp(rung, 0, ladder.Rungs.Count - 1);

        var authored = ladder.Rungs[rung];
        var text = await _llm.PhraseRung(authored, keyState, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(text))
            text = authored; // the rung itself is already a safe, complete hint

        // They've had the last rung already and asked again: say so, rather than silently repeating it.
        if (exhausted && preface.Length == 0)
            preface = PrefaceExhausted;

        text = preface + text;
        if (caveatMessage is not null)
            text = caveatMessage + "\n\n" + text;

        return new HintResponse(HintKind.Progress, text, topic, rung, ladder.Rungs.Count, caveat);
    }

    // ---- lore & mechanic ------------------------------------------------------------------

    private async Task<HintResponse> AnswerFromLore(HintKind kind, string question, IContext state,
        ProgressState progress, string keyState, IReadOnlyList<HintExchange> history)
    {
        var source = _provider.LoreSource.GroundedText(state, progress);
        var text = await _llm.AnswerLore(question, source, keyState, history, _provider.Persona);

        return string.IsNullOrWhiteSpace(text)
            ? Decline(DeclineUnavailable)
            : new HintResponse(kind, text, null, 0, 0, SoftLockKind.None);
    }

    // ---- fallback: solve over everything ---------------------------------------------------

    private async Task<HintResponse> Fallback(string question, IContext state, ProgressState progress,
        string keyState, IReadOnlyList<HintExchange> history, SoftLockKind caveat, string? caveatMessage)
    {
        // The solver gets the full docs plus the invisiclues the player could know, and the complete
        // situation; the revealer gets only the key state, the solution and the conversation.
        var docs = _provider.Docs +
                   "\n\nPart 3 - OFFICIAL HINTS (only those the player could know at this point):\n\n" +
                   _provider.LoreSource.GroundedText(state, progress);
        var fullState = _provider.DescribePlayerContext(state);

        var solution = await _llm.Solve(docs, fullState, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(solution))
            return Decline(DeclineUnavailable, caveat);

        var revealed = await _llm.Reveal(keyState, solution, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(revealed))
            return Decline(DeclineUnavailable, caveat);

        if (caveatMessage is not null)
            revealed = caveatMessage + "\n\n" + revealed;

        return new HintResponse(HintKind.Grounded, revealed, null, 0, 0, caveat);
    }

    // ---- helpers ----------------------------------------------------------------------------

    private IReadOnlyList<HintTopic> Topics()
    {
        return _provider.PuzzleGraph.Nodes.Select(n => new HintTopic(n.Id, n.Title, n.Location)).ToList();
    }

    private string? PickBlocker(ProgressState progress, IContext state)
    {
        return _provider.PuzzleGraph
            .ActiveBlockers(progress, state)
            .FirstOrDefault(id => !progress.IsDone(id));
    }

    private static string? LastTopic(IReadOnlyList<HintExchange> history)
    {
        return history.LastOrDefault(h => h.Topic is not null)?.Topic;
    }

    private static HintResponse Decline(string message, SoftLockKind caveat = SoftLockKind.None)
    {
        return new HintResponse(HintKind.Decline, message, null, 0, 0, caveat);
    }
}
