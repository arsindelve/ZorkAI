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
    public T Choose<T>(List<T> items)
    {
        return items[_rand.Next(items.Count)];
    }

    /// <summary>
    /// Simulates rolling a dice with a specified number of sides and determines whether the result matches the expected value.
    /// </summary>
    /// <param name="sides">The number of sides on the dice. Must be greater than 1.</param>
    /// <returns>True if the dice roll matches the expected value, otherwise false.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of sides is less than or equal to 1.</exception>
    public bool RollDice(int sides)
    {
        if (sides <= 1)
        {
            throw new ArgumentException("Number of sides must be greater than 1.", nameof(sides));
        }

        return _rand.Next(1, sides + 1) == ExpectedDieValue; 
    }
}