namespace Model.Interface;

/// <summary>
/// This interface gives us a way to mock randomness in unit testing. 
/// </summary>
public interface IRandomChooser
{
    /// <summary>
    /// Selects and returns a random item from the provided list.
    /// </summary>
    /// <param name="items">The list of items from which a random selection is made. The list must contain at least one item.</param>
    /// <typeparam name="T">The type of items within the list.</typeparam>
    /// <returns>A randomly chosen item from the provided list.</returns>
    T Choose<T>(List<T> items);

    /// <summary>
    /// Simulates rolling a dice with the specified number of sides and returns the result.
    /// </summary>
    /// <param name="sides">The number of sides on the dice. Typically, this value is greater than 1.</param>
    /// <returns>The result of the dice roll, which is true if the die lands "positive".</returns>
    bool RollDice(int sides);
}