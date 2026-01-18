using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace EscapeRoom.Location;

public class Office : LocationBase
{
    public override string Name => "Office";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "A tidy office with a wooden desk and a comfortable chair. " +
               "A motivational poster on the wall says 'You Can Do It!'. " +
               "The only exit is east to the reception area.";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, new MovementParameters { Location = GetLocation<Reception>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<WoodenDesk>();
    }
}
