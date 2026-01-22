using GameEngine.Location;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda;

internal class AuxiliaryBooth : LocationWithNoStartingItems
{
    public override string Name => "Auxiliary Booth";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<LabOfficeLocation>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is another small booth. Unlike the Miniaturization Booth, this room has no slot or keyboard, " +
            "so presumably it is intended only as a receiving station. An office lies to the west. ";
    }
}
