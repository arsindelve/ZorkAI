namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class FourthFromitzBoard : FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "fourth", "fourth fromitz board", "fourth board", "fourth fromitz"
    ];
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A fourth seventeen-centimeter fromitz board ";
    }
}