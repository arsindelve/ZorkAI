using System.Text.Json;
using Microsoft.Extensions.Logging;
using Model.Hints;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     OpenAI implementation of the hint seam (locked build decision §7.1: all-OpenAI). Plain chat
///     completions. Every call fails safe in the direction that cannot spoil: the router falls back to an
///     open-ended progress read, the phraser to the authored rung it was handed, and the lore answerer and
///     the fallback solver/revealer to empty (the engine then declines). Nothing ever returns the raw
///     solution to the player on error.
/// </summary>
public sealed class OpenAiHintLanguageModel : OpenAIClientBase, IHintLanguageModel
{
    // The default; the actual model is the constructor arg, passed to the base as modelOverride so the
    // base Client is built with it directly (no second, unused client).
    protected override string ModelName => "gpt-5.4-mini";

    // gpt-5.4-mini: 1M-token context so the whole game source + walkthrough fit the fallback solver, and
    // a strong reasoner for working a solution out of raw code. The routing/phrasing calls are tiny.
    public OpenAiHintLanguageModel(ILogger? logger = null, string model = "gpt-5.4-mini")
        : base(logger, requireApiKey: false, modelOverride: model)
    {
    }

    public OpenAiHintLanguageModel(ILogger? logger, IChatCompletionClient client)
        : base(logger, requireApiKey: false, clientOverride: client)
    {
    }

    // ---- routing ------------------------------------------------------------------------------

    public async Task<RoutedIntent> Route(string question, IReadOnlyList<HintExchange> history,
        IReadOnlyList<HintTopic> topics)
    {
        if (!HasApiKey) return RoutedIntent.OpenEnded;

        const string system =
            "You route a player's message to a text-adventure hint system. Reply with ONLY a JSON object of the form " +
            "{\"intent\": \"PROGRESS\" | \"MECHANIC\" | \"LORE\" | \"OUTOFSCOPE\", \"continues\": true | false, " +
            "\"topic\": \"<PUZZLE ID>\" | null}.\n" +
            "intent: PROGRESS = they want help doing or solving something ('what do I do', 'how do I open the door', " +
            "'is the reactor useful', 'what about the tin can'). " +
            "MECHANIC = they ask why something is happening to THEM or how a rule works ('why am I sick', 'why do I keep " +
            "falling asleep', 'why can't I carry this'). " +
            "LORE = they ask about the world, its story, its characters, or why an event happened ('why did the ship " +
            "explode', 'who is Blather', 'what is this place', 'why is everyone gone'). " +
            "OUTOFSCOPE = not about the game at all.\n" +
            "continues: true when the message carries on the previous exchange rather than starting a new subject — " +
            "'more', 'another hint', 'I still don't get it', 'just tell me', or an elliptical follow-up like 'how do I " +
            "open it?' whose subject is the last thing discussed. false for a fresh subject.\n" +
            "topic: for PROGRESS only, the ID of the puzzle the message is about, chosen from the list; null when the " +
            "ask is open-ended ('what now?') or nothing on the list fits. Never set a topic for MECHANIC, LORE or " +
            "OUTOFSCOPE. Never explain; output the JSON only.";

        var catalog = string.Join("\n", topics.Select(t => $"- {t.Id}: {t.Title} ({t.Location})"));
        var user =
            $"PUZZLES:\n{catalog}\n\nCONVERSATION SO FAR:\n{HistoryText(history)}\n\nPLAYER NOW SAYS:\n{question}";

        var raw = await Complete(system, user, fallback: string.Empty, temperature: 0f);
        return ParseRoute(raw, topics) ?? RoutedIntent.OpenEnded;
    }

    /// <summary>
    ///     Parses the router's JSON. Tolerates chatter around the object. An unknown topic id is dropped
    ///     rather than trusted. Null when nothing usable came back.
    /// </summary>
    public static RoutedIntent? ParseRoute(string raw, IReadOnlyList<HintTopic> topics)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var start = raw.IndexOf('{');
        var end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            using var doc = JsonDocument.Parse(raw.Substring(start, end - start + 1));
            var root = doc.RootElement;

            var intentText = root.TryGetProperty("intent", out var i) && i.ValueKind == JsonValueKind.String
                ? i.GetString()!.Trim().ToUpperInvariant().Replace(" ", "").Replace("_", "")
                : "PROGRESS";
            var intent = intentText switch
            {
                "MECHANIC" => HintIntent.Mechanic,
                "LORE" => HintIntent.Lore,
                "OUTOFSCOPE" => HintIntent.OutOfScope,
                _ => HintIntent.Progress
            };

            var continues = root.TryGetProperty("continues", out var c) &&
                            (c.ValueKind == JsonValueKind.True ||
                             c.ValueKind == JsonValueKind.String &&
                             string.Equals(c.GetString(), "true", StringComparison.OrdinalIgnoreCase));

            string? topic = null;
            if (intent == HintIntent.Progress && root.TryGetProperty("topic", out var t) &&
                t.ValueKind == JsonValueKind.String)
            {
                var candidate = t.GetString()!.Trim();
                topic = topics.FirstOrDefault(x => string.Equals(x.Id, candidate, StringComparison.OrdinalIgnoreCase))
                    ?.Id;
            }

            return new RoutedIntent(intent, continues, topic);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // ---- phrasing -----------------------------------------------------------------------------

    public async Task<string> PhraseRung(string rung, string keyState, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        if (!HasApiKey) return rung;

        var system = persona.SystemPrompt +
                     " You are delivering ONE hint. Put it in your own words, in your voice — warmth, a wry aside, a " +
                     "nod to what the player just said are all welcome — in one to three sentences. But add no new " +
                     "information: no steps, objects, places, or explanations that the hint itself does not contain. " +
                     "If the hint is vague, stay exactly as vague. Stay consistent with the player's situation. Never " +
                     "mention hints, levels, or that you are holding anything back.";

        var user =
            $"PLAYER'S SITUATION:\n{keyState}\n\nHINT CONVERSATION SO FAR:\n{HistoryText(history)}\n\n" +
            $"PLAYER NOW ASKS:\n{question}\n\nTHE HINT TO DELIVER:\n{rung}";

        // Fail safe: the authored rung is already a usable hint — never leak the meta-instruction.
        return await Complete(system, user, fallback: rung, temperature: 0.4f);
    }

    public async Task<string> AnswerLore(string question, string groundedSource, string keyState,
        IReadOnlyList<HintExchange> history, HintPersona persona)
    {
        if (!HasApiKey) return string.Empty;

        var system = persona.SystemPrompt +
                     " Answer the player's question from the source text and nothing else — but in your own words " +
                     "and your own voice; never quote or parrot the source. If the source does not answer it, or " +
                     "says the answer comes later in the story, tell them warmly that they can't know that yet, " +
                     "and turn them back toward what is in front of them right now. Never invent. Never give puzzle " +
                     "solutions here; this is about the world and what is happening, not what to type next. Two to " +
                     "four sentences.";

        var user =
            $"PLAYER'S SITUATION:\n{keyState}\n\nSOURCE:\n{groundedSource}\n\nCONVERSATION SO FAR:\n" +
            $"{HistoryText(history)}\n\nPLAYER NOW ASKS:\n{question}";

        // Fail closed: no answer beats an improvised one.
        return await Complete(system, user, fallback: string.Empty, temperature: 0.4f);
    }

    // ---- fallback: solve + reveal ---------------------------------------------------------------

    public async Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        if (!HasApiKey) return string.Empty;

        // This class is shared by every game, so the solver scaffolding below must stay generic: the game's
        // identity and the flags that matter in its state come from the persona (#484). Name the game only
        // when the persona actually supplies one.
        var game = string.IsNullOrWhiteSpace(persona.GameName)
            ? "a text adventure"
            : $"the text-adventure game {persona.GameName}";

        var system =
            $"You are the SOLVER stage of a hint system for {game}. Work out the " +
            "COMPLETE, correct answer to the player's question, using ONLY the knowledge base and their situation.\n" +
            "RESOLVE FOLLOW-UPS: the player may be continuing a thread. If their new question is elliptical " +
            "('how do I open it?', 'more', 'is it serious?'), use the conversation so far to figure out what " +
            "they're still asking about, and keep answering THAT same subject.\n" +
            "ANSWER THE SPECIFIC THING THEY ASKED:\n" +
            "- If they name (or are continuing about) an object, place, puzzle, character, or topic, explain THAT — " +
            "how to solve/use it, OR, if the knowledge base shows it is a dead end, a death trap, or a misconception, " +
            "state that honestly and why. Do NOT redirect them to a different task or item.\n" +
            "- Only if they ask an open-ended question ('what do I do?', 'I'm stuck', 'where next?') should you use " +
            "their current next step from the situation.\n" +
            $"Use the player's situation ({persona.StateGrounding}) as grounding/background — to " +
            "make the answer accurate, NEVER as an excuse to dodge the question. This output is internal reasoning " +
            "for a second stage (not shown to the player) — be specific and complete. Never invent facts beyond the " +
            "knowledge base; if it isn't covered, say so.";

        var user =
            $"KNOWLEDGE BASE:\n{docs}\n\nPLAYER'S CURRENT SITUATION:\n{playerContext}\n\n" +
            $"CONVERSATION SO FAR:\n{HistoryText(history, "We answered")}\n\nPLAYER NOW ASKS:\n{question}\n\n" +
            "Work out the complete answer.";

        // Low temperature: this is the grounding step, pinned to the provided knowledge.
        return await Complete(system, user, fallback: string.Empty, temperature: 0.1f);
    }

    public async Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        if (!HasApiKey) return string.Empty;

        var system = persona.SystemPrompt +
                     " You are giving a HINT. You have been handed the complete solution, but your job is to reveal " +
                     "as LITTLE as possible — the gentlest nudge that still helps. Use the conversation so far to pace " +
                     "yourself: if this is the first time the player is asking about this, give a vague pointer that " +
                     "orients them without giving it away; if they keep asking for more, escalate gradually toward " +
                     "specifics; only give the exact steps if they have clearly, repeatedly pushed for the answer. Keep " +
                     "it to one or two sentences. Never dump the whole solution at once.\n" +
                     "If the solution says something is a dead end, a death trap, or a misconception, say so plainly — " +
                     "don't make them drag it out, and don't redirect them to a different task. When the question is " +
                     "about what to do, include a concrete next move — a real place, object, or action from the " +
                     "solution. When it is about why something is or isn't possible, answer that; a next move is " +
                     "optional. Never promise an item, exit, or option the solution does not contain.";

        var user =
            $"PLAYER'S CURRENT SITUATION:\n{playerContext}\n\nCOMPLETE SOLUTION (do NOT reveal all of this):\n" +
            $"{solution}\n\nHINT CONVERSATION SO FAR:\n{HistoryText(history)}\n\nPLAYER NOW ASKS:\n{question}\n\n" +
            "Reveal only the next appropriate amount.";

        // Fail closed: on error the player gets a decline from the engine, never the complete solution.
        return await Complete(system, user, fallback: string.Empty, temperature: 0.6f);
    }

    // ---- plumbing -------------------------------------------------------------------------------

    private static string HistoryText(IReadOnlyList<HintExchange> history, string weSaid = "You revealed")
    {
        return history.Count == 0
            ? "(this is their first question)"
            : string.Join("\n\n", history.Select(h => $"Player asked: {h.Question}\n{weSaid}: {h.Revealed}"));
    }

    private async Task<string> Complete(string system, string user, string fallback, float temperature)
    {
        var messages = new List<ChatMessage> { new SystemChatMessage(system), new UserChatMessage(user) };
        try
        {
            return await Client!.CompleteChatAsync(messages,
                new ChatCompletionOptions { Temperature = temperature });
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Hint LLM call failed; returning fallback.");
            return fallback;
        }
    }
}
