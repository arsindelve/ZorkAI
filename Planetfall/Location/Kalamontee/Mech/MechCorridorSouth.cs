using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Mech;

internal class MechCorridorSouth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, Go<MechCorridor>() },
        };

    protected override string ContextBasedDescription =>
        "The corridor ends here with doorways to the southwest, south, and southeast. ";

    public override string Name => "Mech Corridor South";
}