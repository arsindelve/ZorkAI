using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Location.Kalamontee;

internal class DormCorridor : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<CorridorJunction>() },
            { Direction.W, Go<MessCorridor>() },
            { Direction.N, Go<DormD>() },
            { Direction.S, Go<DormC>() }
        };

    protected override string ContextBasedDescription =>
        "This is a wide, east-west hallway with openings to the north and south. To the east, the corridor " +
        "stretches off into the distance. That section of the hallway is lined with a motorized walkway " +
        "(no longer running) that was probably intended to transport people or cargo down that tremendously long hall. ";

    public override string Name => "Dorm Corridor";
}