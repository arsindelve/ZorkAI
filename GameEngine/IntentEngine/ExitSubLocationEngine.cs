using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Location;
using Model.Movement;

namespace GameEngine.IntentEngine;

internal class ExitSubLocationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not ExitSubLocationIntent exit)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(exit.NounOne))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        // Model 2: CurrentLocation IS the sub-location (e.g., Planetfall BedLocation)
        // Check this first because the item might not be in scope from the sub-location
        if (context.CurrentLocation is ISubLocation currentAsSubLocation)
            return (null, currentAsSubLocation.GetOut(context));

        var subLocation = Repository.GetItemInScope(exit.NounOne, context);
        if (subLocation == null)
        {
            // Sometimes the name of the sub-location gets stuffed into the second noun. Stupid parser

            if (!string.IsNullOrEmpty(exit.NounTwo))
            {
                subLocation = Repository.GetItemInScope(exit.NounTwo, context);
                if (subLocation == null)
                    return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.NounOne));
            }
            else
            {
                return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.NounOne));
            }
        }

        if (subLocation is not ISubLocation subLocationInstance)
        {
            // Symmetric to EnterSubLocationEngine (issue #262): "exit <door>" means "go out through
            // it" -> Move(Out), so "exit window" from the Kitchen leaves the same way "enter window"
            // from Behind House arrives. Same door definition (see DoorReroute).
            var reroute = await DoorReroute.TryProcess(subLocation, Direction.Out, context, generationClient);
            if (reroute is not null)
                return reroute.Value;

            // Not a door - you simply can't exit it. Say so plainly (matches the enter side, #262).
            return (null, "You can't exit that. ");
        }

        // Model 1: Parent location with SubLocation property set (e.g., ZorkOne boat)
        if (context.CurrentLocation.SubLocation == subLocationInstance)
            return (null, subLocationInstance.GetOut(context));

        return (null, $"You're not in the {exit.NounOne}. ");
    }

    private async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, IContext context,
        string noun)
    {
        // If generation is disabled, return standard response
        if (generationClient.IsDisabled)
            return "You cannot go that way. ";

        var request = new CannotExitSubLocationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), noun);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result;
    }
}