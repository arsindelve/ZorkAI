using Model.Interface;

namespace GameEngine;

public class RandomChooser : IRandomChooser
{
    private readonly Random _rand = new();

    public T Choose<T>(List<T> items)
    {
        return items[_rand.Next(items.Count)];
    }

    public bool RollDice(int sides)
    {
        if (sides <= 1)
        {
            throw new ArgumentException("Number of sides must be greater than 1.", nameof(sides));
        }

        return _rand.Next(1, sides + 1) == 1; 
    }
}