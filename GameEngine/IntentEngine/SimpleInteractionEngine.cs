using System.Diagnostics;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Utilities;

namespace GameEngine.IntentEngine;

internal class SimpleInteractionEngine : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)>
        Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not SimpleIntent simpleInteraction)
            throw new ArgumentException();

        Debug.WriteLine(intent);
        context.LastNoun = simpleInteraction.Noun ?? "";

        DisambiguationInteractionResult? requireDisambiguation = CheckDisambiguation(simpleInteraction, context);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);

        // Turning on a light source should supersede all of this. Check the noun
        bool refersToALightSource = Repository.GetItem(simpleInteraction.Noun) is IAmALightSourceThatTurnsOnAndOff;

        // If it's dark, you can interact with items in your possession that are light sources that can be
        // turned on, but not any items in the room.
        if (context.ItIsDarkHere && !refersToALightSource)
            return (null, "It's too dark to see! ");

        // Ask the context if it knows what to do with this interaction. Usually, this will only 
        // be true if there is an available interaction with one of the items in inventory. 
        var locationInteraction =
            context.CurrentLocation.RespondToSimpleInteraction(simpleInteraction, context, generationClient);

        // We got a meaningful interaction in the location that changed the state of the game
        if (locationInteraction.InteractionHappened)
            return (locationInteraction, locationInteraction.InteractionMessage + Environment.NewLine);

        var contextInteraction = context.RespondToSimpleInteraction(simpleInteraction, generationClient);

        // We got a meaningful interaction from one of the items in inventory that changed the state of the game
        if (contextInteraction.InteractionHappened)
            return (contextInteraction, contextInteraction.InteractionMessage + Environment.NewLine);

        // The noun was present in the given LOCATION, but the verb applied to
        // it has no meaning in the story. I.E: push the sword...that will accomplish nothing. 
        if (locationInteraction is NoVerbMatchInteractionResult noVerb)
            return (locationInteraction,
                await GetGeneratedNoMatchingVerbResponse(noVerb.Noun, noVerb.Verb, generationClient, context));

        // The noun was present in INVENTORY but the verb applied to
        // it has no meaning in the story. I.E: push the sword...that will accomplish nothing. 
        if (contextInteraction is NoVerbMatchInteractionResult noVerbContext)
            return (contextInteraction, await GetGeneratedNoMatchingVerbResponse(noVerbContext.Noun, noVerbContext.Verb,
                generationClient,
                context));

        // The noun exists in the game, but is not currently present. It might be in another location
        // or is hidden inside something else (like the leaflet in the mailbox) 
        if (Repository.ItemExistsInTheStory(simpleInteraction.Noun))
            return (null, await GetGeneratedNounNotPresentResponse(simpleInteraction.Noun, generationClient, context));

        // There is no matching noun at all, anywhere in the game. The user might have
        // talked about a unicorn, a bottle of tequila or some other meaningless item. 
        return (null, await GetGeneratedNoOpResponse(simpleInteraction.OriginalInput ?? "", generationClient, context));
    }

    private DisambiguationInteractionResult? CheckDisambiguation(SimpleIntent intent, IContext context)
    {
        var ambiguousItems = new List<IItem>();

        IEnumerable<IItem> allItemsInSight =
            context.GetAllItemsRecursively
                .Union((context.CurrentLocation as ICanHoldItems)!.GetAllItemsRecursively)
                .ToList();

        foreach (var item in allItemsInSight)
            if (intent.MatchNounAndAdjective(item.NounsForMatching))
                ambiguousItems.Add(item);

        // We have one or fewer items that match the noun. Good to go. 
        if (ambiguousItems.Count <= 1)
            return null;

        var itemNouns = ambiguousItems
            .Select(s => s.NounsForMatching.MaxBy(n => n.Length))
            .ToList()!
            .SingleLineListWithOr();
        var message = $"Do you mean {itemNouns}?";

        // For each item, we need a map of all possible nouns, to the longest noun, and then 
        // we will replace the matching noun with the longest noun. If we don't do
        // this, we'll loop around disambiguating forever. 
        var nounToLongestNounMap = new Dictionary<string, string>();
        foreach (var item in ambiguousItems)
        {
            string? longestNoun = item.NounsForPreciseMatching.MaxBy(noun => noun.Length);
            foreach (var noun in item.NounsForPreciseMatching)
            {
                nounToLongestNounMap[noun] = longestNoun ?? string.Empty;
            }
        }

        var replacement = intent.Verb + " {0}";
        
        return new DisambiguationInteractionResult(
            message,
            nounToLongestNounMap,
            replacement
        );
    }

    private static async Task<string> GetGeneratedNoMatchingVerbResponse(string? noun, string verb,
        IGenerationClient generationClient, IContext context)
    {
        if (string.IsNullOrEmpty(noun))
            return string.Empty;

        IItem? item = Repository.GetItem(noun);
        if (item is null)
            return string.Empty;

        Request request;

        if (item is not IAmANamedPerson)
            request = new VerbHasNoEffectOperationRequest(context.CurrentLocation.DescriptionForGeneration, noun, verb);
        else
            request = new VerbHasNoEffectOnAPersonOperationRequest(context.CurrentLocation.DescriptionForGeneration, noun, verb, item.GenericDescription(context.CurrentLocation));

        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNounNotPresentResponse(string? noun,
        IGenerationClient generationClient, IContext context)
    {
        var request = new NounNotPresentOperationRequest(context.CurrentLocation.DescriptionForGeneration, noun);
        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        IContext context)
    {
        var request =
            new CommandHasNoEffectOperationRequest(context.CurrentLocation.DescriptionForGeneration, input);
        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
        return result;
    }
}