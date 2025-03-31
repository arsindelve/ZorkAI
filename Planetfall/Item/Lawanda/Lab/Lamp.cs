namespace Planetfall.Item.Lawanda.Lab;

public class Lamp : ItemBase, IAmALightSourceThatTurnsOnAndOff, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching =>
    [
        "lamp", "light", "portable lamp", "powerful lamp", "portable light", "powerful light", "powerful portable lamp",
        "portable powerful lamp"
    ];

    public bool IsOn { get; set; }
    public string NowOnText => "The lamp is now producing a bright light. ";
    public string NowOffText => "The lamp goes dark. ";
    public string AlreadyOffText => "It isn't on. ";
    public string AlreadyOnText => "It is on. ";
    public string? CannotBeTurnedOnText => null;

    public string OnBeingTurnedOn(IContext context)
    {
        return String.Empty;
    }

    public void OnBeingTurnedOff(IContext context)
    {
    }


    public string ExaminationDescription => $"The lamp is {(IsOn ? "on" : "off")}. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a portable lamp here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "There is a powerful portable lamp here, currently off. ";
    }
}