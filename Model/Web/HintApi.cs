namespace Model.Web;

/// <summary>
///     Request to the read-only hint endpoint. <see cref="More" /> = "I need more help" (advance the
///     ladder); <see cref="Topic" /> optionally pins a specific puzzle; <see cref="Question" /> carries
///     a free-text question for lore/mechanic modes.
/// </summary>
public record HintApiRequest(string SessionId, string? Question = null, bool More = false, string? Topic = null);

/// <summary>Response from the hint endpoint. Asking for a hint consumes no turn and mutates no game state.</summary>
public record HintApiResponse(
    string Kind,
    string Text,
    string? Topic,
    int Rung,
    int TotalRungs,
    string SoftLock);
