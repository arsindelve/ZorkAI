namespace Planetfall.Item.Feinstein;

public class SurvivalKit : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["survival kit", "kit", "survival kit"];

    public string ExaminationDescription => $"The surival kit is {(IsOpen ? "open" : "closed")}. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a survival kit here. ";
    }

    public override void Init()
    {
        ItemPlacedHere<RedGoo>();
        ItemPlacedHere<BrownGoo>();
        ItemPlacedHere<GreenGoo>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A survival kit ";
    }
}