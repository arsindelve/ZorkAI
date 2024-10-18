using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class TreasureRoom : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        { Direction.Down, Go<CyclopsRoom>() }
    };

    protected override string ContextBasedDescription =>
        "This is a large room, whose east wall is solid granite. A number of discarded bags, which " +
        "crumble at your touch, are scattered about on the floor. There is an exit down a staircase. ";

    public override string Name => "Treasure Room";

    public override void Init()
    {
       
    }
    
    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(25);
        base.OnFirstTimeEnterLocation(context);
    }
}