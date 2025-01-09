using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee;

internal class CorridorJunction : LocationWithNoStartingItems
{
    public override string Name => "Corridor Junction";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<DormCorridor>() },
            { Direction.S, Go<MechCorridorNorth>() },
            { Direction.N, Go<AdminCorridorSouth>() },
            { Direction.E, Go<ElevatorLobby>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "A north-south corridor intersects the main corridor here. To the west, the main corridor extends as far as " +
            "you can see; a nonworking walkway from that direction ends here. To the east, the corridor widens into a well-lit area. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (previousLocation is DormCorridor)
            return
                "You walk down the long, featureless hallway for a long time. Finally, you see an intersection ahead...\n\n";

        return base.BeforeEnterLocation(context, previousLocation);
    }
}