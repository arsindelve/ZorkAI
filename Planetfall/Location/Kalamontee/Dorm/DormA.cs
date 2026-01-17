namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormA : DormBase
{
    public override string Name => "Dorm A";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<RecCorridor>() },
            { Direction.S, Go<SanfacA>() }
        };
    }
}