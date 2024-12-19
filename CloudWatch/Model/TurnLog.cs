namespace CloudWatch.Model;

public record TurnLog : ITurnBasedLog
{
    public required string SessionId { get; init; }
    public required string Location { get; init; }
    public required int Score { get; init; }
    public required int Moves { get; init; }
    public required string Input { get; init; }
    public required string Response { get; init; }
    public string? TurnCorrelationId { get; set; }
}