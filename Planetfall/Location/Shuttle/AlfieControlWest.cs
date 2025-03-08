using Model.Movement;

namespace Planetfall.Location.Shuttle;

public class AlfieControlWest : ShuttleControl
{
    public override string Name => "Alfie Control West";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<ShuttleCarAlfie>() }
        };
    }

    public override ILocation Cabin => Repository.GetLocation<ShuttleCarAlfie>();
}