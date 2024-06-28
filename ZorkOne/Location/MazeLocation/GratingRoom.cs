using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class GratingRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<MazeEleven>() } },
            {
                Direction.Up,
                new MovementParameters
                {
                    Location = GetLocation<Clearing>(), CanGo = _ => GetItem<Grating>().IsOpen,
                    CustomFailureMessage = "The grating is closed. "
                }
            }
        };

    protected override string ContextBasedDescription =>
        "You are in a small room near the maze. There are twisty passages in the immediate vicinity. ";
    public override string Name => "Grating Room";

    public override void Init()
    {
        StartWithItem<Grating>(this);
    }
}