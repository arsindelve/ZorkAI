using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveNorth : DarkLocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "Cave";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<AtlantisRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<TwistingPassage>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a tiny cave with entrances west and north, and a staircase leading down. ";
    }
}