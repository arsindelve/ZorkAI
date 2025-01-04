using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestFour : LocationWithNoStartingItems
{
    public override string Name => "Forest";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, new MovementParameters { Location = GetLocation<ForestTwo>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<ForestTwo>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<ForestTwo>() }
            },
            {
                Direction.E,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The mountains are impassable. " }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The forest thins out, revealing impassible mountains. ";
    }
}