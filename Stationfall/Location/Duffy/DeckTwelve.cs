using Model.AIGeneration;

namespace Stationfall.Location.Duffy;

/// <summary>
///     The starting room aboard the S.P.S. Duffy (ship.zil:5-18). The door to port is a fake that never
///     opens, and the slot beside it always rejects the Assignment Completion Form as unvalidated
///     (ship.zil:20-32; globals.zil:730-734) — the whole west side is a red herring, and the way off the
///     ship is east, through the cargo bay.
/// </summary>
public class DeckTwelve : LocationBase
{
    public override string Name => "Deck Twelve";

    public override string[] NounsForMatching => ["deck 12", "deck twelve"];

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["door", "port door", "closed door", "sealed door"],
            "A heavy door leading to the rest of the Duffy. It is firmly closed, and there is a slot " +
            "set into the bulkhead beside it. ",
            "The door is part of the ship. ")
    ];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<CargoBayEntrance>() },
            { Direction.S, Go<FormStorageRoom>() },
            {
                // The door to port never opens: the slot rejects the only form that would open it.
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => false,
                    CustomFailureMessage = "The door is closed. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in the heart of the administrative level, the largest deck on the Duffy or on " +
               "any other Stellar Patrol ship. The corridor continues to starboard, and a room lies " +
               "aft. Beyond the door to port lies the bulk of the ship. Next to the door is a slot. ";
    }

    public override void Init()
    {
        StartWithItem<DeckTwelveSlot>();
    }
}
