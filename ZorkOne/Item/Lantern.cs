using Game.Item;
using Model.Item;

namespace ZorkOne.Item;

public class Lantern : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeTurnedOnAndOff, IAmALightSource
{
    public override string[] NounsForMatching => ["lantern", "lamp", "light"];

    public override string InInventoryDescription => $"A brass lantern {(IsOn ? "(providing light)" : string.Empty)}";

    string ICanBeExamined.ExaminationDescription => IsOn ? "The lamp is on." : "The lamp is turned off.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a brass lantern (battery-powered) here.";

    public override string NeverPickedUpDescription => "A battery-powered brass lantern is on the trophy case.";
    public bool IsOn { get; set; }

    string ICanBeTurnedOnAndOff.NowOnText => "The brass lantern is now on.";

    string ICanBeTurnedOnAndOff.NowOffText => "The brass lantern is now off.";

    string ICanBeTurnedOnAndOff.AlreadyOffText => "It is already off.";

    string ICanBeTurnedOnAndOff.AlreadyOnText => "It is already on.";
}