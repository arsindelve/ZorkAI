using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;

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
    
    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="MultiNounIntent" /> corresponding
    ///     of a verb and two nouns. Does that combination do anything with items in our inventory?  This
    ///     method will be frequently overriden in child objects.
    /// </summary>
    /// <param name="action">The multi-noun intent representing the interaction.</param>
    /// <param name="context">The context in which the interaction occurs.</param>
    /// <returns>An InteractionResult indicating the result of the interaction.</returns>
    InteractionResult? RespondToMultiNounInteraction(MultiNounIntent action, IContext context);
}