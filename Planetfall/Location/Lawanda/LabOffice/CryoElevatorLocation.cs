using GameEngine.Location;
using Planetfall.Item.Lawanda.CryoElevator;

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


/*>press button
  Stunning. After days of surviving on a hostile, plague-ridden planet, solving several of Infocom's toughest puzzles, and coming
  within one move of completing Planetfall, you blow it all in one amazingly dumb input.

  The doors close and the elevator rises quickly to the top of the shaft. The doors open, and the mutants, which were waiting
  impatiently in the ProjCon Office for just such an occurence, happily saunter in and begin munching.
*/