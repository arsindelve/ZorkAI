﻿namespace Planetfall.Item.Kalamontee.Admin;

public class SmallDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["small desk", "desk"];

    public string ExaminationDescription =>
        $"The desk has a drawer which is currently {(IsOpen ? "open" : "closed")}. ";

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