using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class FrigidRiverOne : LocationWithNoStartingItems, IFrigidRiver
{
    private static PileOfPlastic Boat => Repository.GetItem<PileOfPlastic>();

    public override string Description => Name +
                                          SubLocation?.LocationDescription +
                                          Environment.NewLine +
                                          ContextBasedDescription; 

    
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<DamBase>() } }
        };

    protected override string ContextBasedDescription =>
        "You are on the Frigid River in the vicinity of the Dam. The river flows quietly here. There is a landing on the west shore. " +
        ( Boat.Items.Any() ? Environment.NewLine + Boat.ItemListDescription("magic boat") : "");

    public override string Name => "Frigid River";

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        newLocation.SubLocation = SubLocation;
        SubLocation = null;
        ((ICanHoldItems)newLocation).ItemPlacedHere(Boat);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}