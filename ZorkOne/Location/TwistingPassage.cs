using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

public class TwistingPassage : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveNorth>() } }
        };

    protected override string GetContextBasedDescription() =>
        "This is a winding passage. It seems that there are only exits on the east and north. ";

    public override string Name => "Twisting Passage";
}