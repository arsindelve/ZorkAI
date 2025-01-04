using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class TwistingPassage : DarkLocationWithNoStartingItems
{
    public override string Name => "Twisting Passage";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomNorth>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<CaveNorth>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a winding passage. It seems that there are only exits on the east and north. ";
    }
}