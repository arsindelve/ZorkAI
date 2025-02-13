using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class Fork : LocationWithNoStartingItems
{
    public override string Name => "Fork";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<SystemsCorridorWest>() },
            { Direction.SE, Go<ProjectCorridorWest>() },
            { Direction.W, Go<Escalator>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a hallway which forks to the northeast and southeast. To the west is the top of a long escalator. ";
    }
}