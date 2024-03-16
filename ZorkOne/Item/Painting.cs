using System.Drawing;

namespace ZorkOne.Item;

public class Painting : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["painting"];

    public override string InInventoryDescription => "A painting";

    public string ExaminationDescription =>
        "There is nothing special about the painting";

    public string OnTheGroundDescription => "A painting by a neglected artist is here. ";

    public override string NeverPickedUpDescription =>
        "Fortunately, there is still one chance for you to be a vandal, for on the far wall is a painting of unparalleled beauty. ";

    public override int Size => 4;
    
    // TODO: >cut painting with sword
    // Your skillful swordsmanship slices the painting into innumerable slivers which blow away.
}