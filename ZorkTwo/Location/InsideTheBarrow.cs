using Game.Location;
using Model;
using Model.Movement;

namespace ZorkTwo.Location;

public class InsideTheBarrow : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map { get; } = new();

    public override string Name => "Inside the Barrow";

    protected override string ContextBasedDescription =>
        "You are inside an ancient barrow hidden deep within a dark forest. " +
        "The barrow opens into a narrow tunnel at its southern end. You can see " +
        "a faint glow at the far end. ";

    public override void Init()
    {
    }
}