using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class BioLockWest : LocationBase
{
    public override string Name => "Bio Lock West";

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the first half of a sterilization chamber to prevent contamination of the delicate biological " +
            "experiments in the Bio Lab which lies beyond. The door to the west leads to the main lab, and the bio lock continues eastward. ";
    }

    public override void Init()
    {
        StartWithItem<BioLockDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<BioLockEast>() },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => GetItem<BioLockDoor>().IsOpen, CustomFailureMessage = "The bio-lock door is closed. ",
                    Location = GetLocation<MainLab>()
                }
            }
        };
    }
}