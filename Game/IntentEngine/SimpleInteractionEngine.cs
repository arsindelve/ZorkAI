using System.Diagnostics;

namespace Game.IntentEngine;

internal class SimpleInteractionEngine : IIntentEngine
{
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not SimpleIntent simpleInteraction)
            throw new ArgumentException();

        Debug.WriteLine(intent);
        context.LastNoun = simpleInteraction.Noun ?? "";
        InteractionResult locationInteraction;

        var contextInteraction = context.RespondToSimpleInteraction(simpleInteraction, generationClient);

        // We got a meaningful interaction from one of the items in inventory that changed the state of the game
        if (contextInteraction.InteractionHappened)
            return contextInteraction.InteractionMessage + Environment.NewLine;

        // If it's dark, you can interact with items in your possession (above), but not items in the room.
        // There will be no more processing in a dark room. 
        if (context.ItIsDarkHere)
            return "It's too dark to see! ";

        // Ask the context if it knows what to do with this interaction. Usually, this will only 
        // be true if there is an available interaction with one of the items in inventory. 
        locationInteraction =
            context.CurrentLocation.RespondToSimpleInteraction(simpleInteraction, context, generationClient);

        // We got a meaningful interaction in the location that changed the state of the game
        if (locationInteraction.InteractionHappened)
            return locationInteraction.InteractionMessage + Environment.NewLine;


        // The noun was present in the given LOCATION, but the verb applied to
        // it has no meaning in the story. I.E: push the sword...that will accomplish nothing. 
        if (locationInteraction is NoVerbMatchInteractionResult noVerb)
            return await GetGeneratedNoMatchingVerbResponse(noVerb.Noun, noVerb.Verb, generationClient, context);

        // The noun was present in INVENTORY but the verb applied to
        // it has no meaning in the story. I.E: push the sword...that will accomplish nothing. 
        if (contextInteraction is NoVerbMatchInteractionResult noVerbContext)
            return await GetGeneratedNoMatchingVerbResponse(noVerbContext.Noun, noVerbContext.Verb, generationClient,
                context);

        // The noun exists in the game, but is not currently present. It might be in another location
        // or is hidden inside something else (like the leaflet in the mailbox) 
        if (Repository.ItemExistsInTheStory(simpleInteraction.Noun))
            return await GetGeneratedNounNotPresentResponse(simpleInteraction.Noun, generationClient, context);

        // There is no matching noun at all, anywhere in the game. The user might have
        // talked about a unicorn, a bottle of tequila or some other meaningless item. 
        return await GetGeneratedNoOpResponse(simpleInteraction.OriginalInput, generationClient, context);
    }

    private static async Task<string> GetGeneratedNoMatchingVerbResponse(string? noun, string verb,
        IGenerationClient generationClient, IContext context)
    {
        var request =
            new VerbHasNoEffectOperationRequest(context.CurrentLocation.DescriptionForGeneration, noun, verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNounNotPresentResponse(string? noun,
        IGenerationClient generationClient, IContext context)
    {
        var request = new NounNotPresentOperationRequest(context.CurrentLocation.DescriptionForGeneration, noun);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        IContext context)
    {
        var request =
            new CommandHasNoEffectOperationRequest(context.CurrentLocation.DescriptionForGeneration, input);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        return result;
    }
}