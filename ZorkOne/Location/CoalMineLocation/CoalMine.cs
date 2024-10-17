using GameEngine.Location;

namespace ZorkOne.Location.CoalMineLocation;

internal abstract class CoalMine : DarkLocationWithNoStartingItems
{
    protected override string ContextBasedDescription =>
        "This is a nondescript part of a coal mine. ";

    public override string Name => "Coal Mine";
}