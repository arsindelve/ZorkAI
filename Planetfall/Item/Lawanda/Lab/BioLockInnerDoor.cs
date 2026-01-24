using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockInnerDoor : SimpleDoor
{
    public override string[] NounsForMatching =>
    [
        "door", "lab door"
    ];

    public override string OnOpening(IContext context)
    {
        // Don't trigger state machine if player is inside the Bio Lab
        if (context.CurrentLocation is BioLabLocation)
            return string.Empty;

        var bioLockEast = Repository.GetLocation<BioLockEast>();
        return bioLockEast.StateMachine.HandleDoorOpening(context);
    }

    public override string OnClosing(IContext context)
    {
        // Don't trigger state machine if player is inside the Bio Lab
        if (context.CurrentLocation is BioLabLocation)
            return string.Empty;

        var bioLockEast = Repository.GetLocation<BioLockEast>();
        return bioLockEast.StateMachine.HandleDoorClosing(context);
    }
}