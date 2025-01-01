using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location.ForestLocation;

public class StoneBarrow : LocationWithNoStartingItems
{
    public override string Name => "Stone Barrow";

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.NE, Go<WestOfHouse>()
            },
            {
                Direction.W, Go<InsideTheBarrow>()
            }
        };

    protected override string GetContextBasedDescription() =>
        "You are standing in front of a massive barrow of stone. In the east face is a huge stone door which is open. You cannot see into the dark of the tomb. ";
}