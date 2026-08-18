using GameEngine.Item;

namespace Planetfall.Item.Feinstein;

/// <summary>
///     The wide emergency bulkhead that seals Deck Nine's starboard corridor to the Reactor Lobby
///     when the Feinstein starts coming apart. Ported to restore the original's seal mechanic
///     (issue #529): the game narrates it crashing shut during the explosion
///     (planetfall-source/globals.zil:1214-1217), and both Deck Nine's EAST exit
///     (globals.zil:500) and the Reactor Lobby's WEST exit back (globals.zil:629) are gated on its
///     OPENBIT. It starts open; <see cref="Feinstein.ExplosionCoordinator" /> clears it the turn the
///     bulkheads crash shut.
/// </summary>
/// <remarks>
///     Deliberately NOT seeded into any room's scope, unlike the pod <see cref="BulkheadDoor" />.
///     In the original this and <see cref="GangwayDoor" /> both start INVISIBLE and only become
///     referenceable once they slam shut, and they SHARE the synonyms DOOR/BULKHEAD with the pod
///     door (globals.zil:1131-1143). Placing them in Deck Nine's scope would make "bulkhead"/"door"
///     ambiguous with the pod door the player actually needs to reach — the exact ambiguity
///     <see cref="BulkheadDoor" />'s noun list is written to avoid. Their only job here is to hold
///     the one bit that gates the exits, so they live purely as state; the exit
///     <c>MovementParameters</c> read <see cref="IsOpen" /> off the singleton via <c>CanGo</c>.
/// </remarks>
internal class CorridorDoor : ItemBase
{
    // The wide bulkhead's DESC in the original (globals.zil:1133). Never surfaced in scope, so these
    // never collide with the pod door: containment matching is directional, and a query for
    // "bulkhead"/"door" is shorter than "wide bulkhead" and so never resolves to this item.
    public override string[] NounsForMatching => ["wide bulkhead", "wide emergency bulkhead"];

    // Starts open: Deck Nine's East exit is freely traversable until the explosion seals it.
    public bool IsOpen { get; set; } = true;
}
