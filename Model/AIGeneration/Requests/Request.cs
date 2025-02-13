namespace Model.AIGeneration.Requests;

public abstract class Request
{
    public virtual string? UserMessage { get; protected init; }

    public float Temperature { get; set; } = 0.8f;
}