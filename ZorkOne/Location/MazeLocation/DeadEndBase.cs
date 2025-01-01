using GameEngine.Location;
using Model.Interface;

namespace ZorkOne.Location.MazeLocation;

public abstract class DeadEndBase : DarkLocationWithNoStartingItems
{
    protected override string GetContextBasedDescription(IContext context) => "You have come to a dead end in the maze. ";

    public override string Name => "Dead End";

}