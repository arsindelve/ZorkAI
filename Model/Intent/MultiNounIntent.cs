using Model.Item;

namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that this is a multi-noun intent such as "put leaflet in mailbox"
///     or "unlock the door with the key"
/// </summary>
public record MultiNounIntent : IntentBase
{
    public required string Verb { get; init; }

    public required string NounOne { get; init; }

    public required string NounTwo { get; init; }

    public required string Preposition { get; set; }

    public required string OriginalInput { get; set; }

    public IItem? ItemOne { get; set; }

    public IItem? ItemTwo { get; set; }

    public override string ToString()
    {
        return $"Verb: {Verb}, NounOne: {NounOne}, NounTwo: {NounTwo}, Preposition: {Preposition}";
    }

    public bool MatchNounOne(string[] nouns)
    {
        return nouns.Any(s => s.Equals(NounOne, StringComparison.InvariantCultureIgnoreCase));
    }

    public bool MatchNounTwo(string[] nouns)
    {
        return nouns.Any(s => s.Equals(NounTwo, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    ///     Verbs are matched only against a declared <see cref="VerbFamily" />; see
    ///     <see cref="SimpleIntent.MatchVerb" />.
    /// </summary>
    public bool MatchVerb(VerbFamily verbs)
    {
        return verbs.Matches(Verb);
    }

    public bool MatchPreposition(string[] prepositions)
    {
        return prepositions.Any(s => s.Equals(Preposition, StringComparison.InvariantCultureIgnoreCase));
    }

    public bool Match(VerbFamily verbs, string[] nounsOneForMatching, string[] nounsTwoForMatching,
        string[] prepositionsForMatching)
    {
        return MatchNounOne(nounsOneForMatching) && MatchNounTwo(nounsTwoForMatching) && MatchVerb(verbs) &&
               MatchPreposition(prepositionsForMatching);
    }
}