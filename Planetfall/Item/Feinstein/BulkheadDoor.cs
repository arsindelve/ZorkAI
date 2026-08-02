using Planetfall.Location.Feinstein;

namespace Planetfall.Item.Feinstein;

public class BulkheadDoor : ItemBase, IOpenAndClose
{
    /// <summary>
    ///     The original states this as two objects that share one command: POD-DOOR
    ///     (planetfall-source/globals.zil:1094 — synonyms DOOR/BULKHEAD, adjectives EMERGENCY/ESCAPE/POD)
    ///     and GLOBAL-POD (globals.zil:904 — synonym POD, adjectives EMERGENCY/ESCAPE/PRIMARY), whose
    ///     action routine sends OPEN straight to POD-DOOR (globals.zil:925). This port merges them into
    ///     one item, so it has to carry the union of both name sets.
    /// </summary>
    /// <remarks>
    ///     Every adjective+noun pair must be spelled out. The AI parser hands back either the whole
    ///     phrase as one noun or an adjective/noun split, and SimpleIntent.MatchNounAndAdjective simply
    ///     compares "adjective noun" against this list — there is no containment fallback on this path.
    ///     Leaving "escape pod" and "pod bulkhead" out, the exact two phrases Deck Nine's own description
    ///     teaches, meant "open escape pod" resolved to nothing and the narrator mocked the player for
    ///     naming an object that wasn't there.
    ///     <para>
    ///         Trap when adding to this list: the LONGEST entry doubles as the item's display name
    ///         (ExamineInteractionProcessor prints "nothing special about the {longest noun}"), so a new
    ///         long synonym silently renames the door. "narrow emergency bulkhead" must stay the longest.
    ///     </para>
    /// </remarks>
    public override string[] NounsForMatching =>
    [
        // POD-DOOR
        "bulkhead", "door", "bulkhead door",
        "emergency bulkhead", "escape bulkhead", "pod bulkhead",
        "emergency door", "escape door", "pod door",
        "escape pod bulkhead", "escape pod door", "narrow emergency bulkhead",
        // GLOBAL-POD
        "pod", "emergency pod", "escape pod", "primary pod"
    ];

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