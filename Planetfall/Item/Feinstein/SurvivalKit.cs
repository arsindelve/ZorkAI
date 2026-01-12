using Utilities;

namespace Planetfall.Item.Feinstein;

public class SurvivalKit : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["survival kit", "kit", "survival"];

    public string ExaminationDescription => IsOpen
        ? ItemListDescription("survival kit", null)
        : "The survival kit is closed. ";

    public override string NowOpen(ILocation currentLocation)
    {
        if (!Items.Any())
            return "Opened. ";

        var gooDescriptions = Items.Select(item => "blob of " + item.NounsForMatching[0]).ToList();
        return $"Opening the survival kit reveals {gooDescriptions.SingleLineListWithAnd()}. ";
    }

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
        return !IsOpen ? "A survival kit" : $"A survival kit\n{ItemListDescription("survival kit", null)}";
    }
}