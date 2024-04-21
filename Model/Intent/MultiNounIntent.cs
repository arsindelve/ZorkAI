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
    
    public bool MatchVerb(string[] verbs)
    {
        return verbs.Any(s => s.Equals(Verb, StringComparison.InvariantCultureIgnoreCase));
    }
    
    public bool MatchPreposition(string[] prepositions)
    {
        return prepositions.Any(s => s.Equals(Preposition, StringComparison.InvariantCultureIgnoreCase));
    }
    
    public bool Match(string[] verbs, string[] nounsOneForMatching, string[] nounsTwoForMatching, string[] prepositionsForMatching)
    {
        return MatchNounOne(nounsOneForMatching) && MatchNounTwo(nounsTwoForMatching) && MatchVerb(verbs) &&
               MatchPreposition(prepositionsForMatching);
    }
}