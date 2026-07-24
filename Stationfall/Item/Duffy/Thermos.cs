namespace Stationfall.Item.Duffy;

/// <summary>
///     A plaid Thermos bottle from the survival kit (ship.zil:1280-1286), holding a serving of soup.
///     Its real importance is far downstream: sealed inside it, the station's FREZONE explosive
///     sublimes four times more slowly (interrupts.zil:361).
/// </summary>
public class Thermos : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["thermos", "bottle", "thermos bottle"];

    public override int Size => 4;

    protected override int SpaceForItems => 4;

    public string ExaminationDescription =>
        "A battered insulated bottle, plaid, with little cartoon robots printed all over it. " +
        (IsOpen ? ItemListDescription("Thermos", null) : "It is closed. ");

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A plaid Thermos bottle is here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override void Init()
    {
        ItemPlacedHere<BlueSoup>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A Thermos bottle";
    }
}
