using Model.Item;

namespace Model;

public interface ICanHoldItems : IInteractionTarget
{
    // string ItemListDescription(string name);

    void RemoveItem(IItem item);

    /// <summary>
    /// Adds the dropped item to the container.
    /// </summary>
    /// <param name="item">The item to be dropped.</param>
    void ItemDropped(IItem item);

    /// <summary>
    /// Checks if this location currently has an item of type T.
    /// </summary>
    /// <typeparam name="T">The type of item to check.</typeparam>
    /// <returns>True if the location has an item of type T, otherwise false.</returns>
    bool HasItem<T>() where T : IItem, new();

    /// <summary>
    /// Checks if the container or context has an item with a matching noun.
    /// </summary>
    /// <param name="noun">The noun to match against.</param>
    /// <returns>True if the container or context has an item with a matching noun, otherwise false.</returns>
    bool HasMatchingNoun(string? noun);
}