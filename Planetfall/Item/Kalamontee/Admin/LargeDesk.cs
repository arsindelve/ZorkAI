namespace Planetfall.Item.Kalamontee.Admin;

public class LargeDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["lsrge desk", "desk"];

    public string ExaminationDescription =>
        $"The desk has a drawer which is currently {(IsOpen ? "open" : "closed")}. ";

    public override void Init()
    {
        StartWithItemInside<ShuttleAccessCard>();
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? $"Opening the large desk reveals {SingleLineListOfItems()}. " : "Opened. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (IsOpen)
            return Items.Any() ? $"\n{ItemListDescription("large desk", null)}" : "";

        return base.GenericDescription(currentLocation);
    }
}