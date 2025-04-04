using GameEngine.Location;
using Planetfall.Item.Lawanda;

namespace Planetfall.Location.Lawanda;

internal class CourseControl : LocationBase
{
    public override string Name => "Course Control";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorEast>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a long room whose walls are covered with complicated controls and colored lights. " +
            "One blinking light says \"Kors diivurjins minimiizeeng.\" ";
    }
    
    public override void Init()
    {
        StartWithItem<Pliers>();
        StartWithItem<NinetyOhmBedistor>();
        StartWithItem<LargeMetalCube>();
    }
}
