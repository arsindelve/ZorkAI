using Utilities;

namespace Planetfall;

public enum SicknessLevel
{
    [Description("You are in perfect health. ")]
    PerfectHealth = 1,

    [Description("You are in perfect health. ")]
    PrettyGood = 2,

    [Description("You feel a bit sick and feverish. ")]
    Meh = 3,

    [Description("You feel a bit sick and feverish. ")]
    NotGreat = 4,
    
    [Description("You are somewhat sick and feverish. ")]
    Sick = 5,

    [Description("You are somewhat sick and feverish. ")]
    Sicker = 6,

    [Description("You are very sick and feverish. ")]
    ReallySick = 7,

    [Description("You are very sick and feverish. ")]
    AlmostDead = 8
}