namespace Model.AIGeneration.Requests;

public abstract class MultiNounRequest : Request
{
    public string? Location { get; init; }
    public string? NounOne { get; init; }
    public string? NounTwo { get; init; }
    public string? Preposition { get; init; }
    public string? Verb { get; init; }

    public bool NounOneIsAPerson { get; init; } = false;
    
    public bool NounTwoIsAPerson { get; init; } = false;
    
    public string? PersonOneDescription { get; init; }
    
    public string? PersonTwoDescription { get; init; }
}