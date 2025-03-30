using GameEngine.Location;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class RadiationLockWest : LocationBase
{
    public override string Name => "Radiation Lock West";

    public override void Init()
    {
        StartWithItem<RadiationLockOuterDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<RadiationLockEast>() },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => GetItem<RadiationLockOuterDoor>().IsOpen, CustomFailureMessage = "The radiation-lock door is closed. ",
                    Location = GetLocation<MainLab>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the western half of a decontamination chamber to prevent dangerous radioactive materials from " +
            "leaving the Radiation Lab which lies to the east. The door leads west to the main lab. ";
    }
}