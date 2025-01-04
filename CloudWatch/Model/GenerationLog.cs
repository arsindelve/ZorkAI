namespace CloudWatch.Model;

public record GenerationLog : ITurnBasedLog
{
    public required string LanguageModel { get; set; }

    public required string Prompt { get; set; }

    public required string Response { get; set; }
    public string? TurnCorrelationId { get; set; }
}