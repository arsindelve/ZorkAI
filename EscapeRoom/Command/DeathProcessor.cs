using Model.Interaction;
using Model.Interface;

namespace EscapeRoom.Command;

/// <summary>
///     Class responsible for processing player death in the Escape Room game.
/// </summary>
public class DeathProcessor
{
    /// <summary>
    ///     Process the death of the player.
    /// </summary>
    /// <param name="death">The death message.</param>
    /// <param name="context">The current game context.</param>
    /// <returns>Returns a DeathInteractionResult that signals the game engine to restart.</returns>
    public DeathInteractionResult Process(string death, IContext context)
    {
        if (context is not EscapeRoomContext escapeContext)
            throw new ArgumentException("Context must be EscapeRoomContext");

        escapeContext.DeathCounter++;

        var result = death +
                     "\n\n\t*** You have died ***\n\n" +
                     context.CurrentScore + "\n\n" +
                     "Well, that was unfortunate. But every adventurer deserves another chance...\n\n";

        var deathResult = new DeathInteractionResult(result, escapeContext.DeathCounter);
        escapeContext.PendingDeath = deathResult;
        return deathResult;
    }
}
