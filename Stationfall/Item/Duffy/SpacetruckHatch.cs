namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's hatch. It must be shut before launch — the original vents the cabin and kills a
///     player who leaves it open (ship.zil:1164-1166) — and it cannot be opened at all in deep space
///     (ship.zil:1042-1048).
/// </summary>
public class SpacetruckHatch : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["hatch", "spacetruck hatch", "truck hatch", "door"];

    public override bool IsTransparent => true;

    public string ExaminationDescription =>
        $"A broad loading hatch in the side of the spacetruck. It is {(IsOpen ? "open" : "closed")}. ";

    public override string NowOpen(ILocation currentLocation)
    {
        return "The hatch swings open. ";
    }

    public override string NowClosed(ILocation currentLocation)
    {
        return "The hatch swings shut and seals with a pneumatic sigh. ";
    }

    /// <summary>
    ///     Once the truck is under way there is nothing outside but vacuum, so the hatch stays shut
    ///     until it docks (ship.zil:1042-1048).
    /// </summary>
    public override string? CannotBeOpenedDescription(IContext context)
    {
        return Repository.GetLocation<Spacetruck>().IsInFlight
            ? "You can't open the hatch in deep space! "
            : null;
    }

    public override void Init()
    {
    }
}
