using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class WindingStair : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, Go<Balcony>() },
            { Direction.Up, Go<Courtyard>() }
        };

    protected override string GetContextBasedDescription() =>
        "The middle of a long, steep stairway carved into the face of a cliff. ";

    public override string Name => "Winding Stair";
}