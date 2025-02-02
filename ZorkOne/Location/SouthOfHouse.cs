using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class SouthOfHouse : LocationWithNoStartingItems
{
    public override string Name => "South of House";

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are facing the south side of a white house. There is no door here, and all the windows are boarded.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, new MovementParameters { Location = GetLocation<WestOfHouse>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<BehindHouse>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<ForestThree>() }
            },
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "The windows are all boarded."
                }
            }
        };
    }
}