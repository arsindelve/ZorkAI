namespace ZorkOne.Location;

public class EgyptianRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<Temple>() } }
        };

    public override string Name => "Egyptian Room";

    protected override string ContextBasedDescription =>
        "This is a room which looks like an Egyptian tomb. There is an ascending staircase to the west. ";

    public override void Init()
    {
        StartWithItem(GetItem<Coffin>(), this);
    }
}