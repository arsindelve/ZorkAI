namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class SecondFromitzBoard : FromitzBoardBase
{
    public override string[] NounsForMatching =>
    [
        "fromitz board", "board", "fromitz", "second", "second fromitz board", "second board", "fried", "fried board", "fried fromitz board", "second fromitz"
    ];
    
    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"A {(CurrentLocation is FromitzAccessPanel ? "second" : "fried")} seventeen-centimeter fromitz board ";
    }

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
        return previousLocation is FromitzAccessPanel ? "The fromitz board slides out of the panel, producing an empty socket for another board" : null;
    }
}