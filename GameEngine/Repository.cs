using System.Reflection;
using GameEngine.Item;
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

    private static string[] _allNouns = [];
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
            if (item is ICanHoldItems container)
                container.Init();
            _allItems.Add(typeof(T), item);
        }

        return (T)_allItems[typeof(T)];
    }

    public static IItem? GetItem(string? noun)
    {
        if (string.IsNullOrEmpty(noun))
            return null;
        noun = noun.ToLowerInvariant().Trim();
        return _allItems.Values.FirstOrDefault(i => i.HasMatchingNoun(noun).HasItem);
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

    /// <summary>
    /// For unit testing purposes only. 
    /// </summary>
    /// <returns></returns>
    public static string[] GetNouns(string gameName = "ZorkOne")
    {
        if (_allNouns.Length > 0) return _allNouns;

        lock (_allNouns)
        {
            var allItems = new List<ItemBase>();
            var assembly = Assembly.Load(gameName);

            var types = assembly.GetTypes();

            foreach (var type in types)

                if (type is { IsClass: true, IsGenericType: false, IsAbstract: false } && type.IsSubclassOf(typeof(ItemBase)))
                {
                    var instance = (ItemBase)Activator.CreateInstance(type)!;
                    allItems.Add(instance);
                }

            _allNouns = allItems.SelectMany(s => s.NounsForMatching).ToArray();
            return _allNouns;
        }
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