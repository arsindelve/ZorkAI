namespace Stationfall.Item.Duffy;

/// <summary>
///     Helen — a form-sorting robot in bin two, and the other wrong answer. She is harmless to the
///     player but fatal to paperwork: given a form, she bursts and decollates it into confetti
///     (ship.zil:806-816, 832-836), which can destroy the activation form you need to leave.
/// </summary>
public class Helen : ShipRobot
{
    public override int BinNumber => 2;

    public override string[] NounsForMatching => ["helen", "spindly robot", "small robot"];

    public override string InTheBinDescription =>
        "The second bin holds a spindly little robot bristling with perforating attachments, built to " +
        "burst and decollate multi-part forms. A tiny nameplate reads \"Helen.\" ";

    public override string ExaminationDescription =>
        "Helen is a slender robot of many delicate arms, every one of them designed to separate one " +
        "piece of paper from another. She eyes your forms with unmistakable professional interest. ";

    protected override string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);
        return "Helen follows you, sorting the air with idle little snips. ";
    }
}
