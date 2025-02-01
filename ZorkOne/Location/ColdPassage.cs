using GameEngine.Location;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Location;

public class ColdPassage : LocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "Cold Passage";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },

            { Direction.W, new MovementParameters { Location = GetLocation<SlideRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a cold and damp corridor where a long east-west passageway turns into a southward path.";
    }
}