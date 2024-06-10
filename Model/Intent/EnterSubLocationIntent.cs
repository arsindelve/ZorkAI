namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that there is an intention to enter a vehicle or sub-location
/// </summary>
public record EnterSubLocationIntent : IntentBase
{
    public required string Noun { get; init; }
}