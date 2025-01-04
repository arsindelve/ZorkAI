using GameEngine.Location;
using Model.Interface;

namespace ZorkOne.Location.CoalMineLocation;

internal abstract class CoalMine : DarkLocationWithNoStartingItems
{
    public override string Name => "Coal Mine";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a nondescript part of a coal mine. ";
    }
}