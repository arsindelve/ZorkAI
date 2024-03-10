using System.Collections.ObjectModel;
using Model.Item;

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

    protected abstract string Name { get; }

    private int VisitCount { get; set; }

    private List<IItem> Items { get; } = new();

    public ReadOnlyCollection<IItem> LocationItems => Items.AsReadOnly();

    protected abstract string ContextBasedDescription { get; }

    // public virtual string ItemListDescription(string name)
    // {
    //     return GetItemDescriptions();
    // }

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }

    public void ItemPlacedHere(IItem item)
    {
        Items.Add(item);
    }

    public virtual string OnEnterLocation(IContext context)
    {
        if (VisitCount == 0)
            OnFirstTimeEnterLocation(context);

        VisitCount++;

        return string.Empty;
    }

    public virtual string Description => Name + Environment.NewLine + ContextBasedDescription + GetItemDescriptions();

    public virtual string DescriptionForGeneration => ContextBasedDescription;

    /// <summary>
    ///     We have parsed the user input and determined that we have a <see cref="SimpleIntent" /> corresponding
    ///     of a verb and a noun. Does that combination do anything in this location? The default implementation
    ///     of the base class checks each item in this locations and asks them if they provide any interaction. This
    ///     method will be frequently overriden in child locations.
    /// </summary>
    /// <param name="action">The action to examine. Can we do anything with it?</param>
    /// <param name="context">The current context, in case we need it during action processing.</param>
    /// <returns>InteractionResult that describes if and and how the interaction took place.</returns>
    public virtual InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context)
    {
        InteractionResult? result = null;

        foreach (var item in Items.ToList())
        {
            result = item.RespondToSimpleInteraction(action, context);
            if (result is PositiveInteractionResult or NoVerbMatchInteractionResult)
                return result;
        }

        return result ?? new NoNounMatchInteractionResult();
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

    public bool HasMatchingNoun(string? noun)
    {
        var hasMatch = false;
        Items.ForEach(i => hasMatch |= i.HasMatchingNoun(noun));

        return hasMatch;
    }

    public virtual bool HaveRoomForItem(IItem item)
    {
        return true;
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

    private string GetItemDescriptions()
    {
        if (Items.Count == 0)
            return string.Empty;

        var result = new StringBuilder();

        // Let's deal with all the "fixed" items like windows and mailboxes. 
        Items.Where(s => s is not ICanBeTakenAndDropped)
            .Aggregate(result, (s, item) => s.AppendLine(item.InInventoryDescription));

        // Now let's deal with items that can be picked up. 
        Items.Where(s => s is ICanBeTakenAndDropped).Cast<ICanBeTakenAndDropped>()
            .Aggregate(result, (s, item) =>
                s.AppendLine(item.HasEverBeenPickedUp ? item.OnTheGroundDescription : item.NeverPickedUpDescription));

        return result.ToString();
    }
}