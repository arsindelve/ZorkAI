using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class WestWing : LocationWithNoStartingItems
{
    public override string Name => "West Wing";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<Courtyard>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This was once the west wing of the castle, but the walls are now mostly rubble, " +
               "allowing a view of the cliff and ocean below. Rubble blocks all exits save one, eastward to the courtyard. ";
    }
}