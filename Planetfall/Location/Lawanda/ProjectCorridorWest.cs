using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class ProjectCorridorWest : LocationWithNoStartingItems
{
    public override string Name => "Project Corridor West";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<ProjectCorridor>() },
            { Direction.NW, Go<Fork>() },
            { Direction.W, Go<SanfacF>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a curving hallway leading east and northwest. There is an opening to the west. ";
    }
}