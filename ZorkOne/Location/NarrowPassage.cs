using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class NarrowPassage : DarkLocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "Narrow Passage";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a long and narrow corridor where a long north-south passageway briefly narrows even further. ";
    }
}