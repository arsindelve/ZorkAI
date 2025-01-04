using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public abstract class MazeBase : DarkLocationWithNoStartingItems
{
    public override string Name => "Maze";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is part of a maze of twisty little passages, all alike. ";
    }
}

public class MazeOne : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<TrollRoom>() },
            { Direction.N, Go<MazeOne>() },
            { Direction.S, Go<MazeTwo>() },
            { Direction.W, Go<MazeFour>() }
        };
    }
}

public class MazeTwo : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<MazeThree>() },
            { Direction.S, Go<MazeOne>() },
            { Direction.Down, Go<MazeFour>() }
        };
    }
}

public class MazeThree : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MazeTwo>() },
            { Direction.N, Go<MazeFour>() },
            { Direction.Up, Go<MazeFive>() }
        };
    }
}

public class MazeFour : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MazeThree>() },
            { Direction.N, Go<MazeOne>() },
            { Direction.E, Go<DeadEndOne>() }
        };
    }
}

public class MazeFive : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MazeThree>() },
            { Direction.E, Go<DeadEndTwo>() },
            { Direction.SW, Go<MazeSix>() }
        };
    }

    public override void Init()
    {
        StartWithItem<Skeleton>();
        StartWithItem<BurnedOutLantern>();
        StartWithItem<RustyKnife>();
        StartWithItem<SkeletonKey>();
        StartWithItem<BagOfCoins>();

        base.Init();
    }
}

public class MazeSix : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<MazeFive>() },
            { Direction.W, Go<MazeSix>() },
            { Direction.E, Go<MazeSeven>() },
            { Direction.Up, Go<MazeNine>() }
        };
    }
}

public class MazeSeven : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MazeSix>() },
            { Direction.S, Go<MazeFifteen>() },
            { Direction.E, Go<MazeEight>() },
            { Direction.Down, Go<DeadEndOne>() },
            { Direction.Up, Go<MazeFourteen>() }
        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return LocationHelper.CheckSwordNoLongerGlowing<Cyclops, CyclopsRoom, MazeFifteen>(previousLocation, context,
            base.AfterEnterLocation(context, previousLocation, generationClient));
    }
}

public class MazeEight : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MazeEight>() },
            { Direction.NE, Go<MazeSeven>() },
            { Direction.SE, Go<DeadEndThree>() }
        };
    }
}

public class MazeNine : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NW, Go<MazeNine>() },
            { Direction.N, Go<MazeSix>() },
            { Direction.W, Go<MazeTwelve>() },
            { Direction.E, Go<MazeTen>() },
            { Direction.S, Go<MazeThirteen>() },
            { Direction.Down, Go<MazeEleven>() }
        };
    }
}

public class MazeTen : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<MazeNine>() },
            { Direction.W, Go<MazeThirteen>() },
            { Direction.Up, Go<MazeEleven>() }
        };
    }
}

public class MazeEleven : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<MazeTwelve>() },
            { Direction.NW, Go<MazeThirteen>() },
            { Direction.Down, Go<MazeTen>() },
            { Direction.NE, Go<GratingRoom>() }
        };
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (previousLocation is MazeNine)
            return
                "You won't be able to get back up to the tunnel you are going through when it gets to the next room.\n ";

        return base.BeforeEnterLocation(context, previousLocation);
    }
}

public class MazeTwelve : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<MazeFive>() },
            { Direction.Up, Go<MazeNine>() },
            { Direction.N, Go<DeadEndFour>() },
            { Direction.SW, Go<MazeEleven>() },
            { Direction.E, Go<MazeThirteen>() }
        };
    }
}

public class MazeThirteen : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<MazeTwelve>() },
            { Direction.W, Go<MazeEleven>() },
            { Direction.S, Go<MazeTen>() },
            { Direction.E, Go<MazeNine>() }
        };
    }
}

public class MazeFourteen : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NW, Go<MazeFourteen>() },
            { Direction.NE, Go<MazeSeven>() },
            { Direction.S, Go<MazeSeven>() }
        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return
            LocationHelper.CheckSwordNoLongerGlowing<Cyclops, CyclopsRoom, MazeFifteen>(previousLocation, context,
                base.AfterEnterLocation(context, previousLocation, generationClient));
    }
}

public class MazeFifteen : MazeBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<MazeFourteen>() },
            { Direction.S, Go<MazeSeven>() },
            { Direction.SE, Go<CyclopsRoom>() }
        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingFaintly<Cyclops, CyclopsRoom>(context);

        if (!string.IsNullOrEmpty(glow))
            return Task.FromResult(glow);

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}