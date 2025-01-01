using GameEngine.Location;
using Model;
using Model.Interface;
using Model.Movement;

namespace ZorkTwo.Location;

public class InsideTheBarrow : LocationBase
{
    private readonly Dictionary<Direction, MovementParameters> _map = new();

    protected override Dictionary<Direction, MovementParameters> Map(IContext context) => _map;

    public override string Name => "Inside the Barrow";

    protected override string GetContextBasedDescription(IContext context) =>
        "You are inside an ancient barrow hidden deep within a dark forest. " +
        "The barrow opens into a narrow tunnel at its southern end. You can see " +
        "a faint glow at the far end. ";

    public override void Init()
    {
    }
}