using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class AlfieControlEast : ShuttleControl
{
    public override string Name => "Alfie Control East";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<ShuttleCarAlfie>() }
        };
    }

    public override ILocation Cabin => Repository.GetLocation<ShuttleCarAlfie>();
}