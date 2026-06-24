using Microsoft.Extensions.Logging;
using Model.Hints;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     OpenAI implementation of the two-tier hint seam (locked build decision §7.1: all-OpenAI). Plain
///     chat completions — no Assistants API needed. Degrades to a sensible passthrough when no API key
///     is configured (Solve returns empty, Reveal returns the solver's text), and fails safe on errors.
/// </summary>
public sealed class OpenAiHintLanguageModel : OpenAIClientBase, IHintLanguageModel
{
    protected override string ModelName => "gpt-4o-mini";

    // Own client so the model is selectable (e.g. gpt-4.1-mini for the 1M-token whole-source context).
    // Built in the body, after the base has resolved ApiKey — avoids the virtual-call-before-init trap.
    private readonly ChatClient? _client;

    // gpt-5.4-mini: 1M-token context so the whole game source + walkthroughs fit, and a strong reasoner
    // for working a solution out of raw code.
    public OpenAiHintLanguageModel(ILogger? logger = null, string model = "gpt-5.4-mini")
        : base(logger, requireApiKey: false)
    {
        if (HasApiKey) _client = new ChatClient(model: model, apiKey: ApiKey);
    }

    public async Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        if (!HasApiKey) return string.Empty;

        const string system =
            "You are the SOLVER stage of a hint system for the text-adventure game Planetfall. Work out the " +
            "COMPLETE, correct answer to the player's question, using ONLY the knowledge base and their situation.\n" +
            "RESOLVE FOLLOW-UPS: the player may be continuing a thread. If their new question is elliptical " +
            "('how do I open it?', 'more', 'is it serious?'), use the conversation so far to figure out what " +
            "they're still asking about, and keep answering THAT same subject.\n" +
            "ANSWER THE SPECIFIC THING THEY ASKED:\n" +
            "- If they name (or are continuing about) an object, place, puzzle, character, or topic, explain THAT — " +
            "how to solve/use it, OR, if it appears in the DEAD ENDS / RED HERRINGS, DEATH TRAPS, or MISCONCEPTIONS " +
            "sections, state that honestly (it's a dead end / you can't / it's a misconception) and why. Do NOT " +
            "redirect them to a different task or item.\n" +
            "- Only if they ask an open-ended question ('what do I do?', 'I'm stuck', 'where next?') should you use " +
            "their current next step from the situation.\n" +
            "Use the player's situation (what's done, Floyd alive/dead, their health) as grounding/background — to " +
            "make the answer accurate, NEVER as an excuse to dodge the question. This output is internal reasoning " +
            "for a second stage (not shown to the player) — be specific and complete. Never invent facts beyond the " +
            "knowledge base; if it isn't covered, say so.";

        var historyText = history.Count == 0
            ? "(this is their first question)"
            : string.Join("\n\n", history.Select(h => $"Player asked: {h.Question}\nWe answered: {h.Revealed}"));

        var user =
            $"KNOWLEDGE BASE:\n{docs}\n\nPLAYER'S CURRENT SITUATION:\n{playerContext}\n\n" +
            $"CONVERSATION SO FAR:\n{historyText}\n\nPLAYER NOW ASKS:\n{question}\n\nWork out the complete answer.";

        // Low temperature: this is the grounding step. We want it pinned to the provided knowledge,
        // not freelancing from training memory. The voice/snark happens in Reveal at higher temp.
        return await Complete(system, user, fallback: string.Empty, temperature: 0.1f);
    }

    public async Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        if (!HasApiKey) return solution;

        var system = persona.SystemPrompt +
            " You are giving a HINT. You have been handed the complete solution, but your job is to reveal " +
            "as LITTLE as possible — the gentlest nudge that still helps. Use the conversation so far to pace " +
            "yourself: if this is the first time the player is asking about this, give a vague pointer that " +
            "orients them without giving it away; if they keep asking for more, escalate gradually toward " +
            "specifics; only give the exact steps if they have clearly, repeatedly pushed for the answer. Keep " +
            "it to one or two sentences. Never dump the whole solution at once. If the situation is a dead end / " +
            "death trap / misconception, tell them the honest truth directly (don't make them drag it out).\n" +
            "CRITICAL: however gentle or snarky you are, ALWAYS include the concrete next move — a real place, " +
            "object, or action from the solution. Never let the joke replace the hint. Never promise an item, " +
            "exit, or option that the solution/state does not actually contain (e.g. don't say 'find another X' " +
            "if there is only one X).";

        var historyText = history.Count == 0
            ? "(no prior hint conversation on anything)"
            : string.Join("\n\n", history.Select(h => $"Player asked: {h.Question}\nYou revealed: {h.Revealed}"));

        var user =
            $"PLAYER'S CURRENT SITUATION:\n{playerContext}\n\nCOMPLETE SOLUTION (do NOT reveal all of this):\n{solution}\n\n" +
            $"HINT CONVERSATION SO FAR:\n{historyText}\n\nPLAYER NOW ASKS:\n{question}\n\nReveal only the next appropriate amount.";

        return await Complete(system, user, fallback: solution, temperature: 0.7f);
    }

    private async Task<string> Complete(string system, string user, string fallback, float temperature)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(system), new UserChatMessage(user) };
        try
        {
            ChatCompletion completion = await _client!.CompleteChatAsync(messages,
                new ChatCompletionOptions { Temperature = temperature });
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Hint LLM call failed; returning fallback.");
            return fallback;
        }
    }
}
