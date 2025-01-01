using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class WhiteCliffsBeachSouth : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<WhiteCliffsBeachNorth>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are on a rocky, narrow strip of beach beside the Cliffs. A narrow path leads north along the shore. ";

    public override string Name => "White Cliffs Beach";
}