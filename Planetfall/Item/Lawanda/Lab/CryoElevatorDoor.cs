namespace Planetfall.Item.Lawanda.Lab;

internal class CryoElevatorDoor : SimpleDoor
{
    public override string[] NounsForMatching => ["cryo-elevator door", "elevator door", "door"];

    public override string[] NounsForPreciseMatching => ["cryo-elevator door", "elevator door", "cryo-elevator", "elevator"];

    [UsedImplicitly] public bool IsVisible { get; set; }
}
