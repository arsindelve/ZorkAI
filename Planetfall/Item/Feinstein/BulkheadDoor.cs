using Planetfall.Location.Feinstein;

namespace Planetfall.Item.Feinstein;

public class BulkheadDoor : ItemBase, IOpenAndClose
{
    public override string[] NounsForMatching =>
        ["bulkhead", "bulkhead door", "door", "pod", "pod door", "escape pod door", "narrow emergency bulkhead"];

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
        var escapePod = Repository.GetLocation<EscapePod>();
        var turnsSinceExplosion = escapePod.TurnsSinceExplosion;

        // Before the explosion - no emergency yet
        if (turnsSinceExplosion == 0)
            return "Why open the door to the emergency escape pod if there's no emergency? ";

        // After turn 2, the pod door closes and launch begins. If player is in Deck Nine,
        // they missed their chance to board.
        if (turnsSinceExplosion >= 2 && context.CurrentLocation is DeckNine)
            return "Too late. The pod's launching procedure has already begun. ";

        // Player is in the pod during launch - opening would be stupid
        if (turnsSinceExplosion < 14)
            return "Opening the door now would be a phenomenally stupid idea. ";

        // After landing, can be opened
        return "";
    }

    public override string OnOpening(IContext context)
    {
        return "The bulkhead opens and cold ocean water rushes in! ";
    }
}