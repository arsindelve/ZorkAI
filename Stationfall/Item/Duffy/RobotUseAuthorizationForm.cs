namespace Stationfall.Item.Duffy;

/// <summary>
///     Fed into the Robot Pool slot to unlock the selection keypad (ship.zil:45-52; globals.zil:723).
/// </summary>
public class RobotUseAuthorizationForm : FormBase
{
    public override string FormNumber => "JZ-59-G";

    public override string FormTitle => "Robot Use Authorization";

    // "authorization" is deliberately a unique single word: all three forms answer to "form", so the
    // player needs one unambiguous noun per form to avoid an endless disambiguation prompt.
    public override string[] NounsForMatching =>
        ["authorization", "robot use authorization form", "robot form", "authorization form", "jz-59-g", "form"];

    public override string ReadDescription =>
        "Robot Use Authorization Form JZ-59-G. It certifies that the bearer is entitled to requisition " +
        "one (1) robot for the duration of one (1) assignment, and warns in small red letters that " +
        "robots are Patrol property and may not be traded, wagered, or adopted. ";
}
