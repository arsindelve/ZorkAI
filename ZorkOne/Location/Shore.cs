using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Shore : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.N, Go<SandyBeach>() },
            { Direction.S, Go<AragainFalls>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are on the east shore of the river. The water here seems somewhat treacherous. A path travels from north " +
        "to south here, the south end quickly turning around a sharp corner. ";

    public override string Name => "Shore";
}