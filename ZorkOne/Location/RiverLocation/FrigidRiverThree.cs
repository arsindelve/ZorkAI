using GameEngine;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverThree : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverFour>();

    protected override int TurnsUntilSweptDownstream => 3;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The river descends here into a valley. There is a narrow beach on the west shore below the cliffs. " +
               "In the distance a faint rumbling can be heard. ";
    }
}