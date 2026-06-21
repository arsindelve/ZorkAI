using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace Planetfall.Item.Kalamontee.Mech;

/// <summary>
///     The door between Reactor Control and the Reactor Elevator. In the original game
///     (compone.zil, REACTOR-ELEVATOR-DOOR / REACTOR-ELEVATOR-DOOR-F) the door starts open and
///     cannot be operated by the player: trying to open it yields "It won't budge." and trying to
///     close it yields "You can't close it yourself." The elevator beyond is a flavor dead-end whose
///     buttons go nowhere.
/// </summary>
public class ReactorElevatorDoor : ItemBase, ICanBeExamined, IOpenAndClose
{
    public override string[] NounsForMatching =>
        ["reactor elevator door", "elevator door", "metal door", "door"];

    // The door is "currently open" per the elevator description, and there is no mechanism in the
    // original game to ever change that state.
    public bool IsOpen { get; set; } = true;

    public bool HasEverBeenOpened { get; set; } = true;

    public string ExaminationDescription => $"The door is {(IsOpen ? "open" : "closed")}. ";

    public string AlreadyOpen => "It is already open. ";

    public string AlreadyClosed => "It is closed. ";

    // Reached only if the door is somehow closed; the player still can't pry it open.
    public string? CannotBeOpenedDescription(IContext context) => "It won't budge. ";

    // The door will never close manually - matches REACTOR-ELEVATOR-DOOR-F.
    public override string? CannotBeClosedDescription(IContext context) => "You can't close it yourself. ";

    public string NowOpen(ILocation currentLocation) => "The door slides open. ";

    public string NowClosed(ILocation currentLocation) => "The door slides shut. ";
}
