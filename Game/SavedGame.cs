using Model.Item;

namespace Game;

public class SavedGame<T> where T : IInfocomGame, new()
{
    public Dictionary<Type, IItem>? AllItems;
    public Dictionary<Type, ILocation>? AllLocations;
    public Context<T>? Context;
}