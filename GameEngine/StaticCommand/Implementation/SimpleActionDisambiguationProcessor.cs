using System.Diagnostics;
using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class SimpleActionDisambiguationProcessor(SimpleInteractionDisambiguationInteractionResult disambiguator)
    : IStatefulProcessor
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (string.IsNullOrEmpty(input))
            return Task.FromResult("");

        // The order by here makes sure we get the most precise (longest) matching description 
        foreach (string possibleResponse in disambiguator.PossibleResponses.Keys)
            if (input.ToLowerInvariant().Trim().Contains(possibleResponse))
            {
                var result = $"{disambiguator.Verb} {disambiguator.PossibleResponses[possibleResponse]}";
                Debug.WriteLine($"SimpleActionDisambiguationProcessor: {result}");
                return Task.FromResult(result);
            }

        Debug.WriteLine($"SimpleActionDisambiguationProcessor: {input}");
        return Task.FromResult(input);
    }

    public bool Completed => true;

    public bool ContinueProcessing => true;
}