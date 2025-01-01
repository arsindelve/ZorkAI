using GameEngine.Location;

namespace ZorkOne.Location.MazeLocation;

public abstract class DeadEndBase : DarkLocationWithNoStartingItems
{
    protected override string GetContextBasedDescription() => "You have come to a dead end in the maze. ";

    public override string Name => "Dead End";

}