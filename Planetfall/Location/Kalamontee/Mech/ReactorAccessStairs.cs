using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorAccessStairs : DarkLocationWithNoStartingItems
{
    public override string Name => "Reactor Access Stairs";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Up, Go<ReactorControl>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "";
    }
}