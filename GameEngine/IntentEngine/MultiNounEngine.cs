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
public class MultiNounEngine : IIntentEngine
{
    private readonly List<IMultiNounVerbProcessor> _processors = [new PutProcessor()];

    public async Task<(InteractionResult? resultObject, string ResultMessage)> Process(IntentBase intent,
        IContext context, IGenerationClient generationClient)
    {
        if (intent is not MultiNounIntent interaction)
            throw new ArgumentException();

        if (context.ItIsDarkHere)
            return (null, "It's too dark to see! ");

        // After a multi-noun interaction, we will lose the ability to understand "it". 
        context.LastNoun = "";

        var requireDisambiguation = CheckDisambiguation(interaction, context, interaction.MatchNounOne);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);
        
        requireDisambiguation = CheckDisambiguation(interaction, context, interaction.MatchNounTwo);
        if (requireDisambiguation is not null)
            return (requireDisambiguation, requireDisambiguation.InteractionMessage);

        // Does the location itself have a positive interaction? 
        var result = context.CurrentLocation.RespondToMultiNounInteraction(interaction, context);
        if (result?.InteractionHappened ?? false)
            return (result, result.InteractionMessage);

        // Do any of the items on the ground have a positive interaction? 
        if (context.CurrentLocation is ICanHoldItems items)
            foreach (var nextItem in items.Items)
            {
                result = nextItem.RespondToMultiNounInteraction(interaction, context);
                if (result?.InteractionHappened ?? false)
                    return (result, result.InteractionMessage);
            }

        // Do any of the items in inventory have a positive interaction? 
        foreach (var nextItem in context.Items)
        {
            result = nextItem.RespondToMultiNounInteraction(interaction, context);
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
            return (null, await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
                context));

        // Let all the processors decide if they can handle this interaction. 
        foreach (var processor in _processors)
        {
            var result = processor.Process(interaction, context, itemOne,
                itemTwo);

            if (result is { InteractionHappened: true })
                return (result, result.InteractionMessage);
        }

        // If not positive interaction.....
        return (null, await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
            context));
    }

    private static (bool IsHere, IItem? item) IsItemHere(IContext context, string item)
    {
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

        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
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

        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
        return result;
    }

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        IContext context)
    {
        var request =
            new CommandHasNoEffectOperationRequest(context.CurrentLocation.GetDescriptionForGeneration(context), input);
        var result = await generationClient.GenerateNarration(request) + Environment.NewLine;
        return result;
    }

    private DisambiguationInteractionResult? CheckDisambiguation(MultiNounIntent intent,
        IContext context, Func<string[], bool> matchFunction)
    {
        var ambiguousItems = new List<IItem>();

        List<IItem>? allItemsInLocation = (context.CurrentLocation as ICanHoldItems)?.GetAllItemsRecursively;
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