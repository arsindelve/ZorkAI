using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridorSouth : LocationWithNoStartingItems
{
    public override string Name => "Mech Corridor South";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MechCorridor>() },
            { Direction.S, Go<MachineShop>() },
            { Direction.SW, Go<ToolRoom>() },
            { Direction.SE, Go<RobotShop>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The corridor ends here with doorways to the southwest, south, and southeast. ";
    }
}