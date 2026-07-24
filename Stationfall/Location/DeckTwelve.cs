using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace Stationfall.Location;

/// <summary>
///     The starting room — Deck Twelve of the S.P.S. Duffy (original room: ship.zil DECK-TWELVE).
///     Phase 1 boots here so <c>look</c> works; the real ship exits and the departure puzzle are
///     Phase 2. Description is original scaffold prose (no ZIL text copied).
/// </summary>
public class DeckTwelve : LocationBase
{
    public override string Name => "Deck Twelve";

    public override string[] NounsForMatching => ["deck 12", "deck twelve"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        // Phase 1: neighbouring rooms aren't ported yet, so the map is empty (every direction ->
        // "you can't go that way"). Phase 2 (the ship) wires the real exits, verified against ship.zil:
        //   East  -> Cargo Bay Entrance
        //   South -> Forms Storage Room (a red-herring room)
        //   West  -> a fake door that never opens: "The door is closed." (ship.zil:15-23)
        return new Dictionary<Direction, MovementParameters>();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on Deck Twelve, the cargo-and-forms level of the Stellar Patrol Ship Duffy. " +
               "The corridor runs aft toward the cargo bay entrance to the east, past a storage hatch " +
               "to the south. A sealed door to the west is marked for authorized personnel only. ";
    }

    public override void Init()
    {
        // Phase 2: StartWithItem<...>() for the robot-pool console/slot and the forms-feelie handling.
    }
}
