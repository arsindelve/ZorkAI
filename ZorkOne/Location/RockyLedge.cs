using Game.Location;
using Model;

namespace ZorkOne.Location;

public class RockyLedge : BaseLocation
{
    protected override string Name => "Rocky Ledge";

    protected override string ContextBasedDescription =>
        "You are on a ledge about halfway up the wall of the river canyon. You can see from here that " +
        "the main flow from Aragain Falls twists along a passage which it is impossible for you to enter. " +
        "Below you is the canyon bottom. Above you is more cliff, which appears climbable.";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.Up, new MovementParameters { Location = GetLocation<CanyonView>() }
        }
    };
}