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
}