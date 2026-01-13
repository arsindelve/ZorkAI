namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormD : DormBase
{
    public override string Name => "Dorm D";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<DormCorridor>() },
            { Direction.N, Go<SanfacD>() }
        };
    }
}