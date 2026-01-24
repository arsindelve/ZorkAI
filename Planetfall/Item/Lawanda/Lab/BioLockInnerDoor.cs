using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockInnerDoor : SimpleDoor
{
    public override string[] NounsForMatching =>
    [
        "door", "lab door"
    ];

    /// <summary>
    /// Flag to track if door was just opened this turn from inside Bio Lab.
    /// Used by FungicideTimer to grant a "free turn" for opening the escape route.
    /// </summary>
    [UsedImplicitly]
    public bool JustOpenedFromBioLabThisTurn { get; set; }

    public override string OnOpening(IContext context)
    {
        // Track if opened from inside Bio Lab (free turn for fungicide timer)
        if (context.CurrentLocation is BioLabLocation)
        {
            JustOpenedFromBioLabThisTurn = true;
            return string.Empty;
        }

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