
namespace Planetfall.Item.Kalamontee.Admin;

public abstract class ElevatorDoorBase : ItemBase, ICanBeExamined, IOpenAndClose
{
    public string ExaminationDescription => DescribeAs(IsOpen);

    /// <summary>
    ///     The door's own wording for a given state, so a caller that knows better than the raw flag can
    ///     reuse it instead of copying the literal. The Elevator Lobby needs this: the flag is shared by
    ///     both ends of the shaft, so from there the answer depends on which end the car is at (#505).
    /// </summary>
    public string DescribeAs(bool isOpen)
    {
        return $"The door is {(isOpen ? "open" : "closed")}. ";
    }
    
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