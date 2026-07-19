namespace Planetfall.Item.Kalamontee.Admin;

public class LargeDesk : OpenAndCloseContainerBase
{
    public override string[] NounsForMatching => ["large desk", "desk"];

    // Examine is inherited from OpenAndCloseContainerBase (issue #398): open -> lists the shuttle
    // access card in the drawer; closed -> "The large desk is closed." The old override reported
    // only that the drawer was "currently open" and hid its contents.

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