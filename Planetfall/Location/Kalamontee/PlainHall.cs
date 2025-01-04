using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

internal class PlainHall : LocationWithNoStartingItems
{
    public override string Name => "Plain Hall";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<Courtyard>() },
            { Direction.N, Go<RecArea>() },
            { Direction.NE, Go<RecCorridor>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a featureless hall leading north and south. Although the hallway is old and dusty, " +
               "the construction is of a much more modern style than the castle to the south. A similar " +
               "hall branches off to the northeast. ";
    }
}