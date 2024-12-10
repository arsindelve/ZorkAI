using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;
using Newtonsoft.Json;
using Utilities;

namespace GameEngine.Item;

/// <summary>
///     Represents a base class for containers - items that can hold other items.
/// </summary>
public abstract class ContainerBase : ItemBase, ICanHoldItems
{
    protected virtual int SpaceForItems => 2;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global: The deserializer needs this. 
    public List<IItem> Items { get; set; } = new();

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }

    public void ItemPlacedHere(IItem item)
    {
        var location = item.CurrentLocation;
        location?.RemoveItem(item);
        item.CurrentLocation = this;
        Items.Add(item);
    }
    
    public void ItemPlacedHere<T>() where T : IItem, new()
    {
        T item = Repository.GetItem<T>();
        ItemPlacedHere(item);
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

    public override (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasMatch = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));

        if (hasMatch)
            return (true, this);

        if (lookInsideContainers)
            foreach (var i in Items)
            {
                var result = i.HasMatchingNoun(noun, lookInsideContainers);
                if (result.HasItem)
                    return result;
            }

        return (false, null);
    }

    public virtual bool HaveRoomForItem(IItem item)
    {
        return Items.Sum(s => s.Size) + item.Size <= SpaceForItems;
    }

    public abstract void Init();

    public virtual void OnItemPlacedHere(IItem item, IContext context)
    {
    }

    public virtual void OnItemRemovedFromHere(IItem item, IContext context)
    {
    }

    [JsonIgnore]
    public List<IItem> GetAllItemsRecursively
    {
        get
        {
            var result = new List<IItem>();

            if (this is not IOpenAndClose { IsOpen: true })
                return result;

            foreach (var item in Items)
            {
                result.Add(item);
                if (item is ICanHoldItems holder)
                    result.AddRange(holder.GetAllItemsRecursively);
            }

            return result;
        }
    }

    public int CalculateTotalSize()
    {
        var totalSize = Items.Sum(item => item.Size);

        // Also add the Size of items inside Containers
        foreach (var container in Items.OfType<ContainerBase>()) totalSize += container.CalculateTotalSize();

        return totalSize;
    }

    public string SingleLineListOfItems()
    {
        var nouns = Items.Select(s => s.NounsForMatching.OrderByDescending(q => q.Length).First()).ToList();
        return !nouns.Any() ? "" : nouns.SingleLineListWithAnd();
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        InteractionResult? result = null;

        // See if one of the items inside me has a matching interaction.
        foreach (var item in Items.ToList())
        {
            result = item.RespondToSimpleInteraction(action, context, client);
            if (result is { InteractionHappened: true })
                return result;
        }

        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return ApplyProcessors(action, context, null, client);
    }

    /// <summary>
    ///     Returns a description of the items contained in the specified container.
    /// </summary>
    /// <param name="name">The name of the container - might be needed as part of the description</param>
    /// <returns>A string representing the items contained in the specified container.</returns>
    public virtual string ItemListDescription(string name, ILocation? location)
    {
        if (!Items.Any())
            return $"The {name} is empty.";

        var sb = new StringBuilder();

        if (IsTransparent || this is IOpenAndClose { IsOpen: true })
        {
            sb.AppendLine($"The {name} contains:");
            Items.ForEach(s => sb.AppendLine($"      {s.GenericDescription(location)}"));
        }

        if (!IsTransparent && this is IOpenAndClose { IsOpen: false })
            sb.AppendLine($"The {name} is closed.");

        return sb.ToString();
    }

    protected void StartWithItemInside<T>() where T : ItemBase, new()
    {
        var item = Repository.GetItem<T>();
        Items.Add(item);
        item.CurrentLocation = this;
    }
}