using Planetfall.Item.Lawanda.PlanetaryDefense;

namespace Planetfall.Item.Kalamontee.Mech;

public class CrackedFromitzBoard : FromitzBoardBase
{
    private readonly string[] _inPanelNouns =
    [
        "fromitz board", "board", "fromitz", "second fromitz board", "second board", "second",
        "second board", "second fromitz board", "second fromitz"
    ];
    
    public override string[] NounsForMatching => CurrentLocation is FromitzAccessPanel
        ? _inPanelNouns
        : _outPanelNouns;
    
    private readonly string[] _outPanelNouns =
    [
        "fromitz board", "board", "fromitz", "cracked board", "cracked", "cracked board",
        "cracked fromitz board", "cracked fromitz"
    ];

    public override string ExaminationDescription =>
        "Like most fromitz boards, it is a twisted maze of silicon circuits. It is square, approximately seventeen centimeters on each side. This one looks as though it's been dropped. ";

    public override string? CannotBeTakenDescription => null;

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a cracked seventeen-centimeter fromitz board here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A cracked seventeen-centimeter fromitz board";
    }
}
