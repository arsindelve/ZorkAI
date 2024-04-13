namespace ZorkOne.Location;

public class NarrowPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "This is a long and narrow corridor where a long north-south passageway briefly narrows even further. ";

    public override string Name => "Narrow Passage";

    public override void Init()
    {
    }
}