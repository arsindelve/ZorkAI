namespace Planetfall.Item.Lawanda.Lab;

internal abstract class SimpleDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["door"]).ToArray();

    public string ExaminationDescription => $"The {NounsForMatching[0]} door is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public string AlreadyOpen => "It's already open! ";

    public string AlreadyClosed => "It is closed! ";

    public bool HasEverBeenOpened { get; set; }

    public virtual string NowOpen(ILocation currentLocation)
    {
        return "The door opens. ";
    }

    public virtual string NowClosed(ILocation currentLocation)
    {
        return "The door closes. ";
    }

    public virtual string CannotBeOpenedDescription(IContext context)
    {
        return string.Empty;
    }
}