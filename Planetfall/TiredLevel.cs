using Utilities;

namespace Planetfall;

public enum TiredLevel
{
    [Description("You seem to be well-rested.")]
    WellRested = 0,

    // 7200-8100
    [Description("You feel sort of tired. ")]
    [Notification("You begin to feel weary. It might be time to think about finding a nice safe place to sleep. ")]
    Tired = 1,

    // 7850 - 8500
    [Notification("You're really tired now. You'd better find a place to sleep real soon. ")]
    [Description("You feel quite tired. ")]
    VeryTired = 2,

    // 8650
    [Notification("If you don't get some sleep soon you'll probably drop. ")]
    [Description("You feel phenomenally tired. ")]
    Exhausted = 3,

    // > 8850
    [Description("You feel phenomenally tired. ")]
    [Notification("You can barely keep your eyes open. ")]
    AboutToDrop = 4
}