using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

public class SanfacE : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.W, Go<AdminCorridorSouth>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "Here is another sanitary facility. Like the others, it is dusty and nonfunctional. ";

    public override string Name => "SanFac E";
}