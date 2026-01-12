namespace Planetfall.Item.Feinstein;

/// <summary>
/// Base class for the three types of goo in the survival kit.
/// Each goo has a different flavor but the same nutritional value (1450 tick reset).
/// </summary>
internal abstract class GooBase : ItemBase, ICanBeEaten, ICanBeTakenAndDropped
{
    /// <summary>
    /// Time in ticks that goo provides before hunger returns (1450 ticks).
    /// </summary>
    protected const int GooHungerResetTicks = 1450;

    protected abstract string FlavorDescription { get; }

    public override int Size => 1;

    public override string? CannotBeTakenDescription =>
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

    public string OnEating(IContext context)
    {
        if (context is not PlanetfallContext pfContext)
            return "Thanks, but you're not hungry. ";

        if (pfContext.Hunger == HungerLevel.WellFed)
            return "Thanks, but you're not hungry. ";

        // Goo can only be eaten from the survival kit
        // By the time OnEating() is called, the goo has been destroyed, so we can't check its location.
        // Instead, we check if the survival kit is in the player's inventory or current location.
        var survivalKit = Repository.GetItem<SurvivalKit>();
        var locationHasKit = pfContext.CurrentLocation is ICanContainItems container && container.Items.Contains(survivalKit);
        if (!pfContext.Items.Contains(survivalKit) && !locationHasKit)
            return "You aren't holding that. ";

        // Check if survival kit is open
        if (!survivalKit.IsOpen)
            return "The survival kit is not open. ";

        // Reset hunger to well-fed
        pfContext.Hunger = HungerLevel.WellFed;

        // Reset hunger notifications - goo provides 1450 ticks
        pfContext.HungerNotifications.ResetAfterEating(pfContext.CurrentTime, GooHungerResetTicks);

        // Note: The goo is automatically destroyed by EatAndDrinkInteractionProcessor
        // before OnEating() is called, so no need to remove it here

        return $"Mmmm...that tasted just like {FlavorDescription}. ";
    }
}

internal class RedGoo : GooBase
{
    public override string[] NounsForMatching => ["red goo", "red"];

    protected override string FlavorDescription => "scrumptious cherry pie";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some red goo";
    }
}

internal class BrownGoo : GooBase
{
    public override string[] NounsForMatching => ["brown goo", "brown"];

    protected override string FlavorDescription => "delicious Nebulan fungus pudding";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some brown goo";
    }
}

internal class GreenGoo : GooBase
{
    public override string[] NounsForMatching => ["green goo", "green"];

    protected override string FlavorDescription => "yummy lima beans";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some green goo";
    }
}
