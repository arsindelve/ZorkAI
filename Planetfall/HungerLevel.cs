using Utilities;

namespace Planetfall;

public enum HungerLevel
{
    [Description("You seem to be well-fed.")]
    WellFed = 0,

    [Notification("A growl from your stomach warns that you're getting pretty hungry and thirsty. ")]
    [Description("You seem to be fairly thirsty and hungry.")]
    Hungry = 1,

    [Notification("You're now really ravenous and your lips are quite parched. ")]
    [Description("You seem to be ravenous and parched.")]
    Ravenous = 2,

    [Notification("You're starting to feel faint from lack of food and liquid. ")]
    [Description("You feel faint from lack of food and liquid.")]
    Faint = 3,

    [Notification("If you don't eat or drink something in a few millichrons, you'll probably pass out. ")]
    [Description("You are about to pass out from hunger and thirst.")]
    AboutToPassOut = 4,

    [Description("You have died from hunger and thirst.")]
    Dead = 5
}