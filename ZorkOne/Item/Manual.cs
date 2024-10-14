using GameEngine.Item;

namespace ZorkOne.Item;

public class Manual : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    public override string GenericDescription(ILocation? currentLocation) => "A ZORK owner's manual";

    public override string[] NounsForMatching => ["paper", "piece", "manual"];

    string ICanBeExamined.ExaminationDescription => ((ICanBeRead)this).ReadDescription;

    string ICanBeRead.ReadDescription => """
                                         Congratulations!
                                          
                                         You are the privileged owner of ZORK I: The Great Underground Empire,
                                         a self-contained and self-maintaining universe. If used and maintained
                                         in accordance with normal operating practices for small universes, ZORK
                                         will provide many months of trouble-free operation.
                                         """;

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a ZORK owner's manual here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Loosely attached to a wall is a small piece of paper. ";
    }
}