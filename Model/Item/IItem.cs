using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Model.Location;

namespace Model.Item;

public interface IItem : IInteractionTarget
{
    /// <summary>
    /// These are effectively synonyms that will allows us to know that the adventurer is talking about a specific
    /// item. It can also include nouns with their advective. 
    /// </summary>
    /// <example>Pile of plastic, boat, magic boat, plastic, pile: All refer to the magic boat in Zork One.</example>
    string[] NounsForMatching { get; }

    /// <summary>
    /// The vast majority of the time, this is the same as <see cref="NounsForMatching"/>. The only time this will
    /// differ is when there are multiple objects with the same matching noun, and we MUST differentiate between them for
    /// puzzle solving, not just picking up and dropping, etc. The good lantern and the broken lantern in Zork do NOT need
    /// this because there is no puzzle that requires us to "use" the correct lantern. However in Planetfall, there
    /// are many "cards" and we must use the right card in the right place. 
    /// </summary>
    string[] NounsForPreciseMatching { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item has ever been picked up.
    /// </summary>
    bool HasEverBeenPickedUp { get; set; }

    string Name { get; }

    /// <summary>
    /// Can be a location, a container or the adventurer's inventory (i.e. the context). Or it
    /// can be null if the object is nowhere in the game, for example after being destroyed. 
    /// </summary>
    ICanHoldItems? CurrentLocation { get; set; }

    /// <summary>
    ///     The description of the item when it cannot be taken by the player. Returns null or empty
    /// if the object is able to be taken. 
    /// </summary>
    /// <value>
    ///     The description of the item when it cannot be taken by the player.
    /// </value>
    string? CannotBeTakenDescription { get; set; }

    int Size { get; }

    /// <summary>
    /// If the item can be picked up, this is the description in inventory..."A rope", "A towel".
    /// If the item cannot be picked up, this is the description of the item, on the ground, where you find it.
    /// This can be left blank, and then these items will not appear in the location description, but can still be
    /// interacted with. Sometimes this is done because the item description is part of the room or container description
    /// like the trap door, or the item is special and just does not show up, like the Planetfall slime. 
    /// </summary>
    /// <param name="currentLocation"></param>
    /// <returns></returns>
    string GenericDescription(ILocation? currentLocation);

    /// <summary>
    /// Determines whether the given noun matches any of the nouns for matching of a given item.
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
    (bool HasItem, IItem? TheItem) HasMatchingNounAndAdjective(string? noun, string? adjective,
        bool lookInsideContainers = true);

    Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory);

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="MultiNounIntent" /> corresponding
    ///     of a verb and two nouns. Does that combination do anything with items in our inventory?  This
    ///     method will be frequently overriden in child objects.
    /// </summary>
    /// <param name="action">The multi-noun intent representing the interaction.</param>
    /// <param name="context">The context in which the interaction occurs.</param>
    /// <returns>An InteractionResult indicating the result of the interaction.</returns>
    Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context);
}