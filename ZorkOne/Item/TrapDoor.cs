using Model.Interface;

namespace ZorkOne.Item;

public class TrapDoor : ItemBase, ICanBeExamined, IOpenAndClose
{
    public override string[] NounsForMatching => ["door", "trap door", "trapdoor"];

    public string ExaminationDescription =>
        IsOpen ? "The trap door is open, but I can't tell what's beyond it." : "The trap door is closed.";

    public bool IsOpen { get; set; }

    public string NowOpen => "The door reluctantly opens to reveal a rickety staircase descending into darkness.";

    public string NowClosed => "The door swings shut and closes.";

    public string AlreadyOpen => "Too late for that.";

    public string AlreadyClosed => "Get your eyes checked.";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return context.CurrentLocation == Repository.GetLocation<Cellar>()
            ? "The door is locked from above. "
            : null;
    }
}