namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class SecondFromitzBoard : FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "second", "second fromitz board", "second board", "second fromitz"
    ];
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A second seventeen-centimeter fromitz board ";
    }
}