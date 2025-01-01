using GameEngine.Location;
using Model.Movement;
using Planetfall.Item;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

public class Balcony : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, Go<Crag>() },
            { Direction.Up, Go<WindingStair>() }
        };

    protected override string GetContextBasedDescription() =>
        "This is an octagonal room, half carved into and half built out from the cliff wall. " +
        "Through the shattered windows which ring the outer wall you can see ocean to the horizon. " +
        "A weathered metal plaque with barely readable lettering rests below the windows. The language " +
        "seems to be a corrupt form of Galalingua. A steep stairway, roughly cut into the face of the " +
        "cliff, leads upward. A rocky crag can be seen about eight meters below. ";

    public override string Name => "Balcony";
    public override void Init()
    {
        StartWithItem<Plaque>();
    }
}