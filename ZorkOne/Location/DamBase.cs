using Model.Movement;

namespace ZorkOne.Location;

public class DamBase : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<Dam>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<Dam>() } }
        };

    protected override string ContextBasedDescription =>
        "You are at the base of Flood Control Dam #3, which looms above you and to the north. The river " +
        "Frigid is flowing by here. Along the river are the White Cliffs which seem to form giant walls stretching" +
        " from north to south along the shores of the river as it winds its way downstream. ";

    public override string Name => "Dam Base";

    public override void Init()
    {
        StartWithItem<PileOfPlastic>(this);
    }
}