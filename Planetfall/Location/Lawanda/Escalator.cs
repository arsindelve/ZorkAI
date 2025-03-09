using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Shuttle;

namespace Planetfall.Location.Lawanda;

internal class Escalator : LocationWithNoStartingItems
{
    public override string Name => "Escalator";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<Fork>() },
            { Direction.W, Go<LawandaPlatform>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in the middle of a long mechanical stairway. It is not running, and seems to be in disrepair. ";
    }
}