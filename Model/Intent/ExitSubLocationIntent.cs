namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that there is an intention to leave a vehicle or sub-location
/// </summary>
public record ExitSubLocationIntent : IntentBase
{
    public required string NounOne { get; init; }

    public string? NounTwo { get; init; }
}