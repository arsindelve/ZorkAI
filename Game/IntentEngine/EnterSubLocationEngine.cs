using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Location;

namespace Game.IntentEngine;

internal class EnterSubLocationEngine : IIntentEngine
{
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not EnterSubLocationIntent enter)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(enter.Noun))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        var subLocation = Repository.GetItem(enter.Noun);
        if (subLocation == null)
            return await GetGeneratedCantGoThatWayResponse(generationClient, context);

        if (subLocation is not ISubLocation subLocationInstance)
            return await GetGeneratedCantGoThatWayResponse(generationClient, context);

        return subLocationInstance.GetIn(context);
    }

    private static async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient,
        IContext context)
    {
        //return Task.FromResult("You cannot go that way." + Environment.NewLine);
        var request = new CannotEnterSubLocationRequest(context.CurrentLocation.Description);
        var result = await generationClient.CompleteChat(request);
        return result;
    }
}