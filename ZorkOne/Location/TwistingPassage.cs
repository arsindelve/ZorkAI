using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class TwistingPassage : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveNorth>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a winding passage. It seems that there are only exits on the east and north. ";

    public override string Name => "Twisting Passage";
}