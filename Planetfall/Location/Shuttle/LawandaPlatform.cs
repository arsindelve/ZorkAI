using GameEngine.Location;
using Model.Movement;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location.Shuttle;

internal class LawandaPlatform : LocationWithNoStartingItems
{
    public override string Name => "Lawanda Platform";
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            // TODO: Where we can enter depends on shuttle location. 
            { Direction.E, Go<Escalator>() },
            { Direction.S, Go<ShuttleCarAlfie>()},
            { Direction.N, Go<ShuttleCarBetty>()}
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        // TODO: Description when no shuttles
        // TODO: Description when one shuttles

        return
            "This is a wide, flat strip of concrete. Open shuttle cars lie to the north and south. A wide escalator, " +
            "not currently operating, beckons upward at the east end of the platform. A faded sign " +
            "reads \"Shutul Platform -- Lawanda Staashun.\"";
    }
}