using GameEngine.Location;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Location.Kalamontee;

internal class DormCorridor : LocationWithNoStartingItems
{
    public override string Name => "Dorm Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<CorridorJunction>() },
            { Direction.W, Go<MessCorridor>() },
            { Direction.N, Go<DormD>() },
            { Direction.S, Go<DormC>() }
        };
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Issue #473: prepend the transition text to base (mirroring AdminCorridorNorth) rather than
        // returning it standalone. base.BeforeEnterLocation is the only place VisitCount is
        // incremented (and OnFirstTimeEnterLocation fires); returning early skipped it, so in Brief
        // mode the first-visit room description was silently dropped.
        string prepend = "";
        if (previousLocation is CorridorJunction)
            prepend =
                "You walk down the long, featureless hallway for a long time. Finally, you see an intersection ahead...\n\n";

        return prepend + base.BeforeEnterLocation(context, previousLocation);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a wide, east-west hallway with openings to the north and south. To the east, the corridor " +
               "stretches off into the distance. That section of the hallway is lined with a motorized walkway " +
               "(no longer running) that was probably intended to transport people or cargo down that tremendously long hall. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["walkway", "motorized walkway", "moving walkway"],
            "The motorized walkway that once carried people and cargo down the long hall is no longer running. ",
            "It's built into the floor. ")
    ];
}