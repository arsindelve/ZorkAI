using GameEngine.Item;

namespace EscapeRoom.Item;

public class Couch : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["couch", "sofa", "worn couch"];

    public override string? CannotBeTakenDescription => "The couch is too heavy to move. ";

    string ICanBeExamined.ExaminationDescription =>
        "A comfortable-looking worn couch. It has seen better days but still looks inviting. ";

    public override string NeverPickedUpDescription(ILocation? currentLocation)
    {
        return "A worn couch sits against the wall. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return NeverPickedUpDescription(currentLocation);
    }
}
