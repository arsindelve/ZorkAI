namespace Stationfall.Item.Duffy;

/// <summary>
///     The autopilot slot in the spacetruck. It takes the Class Three Spacecraft Activation Form, but
///     only when both seats are occupied — one by the player, the other by Floyd
///     (globals.zil:709-722).
/// </summary>
public class SpacetruckSlot : FormSlotBase
{
    protected override Type AcceptedFormType => typeof(ClassThreeSpacecraftActivationForm);

    public override string ExaminationDescription =>
        "A slot in the autopilot console, sized for a standard Patrol form. ";

    protected override string AcceptForm(FormBase form, IContext context)
    {
        var truck = Repository.GetLocation<Spacetruck>();

        if (truck.IsActivated)
            return "The spacecraft is already activated. ";

        if (!Spacetruck.BothSeatsOccupied(context))
            return "The form is spit back out. A recorded voice says, \"Safety precautions forbid the " +
                   "activation of the vehicle unless both the pilot and copilot seats are occupied.\" ";

        form.Consumed = true;
        truck.IsActivated = true;
        context.RemoveItem(form);

        return "The slot swallows the form. A recorded voice says, \"Spacecraft activated. Type in the " +
               "course heading.\" ";
    }
}
