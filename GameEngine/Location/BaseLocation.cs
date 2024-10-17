using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;
using Model.Movement;
using Newtonsoft.Json;

namespace GameEngine.Location;

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

    protected abstract string ContextBasedDescription { get; }

    public bool IsTransparent => false;

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global: Needed by deserializer.
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

    public (bool HasItem, IItem? TheItem) HasMatchingNounAndAdjective(string? noun, string? adjective,
        bool lookInsideContainers = true)
    {
        if (string.IsNullOrEmpty(adjective))
            return HasMatchingNoun(noun, lookInsideContainers);

        foreach (var i in Items)
        {
            var result = i.HasMatchingNounAndAdjective(noun, adjective, lookInsideContainers);
            if (result.HasItem)
                return result;
        }

        return (false, null);
    }

    public virtual bool HaveRoomForItem(IItem item)
    {
        return true;
    }

    public void OnItemPlacedHere(IItem item, IContext context)
    {
    }

    [JsonIgnore]
    public List<IItem> GetAllItemsRecursively
    {
        get
        {
            var result = new List<IItem>();
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
        // This makes no sense for locations.
        return 0;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public int VisitCount { get; set; }

    /// <summary>
    ///     This property represents a sub-location inside another location. It can be used to define a location
    ///     that exists within another location, such as a vehicle or a specific area within a larger space.
    /// </summary>
    public virtual ISubLocation? SubLocation { get; set; }

    public abstract string Name { get; }

    public (bool HasItem, IItem? TheItem) HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        if (lookInsideContainers)
            foreach (var i in Items)
            {
                var result = i.HasMatchingNoun(noun, lookInsideContainers);
                if (result.HasItem)
                    return result;
            }

        return (false, null);
    }

    public virtual Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return Task.FromResult(string.Empty);
    }

    public virtual void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
    }

    public virtual InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        return new NoVerbMatchInteractionResult { Noun = string.Empty, Verb = input! };
    }

    public virtual void OnWaiting(IContext context)
    {
        // Default, no unique action. 
    }

    public virtual string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (VisitCount == 0)
            OnFirstTimeEnterLocation(context);

        VisitCount++;

        return string.Empty;
    }

    public virtual string Description => Name +
                                         SubLocation?.LocationDescription +
                                         Environment.NewLine +
                                         ContextBasedDescription +
                                         GetItemDescriptions();

    public abstract void Init();

    public virtual string DescriptionForGeneration => Description;

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in these locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <param name="client"></param>
    /// <returns>InteractionResult that describes if and how the interaction took place.</returns>
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
        if (Map is null)
            throw new Exception($"Location {Name} has a null map");

        return Map.ContainsKey(direction) ? Map[direction] : null;
    }

    public bool HasItem<T>() where T : IItem, new()
    {
        return Items.Contains(Repository.GetItem<T>());
    }

    protected virtual void OnFirstTimeEnterLocation(IContext context)
    {
    }

    protected void StartWithItem<T>() where T : IItem, new()
    {
        var item = Repository.GetItem<T>();
        Items.Add(item);
        item.CurrentLocation = this;
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
            .Aggregate(result, (s, item) => s.AppendLine(item.GenericDescription(this)));

        // Now let's deal with items that can be picked up. 
        Items.Where(s => s is ICanBeTakenAndDropped).Cast<ICanBeTakenAndDropped>()
            .Aggregate(result, (s, item) =>
                s.AppendLine(item.HasEverBeenPickedUp
                    ? item.OnTheGroundDescription(this)
                    : item.NeverPickedUpDescription(this)));

        return result.ToString().Trim();
    }

    // Syntactic sugar 
    protected MovementParameters Go<T>() where T : class, ILocation, new()
    {
        return new MovementParameters { Location = GetLocation<T>() };
    }
}