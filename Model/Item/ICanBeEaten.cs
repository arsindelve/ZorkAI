using Model.Interface;

namespace Model.Item;

/// <summary>
///     Interface for items that can be eaten.
/// </summary>
public interface ICanBeEaten : IInteractionTarget
{
    public string OnEating(IContext context);
}

/// <summary>
///     Interface for an item that can be drunk (i.e consumed, not inebriated)
/// </summary>
public interface IAmADrink : IInteractionTarget
{
    
    public string OnDrinking(IContext context);
}