using Model.Interface;
using Model.Item;
using Model.Location;

namespace Game;

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

        item = item.ToLowerInvariant().Trim();

        return _allItems
            .Any(i => i.Value.NounsForMatching.Any(s => s
                .Equals(item, StringComparison.OrdinalIgnoreCase)));
    }

    public static T GetItem<T>() where T : IItem, new()
    {
        if (!_allItems.ContainsKey(typeof(T)))
        {
            var item = new T();
            if (item is ICanHoldItems container)
                container.Init();
            _allItems.Add(typeof(T), item);
        }

        return (T)_allItems[typeof(T)];
    }

    public static IItem? GetItem(string noun)
    {
        noun = noun.ToLowerInvariant().Trim();
        return _allItems.Values.FirstOrDefault(i => i.HasMatchingNoun(noun));
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
}