using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Admin;

internal class TransportationSupply : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<AdminCorridorNorth>() }
        };
    
    // Forever dark
    protected override string ContextBasedDescription => "";
    
    public override string Name => "Transportation Supply";
}