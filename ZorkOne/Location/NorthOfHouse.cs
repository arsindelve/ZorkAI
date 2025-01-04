using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class NorthOfHouse : LocationWithNoStartingItems
{
    public override string Name => "North of House";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are facing the north side of a white house. There is no door here, " +
               "and all the windows are boarded up. To the north a narrow path winds through the trees. ";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<BehindHouse>() }
            },
            {
                Direction.SE, new MovementParameters { Location = GetLocation<BehindHouse>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<WestOfHouse>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<ForestPath>() }
            },
            {
                Direction.S,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The windows are all boarded" }
            }
        };
    }
}