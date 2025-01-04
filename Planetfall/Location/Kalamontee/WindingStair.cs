using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class WindingStair : LocationWithNoStartingItems
{
    public override string Name => "Winding Stair";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, ((PlanetfallContext)context).Day < 4 ? Go<Balcony>() : Go<Underwater>() },
            { Direction.Up, Go<Courtyard>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "The middle of a long, steep stairway carved into the face of a cliff. " +
               DayChange(context as PlanetfallContext);
    }

    private string DayChange(PlanetfallContext? context)
    {
        return context?.Day switch
        {
            4 => "You hear the lapping of water from below. ",
            >= 5 => "You can see ocean water splashing against the steps below you. ",
            _ => ""
        };
    }
}