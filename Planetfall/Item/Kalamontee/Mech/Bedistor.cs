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
    public override string[] NounsForMatching => ["good ninety-ohm bedistor", "bedistor", "ninety-ohm bedistor", "good bedistor", "ninety-ohm", "90-ohm bedistor", "90-ohm"];

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
