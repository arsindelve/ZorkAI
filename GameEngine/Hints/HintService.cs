using Model.Hints;
using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     The game-agnostic hint engine. Read-only over the game state — asking for a hint never consumes a
///     turn or mutates the world. See Docs/hints/07-common-architecture.md.
///
///     Per request:
///       1. Map live state onto the puzzle DAG (deterministic).
///       2. Route the message: progress / mechanic / lore / out of scope, which puzzle if any, and whether
///          it continues the previous exchange. A continuation is about whatever was last answered — the
///          history carries each exchange's kind — so "more" after a lore answer stays lore.
///       3. Progress: soft-lock check -> pick the topic (asked-about, continued, or the active blocker)
///          -> the next rung of its authored ladder, phrased in voice. The model holds only that rung.
///          A locked topic redirects to the nearest open prerequisite; an exhausted ladder says so.
///          Lore / mechanic: answered from the tier-gated source only.
///          Anything specific the catalog doesn't cover: the fallback solver over the full docs.
///     Disclosure state rides in the client-replayed history (kind, topic, rung per exchange); there is
///     no server-side memory. Every failure declines rather than guessing.
/// </summary>
public sealed class HintService
{
    internal const string DeclineOutOfScope =
        "That is hardly a matter for a humble narrator. Ask me about the game you're in.";

    internal const string DeclineNothingLeft =
        "You appear to have done everything there is to do. The narrator is, frankly, impressed.";

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

        RoutedIntent routed;
        if (question.Length == 0)
        {
            // The Hint button: always a puzzle hint — continue the thread if there is one, else the blocker.
            routed = RoutedIntent.More;
        }
        else
        {
            // The model is unavailable or produced nothing usable: decline. Guessing "what do I do?" here
            // would answer a lore question with the walkthrough's next step, which is the failure this
            // engine exists to prevent.
            var read = await _llm.Route(question, history, Topics());
            if (read is null)
                return new HintResponse(HintKind.Decline, DeclineUnavailable);

            routed = Continue(read, history);
        }

        return routed.Intent switch
        {
            HintIntent.OutOfScope => new HintResponse(HintKind.Decline, DeclineOutOfScope),
            HintIntent.Lore => await AnswerFromLore(HintKind.Lore, routed, question, state, progress, keyState, history),
            HintIntent.Mechanic => await AnswerFromLore(HintKind.Mechanic, routed, question, state, progress, keyState,
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

    /// <summary>
    ///     A continuation ("more", "I still don't get it") is about the last answer, whatever puzzle the
    ///     router guessed from the catalog. The history says what kind of answer that was.
    /// </summary>
    private static RoutedIntent Continue(RoutedIntent routed, IReadOnlyList<HintExchange> history)
    {
        if (!routed.ContinuesThread || history.Count == 0)
            return routed;

        return history[^1].Kind switch
        {
            nameof(HintKind.Lore) => new RoutedIntent(HintIntent.Lore, true, null),
            nameof(HintKind.Mechanic) => new RoutedIntent(HintIntent.Mechanic, true, null),
            // Carry on the fallback conversation, paced by the longer history.
            nameof(HintKind.Grounded) => new RoutedIntent(HintIntent.Progress, true, null, Unlisted: true),
            // Climb the ladder in play — whatever puzzle the router guessed, and even if it called the
            // bare "more" unlisted.
            nameof(HintKind.Progress) => RoutedIntent.More,
            _ => routed
        };
    }

    // ---- progress -------------------------------------------------------------------------

    private async Task<HintResponse> AnswerProgress(RoutedIntent routed, string question, IContext state,
        ProgressState progress, string keyState, IReadOnlyList<HintExchange> history)
    {
        // Soft-lock check. A Hard verdict short-circuits to a "restore" message; a softer verdict becomes
        // a caveat attached to whatever hint follows.
        var caveat = SoftLockVerdict.None;
        foreach (var verdict in _provider.SoftLockRules.Select(r => r.Evaluate(state, progress)))
        {
            if (verdict.Kind == SoftLockKind.Hard)
                return new HintResponse(HintKind.SoftLock, verdict.Message ?? string.Empty, SoftLock: SoftLockKind.Hard);

            if (verdict.Kind != SoftLockKind.None && caveat.Kind == SoftLockKind.None)
                caveat = verdict;
        }

        // Something specific the catalog has no puzzle for — a dead end, a red herring, a mechanic the
        // ladders don't cover — or a router topic we don't recognise: reason over the full docs instead of
        // hinting whatever happens to be the active blocker.
        var known = _provider.PuzzleGraph.Nodes.Select(n => n.Id).ToHashSet();
        if (routed.Unlisted || routed.TopicId is not null && !known.Contains(routed.TopicId))
            return await Fallback(question, state, progress, keyState, history, caveat);

        // Topic: the puzzle they asked about, else the one under discussion, else the active blocker.
        var topic = routed.TopicId;
        var askedAbout = topic is not null;
        if (topic is null && routed.ContinuesThread)
            topic = LastTopic(history);

        var preface = string.Empty;
        if (topic is not null)
            switch (progress.StatusOf(topic))
            {
                case NodeStatus.Done when askedAbout:
                    // A finished stage of a multi-stage puzzle ("the light went gray, now what?"), or the
                    // room a puzzle lives in ("what do I do in the tower?"): the answer is the next open
                    // stage. No preface — the next rung stands on its own. If nothing follows it, the
                    // player wants something the ladders don't model ("how do I use the booth?" with the
                    // card in hand): let the solver answer from the game itself.
                    topic = FirstOpenDependent(topic, progress);
                    if (topic is null)
                        return await Fallback(question, state, progress, keyState, history, caveat);
                    break;
                case NodeStatus.Done:
                    topic = null; // solved since it was last discussed — move on
                    break;
                case NodeStatus.Locked when askedAbout:
                    // Hinting a puzzle they can't reach yet would leak it; hint what actually stands between
                    // them and it instead. Say so only when that is more than one step away — "how do I get
                    // to the shuttle?" from the lower elevator wants the elevator hint, not a lecture.
                    var asked = topic;
                    topic = FirstOpenPrerequisite(asked, progress);
                    if (topic is null || !IsDirectPrerequisite(topic, asked))
                        preface = PrefaceNotYet;
                    break;
                case NodeStatus.Locked:
                    topic = null; // a continued thread that's no longer reachable (a restore, say)
                    break;
            }

        topic ??= PickBlocker(progress, state);
        if (topic is null)
            return Decline(DeclineNothingLeft, caveat);

        if (!_provider.PuzzleCorpus.TryGetLadder(topic, out var ladder) || ladder.Rungs.Count == 0)
            return await Fallback(question, state, progress, keyState, history, caveat);

        // Rung: one past the highest already revealed for this topic; a fresh topic starts at the
        // frustration floor. Clamped, so repeated asks at the end repeat the solution rather than run off.
        var previous = history
            .Where(h => h.Topic == topic && h.Rung is not null)
            .Select(h => h.Rung!.Value)
            .DefaultIfEmpty(-1)
            .Max();
        var exhausted = previous >= ladder.Rungs.Count - 1;
        var rung = previous >= 0 ? previous + 1 : FrustrationModel.RungFloor(state, ladder.Rungs.Count);
        rung = Math.Clamp(rung, 0, ladder.Rungs.Count - 1);

        var authored = ladder.Rungs[rung];
        var text = await _llm.PhraseRung(authored, keyState, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(text))
            text = authored; // the rung itself is already a safe, complete hint

        // They've had the last rung already and asked again: say so, rather than silently repeating it.
        if (exhausted && preface.Length == 0)
            preface = PrefaceExhausted;

        return WithCaveat(new HintResponse(HintKind.Progress, preface + text, topic, rung, ladder.Rungs.Count), caveat);
    }

    // ---- lore & mechanic ------------------------------------------------------------------

    private async Task<HintResponse> AnswerFromLore(HintKind kind, RoutedIntent routed, string question,
        IContext state, ProgressState progress, string keyState, IReadOnlyList<HintExchange> history)
    {
        var source = _provider.LoreSource.GroundedText(state, progress);
        var text = await _llm.AnswerLore(question, source, keyState, history, _provider.Persona);

        if (string.IsNullOrWhiteSpace(text))
            return new HintResponse(HintKind.Decline, DeclineUnavailable);

        // The router read it as a question about the world, but the world has nothing to say: it was a
        // question about a thing ("what's wrong with the cube?", "what does the memo mean?"). Hint the puzzle
        // the router saw in it, or let the solver answer from the game itself — never improvise lore.
        if (text.Trim() == HintSignals.NotInSource)
        {
            var asProgress = new RoutedIntent(HintIntent.Progress, false, routed.TopicId, Unlisted: routed.TopicId is null);
            return await AnswerProgress(asProgress, question, state, progress, keyState, history);
        }

        return new HintResponse(kind, text);
    }

    // ---- fallback: solve over everything ---------------------------------------------------

    private async Task<HintResponse> Fallback(string question, IContext state, ProgressState progress,
        string keyState, IReadOnlyList<HintExchange> history, SoftLockVerdict caveat)
    {
        // The solver gets the static docs (passed by reference — it is the whole game source) plus the
        // player's complete situation with the official hints they could know appended; the revealer gets
        // only the key state, the solution and the conversation.
        var situation = _provider.DescribePlayerContext(state) +
                        "\n\nOFFICIAL HINTS THE PLAYER COULD KNOW AT THIS POINT:\n\n" +
                        _provider.LoreSource.GroundedText(state, progress);

        var solution = await _llm.Solve(_provider.Docs, situation, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(solution))
            return Decline(DeclineUnavailable, caveat);

        var revealed = await _llm.Reveal(keyState, solution, history, question, _provider.Persona);
        if (string.IsNullOrWhiteSpace(revealed))
            return Decline(DeclineUnavailable, caveat);

        return WithCaveat(new HintResponse(HintKind.Grounded, revealed), caveat);
    }

    // ---- helpers ----------------------------------------------------------------------------

    private IReadOnlyList<HintTopic> Topics()
    {
        return _provider.PuzzleGraph.Nodes.Select(n => new HintTopic(n.Id, n.Title, n.Location, n.Aliases)).ToList();
    }

    private string? PickBlocker(ProgressState progress, IContext state)
    {
        return _provider.PuzzleGraph.ActiveBlockers(progress, state).FirstOrDefault();
    }

    /// <summary>
    ///     The Available node nearest to a locked one along its prerequisite edges — what the player must
    ///     actually do next to make the asked-about puzzle reachable. Null if the graph has no answer.
    /// </summary>
    private string? FirstOpenPrerequisite(string lockedTopic, ProgressState progress)
    {
        var byId = _provider.PuzzleGraph.Nodes.ToDictionary(n => n.Id);
        var seen = new HashSet<string> { lockedTopic };
        var queue = new Queue<string>(byId.TryGetValue(lockedTopic, out var start) ? start.Prerequisites : []);

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!seen.Add(id)) continue;

            switch (progress.StatusOf(id))
            {
                case NodeStatus.Available:
                    return id;
                case NodeStatus.Locked when byId.TryGetValue(id, out var node):
                    foreach (var prerequisite in node.Prerequisites)
                        queue.Enqueue(prerequisite);
                    break;
            }
        }

        return null;
    }

    /// <summary>
    ///     The Available node nearest downstream of a finished one — the next stage of the same puzzle.
    ///     Walks through finished stages; null if nothing open depends on it.
    /// </summary>
    private string? FirstOpenDependent(string doneTopic, ProgressState progress)
    {
        var nodes = _provider.PuzzleGraph.Nodes;
        var seen = new HashSet<string> { doneTopic };
        var queue = new Queue<string>();
        queue.Enqueue(doneTopic);

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            foreach (var dependent in nodes.Where(n => n.Prerequisites.Contains(id)))
            {
                if (!seen.Add(dependent.Id)) continue;
                switch (progress.StatusOf(dependent.Id))
                {
                    case NodeStatus.Available:
                        return dependent.Id;
                    case NodeStatus.Done:
                        queue.Enqueue(dependent.Id);
                        break;
                }
            }
        }

        return null;
    }

    private bool IsDirectPrerequisite(string prerequisite, string of)
    {
        return _provider.PuzzleGraph.Nodes.Any(n => n.Id == of && n.Prerequisites.Contains(prerequisite));
    }

    private static string? LastTopic(IReadOnlyList<HintExchange> history)
    {
        return history.LastOrDefault(h => h.Topic is not null)?.Topic;
    }

    private static HintResponse Decline(string message, SoftLockVerdict caveat)
    {
        return new HintResponse(HintKind.Decline, message, SoftLock: caveat.Kind);
    }

    private static HintResponse WithCaveat(HintResponse response, SoftLockVerdict caveat)
    {
        return caveat.Kind == SoftLockKind.None
            ? response
            : response with { Text = caveat.Message + "\n\n" + response.Text, SoftLock = caveat.Kind };
    }
}
