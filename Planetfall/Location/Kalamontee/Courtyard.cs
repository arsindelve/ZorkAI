using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class Courtyard : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, Go<WindingStair>() }
        };

    protected override string ContextBasedDescription =>
        "You are in the courtyard of an ancient stone edifice, vaguely reminiscent of the castles you saw " +
        "during your leave on Ramos Two. It has decayed to the point where it can probably be termed a ruin. " +
        "Openings lead north and west, and a stairway downward is visible to the south. ";

    public override string Name => "Courtyard";
}