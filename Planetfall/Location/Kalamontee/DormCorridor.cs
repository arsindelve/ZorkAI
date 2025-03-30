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
        if (previousLocation is CorridorJunction)
            return
                "You walk down the long, featureless hallway for a long time. Finally, you see an intersection ahead...\n\n";

        return base.BeforeEnterLocation(context, previousLocation);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a wide, east-west hallway with openings to the north and south. To the east, the corridor " +
               "stretches off into the distance. That section of the hallway is lined with a motorized walkway " +
               "(no longer running) that was probably intended to transport people or cargo down that tremendously long hall. ";
    }
}