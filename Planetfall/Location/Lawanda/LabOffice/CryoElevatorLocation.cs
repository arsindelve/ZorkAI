using GameEngine.Location;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class CryoElevatorLocation : LocationBase
{
    public override string Name => "Cryo-Elevator";

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are in a small elevator with metal walls. A single button is mounted on the wall. " +
            "Heavy doors seal the entrance. ";
    }

    public override void Init()
    {
        StartWithItem<CryoElevatorButton>();
        StartWithItem<CryoElevatorDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var button = Repository.GetItem<CryoElevatorButton>();

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => !button.CountdownActive,
                    CustomFailureMessage = "The elevator doors are sealed shut during descent. ",
                    Location = GetLocation<ProjConOffice>()
                }
            }
        };
    }
}
