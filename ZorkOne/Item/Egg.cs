using Model.Interface;

namespace ZorkOne.Item;

public class Egg : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase, IOpenAndClose
{
    public override string[] NounsForMatching => ["egg", "jewel-encrusted egg", "jewel encrusted egg"];

    public override string InInventoryDescription => "A jewel-encrusted egg";

    public string ExaminationDescription => IsOpen ? "" : "The jewel-encrusted egg is closed. ";

    public string OnTheGroundDescription => "There is a jewel-encrusted egg here. ";

    public override string NeverPickedUpDescription =>
        "In the bird's nest is a large egg encrusted with precious jewels, apparently scavenged by a childless " +
        "songbird. The egg is covered with fine gold inlay, and ornamented in lapis lazuli and mother-of-pearl. " +
        "Unlike most eggs, this one is hinged and closed with a delicate looking clasp. The egg appears " +
        "extremely fragile. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public bool IsOpen { get; set; }

    public string NowOpen { get; } = "";

    public string NowClosed { get; } = "";

    public string AlreadyOpen { get; } = "";

    public override int Size => 1;

    public string AlreadyClosed => "It is already closed. ";

    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        return "You have neither the tools nor the expertise. ";
    }
}