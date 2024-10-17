namespace ZorkOne.Location;

internal class MirrorRoomNorth : MirrorRoom
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<ColdPassage>() } }
        };
}