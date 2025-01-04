﻿using GameEngine.Location;
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
        // TODO not until you kill the thief! 
        StartWithItem<SilverChalice>();
    }

    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(25);
        base.OnFirstTimeEnterLocation(context);
    }
}