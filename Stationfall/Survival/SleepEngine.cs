using Stationfall.Command;

namespace Stationfall;

/// <summary>
///     Sleep and the day boundary. Days in this game advance only by sleeping — there is no daemon
///     ticking the calendar over (globals.zil, WAKING-UP) — so everything that is a function of the day
///     is re-established here.
/// </summary>
public static class SleepEngine
{
    /// <summary>
    ///     Injectable randomness, for the dream roll. Tests pin this.
    /// </summary>
    public static IRandomChooser Chooser { get; set; } = new RandomChooser();

    /// <summary>
    ///     The day on which the chronometer is found to have stopped (ship.zil:91-95). Waking into any
    ///     later day leaves it dead.
    /// </summary>
    private const int LastDayTheChronometerWorks = 2;

    /// <summary>
    ///     Runs at the top of each turn. Returns the whole sleep-and-wake narration when the player
    ///     goes under this turn, and null when they stay awake.
    /// </summary>
    public static string? CheckForSleep(StationfallContext context)
    {
        if (context.SleepNotifications.ShouldFallAsleep(context.CurrentTime))
            return DriftOff(context);

        if (context.SleepNotifications.ShouldForceSleep(context.CurrentTime, context.Tired))
            return Collapse(context);

        return null;
    }

    /// <summary>
    ///     Falling asleep the civilised way, having lain down and let the drift-off timer run out.
    /// </summary>
    private static string DriftOff(StationfallContext context)
    {
        context.SleepNotifications.CancelFallAsleep();
        return "\nYou slowly sink into a deep and blissful sleep." + Wake(context);
    }

    /// <summary>
    ///     Running out of stamina wherever you happen to be standing. The original distinguishes
    ///     collapsing in free fall from dropping to the deck; until weightless rooms exist, this is the
    ///     deck case.
    /// </summary>
    private static string Collapse(StationfallContext context)
    {
        return "\nYou can't stay awake a moment longer. You drop to the deck and fall into a deep but " +
               "fitful sleep." + Wake(context);
    }

    /// <summary>
    ///     The night itself, and the morning after.
    /// </summary>
    private static string Wake(StationfallContext context)
    {
        var message = string.Empty;

        var dream = Dreams.GetDream(Chooser);
        if (dream is not null)
            message += "\n\n" + dream;

        return message + "\n\n" + StartNewDay(context);
    }

    /// <summary>
    ///     Advances the calendar and re-establishes everything derived from it: the clock, fatigue, and
    ///     both warning schedules. Also exacts sleep's price — you wake up having let go of whatever you
    ///     were carrying.
    /// </summary>
    private static string StartNewDay(StationfallContext context)
    {
        context.Day++;

        var chronometer = Repository.GetItem<Chronometer>();

        // Set this BEFORE re-seeding the clock: a stopped chronometer must not quietly wind itself
        // forward to the new morning, or the player can tell it isn't really broken.
        if (context.Day > LastDayTheChronometerWorks)
            chronometer.HasStopped = true;

        chronometer.ResetToMorning();

        // Was the player hungry when the day turned over? Read this BEFORE the reset below clears it.
        var wokeFamished = context.Hunger > HungerLevel.WellFed;

        context.Tired = TiredLevel.WellRested;
        context.Hunger = HungerLevel.WellFed;

        // Both schedules are relative to the new morning, so they must be re-armed after the clock has
        // been re-seeded, never before.
        context.SleepNotifications.ResetForNewDay(context.CurrentTime, context.Day);
        context.HungerNotifications.NextWarningAt = context.CurrentTime +
                                                   (wokeFamished
                                                       ? HungerNotifications.FamishedMorningTicks
                                                       : HungerNotifications.RestedMorningTicks);

        // A night's sleep costs the soup too - all of it if the Thermos was left open
        // (globals.zil:1150-1155).
        var soup = Repository.GetItem<BlueSoup>();
        soup.Cool(Repository.GetItem<Thermos>().IsOpen ? soup.Warmth : BlueSoup.DegreesLostOvernight);

        DropEverythingNotWorn(context);

        var message = $"***** NOVEM {context.Day + 3}, 11349 *****\n\n";
        message += WakingDescription(context);

        if (wokeFamished)
            message += " You are also ravenously hungry. Breakfast would be the first order of business.";

        // The first night survived is worth points; later ones are not.
        if (context.Day == 2)
            context.AddPoints(3);

        return message;
    }

    /// <summary>
    ///     You do not keep hold of what you were carrying while unconscious.
    /// </summary>
    private static void DropEverythingNotWorn(StationfallContext context)
    {
        foreach (var item in context.Items.ToList())
        {
            if (item is IAmClothing { BeingWorn: true })
                continue;

            context.RemoveItem(item);
            context.CurrentLocation.ItemPlacedHere(item);
        }
    }

    private static string WakingDescription(StationfallContext context)
    {
        if (context.ItIsDarkHere)
            return "You awake in darkness.";

        // Waking in a seat you were strapped into is a good deal kinder than waking on bare decking.
        if (context.CurrentLocation.SubLocation is SeatBase)
            return "You awake feeling rested and ready for whatever this new day intends to do to you.";

        return "You awake and slowly get to your feet, stiff and sore from a night on the decking.";
    }

    /// <summary>
    ///     Starvation. Kept here beside the rest of the survival clocks rather than in the context, so
    ///     the two ways the clocks can kill you sit together.
    /// </summary>
    public static string Starve(StationfallContext context)
    {
        return new DeathProcessor()
            .Process("You collapse from extreme thirst and hunger.", context)
            .InteractionMessage;
    }
}
