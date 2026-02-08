using Model.Interface;

namespace Model.Item;

/// <summary>
/// Represents an interface for a target that can be turned on or off, such as a light source or device.
/// </summary>
[ApplicableVerbs("turn on", "turn off")]
public interface ITurnOffAndOn : IInteractionTarget
{
    bool IsOn { get; set; }

    public string NowOnText { get; }

    public string NowOffText { get; }

    public string AlreadyOffText { get; }

    public string AlreadyOnText { get; }

    string? CannotBeTurnedOnText { get; }

    string OnBeingTurnedOn(IContext context);

    void OnBeingTurnedOff(IContext context);
}