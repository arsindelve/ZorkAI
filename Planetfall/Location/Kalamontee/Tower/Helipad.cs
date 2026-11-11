using GameEngine.Location;

namespace Planetfall.Location.Kalamontee.Tower;

public class Helipad : LocationWithNoStartingItems
{
    public override string Name => "Helipad";

    public override string[] NounsForMatching => ["landing pad", "helicopter pad"];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<TowerCore>() },
            { Direction.In, Go<Helicopter>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are at the center of a wide, flat area atop the tower. A fence prevents you from " +
            "approaching the perimeter, so your view is limited to cloud-filled sky. A large vehicle, " +
            "severely weathered and topped with rotor blades, lies nearby. A spiral staircase leads " +
            "down into the tower. ";
    }

    /// <summary>
    ///     Issue #519: this room names four things and used to recognise none of them, so every noun in
    ///     its own description fell through to the narrator. The original attaches the vehicle and the
    ///     stairway as local-globals and the fence as a pseudo (planetfall-source/compone.zil:2740-2765),
    ///     so all four are referenceable while standing on the pad.
    ///     The vehicle stays scenery rather than a real item: it is one object seen from two rooms, and a
    ///     single instance seeded into both would have its CurrentLocation overwritten by whichever
    ///     Init() ran last. Boarding it is handled by the map's In exit — <see cref="Helicopter" /> owns
    ///     the matching nouns so "enter vehicle" and "enter helicopter" both resolve to that room.
    /// </summary>
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["vehicle", "large vehicle", "helicopter", "chopper", "copter"],
            "The vehicle is severely weathered, and sits so low on its struts that it looks to have " +
            "rested here for centuries. Its doors, at least, still appear to open. ",
            "The vehicle is rather larger than you are. You could, however, climb into it. "),

        new(["rotor blades", "rotor blade", "rotors", "rotor", "blades", "blade"],
            "The rotor blades droop over the top of the vehicle, pitted and streaked with rust. " +
            "Nothing about them suggests they have turned in living memory. ",
            "The blades are bolted to the top of the vehicle. "),

        new(["fence", "perimeter"],
            "The fence rings the perimeter of the pad, well above your head and anchored firmly to " +
            "the roof. Past it there is nothing to see but cloud-filled sky. ",
            "The fence is bolted to the tower, and is the only thing between you and a very long fall. "),

        new(["staircase", "spiral staircase", "stairs", "stairway", "steps"],
            "The spiral staircase winds down through an opening in the pad, into the tower below. ",
            "The staircase is part of the tower itself. ")
    ];
}