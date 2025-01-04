using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class LadderTop : DarkLocation
{
    public override string Name => "Ladder Top";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<CoalMineFour>() }
            },
            {
                Direction.Down, new MovementParameters { Location = GetLocation<LadderBottom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a very small room. In the corner is a rickety wooden ladder, leading downward. It might be safe to descend. There is also a staircase leading upward. ";
    }

    public override void Init()
    {
        StartWithItem<WoodenLadder>();
    }
}