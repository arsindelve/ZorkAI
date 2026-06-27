using Model.Hints;

namespace Model.Web;

/// <summary>
///     Request to the read-only hint endpoint. <see cref="Question" /> is the player's free-text ask
///     ("what do I do?", "is the reactor important?", "I need more help"). <see cref="History" /> is the
///     prior hint conversation, supplied by the CLIENT — progressive disclosure is paced from it, so the
///     endpoint holds no server-side state and works regardless of which Lambda container answers. The
///     client appends each (question, returned text) pair and replays them next time.
/// </summary>
public record HintApiRequest(string SessionId, string Question, IReadOnlyList<HintExchange>? History = null);

/// <summary>Response from the hint endpoint. Asking for a hint consumes no turn and mutates no game state.</summary>
public record HintApiResponse(string Text);
