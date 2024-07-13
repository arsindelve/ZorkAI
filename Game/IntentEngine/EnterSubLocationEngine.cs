using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace Game.IntentEngine;

internal class EnterSubLocationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not EnterSubLocationIntent enter)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(enter.Noun))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        IItem? subLocation = Repository.GetItem(enter.Noun);
        if (subLocation == null)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));

        if (subLocation is not ISubLocation subLocationInstance)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, enter.Noun));

        if (context.CurrentLocation.SubLocation == subLocationInstance)
            return (null, $"You're already in the {enter.Noun}. ");
        
        return (null, subLocationInstance.GetIn(context));
    }

    private static async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient,
        IContext context, string noun)
    {
        //return Task.FromResult("You cannot go that way." + Environment.NewLine);
        var request = new CannotEnterSubLocationRequest(context.CurrentLocation.Description, noun);
        var result = await generationClient.CompleteChat(request);
        return result;
    }
}