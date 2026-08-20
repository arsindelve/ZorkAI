namespace Stationfall;

/// <summary>
///     Marks somewhere the player can lie down and sleep on purpose, as opposed to collapsing where
///     they stand. A bed changes what a fatigue warning does: rather than nagging the player to go find
///     somewhere safe, it settles them in and starts them drifting off.
///     Implemented by rooms, and by seats and bunks that are sub-locations of a room.
/// </summary>
public interface IAmABed;
