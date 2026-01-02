using Model.Item;

namespace Model.Interface;

/// <summary>
/// This interface is different than "ICanContainItems". This refers to someone or something that has hands (or
/// something like hands) and can literally hold something.
/// </summary>
public interface ICanHoldItems
{
    /// <summary>
    /// The item currently being held (not in a container, but in hand).
    /// </summary>
    IItem? ItemBeingHeld { get; set; }
}