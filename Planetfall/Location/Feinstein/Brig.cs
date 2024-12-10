using GameEngine.Location;
using Model.Movement;
using Planetfall.Item;
using Planetfall.Item.Feinstein;

namespace Planetfall.Location.Feinstein;

internal class Brig : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            {
                Direction.S,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "The cell door is locked. " }
            }
        };

    protected override string ContextBasedDescription =>
        "You are in the Feinstein's brig. Graffiti cover the walls. The cell door to the south is locked. ";

    public override string Name => "Brig";

    public override void Init()
    {
        StartWithItem<Graffiti>();
    }
}