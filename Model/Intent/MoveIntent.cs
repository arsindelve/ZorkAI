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
}