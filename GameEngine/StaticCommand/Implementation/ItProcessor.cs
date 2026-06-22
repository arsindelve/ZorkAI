using System.Text.RegularExpressions;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

internal class ItProcessor : IStatefulProcessor
{
    private string? _lastInput;

    public Pronoun PronounUsed { get; set; }

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }

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

        var pattern = PronounUsed switch
        {
            Pronoun.It => @"\bit\b",
            Pronoun.Them => @"\bthem\b",
            _ => null
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

        var usedItOrThem = DidWeUseItOrThem(input);

        if (!usedItOrThem)
        {
            Completed = true;
            return (false, input);
        }

        if (PronounUsed == Pronoun.Them)
        {
            // "them" refers to the collection of items the player last handled as a group (e.g.
            // "take all" or several individual takes). Expand it into a conjoined noun list so the
            // existing take/drop list handling does the rest — a single noun string can't represent
            // a set of distinct singular items, which is why this used to dead-end (issue #248).
            if (context.LastNouns.Count >= 2)
            {
                // Resolve only to members still relevant to *this* command — you can only drop what
                // you're holding — so a remembered item that has since left scope (already dropped,
                // eaten, left in another room) doesn't drag a spurious "you don't have it" into an
                // otherwise successful "drop them".
                var inScope = ItemsStillInScope(input, context);
                return inScope.Count > 0
                    ? Resolved(Regex.Replace(input, @"\bthem\b", string.Join(" and ", inScope), RegexOptions.IgnoreCase))
                    : AskForClarification(input);
            }

            // A lone item is only a valid "them" antecedent if it is intrinsically plural (candles,
            // matches, ...). Otherwise we still have to ask which item they mean.
            var soleNoun = context.LastNouns.Count == 1 ? context.LastNouns[0] : lastNoun;
            if (string.IsNullOrEmpty(soleNoun) || Repository.GetItem(soleNoun) is not IPluralNoun)
                return AskForClarification(input);

            return Resolved(Regex.Replace(input, @"\bthem\b", soleNoun, RegexOptions.IgnoreCase));
        }

        // Pronoun.It
        if (string.IsNullOrEmpty(lastNoun))
            return AskForClarification(input);

        return Resolved(Regex.Replace(input, @"\bit\b", lastNoun, RegexOptions.IgnoreCase));
    }

    // Verbs whose direct object must be in the player's hand. Used to scope "them" so "drop them"
    // never resolves to an item that is no longer being carried.
    private static readonly string[] DropVerbs = ["drop"];

    private static List<string> ItemsStillInScope(string input, IContext context)
    {
        var verb = input.TrimStart().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        var dropping = DropVerbs.Contains(verb, StringComparer.OrdinalIgnoreCase);

        // Dropping needs the item carried; for everything else (take/put/...) it only needs to be
        // reachable — carried, or present in the current room.
        return context.LastNouns
            .Where(noun => context.HasMatchingNoun(noun).HasItem ||
                           (!dropping && context.CurrentLocation.HasMatchingNoun(noun).HasItem))
            .ToList();
    }

    private (bool, string) Resolved(string input)
    {
        Completed = true;
        PronounUsed = Pronoun.Unknown;
        return (false, input);
    }

    private (bool, string) AskForClarification(string input)
    {
        _lastInput = input;
        Completed = false;
        ContinueProcessing = false;
        // This will trigger the "Process" method above to be called on their next input.
        return (true, "What item are you referring to?\n");
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
    Them
}