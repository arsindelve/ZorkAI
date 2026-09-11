using GameEngine;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
///     The lower shaft's door at its far end — "to the south is a metal door" in the Waiting Area,
///     which named a door the player could not touch until this object existed (issue #532).
/// </summary>
public class LowerElevatorWaitingAreaDoor : ElevatorLandingDoor
{
    public override string[] NounsForMatching => ["metal door", "door", "elevator door", "red door"];

    protected override ElevatorDoorBase ShaftDoor => Repository.GetItem<LowerElevatorDoor>();

    protected override bool IsOpenHere => Repository.GetLocation<LowerElevator>().IsOpenAtTheFarEnd;
}
