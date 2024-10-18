namespace Model.Intent;

/// <summary>
///     This class represents the response from the parser, after the user has typed their input,
///     indicating what the parser thinks the user wants to to do.
/// </summary>
public abstract record IntentBase
{
    public string? Message { get; init; }
}