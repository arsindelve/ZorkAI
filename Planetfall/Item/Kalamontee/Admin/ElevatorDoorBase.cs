
namespace Planetfall.Item.Kalamontee.Admin;

public abstract class ElevatorDoorBase : ItemBase, IDoor
{
    // Defined in terms of DescribeAs so examining the door and reading a room description that reports
    // it can never phrase the state differently. See IDoor.
    public string ExaminationDescription => DescribeAs(IsOpen);

    public string DescribeAs(bool isOpen)
    {
        return $"The door is {(isOpen ? "open" : "closed")}. ";
    }

    // Virtual so a landing door can answer for its own room rather than hold a flag of its own; the
    // shaft's one open/closed state stays here, on the door inside the car. See ElevatorLandingDoor.
    public virtual bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        return "The elevator door slides open. ";
    }

    public string NowClosed(ILocation currentLocation)
    {
        return "The elevator door slides shut. After a moment, you feel a sensation of vertical movement. ";
    }

    public string CannotBeOpenedDescription(IContext context)
    {
        return "It won't budge. ";
    }

    public override string CannotBeClosedDescription(IContext context)
    {
        return "The door seems designed to slide shut on its own. ";
    }

    public string AlreadyOpen => "It is open. ";

    public string AlreadyClosed => "It is closed. ";

    public virtual bool HasEverBeenOpened { get; set; }

}
