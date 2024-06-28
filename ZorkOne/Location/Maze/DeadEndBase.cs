namespace ZorkOne.Location.Maze;

public abstract class DeadEndBase : DarkLocationWithNoStartingItems
{
    protected override string ContextBasedDescription =>
        "You have come to a dead end in the maze. ";

    public override string Name => "Dead End";

}