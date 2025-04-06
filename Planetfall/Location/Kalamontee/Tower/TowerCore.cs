using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Tower;

internal class TowerCore : LocationWithNoStartingItems
{
    public override string Name => "Tower Core";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SW, Go<ObservationDeck>() },
            { Direction.N, Go<UpperElevator>() },
            { Direction.Up, Go<Helipad>() },
            { Direction.NE, Go<CommRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small, circular room. A sliding door leads north, and a spiral staircase heads " +
            "upwards. Other exits lie to the northeast and southwest.";
    }
    
    protected override void OnFirstTimeEnterLocation(IContext context)
    {
        context.AddPoints(4);
    }
}