using Model.Interface;
using Model.Location;

namespace Model.Item;

/// <summary>
///     Represents an object that can be opened and closed like a window or a mailbox.
/// </summary>
public interface IOpenAndClose : IInteractionTarget
{
    bool IsOpen { get; set; }

    /// <summary>
    /// Tell the user that it's open now
    /// </summary>
    /// <param name="currentLocation">Why? Because of the grate in Zork One - Different behavior
    /// if you are above it (clearing) or below it (grating room). The kitchen window could
    /// also behave this way (but doesn't)</param>
    /// <returns></returns>
    string NowOpen(ILocation currentLocation);

    /// <summary>
    /// Tell the user that it's closed now
    /// </summary>
    /// <param name="currentLocation">Why? Because of the grate in Zork One - Different behavior
    /// if you are above it (clearing) or below it (grating room). The kitchen window could
    /// also behave this way (but doesn't)</param>
    /// <returns></returns>
    string NowClosed(ILocation currentLocation);

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

    /// <summary>
    /// Called when the item is opened. The return text is appended to the output. 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string OnOpening(IContext context);
}