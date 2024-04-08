namespace Model.Item;

/// <summary>
///     Represents an item that can be examined or looked at. This will be almost every item in the game.
/// </summary>
public interface ICanBeExamined : IInteractionTarget
{
    // TODO: We need a default implementation of "examine" and to only
    // use this interface when something has a unique response. The vast
    // majority of objects say "There is nothing special about the 'x'"
    public string ExaminationDescription { get; }
}