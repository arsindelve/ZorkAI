namespace Planetfall.Item.Kalamontee;

internal class ProteinLiquid : ItemBase, IAmADrink
{
    public override string[] NounsForMatching =>
    [
        "protein liquid", "brown liquid", "liquid", "protein-rich liquid", "quantity of protein-rich liquid"
    ];

    public override string CannotBeTakenDescription => "It would just slip through your fingers. ";

    string IAmADrink.OnDrinking(IContext context)
    {
        if (context is not PlanetfallContext pfContext)
            return "Thanks, but you're not hungry. ";

        if (pfContext.Hunger == HungerLevel.WellFed)
            return "Thanks, but you're not hungry. ";

        // Reset hunger to well-fed
        pfContext.Hunger = HungerLevel.WellFed;

        // Reset hunger notifications - protein liquid provides 3600 ticks
        pfContext.HungerNotifications.ResetAfterEating(pfContext.CurrentTime, 3600);

        return "Mmmm....that was good. It certainly quenched your thirst and satisfied your hunger. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A quantity of protein-rich liquid";
    }
}