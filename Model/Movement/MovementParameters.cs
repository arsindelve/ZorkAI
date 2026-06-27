using Model.Interface;
using Model.Item;
using Model.Location;

namespace Model.Movement;

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

    /// <summary>
    ///     Is there a limited number of items you can carry to traverse this path?
    ///     Is it a tight squeeze? By default, no, but some locations may override
    ///     this.
    /// </summary>
    public int WeightLimit { get; init; } = int.MaxValue;

    /// <summary>
    ///     If the adventurer cannot get there with what they're carrying
    ///     in their inventory (see what I did there? HA!) (me, weeks later...no I really don't), (me, months later...still don't)
    ///     what message shall we give them?
    /// </summary>
    public string WeightLimitFailureMessage { get; init; } = string.Empty;

    /// <summary>
    /// An optional message to display as we transition to this location.
    /// </summary>
    public string? TransitionMessage { get; init; } = string.Empty;

    /// <summary>
    ///     The item — a door, window, grating, bulkhead — whose state gates this passage, if any.
    ///     Declaring it lets "enter/exit &lt;door&gt;" resolve a noun to the direction that walks through
    ///     it (see <c>DoorReroute</c> / <see cref="ILocation.DirectionGatedBy" />), instead of the
    ///     engine guessing doorness from the item's type. Null for ungated passages or passages gated
    ///     by something that isn't a nameable item (a weight limit, a room flag). (issue #262)
    /// </summary>
    public IItem? GatingItem { get; init; }
}