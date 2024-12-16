namespace Planetfall.Item.Kalamontee;

public class MessDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["door"];

    public string ExaminationDescription => $"The door is {(IsOpen ? "open" : "closed")}.";

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        return "Opened. ";
    }

    public string NowClosed(ILocation currentLocation)
    {
        return "The door is now closed. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"A small door to the north is {(IsOpen ? "open" : "closed")}. ";
    }

    public string AlreadyOpen => "It's already open! ";

    public string AlreadyClosed => "It is closed! ";

    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        var padlock = Repository.GetItem<Padlock>();

        if (padlock.AttachedToDoor)
            return "The door cannot be opened until the padlock is removed. ";

        return string.Empty;
    }
}