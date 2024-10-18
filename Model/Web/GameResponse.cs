using System.Diagnostics.CodeAnalysis;

namespace Model.Web;

public record GameResponse
{
    [SetsRequiredMembers]
    public GameResponse(string response, string locationName, int moves, int score)
    {
        Response = response;
        LocationName = locationName;
        Moves = moves;
        Score = score;
    }

    public required string Response { get; init; }

    public required string LocationName { get; init; }

    public required int Moves { get; init; }

    public required int Score { get; init; }
}

public record SavedGame
{
    [SetsRequiredMembers]
    public SavedGame(string id, string name, DateTime date)
    {
        Id = id;
        Name = name;
        Date = date;
    }
    
    public required string Id { get; init; }
    
    public required string Name { get; init; }
    
    public required DateTime Date { get; init; }
}