namespace Planetfall.Item.Kalamontee.Admin;

public class SmallDesk : OpenAndCloseContainerBase
{
    public override string[] NounsForMatching => ["small desk", "desk"];

    // Examine is inherited from OpenAndCloseContainerBase (issue #398): open -> lists the access
    // cards in the drawer; closed -> "The small desk is closed." The old override reported only
    // that the drawer was "currently open" and hid its contents.

    public override void Init()
    {
        StartWithItemInside<KitchenAccessCard>();
        StartWithItemInside<UpperElevatorAccessCard>();
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? $"Opening the small desk reveals {SingleLineListOfItems()}. " : "Opened. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (IsOpen)
            return Items.Any() ? $"\n{ItemListDescription("small desk", null)}" : "";

        return base.GenericDescription(currentLocation);
    }
}