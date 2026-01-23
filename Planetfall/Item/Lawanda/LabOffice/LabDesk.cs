namespace Planetfall.Item.Lawanda.LabOffice;

public class LabDesk : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["desk", "lab desk", "drawer"];

    public override int Size => 20;

    public string ExaminationDescription
    {
        get
        {
            var memoTaken = Repository.GetItem<Memo>().HasEverBeenPickedUp;

            if (memoTaken)
                return IsOpen ? "The desk is open. " : "The desk is closed. ";

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
}
