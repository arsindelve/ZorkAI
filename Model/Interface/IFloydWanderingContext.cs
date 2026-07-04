namespace Model.Interface;

/// <summary>
///     Marker interface for game contexts whose companion NPC (Floyd, in Planetfall) can randomly
///     wander off. Games with this mechanic implement this so god mode can suppress the random
///     wandering triggers for deterministic playtesting; games without it (Zork) do not.
/// </summary>
public interface IFloydWanderingContext
{
    /// <summary>
    ///     When true, Floyd's two random wandering triggers - failing to follow when the player moves
    ///     to a new location, and spontaneously wandering off mid-conversation - are suppressed. Does
    ///     NOT affect scripted/deliberate wandering (e.g. Floyd storming off when upset), which is not
    ///     random and should still occur. Purely a debug/testing affordance.
    /// </summary>
    bool FloydWanderingDisabled { get; set; }
}
