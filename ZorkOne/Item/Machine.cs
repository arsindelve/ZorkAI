namespace ZorkOne.Item;

public class Machine : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["machine", "dryer"];

    public string ExaminationDescription => IsOpen
        ? Items.Any() ? base.ItemListDescription("machine") : "The machine is empty. "
        : "The machine is closed. ";

    protected override int SpaceForItems => 6;

    public string OnTheGroundDescription => string.Empty;

    public override void Init()
    {
        // Initially empty. 
    }

    public override string CannotBeTakenDescription => "It is far too large to carry. ";
}