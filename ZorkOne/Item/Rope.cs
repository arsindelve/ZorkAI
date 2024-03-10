using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Rope : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    string ICanBeExamined.ExaminationDescription => "There's nothing special about the rope.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a rope here";
    
    public override string[] NounsForMatching => ["rope"];

    public override string InInventoryDescription => "A rope";
    
    public override string NeverPickedUpDescription =>
        "A large coil of rope is lying in the corner.";
}