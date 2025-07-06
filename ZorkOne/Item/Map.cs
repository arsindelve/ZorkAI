using GameEngine.Item;

namespace ZorkOne.Item;

internal class Map : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["map", "ancient map"];

    public string ExaminationDescription =>
        "The map shows a forest with three clearings. The largest clearing contains a house. Three paths leave " +
        "the large clearing. One of these paths, leading southwest, is marked \"To Stone Barrow\".";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an ancient map here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An ancient map ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ((ICanBeTakenAndDropped)this).OnTheGroundDescription(currentLocation);
    }
}