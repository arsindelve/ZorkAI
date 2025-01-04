using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class ClearingBehindHouse : LocationBase
{
    public override string Name => "Clearing";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a small clearing in a well marked forest path that extends to the east and west.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, new MovementParameters { Location = GetLocation<BehindHouse>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<CanyonView>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<ForestThree>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<ForestTwo>() }
            }
        };
    }

    public override void Init()
    {
    }
}