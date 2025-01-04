using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Feinstein;

internal class Brig : LocationBase
{
    public override string Name => "Brig";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.S,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The cell door is locked. " }
            }
        };
    }

    // TODO: Cell door. 

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in the Feinstein's brig. Graffiti cover the walls. The cell door to the south is locked. ";
    }

    public override void Init()
    {
        StartWithItem<Graffiti>();
    }
}