using Model.Interface;

namespace Model.Item;

/// <summary>
///     Interface for an item that can be drunk (i.e consumed, not inebriated)
/// </summary>
[ApplicableVerbs("drink")]
public interface IAmADrink : IInteractionTarget
{
    /// <summary>
    /// Called when the player attempts to drink this item.
    /// </summary>
    /// <param name="context">The game context.</param>
    /// <returns>A tuple containing the message to display and whether the item was consumed.
    /// If WasConsumed is false, the item remains in the game world.</returns>
    public (string Message, bool WasConsumed) OnDrinking(IContext context);
}