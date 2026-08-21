using GameEngine;
using Model.Interface;

namespace Stationfall;

/// <summary>
///     Game state for Stationfall. The chronometer reading lives on the <see cref="Chronometer" /> item
///     (mirroring Planetfall) and is surfaced here for the engine's time-aware plumbing; the autopilot
///     course is derived from it, so tests can pin the item's value and get a reproducible heading.
///     The survival clocks run off the same reading — see <see cref="SleepEngine" />.
/// </summary>
public class StationfallContext : Context<StationfallGame>, ITimeBasedContext
{
    /// <summary>
    ///     Millichrons a turn costs unless the action says otherwise (misc.zil:381).
    /// </summary>
    public const int TurnTimeIncrement = 7;

    /// <summary>
    ///     What this turn costs in millichrons. The original gives actions individual prices — a meal,
    ///     a long walk or an elevator ride all cost more than a glance around — by setting C-ELAPSED and
    ///     letting it fall back to the default afterwards. An action that takes longer than usual sets
    ///     this; <see cref="ProcessEndOfTurn" /> spends it and resets it. This is not cosmetic: it is
    ///     what the survival clocks actually consume, so flattening every action to the same cost would
    ///     change how much time the player really has.
    /// </summary>
    [UsedImplicitly]
    public int ElapsedThisTurn { get; set; } = TurnTimeIncrement;

    // Days advance ONLY by sleeping in the original (WAKING-UP, globals.zil:1059) - there is no daemon
    // ticking the calendar over.
    [UsedImplicitly]
    public int Day { get; set; } = 1;

    /// <summary>
    ///     Number of times the player has died, preserved across death restarts.
    /// </summary>
    [UsedImplicitly]
    public int DeathCounter { get; set; }

    [UsedImplicitly] public HungerLevel Hunger { get; set; } = HungerLevel.WellFed;

    [UsedImplicitly] public TiredLevel Tired { get; set; } = TiredLevel.WellRested;

    [UsedImplicitly] public HungerNotifications HungerNotifications { get; set; } = new();

    [UsedImplicitly] public SleepNotifications SleepNotifications { get; set; } = new();

    /// <summary>
    ///     True on a turn the player spent unconscious, so nothing else tries to narrate over it.
    /// </summary>
    [UsedImplicitly]
    public bool SleepJustOccurred { get; set; }

    /// <summary>
    ///     Whether the player is lying somewhere they could deliberately sleep, rather than merely
    ///     standing somewhere they might collapse.
    /// </summary>
    public bool PlayerIsInBed =>
        CurrentLocation is IAmABed || CurrentLocation.SubLocation is IAmABed;

    public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

    public string CurrentTimeResponse
    {
        get
        {
            var chronometer = Repository.GetItem<Chronometer>();

            if (!chronometer.BeingWorn)
                return "You aren't wearing your chronometer. ";

            return chronometer.HasStopped
                ? "Your chronometer appears to have stopped. "
                : $"According to your chronometer, the current time is {CurrentTime}. ";
        }
    }

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    /// <summary>
    ///     The player begins carrying the three Patrol forms (physical feelies in the original, so they
    ///     are in inventory rather than lying in a room — ship.zil:36-61), plus the worn chronometer and
    ///     uniform.
    /// </summary>
    public override void Init()
    {
        StartWithItem<Chronometer>(this);
        StartWithItem<PatrolUniform>(this);
        StartWithItem<AssignmentCompletionForm>(this);
        StartWithItem<RobotUseAuthorizationForm>(this);
        StartWithItem<ClassThreeSpacecraftActivationForm>(this);

        // Both clocks are relative to the current reading, so they can only be armed once the
        // chronometer exists.
        HungerNotifications.Initialize(CurrentTime);
        SleepNotifications.Initialize(CurrentTime);
    }

    /// <summary>
    ///     Whether the player has any appetite. The original refuses every meal while the hunger ladder
    ///     is at zero (verbs.zil:686-688), which matters because food here is finite: without the guard
    ///     a player could eat their whole supply on a full stomach and starve later holding nothing.
    /// </summary>
    public bool IsHungry => Hunger > HungerLevel.WellFed;

    /// <summary>
    ///     What the player is told when they try to eat with no appetite.
    /// </summary>
    public const string NotHungryMessage = "You're not the least bit hungry or thirsty just now. ";

    /// <summary>
    ///     A meal. Clears the hunger ladder and buys the player a fixed stretch of not having to think
    ///     about it; the turn itself also costs more than a normal one (verbs.zil:690).
    /// </summary>
    public void Eat()
    {
        Hunger = HungerLevel.WellFed;
        HungerNotifications.ResetAfterEating(CurrentTime, HungerNotifications.TicksBoughtByAMeal);
        ElapsedThisTurn = TimeToEatAMeal;
    }

    /// <summary>
    ///     Millichrons a meal costs (verbs.zil:690).
    /// </summary>
    private const int TimeToEatAMeal = 15;

    /// <summary>
    ///     Runs the survival clocks before the player's own command. Sleep is handled first and
    ///     short-circuits the turn: it has already moved time on and dropped what the player was
    ///     carrying, so running their command afterwards would run it against stale state.
    /// </summary>
    public override string? ProcessBeginningOfTurn()
    {
        SleepJustOccurred = false;

        var sleepMessage = SleepEngine.CheckForSleep(this);
        if (!string.IsNullOrEmpty(sleepMessage))
        {
            SleepJustOccurred = true;
            TurnConsumedByForcedEvent = true;
            return sleepMessage + base.ProcessBeginningOfTurn();
        }

        var messages = string.Empty;

        var nextTiredLevel = SleepNotifications.GetNextTiredLevel(CurrentTime, Tired);
        if (nextTiredLevel.HasValue)
        {
            // Compute the message BEFORE advancing the level: GetNotification reads the current level
            // and reschedules from it, while the settling-in path deliberately does neither.
            var settleMessage = SleepNotifications.TrySettleIntoBed(CurrentTime, PlayerIsInBed);
            var sleepNotification = settleMessage ?? SleepNotifications.GetNotification(CurrentTime, Tired);

            Tired = nextTiredLevel.Value;

            if (!string.IsNullOrEmpty(sleepNotification))
                messages += sleepNotification;
        }

        var nextHungerLevel = HungerNotifications.GetNextHungerLevel(CurrentTime, Hunger);
        if (nextHungerLevel.HasValue)
        {
            var hungerNotification = HungerNotifications.GetNotification(CurrentTime, Hunger);

            Hunger = nextHungerLevel.Value;

            if (Hunger == HungerLevel.Dead)
                return messages + "\n" + SleepEngine.Starve(this);

            if (!string.IsNullOrEmpty(hungerNotification))
            {
                if (!string.IsNullOrEmpty(messages))
                    messages += "\n";

                messages += hungerNotification;
            }
        }

        return messages + base.ProcessBeginningOfTurn();
    }

    public override string? ProcessEndOfTurn()
    {
        var chronometer = Repository.GetItem<Chronometer>();

        // A stopped chronometer does not stop time - it only stops the player being able to read it.
        chronometer.CurrentTime += ElapsedThisTurn;
        ElapsedThisTurn = TurnTimeIncrement;

        return base.ProcessEndOfTurn();
    }

    public override int GetDeathCount()
    {
        return DeathCounter;
    }

    public override void SetDeathCount(int count)
    {
        DeathCounter = count;
    }
}
