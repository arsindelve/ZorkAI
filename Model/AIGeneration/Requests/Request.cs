namespace Model.AIGeneration.Requests;

public abstract class Request
{
    public virtual string? UserMessage { get; protected init; }
}