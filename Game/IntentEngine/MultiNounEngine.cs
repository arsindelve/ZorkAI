using System.Diagnostics;

namespace Game.IntentEngine;

public class MultiNounEngine : IIntentEngine
{
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

        var nounOneExistsHere = context.HasMatchingNoun(interaction.NounOne) ||
                                context.CurrentLocation.HasMatchingNoun(interaction.NounOne);

        var nounTwoExistsHere = context.HasMatchingNoun(interaction.NounTwo) ||
                                context.CurrentLocation.HasMatchingNoun(interaction.NounTwo);

        if (!nounOneExistsHere & nounTwoExistsHere)
            return await GetGeneratedNounOneNotFoundResponse(interaction, generationClient,
                context);

        if (nounOneExistsHere & !nounTwoExistsHere)
            return await GetGeneratedNounTwoNotFoundResponse(interaction, generationClient,
                context);

        // TODO: Neither noun here. 
        // TODO: Run the multi-interaction processing here. 

        // If not positive interaction.....
        return await GetGeneratedVerbNotUsefulResponse(interaction, generationClient,
            context);
    }

    private async Task<string> GetGeneratedVerbNotUsefulResponse(MultiNounIntent interaction,
        IGenerationClient generationClient, IContext context)
    {
        var request =
            new VerbHasNoEffectMultiNounOperationRequest(context.CurrentLocation.DescriptionForGeneration,
                interaction.NounOne, interaction.NounTwo, interaction.Preposition, interaction.Verb);
        var result = await generationClient.CompleteChat(request) + Environment.NewLine;
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