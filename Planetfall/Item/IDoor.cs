namespace Planetfall.Item;

/// <summary>
///     A door that owns the wording for its own open/closed state.
///     <para>
///         The point of <see cref="DescribeAs" /> taking the state as a parameter rather than reading
///         <see cref="IOpenAndClose.IsOpen" /> is that a room sometimes knows the effective state better
///         than the raw flag does — the Elevator Lobby is the standing example, where one OPENBIT flag is
///         shared by both ends of a shaft and only means "open here" when the car is parked here (#505).
///         Such a room needs the door's phrasing without the door's answer, and this is how it gets it
///         instead of copying the literal. <see cref="Doorway" /> is the intended caller.
///     </para>
/// </summary>
public interface IDoor : IItem, IOpenAndClose, ICanBeExamined
{
    /// <summary>
    ///     How this door words the given state, e.g. "The door is closed. ". Implementations should define
    ///     <see cref="ICanBeExamined.ExaminationDescription" /> as <c>DescribeAs(IsOpen)</c> so that
    ///     examining the door and reading about it in a room description can never phrase it differently.
    /// </summary>
    string DescribeAs(bool isOpen);
}
