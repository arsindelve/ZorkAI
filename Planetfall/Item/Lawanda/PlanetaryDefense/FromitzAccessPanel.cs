namespace Planetfall.Item.Lawanda.PlanetaryDefense;

public class FromitzAccessPanel : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching =>
        ["access panel", "small access panel", "panel", "fromitz access panel"];

    public override string NowOpen(ILocation currentLocation)
    {
        return "The panel swings open. ";
    }

    public override string NowClosed(ILocation currentLocation)
    {
        return "The panel swings closed. ";
    }

    // The canteen doesn't fit.
    
    // The card clicks neatly into the socket. The warning lights stop flashing.

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<FirstFromitzBoard>();
        StartWithItemInside<SecondFromitzBoard>();
        StartWithItemInside<ThirdFromitzBoard>();
        StartWithItemInside<FourthFromitzBoard>();
    }

    public string ExaminationDescription => IsOpen ? ItemListDescription("access panel", null) : "The access panel is closed. ";
}