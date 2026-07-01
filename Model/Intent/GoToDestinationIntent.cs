namespace Model.Intent;

/// <summary>
///     The parser has determined the player wants to travel to a specific NAMED place/room
///     (e.g. "go to the kitchen", "walk to the maintenance room", "enter the dome room"), as opposed
///     to moving in a cardinal/relative direction. The engine resolves the name against the exits of
///     the room the player is currently standing in and, if it matches, walks there in one move
///     (issue #268).
/// </summary>
public record GoToDestinationIntent : IntentBase
{
    public required string Destination { get; init; }
}
