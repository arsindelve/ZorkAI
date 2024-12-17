using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class DeckNine : LocationBase, ITurnBasedActor
{
    
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Up, Go<Gangway>() },
            { Direction.E, Go<ReactorLobby>() },
            {
                Direction.W,
                new MovementParameters
                {
                    Location = Repository.GetLocation<EscapePod>(),
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The escape pod bulkhead is closed. "
                }
            },
            {
                Direction.In,
                new MovementParameters
                {
                    Location = Repository.GetLocation<EscapePod>(),
                    CanGo = _ => Repository.GetItem<BulkheadDoor>().IsOpen,
                    CustomFailureMessage = "The escape pod bulkhead is closed. "
                }
            }
        };

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["clean", "scrub", "wash"], ["floor", "deck"]))
        {
            return new PositiveInteractionResult("The floor is a bit shinier now. ");
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        Repository.GetItem<Blather>().LeavesTheScene(context);
        Repository.GetItem<Ambassador>().LeavesTheScene(context);
        context.RemoveActor(this);
    }

    protected override string ContextBasedDescription =>
        "This is a featureless corridor similar to every other corridor on the ship. It curves away to starboard," +
        " and a gangway leads up. To port is the entrance to one of the ship's primary escape pods. " +
        $"The pod bulkhead is {(Repository.GetItem<BulkheadDoor>().IsOpen ? "open" : "closed")}. ";

    public override string Name => "Deck Nine";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        // Deck nine is special. This location is the epicenter of the explosion (from a code perspective)
        // so this "act" function will fire every move, no matter where we are, until we safely
        // make it to the escape pod (or die). So below, it's important thay we always confirm
        // where we are, and whether or not we want an action to happen in this location. 

        var ambassador = Repository.GetItem<Ambassador>();
        var blather = Repository.GetItem<Blather>();

        // Let's see if the ambassador or Blather will join us. 
        if (context.Moves is > 1 and < 7 &&
                context.CurrentLocation is DeckNine &&
                !Items.Contains(ambassador) &&
                !Items.Contains(blather))
        {
            int chance = Random.Shared.Next(6);
            switch (chance)
            {
                case 0:
                    return Task.FromResult(ambassador.JoinsTheScene(context, this));
                case 1:
                    return Task.FromResult(blather.JoinsTheScene(context, this));
            }
        }

        return Task.FromResult("");
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation, IGenerationClient generationClient)
    {
        // This covers us in case you enter the pod (which turns off the explosion action)
        // and then step back onto the deck. You will explode soon. 
        context.RegisterActor(new ExplosionCoordinator());
        
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}