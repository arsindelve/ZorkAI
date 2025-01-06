using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Location.Kalamontee;

internal class UpperElevator : ElevatorBase<UpperElevatorDoor>
{
    public override string Name => "Upper Elevator";


    protected override string ExitDirection => "south";
}

internal class LowerElevator : ElevatorBase<LowerElevatorDoor>
{
    public override string Name => "Lower Elevator";

    protected override string ExitDirection => "north";
}

internal class ElevatorLobby : LocationBase
{
    public override string Name => "Elevator Lobby";

    public override void Init()
    {
        StartWithItem<LowerElevatorDoor>();
        StartWithItem<UpperElevatorDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<CorridorJunction>() },
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => GetItem<LowerElevatorDoor>().IsOpen, CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<LowerElevator>()
                }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => GetItem<UpperElevatorDoor>().IsOpen, CustomFailureMessage = "The door is closed.",
                    Location = GetLocation<UpperElevator>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a wide, brightly lit lobby. A blue metal door to the north is closed and a larger red metal " +
            "door to the south is also closed. Beside the blue door is a blue button, and beside the red door " +
            "is a red button. A corridor leads west. To the east is a small room about the size of a telephone booth. ";
    }
}

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