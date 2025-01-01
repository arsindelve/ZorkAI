using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class ForestFour : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
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

    public override string Name => "Forest";

    protected override string GetContextBasedDescription(IContext context) => "The forest thins out, revealing impassible mountains. ";
}