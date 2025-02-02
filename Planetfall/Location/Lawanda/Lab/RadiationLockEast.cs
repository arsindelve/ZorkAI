using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class RadiationLockEast : LocationBase
{
    public override string Name => "Radiation Lock East";

    public override void Init()
    {
        StartWithItem<RadiationLockDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<RadiationLockEast>() }
        };
    }


    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the eastern half of the decontamination chamber. The door to the east leads to the Radiation Lab, " +
            "and the chamber continues westward. A sign on the wall reads \"WORNEENG! Raadeeaashun suuts must bee worn beeyond xis point.\"";
    }
}