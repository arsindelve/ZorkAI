namespace Planetfall.Item.Feinstein;

/// <summary>
/// Base class for the three types of goo in the survival kit.
/// Each goo has a different flavor but the same nutritional value (1450 tick reset).
/// </summary>
internal abstract class GooBase : ItemBase, ICanBeEaten, ICanBeTakenAndDropped
{
    /// <summary>
    /// Remove "goo" from the list of disambiguation nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["goo"]).ToArray();
    
    /// <summary>
    /// Time in ticks that goo provides before hunger returns (1450 ticks).
    /// </summary>
    protected const int GooHungerResetTicks = 1450;

    protected abstract string FlavorDescription { get; }

    public override int Size => 1;

    public override string CannotBeTakenDescription =>
        "It would ooze through your fingers. You'll have to eat it right from the survival kit. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        // Goo should never be on the ground - it's always in the survival kit
        return string.Empty;
    }

    string? ICanBeTakenAndDropped.NeverPickedUpDescription(ILocation currentLocation)
    {
        return null;
    }

    string? ICanBeTakenAndDropped.OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        // This should never be called because CannotBeTakenDescription is set
        return null;
    }

    void ICanBeTakenAndDropped.OnFailingToBeTaken(IContext context)
    {
        // No special action needed
    }

    public (string Message, bool WasConsumed) OnEating(IContext context)
    {
        if (context is not PlanetfallContext pfContext)
            return ("Thanks, but you're not hungry. ", false);

        if (pfContext.Hunger == HungerLevel.WellFed)
            return ("Thanks, but you're not hungry. ", false);

        // Goo can only be eaten from the survival kit
        var survivalKit = Repository.GetItem<SurvivalKit>();
        var locationHasKit = pfContext.CurrentLocation is ICanContainItems container && container.Items.Contains(survivalKit);
        if (!pfContext.Items.Contains(survivalKit) && !locationHasKit)
            return ("You aren't holding that. ", false);

        // Check if survival kit is open
        if (!survivalKit.IsOpen)
            return ("The survival kit is not open. ", false);

        // Reset hunger to well-fed
        pfContext.Hunger = HungerLevel.WellFed;

        // Reset hunger notifications - goo provides 1450 ticks
        pfContext.HungerNotifications.ResetAfterEating(pfContext.CurrentTime, GooHungerResetTicks);

        return ($"Mmmm...that tasted just like {FlavorDescription}. ", true);
    }
}

internal class RedGoo : GooBase
{
    public override string[] NounsForMatching => ["red goo", "red", "goo"];

    protected override string FlavorDescription => "scrumptious cherry pie";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A blob of red goo";
    }
}

internal class BrownGoo : GooBase
{
    public override string[] NounsForMatching => ["brown goo", "brown", "goo"];

    protected override string FlavorDescription => "delicious Nebulan fungus pudding";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A blob of brown goo";
    }
}

internal class GreenGoo : GooBase
{
    public override string[] NounsForMatching => ["green goo", "green", "goo"];

    protected override string FlavorDescription => "yummy lima beans";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A blob of green goo";
    }
}
