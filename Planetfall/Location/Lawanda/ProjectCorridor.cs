using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class ProjectCorridor : LocationWithNoStartingItems
{
    public override string Name => "Project Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<ProjectCorridorEast>() },
            { Direction.W, Go<ProjectCorridorWest>() },
            { Direction.S, Go<ProjConOffice>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are at the center of a long east-west hallway. A doorway, labelled \"PrajKon Awfis\", opens to the south. ";
    }
}