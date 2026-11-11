namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's copilot seat (ship.zil:1050-1058). Floyd's usual perch.
/// </summary>
public class CopilotSeat : SeatBase
{
    public override string[] NounsForMatching => ["copilot seat", "copilot chair", "co-pilot seat", "copilot's seat"];

    protected override SeatBase OtherSeat => Repository.GetItem<PilotSeat>();

    protected override string SeatName => "copilot seat";
}
