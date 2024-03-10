using Game;
using Game.Location;
using Model;
using ZorkOne.Item;

namespace ZorkOne.Location;

internal class Studio : BaseLocation
{
    public Studio()
    {
        StartWithItem(Repository.GetItem<Manual>(), this);
    }

    protected override string Name => "Studio";

    protected override string ContextBasedDescription =>
        "This appears to have been an artist's studio. The walls and floors are splattered with paints of 69 different colors. " +
        "Strangely enough, nothing of value is hanging here. At the south end of the room is an open door (also covered with paint). " +
        "A dark and narrow chimney leads up from a fireplace; although you might be able to get up it, it seems unlikely you could get back down. ";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Gallery>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<Kitchen>() } }
        };
}