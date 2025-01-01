using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

public class ReservoirNorth : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Reservoir>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<AtlantisRoom>() } }
        };

    protected override string GetContextBasedDescription() =>
        "You are in a large cavernous room, the south of which was formerly a lake. However, with the water level " +
        "lowered, there is merely a wide stream running through there.\nThere is a slimy stairway leaving " +
        "the room to the north.";

    public override string Name => "Reservoir North";

    public override void Init()
    {
        StartWithItem<AirPump>();
    }
}