namespace ZorkOne.Location;

public class MirrorRoomNorth : MirrorRoom
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<ColdPassage>() } }
            // { Direction.E, new MovementParameters { Location = GetLocation<CaveSouth>() } },
            // { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } }
        };
}