namespace Planetfall.Item.Lawanda;

internal class Medicine : ItemBase, IAmADrink
{
    public override string[] NounsForMatching => ["medicine"];

    public string OnDrinking(IContext context)
    {
        if (context is PlanetfallContext pc)
            pc.HasTakenExperimentalMedicine = true;

        return "The medicine tasted extremely bitter. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "At the bottom of the bottle is a small quantity of medicine. ";
    }
}