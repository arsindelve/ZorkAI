using Model.Interface;

namespace Model.Item;

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