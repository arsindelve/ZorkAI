using Game.StaticCommand.Implementation;

namespace Game.IntentEngine;

internal class MoveEngine : IIntentEngine
{
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        // TODO: Move from a dark location to another dark location and you die. 

        if (intent is not MoveIntent moveTo)
            throw new ArgumentException("Cast error");

        var movement = context.CurrentLocation.Navigate(moveTo.Direction);

        if (movement == null)
            return await GetGeneratedCantGoThatWayResponse();

        if (!movement.CanGo(context) || movement.Location == null)
            return !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? movement.CustomFailureMessage + Environment.NewLine
                : await GetGeneratedCantGoThatWayResponse();

        // Let's reset the noun context, so we don't get confused with "it" between locations
        context.LastNoun = "";

        context.CurrentLocation = movement.Location;

        var beforeEnteringText = movement.Location.BeforeEnterLocation(context);
        var processorText = await new LookProcessor().Process(null, context, generationClient);
        var afterEnteringText = movement.Location.AfterEnterLocation(context);

        var result = beforeEnteringText + processorText + afterEnteringText + Environment.NewLine;
        return result;
    }

    private static Task<string> GetGeneratedCantGoThatWayResponse()
    {
        return Task.FromResult("You cannot go that way." + Environment.NewLine);
        // var request = new CannotGoThatWayRequest(_context.CurrentLocation.Description);
        // var result = await _generator.CompleteChat(request);
        // return result;
    }
}