namespace Stationfall.Item.Duffy;

/// <summary>
///     The form the whole tedious assignment is about. It is a red herring for leaving the ship: the
///     Deck Twelve slot always rejects it as unvalidated, and the door it would open is a fake that
///     never opens (ship.zil:36-43; globals.zil:730-734).
/// </summary>
public class AssignmentCompletionForm : FormBase
{
    /// <summary>
    ///     The original tracks validation on this form (ship.zil:34), though nothing aboard the Duffy
    ///     can stamp it — the validation stamp is on the station, under the Commander's bed.
    /// </summary>
    public bool IsValidated { get; set; }

    public override string FormNumber => "QX-17-T";

    public override string FormTitle => "Assignment Completion";

    // "assignment" is the unique single-word handle for this form; see RobotUseAuthorizationForm.
    public override string[] NounsForMatching =>
        ["assignment", "assignment completion form", "assignment form", "completion form", "qx-17-t", "form"];

    public override string ReadDescription =>
        "Assignment Completion Form QX-17-T, the reason for your entire miserable errand. " +
        $"It has {(IsValidated ? "" : "not ")}been validated by a commanding officer. ";
}
