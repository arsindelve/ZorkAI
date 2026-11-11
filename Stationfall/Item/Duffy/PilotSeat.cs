namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's pilot seat (ship.zil:1060-1068).
/// </summary>
public class PilotSeat : SeatBase
{
    public override string[] NounsForMatching => ["pilot seat", "pilot chair", "pilot's seat"];

    protected override SeatBase OtherSeat => Repository.GetItem<CopilotSeat>();

    protected override string SeatName => "pilot seat";
}
