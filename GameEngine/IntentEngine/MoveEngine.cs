using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Movement;

namespace GameEngine.IntentEngine;

public class MoveEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        // TODO: Move from a dark location to another dark location and you die. 

        if (intent is not MoveIntent moveTo)
            throw new ArgumentException("Cast error");

        MovementParameters? movement = context.CurrentLocation.Navigate(moveTo.Direction);

        if (movement == null)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));

        if (movement.WeightLimit < context.CarryingWeight)
            return (null, movement.WeightLimitFailureMessage);

        if (!movement.CanGo(context) || movement.Location == null)
            return (null, !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? movement.CustomFailureMessage + Environment.NewLine
                : await GetGeneratedCantGoThatWayResponse(generationClient, moveTo.Direction.ToString(), context));

        // Let's reset the noun context, so we don't get confused with "it" between locations
        context.LastNoun = "";

        return (null, await Go(context, generationClient, movement));
    }

    public static async Task<string> Go(IContext context, IGenerationClient generationClient, MovementParameters movement)
    {
        var previousLocation = context.CurrentLocation;
        context.CurrentLocation.OnLeaveLocation(context, movement.Location!, previousLocation);
        context.CurrentLocation = movement.Location!;

        var beforeEnteringText = movement.Location!.BeforeEnterLocation(context, previousLocation);
        var processorText = LookProcessor.LookAround(context);
        var afterEnteringText = await movement.Location.AfterEnterLocation(context, previousLocation, generationClient);

        var result = beforeEnteringText + processorText + afterEnteringText + Environment.NewLine;
        return result;
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