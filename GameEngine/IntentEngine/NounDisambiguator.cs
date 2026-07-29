using Model.Interface;
using Model.Item;
using Utilities;

namespace GameEngine.IntentEngine;

/// <summary>
///     "Do you mean the lower elevator door or the upper elevator door?" — the one place that decides a
///     noun in scope is ambiguous, and builds the prompt for it.
///     <para>
///         This used to be private and duplicated in <see cref="SimpleInteractionEngine" /> and
///         <see cref="MultiNounEngine" />, which meant the engines that had no copy simply never asked:
///         <see cref="EnterSubLocationEngine" /> and <see cref="ExitSubLocationEngine" /> resolved
///         straight through <see cref="Repository.GetItemInScope" />, which returns the first match. So a
///         room with two doors answered "Do you mean...?" to <c>examine door</c> and silently picked one
///         for <c>enter door</c> — in the Planetfall Elevator Lobby, walking the player into whichever
///         elevator happened to be seeded first (issue #532).
///     </para>
/// </summary>
internal static class NounDisambiguator
{
    /// <summary>
    ///     Returns the prompt to ask when more than one item in scope answers to the noun, or null when
    ///     the noun is unambiguous and the caller should resolve it normally.
    /// </summary>
    /// <param name="context">The player, for inventory and the current room.</param>
    /// <param name="matchesNoun">
    ///     Whether an item's <see cref="IItem.NounsForMatching" /> answers to the noun the player typed.
    ///     Supplied by the caller because each intent carries its noun differently — a
    ///     <c>SimpleIntent</c> has an adjective to honour, a bare string does not.
    /// </param>
    /// <param name="replacement">
    ///     The command to re-run once the player picks, with <c>{0}</c> where the noun goes — e.g.
    ///     <c>"enter {0}"</c>.
    /// </param>
    public static DisambiguationInteractionResult? Check(Func<string[], bool> matchesNoun, IContext context,
        string replacement)
    {
        // Production always gives a real room and a real inventory here; an under-configured IContext
        // mock returns null for either, and nothing to look at is nothing to disambiguate against. Both
        // sides need the guard - Union throws on a null argument, not just a null receiver.
        var itemsInLocation = (context.CurrentLocation as ICanContainItems)?.GetAllItemsRecursively;
        if (itemsInLocation is null)
            return null;

        var ambiguousItems = (context.GetAllItemsRecursively ?? [])
            .Union(itemsInLocation)
            .Where(item => matchesNoun(item.NounsForMatching))
            .ToList();

        // One or fewer items answer to the noun. Good to go.
        if (ambiguousItems.Count <= 1)
            return null;

        var itemNouns = ambiguousItems
            .Select(s => s.NounsForMatching.MaxBy(n => n.Length))
            .ToList()!
            .SingleLineListWithOr();

        // Map every noun each item answers to onto that item's longest one, and replace what the player
        // said with it. Without this the reply would be just as ambiguous as the question and we would
        // loop around disambiguating forever.
        var nounToLongestNounMap = new Dictionary<string, string>();
        foreach (var item in ambiguousItems)
        {
            var longestNoun = item.NounsForPreciseMatching.MaxBy(noun => noun.Length);
            foreach (var noun in item.NounsForPreciseMatching)
                nounToLongestNounMap[noun] = longestNoun ?? string.Empty;
        }

        return new DisambiguationInteractionResult($"Do you mean {itemNouns}?", nounToLongestNounMap, replacement);
    }

    /// <summary>
    ///     The common case: a bare noun with no adjective, matched exactly against an item's nouns.
    /// </summary>
    public static DisambiguationInteractionResult? Check(string noun, IContext context, string replacement)
    {
        return Check(nouns => nouns.Any(n => n.Equals(noun, StringComparison.InvariantCultureIgnoreCase)), context,
            replacement);
    }
}
