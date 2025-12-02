using Model.Interface;
using Model.AIGeneration;

namespace Model.Item;

/// <summary>
/// Marker interface for items that can be the target of a conversation.
/// </summary>
public interface ICanBeTalkedTo : IInteractionTarget
{
    /// <summary>
    /// Invoked when the player attempts to talk to this character.
    /// </summary>
    /// <param name="text">The text the player spoke.</param>
    /// <param name="context">The current game context.</param>
    /// <param name="client">Generation client for producing dialog.</param>
    /// <returns>The response text from the character.</returns>
    Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client);
}
