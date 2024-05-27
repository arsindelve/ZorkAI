using Model.Item;
using Newtonsoft.Json;

namespace Model.Interface;

/// <summary>
///     Represents an object (item, location or context) that can hold other items.
/// </summary>
public interface ICanHoldItems : IInteractionTarget
{
    /// <summary>
    ///     When true, we can see the contents of the container even when it's closed.
    /// </summary>
    bool IsTransparent { get; }

    string Name { get; }

    List<IItem> Items { get; }

    /// <summary>
    ///     Retrieves all items recursively from the container
    /// </summary>
    /// <returns>A list of all items recursively.</returns>
    [JsonIgnore]
    List<IItem> GetAllItemsRecursively { get; }

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
    bool HasMatchingNoun(string? noun, bool lookInsideContainers = true);

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
}