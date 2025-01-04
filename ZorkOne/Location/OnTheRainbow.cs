using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class OnTheRainbow : LocationWithNoStartingItems
{
    public override string Name => "On The Rainbow";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.W, Go<EndOfRainbow>()
            },
            {
                Direction.E, Go<AragainFalls>()
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on top of a rainbow (I bet you never thought you would walk on a rainbow), " +
               "with a magnificent view of the Falls. The rainbow travels east-west here.";
    }
}