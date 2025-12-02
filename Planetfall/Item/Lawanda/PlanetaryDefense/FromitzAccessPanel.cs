using Amazon.DynamoDBv2.DataModel;

namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class FromitzAccessPanel : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching =>
        ["access panel", "small access panel", "panel", "fromitz access panel"];

    public override string NowOpen(ILocation currentLocation)
    {
        return "The panel swings open. ";
    }

    public override Type[] CanOnlyHoldTheseTypes => [typeof(FromitzBoardBase)];

    public override string NowClosed(ILocation currentLocation)
    {
        return "The panel swings closed. ";
    }

    protected override int SpaceForItems => 4;

    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        string response = "The card clicks neatly into the socket. ";

        if (item is ShinyFromitzBoard)
        {
            response += "The card clicks neatly into the socket. The warning lights stop flashing. ";
            Repository.GetLocation<Location.Lawanda.PlanetaryDefense>().ItIsFixed(context);
        }

        return response;
    }

    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) => $"The {nameOfItemWeTriedToPlace} doesn't fit.";

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<FirstFromitzBoard>();
        StartWithItemInside<FriedFromitzBoard>();
        StartWithItemInside<ThirdFromitzBoard>();
        StartWithItemInside<FourthFromitzBoard>();
    }

    public string ExaminationDescription => IsOpen ? ItemListDescription("access panel", null) : "The access panel is closed. ";
}