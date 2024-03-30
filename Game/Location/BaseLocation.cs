using Model.AIGeneration;
using Model.Item;
using Model.Location;

namespace Game.Location;

/// <summary>
///     The base class for all locations in the game.
/// </summary>
public abstract class BaseLocation : ILocation, ICanHoldItems
{
    /// <summary>
    ///     The Map defines all the places the user can go from this location. It can also define
    ///     locations where we cannot go, but for which we want to provide a custom message such
    ///     as "The kitchen window is closed."
    /// </summary>
    protected abstract Dictionary<Direction, MovementParameters> Map { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    public int VisitCount { get; set; }

    protected abstract string ContextBasedDescription { get; }

    public bool IsTransparent => false;

    public abstract string Name { get; }

    public List<IItem> Items { get; set; } = new();

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }

    public void ItemPlacedHere(IItem item)
    {
        var oldLocation = item.CurrentLocation;
        oldLocation?.RemoveItem(item);
        item.CurrentLocation = this;
        Items.Add(item);
    }

    public virtual bool HaveRoomForItem(IItem item)
    {
        return true;
    }

    public void OnItemPlacedHere(IItem item, IContext context)
    {
    }

    public bool HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasMatch = false;

        if (lookInsideContainers)
            Items.ForEach(i => hasMatch |= i.HasMatchingNoun(noun, lookInsideContainers));

        return hasMatch;
    }

    public virtual string AfterEnterLocation(IContext context)
    {
        return string.Empty;
    }

    public virtual InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        return new NoVerbMatchInteractionResult { Noun = string.Empty, Verb = input! };
    }

    public List<ITurnBasedActor> GetActors()
    {
        return Items.OfType<ITurnBasedActor>().ToList();
    }

    public virtual string BeforeEnterLocation(IContext context)
    {
        if (VisitCount == 0)
            OnFirstTimeEnterLocation(context);

        VisitCount++;

        return string.Empty;
    }

    public virtual string Description => Name + Environment.NewLine + ContextBasedDescription + GetItemDescriptions();

    public abstract void Init();

    public virtual string DescriptionForGeneration => Description;

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in this locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <param name="client"></param>
    /// <returns>InteractionResult that describes if and and how the interaction took place.</returns>
    public virtual InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        InteractionResult? result = null;

        foreach (var item in Items.ToList())
        {
            result = item.RespondToSimpleInteraction(action, context, client);
            if (result is PositiveInteractionResult or NoVerbMatchInteractionResult)
                return result;
        }

        return result ?? new NoNounMatchInteractionResult();
    }

    public virtual InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        return new NoNounMatchInteractionResult();
    }

    /// <summary>
    ///     We're trying to move in a direction. Return the <see cref="MovementParameters" /> object
    ///     that tells us if we can get there, and if so, where we end up. If there is no <see cref="MovementParameters" />
    ///     object corresponding to that location, then we cannot, by default, go that way (indicated by
    ///     a null response)
    /// </summary>
    /// <param name="direction">The direction we want to go from here. </param>
    /// <returns>
    ///     A <see cref="MovementParameters" /> that describes our ability to move there, or null
    ///     if movement in that direction is always impossible
    /// </returns>
    public MovementParameters? Navigate(Direction direction)
    {
        return Map.ContainsKey(direction) ? Map[direction] : null;
    }

    public bool HasItem<T>() where T : IItem, new()
    {
        return Items.Contains(Repository.GetItem<T>());
    }

    protected virtual void OnFirstTimeEnterLocation(IContext context)
    {
    }

    protected void StartWithItem(ItemBase item, ICanHoldItems location)
    {
        Items.Add(item);
        item.CurrentLocation = location;
    }

    /// <summary>
    ///     This is a wrapper so that child classes have a shorter
    ///     syntax to reference a location in the repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T GetLocation<T>() where T : class, ILocation, new()
    {
        return Repository.GetLocation<T>();
    }

    /// <summary>
    ///     Wrapper for base classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T GetItem<T>() where T : class, IItem, new()
    {
        return Repository.GetItem<T>();
    }

    protected string GetItemDescriptions()
    {
        if (Items.Count == 0)
            return string.Empty;

        var result = new StringBuilder();

        // Let's deal with all the "fixed" items like windows and mailboxes. 
        Items.Where(s => s is not ICanBeTakenAndDropped)
            // TODO: This is the wrong property name. Why use "InInventory" for something that can't be picked up
            .Aggregate(result, (s, item) => s.AppendLine(item.InInventoryDescription));

        // Now let's deal with items that can be picked up. 
        Items.Where(s => s is ICanBeTakenAndDropped).Cast<ICanBeTakenAndDropped>()
            .Aggregate(result, (s, item) =>
                s.AppendLine(item.HasEverBeenPickedUp ? item.OnTheGroundDescription : item.NeverPickedUpDescription));

        return result.ToString().Trim();
    }
}