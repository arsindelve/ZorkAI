using Microsoft.Extensions.Logging;
using Model.Hints;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     OpenAI implementation of the hint engine's language-model seam (locked build decision §7.1:
///     all-OpenAI). Uses plain chat completions — intent classification and rung/lore phrasing don't
///     need the Assistants API. Degrades to a no-LLM passthrough when no API key is configured, so the
///     engine still returns the (already-grounded) rung/source text.
/// </summary>
public sealed class OpenAiHintLanguageModel : OpenAIClientBase, IHintLanguageModel
{
    // Cheap + fast; mirrors the ZorkLore assistant's model.
    protected override string ModelName => "gpt-4o-mini";

    public OpenAiHintLanguageModel(ILogger? logger = null) : base(logger, requireApiKey: false)
    {
    }

    public async Task<HintIntent> ClassifyIntent(string question)
    {
        if (!HasApiKey) return HintIntent.Progress;

        const string system =
            "Classify the player's message about a text-adventure game into exactly one word: " +
            "PROGRESS (they want help with what to do / how to solve a puzzle), " +
            "MECHANIC (why is something happening to them — sick, tired, hungry, can't carry, etc.), " +
            "LORE (a question about the game's world, history, characters, or backstory), or " +
            "OUTOFSCOPE (not about the game at all). Reply with only that one word.";

        var text = await Complete(system, question);
        return text.Trim().ToUpperInvariant() switch
        {
            var t when t.StartsWith("MECHANIC") => HintIntent.Mechanic,
            var t when t.StartsWith("LORE") => HintIntent.Lore,
            var t when t.StartsWith("OUTOFSCOPE") || t.StartsWith("OUT OF SCOPE") => HintIntent.OutOfScope,
            _ => HintIntent.Progress
        };
    }

    public async Task<string> PhraseRung(string rung, HintPersona persona)
    {
        if (!HasApiKey) return rung;

        var user =
            "Deliver this single hint in your voice, in one or two sentences. Reveal ONLY what it says — " +
            "do not add any further steps, items, or specifics beyond it:\n\n" + rung;

        return await Complete(persona.SystemPrompt, user);
    }

    public async Task<string> PhraseLore(string question, string groundedSource, HintPersona persona)
    {
        if (!HasApiKey) return groundedSource;

        var system = persona.SystemPrompt +
                     " Answer ONLY from the provided source text, in one short paragraph. If the source " +
                     "does not cover the question, say so in character rather than inventing anything.";
        var user = $"Question: {question}\n\nSource:\n{groundedSource}";

        return await Complete(system, user);
    }

    private async Task<string> Complete(string system, string user)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(system),
            new UserChatMessage(user)
        };

        try
        {
            ChatCompletion completion = await Client!.CompleteChatAsync(messages,
                new ChatCompletionOptions { Temperature = 0.7f });
            return completion.Content[0].Text;
        }
        catch (Exception e)
        {
            Logger?.LogWarning(e, "Hint LLM call failed; returning raw source.");
            // Fail safe: return the user payload's source rather than throwing — the grounded text is
            // still useful even unphrased.
            return user;
        }
    }
}
