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

    /// <summary>
    /// The terse, static reply shown when the player directly addresses this character by name
    /// while they are not present — e.g. "Floyd isn't here.". This is a fixed string, never an AI
    /// generation, so it is deterministic. The default capitalizes the character's matching name;
    /// override it for non-default phrasing (e.g. "The ambassador isn't here.").
    /// </summary>
    string NotHereDescription
    {
        get
        {
            var name = (this as IItem)?.Name;
            if (string.IsNullOrWhiteSpace(name))
                return "That character isn't here. ";

            // Name defaults to the lowercase matching noun (e.g. "floyd"); player-facing text
            // should read "Floyd isn't here.".
            return char.ToUpperInvariant(name[0]) + name[1..] + " isn't here. ";
        }
    }
}
