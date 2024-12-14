using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee;

internal class RecCorridor : LocationWithNoStartingItems
{

    protected override Dictionary<Direction, MovementParameters> Map =>
    new()
    {
            { Direction.SW, Go<PlainHall>() },
            { Direction.N, Go<DormB>() }
    };

    protected override string ContextBasedDescription =>
       "This is a wide, east-west hallway. Portals lead north and south, and another corridor branches southwest. ";

    public override string Name => "Rec Corridor";

}

internal class PlainHall : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<Courtyard>() },
             { Direction.NE, Go<RecCorridor>() }
        };

    protected override string ContextBasedDescription =>
        "This is a featureless hall leading north and south. Although the hallway is old and dusty, " +
        "the construction is of a much more modern style than the castle to the south. A similar " +
        "hall branches off to the northeast. ";

    public override string Name => "Plain Hall";
}

internal class RecArea : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<PlainHall>() }
        };

    // TODO: >examine games
    // All the usual games -- Chess, Cribbage, Galactic Overlord, Double Fannucci...

    // TODO: >examine tapes
    // Let's see...here are some musical selections, here are some bestselling romantic novels, here is a biography of a famous Double Fannucci champion...

    protected override string ContextBasedDescription =>
        "This is a recreational facility of some sort. Games and tapes are scattered about the room. " +
        "Hallways head off to the east and south, and to the north is a door which is closed and locked. " +
        "A dial on the door is currently set to 0. ";



    public override string Name => "Rec Area";
}