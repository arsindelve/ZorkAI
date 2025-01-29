using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class LadderBottom : DarkLocation, IThiefMayVisit
{
    public override string Name => "Ladder Bottom";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<LadderTop>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<DeadEnd>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<TimberRoom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a rather wide room. On one side is the bottom of a narrow wooden ladder. To the west and the south are passages leaving the room. ";
    }

    public override void Init()
    {
        StartWithItem<WoodenLadder>();
    }
}