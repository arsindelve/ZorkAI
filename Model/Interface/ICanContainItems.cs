using Model.Item;
using Newtonsoft.Json;

namespace Model.Interface;

/// <summary>
///     Represents an object (item, location or context) that can hold other items.
/// </summary>
public interface ICanContainItems : IInteractionTarget
{
    /// <summary>
    ///     When true, we can see the contents of the container even when it's closed. Examples are a glass bottle,
    /// or a basket with no lid. 
    /// </summary>
    bool IsTransparent { get; }

    string Name { get; }

    List<IItem> Items { get; }

    /// <summary>
    /// A list of types the container can hold. If empty or null, the container can hold anything. 
    /// </summary>
    Type[] CanOnlyHoldTheseTypes { get; }

    /// <summary>
    ///     Retrieves all items recursively from the container
    /// </summary>
    /// <returns>A list of all items recursively.</returns>
    [JsonIgnore]
    List<IItem> GetAllItemsRecursively { get; }

    /// <summary>
    /// Generates a message indicating that there is no room available for an item.
    /// (Alternatively, that putting something into this item is impossible, like toothpaste
    /// back into a tube. 
    /// </summary>
    /// <returns>A string containing the no-room message.</returns>
    string NoRoomMessage { get; }

    /// <summary>
    /// When not-null, when something is placed in this container, it will pass it along to another recipient, usually a sub-component.
    /// Example: The uniform has a pocket. If you try to put something in the uniform, you're really putting it in the pocket. This
    /// distinction can be important because the pocket might open and close (while the uniform does not) so the pocket needs
    /// to be it's own thing. But we don't want the parser to do be so dumb that you cannot put something in the uniform, when
    /// clearly we know the player meant "the pocket of the uniform".
    /// </summary>
    ICanContainItems? ForwardingContainer { get; }

    /// <summary>
    /// An error message to display if an invalid item type is placed in the container.
    /// </summary>
    string? CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace);

    /// <summary>
    ///     Calculates the total size of all the items in the container or context.
    /// </summary>
    /// <returns>The total size of the container or context.</returns>
    int CalculateTotalSize();

    void RemoveItem(IItem item);

    /// <summary>
    ///     Adds the item to the container.
    /// </summary>
    /// <param name="item">The item to be placed.</param>
    void ItemPlacedHere(IItem item);

    /// <summary>
    ///     Adds the item to the container.
    /// </summary>
    void ItemPlacedHere<T>() where T : IItem, new();

    string ItemPlacedHereResult(IItem item, IContext context);

    /// <summary>
    ///     Checks if this location currently has an item of type T.
    /// </summary>
    /// <typeparam name="T">The type of item to check.</typeparam>
    /// <returns>True if the location has an item of type T, otherwise false.</returns>
    bool HasItem<T>() where T : IItem, new();

    /// <summary>
    ///     Checks if the container or context has an item with a matching noun.
    /// </summary>
    /// <param name="noun">The noun to match against.</param>
    /// <param name="lookInsideContainers">
    ///     If true, the container will return true if the item
    ///     is inside the given container, even if it's inside another container
    /// </param>
    /// <returns>True if the container or context has an item with a matching noun, otherwise false.</returns>
    (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true);

    /// <summary>
    ///     Checks if the container or context has an item with a matching noun and adjective, if one was provided in the
    ///     input.
    /// </summary>
    /// <param name="noun">The noun to match against.</param>
    /// <param name="adjective"></param>
    /// <param name="lookInsideContainers">
    ///     If true, the container will return true if the item
    ///     is inside the given container, even if it's inside another container
    /// </param>
    /// <returns>True if the container or context has an item with a matching noun, otherwise false.</returns>
    (bool HasItem, IItem? TheItem) HasMatchingNounAndAdjective(string? noun, string? adjective,
        bool lookInsideContainers = true);

    /// <summary>
    ///     Checks if the container or context has room to hold the given item.
    /// </summary>
    /// <param name="item">The item to be checked.</param>
    /// <returns>True if there is room to hold the item, otherwise false.</returns>
    bool HaveRoomForItem(IItem item);

    /// <summary>
    ///     Initializes the container with whichever items it's holding (if any)
    ///     at the start of the game
    /// </summary>
    void Init();

    /// <summary>
    ///     Event handler that is triggered when an item is placed into this container.
    /// </summary>
    /// <param name="item">The item being placed into the container.</param>
    /// <param name="context"></param>
    void OnItemPlacedHere(IItem item, IContext context);

    /// <summary>
    ///     Event handler that is triggered when an item is removed from this container.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="context"></param>
    void OnItemRemovedFromHere(IItem item, IContext context);
}