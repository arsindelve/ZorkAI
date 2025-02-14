using GameEngine.Item;

namespace ZorkOne.Item;

public class Tube : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["tube", "tube of toothpaste", "toothpaste tube"];

    protected override int SpaceForItems => 0;

    public override bool IsTransparent => false;

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => """
                                     ---> Frobozz Magic Gunk Company <---
                                              All-Purpose Gunk
                                     """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return Items.Any() && IsOpen
            ? "There is an object which looks like a tube of toothpaste here. " + Environment.NewLine +
              ItemListDescription("tube", currentLocation)
            : "There is an object which looks like a tube of toothpaste here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A tube ";
    }

    public override void Init()
    {
        StartWithItemInside<ViscousMaterial>();
    }
}