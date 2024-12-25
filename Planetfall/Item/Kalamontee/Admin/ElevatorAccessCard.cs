namespace Planetfall.Item.Kalamontee.Admin;

public abstract class ElevatorAccessCard : AccessCard
{
    /// <summary>
    /// Remove "elevator" and "elevator access noun" from the list of disambiguation nouns. There are too many items in the game
    /// with these nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => 
        base.NounsForPreciseMatching.Except(["elevator", "elevator access", "elevator card", "elevator access card"]).ToArray();
    
}