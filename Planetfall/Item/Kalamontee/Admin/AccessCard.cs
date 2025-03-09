namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
/// Represents an abstract base class for access cards in the game.
/// Access cards often have specific interactions and unique nouns to match for gameplay purposes.
/// </summary>
/// <remarks>
/// This class inherits from ItemBase and modifies the behavior of NounsForPreciseMatching to exclude
/// general terms like "card" and "access card," ensuring more precise identification during interactions.
/// </remarks>
public abstract class AccessCard : ItemBase
{
    /// <summary>
    /// Remove "card" and "access card" from the list of disambiguation nouns. There are too many items in the game
    /// with these nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["card", "access card"]).ToArray();
}