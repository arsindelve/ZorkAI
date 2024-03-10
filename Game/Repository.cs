using Model.Item;

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
///     Some items, like the kitchen window, can exist in multiple located (outside the house and
///     in the kitchen) but it is the same window and keeps its state of open or closed.
/// </remarks>
public static class Repository
{
    private static Dictionary<Type, IItem> _allItems = new();
    private static Dictionary<Type, ILocation> _allLocations = new();

    internal static bool ItemExistsInTheStory(string? item)
    {
        if (string.IsNullOrEmpty(item))
            return false;

        return _allItems
            .Any(i => i.Value.NounsForMatching.Any(s => s
                .Equals(item, StringComparison.OrdinalIgnoreCase)));
    }

    public static T GetItem<T>() where T : IItem, new()
    {
        if (!_allItems.ContainsKey(typeof(T)))
            _allItems.Add(typeof(T), new T());

        return (T)_allItems[typeof(T)];
    }

    public static T GetLocation<T>() where T : class, ILocation, new()
    {
        if (!_allLocations.ContainsKey(typeof(T)))
            _allLocations.Add(typeof(T), new T());

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
        var instance = Activator.CreateInstance(gameEngine.StartingLocation) as ILocation;

        if (instance == null)
            throw new Exception();

        _allLocations.Add(gameEngine.StartingLocation, instance);
        return instance;
    }
}