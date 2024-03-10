namespace Model.Item;

/// <summary>
/// Interface for items that can be eaten. 
/// </summary>
public interface ICanBeEaten : IInteractionTarget
{
    public string EatenDescription { get; }
}