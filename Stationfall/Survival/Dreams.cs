namespace Stationfall;

/// <summary>
///     The dreams the player may have while asleep. The original prints one most nights
///     (globals.zil:1010) purely as colour — nothing here changes the state of the game.
///     Written fresh for this port rather than lifted from the original.
/// </summary>
public static class Dreams
{
    /// <summary>
    ///     Percentage chance of dreaming at all on a given night (globals.zil:1010).
    /// </summary>
    private const int ChanceOfDreaming = 60;

    private static readonly List<string> All =
    [
        "...you are back on Resida, walking its empty streets in the flat grey light before dawn. " +
        "Every door stands open. You keep meaning to ask someone the way, and keep failing to find " +
        "anyone to ask...",

        "...you are at your desk aboard the Duffy, and the stack of forms in your in-tray has begun " +
        "to grow faster than you can sign them. You work faster. The stack works faster still. " +
        "Somewhere behind you a superior officer clears his throat...",

        "...you are being presented with a medal, in a hall so vast you cannot see the back of it. " +
        "The admiral pins it to your chest, shakes your hand warmly, and asks whether you have " +
        "remembered to file the paperwork for the medal. You have not...",

        "...you are small again, on Gallium, lying in the long grass behind your parents' house and " +
        "watching the freighters climb away toward the stars. You had promised yourself you would be " +
        "aboard one of them. It had not occurred to you to ask what the work would be like...",

        "...you are running, unhurried and unafraid, down a corridor that has no doors in it at all. " +
        "You have the strong impression that this is important, and that you will understand why in " +
        "a moment..."
    ];

    /// <summary>
    ///     A dream, or null on a dreamless night.
    /// </summary>
    public static string? GetDream(IRandomChooser chooser)
    {
        return chooser.RollDice(100) <= ChanceOfDreaming ? chooser.Choose(All) : null;
    }
}
