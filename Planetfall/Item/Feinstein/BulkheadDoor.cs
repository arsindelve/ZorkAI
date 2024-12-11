namespace Planetfall.Item.Feinstein;

public class BulkheadDoor : ItemBase, IOpenAndClose
{
    public override string[] NounsForMatching => ["bulkhead", "bulkhead door", "door", "pod", "pod door", "escape pod door"];
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

    public override string CannotBeClosedDescription(IContext context)
    {
        return "You can't close it yourself. ";
    }

    public string CannotBeOpenedDescription(IContext context)
    {
        return Repository.GetLocation<EscapePod>().TurnsInEscapePod switch
        {
            0 => "Why open the door to the emergency escape pod if there's no emergency? ",
            < 15 => "Opening the door now would be a phenomenally stupid idea. ",
            _ => ""
        };
    }

    public override string OnOpening(IContext context)
    {
        return "The bulkhead opens and cold ocean water rushes in! ";
    }
}