using Planetfall.Location;
using Planetfall.Location.Feinstein;
using ZorkOne;
using ZorkOne.Location.ForestLocation;

namespace Planetfall.Command;

/// <summary>
/// Class responsible for processing player death.
/// </summary>
public class DeathProcessor
{
    /// <summary>
    /// Process the death of the player. Oh, no! 
    /// </summary>
    /// <param name="death">The death message.</param>
    /// <param name="context">The current game context.</param>
    /// <returns>Returns an instance of InteractionResult.</returns>
    /// <exception cref="ArgumentException">Thrown if the context is not of type ZorkIContext.</exception>
    public InteractionResult Process(string death, IContext context)
    {
        if (context is not PlanetfallContext)
            throw new ArgumentException();
        
        var newLocation = Repository.GetLocation<DeckNine>();
        context.CurrentLocation = newLocation;

        var result = death +
                     "\n\t*** You have died ***\n\n" +
                     "Oh, well. According to the Treaty of Gishen IV, signed in 8747 GY, all adventure game players " +
                     "must be given another chance after dying. In the interests of interstellar peace...";

        return new PositiveInteractionResult(result);
    }
}