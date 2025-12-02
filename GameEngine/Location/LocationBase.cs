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
public abstract class LocationBase : ILocation, ICanContainItems
{
    public bool IsTransparent => false;

    [UsedImplicitly] public List<IItem> Items { get; set; } = new();

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }
    
    public virtual ICanContainItems? ForwardingContainer => null;

    /// <summary>
    /// Determines if there is an item within the location that matches the specified noun and adjective.
    /// </summary>
    /// <param name="noun">The noun to search for that identifies the item.</param>
    /// <param name="adjective">The adjective to match alongside the noun for further specificity.</param>
    /// <param name="lookInsideContainers">
    /// A flag indicating whether to include items within containers when performing the search.
    /// </param>
    /// <returns>
    /// A tuple containing a boolean indicating whether a matching item was found, and the matching item itself
    /// if one exists.
    /// </returns>
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

    public string NoRoomMessage => string.Empty;

    public void OnItemPlacedHere(IItem item, IContext context)
    {
    }

    public void OnItemRemovedFromHere(IItem item, IContext context)
    {
    }

    /// <summary>
    /// Retrieves a collection of all items contained within this location and, recursively, all items within any sub-containers.
    /// </summary>
    [JsonIgnore]
    public List<IItem> GetAllItemsRecursively
    {
        get
        {
            var result = new List<IItem>();
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
        // This makes no sense for locations.
        return 0;
    }

    public void ItemPlacedHere<T>() where T : IItem, new()
    {
        var item = Repository.GetItem<T>();
        ItemPlacedHere(item);
    }

    /// <summary>
    /// Handles the result message when an item is placed in the location.
    /// </summary>
    /// <param name="item">The item being placed in the location.</param>
    /// <param name="context">The context in which the placement is occurring.</param>
    /// <returns>
    /// A string representing the result of placing the item in the location, such as a status or descriptive message.
    /// </returns>
    public virtual string ItemPlacedHereResult(IItem item, IContext context)
    {
        return string.Empty;
    }

    /// <summary>
    /// By default, any item can be placed/dropped in any room.  
    /// </summary>
    public Type[] CanOnlyHoldTheseTypes => [];

    /// <summary>
    /// Generates an error message indicating that the attempted item cannot be placed in this location,
    /// as it does not match the allowed item types for this container.
    /// </summary>
    /// <param name="nameOfItemWeTriedToPlaceHere">
    /// The name of the item that was attempted to be placed.
    /// </param>
    /// <returns>
    /// A string containing the error message describing the mismatch between the item's type and
    /// the allowed types for this location.
    /// </returns>
    public virtual string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlaceHere) => string.Empty;

    public void ItemPlacedHere(IItem item)
    {
        var oldLocation = item.CurrentLocation;
        oldLocation?.RemoveItem(item);
        item.CurrentLocation = this;
        Items.Add(item);
    }

    public string LogItems()
    {
        return string.Join(", ", Items.Select(item => item.GenericDescription(this).Trim()));
    }

    /// <summary>
    /// Tracks the number of times the location has been entered.
    /// </summary>
    [UsedImplicitly]
    public int VisitCount { get; set; }

    /// <summary>
    ///     This property represents a sub-location inside another location. It can be used to define a location
    ///     that exists within another location, such as a vehicle or a specific area within a larger space.
    /// </summary>
    public virtual ISubLocation? SubLocation { get; set; }

    public abstract string Name { get; }

    /// <summary>
    /// Determines if there is an item within the location that matches the specified noun.
    /// </summary>
    /// <param name="noun">The noun to search for that identifies the item.</param>
    /// <param name="lookInsideContainers">
    /// A flag indicating whether to include items within containers when performing the search.
    /// </param>
    /// <returns>
    /// A tuple containing a boolean indicating whether a matching item was found, and the matching item itself
    /// if one exists.
    /// </returns>
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

    /// <summary>
    /// Executes actions or triggers events that occur after entering a specific location.
    /// </summary>
    /// <param name="context">The contextual information related to the game environment or player state.</param>
    /// <param name="previousLocation">The location from which the player is arriving.</param>
    /// <param name="generationClient">The client used for dynamically generating content or behavior.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a string
    /// which may describe information or outcomes from entering the location.
    /// </returns>
    public virtual Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        return Task.FromResult(string.Empty);
    }

    /// <summary>
    /// Handles behavior when leaving the current location to transition to a new location.
    /// </summary>
    /// <param name="context">The context in which the location change is occurring. This typically includes details about the actors and items involved.</param>
    /// <param name="newLocation">The location to which the transition is being made.</param>
    /// <param name="previousLocation">The location from which the transition is occurring.</param>
    public virtual void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
    }

    public virtual Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        return Task.FromResult<InteractionResult>(new NoVerbMatchInteractionResult
            { Noun = string.Empty, Verb = input! });
    }

    /// <summary>
    /// Performs actions or triggers responses specific to waiting in the current location context.
    /// </summary>
    /// <param name="context">The contextual information related to the current state and environment of the location.</param>
    public virtual void OnWaiting(IContext context)
    {
        // Default, no unique action. 
    }

    /// <summary>
    /// Executes functionality that should occur immediately before entering a location. Can be overridden by subclasses
    /// to implement location-specific behaviors.
    /// </summary>
    /// <param name="context">The context in which the location interaction is occurring, providing access to shared state and items.</param>
    /// <param name="previousLocation">The location being exited or transitioned from before entering this location.</param>
    /// <returns>A string message describing any events or outcomes triggered during the location transition.</returns>
    public virtual string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (VisitCount == 0)
            OnFirstTimeEnterLocation(context);

        VisitCount++;

        return string.Empty;
    }

    /// <summary>
    /// Retrieves a list of possible exit directions from the current location based on the provided context.
    /// </summary>
    /// <param name="context">The context representing the current environment and state of the game.</param>
    /// <returns>
    /// A list of directions indicating the possible exits available from the current location.
    /// </returns>
    public List<Direction> Exits(IContext context)
    {
        return Map(context)
            .Where(s => s.Value.CanGo(context))
            .Select(s => s.Key)
            .ToList();
    }

    public virtual string GetDescription(IContext context, bool fullDescription = true)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Name);
        stringBuilder.AppendLine(SubLocation?.LocationDescription);

        if (fullDescription)
            stringBuilder.Append(GetContextBasedDescription(context));

        stringBuilder.Append(GetItemDescriptions());
        return stringBuilder.ToString();
    }

    public abstract void Init();

    public virtual string GetDescriptionForGeneration(IContext context)
    {
        return GetDescription(context);
    }

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in these locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <param name="client"></param>
    /// <param name="itemProcessorFactory"></param>
    /// <returns>InteractionResult that describes if and how the interaction took place.</returns>
    public virtual async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        InteractionResult? result = null;

        foreach (var item in Items.ToList())
        {
            result = await item.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
            if (result is PositiveInteractionResult or NoVerbMatchInteractionResult)
                return result;
        }

        return result ?? new NoNounMatchInteractionResult();
    }

    public virtual Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        return Task.FromResult<InteractionResult?>(new NoNounMatchInteractionResult());
    }

    /// <summary>
    ///     We're trying to move in a direction. Return the <see cref="MovementParameters" /> object
    ///     that tells us if we can get there, and if so, where we end up. If there is no <see cref="MovementParameters" />
    ///     object corresponding to that location, then we cannot, by default, go that way (indicated by
    ///     a null response)
    /// </summary>
    /// <param name="direction">The direction we want to go from here. </param>
    /// <param name="context"></param>
    /// <returns>
    ///     A <see cref="MovementParameters" /> that describes our ability to move there, or null
    ///     if movement in that direction is always impossible
    /// </returns>
    public MovementParameters? Navigate(Direction direction, IContext context)
    {
        return Map(context).ContainsKey(direction) ? Map(context)[direction] : null;
    }

    public bool HasItem<T>() where T : IItem, new()
    {
        return Items.Contains(Repository.GetItem<T>());
    }

    /// <summary>
    ///     The Map defines all the places the user can go from this location. It can also define
    ///     locations where we cannot go, but for which we want to provide a custom message such
    ///     as "The kitchen window is closed."
    /// </summary>
    /// <param name="context"></param>
    protected abstract Dictionary<Direction, MovementParameters> Map(IContext context);

    protected abstract string GetContextBasedDescription(IContext context);

    /// <summary>
    /// Executes custom logic when the location is entered for the first time.
    /// </summary>
    /// <param name="context">The context representing the state of the game or environment at the time of entry.</param>
    protected virtual void OnFirstTimeEnterLocation(IContext context)
    {
    }

    /// <summary>
    /// Adds an item of a specified type to the current location's inventory and sets its current location
    /// to this location.
    /// </summary>
    /// <typeparam name="T">
    /// The type of item to add, which must implement the <c>IItem</c> interface and have a parameterless constructor.
    /// </typeparam>
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

    /// <summary>
    /// Generates descriptions for all items within the location.
    /// </summary>
    /// <returns>
    /// A string representing the descriptions of all items, including specific details
    /// for items that can be taken and those that cannot. If there are no items, an empty string is returned.
    /// </returns>
    protected virtual string GetItemDescriptions()
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

        return Environment.NewLine + result.ToString().Trim();
    }

    // Syntactic sugar 
    protected MovementParameters Go<T>() where T : class, ILocation, new()
    {
        return new MovementParameters { Location = GetLocation<T>() };
    }
}