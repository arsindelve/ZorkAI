using GameEngine.Location;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location.Computer;

internal class Station384 : LocationWithNoStartingItems
{
    public override string Name => "Station 384";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<StripNearStation>() },
            {
                Direction.W,
                new MovementParameters
                {
                    Location = Repository.GetLocation<MiniaturizationBooth>(),
                    TransitionMessage =
                        "You feel the familiar wrenching of your innards, and find yourself in a vast room whose distant walls are rushing straight toward you..." + Environment.NewLine
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are standing on a square plate of heavy metal. Above your head, parallel to the plate beneath you, " +
            "is an identical metal plate. To the east is a wide, metallic strip. ";
    }
}