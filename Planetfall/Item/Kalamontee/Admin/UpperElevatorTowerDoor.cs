using GameEngine;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
///     The upper shaft's door at its far end — "a sliding door leads north" in the Tower Core, which
///     named a door the player could not touch until this object existed (issue #532).
/// </summary>
public class UpperElevatorTowerDoor : ElevatorLandingDoor
{
    public override string[] NounsForMatching => ["sliding door", "door", "elevator door", "blue door"];

    protected override ElevatorDoorBase ShaftDoor => Repository.GetItem<UpperElevatorDoor>();

    protected override bool IsOpenHere => Repository.GetLocation<UpperElevator>().IsOpenAtTheFarEnd;
}
