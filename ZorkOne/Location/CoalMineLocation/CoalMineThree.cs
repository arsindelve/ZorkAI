using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class CoalMineThree : CoalMine
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.E, new MovementParameters { Location = GetLocation<CoalMineTwo>() }
        },
        {
            Direction.S, new MovementParameters { Location = GetLocation<CoalMineThree>() }
        },
        {
            Direction.SW, new MovementParameters { Location = GetLocation<CoalMineFour>() }
        }
    };
}