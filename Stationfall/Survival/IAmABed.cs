namespace Stationfall;

/// <summary>
///     Marks somewhere the player can lie down and sleep on purpose, as opposed to collapsing where
///     they stand. A bed changes what a fatigue warning does: rather than nagging the player to go find
///     somewhere safe, it settles them in and starts them drifting off.
///     Implemented by rooms, and by seats and bunks that are sub-locations of a room.
/// </summary>
/// <remarks>
///     Marking a place with this is only half of wiring a bed. The other half is the action that puts
///     the player into it, and that action must start the drift-off timer explicitly:
///     <code>
///     context.SleepNotifications.QueueFallAsleep(
///         context.CurrentTime, SleepNotifications.TicksToDriftOffInBed);
///     </code>
///     The explicit argument matters. Climbing into a bed deliberately takes longer to drop off from
///     than being settled in by a fatigue warning (22 millichrons against 16, globals.zil:853 against
///     :909), and the parameterless overload gives you the shorter one. Nothing would fail if you used
///     it — the delay is intent, not observable behaviour — so it would simply be wrong and stay wrong.
/// </remarks>
public interface IAmABed;
