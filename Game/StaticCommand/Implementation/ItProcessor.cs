using System.Text.RegularExpressions;
using Model.AIGeneration;

namespace Game.StaticCommand.Implementation;

internal record ItProcessorResult
{
    internal bool RequiresClarification { get; init; }

    internal string Output { get; init; } = "";
}

internal class ItProcessor : IStatefulProcessor
{
    private string? _lastInput;

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }

    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        if (string.IsNullOrEmpty(input))
        {
            Completed = true;
            return Task.FromResult(string.Empty);
        }

        if (string.IsNullOrEmpty(_lastInput))
        {
            Completed = true;
            return Task.FromResult(string.Empty);
        }

        Completed = true;
        ContinueProcessing = true;

        return Task.FromResult(Regex.Replace(_lastInput, @"\bit\b", input));
    }

    public ItProcessorResult Check(string input, IContext context)
    {
        if (!input.Contains(" it ", StringComparison.InvariantCultureIgnoreCase) &&
            !input.ToLowerInvariant().EndsWith(" it"))
        {
            Completed = true;
            return new ItProcessorResult { Output = input, RequiresClarification = false };
        }

        var lastNoun = context.LastNoun;

        if (string.IsNullOrEmpty(lastNoun))
        {
            _lastInput = input;
            Completed = false;
            ContinueProcessing = false;
            return new ItProcessorResult { RequiresClarification = true, Output = "What item are you referring to?\n" };
        }

        input = Regex.Replace(input, @"\bit\b", lastNoun);
        Completed = true;

        return new ItProcessorResult { Output = input, RequiresClarification = false };
    }
}