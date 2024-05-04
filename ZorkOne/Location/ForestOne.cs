using Model.Movement;

namespace ZorkOne.Location;

public class ForestOne : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.E, new MovementParameters { Location = GetLocation<ForestPath>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<Clearing>() }
        },
        {
            Direction.S, new MovementParameters { Location = GetLocation<ForestThree>() }
        },
        {
            Direction.W,
            new MovementParameters
                { CanGo = _ => false, CustomFailureMessage = "You would need a machete to go further west. " }
        }
    };

    public override string Name => "Forest";

    protected override string ContextBasedDescription =>
        "This is a forest, with trees in all directions. To the east, there appears to be sunlight. ";
}