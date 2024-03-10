namespace Model.Item;

/// <summary>
/// Represents an item that can be examined or looked at. This will be almost every item in the game. 
/// </summary>
public interface ICanBeExamined : IInteractionTarget
{
    public string ExaminationDescription { get; }
}