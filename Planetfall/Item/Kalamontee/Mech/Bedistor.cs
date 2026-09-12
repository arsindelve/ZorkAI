using Planetfall.Command;
using Planetfall.Item.Lawanda;

namespace Planetfall.Item.Kalamontee.Mech;

public abstract class BedistorBase : ItemBase
{
    /// <summary>
    /// Remove "bedistor" from the list of disambiguation nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["bedistor", "ninety-ohm bedistor", "ninety-ohm", "90-ohm bedistor", "90-ohm"]).ToArray();

    public override int Size => 1;
}


public class GoodBedistor : BedistorBase, ICanBeTakenAndDropped
{
    // Issue #550: the bare adjective "good" must be a handle of its own, exactly as "fused" is on
    // FusedBedistor - the original registers both as adjectives (ADJECTIVE GOOD NINETY OHM,
    // compone.zil:1419) and the Infocom parser resolves an unambiguous adjective with no head noun.
    // The containment fallback in HasMatchingNoun cannot cover for a missing entry here: it asks
    // whether the player's PHRASE contains one of our nouns, so "good" on its own matches nothing
    // unless it is itself registered. That made "put good in cube" a dropped turn - no handler at
    // all, and narrator prose in place of the bedistor swap. Keep the full name FIRST (Name is
    // NounsForMatching.First(), and the printed name is the LONGEST entry, so a short handle is
    // safe to add).
    public override string[] NounsForMatching => ["good ninety-ohm bedistor", "bedistor", "ninety-ohm bedistor", "good bedistor", "good", "ninety-ohm", "90-ohm bedistor", "90-ohm"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a good ninety-ohm bedistor here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A good ninety-ohm bedistor";
    }

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        // Check previousLocation, NOT CurrentLocation. TakeOrDropInteractionProcessor calls
        // context.Take before OnBeingTaken, and Take reassigns CurrentLocation to the player - so by
        // the time this runs the bedistor is already "in your hands" and CurrentLocation can never be
        // the cube. Guarding on it made this death unreachable: pulling the good bedistor back out of
        // a live Course Control socket just answered "Taken." The socket we came out of is exactly
        // what previousLocation is for.
        //
        // The using directives above matter here too: this file used to import ZorkOne.Command, so
        // `new DeathProcessor()` bound to Zork's, which throws "requires a ZorkIContext" on a
        // PlanetfallContext. Between that and the CurrentLocation guard the death was doubly
        // unreachable - the guard hid the wrong-assembly bind, and the wrong bind would have thrown
        // the moment the guard was fixed on its own.
        if (previousLocation != Repository.GetItem<LargeMetalCube>())
            return base.OnBeingTaken(context, previousLocation);
        
        string causeOfDeath = "Kerzap!! You should know better than to touch an active bedistor! ";
        return new DeathProcessor().Process(causeOfDeath, context).InteractionMessage;
    }
}
