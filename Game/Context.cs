using Model.Item;

namespace Game;

/// <summary>
///     The "context" is anything we need to know about the state of the game that is not
///     location or item state dependant. These include the score, number of moves, adventurer inventory...
///     stuff like that. This, along with the state of all objects and locations, (stored in the <see cref="Repository" />
///     encompasses everything we need to know to save and restore the game...i.e preserve the entire game state
/// </summary>
public class Context<T> : IContext where T : IInfocomGame, new()
{
    /// <summary>
    ///     Starts the game in the default start location.
    /// </summary>
    public Context(IGameEngine engine, T gameType)
    {
        GameType = gameType;
        Engine = engine;

        CurrentLocation = Repository.GetStartingLocation<T>();
        Score = 0;
        Moves = 0;
    }

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    public Context()
    {
        CurrentLocation = Repository.GetStartingLocation<T>();
        Score = 0;
        Moves = 0;
        GameType = new T();
    }

    private T GameType { get; set; }

    public IGameEngine? Engine { get; set; }

    public string LastNoun { get; set; } = "";

    public int Moves { get; set; }

    public int Score { get; set; }

    /// <summary>
    ///     Gets or sets the current location of the player in the game.
    /// </summary>
    /// <value>The current location.</value>
    public ILocation CurrentLocation { get; set; }

    /// <summary>
    ///     Represents the inventory of the game's adventurer. It holds various items that the player can carry.
    /// </summary>
    public List<IItem> Items { get; } = new();

    List<ITurnBasedActor> IContext.GetActors()
    {
        return Items.OfType<ITurnBasedActor>().ToList();
    }

    /// <summary>
    ///     Gets a value indicating whether the adventurer has a light source that is on
    /// </summary>
    /// <returns><c>true</c> if the context has a light source; otherwise, <c>false</c>.</returns>
    public bool HasLightSource
    {
        get
        {
            var constantLightSources = Items
                .Where(s => s is IAmALightSource)
                .Where(s => s is ICannotBeTurnedOff);

            var lightSourcesThatAreOn = Items
                .Where(s => s is IAmALightSource)
                .Where(s => s is ICanBeTurnedOnAndOff { IsOn: true });

            return constantLightSources.Any() || lightSourcesThatAreOn.Any();
        }
    }

    public string ItemListDescription(string name)
    {
        if (!Items.Any())
            return "You are empty-handed";

        var sb = new StringBuilder();
        sb.AppendLine("You are carrying:");
        Items.ForEach(s => sb.AppendLine($"   {s.InInventoryDescription}"));

        return sb.ToString();
    }

    public IInfocomGame Game
    {
        get => GameType;
        set => GameType = (T)value;
    }

    public string? LastSaveGameName { get; set; } = "";

    public bool IsTransparent => true;

    public string Name => "Inventory";

    public void RemoveItem(IItem item)
    {
        Items.Remove(item);
    }

    public void ItemPlacedHere(IItem item)
    {
        Take(item);
    }

    /// <summary>
    ///     Checks if a location has an item of type T.
    /// </summary>
    /// <typeparam name="TItem">The type of item to check.</typeparam>
    /// <returns>True if the location has an item of type T, otherwise false.</returns>
    public bool HasItem<TItem>() where TItem : IItem, new()
    {
        return Items.Contains(Repository.GetItem<TItem>());
    }

    public bool HasMatchingNoun(string? noun, bool lookInsideContainers = true)
    {
        var hasMatch = false;
        Items.ForEach(i => hasMatch |= i.HasMatchingNoun(noun, lookInsideContainers));

        return hasMatch;
    }

    public bool HaveRoomForItem(IItem item)
    {
        // TODO: Implement inventory item load; 
        return true;
    }

    public void Init()
    {
        // We start empty-handed
    }

    public void OnItemPlacedHere(IItem item, IContext context)
    {
    }

    public int AddPoints(int points)
    {
        Score += points;
        return Score;
    }

    /// <summary>
    ///     Adds the specified item to the inventory of the game context and removes it from the current location
    /// </summary>
    /// <param name="item">The item to add to the inventory.</param>
    /// <exception cref="Exception">Thrown if a null item is added to the inventory.</exception>
    public void Take(IItem? item)
    {
        if (item == null)
            throw new Exception("Null item was added to inventory");

        Items.Add(item);
        var previousOwner = item.CurrentLocation;
        previousOwner?.RemoveItem(item);
        item.CurrentLocation = this;
        item.HasEverBeenPickedUp = true;
    }

    /// <summary>
    ///     Drops an item from the inventory to the current location.
    /// </summary>
    /// <param name="item">The item to be dropped.</param>
    /// <exception cref="Exception">Thrown when the item is null or the current location cannot hold the item.</exception>
    public void Drop(IItem item)
    {
        if (item == null)
            throw new Exception("Null item was dropped from inventory");

        if (CurrentLocation is not ICanHoldItems newLocation)
            throw new Exception("Current location can't hold item");

        Items.Remove(item);
        item.CurrentLocation = newLocation;
        newLocation.ItemPlacedHere(item);
    }

    /// <summary>
    ///     We've determined that the user's input is a <see cref="SimpleIntent" /> which consists
    ///     of a noun and a verb. Does this have any consequence with the Context itself? Usually
    ///     this is only true where there's an interaction with an item in inventory.
    /// </summary>
    /// <param name="action">The simple interaction that the context needs to respond to.</param>
    /// <param name="client"></param>
    public InteractionResult RespondToSimpleInteraction(SimpleIntent action, IGenerationClient client)
    {
        InteractionResult? result = null;

        foreach (var item in Items.ToList().TakeWhile(_ => result is null or NoNounMatchInteractionResult))
            result = item.RespondToSimpleInteraction(action, this, client);

        return result ?? new NoNounMatchInteractionResult();
    }

    public virtual string? ProcessTurnCounter()
    {
        Moves++;
        return null;
    }
}