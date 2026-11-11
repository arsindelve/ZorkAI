using GameEngine.Item;

namespace Planetfall.Item.Feinstein;

/// <summary>
///     The narrow emergency bulkhead at the base of the gangway that seals Deck Nine off from the
///     Gangway when the Feinstein starts coming apart. Ported to restore the original's seal
///     mechanic (issue #529): the game narrates it crashing shut during the explosion
///     (planetfall-source/globals.zil:1214-1217), and both Deck Nine's UP exit (globals.zil:503)
///     and the Gangway's DOWN exit back (globals.zil:646) are gated on its OPENBIT. It starts open;
///     <see cref="Feinstein.ExplosionCoordinator" /> clears it the turn the bulkheads crash shut.
/// </summary>
/// <remarks>
///     Deliberately NOT seeded into any room's scope — see <see cref="CorridorDoor" /> for why this
///     and the corridor bulkhead live purely as state rather than as scoped, nameable objects.
/// </remarks>
internal class GangwayDoor : ItemBase
{
    // The narrow bulkhead's DESC in the original (globals.zil:1141).
    public override string[] NounsForMatching => ["narrow bulkhead", "narrow emergency bulkhead"];

    // Starts open: Deck Nine's Up exit is freely traversable until the explosion seals it.
    public bool IsOpen { get; set; } = true;
}
