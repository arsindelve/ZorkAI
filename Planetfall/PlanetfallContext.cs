using Model.AIGeneration.Requests;
using Newtonsoft.Json;
using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall;

public class PlanetfallContext : Context<PlanetfallGame>, ITimeBasedContext, ISurvivalClockContext,
    IResettableClockContext, IGodModeTeleportAware, IFloydWanderingContext
{
    private const int TurnTimeIncrement = 54;

    [UsedImplicitly]
    public int Day { get; set; } = 1;

    /// <summary>
    ///     Issue #277: god-mode test affordance. When true, the sleep/fatigue clock is suspended -
    ///     no tiredness escalation, warnings, or forced sleep - and the player is kept well-rested.
    ///     Plain auto-property so it round-trips through the save/restore JSON like the rest of context state.
    /// </summary>
    [UsedImplicitly]
    public bool SleepClockDisabled { get; set; }

    /// <summary>
    ///     Issue #277: god-mode test affordance. When true, the hunger/thirst clock is suspended -
    ///     no hunger escalation, warnings, or starvation death - and the player is kept well-fed.
    ///     Plain auto-property so it round-trips through the save/restore JSON like the rest of context state.
    /// </summary>
    [UsedImplicitly]
    public bool HungerClockDisabled { get; set; }

    /// <summary>
    ///     God-mode test affordance. When true, Floyd's two random wandering triggers (failing to
    ///     follow the player, and spontaneously wandering off) are suppressed, for deterministic
    ///     playtesting/walkthroughs. Plain auto-property so it round-trips through the save/restore
    ///     JSON like the rest of context state.
    /// </summary>
    [UsedImplicitly]
    public bool FloydWanderingDisabled { get; set; }

    /// <summary>
    ///     Number of times the player has died. Preserved across death restarts.
    /// </summary>
    [UsedImplicitly]
    public int DeathCounter { get; set; }

    [UsedImplicitly]
    public SicknessNotifications SicknessNotifications { get; set; } = new();

    [UsedImplicitly]
    public HungerNotifications HungerNotifications { get; set; } = new();

    [UsedImplicitly]
    public SleepNotifications SleepNotifications { get; set; } = new();

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"Current Galactic Standard Time (adjusted to your local day-cycle) is " +
        $"{(WearingWatch ? CurrentTime : "impossible to determine, since you're not wearing your chronometer")}. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    private bool WearingWatch =>
        Items.Contains(Repository.GetItem<Chronometer>()) && Repository.GetItem<Chronometer>().BeingWorn;

    [UsedImplicitly] public HungerLevel Hunger { get; set; } = HungerLevel.WellFed;

    [UsedImplicitly] public TiredLevel Tired { get; set; } = TiredLevel.WellRested;

    /// <summary>
    ///     Issue #116: the disease is a real death clock, not a cosmetic day-derived value. This mutable
    ///     counter advances one level per day (in <see cref="SleepEngine"/> on waking), reaching 9 = death.
    ///     The experimental medicine vial rolls it back two levels. It starts at 1 (== the opening Day), so
    ///     an untreated player's level tracks the day and they still die on the original day-9 schedule;
    ///     treating the disease decouples the two and pushes death back. Mirrors the original's
    ///     SICKNESS-LEVEL (globals.zil:2330-2369).
    /// </summary>
    [UsedImplicitly] public int SicknessCounter { get; set; } = 1;

    /// <summary>
    ///     Base carrying capacity before any disease penalty. Mirrors the original's LOAD-ALLOWED default.
    /// </summary>
    private const int BaseLoadAllowed = 100;

    /// <summary>
    ///     Issue #116: each sick level above the first shaves 10 off the carrying capacity, mirroring the
    ///     original daemon's `LOAD-ALLOWED -= 10` per sick day (globals.zil:2330). Drinking the medicine
    ///     drops the sickness level by two, which restores 20 - exactly the original's `LOAD-ALLOWED += 20`
    ///     (comptwo.zil:170-185).
    /// </summary>
    public int EffectiveLoadAllowed => BaseLoadAllowed - 10 * Math.Max(0, SicknessCounter - 1);

    /// <summary>
    ///     Issue #116: enforce the disease's carrying-capacity squeeze. Planetfall previously had no weight
    ///     limit at all; now the effective limit shrinks as the player gets sicker.
    /// </summary>
    public override bool HaveRoomForItem(IItem item)
    {
        return CalculateTotalSize() + item.Size <= EffectiveLoadAllowed;
    }

    /// <summary>
    /// Flag indicating that sleep was just processed this turn.
    /// Used to prevent the SleepProcessor from adding redundant messages after a sleep cycle.
    /// </summary>
    public bool SleepJustOccurred { get; set; }

    /// <summary>
    /// Stores a prompt for Floyd to comment on during his Act() phase.
    /// When set, Floyd will generate this comment at the start of his Act() instead of random behavior.
    /// Reset at the beginning of each turn.
    /// </summary>
    [JsonIgnore]
    public string? PendingFloydActionCommentPrompt { get; set; }

    /// <summary>
    /// When true, Floyd will not act or talk this turn.
    /// Reset at the beginning of each turn.
    /// </summary>
    [JsonIgnore]
    public bool FloydShouldNotActThisTurn { get; set; }

    /// <summary>
    /// Tracks Floyd action comment prompts that have already been used.
    /// Prompts in this set will not be repeated.
    /// </summary>
    [UsedImplicitly]
    public HashSet<string> UsedFloydActionCommentPrompts { get; set; } = [];

    // Issue #116: derive the health description from the mutable sickness counter, not the calendar day,
    // so treating the disease actually changes how the player feels. Clamp into the enum's 1-8 range.
    public string SicknessDescription => ((SicknessLevel)Math.Clamp(SicknessCounter, 1, 8)).GetDescription();
    
    [UsedImplicitly]
    public int CurrentTime => Repository.GetItem<Chronometer>().CurrentTime;

    public string CurrentTimeResponse =>
        WearingWatch
            ? $"According to the chronometer, the current time is {CurrentTime}. "
            : "It's hard to say, since you've removed your chronometer. ";

    public override void Init()
    {
        StartWithItem<Brush>(this);
        StartWithItem<Diary>(this);
        StartWithItem<Chronometer>(this);
        StartWithItem<PatrolUniform>(this);

        // Initialize hunger system with current time (after Chronometer is set up)
        HungerNotifications.Initialize(CurrentTime);

        // Initialize sleep system with current time
        SleepNotifications.Initialize(CurrentTime);
    }

    /// <summary>
    ///     Floyd's one-shot per-turn flags must reset every turn - including free commands (issue
    ///     #354) that skip the rest of <see cref="ProcessBeginningOfTurn" /> - since actor processing
    ///     (which drives <c>Floyd.Act()</c>) always runs regardless. The engine calls this
    ///     unconditionally before conditionally calling ProcessBeginningOfTurn.
    /// </summary>
    public override void ResetPerTurnActorFlags()
    {
        PendingFloydActionCommentPrompt = null;
        FloydShouldNotActThisTurn = false;
    }

    public override string ProcessBeginningOfTurn()
    {
        // Idempotent safety net: GameEngine.cs already calls ResetPerTurnActorFlags() unconditionally
        // before this (including for free commands that skip the rest of this method), but any other
        // caller of ProcessBeginningOfTurn() directly must still get Floyd's one-shot flags reset
        // without needing to know about that split - calling it twice on a normal turn is harmless,
        // since it only sets two fields to null/false.
        ResetPerTurnActorFlags();

        var messages = string.Empty;

        // Issue #277: when the sleep clock is disabled (god-mode test affordance) we never check for
        // voluntary/forced sleep, and we keep the player well-rested so an already-tired session is
        // immediately relieved. We also cancel any pending fall-asleep so the player can't get stuck
        // in a bed they entered before flipping the toggle.
        if (SleepClockDisabled)
        {
            Tired = TiredLevel.WellRested;
            SleepNotifications.CancelFallAsleep();
        }
        else
        {
            // Check for sleep events (voluntary or forced)
            var sleepMessage = SleepEngine.CheckForSleep(this);
            if (!string.IsNullOrEmpty(sleepMessage))
            {
                SleepJustOccurred = true;

                // Issue #355: sleep (voluntary or forced) may fire on a turn where the player's
                // command was something else entirely (e.g. a movement command issued while
                // exhausted). Sleep already mutated state - dropped carried items, possibly moved
                // the player into a bed - against CurrentLocation as of THIS turn. Tell the engine
                // to short-circuit and defer that command rather than run it against a location
                // this event has already left stale.
                TurnConsumedByForcedEvent = true;
                return sleepMessage + base.ProcessBeginningOfTurn();
            }
        }

        // Check for sickness notifications
        var sicknessNotification = SicknessNotifications.GetNotification(Day, CurrentTime);
        if (!string.IsNullOrEmpty(sicknessNotification))
        {
            messages += sicknessNotification;
        }

        // Check if sleep level should advance (skipped entirely when the sleep clock is disabled - issue #277)
        if (!SleepClockDisabled)
        {
            var nextTiredLevel = SleepNotifications.GetNextTiredLevel(CurrentTime, Tired);
            if (nextTiredLevel.HasValue)
            {
                // Get notification BEFORE advancing level
                var sleepNotification = SleepNotifications.GetNotification(CurrentTime, Tired);

                Tired = nextTiredLevel.Value;

                // Add notification message (with newline separator if sickness notification also fired)
                if (!string.IsNullOrEmpty(sleepNotification))
                {
                    if (!string.IsNullOrEmpty(messages))
                        messages += "\n";
                    messages += sleepNotification;
                }
            }
        }

        // Issue #277: when the hunger clock is disabled (god-mode test affordance) we skip all hunger
        // escalation and the starvation death, and keep the player well-fed so an already-hungry
        // session is immediately relieved.
        if (HungerClockDisabled)
        {
            Hunger = HungerLevel.WellFed;
        }
        else
        {
            // Check if hunger level should advance
            var nextHungerLevel = HungerNotifications.GetNextHungerLevel(CurrentTime, Hunger);
            if (nextHungerLevel.HasValue)
            {
                // Get notification BEFORE advancing level (so it returns notification for the new level)
                var hungerNotification = HungerNotifications.GetNotification(CurrentTime, Hunger);

                Hunger = nextHungerLevel.Value;

                // Check for death
                if (Hunger == HungerLevel.Dead)
                {
                    var deathResult = new DeathProcessor().Process(
                        "You collapse from extreme thirst and hunger.", this);
                    return messages + "\n" + deathResult.InteractionMessage;
                }

                // Add notification message (with newline separator if sickness notification also fired)
                if (!string.IsNullOrEmpty(hungerNotification))
                {
                    if (!string.IsNullOrEmpty(messages))
                        messages += "\n";
                    messages += hungerNotification;
                }
            }
        }

        return messages + base.ProcessBeginningOfTurn();
    }

    public override string? ProcessEndOfTurn()
    {
        Repository.GetItem<Chronometer>().CurrentTime += TurnTimeIncrement;
        SleepJustOccurred = false;
        return base.ProcessEndOfTurn();
    }

    public void ResetClockForGodMode(int targetTime)
    {
        // God-mode commands still take a Planetfall turn, so compensate for the end-of-turn tick.
        Repository.GetItem<Chronometer>().CurrentTime = targetTime - TurnTimeIncrement;
    }

    /// <summary>
    ///     Issue #356 follow-up: "god mode go &lt;place&gt;" is a raw CurrentLocation swap - it never runs
    ///     DeckNine.OnLeaveLocation or EscapePod.AfterEnterLocation, so ExplosionCoordinator (registered
    ///     unconditionally from game start) and EscapePod's own post-landing sinking timer (armed once
    ///     the player stands out of the safety web) stay armed even after a tester teleports away.
    ///     Both only check CurrentLocation, not how the player got there, so either would otherwise
    ///     unconditionally kill the tester once their move count rolls into its death window, wherever
    ///     they happened to be testing.
    ///     ExplosionCoordinator is disarmed unless the destination is DeckNine specifically - staying
    ///     armed there is intentional (mirrors normal play: staying put without reaching the pod is
    ///     still fatal at move 14). EscapePod does NOT get the same exception: its move-14 case in
    ///     ExplosionCoordinator has no location check at all, relying on EscapePod.AfterEnterLocation
    ///     always disarming the coordinator before a real player is ever standing in the pod at that
    ///     point - a god-mode teleport straight into the pod must disarm it the same way, or the
    ///     tester gets killed by their own ship's explosion in the one place meant to be safe.
    ///     EscapePod's own sinking timer is disarmed unless the destination is EscapePod itself, so a
    ///     tester can still teleport in to observe that sequence.
    /// </summary>
    public void OnGodModeTeleport()
    {
        if (CurrentLocation is not DeckNine)
            RemoveActor<ExplosionCoordinator>();

        if (CurrentLocation is not EscapePod)
            RemoveActor<EscapePod>();
    }

    /// <summary>
    ///     Planetfall-specific save game request logic.
    ///     If Floyd is present and active, returns a Floyd-specific request with fourth-wall-breaking comments.
    ///     Otherwise, returns null to use the default AfterSaveGameRequest.
    /// </summary>
    public override Request? GetSaveGameRequest(string location)
    {
        var floyd = Repository.GetItem<Floyd>();
        return floyd.IsHereAndIsOn(this) ?
            new FloydAfterSaveGameRequest(location) :
            null; // Use default AfterSaveGameRequest
    }

    public override int GetDeathCount() => DeathCounter;

    public override void SetDeathCount(int count) => DeathCounter = count;
}


/* 


Here are all the possible endings in the Cryo-Anteroom (comptwo.zil:1393-1478):

   The Three Systems

   Beyond fixing the computer (mandatory to reach the ending), there are three planetary systems you can repair:
   ┌────────────────┬──────────────────────┬────────┐
   │     System     │   Global Variable    │ Points │
   ├────────────────┼──────────────────────┼────────┤
   │ Communications │ COMM-FIXED           │ 6      │
   ├────────────────┼──────────────────────┼────────┤
   │ Meteor Defense │ DEFENSE-FIXED        │ 6      │
   ├────────────────┼──────────────────────┼────────┤
   │ Course Control │ COURSE-CONTROL-FIXED │ 6      │
   └────────────────┴──────────────────────┴────────┘
   ---
   Ending 1: Perfect Victory (All 3 Fixed)

   Conditions: COMM-FIXED + DEFENSE-FIXED + COURSE-CONTROL-FIXED

   Veldina awakens, studies the readouts, and presses several keys. Other cryo-units begin opening. She thanks you: "Thanks to you, the cure has been discovered, and the planetary systems repaired. We are eternally grateful."

   The S.P.S. Flathead arrives in orbit (communications working). A landing party materializes, including Blather (rescued from another escape pod, "babbling cravenly"). Captain Sterling promotes you to Lieutenant First Class.

   As mutant hunters head for the elevator, Veldina offers you leadership of Resida. Sterling mentions that Blather (demoted to Ensign Twelfth Class) has been assigned as your personal toilet attendant.

   A medical robot administers the antidote for The Disease.

   Floyd returns! Robot technicians repaired him. He bounds toward you: "Hi! Floyd feeling better now!" He hands you a helicopter key, a reactor elevator card, and a paddleball set. "Maybe we can use them in the sequel..."

   ---
   Ending 2: Stranded Hero (Course Control Fixed, Missing Comm OR Defense)

   Conditions: COURSE-CONTROL-FIXED + (NOT COMM-FIXED OR NOT DEFENSE-FIXED)

   Veldina awakens, the cure is discovered, other cryo-units open. She's grateful. But...

   "Unfortunately, a second ship from your Stellar Patrol has..."

   - If DEFENSE-FIXED is false: "...been destroyed by our malfunctioning meteor defenses."
   - If COMM-FIXED is false: "...come looking for survivors, and because of our malfunctioning communications system, has given up and departed."

   "I fear that you are stranded on Resida, possibly forever. However, we show our gratitude by offering you an unlimited bank account and a house in the country."

   ---
   Ending 3: Doomed Planet (Course Control NOT Fixed)

   Conditions: NOT COURSE-CONTROL-FIXED

   Veldina awakens, studies the readouts. With a strained voice: "You have fixed our computer and a Cure has been discovered, and we are grateful. But alas, it was all in vain. Our planetary course control system has malfunctioned, and the orbit has now decayed beyond correction. Soon Resida will plunge into the sun."

   Sub-ending 3a: If COMM-FIXED + DEFENSE-FIXED:
   "Fortunately, another ship from your Stellar Patrol has arrived, so at least you will survive." The Flathead materializes and takes you away from the doomed world.

   Sub-ending 3b: If missing Comm or Defense:
   The game just ends — you presumably die with the planet.

   ---
   Summary Table
   ┌────────┬──────┬─────────┬─────────────────────────────────────────────────────────────────┐
   │ Course │ Comm │ Defense │                             Outcome                             │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✓      │ ✓    │ ✓       │ Best: Promotion, Floyd returns, lead Resida, Blather humiliated │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✓      │ ✓    │ ✗       │ Stranded: Rescue ship destroyed by meteors                      │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✓      │ ✗    │ ✓       │ Stranded: Rescue ship left (no communication)                   │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✓      │ ✗    │ ✗       │ Stranded: (same as above)                                       │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✗      │ ✓    │ ✓       │ Planet doomed, but you escape on Flathead                       │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✗      │ ✓    │ ✗       │ Planet doomed, you die                                          │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✗      │ ✗    │ ✓       │ Planet doomed, you die                                          │
   ├────────┼──────┼─────────┼─────────────────────────────────────────────────────────────────┤
   │ ✗      │ ✗    │ ✗       │ Planet doomed, you die                                          │
   └────────┴──────┴─────────┴─────────────────────────────────────────────────────────────────┘
   
   
   */
