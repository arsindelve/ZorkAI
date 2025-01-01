using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class CoalMineTwo : CoalMine
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            {
                Direction.N, new MovementParameters { Location = GetLocation<CoalMineTwo>() }
            },
            {
                Direction.S, new MovementParameters { Location = GetLocation<CoalMineOne>() }
            },
            {
                Direction.SE, new MovementParameters { Location = GetLocation<CoalMineThree>() }
            }
        };
}