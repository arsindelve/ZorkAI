using Model.Hints;

namespace Model.Web;

/// <summary>
///     Request to the read-only hint endpoint. <see cref="Question" /> is the player's free-text ask
///     ("what do I do?", "is the reactor important?", "why did the ship explode?", "more"). Empty means the
///     Hint button: continue the current thread, or hint whatever is blocking them.
///     <see cref="History" /> is the prior hint conversation, supplied by the CLIENT: it appends each
///     exchange as returned — question, text, and the <c>kind</c>/<c>topic</c>/<c>rung</c> the response
///     carried — and replays them next time. That is how a hint ladder resumes where it left off, and how
///     "more" knows what it is continuing, without any server-side state, regardless of which Lambda
///     container answers.
/// </summary>
public record HintApiRequest(string SessionId, string Question, IReadOnlyList<HintExchange>? History = null);

/// <summary>
///     Response from the hint endpoint. Asking for a hint consumes no turn and mutates no game state.
///     <see cref="Kind" /> is one of Progress, Mechanic, Lore, SoftLock, Grounded, Decline; a bare
///     construction is a Decline. <see cref="Kind" />, <see cref="Topic" /> and <see cref="Rung" /> must be
///     echoed back in the next request's history; <see cref="TotalRungs" /> lets a UI show "hint 2 of 3";
///     <see cref="SoftLock" /> (None, Warning, BestEndingOnly, Hard) says whether a caveat rides on the text.
/// </summary>
public record HintApiResponse(
    string Text,
    string Kind = "Decline",
    string? Topic = null,
    int Rung = 0,
    int TotalRungs = 0,
    string SoftLock = "None");
