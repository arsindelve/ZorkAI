namespace ZorkOne.Item;

public class Grating : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["grating"];

    public string ExaminationDescription => $"The grating is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public string NowOpen => throw new NotImplementedException();

    public string NowClosed => throw new NotImplementedException();

    public string AlreadyOpen => throw new NotImplementedException();

    public string AlreadyClosed => throw new NotImplementedException();

    public override string NeverPickedUpDescription => "There is a grating securely fastened into the ground. ";

    public override string InInventoryDescription => NeverPickedUpDescription;

    public bool HasEverBeenOpened { get; set; }

    public virtual string? CannotBeOpenedDescription(IContext context)
    {
        return "The grating is locked. ";
    }
}