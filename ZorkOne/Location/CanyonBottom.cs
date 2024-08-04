using GameEngine.Location;
using Model.Movement;

namespace ZorkOne.Location;

public class CanyonBottom : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.N, new MovementParameters { Location = GetLocation<EndOfRainbow>() }
        },
        {
            Direction.Up, new MovementParameters { Location = GetLocation<RockyLedge>() }
        }
    };

    protected override string ContextBasedDescription =>
        "You are beneath the walls of the river canyon which may be climbable here. The lesser part of the runoff " +
        "of Aragain Falls flows by below. To the north is a narrow path. ";

    public override string Name => "Canyon Bottom";
}