using System.Text.RegularExpressions;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

internal class ItProcessor : IStatefulProcessor
{
    private string? _lastInput;

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }

    public Pronoun PronounUsed { get; set; }

    // This runs if, on the last pass, we could not determine what they meant by "it" or "them",
    // and so we asked for clarification and now they have provided the noun.
    public Task<string> Process(
        string? input,
        IContext context,
        IGenerationClient client,
        Runtime runtime
    )
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

        string? pattern = PronounUsed switch
        {
            Pronoun.It => @"\bit\b",
            Pronoun.Them => @"\bthem\b",
            _ => null,
        };

        return pattern is not null
            ? Task.FromResult(Regex.Replace(_lastInput, pattern, input, RegexOptions.IgnoreCase))
            : Task.FromResult(input);
    }

    // This runs if they have used "it" or "them" and we can be certain what object they're
    // referring to, and so we go ahead and make the replacement and continue processing.
    public (bool, string) Check(string input, IContext context)
    {
        var lastNoun = context.LastNoun;
        IItem? item = Repository.GetItem(lastNoun);

        bool usedItOrThem = DidWeUseItOrThem(input);

        if (!usedItOrThem)
        {
            Completed = true;
            return (false, input);
        }

        if (
            string.IsNullOrEmpty(lastNoun)
            || PronounUsed == Pronoun.Them && item is not IPluralNoun
        )
        {
            _lastInput = input;
            Completed = false;
            ContinueProcessing = false;
            // This will trigger the "Process" method above to be called on their next input.
            return (true, "What item are you referring to?\n");
        }

        input = PronounUsed switch
        {
            Pronoun.It => Regex.Replace(input, @"\bit\b", lastNoun, RegexOptions.IgnoreCase),
            Pronoun.Them => Regex.Replace(input, @"\bthem\b", lastNoun, RegexOptions.IgnoreCase),
            _ => input,
        };

        // Reset.
        Completed = true;
        PronounUsed = Pronoun.Unknown;

        return (false, input);
    }

    private bool DidWeUseItOrThem(string input)
    {
        if (Regex.IsMatch(input, @"\bit\b|\bit$", RegexOptions.IgnoreCase))
        {
            PronounUsed = Pronoun.It;
            return true;
        }
        if (Regex.IsMatch(input, @"\bthem\b|\bthem$", RegexOptions.IgnoreCase))
        {
            PronounUsed = Pronoun.Them;
            return true;
        }

        return false;
    }
}

public enum Pronoun
{
    Unknown,
    It,
    Them,
}
