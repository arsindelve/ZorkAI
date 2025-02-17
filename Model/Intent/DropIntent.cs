namespace Model.Intent;

/// <summary>
/// Represents an intent to perform a "Drop" action within the system.
/// This intent encapsulates the context and data required to process
/// a "Drop" interaction.
/// </summary>
/// <remarks>
/// This record is part of the intent-based pattern for handling actions
/// and is processed by components such as TakeOrDropInteractionProcessor.
/// </remarks>
public record DropIntent : IntentBase
{
    public required string OriginalInput { get; init; }
}