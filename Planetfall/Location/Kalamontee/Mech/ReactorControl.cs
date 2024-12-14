using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class ReactorControl : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, Go<MechCorridor>() },
        };

    protected override string ContextBasedDescription =>
        "This room contains many dials and gauges for controlling a massive planetary power reactor which, according " +
        "to a diagram on the wall, must be buried far below this very complex. The exit is to the west. To the east " +
        "is a metal door, and next to it, a button. A dark stairway winds downward. ";
    public override string Name => "Reactor Control";
}