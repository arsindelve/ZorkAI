namespace Planetfall.Item.Lawanda.LabOffice;

public class LabDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["desk", "lab desk", "drawer"];

    public override int Size => 3;

    public string ExaminationDescription
    {
        get
        {
            var memoTaken = Repository.GetItem<Memo>().HasEverBeenPickedUp;

            if (memoTaken)
            {
                if (IsOpen)
                    return Items.Any() ? ItemListDescription("desk", null) : "The desk is open. ";
                return "The desk is closed. ";
            }

            return IsOpen
                ? ItemListDescription("desk", null)
                : "After inspecting the various papers on the desk, you find only one item of interest, a memo of some sort. " +
                  "The desk itself is closed, but it doesn't look locked. ";
        }
    }

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<GasMask>();
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? $"Opening the desk reveals {SingleLineListOfItems()}. " : "Opened. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (IsOpen)
            return Items.Any() ? $"\n{ItemListDescription("desk", null)}" : "";

        return base.GenericDescription(currentLocation);
    }
}