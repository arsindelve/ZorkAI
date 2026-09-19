namespace Model.Hints;

/// <summary>
///     The hint oracle: one strong model that understands the whole game, sees the player's actual
///     situation, and answers the way a friend who has finished the game would — with judgment, not from
///     an index. There is no router, no puzzle graph and no authored ladder behind this seam; disclosure
///     pacing, spoiler discipline and "what is this player really stuck on" are the model's to work out
///     from the game knowledge, the situation and the conversation so far.
/// </summary>
public interface IHintOracle
{
    /// <param name="gameKnowledge">The game bible: a veteran's complete understanding of the game, in prose. Static per game.</param>
    /// <param name="persona">The narrator's voice.</param>
    /// <param name="situation">Where this player is, what they carry, what they have done and seen — facts and their recent transcript.</param>
    /// <param name="history">The hint conversation so far, oldest first.</param>
    /// <param name="question">What the player just asked. Empty means "give me a hint".</param>
    /// <returns>The narrator's reply, or null when the model is unavailable or failed (the caller declines; it never guesses).</returns>
    Task<string?> Answer(string gameKnowledge, HintPersona persona, string situation,
        IReadOnlyList<HintExchange> history, string question);
}
