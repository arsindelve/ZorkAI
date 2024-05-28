using Model.AIGeneration;
using Model.Interface;

namespace ZorkOne.Location;

public abstract class FrigidRiverBase : LocationWithNoStartingItems, ITurnBasedActor, IFrigidRiver
{
    protected static PileOfPlastic Boat => Repository.GetItem<PileOfPlastic>();

    public override string Description => Name +
                                          SubLocation?.LocationDescription +
                                          Environment.NewLine +
                                          ContextBasedDescription;


    public override string Name => "Frigid River";

    public abstract Task<string> Act(IContext context, IGenerationClient client);

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        newLocation.SubLocation = SubLocation;
        SubLocation = null;
        ((ICanHoldItems)newLocation).ItemPlacedHere(Boat);
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}