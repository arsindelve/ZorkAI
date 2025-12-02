namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class ShinyFromitzBoard : FromitzBoardBase
{
    private readonly string[] _inPanelNouns =
    [
        "fromitz board", "board", "fromitz", "second fromitz board", "second board", "second",
        "second board", "second fromitz board", "second fromitz"
    ];

    private readonly string[] _outPanelNouns =
    [
        "fromitz board", "board", "fromitz", "shiny", "shiny board", "shiny fromitz board", "shiny fromitz"
    ];

    public override string[] NounsForMatching => CurrentLocation is FromitzAccessPanel
        ? _inPanelNouns
        : _outPanelNouns;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"A {(CurrentLocation is FromitzAccessPanel ? "second" : "shiny")} seventeen-centimeter fromitz board ";
    }

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a shiny seventeen-centimeter fromitz board here. ";
    }

    /// <summary>
    ///  This is the only board that can be taken - until it is installed, and then it will shock you, like the rest. 
    /// </summary>
    public override string? CannotBeTakenDescription =>
        CurrentLocation is FromitzAccessPanel ? base.CannotBeTakenDescription : null;
}