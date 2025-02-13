using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

internal class MiniaturizationBooth : LocationWithNoStartingItems
{
    public override string Name => "Miniaturization Booth";

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
            "This is a small room barely large enough for one person. Mounted on the wall is a small slot, " +
            "and next to it a keyboard with numeric keys. The exit is to the north. ";
    }
}