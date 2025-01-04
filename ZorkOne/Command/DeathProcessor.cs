using GameEngine;
using Model.Interface;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Command;

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
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException();

        zorkContext.DeathCounter++;
        zorkContext.LightWoundCounter = 0;
        var newLocation = Repository.GetLocation<ForestOne>();
        context.CurrentLocation = newLocation;

        // TODO: Scatter inventory. 
        // TODO: Die ?? number of times: Don't get another chance. 
        // https://github.com/historicalsource/zork1/blob/7d54d16fca7a5dd7c6191c93651aad925f8c0922/1actions.zil#L4046

        var result = death +
                     "\n\t*** You have died ***\n\n" +
                     "Now, let's take a look here... Well, you probably deserve another chance. I can't quite fix " +
                     "you up completely, but you can't have everything.\n\n" +
                     newLocation.GetDescription(context);

        return new PositiveInteractionResult(result);
    }
}