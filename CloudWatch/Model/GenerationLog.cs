namespace CloudWatch.Model;

public record GenerationLog : ITurnBasedLog
{
    public required string LanguageModel { get; set; }
    public required string UserPrompt { get; set; }
    public required string Response { get; set; }
    public required float Temperature { get; set; }
    public required string SystemPrompt { get; set; }
    public string? TurnCorrelationId { get; set; }
}