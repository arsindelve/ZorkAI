namespace GameEngine;

/// <summary>
///     Recognizes the double-quote characters that delimit spoken dialogue in player input — straight
///     (<c>"</c>) and both smart quotes (<c>“</c>/<c>”</c>). Shared by <see cref="SentenceSplitter" />
///     (which must not split a command on a period inside quoted speech) and
///     <see cref="ConversationHandler" /> (which strips these quotes when routing speech to an NPC), so
///     the two stages always agree on exactly what counts as quoted speech — previously each kept its
///     own copy that had to be edited in lockstep by convention.
/// </summary>
internal static class SpeechQuotes
{
    public static bool IsDoubleQuote(char c) => c is '"' or '“' or '”';
}
