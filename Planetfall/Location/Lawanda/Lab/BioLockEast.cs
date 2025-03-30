using GameEngine.Location;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockEast : LocationBase
{
    public override string Name => "Bio Lock East";

    public override void Init()
    {
        StartWithItem<BioLockInnerDoor>();
    }
    
    //>look through window
    // You can see a large laboratory, dimly illuminated. A blue glow comes from a crack in the northern wall of the lab. Shadowy,
    // ominous shapes move about within the room. On the floor, just inside the door, you can see a magnetic-striped card.
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<BioLockWest>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "The is the second half of the sterilization chamber leading from the main lab to the Bio Lab. The door " +
            "to the east, leading to the Bio Lab, has a window. The bio lock continues to the west. ";
    }
}