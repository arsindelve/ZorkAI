namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class FriedFromitzBoard : FromitzBoardBase
{
    private readonly string[] _inPanelNouns =
    [
        "fromitz board", "board", "fromitz", "second fromitz board", "second board", "second",
        "second board", "second fromitz board", "second fromitz"
    ];

    private readonly string[] _outPanelNouns =
    [
        "fromitz board", "board", "fromitz", "fried board", "fried", "fried board",
        "fried fromitz board", "fried fromitz"
    ];

    public override string[] NounsForMatching => CurrentLocation is FromitzAccessPanel
        ? _inPanelNouns
        : _outPanelNouns;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"A {(CurrentLocation is FromitzAccessPanel ? "second" : "fried")} seventeen-centimeter fromitz board ";
    }

    public override string ExaminationDescription =>
        base.ExaminationDescription + (CurrentLocation is not FromitzAccessPanel
            ? "This one is a bit blackened around the edges, though."
            : "");

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a fried seventeen-centimeter fromitz board here. ";
    }

    /// <summary>
    ///  This is the only board that can be taken.
    /// </summary>
    public override string? CannotBeTakenDescription => null;

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        return previousLocation is FromitzAccessPanel
            ? "The fromitz board slides out of the panel, producing an empty socket for another board"
            : null;
    }
}