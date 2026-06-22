using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class GratingRoom : DarkLocation, IThiefMayVisit
{
    public override string Name => "Grating Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // The grating gates the passage up to the Clearing. "enter grate" routes to Direction.In
        // (EnterSubLocationEngine), so expose that passage under "in" too. (#262)
        var gratingPassage = new MovementParameters
        {
            Location = GetLocation<Clearing>(), CanGo = _ => GetItem<Grating>().IsOpen,
            CustomFailureMessage = "The grating is closed. "
        };

        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<MazeEleven>() } },
            { Direction.Up, gratingPassage },
            { Direction.In, gratingPassage }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a small room near the maze. There are twisty passages in the immediate vicinity. ";
    }

    public override void Init()
    {
        StartWithItem<Grating>();
    }
}