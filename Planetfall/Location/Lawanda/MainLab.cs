using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class MainLab : LocationWithNoStartingItems
{
    public override string Name => "Main Lab";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<ProjectCorridorEast>() },
            { Direction.SW, Go<ComputerRoom>() },
            { Direction.S, Go<LabStorage>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the heart of the Project's vast laboratory complex. There are exits to the west and southwest, " +
            "and heavy metal doors to the northeast and southeast. A small doorway leads south. ";
    }
}