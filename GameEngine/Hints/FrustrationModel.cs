using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     Frustration-sensing disclosure (locked build decision §7.3): a clearly-stuck player starts a fresh
///     ladder further down rather than at the vaguest rung. The signal is read from state, so this is
///     deterministic and testable. The natural "more" advance (previous rung + 1) is the service's job;
///     this only raises the starting rung, only for a topic the player has not been hinted on yet, and
///     never as far as the solution rung — the death count is a lifetime figure, and dying a lot early
///     should not turn every later puzzle's first hint into the answer.
/// </summary>
public static class FrustrationModel
{
    /// <summary>Every N deaths raises the starting rung by one.</summary>
    internal const int DeathsForOneRung = 2;

    /// <summary>The rung index to start a fresh topic on, given current frustration and the ladder's length.</summary>
    public static int RungFloor(IContext state, int rungCount)
    {
        var deaths = Math.Max(0, state.GetDeathCount());
        var highestAllowed = Math.Max(0, rungCount - 2); // the solution rung is never a starting point
        return Math.Min(deaths / DeathsForOneRung, highestAllowed);
    }
}
