using System.Diagnostics.CodeAnalysis;

namespace Lambda.Model;

public record GameResponse
{
    public GameResponse()
    {
    }

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