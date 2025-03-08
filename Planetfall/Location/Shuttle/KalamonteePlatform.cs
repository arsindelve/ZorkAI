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
            { Direction.W, Go<WaitingArea>() },
            { Direction.S, Go<ShuttleCarAlfie>()}
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a wide, flat strip of concrete which continues westward. A large transport of some sort lies " +
            "to the south, its open door beckoning you to enter. A faded sign on the wall reads \"Shutul Platform -- Kalamontee Staashun.\"";
    }
}