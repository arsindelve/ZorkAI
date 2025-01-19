namespace Planetfall.Location;

/// <summary>
/// Represents a location with a special interaction involving Floyd,
/// defining properties related to the interaction's state and prompt message.
/// </summary>
public interface IFloydSpecialInteractionLocation
{
   string FloydPrompt { get; }
}