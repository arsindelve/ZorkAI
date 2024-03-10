namespace Model.Intent;

/// <summary>
///     The parser has reasonable confidence that this is a multi-noun intent such as "put leaflet in mailbox"
///     or "unlock the door with the key"
/// </summary>
public record MultiNounIntent : IntentBase
{
    public required string Verb { get; init; }

    public required string? NounOne { get; init; }

    public required string? NounTwo { get; init; }
    
    public string? Preposition { get; set; }
    
    public required string OriginalInput { get; set; }

    public bool MatchNoun(string[] nounOne, string[] nounTwo)
    {
        return nounOne.Any(s => s.Equals(NounOne, StringComparison.InvariantCultureIgnoreCase))
               && nounTwo.Any(s => s.Equals(NounTwo, StringComparison.InvariantCultureIgnoreCase));
    }
    
    public override string ToString()
    {
        return $"Verb: {Verb}, NounOne: {NounOne}, NounTwo: {NounTwo}, Preposition: {Preposition}";
    }   
}