using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class KitchenWindow : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["window"];

    public string ExaminationDescription => IsOpen
        ? "The kitchen window is open, but I can't tell what's beyond it."
        : !HasEverBeenOpened ? "The window is slightly ajar, but not enough to allow entry" : "The kitchen window is closed.";

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation) => "With great effort, you open the window far enough to allow entry.";

    public string NowClosed(ILocation currentLocation) => "The window closes (more easily than it opened).";

    public string AlreadyOpen => "Too late for that";

    public string AlreadyClosed => "Too late for that.";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return null;
    }
}