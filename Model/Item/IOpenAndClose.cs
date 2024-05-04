using Model.Interface;

namespace Model.Item;

/// <summary>
///     Represents an object that can be opened and closed like a window or a mailbox.
/// </summary>
public interface IOpenAndClose : IInteractionTarget
{
    bool IsOpen { get; set; }

    string NowOpen { get; }

    string NowClosed { get; }

    string AlreadyOpen { get; }

    string AlreadyClosed { get; }

    /// <summary>
    ///     Has this item ever been opened?
    /// </summary>
    bool HasEverBeenOpened { get; set; }

    /// <summary>
    ///     Returns the description of why the item cannot be opened, or null if it can be opened.
    /// </summary>
    /// <param name="context">The context in which the method is executed.</param>
    /// <returns>The description of why the object cannot be opened, or null if it can be opened.</returns>
    string? CannotBeOpenedDescription(IContext context);
}