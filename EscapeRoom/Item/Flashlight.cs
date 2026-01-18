using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace EscapeRoom.Item;

public class Flashlight : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, IAmALightSourceThatTurnsOnAndOff
{
    public override string[] NounsForMatching => ["flashlight", "torch", "light"];

    public override int Size => 2;

    public bool IsOn { get; set; }

    string ITurnOffAndOn.NowOnText => "The flashlight is now on, casting a bright beam of light.";

    string ITurnOffAndOn.NowOffText => "The flashlight is now off.";

    string ITurnOffAndOn.AlreadyOffText => "It is already off.";

    string ITurnOffAndOn.AlreadyOnText => "It is already on.";

    public string? CannotBeTurnedOnText => null;

    public string OnBeingTurnedOn(IContext context)
    {
        return string.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
    }

    string ICanBeExamined.ExaminationDescription =>
        IsOn
            ? "A small but sturdy flashlight. It is currently on, casting a bright beam of light."
            : "A small but sturdy flashlight. It is currently off.";

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return IsOn
            ? "There is a small flashlight here (providing light)."
            : "There is a small flashlight here.";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsOn ? "A flashlight (providing light)" : "A flashlight";
    }
}
