namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class ThirdFromitzBoard : FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "third", "third fromitz board", "third board", "third fromitz"
    ];
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A third seventeen-centimeter fromitz board ";
    }
}