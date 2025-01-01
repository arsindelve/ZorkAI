using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

public class WhiteCliffsBeachNorth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<DampCave>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<WhiteCliffsBeachSouth>() } },
        };

    protected override string GetContextBasedDescription() =>
        "You are on a narrow strip of beach which runs along the base of the White Cliffs. There is a narrow " +
        "path heading south along the Cliffs and a tight passage leading west into the cliffs themselves. ";

    public override string Name => "White Cliffs Beach";
}