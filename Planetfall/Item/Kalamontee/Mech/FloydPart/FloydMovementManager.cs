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

        if (floyd.WanderingTurnsRemaining != 0) return string.Empty; // Still wandering
        
        // Floyd returns to the player
        floyd.IsOffWandering = false;
        floyd.CurrentLocation = context.CurrentLocation as ICanContainItems;
        context.CurrentLocation.ItemPlacedHere(floyd);
        return await GenerateReturnMessage(context, client);
    }

    /// <summary>
    /// Handles Floyd's following behavior when the player moves to a different location.
    /// Returns a message if Floyd follows, starts wandering, or an empty string if he stays put.
    /// Also handles special location comments when Floyd enters a special location.
    /// </summary>
    public async Task<string> HandleFollowingPlayer(IContext context, IGenerationClient client)
    {
        // Don't follow if already wandering, or deliberately parked elsewhere by a scripted sequence
        // (e.g. the Bio Lab fight, which sets CurrentLocation = null to represent Floyd being absent).
        if (floyd.IsOffWandering || floyd.IsAwayOnScriptedSequence)
            return string.Empty;

        bool isInTheRoom = floyd.CurrentLocation == context.CurrentLocation;
        if (!isInTheRoom)
        {
            // Random chance to no longer follow (1 in 5 chance) - skipped when god mode has disabled
            // Floyd's random wandering for deterministic playtesting.
            var wanderingDisabled = context is PlanetfallContext { FloydWanderingDisabled: true };
            if (!wanderingDisabled && floyd.Chooser.RollDiceSuccess(5))
            {
                floyd.IsOffWandering = true;
                floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns
                floyd.CurrentLocation?.RemoveItem(floyd); // Remove Floyd from previous location
                floyd.CurrentLocation = null; // Floyd is not in any location while wandering
                return string.Empty; // No message - player just doesn't see "Floyd follows you"
            }

            // Normal follow behavior - remove from old location before adding to new
            floyd.CurrentLocation?.RemoveItem(floyd);
            context.CurrentLocation.ItemPlacedHere(floyd);

            // Check for special location comment
            var specialLocationComment = await CheckForSpecialLocationComment(context, client);

            return string.IsNullOrEmpty(specialLocationComment)
                ? "Floyd follows you. "
                : "Floyd follows you. " + specialLocationComment;
        }

        return string.Empty;
    }

    private async Task<string> CheckForSpecialLocationComment(IContext context, IGenerationClient client)
    {
        if (context.CurrentLocation is not FloydSpecialInteractionLocation specialLocation)
            return string.Empty;

        if (specialLocation.InteractionHasHappened)
            return string.Empty;

        // This one is an embellishment on "Floyd follows you. ", so it has no canned stand-in - it
        // just drops. What it must never do is take the follow announcement down with it (issue #549),
        // nor burn the location's one-shot flag on a beat the player never saw: the flag is spent only
        // once a line actually exists, so a failed call leaves the moment available to fire later.
        var comment = await GenerateOrFallBack(
            () => floyd.GenerateCompanionSpeech(context, client, specialLocation.FloydPrompt),
            () => string.Empty);

        if (string.IsNullOrWhiteSpace(comment))
            return string.Empty;

        specialLocation.InteractionHasHappened = true;
        return Environment.NewLine + Environment.NewLine + comment;
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

        // God mode may have disabled Floyd's random wandering for deterministic playtesting.
        if (context is PlanetfallContext { FloydWanderingDisabled: true }) return null;

        if (!floyd.Chooser.RollDiceSuccess(20)) return null;
        
        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns
        (context.CurrentLocation as ICanContainItems)?.RemoveItem(floyd);
        floyd.CurrentLocation = null; // Floyd is not in any location while wandering
        return await GenerateDepartureMessage(context, client);

    }

    /// <summary>
    /// Makes Floyd start wandering for a random number of turns (1-5).
    /// Floyd will be removed from his current location and will return to the player later.
    /// </summary>
    public void StartWandering(IContext context)
    {
        if (!floyd.IsAlive)
            return; // Can't wander if he's not on or dead

        floyd.IsOffWandering = true;
        floyd.WanderingTurnsRemaining = floyd.Chooser.RollDice(5); // 1-5 turns

        // Remove Floyd from the current location
        floyd.CurrentLocation?.RemoveItem(floyd);
        floyd.CurrentLocation = null; // Floyd is not in any location while wandering
    }

    private async Task<string> GenerateDepartureMessage(IContext context, IGenerationClient client)
    {
        return await GenerateOrFallBack(
            () => floyd.GenerateCompanionSpeech(context, client, FloydPrompts.LeavingToExplore),
            () => FloydConstants.GoingExploring);
    }

    private async Task<string> GenerateReturnMessage(IContext context, IGenerationClient client)
    {
        return await GenerateOrFallBack(
            () => floyd.GenerateCompanionSpeech(context, client, FloydPrompts.ReturningFromExploring),
            () => floyd.Chooser.Choose(FloydConstants.ReturnMessages.ToList()));
    }

    /// <summary>
    /// Runs a generated line, falling back to a canned one if the generation throws or comes back
    /// empty.
    /// </summary>
    /// <remarks>
    /// Every one of Floyd's comings and goings commits its state change - him being placed back in
    /// the room, or pulled out of it - BEFORE the line announcing it is generated. So when the
    /// generation call failed, the move still happened and the player was never told: Floyd simply
    /// rematerialized beside a player who sat through ten more "wait" turns believing he was still
    /// gone (issue #549). A thrown call was worse still, since the engine's top-level guard replaced
    /// the entire turn's output with its generic "something goes wrong" line.
    /// <para>
    /// A companion's movements are load-bearing state changes, so the announcement must never depend
    /// on a live LLM call succeeding - the same reasoning behind the canned fallback in
    /// <see cref="Floyd.OnBeingTalkedTo" />. The fallback is lazy so the chooser is only consulted on
    /// the path that actually needs it.
    /// </para>
    /// </remarks>
    private static async Task<string> GenerateOrFallBack(Func<Task<string>> generate, Func<string> fallback)
    {
        try
        {
            var message = await generate();
            return string.IsNullOrWhiteSpace(message) ? fallback() : message;
        }
        catch (Exception)
        {
            return fallback();
        }
    }
}
