using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using Planetfall.Item;

namespace Planetfall.Location;

public class DeckNine : BaseLocation, ITurnBasedActor
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "This is a featureless corridor similar to every other corridor on the ship. It curves away to starboard," +
        " and a gangway leads up. To port is the entrance to one of the ship's primary escape pods. " +
        "The pod bulkhead is closed. ";

    public override string Name => "Deck Nine";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (context.Moves == 10)
        {
            Repository.GetItem<BulkheadDoor>().IsOpen = true;
            return Task.FromResult(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");
        }

        return Task.FromResult<string>("");
    }

    public override void Init()
    {
        StartWithItem<BulkheadDoor>(this);
    }
}