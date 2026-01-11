using Planetfall.Item.Lawanda;
using ZorkOne.Command;

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
        if (CurrentLocation != Repository.GetItem<LargeMetalCube>())
            return base.OnBeingTaken(context, previousLocation);
        
        string causeOfDeath = "Kerzap!! You should know better than to touch an active bedistor! ";
        return new DeathProcessor().Process(causeOfDeath, context).InteractionMessage;
    }
}
