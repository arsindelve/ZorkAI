namespace Model.Web;

/// <summary>
///     Request to the read-only hint endpoint. <see cref="Question" /> is the player's free-text ask
///     ("what do I do?", "is the reactor important?", "I need more help"). Progressive disclosure comes
///     from the per-session chat history, so there is no rung/topic to pass.
/// </summary>
public record HintApiRequest(string SessionId, string Question);

/// <summary>Response from the hint endpoint. Asking for a hint consumes no turn and mutates no game state.</summary>
public record HintApiResponse(string Text);
