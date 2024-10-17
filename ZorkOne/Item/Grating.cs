using Model.Interface;

namespace ZorkOne.Item;

public class Grating : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["grating"];

    public override string NeverPickedUpDescription => "There is a grating securely fastened into the ground. ";

    public override string InInventoryDescription => NeverPickedUpDescription;

    public string ExaminationDescription => $"The grating is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public bool IsLocked { get; set; } = true;

    public string NowOpen => "";

    public string NowClosed => "";

    public string AlreadyOpen => "";

    public string AlreadyClosed => "";

    public bool HasEverBeenOpened { get; set; }

    public string? CannotBeOpenedDescription(IContext context)
    {
        return IsLocked ? "The grating is locked. " : null;
    }
}