namespace Model.Intent;

/// <summary>
/// Represents an intent to perform a "Take" action within the system.
/// This intent encapsulates the context and data required to process
/// a "Take" interaction.
/// </summary>
/// <remarks>
/// This record is part of the intent-based pattern for handling actions
/// and is processed by components such as TakeOrDropInteractionProcessor.
/// </remarks>
public record TakeIntent : IntentBase
{
    public required string OriginalInput { get; init; }
    
    public string? Noun { get; init; }
}
