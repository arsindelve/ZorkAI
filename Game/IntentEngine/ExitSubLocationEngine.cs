using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Location;

namespace Game.IntentEngine;

internal class ExitSubLocationEngine : IIntentEngine
{
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not ExitSubLocationIntent exit)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(exit.Noun))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        var subLocation = Repository.GetItem(exit.Noun);
        if (subLocation == null)
            return await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.Noun);

        if (subLocation is not ISubLocation subLocationInstance)
            return await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.Noun);
      
        if (context.CurrentLocation.SubLocation != subLocationInstance)
            return $"You're not in the {exit.Noun}. ";
        
        return subLocationInstance.GetOut(context);
    }

    private async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, IContext context, string noun)
    {
        var request = new CannotExitSubLocationRequest(context.CurrentLocation.Description, noun);
        var result = await generationClient.CompleteChat(request);
        return result;
    }
}