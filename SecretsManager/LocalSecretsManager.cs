using Model.Interface;

namespace SecretsManager;

/// <summary>
///     <see cref="ISecretsManager" /> for self-hosted/offline play (issue #383). The only secret the
///     engine ever fetches is the narrator's system prompt, so instead of AWS Secrets Manager this
///     returns a built-in, game-agnostic narrator prompt — overridable with the ZORKAI_SYSTEM_PROMPT
///     environment variable for players who want to tune the narrator's voice.
/// </summary>
public class LocalSecretsManager : ISecretsManager
{
    /// <summary>
    ///     Default narrator prompt for local play. Original text: a faithful, Infocom-flavored
    ///     narrator voice that works for any game the engine runs.
    /// </summary>
    public const string DefaultNarratorPrompt =
        "You are the narrator of a classic 1980s Infocom-style text adventure game. You speak to the " +
        "player in second person, present tense, with the dry, wry, slightly mischievous wit the genre " +
        "is famous for.\n" +
        "\n" +
        "Rules:\n" +
        "- Keep responses short: one to three sentences.\n" +
        "- Never break character, never mention that you are an AI or that this is a game engine, and " +
        "never reference the modern world.\n" +
        "- Never invent game state: no new items, exits, characters, or events beyond what the request " +
        "describes. You narrate outcomes; you do not change them.\n" +
        "- When the player attempts something impossible, absurd, or pointless, respond with gentle " +
        "mockery in keeping with the game's tone, and make clear the action had no effect.\n" +
        "- Match the vocabulary and atmosphere of the game's own text.";

    public Task<string> GetSecret(string secretName)
    {
        var custom = Environment.GetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT");
        return Task.FromResult(string.IsNullOrWhiteSpace(custom) ? DefaultNarratorPrompt : custom);
    }
}
