using Model.Intent;
using Model.Interaction;

namespace Model.Item;

public interface IItem
{
    string[] NounsForMatching { get; }
    string InInventoryDescription { get; }
    bool HasMatchingNoun(string? noun);
    InteractionResult? RespondToSimpleInteraction(SimpleIntent action, IContext context);

    /// <summary>
    /// Gets or sets a value indicating whether the item has ever been picked up.
    /// </summary>
    bool HasEverBeenPickedUp { get; set; }
    
    
    ICanHoldItems? CurrentLocation { get; set; }

    /// <summary>
    /// The description of the item when it cannot be taken by the player.
    /// </summary>
    /// <value>
    /// The description of the item when it cannot be taken by the player.
    /// </value>
    string? CannotBeTakenDescription { get; set; }
}