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

        var reply = input.ToLowerInvariant().Trim();

        // Replies are matched by SUBSTRING, so more than one key can hit at once: "the tan one" contains
        // both "tan" and "one", which are different buttons in Booth 2, and "round white button" contains
        // both "round" and "white" in the Machine Shop. Prefer the longest match, since that is the most
        // specific, and break ties on where the player mentioned it — their first mention is what they
        // meant. Both rules are properties of the reply, so the winner no longer depends on the order the
        // caller happened to list its choices in. That used to be the silent tie-breaker even though a
        // Dictionary promises no enumeration order, which meant re-sorting a caller's choices could
        // quietly change which button a player got.
        var match = disambiguator.PossibleResponses.Keys
            .Select(key => (key, index: reply.IndexOf(key, StringComparison.Ordinal)))
            .Where(candidate => candidate.index >= 0)
            .OrderByDescending(candidate => candidate.key.Length)
            .ThenBy(candidate => candidate.index)
            .Select(candidate => candidate.key)
            .FirstOrDefault();

        if (match is null)
        {
            Debug.WriteLine($"ComplexActionDisambiguationProcessor: {input}");
            return Task.FromResult(input);
        }

        var result = string.Format(disambiguator.ReplacementString, disambiguator.PossibleResponses[match]);
        Debug.WriteLine($"ComplexActionDisambiguationProcessor: {result}");
        return Task.FromResult(result);
    }

    public bool Completed => true;

    public bool ContinueProcessing => true;
}