using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class MainLab : LocationBase
{
    public override string Name => "Main Lab";

    public override void Init()
    {
        StartWithItem<BioLockDoor>();
        StartWithItem<RadiationLockDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<ProjectCorridorEast>() },
            { Direction.SW, Go<ComputerRoom>() },
            { Direction.S, Go<LabStorage>() },
            {
                Direction.SE,
                new MovementParameters
                {
                    CanGo = _ => GetItem<BioLockDoor>().IsOpen, 
                    CustomFailureMessage = "The bio-lock door is closed. ",
                    Location = GetLocation<BioLockWest>()
                }
            },
            {
                Direction.NE,
                new MovementParameters
                {
                    CanGo = _ => GetItem<RadiationLockDoor>().IsOpen,
                    CustomFailureMessage = "The radiation-lock door is closed. ",
                    Location = GetLocation<RadiationLockWest>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the heart of the Project's vast laboratory complex. There are exits to the west and southwest, " +
            "and heavy metal doors to the northeast and southeast. A small doorway leads south. ";
    }
}