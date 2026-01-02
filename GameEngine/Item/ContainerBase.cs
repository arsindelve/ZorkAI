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
public abstract class ContainerBase : ItemBase, ICanContainItems
{
    /// <summary>
    /// Specifies the maximum capacity for items that the container can hold, represented as an integer value.
    /// The value determines the combined size of items the container can accommodate.
    /// </summary>
    protected virtual int SpaceForItems => 2;

    [UsedImplicitly] public List<IItem> Items { get; set; } = new();

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }
    
    public virtual ICanContainItems? ForwardingContainer => null;

    public void ItemPlacedHere(IItem item)
    {
        var location = item.CurrentLocation;
        location?.RemoveItem(item);
        item.CurrentLocation = this;
        Items.Add(item);
    }

    public void ItemPlacedHere<T>() where T : IItem, new()
    {
        var item = Repository.GetItem<T>();
        ItemPlacedHere(item);
    }

    public virtual string ItemPlacedHereResult(IItem item, IContext context)
    {
        return "Done. ";
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

    /// <summary>
    /// Determines if any item within the container, or the container itself, matches the specified noun.
    /// </summary>
    /// <param name="noun">The noun to match against items or the container.</param>
    /// <param name="lookInsideContainers">
    /// A flag indicating whether to look inside nested containers for a matching item.
    /// </param>
    /// <returns>
    /// A tuple containing a boolean that indicates if a matching item was found, and the matching item, if any.
    /// </returns>
    public override (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasMatch = NounsForMatching.Any(s => s.Equals(noun, StringComparison.InvariantCultureIgnoreCase));

        if (hasMatch)
            return (true, this);

        if (lookInsideContainers)
        {
            // Check items in the Items collection
            foreach (var i in Items)
            {
                var result = i.HasMatchingNoun(noun, lookInsideContainers);
                if (result.HasItem)
                    return result;
            }

            // Also check ItemBeingHeld if this implements ICanHoldItems (held items are always accessible)
            if (this is ICanHoldItems { ItemBeingHeld: not null } holder)
            {
                var heldResult = holder.ItemBeingHeld.HasMatchingNoun(noun, lookInsideContainers);
                if (heldResult.HasItem)
                    return heldResult;
            }
        }

        return (false, null);
    }

    /// <summary>
    /// Determines whether the container has enough room for the specified item.
    /// </summary>
    /// <param name="item">The item to check for available space.</param>
    /// <returns>True if the container has enough space for the item, otherwise false.</returns>
    public virtual bool HaveRoomForItem(IItem item)
    {
        return Items.Sum(s => s.Size) + item.Size <= SpaceForItems;
    }

    public virtual string NoRoomMessage => "There's no room. ";

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
                if (item is ICanContainItems holder)
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

    /// <summary>
    /// By default, any item can be placed in any container, if there's room.  
    /// </summary>
    public virtual Type[] CanOnlyHoldTheseTypes => [];

    public virtual string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) => string.Empty;

    public string SingleLineListOfItems()
    {
        var nouns = Items.Select(s => s.NounsForMatching.OrderByDescending(q => q.Length).First()).ToList();
        return !nouns.Any() ? "" : nouns.SingleLineListWithAnd();
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        InteractionResult? result = null;

        // See if one of the items inside me has a matching interaction.
        foreach (var item in Items.ToList())
        {
            result = await item.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
            if (result is { InteractionHappened: true })
                return result;
        }
        
        // Also check ItemBeingHeld if this implements ICanHoldItems (held items are always accessible)
        if (this is ICanHoldItems { ItemBeingHeld: not null } holder)
        {
            var itemBeingHeld = holder.ItemBeingHeld;
            result = await itemBeingHeld.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
            if (result is { InteractionHappened: true })
                return result;
        }
        
        if (result != null && result is not NoNounMatchInteractionResult)
            return result;

        if (!action.MatchNoun(NounsForMatching))
            return new NoNounMatchInteractionResult();

        return await ApplyProcessors(action, context, null, client, itemProcessorFactory);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // See if one of the items inside me has a matching interaction.
        foreach (var item in Items.ToList())
        {
            var result = await item.RespondToMultiNounInteraction(action, context);
            if (result is { InteractionHappened: true })
                return result;
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }

    /// <summary>
    ///     Returns a description of the items contained in the specified container.
    /// </summary>
    /// <param name="name">The name of the container - might be needed as part of the description</param>
    /// <param name="location"></param>
    /// <returns>A string representing the items contained in the specified container.</returns>
    public virtual string ItemListDescription(string name, ILocation? location)
    {
        if (!Items.Any() && !string.IsNullOrEmpty(name))
            return $"The {name} is empty.";

        var sb = new StringBuilder();

        if (IsTransparent || this is IOpenAndClose { IsOpen: true })
        {
            if (!string.IsNullOrEmpty(name))
                sb.AppendLine($"The {name} contains:");
            Items.ForEach(s => sb.AppendLine($"   {s.GenericDescription(location)}"));
        }

        if (!IsTransparent && this is IOpenAndClose { IsOpen: false })
            sb.AppendLine($"The {name} is closed.");

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Adds an item of type T to this container and sets its current location to the container.
    /// </summary>
    /// <typeparam name="T">The type of the item to add, which must inherit from ItemBase and have a parameterless constructor.</typeparam>
    protected void StartWithItemInside<T>() where T : ItemBase, new()
    {
        var item = Repository.GetItem<T>();
        Items.Add(item);
        item.CurrentLocation = this;
    }
}