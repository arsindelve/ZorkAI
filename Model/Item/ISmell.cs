using Model.Interface;

namespace Model.Item;

/// <summary>
///     Represents an item that can emit a smell and has a non-default response when interacted with.
/// </summary>
public interface ISmell : IInteractionTarget
{
    public string SmellDescription { get; }
}