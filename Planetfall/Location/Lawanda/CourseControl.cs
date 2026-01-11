using GameEngine.Location;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Location.Lawanda;

internal class CourseControl : LocationBase
{
    public override string Name => "Course Control";
    
    [UsedImplicitly]
    public bool Fixed { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridorEast>() }
        };
    }

    internal void ItIsFixed(IContext context)
    {
        Fixed = true;
        Repository.GetLocation<SystemsMonitors>().MarkCourseControlFixed();
        context.AddPoints(6);
    }
    
    protected override string GetContextBasedDescription(IContext context)
    {
        if (Fixed)
            return """
                   This is a long room whose walls are covered with complicated controls and colored lights. One blinking light says "Kors diivurjins minimiizeeng.
                   """;
            
        return
            """
            This is a long room whose walls are covered with complicated controls and colored lights. Two of these lights are blinking. The first one reads "Bedistur Faalyur!" The other light reads "Kritikul diivurjins frum pland kors." 
            """;
    }
    
    public override void Init()
    {
        StartWithItem<LargeMetalCube>();
    }
}
