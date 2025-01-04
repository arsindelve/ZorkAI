using GameEngine;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public class FrigidRiverOne : FrigidRiverBase
{
    protected override FrigidRiverBase CarriedToLocation => Repository.GetLocation<FrigidRiverTwo>();

    protected override int TurnsUntilSweptDownstream => 3;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<DamBase>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are on the Frigid River in the vicinity of the Dam. The river flows quietly here. There is a landing on the west shore. " +
            (Boat.Items.Any() ? Environment.NewLine + Boat.ItemListDescription("magic boat", null) : "");
    }
}