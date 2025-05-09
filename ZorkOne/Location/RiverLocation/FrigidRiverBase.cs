using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.RiverLocation;

public abstract class FrigidRiverBase : LocationWithNoStartingItems, ITurnBasedActor, IFrigidRiver
{
    protected abstract FrigidRiverBase CarriedToLocation { get; }

    protected static PileOfPlastic Boat => Repository.GetItem<PileOfPlastic>();

    protected abstract int TurnsUntilSweptDownstream { get; }

    [UsedImplicitly]
    public int TurnsInThisLocation { get; set; }

    public override string Name => "Frigid River";

    public virtual async Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsInThisLocation++;

        if (TurnsInThisLocation >= TurnsUntilSweptDownstream)
        {
            var moveInteraction = new MovementParameters { Location = CarriedToLocation };
            var result = await MoveEngine.Go(context, client, moveInteraction);
            return "\nThe flow of the river carries you downstream. \n\n" + result;
        }

        return string.Empty;
    }

    public override string GetDescription(IContext context, bool fullDescription = true)
    {
        return Name +
               SubLocation?.LocationDescription +
               Environment.NewLine +
               GetContextBasedDescription(context);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void OnWaiting(IContext context)
    {
        // Waiting automatically pushes you downstream
        TurnsInThisLocation = TurnsUntilSweptDownstream;
        base.OnWaiting(context);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        newLocation.SubLocation = SubLocation;
        SubLocation = null;
        ((ICanContainItems)newLocation).ItemPlacedHere(Boat);
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }
}