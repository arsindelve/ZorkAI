using System.Reflection;
using GameEngine.Item;
using GameEngine.Location;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace GameEngine;

/// <summary>
///     This is a "lazy-load" database of every item and location that has been loaded in the game.
///     They are loaded (instantiated) the first time they are asked for. Every location and item
///     in the game are singletons - i.e. there can be no duplicates. There is only one "sword"
///     and one "west of house". This repository assures that every location and item can only exist
///     once.
/// </summary>
/// <remarks>
///     To make this work, ALL requests for an item or a location MUST go through the repository.
///     They must never be instantiated outside of here, otherwise they will not be tracked
///     and will effectively be a duplicate.
///     Some items, like the kitchen window, can exist in multiple locations (outside the house and
///     in the kitchen) but it is the same window and keeps its state of open or closed.
/// </remarks>
public static class Repository
{
    private static Dictionary<Type, IItem> _allItems = new();
    private static Dictionary<Type, ILocation> _allLocations = new();

    private static string[] _allNouns = Array.Empty<string>();
    private static string[] _allContainers = [];

    internal static SavedGame<T> Save<T>() where T : IContext, new()
    {
        return new SavedGame<T>
        {
            AllLocations = new Dictionary<Type, ILocation>(_allLocations),
            AllItems = new Dictionary<Type, IItem>(_allItems)
        };
    }

    internal static bool ItemExistsInTheStory(string? item)
    {
        if (string.IsNullOrEmpty(item))
            return false;

        return GetItem(item) is not null;
    }

    public static T GetItem<T>() where T : IItem, new()
    {
        if (!_allItems.ContainsKey(typeof(T)))
        {
            var item = new T();
            if (item is ICanContainItems container)
                container.Init();
            _allItems.Add(typeof(T), item);
        }

        return (T)_allItems[typeof(T)];
    }

    /// <summary>
    /// Searches for an item by noun in the player's inventory only.
    /// This is the preferred method for drop operations since you can only drop what you're carrying.
    /// </summary>
    public static IItem? GetItemInInventory(string? noun, IContext context)
    {
        if (string.IsNullOrEmpty(noun))
            return null;

        noun = noun.ToLowerInvariant().Trim();

        var (hasMatch, item) = context.HasMatchingNoun(noun, lookInsideContainers: true);
        return hasMatch ? item : null;
    }

    /// <summary>
    /// Searches for an item by noun in the player's scope (inventory, current location, and containers).
    /// This is the preferred method for player-initiated actions like "take" or "examine".
    /// Prioritizes items in the current location over items in other locations to avoid ambiguity.
    /// </summary>
    public static IItem? GetItemInScope(string? noun, IContext context)
    {
        if (string.IsNullOrEmpty(noun))
            return null;

        noun = noun.ToLowerInvariant().Trim();

        // First search in the current room (including all nested containers)
        if (context.CurrentLocation != null)
        {
            var (found, item) = context.CurrentLocation.HasMatchingNoun(noun, lookInsideContainers: true);
            if (found && item != null)
            {
                // Validate that the item is accessible (walk up the hierarchy)
                if (IsItemAccessible(item, context))
                    return item;
            }
        }

        // Then search in inventory (including all nested containers)
        var (hasMatch, matchedItem) = context.HasMatchingNoun(noun, lookInsideContainers: true);
        if (hasMatch && matchedItem != null)
        {
            // Validate that the item is accessible (walk up the hierarchy)
            if (IsItemAccessible(matchedItem, context))
                return matchedItem;
        }

        return null;
    }

    /// <summary>
    /// Checks if an item is accessible by walking up its containment hierarchy.
    /// An item is accessible if all containers in the chain are either open or transparent,
    /// and the highest level parent is in the room or inventory.
    /// </summary>
    private static bool IsItemAccessible(IItem item, IContext context)
    {
        // If the item is directly in the room or inventory, it's accessible
        if (item.CurrentLocation == context.CurrentLocation || item.CurrentLocation == context)
            return true;

        // Walk up the hierarchy checking accessibility at each level
        var current = item.CurrentLocation;

        while (current is IItem holderItem)
        {
            // Check if the holder is in the room or inventory
            if (holderItem.CurrentLocation == context.CurrentLocation || holderItem.CurrentLocation == context)
            {
                // For ICanHoldItems (like Floyd), held items are always accessible
                if (holderItem is ICanHoldItems)
                    return true;

                // For ICanContainItems (like boxes), check if accessible
                if (holderItem is ICanContainItems container)
                {
                    bool isAccessible = container.IsTransparent ||
                                        (container is IOpenAndClose openable && openable.IsOpen);

                    return isAccessible;
                }

                // Item is in scope
                return true;
            }

            // Move up to the next level in the hierarchy
            current = holderItem.CurrentLocation;
        }

        // Item is not in scope
        return false;
    }

    /// <summary>
    /// Searches globally for any item matching the noun.
    /// WARNING: This can return unpredictable results when multiple items share the same noun.
    /// Prefer GetItemInScope for player-initiated actions.
    /// </summary>
    public static IItem? GetItem(string? noun)
    {
        if (string.IsNullOrEmpty(noun))
            return null;
        noun = noun.ToLowerInvariant().Trim();
        return _allItems.Values.FirstOrDefault(i => i.HasMatchingNoun(noun, false).HasItem);
    }

    public static ILocation? GetLocation(string? noun)
    {
        if (string.IsNullOrEmpty(noun))
            return null;
        noun = noun.ToLowerInvariant().Trim();
        return _allLocations.Values.FirstOrDefault(i => i.Name.ToLowerInvariant().Equals(noun));
    }

    public static T GetLocation<T>() where T : class, ILocation, new()
    {
        if (_allLocations.ContainsKey(typeof(T)))
            return (T)_allLocations[typeof(T)];

        var location = new T();
        location.Init();
        _allLocations.Add(typeof(T), location);
        return (T)_allLocations[typeof(T)];
    }

    /// <summary>
    ///     For cleaning up after unit testing.
    /// </summary>
    internal static void Reset()
    {
        _allLocations = new Dictionary<Type, ILocation>();
        _allItems = new Dictionary<Type, IItem>();
    }

    /// <summary>
    ///     For debugging purposes - returns all items currently in the repository.
    /// </summary>
    public static IEnumerable<IItem> GetAllItems()
    {
        return _allItems.Values;
    }

    public static ILocation GetStartingLocation<T>() where T : IInfocomGame, new()
    {
        Reset();
        var gameEngine = new T();

        if (Activator.CreateInstance(gameEngine.StartingLocation) is not ILocation instance)
            throw new Exception();

        _allLocations.Add(gameEngine.StartingLocation, instance);
        return instance;
    }

    internal static void Restore(Dictionary<Type, IItem> allItems, Dictionary<Type, ILocation> allLocations)
    {
        _allLocations = allLocations;
        _allItems = allItems;
    }

    /// <summary>
    /// For unit testing purposes only. 
    /// </summary>
    /// <returns></returns>
    public static string[] GetNouns(string gameName = "ZorkOne")
    {
        lock (_allNouns)
        {
            var allItems = new List<ItemBase>();
            var assembly = Assembly.Load(gameName);

            var types = assembly.GetTypes();

            foreach (var type in types)

                if (type is { IsClass: true, IsGenericType: false, IsAbstract: false } &&
                    type.IsSubclassOf(typeof(ItemBase)))
                {
                    var instance = (ItemBase)Activator.CreateInstance(type)!;
                    allItems.Add(instance);
                }

            _allNouns = allItems.SelectMany(s => s.NounsForMatching).ToArray();
            return _allNouns;
        }
    }

    /// <summary>
    /// For god mode purposes only. 
    /// </summary>
    /// <returns></returns>
    public static void LoadAllLocations(string gameName = "ZorkOne")
    {
        lock (_allLocations)
        {
            _allLocations = new Dictionary<Type, ILocation>();
            var types = Assembly.Load(gameName).GetTypes();

            foreach (var type in types)

                if (type is { IsClass: true, IsGenericType: false, IsAbstract: false } &&
                    type.IsSubclassOf(typeof(LocationBase)))
                {
                    var instance = (LocationBase)Activator.CreateInstance(type)!;
                    _allLocations.Add(type, instance);
                }
        }
    }

    /// <summary>
    /// For god mode purposes only. 
    /// </summary>
    /// <returns></returns>
    public static void LoadAllItems(string gameName = "ZorkOne")
    {
        lock (_allItems)
        {
            _allItems = new Dictionary<Type, IItem>();
            var types = Assembly.Load(gameName).GetTypes();

            foreach (var type in types)

                if (type is { IsClass: true, IsGenericType: false, IsAbstract: false } &&
                    type.IsSubclassOf(typeof(ItemBase)))
                {
                    var instance = (ItemBase)Activator.CreateInstance(type)!;
                    _allItems.Add(type, instance);
                }
        }
    }

    /// <summary>
    /// Removes an item of the specified type from its current location,
    /// detaching it by setting its current location to null.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the item to be destroyed, which must implement the IItem interface and have a parameterless constructor.
    /// </typeparam>
    public static void DestroyItem<T>() where T : IItem, new()
    {
        var item = GetItem<T>();
        var currentLocation = item.CurrentLocation;
        currentLocation?.RemoveItem(item);
        item.CurrentLocation = null;
    }
    
    public static void DestroyItem(IItem item)
    {
        var currentLocation = item.CurrentLocation;
        currentLocation?.RemoveItem(item);
        item.CurrentLocation = null;
    }

    /// <summary>
    /// For unit testing purposes only. 
    /// </summary>
    /// <returns></returns>
    public static string[] GetContainers(string gameName = "ZorkOne")
    {
        if (_allContainers.Length > 0) return _allContainers;

        lock (_allContainers)
        {
            var allItems = new List<ItemBase>();
            var assembly = Assembly.Load(gameName);

            var types = assembly.GetTypes();

            foreach (var type in types)

                if (type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(ContainerBase)))
                {
                    var instance = (ItemBase)Activator.CreateInstance(type)!;
                    allItems.Add(instance);
                }

            _allContainers = allItems.SelectMany(s => s.NounsForMatching).ToArray();
            return _allContainers;
        }
    }
}