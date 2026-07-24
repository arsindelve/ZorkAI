using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace Stationfall.Location;

/// <summary>
///     Placeholder starting room for the Stationfall scaffold — Deck Twelve of the SPS Duffy.
///     The description below is original scaffold text, to be replaced with a faithfully-ported
///     room; exits are empty until the ship deck is mapped. (Original room: ship.zil DECK-TWELVE.)
/// </summary>
public class DeckTwelve : LocationBase
{
    private readonly Dictionary<Direction, MovementParameters> _map = new();

    public override string Name => "Deck Twelve";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return _map;
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on Deck Twelve of the Stellar Patrol Ship Duffy. (Scaffold room — the real " +
               "deck layout and description are still to be ported.) ";
    }

    public override void Init()
    {
    }
}
