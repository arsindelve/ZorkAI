namespace Model.Intent;

/// <summary>
///     We have attempted to parse the input and cannot, with high confidence, match to any of the know intents.
/// </summary>
/// <example>
///     The user wrote "this game is stupid". Yeah, we're not going to be able to provide any interaction
///     from that input
/// </example>
public record NullIntent : IntentBase;