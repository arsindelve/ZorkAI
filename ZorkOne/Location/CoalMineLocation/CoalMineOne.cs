using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class CoalMineOne : CoalMine
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.N, new MovementParameters { Location = GetLocation<GasRoom>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<CoalMineOne>() }
            },
            {
                Direction.NE, new MovementParameters { Location = GetLocation<CoalMineTwo>() }
            }
        };
    }
}