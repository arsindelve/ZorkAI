using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MachineShop : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.N, Go<MechCorridorSouth>() },
            { Direction.W, Go<ToolRoom>() },
            { Direction.E, Go<RobotShop>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This room is probably some sort of machine shop filled with a variety of unusual machines. Doorways lead " +
        "north, east, and west.\n\nStanding against the rear wall is a large dispensing machine with a spout. " +
        "The dispenser is lined with brightly colored\nbuttons. The first four buttons, labelled \"KUULINTS 1 - 4\", " +
        "are colored red, blue, green, and yellow. The next three\nbuttons, labelled \"KATALISTS 1 - 3\", are colored " +
        "gray, brown, and black. The last two buttons are both white. One of these is square and says \"BAAS.\" The " +
        "other white button is round and says \"ASID.\" ";

    public override string Name => "Machine Shop";
}