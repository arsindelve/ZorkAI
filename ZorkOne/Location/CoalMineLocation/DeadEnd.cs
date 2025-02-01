using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class DeadEnd : DarkLocation, IThiefMayVisit
{
    public override string Name => "Dead End";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.N, new MovementParameters { Location = GetLocation<LadderBottom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You have come to a dead end in the mine.  ";
    }

    public override void Init()
    {
        StartWithItem<Coal>();
    }
}