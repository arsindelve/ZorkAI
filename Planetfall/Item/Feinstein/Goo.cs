namespace Planetfall.Item.Feinstein;

/// <summary>
/// Base class for the three types of goo in the survival kit.
/// Each goo has a different flavor but the same nutritional value (1450 tick reset).
/// </summary>
internal abstract class GooBase : ItemBase, ICanBeEaten
{
    protected abstract string FlavorDescription { get; }

    public string OnEating(IContext context)
    {
        if (context is not PlanetfallContext pfContext)
            return "Thanks, but you're not hungry. ";

        if (pfContext.Hunger == HungerLevel.WellFed)
            return "Thanks, but you're not hungry. ";

        // Check if player is holding the survival kit
        var survivalKit = Repository.GetItem<SurvivalKit>();
        if (!pfContext.Items.Contains(survivalKit))
            return "You aren't holding that. ";

        // Reset hunger to well-fed
        pfContext.Hunger = HungerLevel.WellFed;

        // Reset hunger notifications - goo provides 1450 ticks
        pfContext.HungerNotifications.ResetAfterEating(pfContext.CurrentTime, 1450);

        return $"Mmmm...that tasted just like {FlavorDescription}. ";
    }
}

internal class RedGoo : GooBase
{
    public override string[] NounsForMatching => ["red goo", "goo"];

    protected override string FlavorDescription => "scrumptious cherry pie";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some red goo";
    }
}

internal class BrownGoo : GooBase
{
    public override string[] NounsForMatching => ["brown goo", "goo"];

    protected override string FlavorDescription => "delicious Nebulan fungus pudding";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some brown goo";
    }
}

internal class GreenGoo : GooBase
{
    public override string[] NounsForMatching => ["green goo", "goo"];

    protected override string FlavorDescription => "yummy lima beans";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some green goo";
    }
}
