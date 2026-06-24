using Model.Interface;

namespace GameEngine.Hints;

/// <summary>
///     Frustration-sensing disclosure (locked build decision §7.3). A clearly-stuck player should
///     start further down the hint ladder rather than always from the vaguest rung. The signals are
///     read entirely from state + memory, so this is fully deterministic and testable: an eval fixture
///     sets deaths / moves / ask-count and asserts the resulting rung floor.
/// </summary>
public static class FrustrationModel
{
    // Tunable thresholds. Each crossed threshold raises the starting-rung floor by one.
    // NOTE: the natural "I need more help" advance (prev rung + 1) is handled by the service; this
    // floor only *boosts the starting rung* for a player who is clearly stuck, so ask-count is
    // deliberately NOT a term here (that would double-count the natural advance).
    internal const int DeathsForOneRung = 2;      // every N deaths -> +1 starting floor
    internal const int MovesStuckForOneRung = 30; // every N moves with no progress on the topic -> +1 floor

    /// <summary>
    ///     The minimum rung index to reveal for a topic given current frustration. The engine takes
    ///     max(this, naturally-advanced rung), then clamps to the ladder length.
    /// </summary>
    public static int RungFloor(IContext state, HintMemory memory, string topic)
    {
        var deaths = Math.Max(0, state.GetDeathCount());

        var movesStuck = 0;
        if (memory.TopicStartMove.TryGetValue(topic, out var startMove))
            movesStuck = Math.Max(0, state.Moves - startMove);

        var floor =
            deaths / DeathsForOneRung +
            movesStuck / MovesStuckForOneRung;

        return Math.Max(0, floor);
    }
}
