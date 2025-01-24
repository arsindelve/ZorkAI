using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class RandomChooser : IRandomChooser
{
    private readonly Random _rand = new();

    public T Choose<T>(List<T> items)
    {
        return items[_rand.Next(items.Count)];
    }
}