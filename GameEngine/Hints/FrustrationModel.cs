using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     Frustration-sensing disclosure (locked build decision §7.3): a clearly-stuck player starts further
///     down a fresh ladder rather than at the vaguest rung. The signal is read from state, so this is
///     deterministic and testable. The natural "more" advance (previous rung + 1) is the service's job;
///     this only raises the starting rung, and only for a topic the player has not been hinted on yet.
/// </summary>
public static class FrustrationModel
{
    /// <summary>Every N deaths raises the starting rung by one.</summary>
    internal const int DeathsForOneRung = 2;

    /// <summary>The minimum rung index to start a fresh topic on, given current frustration.</summary>
    public static int RungFloor(IContext state)
    {
        var deaths = Math.Max(0, state.GetDeathCount());
        return deaths / DeathsForOneRung;
    }
}
