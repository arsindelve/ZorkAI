using Game.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Movement;

namespace Game.IntentEngine;

public class MoveEngine : IIntentEngine
{
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        // TODO: Move from a dark location to another dark location and you die. 

        if (intent is not MoveIntent moveTo)
            throw new ArgumentException("Cast error");

        var movement = context.CurrentLocation.Navigate(moveTo.Direction);

        if (movement == null)
            return await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context);

        if (movement.WeightLimit < context.CarryingWeight)
            return movement.WeightLimitFailureMessage;

        if (!movement.CanGo(context) || movement.Location == null)
            return !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? movement.CustomFailureMessage + Environment.NewLine
                : await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context);

        // Let's reset the noun context, so we don't get confused with "it" between locations
        context.LastNoun = "";

        return await Go(context, generationClient, movement);
    }

    public static Task<string> Go(IContext context, IGenerationClient generationClient, MovementParameters movement)
    {
        var previousLocation = context.CurrentLocation;
        context.CurrentLocation.OnLeaveLocation(context, movement.Location!, previousLocation);
        context.CurrentLocation = movement.Location!;

        var beforeEnteringText = movement.Location!.BeforeEnterLocation(context, previousLocation);
        var processorText = LookProcessor.LookAround(context);
        var afterEnteringText = movement.Location.AfterEnterLocation(context, previousLocation);

        var result = beforeEnteringText + processorText + afterEnteringText + Environment.NewLine;
        return Task.FromResult(result);
    }

    private static async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, string direction, 
        IContext context)
    {
        //return Task.FromResult("You cannot go that way." + Environment.NewLine);
        var request = new CannotGoThatWayRequest(context.CurrentLocation.Description, direction);
        var result = await generationClient.CompleteChat(request);
        return result;
    }
}