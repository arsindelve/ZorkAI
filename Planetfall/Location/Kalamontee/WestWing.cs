using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class WestWing : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.E, Go<Courtyard>() }
        };

    protected override string GetContextBasedDescription() =>
        "This was once the west wing of the castle, but the walls are now mostly rubble, " +
        "allowing a view of the cliff and ocean below. Rubble blocks all exits save one, eastward to the courtyard. ";

    public override string Name => "West Wing";
}