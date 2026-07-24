namespace Stationfall.Item.Duffy;

/// <summary>
///     The requisition slot in the Robot Pool. Swallowing the Robot Use Authorization Form unlocks the
///     selection keypad (globals.zil:723-729).
/// </summary>
public class RobotPoolSlot : FormSlotBase
{
    protected override Type AcceptedFormType => typeof(RobotUseAuthorizationForm);

    public override string ExaminationDescription =>
        "A slot in the requisition console, sized for a standard Patrol form. ";

    protected override string AcceptForm(FormBase form, IContext context)
    {
        var pool = Repository.GetLocation<RobotPool>();

        if (pool.IsAuthorized)
            return "The slot has already taken your authorization. ";

        form.Consumed = true;
        pool.IsAuthorized = true;
        context.RemoveItem(form);

        return "The slot swallows the form. A recorded voice says, \"Authorization approved. Type the " +
               "bin number of the desired robot.\" ";
    }
}
