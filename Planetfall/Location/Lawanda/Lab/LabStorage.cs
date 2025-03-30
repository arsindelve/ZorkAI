using GameEngine.Location;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class LabStorage : LocationBase
{
    public override string Name => "Lab Storage";

    public override void Init()
    {
        StartWithItem<FreshBattery>();
        StartWithItem<LabUniform>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<MainLab>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a tiny room for the storage of laboratory supplies. The sole exit is to the north. ";
    }
}