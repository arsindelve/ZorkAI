namespace Planetfall.Item.Kalamontee.Admin;

public abstract class AccessCard : ItemBase
{
    /// <summary>
    /// Remove "card" and "access card" from the list of disambiguation nouns. There are too many items in the game
    /// with these nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["card", "access card"]).ToArray();
}