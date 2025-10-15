namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public abstract class FromitzBoardBase : ItemBase, ICanBeExamined, ICanBeTakenAndDropped
{
    /// <summary>
    /// Remove "board" and "fromitz board" from the list of disambiguation nouns. The adventurer will have to be more specific. 
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["card", "access card"]).ToArray();

    public string ExaminationDescription =>
        "Like most fromitz boards, it is a twisted maze of silicon circuits. It is square, approximately seventeen centimeters on each side. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a fried seventeen-centimeter fromitz board here. ";
    }

    public override string CannotBeTakenDescription =>
        "You jerk your hand back as you receive a powerful shock from the fromitz board. ";
}