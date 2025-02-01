using GameEngine.Location;
using Model.Interface;

namespace ZorkOne.Location.MazeLocation;

public abstract class DeadEndBase : DarkLocationWithNoStartingItems, IThiefMayVisit
{
    public override string Name => "Dead End";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You have come to a dead end in the maze. ";
    }
}