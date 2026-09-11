namespace Stationfall.Item.Duffy;

/// <summary>
///     Fed into the spacetruck slot to activate the vehicle — but only when both the pilot and copilot
///     seats are occupied (ship.zil:54-61; globals.zil:709-722).
/// </summary>
public class ClassThreeSpacecraftActivationForm : FormBase
{
    public override string FormNumber => "HB-56-V";

    public override string FormTitle => "Class Three Spacecraft Activation";

    // "activation" is the unique single-word handle for this form; see RobotUseAuthorizationForm.
    public override string[] NounsForMatching =>
    [
        "activation", "class three spacecraft activation form", "spacecraft activation form", "activation form",
        "spacecraft form", "class three form", "hb-56-v", "form"
    ];

    public override string ReadDescription =>
        "Class Three Spacecraft Activation Form HB-56-V. It authorizes the bearer to operate one (1) " +
        "Class Three vehicle, and reminds them that the Patrol considers fuel a privilege rather than " +
        "a right. ";
}
