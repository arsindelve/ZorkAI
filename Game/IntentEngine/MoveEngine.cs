using Game.StaticCommand.Implementation;

namespace Game.IntentEngine;

internal class MoveEngine : IIntentEngine
{
    public Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not MoveIntent moveTo)
            throw new ArgumentException();

        var movement = context.CurrentLocation.Navigate(moveTo.Direction);

        if (movement == null)
            return GetGeneratedCantGoThatWayResponse();

        if (!movement.CanGo(context) || movement.Location == null)
            return !string.IsNullOrEmpty(movement.CustomFailureMessage)
                ? Task.FromResult(movement.CustomFailureMessage + Environment.NewLine)
                : GetGeneratedCantGoThatWayResponse();

        // Let's reset the noun context, so we don't get confused with "it" between locations
        context.LastNoun = "";

        context.CurrentLocation = movement.Location;
        movement.Location.OnEnterLocation(context);
        return Task.FromResult(new LookProcessor().Process(null, context) + Environment.NewLine);
    }

    private static Task<string> GetGeneratedCantGoThatWayResponse()
    {
        return Task.FromResult("You cannot go that way." + Environment.NewLine);
        // var request = new CannotGoThatWayRequest(_context.CurrentLocation.Description);
        // var result = await _generator.CompleteChat(request);
        // return result;
    }
}