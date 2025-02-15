using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class SandyBeach : DarkLocation // Explain to me how a beach can be a dark location? Well, it is. 
{
    public override string Name => "Sandy Beach";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NE, Go<SandyCave>() },
            { Direction.S, Go<Shore>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on a large sandy beach on the east shore of the river, which is flowing quickly by. " +
               "A path runs beside the river to the south here, and a passage is partially buried in sand to the northeast. ";
    }

    public override void Init()
    {
        StartWithItem<Shovel>();
    }
}