namespace Planetfall.Item.Kalamontee.Mech;

public class CardboardBox : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["small cardboard box", "box", "cardboard box", "small box", "cardboard"];

    public override bool IsTransparent => true;

    protected override int SpaceForItems => 10;

    public override int Size => 3;

    public string ExaminationDescription => Items.Any() ? ItemListDescription("cardboard box", null) : "The cardboard box is empty. ";

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a cardboard box here. " +
               (Items.Any() ? $"\n{ItemListDescription("cardboard box", currentLocation)}" : "");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On the floor beneath the shelves sits a small cardboard box. " +
               (Items.Any() ? $"\n{ItemListDescription("cardboard box", currentLocation)}" : "");
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A cardboard box" + (Items.Any() ? $"\n{ItemListDescription("cardboard box", currentLocation)}" : "");
    }

    public override void Init()
    {
        StartWithItemInside<GoodBedistor>();
        StartWithItemInside<KSeriesMegafuse>();
        StartWithItemInside<BSeriesMegafuse>();
        StartWithItemInside<CrackedFromitzBoard>();
    }
}
