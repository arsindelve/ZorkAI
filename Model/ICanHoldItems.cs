using Model.Item;

namespace Model;

public interface ICanHoldItems : IInteractionTarget
{
    /// <summary>
    ///     When true, we can see the contents of the container even when it's closed.
    /// </summary>
    bool IsTransparent { get; }

    string Name { get; }

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
    /// <returns>True if the container or context has an item with a matching noun, otherwise false.</returns>
    bool HasMatchingNoun(string? noun);

    List<IItem>? Items { get; }
    
    /// <summary>
    ///     Checks if the container or context has room to hold the given item.
    /// </summary>
    /// <param name="item">The item to be checked.</param>
    /// <returns>True if there is room to hold the item, otherwise false.</returns>
    bool HaveRoomForItem(IItem item);
}