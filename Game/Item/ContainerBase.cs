using Model.Item;

namespace Game.Item;

/// <summary>
///     Represents a base class for containers - items that can hold other items.
/// </summary>
public abstract class ContainerBase : ItemBase, ICanHoldItems
{
    protected virtual int SpaceForItems => 2;

    public List<IItem> Items { get; } = new();

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }

    public void ItemPlacedHere(IItem item)
    {
        Items.Add(item);
    }

    public virtual bool IsTransparent => false;

    /// <summary>
    ///     Checks if a location has an item of type T.
    /// </summary>
    /// <typeparam name="T">The type of item to check.</typeparam>
    /// <returns>True if the location has an item of type T, otherwise false.</returns>
    public bool HasItem<T>() where T : IItem, new()
    {
        return Items.Contains(Repository.GetItem<T>());
    }

    public override bool HasMatchingNoun(string? noun)
    {
        var hasMatch = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));
        Items.ForEach(i => hasMatch |= i.HasMatchingNoun(noun));

        return hasMatch;
    }

    public virtual bool HaveRoomForItem(IItem item)
    {
        return Items.Sum(s => s.Size) + item.Size <= SpaceForItems;
    }

    public abstract void Init();


    /// <summary>
    ///     Returns a description of the items contained in the specified container.
    /// </summary>
    /// <param name="name">The name of the container - might be needed as part of the description</param>
    /// <returns>A string representing the items contained in the specified container.</returns>
    protected string ItemListDescription(string name)
    {
        if (!Items.Any())
            return $"The {name} is empty.";

        var sb = new StringBuilder();
        
        if (IsTransparent || this is IOpenAndClose { IsOpen: true })
        {
            sb.AppendLine($"   The {name} contains:");
            Items.ForEach(s => sb.AppendLine($"      {s.InInventoryDescription}"));
        }

        return sb.ToString();
    }

    protected void StartWithItemInside<T>() where T : ItemBase, new()
    {
        var item = Repository.GetItem<T>();
        Items.Add(item);
        item.CurrentLocation = this;
    }
}