using System.Reflection;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Movement;

namespace GameEngine.IntentEngine;

public class MoveEngine : IIntentEngine
{
    internal virtual IRandomChooser Chooser => new RandomChooser();
    
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not MoveIntent moveTo)
            throw new ArgumentException("Cast error");

        context.LastMovementDirection = moveTo.Direction;

        MovementParameters? movement = context.CurrentLocation.Navigate(moveTo.Direction, context);

        if (movement == null)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));

        if (movement.WeightLimit < context.CarryingWeight)
            return (null, movement.WeightLimitFailureMessage);

        if (!movement.CanGo(context) || movement.Location == null)
            return (null, !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? movement.CustomFailureMessage + Environment.NewLine
                : await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));

        // Check if we're currently in a dark location before moving
        var wasInDarkLocation = context.ItIsDarkHere;

        // Let's reset the noun context, so we don't get confused with "it" between locations
        context.LastNoun = "";

        var moveResult = await Go(context, generationClient, movement);

        // After moving, check if we're now in a dark location too
        // If moving from dark to dark, there's a 50% chance of being eaten by a grue
        if (wasInDarkLocation && context.ItIsDarkHere && Chooser.RollDiceSuccess(2))
        {
            var deathResult = HandleDarkLocationDeath(context);
            if (deathResult != null)
                return (deathResult, "");
        }

        return (null, moveResult);
    }

    public static async Task<string> Go(IContext context, IGenerationClient generationClient, MovementParameters movement)
    {
        var previousLocation = context.CurrentLocation;
        context.CurrentLocation.OnLeaveLocation(context, movement.Location!, previousLocation);
        context.CurrentLocation = movement.Location!;

        var transitionMessage = movement.TransitionMessage ?? string.Empty;
        var beforeEnteringText = movement.Location!.BeforeEnterLocation(context, previousLocation);
        var processorText = LookProcessor.LookAround(context);
        var afterEnteringText = await movement.Location.AfterEnterLocation(context, previousLocation, generationClient);

        var result = transitionMessage + beforeEnteringText + processorText + afterEnteringText + Environment.NewLine;
        return result;
    }

    private async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, string direction,
        IContext context)
    {
        // 20% of the time, let's generate a response. Otherwise, give the standard response

        if (!Chooser.RollDiceSuccess(5))
            return "You cannot go that way. ";

        var request =
            new CannotGoThatWayRequest(context.CurrentLocation.GetDescriptionForGeneration(context), direction);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result;
    }

    /// <summary>
    /// Handles death when moving from a dark location to another dark location (eaten by a grue).
    /// Uses reflection to call the game-specific DeathProcessor.
    /// </summary>
    private InteractionResult? HandleDarkLocationDeath(IContext context)
    {
        try
        {
            // Get the game name from context to find the appropriate DeathProcessor
            var gameName = context.Game.GameName;

            // Find the DeathProcessor type in the game's assembly
            var deathProcessorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "DeathProcessor" && t.Namespace?.StartsWith(gameName) == true);

            if (deathProcessorType == null)
                return null;

            // Create an instance of the DeathProcessor
            var deathProcessor = Activator.CreateInstance(deathProcessorType);

            // Find the Process method
            var processMethod = deathProcessorType.GetMethod("Process", new[] { typeof(string), typeof(IContext) });

            if (processMethod == null)
                return null;

            // Call the Process method with the grue death message
            var deathMessage = "Oh no! You have walked into the slavering fangs of a lurking grue!";
            var result = processMethod.Invoke(deathProcessor, new object[] { deathMessage, context });

            return result as InteractionResult;
        }
        catch
        {
            // If reflection fails, return null and the game continues normally
            return null;
        }
    }
}