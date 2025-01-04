using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class CanyonBottom : LocationWithNoStartingItems
{
    public override string Name => "Canyon Bottom";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.N, new MovementParameters { Location = GetLocation<EndOfRainbow>() }
            },
            {
                Direction.Up, new MovementParameters { Location = GetLocation<RockyLedge>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are beneath the walls of the river canyon which may be climbable here. The lesser part of the runoff " +
            "of Aragain Falls flows by below. To the north is a narrow path. ";
    }
}