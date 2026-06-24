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

    public OpenAiHintLanguageModel(ILogger? logger = null) : base(logger, requireApiKey: false)
    {
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

        return await Complete(system, user, fallback: string.Empty);
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
            "death trap / misconception, tell them the honest truth directly (don't make them drag it out).";

        var historyText = history.Count == 0
            ? "(no prior hint conversation on anything)"
            : string.Join("\n\n", history.Select(h => $"Player asked: {h.Question}\nYou revealed: {h.Revealed}"));

        var user =
            $"PLAYER'S CURRENT SITUATION:\n{playerContext}\n\nCOMPLETE SOLUTION (do NOT reveal all of this):\n{solution}\n\n" +
            $"HINT CONVERSATION SO FAR:\n{historyText}\n\nPLAYER NOW ASKS:\n{question}\n\nReveal only the next appropriate amount.";

        return await Complete(system, user, fallback: solution);
    }

    private async Task<string> Complete(string system, string user, string fallback)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(system), new UserChatMessage(user) };
        try
        {
            ChatCompletion completion = await Client!.CompleteChatAsync(messages,
                new ChatCompletionOptions { Temperature = 0.7f });
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Hint LLM call failed; returning fallback.");
            return fallback;
        }
    }
}
