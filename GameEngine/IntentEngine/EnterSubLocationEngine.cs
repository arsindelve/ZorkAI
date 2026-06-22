using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Model.Location;

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
        {
            // Issue #268: "enter <room>" — when the noun isn't an item in scope, it may be the NAME of
            // an adjacent room ("enter the kitchen", "enter elevator"). Share the move/disambiguate flow
            // with "go to <room>"; a null result means no adjacent room matched, so we fall back to the
            // "enter"-flavoured refusal.
            return await DestinationNavigation.TryNavigate(enter.Noun, context, generationClient)
                   ?? (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));
        }

        if (subLocation is not ISubLocation subLocationInstance)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));

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