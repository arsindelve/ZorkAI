using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CaveNorth : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<AtlantisRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<TwistingPassage>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a tiny cave with entrances west and north, and a staircase leading down. ";

    public override string Name => "Cave";
}