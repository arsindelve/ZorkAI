using Model.Item;

namespace Game;

public class SavedGame<T> where T : IContext, new()
{
    public Dictionary<Type, IItem>? AllItems;
    public Dictionary<Type, ILocation>? AllLocations;
    public T? Context;
}