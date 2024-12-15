namespace Planetfall.Item.Mech;

public class Flask : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["glass flask", "flask"];

    public string ExaminationDescription =>
        "The flask has a wide mouth and looks large enough to hold one or two liters. It is made of glass, " +
        "or perhaps some tough plastic.";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Sitting on the floor below the lowest shelf is a large glass flask. ";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a glass flask here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A flask";
    }
}