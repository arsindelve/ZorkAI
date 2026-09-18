namespace Model.Hints;

/// <summary>The narrator's voice for a game, and the game's name.</summary>
public sealed record HintPersona(string SystemPrompt, string GameName = "");

/// <summary>
///     One turn of the hint conversation, as the client replays it: what the player asked and what the narrator
///     said. The hint endpoint is stateless — this history IS the memory, and it is how the oracle knows what it
///     has already given away and goes one step further next time.
/// </summary>
public sealed record HintExchange(string Question, string Revealed);
