using Model.Location;
using Planetfall.Location.Feinstein;

namespace Planetfall.Item.Feinstein;

public class BulkheadDoor : ItemBase, IOpenAndClose
{
    public override string[] NounsForMatching => ["bulkhead", "bulkhead door", "door"];
    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        throw new NotImplementedException();
    }

    public string NowClosed(ILocation currentLocation)
    {
        throw new NotImplementedException();
    }

    public string AlreadyClosed => "It is closed! ";
    public string AlreadyOpen => "It's already open! ";
    public bool HasEverBeenOpened { get; set; }

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