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
        return "Thanks, but you're not hungry. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A quantity of protein-rich liquid";
    }
}