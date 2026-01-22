namespace Planetfall.Item.Lawanda.Lab;

internal class OfficeDoor : SimpleDoor
{
    public override string[] NounsForMatching => ["office door", "door"];

    public override string[] NounsForPreciseMatching => ["office door", "office"];
}
