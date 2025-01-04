using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

public class SanfacE : LocationWithNoStartingItems
{
    public override string Name => "SanFac E";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<AdminCorridorSouth>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "Here is another sanitary facility. Like the others, it is dusty and nonfunctional. ";
    }
}