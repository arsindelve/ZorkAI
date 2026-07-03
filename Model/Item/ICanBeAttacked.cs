using Model.Interface;

namespace Model.Item;

/// <summary>
/// Marker interface for people/creatures that can be attacked by the adventurer.
/// </summary>
public interface ICanBeAttacked
{
    /// <summary>
    /// Issue #374: puts this creature into the same "dead" state its own combat engine would leave
    /// it in after a fatal blow, for deterministic god-mode playtesting past combat gates (see
    /// GodModeProcessor.Kill). The context is needed because some creatures (e.g. the Thief) drop
    /// their stash wherever the player currently is, mirroring what a real kill does. Returns true
    /// if this creature supports being killed this way; the default (false) leaves the creature
    /// untouched, so unsupported creatures fall through to the generic god-mode error message.
    /// </summary>
    bool GodModeKill(IContext context) => false;
}