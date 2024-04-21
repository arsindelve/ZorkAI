namespace Model.Item;

/// <summary>
///     Represents an item that can be turned on and off.
/// </summary>
public interface IAmALightSourceThatTurnsOnAndOff : IInteractionTarget, IAmALightSource
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

public interface ICannotBeTurnedOff : IInteractionTarget
{
    public string CannotBeTurnedOffMessage { get; }
}