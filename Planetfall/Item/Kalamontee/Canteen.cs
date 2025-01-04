namespace Planetfall.Item.Kalamontee;

public class Canteen : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["canteen", "octagonally shaped canteen"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(ProteinLiquid)];

    public override int Size => 1;

    public override bool IsTransparent => false;

    public string ExaminationDescription => Items.Any()
        ? ItemListDescription("canteen", null)
        : $"The canteen is {(IsOpen ? "open" : "closed")}. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Although the room is quite barren, an octagonally shaped canteen is sitting on one of the benches. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a canteen here. " + (Items.Any() && IsOpen ? $"\n{ItemListDescription("canteen", null)}" : "");
    }

    public override void Init()
    {
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override string OnOpening(IContext context)
    {
        if (Items.Any())
            return "Opening the canteen reveals a quantity of protein-rich liquid. ";

        return base.OnOpening(context);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A canteen" +
               (Items.Any() && IsOpen ? $"\n{ItemListDescription("canteen", null)}" : "");
    }
}