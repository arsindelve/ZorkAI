namespace Model.Item;

/// <summary>
///     Represents an item that has a non-default response when examined or looked at.
/// </summary>
public interface ICanBeExamined : IInteractionTarget
{
    public string ExaminationDescription { get; }
}