using Model.Interface;

namespace Model.Item;

/// <summary>
///     Interface for items that can be read, like a book or a leaflet.
/// </summary>
public interface ICanBeRead : IInteractionTarget
{
    public string ReadDescription { get; }
}