using Model.Intent;
using Model.Interaction;

namespace Model.Item;

public interface IItem : IInteractionTarget
{
    string[] NounsForMatching { get; }
    
    string InInventoryDescription { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item has ever been picked up.
    /// </summary>
    bool HasEverBeenPickedUp { get; set; }


    ICanHoldItems? CurrentLocation { get; set; }

    /// <summary>
    ///     The description of the item when it cannot be taken by the player.
    /// </summary>
    /// <value>
    ///     The description of the item when it cannot be taken by the player.
    /// </value>
    string? CannotBeTakenDescription { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item is large, like a sword or a painting.
    /// </summary>
    bool IsLarge { get; }

    bool HasMatchingNoun(string? noun);
    
    InteractionResult? RespondToSimpleInteraction(SimpleIntent action, IContext context);
}