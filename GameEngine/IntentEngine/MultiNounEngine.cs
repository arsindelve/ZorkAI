using GameEngine.Item.ItemProcessor;
using GameEngine.Item.MultiItemProcessor;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Utilities;

namespace GameEngine.IntentEngine;

/// <summary>
///     This is the engine for processing any multi-noun intents such as
///     "put thing1 in thing2" or "kill thing1 with sharp-thing-2"
/// </summary>
/// <param name="itemProcessorFactory">
///     Carries the agentic fall-through narrator seam (issue #136). Optional: without it the engine
///     keeps the narration-only fall-through behavior.
/// </param>
public class MultiNounEngine(IItemProcessorFactory? itemProcessorFactory = null) : IIntentEngine
{
    private readonly List<IMultiNounVerbProcessor> _processors = [new PutProcessor()];

    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent,
        IContext context, IGenerationClient generationClient)
    {
        if (intent is not MultiNounIntent interaction)
            throw new ArgumentException();

        if (context.ItIsDarkHere)
            return (null, "It's too dark to see! ");

        // After a multi-noun interaction, we lose the ability to understand "it". It isn't a
        // take/drop either, so it also ends any "them" group (issue #248).
        context.LastNoun = "";
        context.LastNouns = [];

        var requireDisambiguation = CheckDisambiguation(interaction, context, interaction.MatchNounOne);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);
        
        requireDisambiguation = CheckDisambiguation(interaction, context, interaction.MatchNounTwo);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);

        // Does the location itself have a positive interaction? 
        var result = await context.CurrentLocation.RespondToMultiNounInteraction(interaction, context);
        if (result?.InteractionHappened ?? false)
            return (result, result.InteractionMessage);

        // Do any of the items on the ground have a positive interaction? 
        if (context.CurrentLocation is ICanContainItems items)
            foreach (var nextItem in items.Items)
            {
                result = await nextItem.RespondToMultiNounInteraction(interaction, context);
                if (result?.InteractionHappened ?? false)
                    return (result, result.InteractionMessage);
            }

        // Do any of the items in inventory have a positive interaction? 
        foreach (var nextItem in context.Items)
        {
            result = await nextItem.RespondToMultiNounInteraction(interaction, context);
            if (result?.InteractionHappened ?? false)
                return (result, result.InteractionMessage);
        }

        return await ProcessNonLocationTwoItemInteraction(context, generationClient, interaction);
    }

    private async Task<(InteractionResult? resultObject, string ResultMessage)> ProcessNonLocationTwoItemInteraction(
        IContext context,
        IGenerationClient generationClient,
        MultiNounIntent interaction)
    {
        // There is no matching noun one or noun two at all, anywhere in the game. The user might have
        // talked about a unicorn, a bottle of tequila or some other meaningless item. 
        if (!Repository.ItemExistsInTheStory(interaction.NounOne) &&
            !Repository.ItemExistsInTheStory(interaction.NounTwo))
            return (null, await GetGeneratedNoOpResponse(interaction.OriginalInput, generationClient, context));

        var (nounOneExistsHere, itemOne) = IsItemHere(context, interaction.NounOne);
        var (nounTwoExistsHere, itemTwo) = IsItemHere(context, interaction.NounTwo);

        interaction.ItemOne = itemOne;
        interaction.ItemTwo = itemTwo;

        if (!nounOneExistsHere & nounTwoExistsHere)
        {
            if (itemTwo is IAmANamedPerson)
                return (null, await GetGeneratedResponse<MissingFirstNounMultiNounWithPersonOperationRequest>(
                    interaction,
                    generationClient,
                    context));

            return (null, await GetGeneratedResponse<MissingFirstNounMultiNounOperationRequest>(interaction,
                generationClient,
                context));
        }

        if (nounOneExistsHere & !nounTwoExistsHere)
        {
            // Issue #136 Hook B: "throw the sword into the chasm" - the destination usually isn't a
            // real item, so this missing-second-noun branch would narrate "there's no chasm here"
            // before the generic fall-through ever runs. When the acted-upon first noun is a HELD
            // item, consult the agentic narrator first; the destination is context for its decision,
            // never a target.
            var agentic = await TryAgenticAction(interaction, context);
            if (agentic is not null)
                return agentic.Value;

            if (itemOne is IAmANamedPerson)
                return (null, await GetGeneratedResponse<MissingSecondNounWithPersonMultiNounOperationRequest>(
                    interaction,
                    generationClient,
                    context));

            return (null, await GetGeneratedResponse<MissingSecondNounMultiNounOperationRequest>(interaction,
                generationClient,
                context));
        }

        if (!nounOneExistsHere & !nounTwoExistsHere)
            return (null, await GetGeneratedResponse<MissingBothNounsMultiNounOperationRequest>(interaction,
                generationClient,
                context));

        // This indicates that one of the two items is not real, i.e. it's part of
        // the location description like the kitchen table. No real interaction is
        // possible.
        if (itemOne is null || itemTwo is null)
        {
            // Issue #136 Hook B: same seam when the destination is scenery from the room description
            // ("throw the sword into the river" with a river actually described here).
            var agentic = await TryAgenticAction(interaction, context);
            if (agentic is not null)
                return agentic.Value;

            return (null, await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
                context));
        }

        // Let all the processors decide if they can handle this interaction.
        foreach (var processor in _processors)
        {
            var result = processor.Process(interaction, context, itemOne,
                itemTwo);

            if (result is { InteractionHappened: true })
                return (result, result.InteractionMessage);
        }

        // Issue #136 Hook B: both nouns are real items but nothing modelled the action
        // ("throw the sword at the mailbox") - last chance before the generic no-effect narration.
        var agenticFallThrough = await TryAgenticAction(interaction, context);
        if (agenticFallThrough is not null)
            return agenticFallThrough.Value;

        // If not positive interaction.....
        return (null, await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
            context));
    }

    /// <summary>
    ///     Issue #136: consult the agentic fall-through narrator for an unhandled multi-noun action.
    ///     Gated (inside the handler) on NounOne resolving to an item in the player's inventory; a
    ///     null return means "keep the existing narration-only path". The no-tools result object is
    ///     null to match this engine's other fall-through returns.
    /// </summary>
    private async Task<(InteractionResult? resultObject, string ResultMessage)?> TryAgenticAction(
        MultiNounIntent interaction, IContext context)
    {
        return await AgenticActionHandler.TryResolveAgenticAction(interaction.OriginalInput, interaction.NounOne,
            context, itemProcessorFactory, null);
    }

    private static (bool IsHere, IItem? item) IsItemHere(IContext context, string item)
    {
        // Issue #246: an adjective-qualified noun ("kitchen card", "good bedistor") must resolve to
        // the precise item, not the first raw-containment match (a shuttle "card" is contained in
        // "kitchen card"; a fused "bedistor" in "good bedistor"). Mirror the single-noun #244 fix by
        // giving the shared adjective-aware pass priority over the containment fallback below. The
        // existing inventory-first fallback is left untouched so plain, non-adjective nouns behave
        // exactly as before.
        //
        // Note the precise pass searches room-before-inventory (matching GetItemInScope), while the
        // fallback below stays inventory-before-room - intentional, and not a contradiction to
        // "fix": the two orders can only disagree when one noun is an exact precise-noun match on
        // two different in-scope items at once, and Process() runs CheckDisambiguation (over the
        // same scope) before we ever get here, so that collision is already turned into a "do you
        // mean..." prompt rather than a silent pick.
        var preciseMatch = Repository.GetPreciseMatchInScope(item, context);
        if (preciseMatch is not null)
            return (true, preciseMatch);

        var result = context.HasMatchingNoun(item);
        if (result.HasItem)
            return result;

        result = context.CurrentLocation.HasMatchingNoun(item);
        if (result.HasItem)
            return result;

        return
            (context.CurrentLocation.GetDescriptionForGeneration(context).ToLower().Contains(item.ToLowerInvariant()), null);
    }

    private async Task<string> GetGeneratedVerbNotUsefulResponse(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context)
    {
        Request request = new VerbHasNoEffectMultiNounOperationRequest(context.CurrentLocation.GetDescriptionForGeneration(context),
            interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);

        if (interaction.ItemOne is IAmANamedPerson personOne)
            request = new VerbHasNoEffectMultiNounOperationItemOneIsAPersonRequest(
                context.CurrentLocation.GetDescriptionForGeneration(context),
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb,
                personOne.ExaminationDescription);

        if (interaction.ItemTwo is IAmANamedPerson personTwo)
            request = new VerbHasNoEffectMultiNounOperationItemTwoIsAPersonRequest(
                context.CurrentLocation.GetDescriptionForGeneration(context),
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb,
                personTwo.ExaminationDescription);

        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum) + Environment.NewLine;
        return result;
    }

    private async Task<string> GetGeneratedResponse<T>(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context) where T : MultiNounRequest, new()
    {
        var request =
            new T
            {
                Location = context.CurrentLocation.GetDescriptionForGeneration(context),
                NounOne = interaction.NounOne,
                NounTwo = interaction.NounTwo,
                Preposition = interaction.Preposition,
                Verb = interaction.Verb,
                PersonOneDescription = interaction.ItemOne is IAmANamedPerson personOne
                    ? personOne.ExaminationDescription
                    : string.Empty,
                PersonTwoDescription = interaction.ItemTwo is IAmANamedPerson personTwo
                    ? personTwo.ExaminationDescription
                    : string.Empty
            };

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

    private DisambiguationInteractionResult? CheckDisambiguation(MultiNounIntent intent,
        IContext context, Func<string[], bool> matchFunction)
    {
        var ambiguousItems = new List<IItem>();

        List<IItem>? allItemsInLocation = (context.CurrentLocation as ICanContainItems)?.GetAllItemsRecursively;
        if (allItemsInLocation is null)
            return null;
        
        IEnumerable<IItem> allItemsInSight =
            context.GetAllItemsRecursively
                .Union(allItemsInLocation)
                .ToList();
        
        foreach (var item in allItemsInSight)
            if (matchFunction(item.NounsForMatching))
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
            var longestNoun = item.NounsForPreciseMatching.MaxBy(noun => noun.Length);
            foreach (var noun in item.NounsForPreciseMatching) nounToLongestNounMap[noun] = longestNoun ?? string.Empty;
        }

        var replacement = intent.OriginalInput.Replace(intent.NounOne, "{0}");

        return new DisambiguationInteractionResult(
            message,
            nounToLongestNounMap,
            replacement
        );
    }
}