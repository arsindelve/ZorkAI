namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that this is a simple VERB/NOUN intent such as "take leaflet"
///     or "open the mailbox"
/// </summary>
public record SimpleIntent : IntentBase
{
    public string? OriginalInput { get; set; }

    public required string Verb { get; init; }

    public required string? Noun { get; init; }

    public string? Adverb { get; init; }

    public string? Adjective { get; set; }

    public bool MatchNoun(string[] nouns)
    {
        return nouns.Any(s => s.Equals(Noun, StringComparison.InvariantCultureIgnoreCase));
    }

    public bool MatchNounAndAdjective(string[] nouns)
    {
        if (string.IsNullOrEmpty(Adjective))
            return MatchNoun(nouns);

        return nouns.Any(s => s.Equals($"{Adjective} {Noun}", StringComparison.InvariantCultureIgnoreCase));
    }

    public bool MatchVerb(string[] verbs)
    {
        return verbs.Any(s => s.Equals(Verb, StringComparison.InvariantCultureIgnoreCase));
    }

    public override string ToString()
    {
        return $"Verb: {Verb}, Noun: {Noun}";
    }

    public bool Match(string[] verbs, string[] nounsForMatching)
    {
        return MatchNoun(nounsForMatching) && MatchVerb(verbs);
    }

    /// <summary>
    ///     A parse-resilient companion to <see cref="Match" /> for prepositional commands such as
    ///     "look through window" / "peer through crack" / "look under table". The AI parser does not
    ///     deterministically extract verb ∈ <paramref name="verbs" /> + the head <see cref="Noun" /> from
    ///     these phrasings — the preposition intermittently gets absorbed into the noun or shifts the verb,
    ///     so a handler that gates on <see cref="Match" /> fires only when the parse happens to land right
    ///     (measured ~3/8 on prod — issues #423, #447). This instead matches against the RAW
    ///     <see cref="OriginalInput" />: it fires when the input contains one of <paramref name="verbs" />,
    ///     the <paramref name="preposition" />, and one of <paramref name="nouns" /> — independent of how the
    ///     sentence parsed. Prefer it over <see cref="Match" /> whenever a location must recognise a
    ///     "verb PREPOSITION noun" command reliably. (Mirrors the raw-input pattern already used in
    ///     ZorkOne's BehindHouse "through window" handling.)
    /// </summary>
    public bool MatchInInput(string[] verbs, string preposition, string[] nouns)
    {
        if (string.IsNullOrWhiteSpace(OriginalInput))
            return false;

        var lowered = OriginalInput.ToLowerInvariant();
        var words = lowered.Split([' ', ',', '.', '!', '?', ';', ':'], StringSplitOptions.RemoveEmptyEntries);
        var wordSet = new HashSet<string>(words);

        // Single-word tokens match as whole words (so "look" doesn't hit "overlook"); multi-word tokens
        // like "look at" match as a substring of the raw input.
        bool Present(string token)
        {
            var t = token.ToLowerInvariant();
            return t.Contains(' ') ? lowered.Contains(t) : wordSet.Contains(t);
        }

        return wordSet.Contains(preposition.ToLowerInvariant())
               && verbs.Any(Present)
               && nouns.Any(Present);
    }
}