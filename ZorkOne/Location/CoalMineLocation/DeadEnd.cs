using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class DeadEnd : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.N, new MovementParameters { Location = GetLocation<LadderBottom>() }
            }
        };

    protected override string ContextBasedDescription =>
        "You have come to a dead end in the mine.  ";

    public override string Name => "Dead End";

    public override void Init()
    {
        StartWithItem<Coal>(this);
    }
}