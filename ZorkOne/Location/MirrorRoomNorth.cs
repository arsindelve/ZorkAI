using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class MirrorRoomNorth : MirrorRoom
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<ColdPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveNorth>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<TwistingPassage>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<Mirror>();
    }
}