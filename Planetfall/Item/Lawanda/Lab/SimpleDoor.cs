namespace Planetfall.Item.Lawanda.Lab;

internal abstract class SimpleDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["door"]).ToArray();

    public string ExaminationDescription => $"The {DisplayName} is {(IsOpen ? "open" : "closed")}. ";

    // Issue #404: some lock doors already carry "door" in their primary noun (e.g. "radiation-lock
    // door", "bio-lock door", or the bare "door"), so unconditionally appending " door" doubled it
    // ("radiation-lock door door"). Only append when the noun is a bare descriptor (e.g. "cell"),
    // so those still read "The cell door is …". The original prints the door's DESC verbatim.
    private string DisplayName =>
        NounsForMatching[0].Equals("door", StringComparison.OrdinalIgnoreCase)
        || NounsForMatching[0].EndsWith(" door", StringComparison.OrdinalIgnoreCase)
            ? NounsForMatching[0]
            : $"{NounsForMatching[0]} door";

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

    public virtual string OnClosing(IContext context)
    {
        return string.Empty;
    }
}