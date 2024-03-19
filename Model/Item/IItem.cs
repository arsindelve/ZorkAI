using Model.Intent;
using Model.Interaction;
using OpenAI;

namespace Model.Item;

public interface IItem : IInteractionTarget
{
    string[] NounsForMatching { get; }

    string InInventoryDescription { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item has ever been picked up.
    /// </summary>
    bool HasEverBeenPickedUp { get; set; }

    string Name { get; }


    ICanHoldItems? CurrentLocation { get; set; }

    /// <summary>
    ///     The description of the item when it cannot be taken by the player.
    /// </summary>
    /// <value>
    ///     The description of the item when it cannot be taken by the player.
    /// </value>
    string? CannotBeTakenDescription { get; set; }

    int Size { get; }

    bool HasMatchingNoun(string? noun, bool lookInsideContainers = true);

    InteractionResult? RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client);
}