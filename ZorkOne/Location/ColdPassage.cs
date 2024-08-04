using GameEngine.Location;
using Model.Movement;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Location;

public class ColdPassage : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
          
             { Direction.W, new MovementParameters { Location = GetLocation<SlideRoom>() } }
        };

    protected override string ContextBasedDescription =>
        "This is a cold and damp corridor where a long east-west passageway turns into a southward path.";

    public override string Name => "Cold Passage";
}