using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Location.Kalamontee;

internal class RecCorridor : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, Go<PlainHall>() },
            { Direction.W, Go<RecArea>() },
            { Direction.N, Go<DormB>() },
            { Direction.S, Go<DormA>() },
            { Direction.E, Go<MessCorridor>() }
        };

    protected override string GetContextBasedDescription() =>
        "This is a wide, east-west hallway. Portals lead north and south, and another corridor branches southwest. ";

    public override string Name => "Rec Corridor";
}