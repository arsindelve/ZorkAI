using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestThree : LocationWithNoStartingItems
{
    public override string Name => "Forest";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.NW, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
            },
            {
                Direction.S,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "Storm-tossed trees block your way. " }
            },
            {
                Direction.E,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "The rank undergrowth prevents eastward movement. " }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a dimly lit forest, with trees all around. ";
    }
}