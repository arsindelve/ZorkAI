using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     The game-agnostic hint engine. One instance per game (constructed with that game's
///     <see cref="IHintProvider" />). Read-only over the game state — asking for a hint never consumes
///     a turn or mutates the world. See Docs/hints/07-common-architecture.md.
/// </summary>
public sealed class HintService
{
    internal const string DeclineNothingLeft =
        "You appear to have done everything there is to do. The narrator is, frankly, impressed.";

    internal const string DeclineNoHint =
        "The narrator searches the void for wisdom on that, and comes up empty.";

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
        var progress = _provider.ProgressMapper.Map(request.StateSnapshot);

        // 1. Route intent. An explicit "more" (or a bare hint request) is always a progress hint;
        // otherwise classify the free-text question.
        var intent = request.More || string.IsNullOrWhiteSpace(request.Question)
            ? HintIntent.Progress
            : await _llm.ClassifyIntent(request.Question!);

        return intent switch
        {
            HintIntent.OutOfScope => Decline(request.StateSnapshot, progress, HintKind.Decline),
            HintIntent.Lore => await AnswerLore(request, progress),
            HintIntent.Mechanic => await AnswerMechanic(request, progress),
            _ => await AnswerProgress(request, progress)
        };
    }

    // ---- progress mode -----------------------------------------------------------------

    private async Task<HintResponse> AnswerProgress(HintRequest request, ProgressState progress)
    {
        var state = request.StateSnapshot;
        var memory = await _memory.Load(request.SessionId);

        // Soft-lock check. A Hard verdict short-circuits to a "restore" message; a softer verdict
        // becomes a caveat attached to the hint.
        var caveat = SoftLockKind.None;
        string? caveatMessage = null;
        foreach (var rule in _provider.SoftLockRules)
        {
            var verdict = rule.Evaluate(state, progress);
            if (verdict.Kind == SoftLockKind.Hard)
                return new HintResponse(HintKind.SoftLock, await Phrase(verdict.Message!),
                    null, 0, 0, SoftLockKind.Hard);

            if (verdict.Kind != SoftLockKind.None && caveat == SoftLockKind.None)
            {
                caveat = verdict.Kind;
                caveatMessage = verdict.Message;
            }
        }

        // Pick the topic: explicit, else the live active blocker (skipping already-solved topics).
        var topic = request.Topic ?? PickActiveBlocker(progress, state, memory);
        if (topic is null)
            return new HintResponse(HintKind.Decline, DeclineNothingLeft, null, 0, 0, caveat);

        // Reconcile: live state is authoritative for "is this still open". If solved, close + re-pick.
        if (IsDone(progress, topic))
        {
            memory.Closed.Add(topic);
            topic = PickActiveBlocker(progress, state, memory);
            if (topic is null)
            {
                await _memory.Save(request.SessionId, memory);
                return new HintResponse(HintKind.Decline, DeclineNothingLeft, null, 0, 0, caveat);
            }
        }

        if (!_provider.PuzzleCorpus.TryGetLadder(topic, out var ladder) || ladder.Rungs.Count == 0)
            return new HintResponse(HintKind.Decline, DeclineNoHint, topic, 0, 0, caveat);

        // Memory bookkeeping.
        var isNewTopic = memory.ActiveTopic != topic;
        if (isNewTopic)
        {
            memory.ActiveTopic = topic;
            memory.TopicStartMove.TryAdd(topic, state.Moves);
        }

        memory.AskCount[topic] = memory.AskCount.GetValueOrDefault(topic) + 1;

        // Rung selection: natural advance on "more", frustration floor sets the starting rung.
        var prev = memory.RungReached.GetValueOrDefault(topic, -1);
        var floor = FrustrationModel.RungFloor(state, memory, topic);
        var rung = request.More ? prev + 1 : isNewTopic ? floor : Math.Max(prev, floor);
        rung = Math.Clamp(rung, 0, ladder.Rungs.Count - 1);
        memory.RungReached[topic] = Math.Max(prev, rung);

        await _memory.Save(request.SessionId, memory);

        var text = await _llm.PhraseRung(ladder.Rungs[rung], _provider.Persona);
        if (caveatMessage is not null)
            text = caveatMessage + "\n\n" + text;

        return new HintResponse(HintKind.Progress, text, topic, rung, ladder.Rungs.Count, caveat);
    }

    private string? PickActiveBlocker(ProgressState progress, IContext state, HintMemory memory)
    {
        return _provider.PuzzleGraph
            .ActiveBlockers(progress, state)
            .FirstOrDefault(t => !memory.Closed.Contains(t) && !IsDone(progress, t));
    }

    private static bool IsDone(ProgressState progress, string nodeId)
    {
        return progress.Nodes.TryGetValue(nodeId, out var status) && status == NodeStatus.Done;
    }

    // ---- lore & mechanic modes ---------------------------------------------------------

    private async Task<HintResponse> AnswerLore(HintRequest request, ProgressState progress)
    {
        var answer = await _provider.LoreSource.Answer(request.Question!, request.StateSnapshot, progress, _llm);
        return new HintResponse(HintKind.Lore, answer.Text, null, 0, 0, SoftLockKind.None);
    }

    private async Task<HintResponse> AnswerMechanic(HintRequest request, ProgressState progress)
    {
        var answer = await _provider.Mechanics.Explain(request.Question!, request.StateSnapshot, _llm);
        if (answer.Grounded)
            return new HintResponse(HintKind.Mechanic, answer.Text, null, 0, 0, SoftLockKind.None);

        // Not a recognised mechanic question — fall back to lore, then a graceful decline.
        var lore = await _provider.LoreSource.Answer(request.Question!, request.StateSnapshot, progress, _llm);
        return lore.Grounded
            ? new HintResponse(HintKind.Lore, lore.Text, null, 0, 0, SoftLockKind.None)
            : Decline(request.StateSnapshot, progress, HintKind.Decline);
    }

    // ---- helpers -----------------------------------------------------------------------

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

    private HintResponse Decline(IContext state, ProgressState progress, HintKind kind)
    {
        return new HintResponse(kind, DeclineNoHint, null, 0, 0, SoftLockKind.None);
    }

    private Task<string> Phrase(string rung)
    {
        return _llm.PhraseRung(rung, _provider.Persona);
    }
}
