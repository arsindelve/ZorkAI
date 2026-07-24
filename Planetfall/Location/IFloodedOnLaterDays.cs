namespace Planetfall.Location;

/// <summary>
///     A location on the lower cliff that the rising ocean takes away as the calendar advances - the
///     rooms whose neighbours stop routing to them and start routing to <c>Underwater</c> instead.
///     Normal play can never strand the player in one: the only way to cross a day is to sleep, and
///     sleeping down there drowns you. A god-mode calendar advance deliberately skips that death (see
///     <see cref="SleepEngine.StartNewDay" />), so it has to move the player off the room itself, which
///     is what this interface is for.
///     Each room answers for itself rather than a central list deciding on its behalf, so the rule
///     lives beside the day-conditional <c>Map</c> that already encodes the same flooding.
/// </summary>
public interface IFloodedOnLaterDays
{
    /// <summary>
    ///     The room to spill the player into once the given day has put this one under water, or null
    ///     while it is still dry. The destination must be strictly further from the water, so that
    ///     following it repeatedly terminates.
    /// </summary>
    ILocation? HigherGroundOn(int day);
}
