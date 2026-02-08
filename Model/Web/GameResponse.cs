using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Model.Interface;
using Model.Movement;

// ReSharper disable TypeWithSuspiciousEqualityIsUsedInRecord.Global

namespace Model.Web;

[method: SetsRequiredMembers]
public record GameResponse(
    string Response,
    string LocationName,
    int Moves,
    int Score,
    int Time,
    string? PreviousLocationName,
    string? LastMovementDirection,
    List<string> Inventory,
    List<Direction> Exits,
    List<string> ActionsAvailaibleFromLocation,
    List<string> ActionsAvailaibleFromInventory)
{
    [SetsRequiredMembers]
    public GameResponse(string response, IGameEngine gameEngine) : this(response, gameEngine.LocationName,
        gameEngine.Moves,
        gameEngine.Score,
        gameEngine.CurrentTime,
        gameEngine.PreviousLocationName,
        gameEngine.LastMovementDirection.ToString(),
        gameEngine.Inventory,
        gameEngine.Exits,
        gameEngine.Context?.CurrentLocation.GetAvailableActionsInLocation() ?? new List<string>(),
        gameEngine.Context?.GetAvailableActionsForInventory() ?? new List<string>())
    {
    }

    [UsedImplicitly] public required string Response { get; init; } = Response;

    [UsedImplicitly] public required string LocationName { get; init; } = LocationName;

    [UsedImplicitly] public required int Moves { get; init; } = Moves;

    [UsedImplicitly] public required int Score { get; init; } = Score;

    [UsedImplicitly] public required List<string> Inventory { get; init; } = Inventory;

    [UsedImplicitly]
    public required List<string> ActionsAvailaibleFromInventory { get; init; } = ActionsAvailaibleFromInventory;

    [UsedImplicitly]
    public required List<string> ActionsAvailaibleFromLocation { get; init; } = ActionsAvailaibleFromLocation;

    [UsedImplicitly] public required int Time { get; init; } = Time;

    [UsedImplicitly] public required string? PreviousLocationName { get; init; } = PreviousLocationName;

    [UsedImplicitly] public required string? LastMovementDirection { get; init; } = LastMovementDirection;

    [UsedImplicitly] public required List<Direction> Exits { get; init; } = Exits;
}