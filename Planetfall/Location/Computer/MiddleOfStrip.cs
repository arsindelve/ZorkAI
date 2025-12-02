using GameEngine.Location;

namespace Planetfall.Location.Computer;

internal class MiddleOfStrip : LocationWithNoStartingItems
{
    public override string Name => "Middle of Strip";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<StripNearStation>() },
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

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are standing on a section of the strip with a bottomless void stretching out on both sides. The strip continues to the north and south. ";
    }
}