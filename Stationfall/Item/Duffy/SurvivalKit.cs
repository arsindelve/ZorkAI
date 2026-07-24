namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's survival kit (ship.zil:1228-1236). Worth taking before you leave the truck: the
///     Thermos inside is the only thing that keeps the station's explosive from subliming away, much
///     later in the game.
/// </summary>
public class SurvivalKit : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["survival kit", "kit", "survival"];

    public override int Size => 10;

    protected override int SpaceForItems => 20;

    public string ExaminationDescription => IsOpen
        ? ItemListDescription("survival kit", null)
        : "The survival kit is closed. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A survival kit is stowed here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override void Init()
    {
        ItemPlacedHere<Thermos>();
        ItemPlacedHere<GrayGoo>();
        ItemPlacedHere<OrangeGoo>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !IsOpen ? "A survival kit" : $"A survival kit\n{ItemListDescription("survival kit", null)}";
    }
}
