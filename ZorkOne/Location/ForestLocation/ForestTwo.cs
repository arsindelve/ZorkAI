using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestTwo : LocationWithNoStartingItems
{
    public override string Name => "Forest";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
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
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a dimly lit forest, with trees all around";
    }
}