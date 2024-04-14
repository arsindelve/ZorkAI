using Newtonsoft.Json;

namespace Utilities;

/// <summary>
///     Represents a limited-size stack - once you have the maximum number of items, pushing another
///     item deletes the oldest.
/// </summary>
/// <typeparam name="T">The type of elements in the stack.</typeparam>
public class LimitedStack<T>
{
    private const int MaxCount = 5;
    
    [JsonProperty]
    private readonly LinkedList<T> _list = new();

    public void Push(T item)
    {
        _list.AddLast(item);
        if (_list.Count > MaxCount) _list.RemoveFirst();
    }

    public List<T> GetAll()
    {
        return _list.ToList();
    }

    public T? Peek()
    {
        if (_list.Count == 0)
            return default;

        return _list.Last == null ? default : _list.Last.Value;
    }
}