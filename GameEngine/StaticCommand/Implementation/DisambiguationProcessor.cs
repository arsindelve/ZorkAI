using System.Diagnostics;
using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class DisambiguationProcessor(DisambiguationInteractionResult disambiguator)
    : IStatefulProcessor
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (string.IsNullOrEmpty(input))
            return Task.FromResult("");

        // Order by length descending to check longer (more precise) matches first
        foreach (var possibleResponse in disambiguator.PossibleResponses.Keys.OrderByDescending(k => k.Length))
            if (input.ToLowerInvariant().Trim().Contains(possibleResponse))
            {
                var result = string.Format(disambiguator.ReplacementString,
                    disambiguator.PossibleResponses[possibleResponse]);
                Debug.WriteLine($"ComplexActionDisambiguationProcessor: {result}");
                return Task.FromResult(result);
            }

        Debug.WriteLine($"ComplexActionDisambiguationProcessor: {input}");
        return Task.FromResult(input);
    }

    public bool Completed => true;

    public bool ContinueProcessing => true;
}