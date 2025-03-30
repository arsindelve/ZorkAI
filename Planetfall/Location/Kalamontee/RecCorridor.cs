using GameEngine.Location;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall.Location.Kalamontee;

internal class RecCorridor : LocationWithNoStartingItems
{
    public override string Name => "Rec Corridor";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<PlainHall>() },
            { Direction.W, Go<RecArea>() },
            { Direction.N, Go<DormB>() },
            { Direction.S, Go<DormA>() },
            { Direction.E, Go<MessCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a wide, east-west hallway. Portals lead north and south, and another corridor branches southwest. ";
    }
}