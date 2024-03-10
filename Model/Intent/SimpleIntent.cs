namespace Model.Intent;

/// <summary>
/// The parser has reasonable confidence that this is a simple VERB/NOUN intent such as "take leaflet"
/// or "open the mailbox" 
/// </summary>
public record SimpleIntent : IntentBase
{
    public required string OriginalInput { get; set; }
    
    public required string Verb { get; init; }
    
    public required string? Noun { get; init; }

    public bool MatchNoun(string[] noun)
    {
        return noun.Any(s => s.Equals(Noun, StringComparison.InvariantCultureIgnoreCase));
    }

    public override string ToString()
    {
        return $"Verb: {Verb}, Noun: {Noun}";
    }
}