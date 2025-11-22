namespace Planetfall.Item.Feinstein;

public class CellDoor : ItemBase, IOpenAndClose
{
    public override string[] NounsForMatching =>
        ["cell door", "door"];

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        return string.Empty;
    }

    public string NowClosed(ILocation currentLocation)
    {
        return string.Empty;
    }

    public string AlreadyClosed => "It is closed! ";
    public string AlreadyOpen => "It's already open! ";
    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        return "No way, Jose.";
    }

    public string OnClosing(IContext context)
    {
        return string.Empty;
    }
}
