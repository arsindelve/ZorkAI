using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Location;
using Model.Movement;

namespace GameEngine.IntentEngine;

internal class EnterSubLocationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not EnterSubLocationIntent enter)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(enter.Noun))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        IItem? subLocation = Repository.GetItemInScope(enter.Noun, context);
        if (subLocation == null)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));

        if (subLocation is not ISubLocation subLocationInstance)
        {
            // The noun resolved to a real, in-scope item that isn't a sub-location (issue #262). If
            // it's a passage door, "enter <door>" means "go through it" - defer to movement
            // (Direction.In) so the bare noun "pod" behaves like the full phrase "escape pod" (a
            // Move), instead of a generic refusal the narrator dresses up as a mock. See DoorReroute
            // for what counts as a door and why.
            var reroute = await DoorReroute.TryProcess(subLocation, Direction.In, context, generationClient);
            if (reroute is not null)
                return reroute.Value;

            // Not a door - you simply can't enter it. Say so plainly rather than mocking a valid noun.
            return (null, "You can't enter that. ");
        }

        if (context.CurrentLocation.SubLocation == subLocationInstance)
            return (null, $"You're already in the {enter.Noun}. ");

        return (null, subLocationInstance.GetIn(context));
    }

    private static async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient,
        IContext context, string noun)
    {
        // If generation is disabled, return standard response
        if (generationClient.IsDisabled)
            return "You cannot go that way. ";

        var request = new CannotEnterSubLocationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), noun);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result;
    }
}