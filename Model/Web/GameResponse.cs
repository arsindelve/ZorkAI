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
    Dictionary<string, List<string>> ActionsAvailableFromInventory,
    // Both defaulted so the positional constructor stays source-compatible; the engine-based
    // constructor below always passes them explicitly.
    bool ItIsDarkHere = false,
    string? LocationKey = null)
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
        // the location's exits or action chips either — both location-derived fields populate only
        // when the room is lit (same `is { ItIsDarkHere: false }` polarity on each). Inventory-derived
        // fields stay populated: the player can still feel what they're carrying.
        gameEngine.Context is { ItIsDarkHere: false } ? gameEngine.Exits : new List<Direction>(),
        gameEngine.Context is { ItIsDarkHere: false } litContext
            ? litContext.CurrentLocation.GetAvailableActionsInLocation()
            : new Dictionary<string, List<string>>(),
        gameEngine.Context?.GetAvailableActionsForInventory() ?? new Dictionary<string, List<string>>(),
        // Reported rather than merely acted on, because the client has its own location-derived
        // things to withhold in the dark — the room artwork above all. A picture of a room the
        // player cannot see is the same leak issue #238 closed for exits and action chips.
        //
        // Negated rather than written as `is { ItIsDarkHere: true }` so a null Context lands on
        // the same side as the two fields above: those use `is { ItIsDarkHere: false }`, which a
        // null fails, and so withhold. The positive form would have a null report the room as
        // *lit* while its exits and chips had already been stripped as if it were dark.
        gameEngine.Context is not { ItIsDarkHere: false },
        // A stable identity for the room, because the display name is not one: Zork I has two
        // rooms called "Clearing", two called "Cave" and four called "Forest". Only one of the
        // Clearings has the grating hidden under the leaves, and a client picking artwork by
        // name alone cannot tell them apart.
        gameEngine.Context?.CurrentLocation?.GetType().Name)
    {
    }

    [UsedImplicitly] public required string Response { get; init; } = Response;

    [UsedImplicitly] public required string LocationName { get; init; } = LocationName;

    [UsedImplicitly] public required string? LocationKey { get; init; } = LocationKey;

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

    [UsedImplicitly] public required bool ItIsDarkHere { get; init; } = ItIsDarkHere;
}