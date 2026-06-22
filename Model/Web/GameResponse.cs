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
    Dictionary<string, List<string>> ActionsAvailableFromLocation,
    Dictionary<string, List<string>> ActionsAvailableFromInventory)
{
    [SetsRequiredMembers]
    public GameResponse(string response, IGameEngine gameEngine) : this(response, gameEngine.LocationName,
        gameEngine.Moves,
        gameEngine.Score,
        gameEngine.CurrentTime,
        gameEngine.PreviousLocationName,
        gameEngine.LastMovementDirection.ToString(),
        gameEngine.Inventory,
        // Issue #238: in the dark the prose hides the room, so the structured payload must not leak
        // the location's exits or action chips either. Inventory-derived fields stay populated —
        // the player can still feel what they're carrying. (Context is always initialized in
        // GameEngine and never null in production; the null fall-throughs here exist only because
        // IGameEngine.Context is annotated nullable. Both darkness checks use the same idiom so a
        // future third location-derived field can copy the pattern without re-introducing skew.)
        gameEngine.Context is { ItIsDarkHere: true } ? new List<Direction>() : gameEngine.Exits,
        gameEngine.Context is { ItIsDarkHere: false } litContext
            ? litContext.CurrentLocation.GetAvailableActionsInLocation()
            : new Dictionary<string, List<string>>(),
        gameEngine.Context?.GetAvailableActionsForInventory() ?? new Dictionary<string, List<string>>())
    {
    }

    [UsedImplicitly] public required string Response { get; init; } = Response;

    [UsedImplicitly] public required string LocationName { get; init; } = LocationName;

    [UsedImplicitly] public required int Moves { get; init; } = Moves;

    [UsedImplicitly] public required int Score { get; init; } = Score;

    [UsedImplicitly] public required List<string> Inventory { get; init; } = Inventory;

    [UsedImplicitly]
    public required Dictionary<string, List<string>> ActionsAvailableFromInventory { get; init; } = ActionsAvailableFromInventory;

    [UsedImplicitly]
    public required Dictionary<string, List<string>> ActionsAvailableFromLocation { get; init; } = ActionsAvailableFromLocation;

    [UsedImplicitly] public required int Time { get; init; } = Time;

    [UsedImplicitly] public required string? PreviousLocationName { get; init; } = PreviousLocationName;

    [UsedImplicitly] public required string? LastMovementDirection { get; init; } = LastMovementDirection;

    [UsedImplicitly] public required List<Direction> Exits { get; init; } = Exits;
}