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
            { Direction.Up, Go<Gangway>() }
        };

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
                $"\nA massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. " +
                $"{(context.CurrentLocation == this ? "The door to port slides open." : "")}");
        }

        if (context.Moves == 11)
            return Task.FromResult<string>(
                "\nMore distant explosions! A narrow emergency bulkhead at the base of the " +
                "gangway and a wider one along the corridor to starboard both crash shut!");

        if (context.Moves == 12)
        {
            Repository.GetItem<BulkheadDoor>().IsOpen = false;
            return Task.FromResult(
                $"\nMore powerful explosions buffet the ship. The lights flicker madly" +
                $"{(context.CurrentLocation == this ? ", and the escape-pod bulkhead clangs shut" : "")}. ");
        }

        if (context.Moves == 13) return Task.FromResult<string>("\nExplosions continue to rock the ship. ");

        if (context.Moves == 14)
        {
            var result =
                "\nAn enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...";
            return Task.FromResult(new DeathProcessor().Process(result, context).InteractionMessage);
        }

        return Task.FromResult<string>("");
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>();
    }
}