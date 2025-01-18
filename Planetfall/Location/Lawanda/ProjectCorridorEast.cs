using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class ProjectCorridorEast : LocationWithNoStartingItems
{
    public override string Name => "Project Corridor East";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<LibraryLobby>() },
            { Direction.W, Go<ProjectCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The hallway ends here but continues back toward the west. Doorways lead north, south and east. ";
    }
}