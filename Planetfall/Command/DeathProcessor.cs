namespace Planetfall.Command;

/// <summary>
///     Class responsible for processing player death.
/// </summary>
public class DeathProcessor
{
    /// <summary>
    ///     Process the death of the player. Oh, no!
    /// </summary>
    /// <param name="death">The death message.</param>
    /// <param name="context">The current game context.</param>
    /// <returns>Returns an instance of InteractionResult.</returns>
    /// <exception cref="ArgumentException">Thrown if the context is not of type PlanetfallContext.</exception>
    public InteractionResult Process(string death, IContext context)
    {
        if (context is not PlanetfallContext)
            throw new ArgumentException();

        // TODO: Would you like to restart this game from the beginning, restore a saved position, end this session of the game, or look at hints? (Type RESTART, RESTORE, QUIT, or HINTS):
        var newLocation = Repository.GetLocation<DeckNine>();
        context.CurrentLocation = newLocation;

        var result = death +
                     "\n\n\t*** You have died ***\n\n" +
                     context.CurrentScore + "\n\n" +
                     "Oh, well. According to the Treaty of Gishen IV, signed in 8747 GY, all adventure game players " +
                     "must be given another chance after dying. In the interests of interstellar peace...";

        return new PositiveInteractionResult(result);
    }
}