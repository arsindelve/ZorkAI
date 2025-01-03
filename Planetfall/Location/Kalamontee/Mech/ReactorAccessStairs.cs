using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorAccessStairs : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Up, Go<ReactorControl>() }
        };

    protected override string GetContextBasedDescription(IContext context) => "";

    public override string Name => "Reactor Access Stairs";
}