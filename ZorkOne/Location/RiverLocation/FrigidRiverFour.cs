using GameEngine;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverFour : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverFive>();

    protected override int TurnsUntilSweptDownstream => 3;

    public override string Name => "Frigid River";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E,
                new MovementParameters
                {
                    Location = GetLocation<SandyBeach>()
                }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    Location = GetLocation<WhiteCliffsBeachSouth>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The river is running faster here and the sound ahead appears to be that of rushing water. On the east " +
            "shore is a sandy beach. A small area of beach can also be seen below the cliffs on the west shore. " +
            (Items.Contains(Repository.GetItem<Buoy>())
                ? "\nThere is a red buoy here (probably a warning). "
                : "");
    }

    public override void Init()
    {
        StartWithItem<Buoy>();
    }
}