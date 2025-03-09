using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Shuttle;

internal class KalamonteePlatform : LocationWithNoStartingItems
{
    public override string Name => "Kalamontee Platform";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            // TODO: Where we leave depends on shuttle locations. 
            { Direction.W, Go<WaitingArea>() },
            { Direction.S, Go<ShuttleCarAlfie>()},
            { Direction.N, Go<ShuttleCarBetty>()}
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // TODO: Description when no shuttles
        // TODO: Description when two shuttles
        return
            "This is a wide, flat strip of concrete which continues westward. A large transport of some sort lies " +
            "to the south, its open door beckoning you to enter. A faded sign on the wall reads \"Shutul Platform -- Kalamontee Staashun.\"";
    }
}