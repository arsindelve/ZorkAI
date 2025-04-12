using System.ComponentModel;

namespace Planetfall.Item.Lawanda.Library;

public abstract class SpoolBase : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    /// <summary>
    /// Remove "spool" from the list of disambiguation nouns. There are too many items in the game
    /// with these nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["spool"]).ToArray();

    public abstract string Contents { get; }

    public abstract string ExaminationDescription { get; }

    public abstract string OnTheGroundDescription(ILocation currentLocation);

    public override int Size => 1;

    public override string? OnBeingTaken(IContext context, ICanContainItems? container)
    {
        if(container is SpoolReader)
            return "The screen goes blank as you take the spool.";
        
        return base.OnBeingTaken(context, container);
    }
}