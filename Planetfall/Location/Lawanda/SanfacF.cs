using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class SanfacF : LocationWithNoStartingItems
{
    public override string Name => "Sanfac F";

    public override string[] NounsForMatching => ["bathroom", "restroom", "washroom", "toilet"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<ProjectCorridorWest>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is another dusty sanitary facility. Unlike the ones near the dorms, this one is smaller and has no bathing fixtures. ";
    }
}