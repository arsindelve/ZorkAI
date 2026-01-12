using GameEngine.Item;
using Model.Interface;
using Utilities;

namespace Planetfall.Item.Kalamontee;

/// <summary>
/// Global bed object available in all dormitory rooms.
/// Represents the multi-tiered bunk beds where the player can sleep safely.
/// </summary>
internal class Bed : ItemBase, ISubLocation
{
    public override string[] NounsForMatching => ["bed", "bunk", "bunks"];

    public override string Name => "Bed";

    public string LocationDescription => "You are lying in one of the bunk beds. ";

    [UsedImplicitly]
    public bool PlayerInBed { get; set; }

    public string GetIn(IContext context)
    {
        if (!(context is PlanetfallContext planetfallContext))
            return "You can't get in the bed right now. ";

        PlayerInBed = true;

        // If player is tired, queue the fall asleep interrupt
        if (planetfallContext.Tired > TiredLevel.WellRested)
        {
            planetfallContext.SleepNotifications.QueueFallAsleep(planetfallContext.CurrentTime);
            return "Ahhh...the bed is soft and comfortable. You should be asleep in short order. ";
        }

        return "You are now in bed. ";
    }

    public string GetOut(IContext context)
    {
        if (!(context is PlanetfallContext planetfallContext))
            return "You climb out of the bed. ";

        // Cannot leave if falling asleep is queued
        if (planetfallContext.SleepNotifications.FallAsleepQueued)
        {
            return "How could you suggest such a thing when you're so tired and this bed is so comfy? ";
        }

        PlayerInBed = false;
        return "You climb out of the bed. ";
    }
}
