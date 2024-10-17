using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class GasRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.Up, new MovementParameters { Location = GetLocation<SmellyRoom>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<CoalMineOne>() }
            }
        };

    protected override string ContextBasedDescription =>
        "This is a small room which smells strongly of coal gas. There is a short climb up some stairs " +
        "and a narrow tunnel leading east. ";

    public override string Name => "Gas Room";

    public override void Init()
    {
        StartWithItem<SapphireBracelet>(this);
    }
}