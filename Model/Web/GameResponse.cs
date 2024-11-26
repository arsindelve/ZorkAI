using System.Diagnostics.CodeAnalysis;
using Model.Interface;
using Model.Movement;

namespace Model.Web;

[method: SetsRequiredMembers]
public record GameResponse(
    string Response,
    string LocationName,
    int Moves,
    int Score,
    string? PreviousLocationName,
    string? LastMovementDirection)
{
    [SetsRequiredMembers]
    public GameResponse(string response, IGameEngine gameEngine) : this(response, gameEngine.LocationName,
        gameEngine.Moves, gameEngine.Score, gameEngine.PreviousLocationName, gameEngine.LastMovementDirection.ToString())
    {
    }

    public required string Response { get; init; } = Response;

    public required string LocationName { get; init; } = LocationName;

    public required int Moves { get; init; } = Moves;

    public required int Score { get; init; } = Score;
}