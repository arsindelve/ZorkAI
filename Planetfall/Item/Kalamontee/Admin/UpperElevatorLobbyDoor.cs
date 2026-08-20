using GameEngine;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
///     The blue door as it stands in the Elevator Lobby.
/// </summary>
public class UpperElevatorLobbyDoor : ElevatorLandingDoor
{
    public override string[] NounsForMatching => ["blue door", "door", "elevator door", "upper elevator door"];

    protected override ElevatorDoorBase ShaftDoor => Repository.GetItem<UpperElevatorDoor>();

    protected override bool IsOpenHere => Repository.GetLocation<UpperElevator>().IsOpenAtTheLobby;
}
