namespace Model.Interface;

/// <summary>
///     Marker interface for game contexts with a clock that can be reset by god mode for deterministic
///     playtesting. Games without a resettable clock do not implement this, so the command falls through
///     to the generic god-mode error.
/// </summary>
public interface IResettableClockContext
{
    /// <summary>
    ///     Resets the game's clock so it lands on the requested value after normal turn processing.
    /// </summary>
    void ResetClockForGodMode(int targetTime);
}
