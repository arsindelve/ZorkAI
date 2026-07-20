using System.Diagnostics;
using Model;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Utilities;

namespace GameEngine.IntentEngine;

/// <summary>
/// Represents an engine responsible for processing simple interaction intents
/// in the game's context. This engine utilizes an ItemProcessorFactory for handling
/// interactions with various game items. Implements IIntentEngine for processing intents.
/// </summary>
internal class SimpleInteractionEngine(IItemProcessorFactory itemProcessorFactory) : IIntentEngine
{
    public async Task<(InteractionResult? resultObject, string ResultMessage)>
        Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not SimpleIntent simpleInteraction)
            throw new ArgumentException();

        Debug.WriteLine(intent);
        context.LastNoun = simpleInteraction.Noun ?? "";

        // "them" tracks only a contiguous run of take/drop commands; any other interaction (examine,
        // open, push, ...) ends that group, mirroring how LastNoun is overwritten every turn. Without
        // this, an item taken long ago would be swept into a later "drop them" (issue #248).
        var verb = simpleInteraction.Verb?.ToLowerInvariant().Trim() ?? string.Empty;
        if (!Verbs.TakeVerbs.Contains(verb) && !Verbs.DropVerbs.Contains(verb))
            context.LastNouns = [];

        DisambiguationInteractionResult? requireDisambiguation = CheckDisambiguation(simpleInteraction, context);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);

        // Turning on a light source should supersede all of this. Check the noun
        bool refersToALightSource = Repository.GetItemInScope(simpleInteraction.Noun, context) is IAmALightSourceThatTurnsOnAndOff;

        // If it's dark, you can interact with items in your possession that are light sources that can be
        // turned on, but not any items in the room.
        if (context.ItIsDarkHere && !refersToALightSource)
            return (null, "It's too dark to see! ");

        // Ask the context if it knows what to do with this interaction. Usually, this will only 
        // be true if there is an available interaction with one of the items in inventory. 
        var locationInteraction =
            await context.CurrentLocation!.RespondToSimpleInteraction(simpleInteraction, context, generationClient, itemProcessorFactory);

        // We got a meaningful interaction in the location that changed the state of the game
        if (locationInteraction.InteractionHappened)
            return (locationInteraction, locationInteraction.InteractionMessage + Environment.NewLine);

        var contextInteraction = await context.RespondToSimpleInteraction(simpleInteraction, generationClient, itemProcessorFactory);

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
        {
            // Issue #136 Hook A: before settling for flavor text, give the agentic narrator a chance
            // to resolve an unhandled action on a HELD item into a real DROP/DESTROY state change
            // ("throw the leaflet in the air", "tear up the leaflet"). Room items (the location
            // branch above) deliberately stay narration-only.
            var agentic = await AgenticActionHandler.TryResolveAgenticAction(
                simpleInteraction.OriginalInput ?? $"{simpleInteraction.Verb} {simpleInteraction.Noun}",
                noVerbContext.Noun, context, itemProcessorFactory, contextInteraction);
            if (agentic is not null)
                return agentic.Value;

            return (contextInteraction, await GetGeneratedNoMatchingVerbResponse(noVerbContext.Noun,
                noVerbContext.Verb,
                generationClient,
                context));
        }

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
                .Union((context.CurrentLocation as ICanContainItems)!.GetAllItemsRecursively)
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
        var location = context.CurrentLocation.GetDescriptionForGeneration(context);

        // We only get here because a verb landed on something present but no processor handled it, so
        // we MUST narrate a no-effect interaction rather than returning a blank line. Two ways the
        // noun can be unusable here previously short-circuited to "" and printed a blank line (#282):
        //
        //   * The noun is empty (a verb-only NoVerbMatch). Fall back to the generic "that command
        //     does nothing" narration, which needs no noun.
        //   * GetItemInScope can't resolve the noun even though it matched a present object - e.g. the
        //     escape-pod BulkheadDoor, one shared instance seeded into both Deck Nine and the Escape
        //     Pod, whose CurrentLocation is the pod even while you stand on Deck Nine, so the
        //     accessibility check rejects it. The resolved item is only needed to pick the
        //     person-specific prompt; when it is null we use the generic "verb has no effect"
        //     narration.
        Request request;
        if (string.IsNullOrEmpty(noun))
        {
            request = new CommandHasNoEffectOperationRequest(location, verb);
        }
        else
        {
            IItem? item = Repository.GetItemInScope(noun, context);
            request = item is IAmANamedPerson
                ? new VerbHasNoEffectOnAPersonOperationRequest(location, noun, verb, item.GenericDescription(context.CurrentLocation))
                : new VerbHasNoEffectOperationRequest(location, noun, verb);
        }

        return await generationClient.GenerateNarration(request, context.SystemPromptAddendum) + Environment.NewLine;
    }

    private static async Task<string> GetGeneratedNounNotPresentResponse(string? noun,
        IGenerationClient generationClient, IContext context)
    {
        var request = new NounNotPresentOperationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), noun);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        IContext context)
    {
        var request =
            new CommandHasNoEffectOperationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), input);
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum) + Environment.NewLine;
        return result;
    }
}