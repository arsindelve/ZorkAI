namespace CloudWatch.Model;

public record GenerationLog : ITurnBasedLog
{
    public string? TurnCorrelationId { get; set; }
    
    public required string LanguageModel { get; set; }
    
    public required string Prompt { get; set; }
    
    public required string Response { get; set; }
    
    
    public required float Temperature { get; set; }
}