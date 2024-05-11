using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class CoalMineFour : CoalMine
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.W, new MovementParameters { Location = GetLocation<CoalMineFour>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<CoalMineThree>() }
        }
    };
}