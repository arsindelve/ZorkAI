using GameEngine.Location;
using Model.Interface;

namespace ZorkOne.Location.CoalMineLocation;

internal abstract class CoalMine : DarkLocationWithNoStartingItems
{
    protected override string GetContextBasedDescription(IContext context) => "This is a nondescript part of a coal mine. ";

    public override string Name => "Coal Mine";
}