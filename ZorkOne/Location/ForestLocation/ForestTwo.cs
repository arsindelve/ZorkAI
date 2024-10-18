using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestTwo : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestPath>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<ForestFour>() }
        },
        {
            Direction.S, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
        },
        {
            Direction.N,
            new MovementParameters
                { CanGo = _ => false, CustomFailureMessage = "The forest becomes impenetrable to the north. " }
        }
    };

    public override string Name => "Forest";

    protected override string ContextBasedDescription => "This is a dimly lit forest, with trees all around";
}