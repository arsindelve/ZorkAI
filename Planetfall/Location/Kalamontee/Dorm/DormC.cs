namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormC : DormBase
{
    public override string Name => "Dorm C";

    public override string[] NounsForMatching => ["dormitory", "bedroom", "barracks", "quarters"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<DormCorridor>() },
            { Direction.S, Go<SanfacC>() }
        };
    }
}