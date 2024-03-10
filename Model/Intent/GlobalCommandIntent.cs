namespace Model.Intent;

/// <summary>
///     The parser is confident that is a global command like SAVE, QUIT or LOOK.
/// </summary>
public record GlobalCommandIntent : IntentBase
{
    public IGlobalCommand Command { get; init; }
}