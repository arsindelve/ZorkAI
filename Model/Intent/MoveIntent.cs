using Model.Movement;

namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that the input is a request to move the adventurer. We only need to
///     parse the "direction" provided by the parser and translate it into one of the known directions
///     in the enum. The engine will handle the rest.
/// </summary>
public record MoveIntent : IntentBase
{
    public Direction Direction { get; init; }

    /// <summary>
    ///     The object the player named alongside the direction, when they named one ("enter door",
    ///     "exit door", "go in the boat"). Null for a bare direction, which is every move the engine
    ///     builds internally. Only <see cref="Direction.In" /> and <see cref="Direction.Out" /> consult
    ///     it - see MoveEngine, issue #551.
    /// </summary>
    public string? Noun { get; init; }
}
