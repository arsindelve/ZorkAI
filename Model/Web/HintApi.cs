using Model.Hints;

namespace Model.Web;

/// <summary>
///     Request to the read-only hint endpoint. <see cref="Question" /> is the player's free-text ask ("what do I
///     do?", "is the reactor important?", "why did the ship explode?", "more"); empty means the Hint button.
///     The endpoint is stateless, so the CLIENT supplies the two things only it has: <see cref="History" />, the
///     hint conversation so far (each question and the reply it got), which is how the narrator knows what it has
///     already given away; and <see cref="Transcript" />, the recent stretch of the game as the player saw it
///     (plain text, commands and responses), which is how it knows what they have actually encountered.
/// </summary>
public record HintApiRequest(
    string SessionId, string Question, IReadOnlyList<HintExchange>? History = null, string? Transcript = null);

/// <summary>
///     Response from the hint endpoint. Asking for a hint consumes no turn and mutates no game state.
///     <see cref="IsHint" /> is false for a non-answer (no session, narrator unavailable): display it, but do
///     NOT append it to the client-held conversation.
/// </summary>
public record HintApiResponse(string Text, bool IsHint = true);
