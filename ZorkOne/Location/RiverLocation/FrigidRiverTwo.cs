using GameEngine;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverTwo : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverThree>();

    protected override int TurnsUntilSweptDownstream => 3;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The river turns a corner here making it impossible to see the Dam. The White Cliffs loom on the east bank " +
            "and large rocks prevent landing on the west. ";
    }
}