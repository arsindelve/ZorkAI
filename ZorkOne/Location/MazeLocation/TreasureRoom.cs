using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class TreasureRoom : LocationBase
{
    public override string Name => "Treasure Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<CyclopsRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a large room, whose east wall is solid granite. A number of discarded bags, which " +
               "crumble at your touch, are scattered about on the floor. There is an exit down a staircase. ";
    }

    public override void Init()
    {
        StartWithItem<SilverChalice>();
        StartWithItem<Thief>();
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor<Thief>();
        GetItem<Thief>().IsUnconscious = false;
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (GetItem<Thief>().IsDead)
            return base.BeforeEnterLocation(context, previousLocation);

        ItemPlacedHere<Thief>();
        context.RegisterActor(GetItem<Thief>());

        base.BeforeEnterLocation(context, previousLocation);
        
        return "You hear a scream of anguish as you violate the robber's hideaway. Using passages unknown to you, he " +
               "rushes to its defense.\nThe thief gestures mysteriously, and the treasures in the room suddenly vanish.\n\n";
               
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(25);
        base.OnFirstTimeEnterLocation(context);
    }
}