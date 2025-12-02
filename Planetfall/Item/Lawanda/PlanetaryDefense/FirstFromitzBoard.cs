namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class FirstFromitzBoard : FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "first", "first fromitz board", "first board", "first fromitz"
    ];

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A first seventeen-centimeter fromitz board ";
    }
}