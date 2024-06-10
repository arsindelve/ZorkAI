using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverFour : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverFive>();

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.E,
                new MovementParameters
                {
                    Location = GetLocation<SandyBeach>()
                }
            }
        };

    protected override int TurnsUntilSweptDownstream => 3;

    protected override string ContextBasedDescription =>
        "The river is running faster here and the sound ahead appears to be that of rushing water. On the east " +
        "shore is a sandy beach. A small area of beach can also be seen below the cliffs on the west shore. " +
        (Items.Contains(Repository.GetItem<Buoy>())
            ? "\nThere is a red buoy here (probably a warning). "
            : "");

    public override string Name => "Frigid River";

    public override void Init()
    {
        StartWithItem<Buoy>(this);
    }
}