using Model.Item;
using Planetfall.Command;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Dorm;

namespace Planetfall;

/// <summary>
/// Handles all sleep-related mechanics including forced sleep, dreaming, and waking up.
/// This engine processes sleep warnings, forced sleep in dangerous locations, and day progression.
/// </summary>
public class SleepEngine
{
    private static readonly Random Random = new();

    /// <summary>
    /// Dangerous locations where sleeping on the ground can result in drowning on specific days.
    /// </summary>
    private static readonly Dictionary<Type, int> DrowningLocations = new()
    {
        { typeof(Crag), 1 },           // Day 1: drown at Crag
        { typeof(Balcony), 3 },        // Day 3: drown at Balcony
        { typeof(WindingStair), 5 }    // Day 5: drown at Winding Stair
    };

    /// <summary>
    /// Processes voluntary sleep (player enters bed while tired).
    /// Called when the fall asleep timer triggers after 16 ticks.
    /// </summary>
    public static string ProcessFallAsleep(PlanetfallContext context)
    {
        context.SleepNotifications.CancelFallAsleep();

        var message = "\nYou slowly sink into a deep and restful sleep.";

        // Add dream
        var dream = Dreams.GetDream(context, Random);
        if (dream != null)
            message += dream;

        // Wake up
        message += "\n" + ProcessWakeUp(context);

        return message;
    }

    /// <summary>
    /// Processes forced sleep when player reaches maximum tiredness (AboutToDrop level).
    /// Outcome depends on current location.
    /// </summary>
    public static string ProcessForcedSleep(PlanetfallContext context)
    {
        var location = context.CurrentLocation;

        // Already in bed - safe
        if (location is BedLocation)
        {
            var message = "\nYou slowly sink into a deep and blissful sleep.";
            var dream = Dreams.GetDream(context, Random);
            if (dream != null)
                message += dream;

            message += "\n" + ProcessWakeUp(context);
            return message;
        }

        // In a dormitory room - automatically climb into bed
        if (location is DormA or DormB or DormC or DormD)
        {
            var bed = Repository.GetItem<Bed>();
            bed.PlayerInBed = true;
            context.CurrentLocation = Repository.GetLocation<BedLocation>();

            var message = "\nYou climb into one of the bunk beds and immediately fall asleep.";
            var dream = Dreams.GetDream(context, Random);
            if (dream != null)
                message += dream;

            message += "\n" + ProcessWakeUp(context);
            return message;
        }

        // Dangerous location - sleep on ground
        return ProcessGroundSleep(context);
    }

    /// <summary>
    /// Handles sleeping on the ground in potentially dangerous locations.
    /// 30% chance of beast attack, day-specific drowning, or survival.
    /// </summary>
    private static string ProcessGroundSleep(PlanetfallContext context)
    {
        var message = "\nYou can't stay awake a moment longer. You drop to the ground and fall into a " +
                      "deep but fitful sleep.";

        // Check for day-specific drowning deaths
        var locationType = context.CurrentLocation.GetType();
        if (DrowningLocations.TryGetValue(locationType, out var drowningDay) && context.Day == drowningDay)
        {
            var deathMessage = "\n\nSuddenly, in the middle of the night, a wave of water washes over you. " +
                               "Before you can quite get your bearings, you drown.";
            return message + new DeathProcessor().Process(deathMessage, context).InteractionMessage;
        }

        // 30% chance of beast attack
        if (Random.Next(100) < 30)
        {
            var deathMessage = "\n\nSuddenly, in the middle of the night, you awake as several ferocious beasts " +
                               "(could they be grues?) surround and attack you. Perhaps you should have found a " +
                               "slightly safer place to sleep.";
            return message + new DeathProcessor().Process(deathMessage, context).InteractionMessage;
        }

        // 70% chance of survival - still wake up normally
        var dream = Dreams.GetDream(context, Random);
        if (dream != null)
            message += dream;

        message += "\n" + ProcessWakeUp(context);
        return message;
    }

    /// <summary>
    /// Handles waking up after sleep.
    /// Advances day, resets timers, drops items, spoils food, and displays wake messages.
    /// </summary>
    private static string ProcessWakeUp(PlanetfallContext context)
    {
        // Advance day
        context.Day++;

        // Check for day 9 death
        if (context.Day >= 9)
        {
            return new DeathProcessor().Process(
                "Unfortunately, you don't seem to have survived the night.",
                context).InteractionMessage;
        }

        // Reset fatigue
        context.Tired = TiredLevel.WellRested;

        // Reset sleep timer for new day
        context.SleepNotifications.ResetForNewDay(context.CurrentTime, context.Day);

        // Enable next sickness check
        context.SicknessNotifications.DaysNotified.Remove(context.Day);

        // Drop non-worn inventory items
        var droppedItems = new List<IItem>();
        var itemsToRemove = new List<IItem>();

        foreach (var item in context.Items.ToList())
        {
            // Check if item is worn
            if (item is IAmClothing clothing && clothing.BeingWorn)
                continue;

            // Drop the item
            context.RemoveItem(item);
            context.CurrentLocation.ItemPlacedHere(item);
            droppedItems.Add(item);

            // Spoil food in open canteen
            if (item is Canteen canteen)
            {
                var proteinLiquid = Repository.GetItem<ProteinLiquid>();
                if (canteen.IsOpen && canteen.Items.Contains(proteinLiquid))
                {
                    canteen.RemoveItem(proteinLiquid);
                    itemsToRemove.Add(proteinLiquid);
                }
            }

            // Spoil chemical fluid in flask (if needed in future)
            // Note: Flask spoiling logic would go here if ChemicalFluid item exists
        }

        // Build wake message
        var message = $"***** SEPTEM {context.Day + 5}, 11344 *****\n\n";

        // Location-based wake message
        if (context.CurrentLocation is not BedLocation)
        {
            message += "You wake and slowly stand up, feeling stiff from your night on the floor. ";
        }
        else
        {
            // Health-based message (sickness gets worse each day)
            var sicknessLevel = (SicknessLevel)context.Day;
            if (sicknessLevel < SicknessLevel.Meh)
            {
                message += "You wake up feeling refreshed and ready to face the challenges of this mysterious world. ";
            }
            else if (sicknessLevel < SicknessLevel.ReallySick)
            {
                message += "You wake after sleeping restlessly. You feel weak and listless. ";
            }
            else
            {
                message += "You wake feeling weak and worn-out. It will be an effort just to stand up. ";
            }
        }

        // Hunger adjustment
        if (context.Hunger > HungerLevel.WellFed)
        {
            context.Hunger = HungerLevel.AboutToPassOut;
            context.HungerNotifications.ResetAfterEating(context.CurrentTime, 100);
            message += "You are also incredibly famished. Better get some breakfast! ";
        }
        else
        {
            context.HungerNotifications.ResetAfterEating(context.CurrentTime, 400);
        }

        // Floyd greeting (if present and active)
        var floyd = Repository.GetItem<Floyd>();
        if (floyd.HasEverBeenOn && floyd.IsOn)
        {
            // Move Floyd to player's location
            floyd.CurrentLocation?.RemoveItem(floyd);
            context.CurrentLocation.ItemPlacedHere(floyd);

            if (context.CurrentLocation is BedLocation)
            {
                message += "\n\nFloyd bounces impatiently at the foot of the bed. \"About time you woke up, " +
                          "you lazy bones! Let's explore around some more!\"";
            }
            else
            {
                message += "\n\nFloyd gives you a nudge with his foot and giggles. \"You sure look silly " +
                          "sleeping on the floor,\" he says.";
            }
        }

        // Exit bed if in bed
        if (context.CurrentLocation is BedLocation bedLocation)
        {
            var bed = Repository.GetItem<Bed>();
            bed.PlayerInBed = false;
            context.CurrentLocation = bedLocation.ParentLocation;
        }

        return message;
    }

    /// <summary>
    /// Checks if the player should fall asleep (voluntary or forced).
    /// Returns a message if sleep occurs, null otherwise.
    /// </summary>
    public static string? CheckForSleep(PlanetfallContext context)
    {
        // Check for voluntary sleep (in bed, fall asleep queued)
        if (context.SleepNotifications.ShouldFallAsleep(context.CurrentTime))
        {
            return ProcessFallAsleep(context);
        }

        // Check for forced sleep (maximum tiredness reached)
        if (context.SleepNotifications.ShouldForceSleep(context.CurrentTime, context.Tired))
        {
            return ProcessForcedSleep(context);
        }

        return null;
    }
}
