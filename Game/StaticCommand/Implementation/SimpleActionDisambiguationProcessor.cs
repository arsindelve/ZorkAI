using Model.AIGeneration;
using Model.Interface;

namespace Game.StaticCommand.Implementation;

internal class SimpleActionDisambiguationProcessor(SimpleInteractionDisambiguationInteractionResult disambiguator)
    : IStatefulProcessor
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (string.IsNullOrEmpty(input))
            return Task.FromResult("");

        foreach (var possibleResponse in disambiguator.PossibleResponses)
            if (input.ToLowerInvariant().Trim().Contains(possibleResponse))
                return Task.FromResult($"{disambiguator.Verb} {possibleResponse}");

        return Task.FromResult(input);
    }

    public bool Completed => true;

    public bool ContinueProcessing => true;
}