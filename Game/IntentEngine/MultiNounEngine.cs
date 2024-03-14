using System.Diagnostics;
using Game.Item.MultiItemProcessor;
using Model.Item;

namespace Game.IntentEngine;

public class MultiNounEngine : IIntentEngine
{
    private readonly List<IMultiNounVerbProcessor> _processors = [new PutProcessor()];
    
    public async Task<string> Process(IntentBase intent, IContext context, IGenerationClient generationClient)
    {
        if (intent is not MultiNounIntent interaction)
            throw new ArgumentException();

        Debug.WriteLine(interaction);

        // After a multi-noun interaction, we will lose the ability to understand "it"
        context.LastNoun = "";

        // There is no matching noun one or noun two at all, anywhere in the game. The user might have
        // talked about a unicorn, a bottle of tequila or some other meaningless item. 
        if (!Repository.ItemExistsInTheStory(interaction.NounOne) &&
            !Repository.ItemExistsInTheStory(interaction.NounTwo))
            return await GetGeneratedNoOpResponse(interaction.OriginalInput, generationClient, context);

        var nounOneExistsHere = IsItemHere(context, interaction.NounOne);
        var nounTwoExistsHere = IsItemHere(context, interaction.NounTwo);

        if (!nounOneExistsHere & nounTwoExistsHere)
            return await GetGeneratedNounOneNotFoundResponse(interaction, generationClient,
                context);

        if (nounOneExistsHere & !nounTwoExistsHere)
            return await GetGeneratedNounTwoNotFoundResponse(interaction, generationClient,
                context);
        
        if (!nounOneExistsHere & !nounTwoExistsHere)
            return await GetGeneratedNounOneAndNounTwoNotFoundResponse(interaction, generationClient,
                context);

        IItem? itemOne = Repository.GetItem(interaction.NounOne);
        IItem? itemTwo = Repository.GetItem(interaction.NounTwo);

        // This should never happen, since we checked above that it exists. 
        if (itemOne is null)
            return await GetGeneratedNounOneNotFoundResponse(interaction, generationClient,
                context);
        
        if (itemTwo is null)
            return await GetGeneratedNounTwoNotFoundResponse(interaction, generationClient,
                context);
        
        // Let all the processors decide if they can handle this interaction. 
        foreach (IMultiNounVerbProcessor processor in _processors)
        {
            InteractionResult? result = processor.Process(interaction, context, itemOne,
                itemTwo);

            if (result is { InteractionHappened: true })
                return result.InteractionMessage;
        }
        
        // If not positive interaction.....
        return await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
            context);
    }

    private static bool IsItemHere(IContext context, string item)
    {
        return context.HasMatchingNoun(item) ||
               context.CurrentLocation.HasMatchingNoun(item);
    }

    private async Task<string> GetGeneratedNounOneAndNounTwoNotFoundResponse(MultiNounIntent interaction, IGenerationClient generationClient, IContext context)
    {
        var request =
            new MultiNounOperationBothMissingRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        return result;
    }

    // TODO: Refactor these three methods into a single "generic" method. 
    
    private async Task<string> GetGeneratedVerbNotUsefulResponse(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context)
    {
        var request =
            new VerbHasNoEffectMultiNounOperationRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        return result;
    }

    private async Task<string> GetGeneratedResponse<T>(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context) where T: Request, new()
    {
        var request =
            new MissingSecondNounMultiNounOperationRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        ;
        return result;
    }
    
    private async Task<string> GetGeneratedNounTwoNotFoundResponse(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context)
    {
        var request =
            new MissingSecondNounMultiNounOperationRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
        ;
        return result;
    }

    private async Task<string> GetGeneratedNounOneNotFoundResponse(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context)
    {
        var request =
            new MissingFirstNounMultiNounOperationRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounTwo, interaction.NounOne, interaction.Preposition, interaction.Verb);
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