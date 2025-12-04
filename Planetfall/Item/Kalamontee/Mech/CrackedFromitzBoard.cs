namespace Planetfall.Item.Kalamontee.Mech;

public class CrackedFromitzBoard : Lawanda.PlanetaryDefense.FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "cracked", "cracked fromitz board", "cracked board",
        "cracked fromitz", "seventeen-centimeter fromitz board", "17-centimeter fromitz board",
        "seventeen-centimeter board", "17-centimeter board"
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
