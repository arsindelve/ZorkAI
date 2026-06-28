using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Computer;

namespace Planetfall.Location.Computer;

internal class MiddleOfStrip : LocationWithNoStartingItems
{
    public override string Name => "Middle of Strip";

    private const string MicrobeBlocksMessage =
        "Not a chance -- unless, of course, you don't mind walking into the gullet of a hungry microbe. ";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.S,
                Repository.GetItem<Microbe>().IsActive
                    ? new MovementParameters { CanGo = _ => false, CustomFailureMessage = MicrobeBlocksMessage }
                    : Go<StripNearStation>()
            },
            { Direction.N, Go<StripNearRelay>() },
            {
                Direction.E,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "Do you have a penchant for bottomless voids?" }

            },
            {
                Direction.W,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "Do you have a penchant for bottomless voids?" }
            }

        };
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        // The microbe attacks the moment you set foot on the strip with the computer fixed
        // (MIDDLE-OF-STRIP-F M-ENTER, comptwo.zil:2478).
        var relay = Repository.GetItem<Relay>();
        var microbe = Repository.GetItem<Microbe>();

        if (relay.SpeckDestroyed)
        {
            var spawnText = microbe.SpawnOnStrip(context, this);
            if (spawnText is not null)
                return Task.FromResult(spawnText);
        }

        // The microbe trails you back into this room if it was chasing you elsewhere on the strip.
        if (microbe.IsActive && microbe.CurrentLocation != this)
        {
            var followText = microbe.FollowInto(this);
            if (followText is not null)
                return Task.FromResult(followText);
        }

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are standing on a section of the strip with a bottomless void stretching out on both sides. The strip continues to the north and south. ";
    }
}