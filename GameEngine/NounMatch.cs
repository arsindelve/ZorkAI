using Model.Item;

namespace GameEngine;

/// <summary>
/// The comparison half of noun resolution, sitting beside <see cref="Repository"/>, which owns the
/// lookup half (<see cref="Repository.GetItemInScope"/>, <see cref="Repository.GetPreciseMatchInScope"/>).
/// Where those answer "what item is this noun?", these answer "could this noun be naming this item?"
/// and "does this command name one object at all?" - pure predicates over a noun and an item's
/// vocabulary, with no verb, context or repository involved.
///
/// <para>Deliberately static rather than an instance method on <c>ItemBase</c>. The whole reason
/// <see cref="CouldName"/> exists is that <c>ContainerBase</c> and <c>OpenAndCloseContainerBase</c>
/// override <see cref="IItem.HasMatchingNoun"/> into exact equality; an instance method here would
/// invite exactly the same override and reintroduce the trap. A static that reads
/// <see cref="IItem.NounsForMatching"/> from the outside cannot be shadowed.</para>
/// </summary>
public static class NounMatch
{
    /// <summary>
    /// Collective words that stand in for "whatever qualifies" rather than naming an object. The
    /// IntentParser hands these through as the noun, so they can never match a candidate.
    /// </summary>
    private static readonly string[] CollectiveNouns =
        ["all", "everything", "anything", "something", "both", "them", "these", "those", "stuff", "things"];

    /// <summary>
    /// Markers that the command covers more than one object. Padded on both sides at the comparison
    /// site, so "take the ball" does not trip on " all " and "press the button" does not trip on
    /// " but ".
    /// </summary>
    private static readonly string[] MultipleObjectMarkers =
        [" and ", " & ", " , ", " except ", " but ", " besides ", " all ", " everything ", " anything ", " both "];

    /// <summary>
    /// Could the player's noun be naming this item? Compares against the item's own nouns only - a
    /// bag that happens to hold the named object is not itself the named object.
    ///
    /// <para>Deliberately does NOT call <see cref="IItem.HasMatchingNoun"/>: only <c>ItemBase</c> has
    /// the word-boundary containment fallback that lets "brass lantern" match a bare "lantern".
    /// <c>ContainerBase</c> and <c>OpenAndCloseContainerBase</c> override it with plain exact
    /// equality, so routing through it would make the check exact-match-only for every
    /// container-derived item (sack, bottle, canteen, coffin, survival kit...) and refuse any
    /// adjective the player added but the parser didn't echo - "take the elongated sack",
    /// "drop the full canteen". Containment runs in both directions so it doesn't matter which side
    /// carries the extra adjective.</para>
    /// </summary>
    public static bool CouldName(IItem candidate, string? noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return false;

        var typed = noun.ToLowerInvariant().Trim();

        var candidateNouns = candidate.NounsForMatching
            .Concat(candidate.NounsForPreciseMatching)
            .Select(n => n.ToLowerInvariant().Trim())
            .Where(n => n.Length > 0)
            .Distinct()
            .ToList();

        if (candidateNouns.Contains(typed))
            return true;

        // Padding both sides with spaces avoids false positives like matching "key" inside "monkey".
        var paddedTyped = $" {typed} ";
        return candidateNouns.Any(n => paddedTyped.Contains($" {n} ") || $" {n} ".Contains(paddedTyped));
    }

    /// <summary>
    /// True only when the command names exactly one object. Callers that verify an AI list-parser's
    /// candidate against the noun the IntentParser extracted must gate on this first (issue #502).
    ///
    /// <para>The trap: <c>TakeIntent.Noun</c>/<c>DropIntent.Noun</c> is just
    /// <c>nouns.FirstOrDefault()</c> (<c>ParsingHelper</c>), so on a quantified command it is a
    /// collective word ("everything") or even the *excluded* noun - "pick up everything except the
    /// tube" yields <c>Noun = "tube"</c>. Checking a parser candidate against that noun would not
    /// just refuse a legitimate take; it would resolve to the tube and act on the very object the
    /// player excluded. Likewise on "take X and Y" where only Y is here, the parser returns Y while
    /// <c>Noun</c> is X: a single candidate is a legitimate partial resolution of a multi-object
    /// request, not a substitution. These phrasings are supported - see
    /// <c>IntegrationTests/OpenAITakeAndDropParserTests.cs</c>.</para>
    /// </summary>
    /// <param name="noun">The single noun the IntentParser extracted from the command.</param>
    /// <param name="originalInput">The player's raw input, which is where a multi-object phrasing shows.</param>
    public static bool NamesASingleObject(string? noun, string? originalInput)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return false;

        if (CollectiveNouns.Contains(noun.ToLowerInvariant().Trim()))
            return false;

        var input = $" {(originalInput ?? string.Empty).ToLowerInvariant().Replace(",", " , ")} ";
        return !MultipleObjectMarkers.Any(input.Contains);
    }
}
