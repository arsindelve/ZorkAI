using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class AuxiliaryBooth : LocationWithNoStartingItems
{
    public override string Name => "Auxiliary Booth";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ComputerRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is another small booth. Unlike the Miniaturization Booth, this room has no slot or keyboard, " +
            "so presumably it is intended only as a receiving station. The exit is on the northern side. ";
    }
}
