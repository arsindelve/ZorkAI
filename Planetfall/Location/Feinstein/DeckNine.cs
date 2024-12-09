using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Command;
using Planetfall.Item;

namespace Planetfall.Location.Feinstein;

internal class DeckNine : BaseLocation, ITurnBasedActor
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
            }
        };

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client)
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
        if (context.Moves == 10)
        {
            Repository.GetItem<BulkheadDoor>().IsOpen = true;

            return Task.FromResult(
                $"\n\nA massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. " +
                $"{(context.CurrentLocation == this ? "The door to port slides open. " : "")}");
        }

        if (context.Moves == 11)
        {
            return context.CurrentLocation switch
            {
                _ when context.CurrentLocation is DeckNine => Task.FromResult(
                    "\n\nMore distant explosions! A narrow emergency bulkhead at the base of the " +
                    "gangway and a wider one along the corridor to starboard both crash shut! "),
                _ when context.CurrentLocation is Gangway => Task.FromResult(
                    "Another explosion. A narrow bulkhead at the base of the gangway slams shut! "),
                _ => Task.FromResult(
                    "\n\nThe ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing. ")
            };
        }

        if (context.Moves == 12)
        {
            if (context.CurrentLocation is not DeckNine && context.CurrentLocation is not EscapePod)
            {
                var result =
                    "\n\nThe ship rocks from the force of multiple explosions. The lights go out, and you feel a " +
                    "sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...";
                
                return YouExploded(context, result);
            }
            
            Repository.GetItem<BulkheadDoor>().IsOpen = false;
            return Task.FromResult(
                $"\n\nMore powerful explosions buffet the ship. The lights flicker madly" +
                $"{(context.CurrentLocation == this ? ", and the escape-pod bulkhead clangs shut" : "")}. ");
        }

        if (context.Moves == 13) return Task.FromResult<string>("\nExplosions continue to rock the ship. ");

        if (context.Moves == 14)
        {
            var result =
                "\n\nAn enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...";
           
            return YouExploded(context, result);
        }

        return Task.FromResult<string>("");
    }

    private Task<string> YouExploded(IContext context, string result)
    {
        context.RemoveActor(this);
        return Task.FromResult(new DeathProcessor().Process(result, context).InteractionMessage);
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }
    
    // TODO: Chance of Ambassador
    
    // TODO: Chance of "Ensign First Class Blather swaggers in. He studies your work with half-closed eyes. "You call this polishing, Ensign Seventh Class?" he sneers. "We have a position for an Ensign Ninth Class in the toilet-scrubbing division, you know. Thirty demerits." He glares at you, his arms crossed. 
    // TODO: and then: Blather, adding fifty more demerits for good measure, moves off in search of more young ensigns to terrorize.

}