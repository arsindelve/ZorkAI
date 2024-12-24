using Model.Item;

namespace GameEngine;

public static class IntentHelper
{
    /// <summary>
    /// Saves the caller having to get the item from the repository first.
    /// The MultiNounIntent can't do that itself because of project dependencies. 
    /// </summary>
    /// <param name="intent"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool MatchNounOne<T>(this MultiNounIntent intent) where T : IItem, new()
    {
        T item = Repository.GetItem<T>();
        return intent.MatchNounOne(item.NounsForMatching);
    }
    
    /// <summary>
    /// Saves the caller having to get the item from the repository first.
    /// The MultiNounIntent can't do that itself because of project dependencies. 
    /// </summary>
    /// <param name="intent"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool MatchNounTwo<T>(this MultiNounIntent intent) where T : IItem, new()
    {
        T item = Repository.GetItem<T>();
        return intent.MatchNounTwo(item.NounsForMatching);
    }

    /// <summary>
    /// Saves the caller having to get item two from the repository first.
    /// The MultiNounIntent can't do that itself because of project dependencies. 
    /// </summary>
    /// <param name="intent"></param>
    /// <param name="nounsOneForMatching"></param>
    /// <param name="prepositionsForMatching"></param>
    /// <param name="verbs"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNounTwo"></typeparam>
    /// <returns></returns>
    public static bool Match<TNounTwo>(this MultiNounIntent intent, string[] verbs, string[] nounsOneForMatching,
        string[] prepositionsForMatching) where TNounTwo : IItem, new()
    {
        TNounTwo itemTwo = Repository.GetItem<TNounTwo>();
        return intent.Match(verbs, nounsOneForMatching, itemTwo.NounsForMatching, prepositionsForMatching);
    }

    /// <summary>
    /// Saves the caller having to get items from the repository first.
    /// The MultiNounIntent can't do that itself because of project dependencies. 
    /// </summary>
    /// <param name="intent"></param>
    /// <param name="prepositionsForMatching"></param>
    /// <param name="verbs"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TNounTwo"></typeparam>
    /// <typeparam name="TNounOne"></typeparam>
    /// <returns></returns>
    public static bool Match<TNounOne, TNounTwo>(this MultiNounIntent intent, string[] verbs,
        string[] prepositionsForMatching) where TNounTwo : IItem, new() where TNounOne : IItem, new()
    {
        TNounOne itemOne = Repository.GetItem<TNounOne>();
        TNounTwo itemTwo = Repository.GetItem<TNounTwo>();
        return intent.Match(verbs, itemOne.NounsForMatching, itemTwo.NounsForMatching, prepositionsForMatching);
    }
}