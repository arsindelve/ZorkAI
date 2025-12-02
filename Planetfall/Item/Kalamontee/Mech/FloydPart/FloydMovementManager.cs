using Model.AIGeneration;
using Planetfall.Location;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydMovementManager(Floyd floyd)
{
    /// <summary>
    /// Handles the wandering countdown when Floyd is off exploring.
    /// Returns a message if Floyd returns to the player, empty string if still wandering, or null if not wandering.
    /// </summary>
    public async Task<string?> HandleWanderingCountdown(IContext context, IGenerationClient client)
    {
        if (!floyd.IsOffWandering || floyd.WanderingTurnsRemaining <= 0)
            return null;

        floyd.WanderingTurnsRemaining--;

        if (floyd.WanderingTurnsRemaining == 0)
        {
            // Floyd returns to the player
            floyd.IsOffWandering = false;
            floyd.CurrentLocation = context.CurrentLocation as ICanContainItems;
            context.CurrentLocation.ItemPlacedHere(floyd);
            return await GenerateReturnMessage(context, client);
        }

        return string.Empty; // Still wandering
    }

    /// <summary>
    /// Handles Floyd's following behavior when the player moves to a different location.
    /// Returns a message if Floyd follows, starts wandering, or an empty string if he stays put.
    /// </summary>
    public string HandleFollowingPlayer(IContext context)
    {
        // Don't follow if already wandering
        if (floyd.IsOffWandering)
            return string.Empty;

        bool isInTheRoom = floyd.CurrentLocation == context.CurrentLocation;
        if (!isInTheRoom)
        {
            // Random chance to not follow (1 in 5 chance)
            if (floyd.Chooser.RollDiceSuccess(5))
            {
                floyd.IsOffWandering = true;
                floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns
                floyd.CurrentLocation = null; // Floyd is not in any location while wandering
                return string.Empty; // No message - player just doesn't see "Floyd follows you"
            }

            // Normal follow behavior
            context.CurrentLocation.ItemPlacedHere(floyd);
            return "Floyd follows you. ";
        }

        return string.Empty;
    }

    /// <summary>
    /// Handles spontaneous wandering trigger - 1 in 20 chance per turn.
    /// Returns a departure message if Floyd starts wandering, or null if he doesn't.
    /// </summary>
    public async Task<string?> HandleSpontaneousWandering(IContext context, IGenerationClient client)
    {
        // Don't wander if already wandering, not in the same room, or in a location where Floyd doesn't talk
        bool isInTheRoom = floyd.CurrentLocation == context.CurrentLocation;
        if (floyd.IsOffWandering || !isInTheRoom || context.CurrentLocation is IFloydDoesNotTalkHere)
            return null;

        if (floyd.Chooser.RollDiceSuccess(20))
        {
            floyd.IsOffWandering = true;
            floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns
            (context.CurrentLocation as ICanContainItems)?.RemoveItem(floyd);
            floyd.CurrentLocation = null; // Floyd is not in any location while wandering
            return await GenerateDepartureMessage(context, client);
        }

        return null;
    }

    /// <summary>
    /// Makes Floyd start wandering for a random number of turns (1-5).
    /// Floyd will be removed from his current location and will return to the player later.
    /// </summary>
    public void StartWandering(IContext context)
    {
        if (!floyd.IsOn || floyd.HasDied)
            return; // Can't wander if he's not on or dead

        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns

        // Remove Floyd from the current location
        floyd.CurrentLocation?.RemoveItem(floyd);
        floyd.CurrentLocation = null; // Floyd is not in any location while wandering
    }

    private async Task<string> GenerateDepartureMessage(IContext context, IGenerationClient client)
    {
        return await floyd.GenerateCompanionSpeech(context, client, FloydPrompts.LeavingToExplore);
    }

    private async Task<string> GenerateReturnMessage(IContext context, IGenerationClient client)
    {
        return await floyd.GenerateCompanionSpeech(context, client, FloydPrompts.ReturningFromExploring);
    }
}
