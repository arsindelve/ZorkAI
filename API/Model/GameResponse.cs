namespace API.Model;

public record GameResponse
{
    public required string Response { get; init; }
    
    public required string LocationName { get; init; }
    
    public required int Moves { get; init; }
    
    public required int Score { get; init; }
}