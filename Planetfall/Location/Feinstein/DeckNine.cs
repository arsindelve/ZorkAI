using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;
using Planetfall.Item;
using Planetfall.Item.Feinstein;

namespace Planetfall.Location.Feinstein;

internal class DeckNine : BaseLocation, ITurnBasedActor
{
    public bool AmbassadorHasCome { get; set; }

    public bool BlatherHasCome { get; set; }

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

    protected override string ContextBasedDescription =>
        "This is a featureless corridor similar to every other corridor on the ship. It curves away to starboard," +
        " and a gangway leads up. To port is the entrance to one of the ship's primary escape pods. " +
        $"The pod bulkhead is {(Repository.GetItem<BulkheadDoor>().IsOpen ? "open" : "closed")}. ";

    public override string Name => "Deck Nine";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        var action = HandleExplosion(context);

        // Let's see if the ambassador will join us. 
        if (context.Moves is > 1 and < 8 && !AmbassadorHasCome && !BlatherHasCome)
        {
            if (Random.Shared.Next(3) == 0)
            {
                AmbassadorHasCome = true; 
                return Task.FromResult(Repository.GetItem<Ambassador>().JoinsTheScene(context, this));
            }
        }

        return Task.FromResult(action);
    }

    private string HandleExplosion(IContext context)
    {
        string action = "";

        switch (context.Moves)
        {
            case 10:

                Repository.GetItem<BulkheadDoor>().IsOpen = true;
                action +=
                    $"\n\nA massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. " +
                    $"{(context.CurrentLocation == this ? "The door to port slides open. " : "")}";
                break;

            case 11:
            {
                action = context.CurrentLocation switch
                {
                    _ when context.CurrentLocation is DeckNine =>
                        "\n\nMore distant explosions! A narrow emergency bulkhead at the base of the " +
                        "gangway and a wider one along the corridor to starboard both crash shut! ",
                    _ when context.CurrentLocation is Gangway =>
                        "Another explosion. A narrow bulkhead at the base of the gangway slams shut! ",
                    _ =>
                        "\n\nThe ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. "
                };

                break;
            }

            case 12:
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

            case 13:
                action = "\nExplosions continue to rock the ship. ";
                break;

            case 14:
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

    // TODO: Chance of Ambassador

    // TODO: Chance of "Ensign First Class Blather swaggers in. He studies your work with half-closed eyes. "You call this polishing, Ensign Seventh Class?" he sneers. "We have a position for an Ensign Ninth Class in the toilet-scrubbing division, you know. Thirty demerits." He glares at you, his arms crossed. 
    // TODO: and then: Blather, adding fifty more demerits for good measure, moves off in search of more young ensigns to terrorize.
}