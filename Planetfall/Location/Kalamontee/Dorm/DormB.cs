namespace Planetfall.Location.Kalamontee.Dorm;

internal class DormB : DormBase
{
    public override string Name => "Dorm B";

    public override string[] NounsForMatching => ["dormitory", "bedroom", "barracks", "quarters"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<RecCorridor>() },
            { Direction.N, Go<SanfacB>() }
        };
    }
}