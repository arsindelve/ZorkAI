namespace Stationfall.Item.Duffy;

/// <summary>
///     The slot beside the fake door on Deck Twelve. It accepts nothing: the Assignment Completion Form
///     is refused for want of a commanding officer's validation, and there is no stamp aboard the ship
///     (globals.zil:730-734).
/// </summary>
public class DeckTwelveSlot : FormSlotBase
{
    // No form opens this door — the whole west side of Deck Twelve is a red herring.
    protected override Type? AcceptedFormType => null;

    public override string ExaminationDescription =>
        "A narrow slot set into the bulkhead beside the closed door. ";

    protected override string AcceptForm(FormBase form, IContext context)
    {
        // Unreachable: nothing is ever accepted here.
        return RejectForm(form, context);
    }

    protected override string RejectForm(FormBase form, IContext context)
    {
        if (form is AssignmentCompletionForm { IsValidated: false })
            return "The slot takes the form, considers it, and pushes it back out. A recorded voice " +
                   "says, \"This form has not been validated by a commanding officer.\" ";

        return base.RejectForm(form, context);
    }
}
