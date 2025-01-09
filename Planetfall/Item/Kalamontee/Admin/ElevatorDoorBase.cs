
namespace Planetfall.Item.Kalamontee.Admin;

public abstract class ElevatorDoorBase : ItemBase, ICanBeExamined, IOpenAndClose
{
    public string ExaminationDescription => $"The door is {(IsOpen ? "open" : "closed")}. ";
    
    public bool IsOpen { get; set; }

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

    public bool HasEverBeenOpened { get; set; }

}