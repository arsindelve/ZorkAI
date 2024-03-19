namespace ZorkOne.Item;

public class Rope : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["rope"];

    public override string InInventoryDescription => "A rope";

    public override int Size => 2;

    public bool TiedToRailing { get; set; }

    string ICanBeExamined.ExaminationDescription => "There's nothing special about the rope. ";

    string ICanBeTakenAndDropped.OnTheGroundDescription => TiedToRailing
        ? "Hanging down from the railing is a rope which ends about ten feet from the floor below. "
        : "There is a rope here. ";

    public override string NeverPickedUpDescription =>
        "A large coil of rope is lying in the corner.";

    public override void OnBeingTaken(IContext context)
    {
        TiedToRailing = false;
        base.OnBeingTaken(context);
    }
}