namespace ZorkOne.Location;

public class MirrorRoomSouth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<NarrowPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Cave>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } }
        };

    protected override string ContextBasedDescription =>
        "You are in a large square room with tall ceilings. On the south wall is an " +
        "enormous mirror which fills the entire wall. There are exits on the other three sides of the room.";

    public override string Name => "Mirror Room";
}

public class WindingPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Cave>() } }
        };

    // TODO: Your sword is glowing with a faint blue glow.
    protected override string ContextBasedDescription =>
        "This is a winding passage. It seems that there are only exits on the east and north. ";

    // TODO: Your sword is no longer glowing.

    public override string Name => "Winding Passage";

    public override void Init()
    {
    }
}