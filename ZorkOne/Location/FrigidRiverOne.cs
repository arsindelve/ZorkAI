using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class FrigidRiverOne : LocationWithNoStartingItems, IFrigidRiver
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<DamBase>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on the Frigid River in the vicinity of the Dam. The river flows quietly here. There is a landing on the west shore. " +
        Repository.GetItem<PileOfPlastic>().ItemListDescription("magic boat");

    public override string Name => "Frigid River";
}