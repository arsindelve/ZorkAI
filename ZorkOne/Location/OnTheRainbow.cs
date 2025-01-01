using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class OnTheRainbow : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            {
                Direction.W, Go<EndOfRainbow>()
            },
            {
                Direction.E, Go<AragainFalls>()
            }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are on top of a rainbow (I bet you never thought you would walk on a rainbow), " +
        "with a magnificent view of the Falls. The rainbow travels east-west here.";

    public override string Name => "On The Rainbow";
}