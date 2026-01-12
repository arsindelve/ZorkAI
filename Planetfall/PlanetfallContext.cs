using Model.AIGeneration.Requests;
using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall;

public class PlanetfallContext : Context<PlanetfallGame>, ITimeBasedContext
{
    [UsedImplicitly]
    public int Day { get; set; } = 1;

    [UsedImplicitly]
    public SicknessNotifications SicknessNotifications { get; set; } = new();

    [UsedImplicitly]
    public HungerNotifications HungerNotifications { get; set; } = new();

    public override string CurrentScore =>
        $"Your score would be {Score} (out of 80 points). It is Day {Day} of your adventure. " +
        $"Current Galactic Standard Time (adjusted to your local day-cycle) is " +
        $"{(WearingWatch ? CurrentTime : "impossible to determine, since you're not wearing your chronometer")}. " +
        $"\nThis score gives you the rank of {Game.GetScoreDescription(Score)}. ";

    private bool WearingWatch =>
        Items.Contains(Repository.GetItem<Chronometer>()) && Repository.GetItem<Chronometer>().BeingWorn;

    [UsedImplicitly] public HungerLevel Hunger { get; set; } = HungerLevel.WellFed;

    [UsedImplicitly] public TiredLevel Tired { get; set; } = TiredLevel.WellRested;
    
    [UsedImplicitly] public bool HasTakenExperimentalMedicine { get; set; }

    public string SicknessDescription => ((SicknessLevel)Day).GetDescription();
    
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
    }

    public override string ProcessBeginningOfTurn()
    {
        var messages = string.Empty;

        // Check for sickness notifications
        messages += SicknessNotifications.GetNotification(Day, CurrentTime);

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

            // Add notification message
            if (!string.IsNullOrEmpty(hungerNotification))
            {
                messages += "\n" + hungerNotification;
            }
        }

        return messages + base.ProcessBeginningOfTurn();
    }

    public override string? ProcessEndOfTurn()
    {
        Repository.GetItem<Chronometer>().CurrentTime += 54;
        return base.ProcessEndOfTurn();
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