namespace ZorkOne.Location;

public class Attic : DarkLocation
{
    public Attic()
    {
        StartWithItem(Repository.GetItem<Rope>(), this);
        StartWithItem(Repository.GetItem<Knife>(), this);
    }

    protected override string Name => "Attic";

    protected override string ContextBasedDescription =>
        "This is the attic. The only exit is a stairway leading down. ";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, new MovementParameters { Location = GetLocation<Kitchen>() } }
        };
}