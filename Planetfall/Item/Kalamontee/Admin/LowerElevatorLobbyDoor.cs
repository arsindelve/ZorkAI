using GameEngine;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
///     The red door as it stands in the Elevator Lobby.
/// </summary>
public class LowerElevatorLobbyDoor : ElevatorLandingDoor
{
    public override string[] NounsForMatching => ["red door", "door", "elevator door", "lower elevator door"];

    protected override ElevatorDoorBase ShaftDoor => Repository.GetItem<LowerElevatorDoor>();

    protected override bool IsOpenHere => Repository.GetLocation<LowerElevator>().IsOpenAtTheLobby;
}
