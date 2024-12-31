using Utilities;

namespace Planetfall;

public enum HungerLevel
{
    [Description("You seem to be well-fed.")]
    WellFed,

    // A growl from your stomach warns that you're getting pretty hungry and thirsty.
    [Description("You seem to be fairly thirsty and hungry.")]
    Hungry,

    [Description("You seem to be noticeably thirsty and hungry.")]
    Famished
}