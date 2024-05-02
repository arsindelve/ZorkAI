namespace ZorkOne.Location;

public class SqueakyRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<MineEntrance>() }
            },
            {
                Direction.N, new MovementParameters { Location = GetLocation<BatRoom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "You are in a small room. Strange squeaky sounds may be heard coming from the passage at the north end. You may also escape to the east. ";

    public override string Name => "Squeaky Room ";

    public override void Init()
    {
    }
}