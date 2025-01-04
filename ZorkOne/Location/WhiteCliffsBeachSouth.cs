using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class WhiteCliffsBeachSouth : LocationWithNoStartingItems
{
    public override string Name => "White Cliffs Beach";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = GetLocation<WhiteCliffsBeachNorth>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are on a rocky, narrow strip of beach beside the Cliffs. A narrow path leads north along the shore. ";
    }
}