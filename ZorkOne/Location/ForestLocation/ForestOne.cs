using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestOne : LocationWithNoStartingItems
{
    public override string Name => "Forest";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E,
                new MovementParameters { Location = GetLocation<ForestPath>() }
            },
            {
                Direction.N,
                new MovementParameters { Location = GetLocation<Clearing>() }
            },
            {
                Direction.S,
                new MovementParameters { Location = GetLocation<ForestThree>() }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "You would need a machete to go further west. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a forest, with trees in all directions. To the east, there appears to be sunlight. ";
    }
}