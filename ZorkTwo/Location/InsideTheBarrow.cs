using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkTwo.Location;

public class InsideTheBarrow : LocationBase
{
    private readonly Dictionary<Direction, MovementParameters> _map = new();

    public override string Name => "Inside the Barrow";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return _map;
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are inside an ancient barrow hidden deep within a dark forest. " +
               "The barrow opens into a narrow tunnel at its southern end. You can see " +
               "a faint glow at the far end. ";
    }

    public override void Init()
    {
    }
}