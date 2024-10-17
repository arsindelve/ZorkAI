using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Location;

namespace Model.Item;

public interface IItem : IInteractionTarget
{
    string[] NounsForMatching { get; }

    string GenericDescription(ILocation? currentLocation);

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

    /// <summary>
    /// Determines whether the given noun matches any of the nouns for matching of an item.
    /// </summary>
    /// <param name="noun">The noun to match.</param>
    /// <param name="lookInsideContainers">Indicates whether to also look inside containers for matches.</param>
    /// <returns>True if the noun matches any of the item's nouns for matching or any of its contained items' nouns for matching; otherwise, false.</returns>
    (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true);

    /// <summary>
    /// Determines whether the item has a matching noun and adjective. This method checks if the item's noun and adjective
    /// matches the provided noun and adjective, respectively. It also has an option to look inside containers for a match.
    /// </summary>
    /// <param name="noun">The noun to match against.</param>
    /// <param name="adjective">The adjective to match against.</param>
    /// <param name="lookInsideContainers">A flag indicating whether to look inside containers for a match. Default is true.</param>
    /// <returns>True if the item has a matching noun and adjective; otherwise, false.</returns>
    (bool HasItem, IItem? TheItem) HasMatchingNounAndAdjective(string? noun, string? adjective, bool lookInsideContainers = true);

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