using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Location;

namespace GameEngine.IntentEngine;

internal class ExitSubLocationEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not ExitSubLocationIntent exit)
            throw new ArgumentException("Cast error");

        if (string.IsNullOrEmpty(exit.NounOne))
            throw new ArgumentException("Null or empty noun. What's up with that?");

        var subLocation = Repository.GetItem(exit.NounOne);
        if (subLocation == null)
        {
            // Sometimes the name of the sub-location gets stuffed into the second noun. Stupid parser 

            if (!string.IsNullOrEmpty(exit.NounTwo))
            {
                subLocation = Repository.GetItem(exit.NounTwo);
                if (subLocation == null)
                    return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.NounOne));
            }
            else
            {
                return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.NounOne));
            }
        }

        if (subLocation is not ISubLocation subLocationInstance)
            return (null, await GetGeneratedCantGoThatWayResponse(generationClient, context, exit.NounOne));

        if (context.CurrentLocation.SubLocation != subLocationInstance)
            return (null, $"You're not in the {exit.NounOne}. ");

        return (null, subLocationInstance.GetOut(context));
    }

    private async Task<string> GetGeneratedCantGoThatWayResponse(IGenerationClient generationClient, IContext context,
        string noun)
    {
        var request = new CannotExitSubLocationRequest(context.CurrentLocation.GetDescription(context), noun);
        var result = await generationClient.GenerateNarration(request);
        return result;
    }
}