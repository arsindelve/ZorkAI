using GameEngine;
using GameEngine.Hints;
using Model.Interface;
using Planetfall.Item.Computer;

namespace Planetfall.Hints;

/// <summary>
///     Planetfall's soft-lock rules (Docs/hints/planetfall/03) and proactive survival nudges (01 §survival).
///     The Disease rules read the game's own sickness clock (<see cref="PlanetfallContext.SicknessCounter" />,
///     which the experimental medicine rolls back), not the calendar day, so they agree with the health the
///     key state reports on the same response.
/// </summary>
internal static class PlanetfallHintRules
{
    /// <summary>Three days from the death threshold: the warning caveat.</summary>
    internal const int DiseaseWarningLevel = SleepEngine.SicknessDeathLevel - 3;

    /// <summary>The proactive nudge starts once the player is visibly ill.</summary>
    internal const int DiseaseNudgeLevel = 4;

    public static readonly IReadOnlyList<ISoftLockRule> SoftLocks = new ISoftLockRule[]
    {
        new DiseaseClockRule()
    };

    public static readonly IReadOnlyList<IProactiveRule> Proactive = new IProactiveRule[]
    {
        new TiredRule(),
        new HungerRule(),
        new DiseaseRule()
    };

    private static bool CureDone()
    {
        return Repository.GetItem<Relay>().SpeckDestroyed;
    }

    /// <summary>The Disease is a timed pressure, not a hard lock — an escalating caution while the cure is incomplete.</summary>
    private sealed class DiseaseClockRule : ISoftLockRule
    {
        public SoftLockVerdict Evaluate(IContext liveState, ProgressState progress)
        {
            if (liveState is not PlanetfallContext ctx) return SoftLockVerdict.None;
            if (CureDone()) return SoftLockVerdict.None;

            return ctx.SicknessCounter >= DiseaseWarningLevel
                ? new SoftLockVerdict(SoftLockKind.Warning,
                    "The Disease is well advanced — time is short. Make the lab and the cure your priority.")
                : SoftLockVerdict.None;
        }
    }

    private sealed class TiredRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s)
        {
            return s is PlanetfallContext c && (int)c.Tired >= 1
                ? new ProactiveNudge("sleep", "You're getting tired — find a safe place to sleep (a dorm bunk).", 3)
                : null;
        }
    }

    private sealed class HungerRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s)
        {
            return s is PlanetfallContext c && (int)c.Hunger >= 1
                ? new ProactiveNudge("hunger", "You're getting hungry and thirsty — find food and water.", 3)
                : null;
        }
    }

    private sealed class DiseaseRule : IProactiveRule
    {
        public ProactiveNudge? Evaluate(IContext s)
        {
            return s is PlanetfallContext c && c.SicknessCounter >= DiseaseNudgeLevel && !CureDone()
                ? new ProactiveNudge("disease", "You're getting sicker by the day — the cure is in the lab.", 5)
                : null;
        }
    }
}
