using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

public class Courtyard : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.Down, Go<WindingStair>() },
            { Direction.S, Go<WindingStair>() },
            { Direction.W, Go<WestWing>() },
            { Direction.N, Go<PlainHall>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are in the courtyard of an ancient stone edifice, vaguely reminiscent of the castles you saw " +
        "during your leave on Ramos Two. It has decayed to the point where it can probably be termed a ruin. " +
        "Openings lead north and west, and a stairway downward is visible to the south. " + DayChange(context as PlanetfallContext);

    private string DayChange(PlanetfallContext? context)
    {
        return context?.Day switch
        {
            > 4 => "From the direction of the stairway comes the sound of ocean surf. ",
            _ => ""
        };
    }
    
    public override string Name => "Courtyard";
}