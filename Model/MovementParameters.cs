using Model.Location;

namespace Model;

/// <summary>
///     This class tells us about movement from one location to another. Is it possible? Under
///     what conditions?
/// </summary>
public record MovementParameters
{
    /// <summary>
    ///     A function which will execute to tell us, based on the current state of the game, whether
    ///     or not we can move in this direction.
    /// </summary>
    /// <remarks>
    ///     The default is true because usually if no movement is permitted, we simply do not create
    ///     any <see cref="MovementParameters" /> object at all - the absence of record for that direction will
    ///     prevent movement in that direction.
    /// </remarks>
    public Func<IContext, bool> CanGo { get; init; } = _ => true;

    /// <summary>
    ///     Which location are you trying to go? This can be omitted if <see cref="CanGo" /> is always false.
    ///     Use this scenario when you want to provide a non-generic explanation for why they can't go
    ///     that way.
    /// </summary>
    public ILocation? Location { get; init; }

    /// <summary>
    ///     If <see cref="CanGo" /> resolves to false, this message will be displayed instead of the typical
    ///     hard-coded "You cannot go that way" message.
    ///     <example>
    ///         "The kitchen window is closed."
    ///     </example>
    /// </summary>
    public string? CustomFailureMessage { get; init; }
}