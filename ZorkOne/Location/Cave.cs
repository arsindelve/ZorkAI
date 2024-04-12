namespace ZorkOne.Location;

public class Cave : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<WindingPassage>() } }
        };

    // TODO: Your sword is glowing with a faint blue glow.
    protected override string ContextBasedDescription =>
        "This is a tiny cave with entrances west and north, and a dark, forbidding staircase leading down. ";
    
    public override string Name => "Cave";

    public override void Init()
    {
    }
}