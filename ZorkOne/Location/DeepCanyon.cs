namespace ZorkOne.Location;

public class DeepCanyon : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.SW, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on the south edge of a deep canyon. Passages lead off to the east, " +
        "northwest and southwest. A stairway leads down. You can hear the sound of flowing water from below.";

    public override string Name => "Deep Canyon";

    public override void Init()
    {
    }
}