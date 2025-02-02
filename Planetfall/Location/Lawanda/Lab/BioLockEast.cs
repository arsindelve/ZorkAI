using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockEast : LocationBase
{
    public override string Name => "Bio Lock East";

    public override void Init()
    {
        throw new NotImplementedException();
    }

    // Opening the door reveals a Bio-Lab full of horrible mutations. You stare at them, frozen with horror. Growling with hunger
    // and delight, the mutations march into the bio-lock and devour you.
    
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