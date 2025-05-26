using Model.Interface;

namespace GameEngine;

/// <summary>
/// Provides random selection and random number rolling mechanics for use in game engines.
/// Implements the <see cref="Model.Interface.IRandomChooser"/> interface to allow for testable randomness in unit tests.
/// </summary>
public class RandomChooser : IRandomChooser
{
    private readonly Random _rand = new();
    private const int ExpectedDieValue = 1;

    /// <summary>
    /// Selects a random item from a list of items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="items">The list of items to choose from.</param>
    /// <returns>A randomly selected item from the list.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the items list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the items list is empty.</exception>
    public T Choose<T>(List<T> items)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items), "Items list cannot be null.");
        }
        
        if (items.Count == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(items), "Items list cannot be empty.");
        }
        
        return items[_rand.Next(items.Count)];
    }
    
    /// <summary>
    /// Simulates rolling a dice with a specified number of sides and determines whether the result matches the expected value.
    /// </summary>
    /// <param name="sides">The number of sides on the dice. Must be greater than 1.</param>
    /// <returns>True if the dice roll matches the expected value, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of sides is less than or equal to 1.</exception>
    public bool RollDiceSuccess(int sides)
    {
        if (sides <= 1)
        {
            throw new ArgumentException("Number of sides must be greater than 1.", nameof(sides));
        }
    
        return _rand.Next(1, sides + 1) == ExpectedDieValue; 
    }
    
    /// <summary>
    /// Simulates rolling a dice with the specified number of sides and returns the result.
    /// </summary>
    /// <param name="sides">The number of sides on the dice. Must be greater than 1.</param>
    /// <returns>An integer representing the result of the dice roll, ranging from 1 to the number of sides.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the number of sides is less than or equal to 1.</exception>
    public int RollDice(int sides)
    {
        if (sides <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sides), "Number of sides must be greater than 1.");
        }
        
        return _rand.Next(1, sides + 1);
    }
}