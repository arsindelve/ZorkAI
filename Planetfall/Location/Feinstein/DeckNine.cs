using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;

namespace Planetfall.Location.Feinstein;

internal class DeckNine : LocationBase, ITurnBasedActor
{
    public const byte TurnWhenFeinsteinBlowsUp = 10;

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

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        Repository.GetItem<Blather>().LeavesTheScene(context);
        Repository.GetItem<Ambassador>().LeavesTheScene(context);
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

        return Task.FromResult(HandleExplosion(context));
    }

    private string HandleExplosion(IContext context)
    {
        string action = "";

        switch (context.Moves)
        {
            case TurnWhenFeinsteinBlowsUp:

                Repository.GetItem<BulkheadDoor>().IsOpen = true;
                action +=
                    $"\n\nA massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. " +
                    $"{(context.CurrentLocation == this ? "The door to port slides open. " : "")}";

                if (context.CurrentLocation is BlatherLocation)
                    action += "Blather, looking slightly disoriented, barks at you to resume your assigned duties. ";
                break;

            case TurnWhenFeinsteinBlowsUp + 1:
                {
                    action = context.CurrentLocation switch
                    {
                        _ when context.CurrentLocation is BlatherLocation => "You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.",
                        _ when context.CurrentLocation is DeckNine => "\n\nMore distant explosions! A narrow emergency bulkhead at the base of the gangway and a wider one along the corridor to starboard both crash shut! ",
                        _ when context.CurrentLocation is Gangway => "Another explosion. A narrow bulkhead at the base of the gangway slams shut! ",
                        _ => "\n\nThe ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. "
                    };

                    break;
                }

            case TurnWhenFeinsteinBlowsUp + 2:
                {
                    if (context.CurrentLocation is not DeckNine && context.CurrentLocation is not EscapePod)
                    {
                        var result =
                            "\n\nThe ship rocks from the force of multiple explosions. The lights go out, and you feel a " +
                            "sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...";

                        action = YouExploded(context, result);
                        break;
                    }

                    Repository.GetItem<BulkheadDoor>().IsOpen = false;
                    action =
                        $"\n\nMore powerful explosions buffet the ship. The lights flicker madly" +
                        $"{(context.CurrentLocation == this ? ", and the escape-pod bulkhead clangs shut" : "")}. ";
                    break;
                }

            case TurnWhenFeinsteinBlowsUp + 3:
                action = "\n\nExplosions continue to rock the ship. ";
                break;

            case TurnWhenFeinsteinBlowsUp + 4:
                {
                    var result =
                        "\n\nAn enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...";

                    action = YouExploded(context, result);
                    break;
                }
        }

        return action;
    }

    private string YouExploded(IContext context, string result)
    {
        context.RemoveActor(this);
        return new DeathProcessor().Process(result, context).InteractionMessage;
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation, IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        context.RemoveActor(Repository.GetLocation<EscapePod>());
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }
}