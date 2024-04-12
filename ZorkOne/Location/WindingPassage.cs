namespace ZorkOne.Location;

public class WindingPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Temple>() } }
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