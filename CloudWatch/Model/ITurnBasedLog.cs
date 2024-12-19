namespace CloudWatch.Model;

public interface ITurnBasedLog
{ 
    string? TurnCorrelationId { get; set; }
}