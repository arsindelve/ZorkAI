using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

internal class RecCorridor : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, Go<PlainHall>() },
            { Direction.N, Go<DormB>() },
            { Direction.S, Go<DormA>() }
        };

    protected override string ContextBasedDescription =>
        "This is a wide, east-west hallway. Portals lead north and south, and another corridor branches southwest. ";

    public override string Name => "Rec Corridor";
}