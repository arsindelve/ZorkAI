using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class ReservoirNorth : DarkLocation
{
    public override string Name => "Reservoir North";

    // No "lake" alias here: it is shared with the central Reservoir and Reservoir South, and from the
    // central bed it produced a "which shore?" prompt (issue #268 review). "go to the lake" from this
    // shore still reaches the central lake bed, which keeps the alias.

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Reservoir>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<AtlantisRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a large cavernous room, the south of which was formerly a lake. However, with the water level " +
            "lowered, there is merely a wide stream running through there.\nThere is a slimy stairway leaving " +
            "the room to the north.";
    }

    public override void Init()
    {
        StartWithItem<AirPump>();
    }
}