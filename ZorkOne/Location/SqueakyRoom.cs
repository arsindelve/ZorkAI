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
        "You are standing at the entrance of what might have been a coal mine. The shaft enters " +
        "the west wall, and there is another exit on the south end of the room.";

    public override string Name => "Mine Entrance";

    public override void Init()
    {
    }
}