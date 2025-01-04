using Model.Interface;

namespace Model.Item;

public interface ICannotBeTurnedOff : IInteractionTarget
{
    public string CannotBeTurnedOffMessage { get; }
}