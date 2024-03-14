namespace OpenAI.Requests;

public abstract class MultiNounRequest : Request
{
    public string? Location { get; init; }
    public string? NounOne { get; init; }
    public string? NounTwo { get; init; }
    public string? Preposition { get; init; }
    public string? Verb { get; init; }
}