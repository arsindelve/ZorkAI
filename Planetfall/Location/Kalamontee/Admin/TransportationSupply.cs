using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Admin;

internal class TransportationSupply : DarkLocationWithNoStartingItems
{
    public override string Name => "Transportation Supply";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<AdminCorridorNorth>() }
        };
    }

    // Forever dark
    protected override string GetContextBasedDescription(IContext context)
    {
        return "";
    }
}