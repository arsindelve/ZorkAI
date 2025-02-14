using GameEngine.Item;

namespace ZorkOne.Item;

public class Tube : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["tube", "tube of toothpaste", "toothpaste tube"];

    /// <summary>
    /// Nothing can be put back into the tube. 
    /// </summary>
    protected override int SpaceForItems => 0;

    /// <summary>
    /// Cannot tell what is in the tube unless it's open. 
    /// </summary>
    public override bool IsTransparent => false;

    public override string NoRoomMessage => "That's not really going to work out for you. ";

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

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the tube reveals a viscous material. " : base.NowOpen(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A tube " + (Items.Any() && IsOpen
            ? Environment.NewLine + ItemListDescription("tube", currentLocation)
            : "");
    }

    public override void Init()
    {
        StartWithItemInside<ViscousMaterial>();
    }
}